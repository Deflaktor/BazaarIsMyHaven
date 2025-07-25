using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHome
{
    public class BazaarPrayer : BazaarBase
    {
        AsyncOperationHandle<InteractableSpawnCard> iscShrineHealing;

        AsyncOperationHandle<GameObject> LevelUpEffect;
        AsyncOperationHandle<GameObject> MoneyPackPickupEffect;
        AsyncOperationHandle<GameObject> TeamWarCryActivation;
        AsyncOperationHandle<GameObject> ShrineUseEffect;

        Dictionary<NetworkUserId, PlayerStruct> PlayerStructs = new Dictionary<NetworkUserId, PlayerStruct>();
        List<SpecialItemStruct> SpecialCodes = new List<SpecialItemStruct>
        {
            new SpecialItemStruct("BoostAttackSpeed", 10),
            new SpecialItemStruct("BoostDamage", 10),
            new SpecialItemStruct("BoostEquipmentRecharge", 10),
            new SpecialItemStruct("BoostHp", 10),
            new SpecialItemStruct("BurnNearby", 1),
            new SpecialItemStruct("CrippleWardOnLevel", 10),
            new SpecialItemStruct("CooldownOnCrit", 1),
            new SpecialItemStruct("EmpowerAlways", 1),
            new SpecialItemStruct("Ghost", 1),
            new SpecialItemStruct("Incubator", 3),
            new SpecialItemStruct("InvadingDoppelganger", 1),
            new SpecialItemStruct("LevelBonus", 10),
            new SpecialItemStruct("WarCryOnCombat", 10),
            new SpecialItemStruct("TempestOnKill", 10),
        };

        // 如果游戏更新，要检查一下
        List<string> EquipmentCodes = new List<string>
        {
            "EliteEarthEquipment" ,
            "EliteFireEquipment",
            "EliteHauntedEquipment",
            "EliteIceEquipment",
            "EliteLightningEquipment",
            "ElitePoisonEquipment",
            "EliteVoidEquipment",
            "EliteLunarEquipment",
            "EliteAurelioniteEquipment",
            "EliteBeadEquipment",
            "LunarPortalOnUse",
        };

        public override void Init()
        {
            iscShrineHealing = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineHealing/iscShrineHealing.asset");
            LevelUpEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LevelUpEffect.prefab");
            MoneyPackPickupEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BonusGoldPackOnKill/MoneyPackPickupEffect.prefab");
            TeamWarCryActivation = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamWarCry/TeamWarCryActivation.prefab");
            ShrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab");
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        public override void SetupBazaar()
        {
            if (ModConfig.EnableShrineHealing.Value)
            {
                InitPrayData();
                PlayerStructs.Clear();
                SpawnShrineHealing();
            }
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("ShrineHealing"))
                {
                    //HasBeenDonate = true;
                    NetworkUser networkUser = Util.LookUpBodyNetworkUser(activator.gameObject);
                    CharacterMaster characterMaster = activator.GetComponent<CharacterBody>().master;
                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    Inventory inventory = characterBody.inventory;

                    PlayerStructs.TryGetValue(networkUser.id, out PlayerStruct playerStruct);
                    if (playerStruct is null)
                    {
                        playerStruct = new PlayerStruct(networkUser, 1);
                        PlayerStructs.Add(networkUser.id, playerStruct);
                        ChatHelper.ThanksTip(networkUser, characterBody);
                    }
                    else
                    {
                        playerStruct.DonateCount += 1; // 加一次捐赠次数
                        if (playerStruct.DonateCount % 10 == 0) // 每满10次捐赠
                        {
                            playerStruct.RewardCount += 1;
                            if (playerStruct.RewardCount <= ModConfig.RewardCount.Value)
                            {
                                GiftReward(self, networkUser, characterBody, inventory); // 给奖励
                            }
                        }
                    }
                    if (playerStruct.DonateCount <= (ModConfig.RewardCount.Value * 10))
                    {
                        // 购买特效
                        SpawnEffect(ShrineUseEffect, self.transform.position, new Color32(64, 127, 255, 255), 5f);
                        networkUser.DeductLunarCoins((uint)self.Networkcost);
                    }
                    //ChatHelper.Send($"DonateCount = {playerStruct.DonateCount }, RewardCount = {playerStruct.RewardCount}");
                    return;
                }
            }
            orig(self, activator);
        }
        private void InitPrayData()
        {
            //SpecialCodes 
            SpecialCodes.ForEach(x => x.IsUse = false);
            string[] codes = ModConfig.PrayPeculiarList.Value.Split(',');
            for (int i = 0; i < codes.Length; i++)
            {
                string code = codes[i].Trim().ToLower();
                SpecialItemStruct result = SpecialCodes.FirstOrDefault(x => x.Name.ToLower() == code);
                result.IsUse = true;
            }
        }

        private void GiftReward(PurchaseInteraction self, NetworkUser networkUser, CharacterBody characterBody, Inventory inventory)
        {
            float w1 = ModConfig.PrayNormalWeight.Value, w2 = ModConfig.PrayEliteWeight.Value, w3 = ModConfig.PrayPeculiarWeight.Value;
            double random = RNG.NextDouble() * (w1 + w2 + w3);
            if (random <= w1)
            {
                WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                weightedSelection.AddChoice(Run.instance.availableTier1DropList, 100f);
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, 60f);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, 10f);
                List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                PickupDef pickupDef = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]);
                inventory.GiveItem((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None, 1);
                // 特效
                PurchaseInteraction.CreateItemTakenOrb(self.gameObject.transform.position, characterBody.gameObject, pickupDef.itemIndex);
                ChatHelper.ThanksTip(networkUser, characterBody, pickupDef);
            }
            else if (random <= w1 + w2)
            {
                string equipCode = EquipmentCodes[RNG.Next(EquipmentCodes.Count)];
                EquipmentIndex equipIndex = EquipmentCatalog.FindEquipmentIndex(equipCode);
                EquipmentIndex IsHasEquip = inventory.GetEquipmentIndex();
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipIndex);

                if (IsHasEquip != EquipmentIndex.None) // 如果玩家身上有主动装备，掉落出来
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(IsHasEquip), characterBody.gameObject.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + self.transform.forward * 2f);
                inventory.SetEquipmentIndex(equipIndex); // 为了获得文本，其实可以用GiveEquipmentString()

                ChatHelper.ThanksTip(networkUser, characterBody, equipmentDef);
            }
            else
            {
                SpecialItemStruct specialItemStruct = SpecialCodes[RNG.Next(SpecialCodes.Count)];
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(specialItemStruct.Name);
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                inventory.GiveItem(itemDef, specialItemStruct.Count);
                // 特效
                PurchaseInteraction.CreateItemTakenOrb(self.gameObject.transform.position, characterBody.gameObject, itemIndex);
                ChatHelper.ThanksTip(networkUser, characterBody, itemDef, specialItemStruct.Count);
            }
            SpawnEffect(LevelUpEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(MoneyPackPickupEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(TeamWarCryActivation, self.transform.position, new Color32(255, 255, 255, 255), 3f);
        }

        private void SpawnShrineHealing()
        {
            if (ModConfig.EnableShrineHealing.Value)
            {
                // 木灵
                SpawnCard spawnCard = iscShrineHealing.WaitForCompletion();
                GameObject gameObject = spawnCard.DoSpawn(new Vector3(-119f, -23f, -52f), Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectPlacement, Run.instance.runRNG)).spawnedInstance;
                gameObject.transform.eulerAngles = new Vector3(0.0f, 210f, 0.0f);
                gameObject.GetComponent<PurchaseInteraction>().costType = CostTypeIndex.LunarCoin;
                gameObject.GetComponent<PurchaseInteraction>().cost = ModConfig.PrayCost.Value * ModConfig.PenaltyCoefficient_Temp;
                gameObject.GetComponent<PurchaseInteraction>().Networkcost = ModConfig.PrayCost.Value * ModConfig.PenaltyCoefficient_Temp;

                //gameObject.GetComponent<ShrineHealingBehavior>().baseRadius = 80;
                //gameObject.GetComponent<ShrineHealingBehavior>().radiusBonusPerPurchase = 0;
                gameObject.GetComponent<ShrineHealingBehavior>().costMultiplierPerPurchase = 1;
                gameObject.GetComponent<ShrineHealingBehavior>().maxPurchaseCount = int.MaxValue;
            }
        }
    }
}
