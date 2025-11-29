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
    public class BazaarCauldron : BazaarBase
    {
        AsyncOperationHandle<GameObject> lunarCauldronWhiteToGreen;
        AsyncOperationHandle<GameObject> lunarCauldronGreenToRed;
        AsyncOperationHandle<GameObject> lunarCauldronRedToWhite;
        AsyncOperationHandle<GameObject>[] LunarCauldronsCode;
        Dictionary<int, SpawnCardStruct> DicCauldrons = new Dictionary<int, SpawnCardStruct>();

        public override void Preload()
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

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
        }
        public override void RunStart()
        {
            
        }

        public override void SetupBazaar()
        {
            if (ModConfig.CauldronSectionEnabled.Value) {
                SpawnLunarCauldron();
            }
        }

        public void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value && ModConfig.CauldronSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("LunarCauldron, WhiteToGreen")) {
                    self.cost = ModConfig.CauldronWhiteToGreenCost.Value;
                    self.Networkcost = ModConfig.CauldronWhiteToGreenCost.Value;
                }
                if (self.name.StartsWith("LunarCauldron, GreenToRed"))
                {
                    self.cost = ModConfig.CauldronGreenToRedCost.Value;
                    self.Networkcost = ModConfig.CauldronGreenToRedCost.Value;
                }
                if (self.name.StartsWith("LunarCauldron, RedToWhite"))
                {
                    self.cost = ModConfig.CauldronRedToWhiteCost.Value;
                    self.Networkcost = ModConfig.CauldronRedToWhiteCost.Value;
                }
            }
        }

        public void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && ModConfig.CauldronSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
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
            if (ModConfig.EnableMod.Value && ModConfig.CauldronSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("LunarCauldronGreen"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[RNG.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronRed"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[RNG.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronWhite"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[RNG.Next(0, list.Count)];
                }
            }
            orig(self, newPickupIndex, newHidden);
        }

        public void SpawnLunarCauldron()
        {
            if (ModConfig.CauldronAmount.Value > 0)
            {
                DicCauldrons.Clear();
                SetCauldron();
                int count = 0;
                if (ModConfig.SpawnCountByStage.Value) count = SetCountbyGameStage(ModConfig.CauldronAmount.Value, ModConfig.SpawnCountOffset.Value);
                else count = ModConfig.CauldronAmount.Value;
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
                int index = RNG.Next(total.Count);
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
            float w_g = ModConfig.CauldronWhiteToGreenWeight.Value;
            float g_r = ModConfig.CauldronGreenToRedWeight.Value;
            float r_w = ModConfig.CauldronRedToWhiteWeight.Value;
            float total = w_g + g_r + r_w;
            double d = RNG.NextDouble() * total;
            if (d <= w_g) return LunarCauldronsCode[0];
            else if (d <= w_g + g_r) return LunarCauldronsCode[1];
            else { return LunarCauldronsCode[2]; }
        }
    }
}
