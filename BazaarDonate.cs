using BepInEx.Logging;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    public class BazaarDonate : BazaarBase
    {
        AsyncOperationHandle<GameObject> altarPrefab;
        AsyncOperationHandle<GameObject> LevelUpEffect;
        AsyncOperationHandle<GameObject> MoneyPackPickupEffect;
        AsyncOperationHandle<GameObject> TeamWarCryActivation;
        AsyncOperationHandle<GameObject> ShrineUseEffect;

        private readonly Dictionary<PlayerCharacterMasterController, int> donationsDuringRun = new Dictionary<PlayerCharacterMasterController, int>();
        private DirectorCard card;
        private bool spawned;
        private GameObject altar;
        // above the reward buds
        private const float AltarSignHeight = 6.5f;

        // Rewards are rolled several times a second, so each warning is logged once per run
        private readonly ManualLogSource rewardLog = new ManualLogSource(Main.PluginName);
        private readonly HashSet<string> loggedRewardWarnings = new HashSet<string>();

        public BazaarDonate()
        {
            rewardLog.LogEvent += (sender, args) =>
            {
                string message = args.Data?.ToString();
                if (message != null && loggedRewardWarnings.Add(message))
                    Log.GetSource().Log(args.Level, message);
            };
        }

        public override void Preload()
        {
            altarPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DeepVoidPortalBattery/DeepVoidPortalBattery.prefab");
            LevelUpEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LevelUpEffect.prefab");
            MoneyPackPickupEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BonusGoldPackOnKill/MoneyPackPickupEffect.prefab");
            TeamWarCryActivation = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamWarCry/TeamWarCryActivation.prefab");
            ShrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab");
            DonationRewardDisplay.Preload();
            PriceSigns.Preload();
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }
        public override void RunStart()
        {
            donationsDuringRun.Clear();
            loggedRewardWarnings.Clear();
        }

        public override void RunEnd()
        {
            donationsDuringRun.Clear();
        }

        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            card = null;
            if (ModConfig.DonateSectionEnabled.Value)
            {
                card = new DirectorCard("Donation Altar", ModConfig.DonateDirectorCost, ModConfig.DonateDirectorWeight, () => 1);
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            spawned = false;
            altar = null;
            SpawnLatePicks();
        }

        public override void SpawnLatePicks()
        {
            if (card == null || card.Count == 0 || spawned)
                return;
            SpawnDonateAltar();
            spawned = true;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && ModConfig.DonateSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (altar && self.gameObject == altar)
                {
                    CharacterBody characterBody = activator ? activator.GetComponent<CharacterBody>() : null;
                    if (!characterBody || !characterBody.master || !characterBody.inventory)
                        return;

                    CharacterMaster characterMaster = characterBody.master;
                    var pc = characterMaster.playerCharacterMasterController;
                    // This handler takes the cost itself, since the game's purchase would unlock a blueprint
                    if (!pc || !self.available || self.cost < 0 || !self.CanBeAffordedByInteractor(activator))
                        return;

                    var playerStruct = Main.instance.GetPlayerStruct(pc);
                    if (CanGetReward(pc))
                    {
                        if (!TryGetReward(characterBody.bodyIndex, donationsDuringRun.GetValueOrDefault(pc), out var resolvedItems, out int tier))
                            return;

                        // pay first, so items paid with can't be the reward
                        Helper.PayCost(self.costType, self.cost, activator, characterBody, self.gameObject, self.rng);
                        Helper.GivePickups(characterBody, resolvedItems, self.transform.position + Vector3.up * 6.0f, true);
                        playerStruct.RewardCount += 1;
                        donationsDuringRun[pc] = donationsDuringRun.GetValueOrDefault(pc) + 1;
                        ShowRewardEffects(self, characterBody, resolvedItems, tier);
                        SpawnEffect(ShrineUseEffect, self.transform.position, new Color32(64, 127, 255, 255), 5f);
                        // empty the buds right away if the limit is reached
                        var display = self.GetComponent<DonationRewardDisplay>();
                        if (display)
                            display.ShowNextReward(pc);
                    }
                    return;
                }
            }
            orig(self, activator);
        }

        // Pays like the game's purchase does, in any cost type, but without its purchase events
        private bool CanGetReward(PlayerCharacterMasterController pc)
        {
            return Main.instance.GetPlayerStruct(pc).RewardCount < ModConfig.DonateRewardLimitPerVisit.Value
                && donationsDuringRun.GetValueOrDefault(pc) < ModConfig.DonateRewardLimitPerRun.Value;
        }

        // An example reward for the altar's display
        private Dictionary<PickupIndex, int> RollReward(PlayerCharacterMasterController pc)
        {
            var master = pc.master;
            if (!master || !CanGetReward(pc))
                return null;

            // dead players have no body, so use the one they spawn as
            var body = master.GetBody();
            var bodyIndex = body ? body.bodyIndex : master.bodyPrefab ? BodyCatalog.FindBodyIndex(master.bodyPrefab) : BodyIndex.None;
            return TryGetReward(bodyIndex, donationsDuringRun.GetValueOrDefault(pc), out var resolvedItems, out _) ? resolvedItems : null;
        }

        private bool TryGetReward(BodyIndex bodyIndex, int donations, out Dictionary<PickupIndex, int> resolvedItems, out int tier)
        {
            tier = 0;
            resolvedItems = new Dictionary<PickupIndex, int>();
            var combined = new List<(float weight, int tier)>
            {
                (ModConfig.DonateRewardList1Weight.Value, 1),
                (ModConfig.DonateRewardList2Weight.Value, 2),
                (ModConfig.DonateRewardList3Weight.Value, 3),
                (ModConfig.DonateRewardListCharacterWeight.Value, 4),
            };
            // Skip disabled or bad weights before picking a reward.
            combined.RemoveAll(item => item.weight <= 0);
            if (combined.Count == 0)
            {
                rewardLog.LogWarning("No donation lists with a valid weight; the Donation Altar gives no rewards.");
                return false;
            }

            if (ModConfig.DonateSequentialRewardLists.Value)
            {
                // Sort by descending weight
                combined.Sort((a, b) => b.weight.CompareTo(a.weight));

                tier = combined[donations % combined.Count].tier;
            }
            else
            {
                double random = RNG.NextDouble() * combined.Sum(item => item.weight);
                tier = combined[combined.Count - 1].tier;
                foreach (var entry in combined)
                {
                    if (random < entry.weight)
                    {
                        tier = entry.tier;
                        break;
                    }
                    random -= entry.weight;
                }
            }

            switch (tier)
            {
                case 1:
                    ParseRewards(ModConfig.DonateRewardList1.Value, resolvedItems);
                    break;
                case 2:
                    ParseRewards(ModConfig.DonateRewardList2.Value, resolvedItems);
                    break;
                case 3:
                    ParseRewards(ModConfig.DonateRewardList3.Value, resolvedItems);
                    break;
                case 4:
                    // characters without a list of their own, or whose list gives nothing, get the default list
                    if (!ModConfig.DonateRewardListCharacters.TryGetValue(bodyIndex, out var characterList) || !ParseRewards(characterList.Value, resolvedItems))
                    {
                        ParseRewards(ModConfig.DonateRewardListCharacterDefault.Value, resolvedItems);
                    }
                    break;
            }

            if (resolvedItems.Count == 0)
            {
                rewardLog.LogWarning("A donation list has no valid rewards; donations that choose it charge no coins.");
                return false;
            }
            return true;
        }

        // Leaves only the items and equipment that can be given; returns whether there are any
        private bool ParseRewards(string rewardList, Dictionary<PickupIndex, int> resolvedItems)
        {
            resolvedItems.Clear();
            ItemStringParser.ItemStringParser.ParseItemString(rewardList, resolvedItems, rewardLog, false);
            foreach (var entry in resolvedItems.ToArray())
            {
                var pickupDef = PickupCatalog.GetPickupDef(entry.Key);
                if (entry.Value <= 0 || pickupDef == null ||
                    (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None))
                    resolvedItems.Remove(entry.Key);
            }
            return resolvedItems.Count > 0;
        }

        private void ShowRewardEffects(PurchaseInteraction self, CharacterBody characterBody, Dictionary<PickupIndex, int> resolvedItems, int tier)
        {
            ChatHelper.DonationThanks(characterBody, resolvedItems, tier);

            SpawnEffect(LevelUpEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(MoneyPackPickupEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(TeamWarCryActivation, self.transform.position, new Color32(255, 255, 255, 255), 3f);
        }

        // The altar is the game's Deep Void Signal: unmodded clients only spawn prefabs the game registers for them.
        // It never charges, since the donation handler takes its interaction.
        private void SpawnDonateAltar()
        {
            altar = GameObject.Instantiate(altarPrefab.WaitForCompletion(), new Vector3(-114.4226f, -22.6017f, -49.9827f), Quaternion.Euler(0f, 300f, 0f));
            var purchaseInteraction = altar.GetComponent<PurchaseInteraction>();
            var cost = Helper.GetCost(ModConfig.DonateCost.Value, ModConfig.DonateCostType.Value);
            purchaseInteraction.cost = cost;
            purchaseInteraction.Networkcost = cost;
            purchaseInteraction.NetworkcostType = ModConfig.DonateCostType.Value;
            purchaseInteraction.NetworkdisplayNameToken = "NEWT_STATUE_NAME";
            purchaseInteraction.NetworkcontextToken = "NEWT_STATUE_CONTEXT";

            DonationRewardDisplay.Apply(altar, RollReward);
            NetworkServer.Spawn(altar);
            // only the signal's model stays, without its beam
            VoidSignalBeam.Remove(altar);
            // the Deep Void Signal has no price hologram of its own
            PriceSigns.Apply(new List<GameObject> { altar }, cost, ModConfig.DonateCostType.Value, AltarSignHeight);
        }
    }
}
