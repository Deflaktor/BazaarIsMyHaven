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
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
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
                    //Ensure order, payment, then items. Mostly a precaution.
                    if (IsMultiplayer() && ModCompatibilityShareSuite.enabled && ModCompatibilityShareSuite.IsShareSuite_PrinterCauldronFixEnabled())
                    {
                        var body = activator ? activator.GetComponent<CharacterBody>() : null;
                        Inventory inventory = body ? body.inventory : null;
                        ShopTerminalBehavior shop = self.GetComponent<ShopTerminalBehavior>();
                        var pickupDef = shop ? PickupCatalog.GetPickupDef(shop.CurrentPickup().pickupIndex) : null;
                        if (inventory && pickupDef != null && pickupDef.itemIndex != ItemIndex.None)
                        {
                            // Pay back only after the game accepts and pays for this buy.
                            bool compensated = false;
                            UnityEngine.Events.UnityAction<CostTypeDef.PayCostContext, CostTypeDef.PayCostResults> listener =
                                (context, results) =>
                                {
                                    if (!compensated && context.activator == activator && inventory)
                                    {
                                        compensated = true;
                                        inventory.GiveItemPermanent(pickupDef.itemIndex, 2);
                                    }
                                };
                            self.onDetailedPurchaseServer.AddListener(listener);
                            try
                            {
                                orig(self, activator);
                            }
                            finally
                            {
                                self.onDetailedPurchaseServer.RemoveListener(listener);
                            }
                            return;
                        }
                    }
                }
            }

            orig(self, activator);
        }

        public void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.CauldronSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("LunarCauldronGreen"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    if (list.Count > 0)
                        newPickup.pickupIndex = list[RNG.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronRed"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    if (list.Count > 0)
                        newPickup.pickupIndex = list[RNG.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronWhite"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    if (list.Count > 0)
                        newPickup.pickupIndex = list[RNG.Next(0, list.Count)];
                }
            }
            orig(self, newPickup, newHidden);
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
                    if (!randomCauldron.IsValid())
                    {
                        Log.LogWarning("No cauldron types with a valid weight; skipping cauldrons.");
                        break;
                    }
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

            // Negative weights count as off.
            if (w_g < 0) w_g = 0;
            if (g_r < 0) g_r = 0;
            if (r_w < 0) r_w = 0;

            float total = w_g + g_r + r_w;

            // No cauldron type is on when all weights are zero.
            if (total == 0)
                return default;

            double d = RNG.NextDouble() * total;
            if (d < w_g) return LunarCauldronsCode[0];
            else if (d < w_g + g_r) return LunarCauldronsCode[1];
            else { return LunarCauldronsCode[2]; }
        }
    }
}
