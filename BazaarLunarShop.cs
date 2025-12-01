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
        int currentLunarShopStaticItemIndex = 0;
        int generateNewPickupIndex = 0;
        int lunarRecyclerRerolledCount = 0;
        List<GameObject> ObjectLunarShopTerminals_Spawn = new List<GameObject>();
        PlayerCharacterMasterController currentActivator = null;
        Dictionary<PurchaseInteraction, List<PlayerCharacterMasterController>> whichStallsHaveBeenBoughtOnce = new Dictionary<PurchaseInteraction, List<PlayerCharacterMasterController>>();

        public override void Preload()
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
            On.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += ShopTerminalBehavior_GenerateNewPickupServer_bool;
        }
        public override void RunStart()
        {

        }
        public override void SetupBazaar()
        {
            if (ModConfig.LunarRecyclerSectionEnabled.Value)
            {
                lunarRecyclerRerolledCount = 0;
            }
            if (ModConfig.LunarShopSectionEnabled.Value) {
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
                if (ModConfig.LunarShopSectionEnabled.Value && ModConfig.LunarShopCost.Value >= 0) 
                {
                    if (self.name.StartsWith("LunarShopTerminal"))
                    {
                        self.cost = ModConfig.LunarShopCost.Value;
                        self.Networkcost = ModConfig.LunarShopCost.Value;
                    }
                }
                if (ModConfig.LunarRecyclerSectionEnabled.Value)
                {
                    if (self.name.StartsWith("LunarRecycler"))
                    {
                        if (ModConfig.LunarRecyclerAvailable.Value && ModConfig.LunarRecyclerCost.Value >= 0)
                        {
                            self.cost = ModConfig.LunarRecyclerCost.Value;
                            self.Networkcost = ModConfig.LunarRecyclerCost.Value;
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

                    if (usesLeft <= 0 && ModConfig.LunarShopBuyLimit.Value >= 0) {
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
                            if (ModConfig.LunarShopBuyLimit.Value >= 0) {
                                var usesLeft = ModConfig.LunarShopBuyLimit.Value - playerStruct.LunarShopUseCount;
                                ChatHelper.LunarShopTerminalUsesLeft(playerCharacterMasterController, usesLeft);
                            }
                            whichStallsHaveBeenBoughtOnce[self].Add(playerCharacterMasterController);
                        }
                    }
                    if (self.name.StartsWith("LunarRecycler"))
                    {
                        if (ModConfig.LunarShopReplaceLunarBudsWithTerminals.Value)
                        {
                            float time = 0f;
                            foreach (GameObject lunarShopTerminal in ObjectLunarShopTerminals_Spawn)
                            {
                                Main.instance.StartCoroutine(DelayRerollEffect(lunarShopTerminal, time, currentLunarShopStaticItemIndex));
                                currentLunarShopStaticItemIndex += 1;
                                time = time + 0.1f;
                            }
                        }
                    }
                }
                if (ModConfig.EnableMod.Value && ModConfig.LunarRecyclerSectionEnabled.Value && ModConfig.LunarRecyclerRerollLimit.Value >= 0 && IsCurrentMapInBazaar() && NetworkServer.active)
                {
                    if(self.name.StartsWith("LunarRecycler"))
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
                    scalar = (float)ModConfig.LunarRecyclerCostMultiplier.Value;
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
                    if(ModConfig.LunarRecyclerRerollLimit.Value >= 0) { 
                        newAvailable = lunarRecyclerRerolledCount < ModConfig.LunarRecyclerRerollLimit.Value;
                    } else
                    {
                        newAvailable = true;
                    }
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
                        var droppedEquipment = Helper.GivePickup(body, self.CurrentPickup().pickupIndex, self.transform.position, false);
                        if (droppedEquipment != EquipmentIndex.None)
                        {
                            self.SetPickup(new UniquePickup(PickupCatalog.FindPickupIndex(droppedEquipment)));
                            var purchaseInteraction = self.GetComponent<PurchaseInteraction>();
                            purchaseInteraction.SetAvailable(true);
                        }
                        else
                        {
                            self.SetHasBeenPurchased(newHasBeenPurchased: true);
                            self.SetNoPickup();
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

        private void ShopTerminalBehavior_GenerateNewPickupServer_bool(On.RoR2.ShopTerminalBehavior.orig_GenerateNewPickupServer_bool orig, ShopTerminalBehavior self, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.LunarShopSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active && self.name.StartsWith("LunarShopTerminal"))
            {
                if (!ModConfig.LunarShopSequentialItems.Value)
                    generateNewPickupIndex = -1;

                Dictionary<PickupIndex, int> resolvedItems = new Dictionary<PickupIndex, int>();
                ItemStringParser.ItemStringParser.ParseItemString(ModConfig.LunarShopItemList.Value, resolvedItems, Log.GetSource(), false, generateNewPickupIndex);
                bool set = false;
                foreach (var (pickupIndex, amount) in resolvedItems)
                {
                    if (amount > 0)
                    {
                        self.SetPickup(new UniquePickup(pickupIndex), newHidden);
                        set = true;
                        generateNewPickupIndex += 1;
                        break;
                    }
                }
                if (!set)
                {
                    Log.LogError($"Could not get a proper pickup index from EquipmentReplaceWithEliteList: {ModConfig.EquipmentReplaceWithEliteList.Value}");
                }
            }
            else
            {
                orig(self, newHidden);
            }
        }

        IEnumerator DelayRerollEffect(GameObject lunarShopTerminal, float time, int itemIndex)
        {
            yield return new WaitForSeconds(time);

            generateNewPickupIndex = itemIndex;
            lunarShopTerminal.GetComponent<ShopTerminalBehavior>().GenerateNewPickupServer();
            SpawnEffect(LunarRerollEffect, lunarShopTerminal.transform.position - Vector3.up * 2.5f, new Color32(255, 255, 255, 255), 2f);
        }

        public static List<Vector2> GenerateCirclePoints(float radius, float startAngle, float endAngle, float orientation, int numberOfPoints)
        {
            List<Vector2> points = new List<Vector2>();
            float angleStep = (endAngle - startAngle) / (numberOfPoints - 1);
            if (numberOfPoints <= 1)
            {
                angleStep = 0;
            }

            for (int i = 0; i < numberOfPoints; i++)
            {
                float angleInDegrees = startAngle + i * angleStep + orientation;
                float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
                float x = radius * Mathf.Cos(angleInRadians);
                float y = radius * Mathf.Sin(angleInRadians);
                points.Add(new Vector2(x, y));
            }
            return points;
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
            const float middleCapacity = 10;
            const float maxCapacity = 20;

            List<Vector2> points = new List<Vector2>();

            int count = 0;
            if (ModConfig.SpawnCountByStage.Value)
                count = SetCountbyGameStage(ModConfig.LunarShopAmount.Value, ModConfig.SpawnCountOffset.Value);
            else
                count = ModConfig.LunarShopAmount.Value;

            if (count <= middleCapacity)
            {
                if (count < middleCapacity)
                {
                    // place them closer together
                    float angleDiff = tableEndAngleMiddle - tableStartAngleMiddle;
                    tableStartAngleMiddle += angleDiff / (float)(count + 1f);
                    tableEndAngleMiddle -= angleDiff / (float)(count + 1f);
                }
                points = GenerateCirclePoints(tableRadiusMiddle, tableStartAngleMiddle, tableEndAngleMiddle, orientation, count);
                points.Reverse();
            }
            else
            {
                List<Vector2> samples = new List<Vector2>();
                var innerCountIdeal = count * tableRadiusInner / (tableRadiusInner + tableRadiusOuter);
                var outerCountIdeal = count * tableRadiusOuter / (tableRadiusInner + tableRadiusOuter);
                int innerCount = Mathf.RoundToInt(innerCountIdeal);
                int outerCount = Mathf.RoundToInt(outerCountIdeal);
                if(innerCount + outerCount != count)
                {
                    // possibility 1: round inner
                    var innerDistanceInnerRounded = 2f * Mathf.PI + tableRadiusInner / innerCount;
                    var outerDistanceInnerRounded = 2f * Mathf.PI + tableRadiusOuter / (count - innerCount);
                    // possibility 2: round outer
                    var innerDistanceOuterRounded = 2f * Mathf.PI + tableRadiusInner / outerCount;
                    var outerDistanceOuterRounded = 2f * Mathf.PI + tableRadiusOuter / (count - outerCount);
                    // choose the one where the distance is more equal
                    if(Math.Abs(innerDistanceInnerRounded - outerDistanceInnerRounded) < Math.Abs(innerDistanceOuterRounded - outerDistanceOuterRounded))
                    {
                        outerCount = count - innerCount;
                    }
                    else
                    {
                        innerCount = count - outerCount;
                    }
                }
                if (count < maxCapacity)
                {
                    // place them closer together
                    float angleDiff = tableEndAngleOuter - tableStartAngleOuter;
                    tableStartAngleOuter += angleDiff / (float)(outerCount + 1f);
                    tableEndAngleOuter -= angleDiff / (float)(outerCount + 1f);
                    angleDiff = tableEndAngleInner - tableStartAngleInner;
                    tableStartAngleInner += angleDiff / (float)(innerCount + 1f);
                    tableEndAngleInner -= angleDiff / (float)(innerCount + 1f);
                }

                var innerSamples = GenerateCirclePoints(tableRadiusInner, tableStartAngleInner, tableEndAngleInner, orientation, innerCount);
                var outerSamples = GenerateCirclePoints(tableRadiusOuter, tableStartAngleOuter, tableEndAngleOuter, orientation, outerCount);
                outerSamples.Reverse();
                points.AddRange(outerSamples);
                points.AddRange(innerSamples);
                //List<Vector2> centroids = Lloyd.Centroids(samples, count);
                //points = Lloyd.MapSamplesOrderToCentroids(samples, centroids);
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
            currentLunarShopStaticItemIndex = 0;
            DicLunarShopTerminals.Clear();
            SetLunarShopTerminal();

            // find original lunar buds
            var gameObjects = new List<GameObject>();
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (obj.name.StartsWith("LunarShopTerminal"))
                {
                    gameObjects.Add(obj);
                }
            }

            if (ModConfig.LunarShopReplaceLunarBudsWithTerminals.Value)
            {
                // Remove original Lunar Buds
                gameObjects.ForEach(NetworkServer.Destroy);
                // Spawn Shop Terminals
                gameObjects = DoSpawnGameObject(DicLunarShopTerminals, lunarShopTerminal, ModConfig.LunarShopAmount.Value);
                ObjectLunarShopTerminals_Spawn.AddRange(gameObjects);
            }

            gameObjects.ForEach(gameObject => {
                gameObject.name = "LunarShopTerminal";
                var purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
                var shopTerminalBehavior = gameObject.GetComponent<ShopTerminalBehavior>();
                if (ModConfig.LunarShopInstancedPurchases.Value)
                {
                    var instancedPurchase = gameObject.AddComponent<InstancedPurchase>();
                    instancedPurchase.original.available = purchaseInteraction.available;
                    instancedPurchase.original.pickup = shopTerminalBehavior.pickup;
                    instancedPurchase.original.hasBeenPurchased = shopTerminalBehavior.hasBeenPurchased;
                }
                // purchaseInteraction.onPurchase.AddListener((interactor) => shopTerminalBehavior.SetNoPickup());
                
                whichStallsHaveBeenBoughtOnce.Add(purchaseInteraction, new List<PlayerCharacterMasterController>());
                //Main.instance.StartCoroutine(DelayRerollEffect(shopTerminalBehavior, 0.1f, false));
            });
        }
    }
}
