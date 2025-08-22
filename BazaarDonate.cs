using RoR2;
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

                    var playerStruct = Main.instance.GetPlayerStruct(characterMaster.playerCharacterMasterController);
                    if(playerStruct.DonateCount == 0)
                    {
                        ChatHelper.ThanksTip(networkUser, characterMaster.playerCharacterMasterController);
                    }
                    playerStruct.DonateCount += 1;
                    if (playerStruct.DonateCount % 10 == 0)
                    {
                        playerStruct.RewardCount += 1;
                        if (playerStruct.RewardCount <= ModConfig.DonateRewardLimit.Value)
                        {
                            GiftReward(self, networkUser, characterBody, inventory);
                        }
                    }
                    if (playerStruct.DonateCount <= (ModConfig.DonateRewardLimit.Value * 10))
                    {
                        SpawnEffect(ShrineUseEffect, self.transform.position, new Color32(64, 127, 255, 255), 5f);
                        networkUser.DeductLunarCoins((uint)self.Networkcost);
                    }
                    //ChatHelper.Send($"DonateCount = {playerStruct.DonateCount }, RewardCount = {playerStruct.RewardCount}");
                    return;
                }
            }
            orig(self, activator);
        }

        private void GiftReward(PurchaseInteraction self, NetworkUser networkUser, CharacterBody characterBody, Inventory inventory)
        {
            float w1 = ModConfig.DonateRewardList1Weight.Value, w2 = ModConfig.DonateRewardList2Weight.Value, w3 = ModConfig.DonateRewardList3Weight.Value;
            double random = RNG.NextDouble() * (w1 + w2 + w3);
            int tier = 0;
            PickupIndex[] rewards = null;
            if (random <= w1)
            {
                tier = 1;
                rewards = ResolveItemRewardFromStringList(ModConfig.DonateRewardList1.Value);
            }
            else if (random <= w1 + w2)
            {
                tier = 2;
                rewards = ResolveItemRewardFromStringList(ModConfig.DonateRewardList2.Value);
            }
            else
            {
                tier = 3;
                rewards = ResolveItemRewardFromStringList(ModConfig.DonateRewardList3.Value);
            }

            if (rewards == null)
            {
                return;
            }

            var itemTakenOrbs = 0;
            foreach (var pickupIndex in rewards)
            {
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                if (pickupDef.itemIndex != ItemIndex.None)
                {
                    if (itemTakenOrbs < 20)
                    {
                        PurchaseInteraction.CreateItemTakenOrb(self.transform.position + Vector3.up * 6.0f, characterBody.gameObject, pickupDef.itemIndex);
                        itemTakenOrbs++;
                    }
                    inventory.GiveItem(pickupDef.itemIndex);
                }
                else if (pickupDef.equipmentIndex != EquipmentIndex.None)
                {
                    EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                    EquipmentIndex IsHasEquip = inventory.GetEquipmentIndex();
                    if (IsHasEquip != EquipmentIndex.None)
                        PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(IsHasEquip), characterBody.gameObject.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + self.transform.forward * 2f);
                    inventory.SetEquipmentIndex(pickupDef.equipmentIndex);
                }
            }

            switch(tier)
            {
                case 1:
                    ChatHelper.ThanksTipNormal(networkUser, characterBody.master.playerCharacterMasterController, rewards);
                    break;
                case 2:
                    ChatHelper.ThanksTipElite(networkUser, characterBody.master.playerCharacterMasterController, rewards);
                    break;
                case 3:
                    ChatHelper.ThanksTipPeculiar(networkUser, characterBody.master.playerCharacterMasterController, rewards);
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
