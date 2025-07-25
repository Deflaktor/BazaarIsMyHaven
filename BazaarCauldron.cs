using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHome
{
    public class BazaarCauldron
    {
        List<CauldronHackedStruct> CauldronHackedStructs = new List<CauldronHackedStruct>();
        AsyncOperationHandle<GameObject> lunarCauldronWhiteToGreen;
        AsyncOperationHandle<GameObject> lunarCauldronGreenToRed;
        AsyncOperationHandle<GameObject> lunarCauldronRedToWhite;
        AsyncOperationHandle<GameObject>[] LunarCauldronsCode;
        Dictionary<int, SpawnCardStruct> DicCauldrons = new Dictionary<int, SpawnCardStruct>();

        public void Init()
        {
            lunarCauldronWhiteToGreen = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, WhiteToGreen.prefab");
            lunarCauldronGreenToRed = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, GreenToRed Variant.prefab");
            lunarCauldronRedToWhite = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, RedToWhite Variant.prefab");
            LunarCauldronsCode = [
                lunarCauldronWhiteToGreen,
                lunarCauldronGreenToRed,
                lunarCauldronRedToWhite
            ];
        }

        public void Hook()
        {
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
        }

        public void EnterBazaar()
        {
            SetCauldronList_Hacked();
            SpawnLunarCauldron();
        }

        public void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value && BazaarIsMyHome.instance.IsCurrentMapInBazaar())
            {
                if (ModConfig.CauldronCount.Value > 0)
                {
                    if (ModConfig.EnableCauldronHacking.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        double random = BazaarIsMyHome.instance.random.NextDouble();
                        if (self.name.StartsWith("LunarCauldron, WhiteToGreen")) // 绿锅
                        {
                            self.cost = ModConfig.CauldronWhiteToGreenCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.CauldronWhiteToGreenCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            if (random <= ModConfig.CauldronGreenHackedChance.Value && ModConfig.EnableCauldronHacking.Value) // 被黑概率
                            {
                                //ChatHelper.Send("一台绿锅被黑");
                                CauldronHacked_Start(self, "LunarCauldronGreen"); // 变特定锅
                            }
                        }
                        if (self.name.StartsWith("LunarCauldron, GreenToRed")) // 红锅
                        {
                            self.cost = ModConfig.CauldronGreenToRedCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.CauldronGreenToRedCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            if (random <= ModConfig.CauldronRedHackedChance.Value && ModConfig.EnableCauldronHacking.Value)
                            {
                                //ChatHelper.Send("一台红锅被黑");
                                CauldronHacked_Start(self, "LunarCauldronRed");
                            }
                        }
                        if (self.name.StartsWith("LunarCauldron, RedToWhite")) // 白锅
                        {
                            self.cost = ModConfig.CauldronRedToWhiteCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.CauldronRedToWhiteCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            if (ModConfig.CauldronWhiteCostTypeChange.Value)
                            {
                                self.costType = CostTypeIndex.GreenItem;
                            }
                            if (random <= ModConfig.CauldronWhiteHackedChance.Value && ModConfig.EnableCauldronHacking.Value)
                            {
                                //ChatHelper.Send("一台白锅被黑");
                                CauldronHacked_Start(self, "LunarCauldronWhite");
                            }
                        }
                    }
                }
                else
                {
                    if (ModConfig.PenaltyCoefficient_Temp != 1) // 如果商人死了，关了数量，开了惩罚系数
                    {
                        if (self.name.StartsWith("LunarCauldron, WhiteToGreen")
                            || self.name.StartsWith("LunarCauldron, GreenToRed")
                            || self.name.StartsWith("LunarCauldron, RedToWhite"))
                        {
                            self.cost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
            }
        }

        public void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && BazaarIsMyHome.instance.IsCurrentMapInBazaar())
            {
                // 修复白色大锅
                if (self.name.StartsWith("LunarCauldron, RedToWhite Variant"))
                {
                    if (IsMultiplayer() && ModCompatibilityShareSuite.enabled && ModCompatibilityShareSuite.IsShareSuite_PrinterCauldronFixEnabled())
                    {
                        Inventory inventory = activator.GetComponent<CharacterBody>().inventory;
                        ShopTerminalBehavior shop = self.GetComponent<ShopTerminalBehavior>();
                        inventory.GiveItem(PickupCatalog.GetPickupDef(shop.CurrentPickupIndex()).itemIndex, 2);
                    }
                }
            }
            orig(self, activator);
        }

        public void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && BazaarIsMyHome.instance.IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("LunarCauldronGreen"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[BazaarIsMyHome.instance.random.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronRed"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[BazaarIsMyHome.instance.random.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronWhite"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[BazaarIsMyHome.instance.random.Next(0, list.Count)];
                }
            }
            orig(self, newPickupIndex, newHidden);
        }

        private bool IsMultiplayer()
        {
            return PlayerCharacterMasterController.instances.Count > 1;
        }

        public void SpawnLunarCauldron()
        {
            if (ModConfig.CauldronCount.Value > 0)
            {
                // 大锅
                DicCauldrons.Clear();
                SetCauldron();
                int count = 0;
                if (ModConfig.SpawnCountByStage.Value) count = BazaarIsMyHome.instance.SetCountbyGameStage(ModConfig.CauldronCount.Value, ModConfig.SpawnCountOffset.Value);
                else count = ModConfig.CauldronCount.Value;
                for (int i = 0; i < count; i++)
                {
                    AsyncOperationHandle<GameObject> randomCauldron = GetRandomLunarCauldron();
                    GameObject gameObject = randomCauldron.WaitForCompletion();
                    gameObject = UnityEngine.Object.Instantiate<GameObject>(gameObject, DicCauldrons[i].Position, Quaternion.identity);
                    gameObject.transform.eulerAngles = DicCauldrons[i].Rotation;
                    NetworkServer.Spawn(gameObject);
                }
            }
        }
        private void SetCauldron()
        {
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            List<int> random = new List<int>();

            while (total.Count > 0)
            {
                int index = BazaarIsMyHome.instance.random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicCauldrons.Add(random[0], new SpawnCardStruct(new Vector3(-115.9816f, -24.1175f, -6.2091f), new Vector3(0.0f, 120.0f, 0.0f)));
            DicCauldrons.Add(random[1], new SpawnCardStruct(new Vector3(-119.9280f, -24.1238f, -7.0865f), new Vector3(0.0f, 140.0f, 0.0f)));
            DicCauldrons.Add(random[2], new SpawnCardStruct(new Vector3(-123.4725f, -23.7951f, -5.4690f), new Vector3(0.0f, 160.0f, 0.0f)));
            DicCauldrons.Add(random[3], new SpawnCardStruct(new Vector3(-107.8159f, -23.8448f, -4.5170f), new Vector3(0.0f, 130.0f, 0.0f)));
            DicCauldrons.Add(random[4], new SpawnCardStruct(new Vector3(-101.2425f, -24.8612f, -9.1464f), new Vector3(0.0f, 160.0f, 0.0f)));
            DicCauldrons.Add(random[5], new SpawnCardStruct(new Vector3(-98.5219f, -25.6548f, -12.3659f), new Vector3(0.0f, 155.0f, 0.0f)));
            DicCauldrons.Add(random[6], new SpawnCardStruct(new Vector3(-94.6071f, -25.8717f, -13.6159f), new Vector3(0.0f, 135.0f, 0.0f)));
            //DicCauldrons.Add(random[6], new SpawnCardStruct(new Vector3(-91.1582f, -25.0957f, -10.9174f), new Vector3(0.0f, 80.0f, 0.0f)));
            //DicCauldrons.Add(random[7], new SpawnCardStruct(new Vector3(-89.8054f, -24.0894f, -7.2084f), new Vector3(0.0f, 80.0f, 0.0f)));
            //DicCauldrons.Add(random[8], new SpawnCardStruct(new Vector3(-85.7223f, -23.6673f, -4.8544f), new Vector3(0.0f, 85.0f, 0.0f)));
        }

        public void SetCauldronList_Hacked()
        {
            if (ModConfig.CauldronCount.Value > 0)
            {
                if (ModConfig.EnableCauldronHacking.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                {
                    CauldronHackedStructs.Clear();
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronGreen-Yellow", ModConfig.CauldronWhiteToGreenCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.WhiteItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronGreen-Blue", ModConfig.CauldronWhiteToGreenCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.WhiteItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronGreen-Purple", ModConfig.CauldronWhiteToGreenCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.WhiteItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronRed-Yellow", ModConfig.CauldronGreenToRedCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.GreenItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronRed-Blue", ModConfig.CauldronGreenToRedCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.GreenItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronRed-Purple", ModConfig.CauldronGreenToRedCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.GreenItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronWhite-Yellow", ModConfig.CauldronRedToWhiteCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.BossItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronWhite-Blue", ModConfig.CauldronRedToWhiteCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.LunarItemOrEquipment));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronWhite-Purple", ModConfig.CauldronRedToWhiteCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.RedItem));
                }
            }
        }

        private void CauldronHacked_Start(PurchaseInteraction self, string newName)
        {
            float w1 = ModConfig.CauldronYellowWeight.Value, w2 = ModConfig.CauldronBlueWeight.Value, w3 = ModConfig.CauldronPurpleWeight.Value;
            float total = w1 + w2 + w3;
            double random = BazaarIsMyHome.instance.random.NextDouble() * total;
            CauldronHackedStruct cauldronHacked = null;
            if (random <= w1)
            {
                //ChatHelper.Send("被黑成黄色");
                cauldronHacked = CauldronHackedStructs.FirstOrDefault(x => x.Name.StartsWith(newName) && x.Name.EndsWith("Yellow"));
                self.name = cauldronHacked.Name;
                self.cost = cauldronHacked.Cost;
                self.Networkcost = cauldronHacked.Cost;
                self.costType = cauldronHacked.CostTypeIndex;
            }
            else if (random <= w1 + w2)
            {
                //ChatHelper.Send("被黑成蓝色");
                cauldronHacked = CauldronHackedStructs.FirstOrDefault(x => x.Name.StartsWith(newName) && x.Name.EndsWith("Blue"));
                self.name = cauldronHacked.Name;
                self.cost = cauldronHacked.Cost;
                self.Networkcost = cauldronHacked.Cost;
                self.costType = cauldronHacked.CostTypeIndex;
            }
            else
            {
                //ChatHelper.Send("被黑成紫色");
                cauldronHacked = CauldronHackedStructs.FirstOrDefault(x => x.Name.StartsWith(newName) && x.Name.EndsWith("Purple"));
                self.name = cauldronHacked.Name;
                self.cost = cauldronHacked.Cost;
                self.Networkcost = cauldronHacked.Cost;
                self.costType = cauldronHacked.CostTypeIndex;
            }
        }

        private void CauldronHacked_SetPickupIndex(ShopTerminalBehavior self, out List<PickupIndex> listLunarItem)
        {
            listLunarItem = new List<PickupIndex>();
            if (self.name.EndsWith("Yellow"))
            {
                listLunarItem.AddRange(Run.instance.availableBossDropList);
            }
            if (self.name.EndsWith("Blue"))
            {
                listLunarItem.AddRange(Run.instance.availableLunarItemDropList);
            }
            if (self.name.EndsWith("Purple"))
            {
                listLunarItem.AddRange(Run.instance.availableVoidTier1DropList);
                listLunarItem.AddRange(Run.instance.availableVoidTier2DropList);
                listLunarItem.AddRange(Run.instance.availableVoidTier3DropList);
                listLunarItem.AddRange(Run.instance.availableVoidBossDropList);
            }
        }

        private AsyncOperationHandle<GameObject> GetRandomLunarCauldron()
        {
            float w_g = ModConfig.CauldronGreenWeight.Value;
            float g_r = ModConfig.CauldronRedWeight.Value;
            float g_w = ModConfig.CauldronWhiteWeight.Value;
            float total = w_g + g_r + g_w;
            double d = BazaarIsMyHome.instance.random.NextDouble() * total;
            if (d <= w_g) return LunarCauldronsCode[0];
            else if (d <= w_g + g_r) return LunarCauldronsCode[1];
            else { return LunarCauldronsCode[2]; }
        }
        private void GetRandomLunarCauldron_DLC1()
        {
            int total = 0;
            if (ModConfig.CauldronGreenWeight.Value != 0f) total++;
            if (ModConfig.CauldronRedWeight.Value != 0f) total++;
            if (ModConfig.CauldronWhiteWeight.Value != 0f) total++;
            if (ModConfig.CauldronYellowWeight.Value != 0f) total++;
            if (ModConfig.CauldronBlueWeight.Value != 0f) total++;
            if (ModConfig.CauldronPurpleWeight.Value != 0f) total++;
        }
    }
}
