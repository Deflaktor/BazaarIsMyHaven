using BepInEx;
using RoR2;
using ShareSuite.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace BazaarIsMyHaven
{
    public class BazaarLunarShop : BazaarBase
    {
        AsyncOperationHandle<GameObject> lunarShopTerminal;
        AsyncOperationHandle<GameObject> LunarRerollEffect;

        Dictionary<int, SpawnCardStruct> DicLunarShopTerminals = new Dictionary<int, SpawnCardStruct>();
        int LunarShopTerminalTotalCount = 0;
        int currentLunarShopStaticItemIndex = 0;
        int lunarRecyclerRerolledCount = 0;
        List<PurchaseInteraction> ObjectLunarShopTerminals_Spawn = new List<PurchaseInteraction>();
        PlayerCharacterMasterController currentActivator = null;
        Dictionary<PurchaseInteraction, List<PlayerCharacterMasterController>> whichStallsHaveBeenBoughtOnce = new Dictionary<PurchaseInteraction, List<PlayerCharacterMasterController>>();

        public override void Init()
        {
            // lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
            // lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FreeChestTerminal/FreeChestTerminal.prefab");
            // lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MultiShopTerminal/ShopTerminal.prefab");
            lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FreeChestTerminalShippingDrone/FreeChestTerminalShippingDrone.prefab");
            // lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/SeerStation.prefab");
            LunarRerollEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarRecycler/LunarRerollEffect.prefab");
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            IL.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionEnd;
            On.RoR2.PurchaseInteraction.ScaleCost += PurchaseInteraction_ScaleCost;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.DropPickup += ShopTerminalBehavior_DropPickup;
        }

        public override void SetupBazaar()
        {
            if (ModConfig.LunarRecyclerSectionEnabled.Value)
            {
                lunarRecyclerRerolledCount = 0;
            }
            if(ModConfig.LunarShopSectionEnabled.Value) {
                currentLunarShopStaticItemIndex = 0;
                whichStallsHaveBeenBoughtOnce.Clear();
                SpawnLunarShopTerminal();
            }
        }

        public void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if(ModConfig.LunarShopSectionEnabled.Value && ModConfig.LunarShopTerminalCost.Value >= 0) 
                {
                    if (self.name.StartsWith("LunarShopTerminal"))
                    {
                        self.cost = ModConfig.LunarShopTerminalCost.Value;
                        self.Networkcost = ModConfig.LunarShopTerminalCost.Value;
                    }
                }
                if (ModConfig.LunarRecyclerSectionEnabled.Value)
                {
                    if (self.name.StartsWith("LunarRecycler"))
                    {
                        if (ModConfig.LunarRecyclerAvailable.Value && ModConfig.LunarRecyclerRerollCost.Value >= 0)
                        {
                            self.cost = ModConfig.LunarRecyclerRerollCost.Value;
                            self.Networkcost = ModConfig.LunarRecyclerRerollCost.Value;
                        }
                        else
                        {
                            NetworkServer.Destroy(self.gameObject);
                        }
                    }
                }
            }
        }

        public void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && ModConfig.LunarShopSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if(self.name.StartsWith("LunarShopTerminal"))
                {
                    var playerCharacterMasterController = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
                    var playerStruct = Main.instance.GetPlayerStruct(playerCharacterMasterController);

                    var usesLeft = ModConfig.LunarShopBuyLimit.Value - playerStruct.LunarShopUseCount;

                    if (usesLeft <= 0) {
                        ChatHelper.LunarShopTerminalUsesLeft(playerCharacterMasterController, usesLeft);
                        return;
                    }
                    try { 
                        currentActivator = playerCharacterMasterController;
                        orig(self, activator);
                        return;
                    }
                    finally
                    {
                        currentActivator = null;
                    }
                }
                if (self.name.StartsWith("LunarRecycler"))
                {
                    float time = 0f;
                    foreach (PurchaseInteraction interaction in ObjectLunarShopTerminals_Spawn)
                    {
                        Main.instance.StartCoroutine(DelayEffect(interaction, time, true));
                        time = time + 0.1f;
                    }
                }
            }
            orig(self, activator);
        }

        private void PurchaseInteraction_OnInteractionEnd(MonoMod.Cil.ILContext il)
        {
            HookHelper.HookEndOfMethod(il, (PurchaseInteraction self, Interactor activator) =>
            {
                if (ModConfig.EnableMod.Value && ModConfig.LunarShopSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
                {
                    var playerCharacterMasterController = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
                    var playerStruct = Main.instance.GetPlayerStruct(playerCharacterMasterController);
                    if (self.name.StartsWith("LunarShopTerminal"))
                    {
                        if (!whichStallsHaveBeenBoughtOnce.TryGetValue(self, out List<PlayerCharacterMasterController> buyers) || !buyers.Contains(playerCharacterMasterController))
                        {
                            playerStruct.LunarShopUseCount++;
                            var usesLeft = ModConfig.LunarShopBuyLimit.Value - playerStruct.LunarShopUseCount;
                            ChatHelper.LunarShopTerminalUsesLeft(playerCharacterMasterController, usesLeft);
                            whichStallsHaveBeenBoughtOnce[self].Add(playerCharacterMasterController);
                        }
                    }
                    if (self.name.StartsWith("LunarRecycler"))
                    {
                        lunarRecyclerRerolledCount++;
                        var usesLeft = ModConfig.LunarRecyclerRerollLimit.Value - lunarRecyclerRerolledCount;
                        ChatHelper.LunarRecyclerUsesLeft(usesLeft);
                    }
                }
            });
        }

        private void PurchaseInteraction_ScaleCost(On.RoR2.PurchaseInteraction.orig_ScaleCost orig, PurchaseInteraction self, float scalar)
        {
            if (ModConfig.EnableMod.Value && ModConfig.LunarRecyclerSectionEnabled.Value && ModConfig.LunarRecyclerAvailable.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("LunarRecycler"))
                {
                    scalar = (float)ModConfig.LunarRecyclerRerollCostMultiplier.Value;
                }
            }
            orig(self, scalar);
        }
        private void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            if (ModConfig.EnableMod.Value && ModConfig.LunarRecyclerSectionEnabled.Value && ModConfig.LunarRecyclerAvailable.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("LunarRecycler"))
                {
                    newAvailable = lunarRecyclerRerolledCount < ModConfig.LunarRecyclerRerollLimit.Value;
                }
            }
            orig(self, newAvailable);
        }
        private void ShopTerminalBehavior_DropPickup(On.RoR2.ShopTerminalBehavior.orig_DropPickup orig, ShopTerminalBehavior self)
        {
            if (ModConfig.EnableMod.Value && ModConfig.LunarShopSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active && self.name.StartsWith("LunarShopTerminal"))
            {
                if (ModConfig.LunarShopBuyToInventory.Value)
                {
                    var body = currentActivator.master.GetBody();
                    if (body != null)
                    {
                        var pickupDef = PickupCatalog.GetPickupDef(self.CurrentPickupIndex());
                        if (pickupDef.itemIndex != ItemIndex.None)
                        {
                            PurchaseInteraction.CreateItemTakenOrb(self.transform.position, body.gameObject, PickupCatalog.GetPickupDef(self.CurrentPickupIndex()).itemIndex);
                            currentActivator.master.inventory.GiveItem(pickupDef.itemIndex);

                            self.SetHasBeenPurchased(newHasBeenPurchased: true);
                            self.SetNoPickup();
                        }
                        else if (pickupDef.equipmentIndex != EquipmentIndex.None)
                        {
                            if (currentActivator.master.inventory.GetEquipmentIndex() != EquipmentIndex.None)
                            {
                                var placeEquipment = PickupCatalog.FindPickupIndex(currentActivator.master.inventory.GetEquipmentIndex());
                                self.SetPickupIndex(placeEquipment);
                                var purchaseInteraction = self.GetComponent<PurchaseInteraction>();
                                purchaseInteraction.SetAvailable(true);
                            }
                            else
                            {
                                self.SetHasBeenPurchased(newHasBeenPurchased: true);
                                self.SetNoPickup();
                            }
                            currentActivator.master.inventory.SetEquipmentIndex(pickupDef.equipmentIndex);
                        }
                    }
                }
                else
                {
                    orig(self);
                    self.SetNoPickup();
                }
            }
            else
            {
                orig(self);
            }
        }

        IEnumerator DelayEffect(PurchaseInteraction lunarShopTerminal, float time, bool spawnEffect)
        {
            yield return new WaitForSeconds(time);

            RerollLunarShopTerminal(lunarShopTerminal);
            if(spawnEffect)
                SpawnEffect(LunarRerollEffect, lunarShopTerminal.gameObject.transform.position - Vector3.up * 2.5f, new Color32(255, 255, 255, 255), 2f);
        }

        private void RerollLunarShopTerminal(PurchaseInteraction lunarShopTerminal)
        {
            PickupIndex pickupIndex = PickupIndex.none;

            // var index = ObjectLunarShopTerminals_Spawn.IndexOf(lunarShopTerminal);
            if (!ModConfig.LunarShopItemsList.Value.IsNullOrWhiteSpace())
            {
                List<string> codes = ModConfig.LunarShopItemsList.Value.Split(',').ToList();

                TryAgain:

                if(codes.Count > 0)
                {
                    var index = RNG.Next(codes.Count);
                    if (ModConfig.EnableLunarShopStaticItems.Value)
                    {
                        index = currentLunarShopStaticItemIndex % codes.Count;
                    }
                    var code = codes[index].Trim();

                    ItemTier itemTier;
                    var isItemTier = Enum.TryParse(code, out itemTier);
                    if (isItemTier)
                    {
                        switch (itemTier)
                        {
                            case ItemTier.Tier1:
                                pickupIndex = GetRandom(Run.instance.availableTier1DropList, PickupIndex.none);
                                break;
                            case ItemTier.Tier2:
                                pickupIndex = GetRandom(Run.instance.availableTier2DropList, PickupIndex.none);
                                break;
                            case ItemTier.Tier3:
                                pickupIndex = GetRandom(Run.instance.availableTier3DropList, PickupIndex.none);
                                break;
                            case ItemTier.Boss:
                                pickupIndex = GetRandom(Run.instance.availableBossDropList, PickupIndex.none);
                                break;
                            case ItemTier.Lunar:
                                pickupIndex = GetRandom(Run.instance.availableLunarItemDropList, PickupIndex.none);
                                break;
                            case ItemTier.VoidTier1:
                                pickupIndex = GetRandom(Run.instance.availableVoidTier1DropList, PickupIndex.none);
                                break;
                            case ItemTier.VoidTier2:
                                pickupIndex = GetRandom(Run.instance.availableVoidTier2DropList, PickupIndex.none);
                                break;
                            case ItemTier.VoidTier3:
                                pickupIndex = GetRandom(Run.instance.availableVoidTier3DropList, PickupIndex.none);
                                break;
                            case ItemTier.VoidBoss:
                                pickupIndex = GetRandom(Run.instance.availableVoidBossDropList, PickupIndex.none);
                                break;
                        }
                        if (pickupIndex == PickupIndex.none)
                        {
                            Log.LogWarning($"LunarShopItemsList: Could not get pickup from item tier: {code}, skipping it.");
                            codes.RemoveAt(index);
                            goto TryAgain;
                        }
                    }
                    else
                    {
                        ItemIndex itemIndex = ItemCatalog.FindItemIndex(code);
                        if (itemIndex != ItemIndex.None)
                        {
                            pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
                        }
                        else
                        {
                            EquipmentIndex equipmentIndex = EquipmentCatalog.FindEquipmentIndex(code);
                            if (equipmentIndex != EquipmentIndex.None)
                            {
                                pickupIndex = PickupCatalog.FindPickupIndex(equipmentIndex);
                            }
                            else
                            {
                                pickupIndex = PickupIndex.none;
                                Log.LogError($"LunarShopItemsList: Could not find item name: {code}");
                            }
                        }
                    }
                }
                else
                {
                    pickupIndex = GetRandom(Run.instance.availableLunarCombinedDropList, PickupIndex.none);
                }
            }
            else
            {
                pickupIndex = GetRandom(Run.instance.availableLunarCombinedDropList, PickupIndex.none);
            }
            ShopTerminalBehavior shopTerminal = lunarShopTerminal.gameObject.GetComponent<ShopTerminalBehavior>();
            shopTerminal.SetPickupIndex(pickupIndex);
            currentLunarShopStaticItemIndex += 1;
        }
        private void SetLunarShopTerminal()
        {
            Vector3 lunarTablePosition = new Vector3(-76.6438f, -24.0468f, -41.6449f);
            float orientation = 280f;
            Vector3 lunarTableDroneShopPosition = new Vector3(-139.8156f, -21.8568f, 2.9263f);
            const float droneTableOrientation = 160f;

            const float tableRadiusInner = 3.0f;
            const float tableRadiusMiddle = 4.0f;
            const float tableRadiusOuter = 5.0f;
            float tableStartAngleInner = 140f;
            float tableStartAngleMiddle = 135f;
            float tableStartAngleOuter = 123f;
            float tableEndAngleInner = 330f;
            float tableEndAngleMiddle = 325f;
            float tableEndAngleOuter = 339f;
            
            const float minDistance = 19f;
            const float innerCapacity = 5;//(int)(2 * Math.PI * tableRadiusInner * (tableEndAngleInner - tableStartAngleInner) / 360f / minDistance);
            const float middleCapacity = 8;//(int)(2 * Math.PI * tableRadiusMiddle * (tableEndAngleMiddle - tableStartAngleMiddle) / 360f / minDistance);
            const float outerCapacity = 10;//(int)(2 * Math.PI * tableRadiusOuter * (tableEndAngleOuter - tableStartAngleOuter) / 360f / minDistance);

            List<Vector2> points = new List<Vector2>();

            int count = 0;
            if (ModConfig.SpawnCountByStage.Value)
                count = SetCountbyGameStage(ModConfig.LunarShopTerminalCount.Value, ModConfig.SpawnCountOffset.Value);
            else
                count = ModConfig.LunarShopTerminalCount.Value;

            if (count <= middleCapacity)
            {
                if (count < middleCapacity)
                {
                    // place them closer together
                    float angleDiff = tableEndAngleMiddle - tableStartAngleMiddle;
                    tableStartAngleMiddle += angleDiff / (float)(count + 1f);
                    tableEndAngleMiddle -= angleDiff / (float)(count + 1f);
                }
                points = Lloyd.GenerateCirclePoints(tableRadiusMiddle, tableStartAngleMiddle, tableEndAngleMiddle, orientation, count);
            }
            else
            {
                List<Vector2> samples = new List<Vector2>();
                var innerSamples = Lloyd.GenerateCirclePoints(tableRadiusInner, tableStartAngleInner, tableEndAngleInner, orientation, (int)(2f * Mathf.PI * tableRadiusInner * 10f));
                var outerSamples = Lloyd.GenerateCirclePoints(tableRadiusOuter, tableStartAngleOuter, tableEndAngleOuter, orientation, (int)(2f * Mathf.PI * tableRadiusOuter * 10f));
                outerSamples.Reverse();
                samples.AddRange(innerSamples);
                samples.AddRange(outerSamples);
                List<Vector2> centroids = Lloyd.Centroids(samples, count);
                points = Lloyd.MapSamplesOrderToCentroids(samples, centroids);
            }
            for (int i = 0; i < points.Count; i++) {
                Quaternion rotation = Quaternion.LookRotation(new Vector3(-points[i].x, 0, -points[i].y));
                if (count > middleCapacity && points[i].magnitude < tableRadiusMiddle)
                {
                    // we are on the inner row
                    rotation = Quaternion.LookRotation(new Vector3(points[i].x, 0, points[i].y));
                }
                Quaternion rotationUpsideDown = Quaternion.Euler(180, 0, 0);
                rotation = rotation * rotationUpsideDown;
                var position = new Vector3(lunarTablePosition.x + points[i].x, lunarTablePosition.y + 4.0f , lunarTablePosition.z + points[i].y);
                DicLunarShopTerminals.Add(i, new SpawnCardStruct(position, rotation.eulerAngles));
            }
        }

        private void SpawnLunarShopTerminal()
        {
            ObjectLunarShopTerminals_Spawn.Clear();
            LunarShopTerminalTotalCount = 0;
            if (ModConfig.LunarShopTerminalCount.Value >= 0)
            {
                Main.instance.StartCoroutine(DestroyLunarShopTerminalsDelayed());
            }
        }

        private IEnumerator DestroyLunarShopTerminalsDelayed()
        {
            yield return new WaitForSeconds(1.0f);
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (obj.name.StartsWith("LunarShopTerminal") && !obj.name.EndsWith("(Clone)"))
                {
                    NetworkServer.Destroy(obj);
                }
            }

            DicLunarShopTerminals.Clear();
            SetLunarShopTerminal();
            var gameObjects = DoSpawnGameObject(DicLunarShopTerminals, lunarShopTerminal, ModConfig.LunarShopTerminalCount.Value);
            gameObjects.ForEach(gameObject => {
                gameObject.name = "LunarShopTerminal";
                var purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
                var shopTerminalBehavior = gameObject.GetComponent<ShopTerminalBehavior>();
                if (ModConfig.LunarShopInstanced.Value)
                {
                    var instancedPurchase = gameObject.AddComponent<InstancedPurchase>();
                    instancedPurchase.original.available = purchaseInteraction.available;
                    instancedPurchase.original.pickupIndex = shopTerminalBehavior.pickupIndex;
                    instancedPurchase.original.hasBeenPurchased = shopTerminalBehavior.hasBeenPurchased;
                }
                // purchaseInteraction.onPurchase.AddListener((interactor) => shopTerminalBehavior.SetNoPickup());
                ObjectLunarShopTerminals_Spawn.Add(purchaseInteraction);
                whichStallsHaveBeenBoughtOnce.Add(purchaseInteraction, new List<PlayerCharacterMasterController>());
                Main.instance.StartCoroutine(DelayEffect(purchaseInteraction, 0.1f, false));
            });
        }
    }
}
