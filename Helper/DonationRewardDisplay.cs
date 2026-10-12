using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    // Clients build the Donation Altar's display from their prefab, which only holds Transcendence, so the altar hides it.
    // Each player sees rewards they could get on half-scale Lunar Buds above the altar instead, sent as instanced bud states.
    // The shown rewards are only examples; a donation rolls its own reward.
    public class DonationRewardDisplay : MonoBehaviour
    {
        private const string BudObjectName = "DonationRewardBud";
        private const float CycleInterval = 2f;
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
        private Func<PlayerCharacterMasterController, Dictionary<PickupIndex, int>> rollReward;
        private PurchaseInteraction purchaseInteraction;
        private float nextCycleTime;
        private float nextCostResendTime;

        public static void Preload()
        {
            budPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
        }

        // Call before spawning the altar. rollReward returns null for players who cannot get a reward.
        public static void Apply(GameObject altar, Func<PlayerCharacterMasterController, Dictionary<PickupIndex, int>> rollReward)
        {
            var terminal = altar.GetComponent<BlueprintTerminal>();
            // clients only hide their display when it is purchased; spawning with the flag set means they never build it
            terminal.hasBeenPurchased = true;

            var display = altar.AddComponent<DonationRewardDisplay>();
            display.rollReward = rollReward;
            display.purchaseInteraction = altar.GetComponent<PurchaseInteraction>();
            display.SpawnBuds(terminal.displayBaseTransform.position);
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

            if (now < nextCycleTime)
            {
                return;
            }
            nextCycleTime = now + CycleInterval;

            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                if (pc)
                {
                    ShowNextReward(pc);
                }
            }
        }

        public void ShowNextReward(PlayerCharacterMasterController pc)
        {
            var reward = rollReward(pc);

            // one bud per copy, so the amount shows too
            var pickups = new List<PickupIndex>();
            if (reward != null)
            {
                foreach (var item in reward)
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
    }
}
