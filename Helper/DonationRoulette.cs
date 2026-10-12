using RoR2;
using RoR2.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    // Clients build the Donation Altar's display from their prefab, which only holds Transcendence, so the altar hides it.
    // Each player sees their own roll on half-scale Lunar Buds above the altar instead, sent as instanced bud states.
    // Like the Adaptive Chest, a donation gets the roll the player saw: the host remembers when it showed each roll
    // and looks back by the player's round trip time.
    public class DonationRoulette : MonoBehaviour
    {
        public class Roll
        {
            public readonly Dictionary<PickupIndex, int> items;
            public readonly int tier;
            public bool taken;

            public Roll(Dictionary<PickupIndex, int> items, int tier)
            {
                this.items = items;
                this.tier = tier;
            }
        }

        private struct ShownRoll
        {
            public float time;
            public Roll roll;
        }

        private const string BudObjectName = "DonationRouletteBud";
        // the Adaptive Chest's allowance for the delay between seeing a roll and pressing the button
        private const float RewindTime = 0.05f;
        // longer than any round trip time a player could donate with
        private const float HistoryDuration = 5f;
        private const float MinimumInterval = 0.1f;
        // PurchaseInteraction's cost SyncVar
        private const uint CostDirtyBit = 16u;
        private const float CostResendInterval = 1f;
        // at the prefab's half scale, a bud holds its item this high above its base
        private const float BudItemHeight = 0.94f;
        // in the altar's space: the first bud holds its item where Transcendence floated, the others are around and below it
        private static readonly Vector3[] BudOffsets =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0.9f, -0.3f, 0f),
            new Vector3(-0.9f, -0.3f, 0f),
            new Vector3(0f, -0.3f, 0.9f),
            new Vector3(0f, -0.3f, -0.9f),
        };

        private static AsyncOperationHandle<GameObject> budPrefab;

        private readonly List<InstancedPurchase> buds = new List<InstancedPurchase>();
        private readonly Dictionary<PlayerCharacterMasterController, List<ShownRoll>> shownRolls = new Dictionary<PlayerCharacterMasterController, List<ShownRoll>>();
        private Func<PlayerCharacterMasterController, Roll> rollReward;
        private PurchaseInteraction purchaseInteraction;
        private float nextRollTime;
        private float nextCostResendTime;

        public static void Preload()
        {
            budPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
        }

        // Call before spawning the altar. rollReward returns null for players who cannot get a reward.
        public static void Apply(GameObject altar, Func<PlayerCharacterMasterController, Roll> rollReward)
        {
            var terminal = altar.GetComponent<BlueprintTerminal>();
            // clients only hide their display when it is purchased; spawning with the flag set means they never build it
            terminal.hasBeenPurchased = true;

            var roulette = altar.AddComponent<DonationRoulette>();
            roulette.rollReward = rollReward;
            roulette.purchaseInteraction = altar.GetComponent<PurchaseInteraction>();
            roulette.SpawnBuds(terminal.displayBaseTransform.position);
        }

        private void SpawnBuds(Vector3 displayPosition)
        {
            // QoLChests would hide the buds, since they are never available
            if (ModCompatibilityQoLChests.enabled)
            {
                ModCompatibilityQoLChests.RegisterQoLChestsBlacklist(BudObjectName);
            }

            var altarRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            var center = displayPosition - Vector3.up * BudItemHeight;
            foreach (var offset in BudOffsets)
            {
                var outward = altarRotation * new Vector3(offset.x, 0f, offset.z);
                var rotation = outward == Vector3.zero ? altarRotation : Quaternion.LookRotation(outward);
                var bud = Instantiate(budPrefab.WaitForCompletion(), center + altarRotation * offset, rotation);
                // the Lunar Shop replaces every object with the prefab's name
                bud.name = BudObjectName;
                bud.GetComponent<PurchaseInteraction>().available = false;
                bud.GetComponent<ShopTerminalBehavior>().selfGeneratePickup = false;
                // every player starts with an empty, unavailable bud
                buds.Add(bud.AddComponent<InstancedPurchase>());
                NetworkServer.Spawn(bud);
            }
        }

        private void Update()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now >= nextCostResendTime)
            {
                nextCostResendTime = now + CostResendInterval;
                // clients set the prefab's price when they build the altar, which late joiners do at any time
                purchaseInteraction.SetDirtyBit(CostDirtyBit);
            }

            if (now < nextRollTime)
            {
                return;
            }
            nextRollTime = now + Mathf.Max(MinimumInterval, ModConfig.DonateRouletteInterval.Value);

            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                if (pc)
                {
                    ShowNextRoll(pc);
                }
            }
            foreach (var pc in shownRolls.Keys.Where(pc => !pc).ToList())
            {
                shownRolls.Remove(pc);
            }
        }

        public void ShowNextRoll(PlayerCharacterMasterController pc)
        {
            float now = Time.unscaledTime;
            var roll = rollReward(pc);
            if (!shownRolls.TryGetValue(pc, out var history))
            {
                history = new List<ShownRoll>();
                shownRolls.Add(pc, history);
            }
            history.Add(new ShownRoll { time = now, roll = roll });
            // keep the roll that was showing when the remembered time starts
            while (history.Count > 1 && history[1].time <= now - HistoryDuration)
            {
                history.RemoveAt(0);
            }

            // one bud per copy, so the amount shows too
            var pickups = new List<PickupIndex>();
            if (roll != null)
            {
                foreach (var item in roll.items)
                {
                    for (int i = 0; i < item.Value && pickups.Count < buds.Count; i++)
                    {
                        pickups.Add(item.Key);
                    }
                }
            }

            for (int i = 0; i < buds.Count; i++)
            {
                if (!buds[i])
                {
                    continue;
                }
                var state = buds[i].GetOrCreate(pc);
                var pickup = i < pickups.Count ? new UniquePickup(pickups[i]) : UniquePickup.none;
                if (!state.pickup.Equals(pickup))
                {
                    state.pickup = pickup;
                    InstancedPurchases.UpdatePlayer(buds[i].gameObject, pc);
                }
            }
        }

        // The roll the player saw when donating, or null if nothing was shown. Each roll can be taken once.
        public Roll TakeShownRoll(PlayerCharacterMasterController pc)
        {
            if (!pc || !shownRolls.TryGetValue(pc, out var history) || history.Count == 0)
            {
                return null;
            }

            // the host sees its rolls at once; a client sees them half a round trip later, and its donation takes the other half
            float lookBack = RewindTime;
            if (!pc.hasAuthority && pc.networkUser)
            {
                lookBack += RttManager.GetConnectionRTT(pc.networkUser.connectionToClient);
            }
            float time = Time.unscaledTime - lookBack;

            int index = Math.Max(history.FindLastIndex(shown => shown.time <= time), 0);
            var roll = history[index].roll;
            if (roll == null || roll.taken)
            {
                return null;
            }
            roll.taken = true;
            return roll;
        }
    }
}
