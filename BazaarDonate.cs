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
            if (ModConfig.EnableMod.Value & ModConfig.DonateSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("BlueprintStation"))
                {
                    NetworkUser networkUser = Util.LookUpBodyNetworkUser(activator.gameObject);
                    CharacterMaster characterMaster = activator.GetComponent<CharacterBody>().master;
                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    Inventory inventory = characterBody.inventory;
                    var pc = characterMaster.playerCharacterMasterController;


                    var playerStruct = Main.instance.GetPlayerStruct(pc);
                    if (playerStruct.RewardCount < ModConfig.DonateRewardLimitPerVisit.Value && donationsDuringRun.GetValueOrDefault(pc) < ModConfig.DonateRewardLimitPerRun.Value)
                    {
                        GiftReward(self, networkUser, characterBody, inventory, donationsDuringRun.GetValueOrDefault(pc));
                        playerStruct.RewardCount += 1;
                        donationsDuringRun[pc] = donationsDuringRun.GetValueOrDefault(pc) + 1;
                        SpawnEffect(ShrineUseEffect, self.transform.position, new Color32(64, 127, 255, 255), 5f);
                        networkUser.DeductLunarCoins((uint)self.Networkcost);
                    }
                    return;
                }
            }
            orig(self, activator);
        }

        private void GiftReward(PurchaseInteraction self, NetworkUser networkUser, CharacterBody characterBody, Inventory inventory, int donations)
        {
            int tier = 0;
            if (ModConfig.DonateSequentialRewardLists.Value)
            {
                var combined = new List<(float weight, int tier)>
                {
                    (ModConfig.DonateRewardList1Weight.Value, 1),
                    (ModConfig.DonateRewardList2Weight.Value, 2),
                    (ModConfig.DonateRewardList3Weight.Value, 3),
                    (ModConfig.DonateRewardListCharacterWeight.Value, 4),
                };
                // Remove entries where weight is 0
                combined.RemoveAll(item => item.weight == 0);

                // Sort by descending weight
                combined.Sort((a, b) => b.weight.CompareTo(a.weight));

                tier = combined[donations % combined.Count].tier;
            }
            else
            {
                float w1 = ModConfig.DonateRewardList1Weight.Value;
                float w2 = ModConfig.DonateRewardList2Weight.Value;
                float w3 = ModConfig.DonateRewardList3Weight.Value;
                float w4 = ModConfig.DonateRewardListCharacterWeight.Value;
                double random = RNG.NextDouble() * (w1 + w2 + w3 + w4);
                if (random <= w1)
                    tier = 1;
                else if (random <= w1 + w2)
                    tier = 2;
                else if (random <= w1 + w2 + w3)
                    tier = 3;
                else
                    tier = 4;
            }

            Dictionary<PickupIndex, int> resolvedItems = new Dictionary<PickupIndex, int>();
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

            var itemTakenOrbs = 0;
            uint equipmentsGiven = 0;
            int equipSkip = 0;
            bool equipLoop = false;
            foreach (var (pickupIndex, itemAmount) in resolvedItems)
            {
                if (itemAmount <= 0)
                    continue;
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                // handle items
                var itemIndex = pickupDef.itemIndex;
                var equipmentIndex = pickupDef.equipmentIndex;
                if (itemIndex != ItemIndex.None)
                {
                    if (itemTakenOrbs < 20)
                    {
                        PurchaseInteraction.CreateItemTakenOrb(self.transform.position + Vector3.up * 6.0f, characterBody.gameObject, pickupDef.itemIndex);
                        itemTakenOrbs++;
                    }
                    inventory.GiveItemPermanent(itemIndex, itemAmount);
                } else { 
                    // handle equipments
                    var equipmentAmount = itemAmount;
                    int maxEquipmentSlots = Helper.IsToolbotWithSwapSkill(characterBody.master) ? 2 : 1;
                    int maxEquipmentSets = inventory.GetItemCountEffective(DLC3Content.Items.ExtraEquipment.itemIndex) + 1;
                    int maxEquipmentCount = maxEquipmentSlots * maxEquipmentSets;
                    while (equipmentIndex != EquipmentIndex.None && equipmentAmount > 0 && equipmentsGiven < maxEquipmentCount)
                    {
                        var index = equipmentsGiven + equipSkip;
                        if(index >= maxEquipmentCount)
                        {
                            equipLoop = true;
                            index = 0;
                        }
                        uint slot = (uint)(index % maxEquipmentSlots);
                        uint set = (uint)(index / maxEquipmentSlots);
                        var equipmentState = inventory.GetEquipment(slot, set);
                        if(EquipmentState.empty.Equals(equipmentState))
                        {
                            // has no equipment in this slot -> set it
                            inventory.SetEquipmentIndexForSlot(equipmentIndex, slot, set);
                            equipmentAmount--;
                            equipmentsGiven++;
                        }
                        else
                        {
                            if(!equipLoop)
                            {
                                // skip this slot, because it already has an equip
                                equipSkip++;
                            }
                            else
                            {
                                // we already looped once -> time to drop the old equipment
                                var oldEquipment = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentState.equipmentIndex));
                                PickupDropletController.CreatePickupDroplet(oldEquipment, characterBody.gameObject.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + self.transform.forward * 2f, false);
                                inventory.SetEquipmentIndexForSlot(equipmentIndex, slot, set);
                                equipmentAmount--;
                                equipmentsGiven++;
                            }
                                
                        }
                    }
                }
            }

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
