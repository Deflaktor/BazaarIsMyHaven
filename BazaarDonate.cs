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
        //AsyncOperationHandle<InteractableSpawnCard> iscShrineHealing;
        AsyncOperationHandle<GameObject> BlueprintStation;
        AsyncOperationHandle<GameObject> LevelUpEffect;
        AsyncOperationHandle<GameObject> MoneyPackPickupEffect;
        AsyncOperationHandle<GameObject> TeamWarCryActivation;
        AsyncOperationHandle<GameObject> ShrineUseEffect;

        private readonly Dictionary<PlayerCharacterMasterController, int> donationsDuringRun = new Dictionary<PlayerCharacterMasterController, int>();

        public override void Preload()
        {
            // iscShrineHealing = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineHealing/iscShrineHealing.asset");
            BlueprintStation = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/BlueprintStation.prefab");
            LevelUpEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LevelUpEffect.prefab");
            MoneyPackPickupEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BonusGoldPackOnKill/MoneyPackPickupEffect.prefab");
            TeamWarCryActivation = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamWarCry/TeamWarCryActivation.prefab");
            ShrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab");
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.BlueprintTerminal.Rebuild += BlueprintTerminal_Rebuild;
        }
        public override void RunStart()
        {
            donationsDuringRun.Clear();
        }

        public override void RunEnd()
        {
            donationsDuringRun.Clear();
        }

        public override void SetupBazaar()
        {
            if (ModConfig.DonateSectionEnabled.Value)
            {
                SpawnDonateAltar();
            }
        }

        public void BlueprintTerminal_Rebuild(On.RoR2.BlueprintTerminal.orig_Rebuild orig, BlueprintTerminal self)
        {
            if (ModConfig.EnableMod.Value & ModConfig.DonateSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active && self.name.StartsWith("BlueprintStation"))
            {
                PurchaseInteraction purchaseInteraction = self.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction != null)
                {
                    purchaseInteraction.cost = ModConfig.DonateCost.Value;
                    purchaseInteraction.Networkcost = ModConfig.DonateCost.Value;
                }
            }
            else
            {
                orig(self);
            }
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && ModConfig.DonateSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("BlueprintStation"))
                {
                    CharacterBody characterBody = activator ? activator.GetComponent<CharacterBody>() : null;
                    if (!characterBody || !characterBody.master || !characterBody.inventory)
                        return;

                    NetworkUser networkUser = Util.LookUpBodyNetworkUser(activator.gameObject);
                    CharacterMaster characterMaster = characterBody.master;
                    var pc = characterMaster.playerCharacterMasterController;
                    // This handler takes lunar coins directly, so check the exact amount.
                    if (!pc || !networkUser || !self.available || self.Networkcost < 0 ||
                        networkUser.lunarCoins < (uint)self.Networkcost)
                        return;

                    var playerStruct = Main.instance.GetPlayerStruct(pc);
                    if (playerStruct.RewardCount < ModConfig.DonateRewardLimitPerVisit.Value && donationsDuringRun.GetValueOrDefault(pc) < ModConfig.DonateRewardLimitPerRun.Value)
                    {
                        if (!TryGetReward(characterBody, donationsDuringRun.GetValueOrDefault(pc), out var resolvedItems, out int tier))
                            return;

                        Helper.GivePickups(characterBody, resolvedItems, self.transform.position + Vector3.up * 6.0f, true);
                        playerStruct.RewardCount += 1;
                        donationsDuringRun[pc] = donationsDuringRun.GetValueOrDefault(pc) + 1;
                        networkUser.DeductLunarCoins((uint)self.Networkcost);
                        ShowRewardEffects(self, networkUser, characterBody, resolvedItems, tier);
                        SpawnEffect(ShrineUseEffect, self.transform.position, new Color32(64, 127, 255, 255), 5f);
                    }
                    return;
                }
            }
            orig(self, activator);
        }

        private bool TryGetReward(CharacterBody characterBody, int donations, out Dictionary<PickupIndex, int> resolvedItems, out int tier)
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
                Log.LogWarning("No donation lists with a valid weight; no coins charged.");
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
                    ItemStringParser.ItemStringParser.ParseItemString(ModConfig.DonateRewardList1.Value, resolvedItems, Log.GetSource(), false);
                    break;
                case 2:
                    ItemStringParser.ItemStringParser.ParseItemString(ModConfig.DonateRewardList2.Value, resolvedItems, Log.GetSource(), false);
                    break;
                case 3:
                    ItemStringParser.ItemStringParser.ParseItemString(ModConfig.DonateRewardList3.Value, resolvedItems, Log.GetSource(), false);
                    break;
                case 4:
                    var rewardList = ModConfig.DonateRewardListCharacters.GetValueOrDefault(characterBody.bodyIndex, ModConfig.DonateRewardListCharacterDefault).Value;
                    ItemStringParser.ItemStringParser.ParseItemString(rewardList, resolvedItems, Log.GetSource(), false);
                    break;
            }
            foreach (var entry in resolvedItems.ToArray())
            {
                var pickupDef = PickupCatalog.GetPickupDef(entry.Key);
                if (entry.Value <= 0 || pickupDef == null ||
                    (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None))
                    resolvedItems.Remove(entry.Key);
            }

            if (resolvedItems.Count == 0)
            {
                Log.LogWarning("The chosen donation list has no valid rewards; no coins charged.");
                return false;
            }
            return true;
        }

        private void ShowRewardEffects(PurchaseInteraction self, NetworkUser networkUser, CharacterBody characterBody, Dictionary<PickupIndex, int> resolvedItems, int tier)
        {
            switch(tier)
            {
                case 1:
                    ChatHelper.ThanksTipNormal(networkUser, characterBody.master.playerCharacterMasterController, resolvedItems);
                    break;
                case 2:
                    ChatHelper.ThanksTipElite(networkUser, characterBody.master.playerCharacterMasterController, resolvedItems);
                    break;
                case 3:
                    ChatHelper.ThanksTipPeculiar(networkUser, characterBody.master.playerCharacterMasterController, resolvedItems);
                    break;
                case 4:
                    ChatHelper.ThanksTipCharacter(networkUser, characterBody.master.playerCharacterMasterController, resolvedItems);
                    break;
            }

            SpawnEffect(LevelUpEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(MoneyPackPickupEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(TeamWarCryActivation, self.transform.position, new Color32(255, 255, 255, 255), 3f);
        }

        private void SpawnDonateAltar()
        {
            GameObject gameObject = GameObject.Instantiate(BlueprintStation.WaitForCompletion(), new Vector3(-117.1011f, -24.1373f, -48.4219f), Quaternion.identity);
            // RoR2/Base/WarCryOnMultiKill/WarCryEffect.prefab: -17.2625f

            gameObject.transform.eulerAngles = new Vector3(0.0f, 300f, 0.0f);
            gameObject.GetComponent<PurchaseInteraction>().cost = ModConfig.DonateCost.Value;
            gameObject.GetComponent<PurchaseInteraction>().Networkcost = ModConfig.DonateCost.Value;
            gameObject.GetComponent<PurchaseInteraction>().contextToken = "NEWT_STATUE_CONTEXT";
            gameObject.GetComponent<PurchaseInteraction>().NetworkcontextToken = "NEWT_STATUE_CONTEXT";

            NetworkServer.Spawn(gameObject);
        }
    }
}
