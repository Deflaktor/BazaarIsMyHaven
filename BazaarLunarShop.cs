using BepInEx;
using RoR2;
using RoR2.Networking;
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
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace BazaarIsMyHaven
{
    public class BazaarLunarShop : BazaarBase
    {
        // unique name so QoLChests can exempt the buds; it still starts with the vanilla name
        private const string LunarShopObjectName = "LunarShopTerminal_BazaarIsMyHaven";

        AsyncOperationHandle<GameObject> lunarShopBud;
        AsyncOperationHandle<GameObject> LunarRerollEffect;

        Dictionary<int, SpawnCardStruct> DicLunarShopTerminals = new Dictionary<int, SpawnCardStruct>();
        int generateNewPickupIndex = 0;
        int lunarRecyclerRerolledCount = 0;
        List<GameObject> ObjectLunarShopTerminals_Spawn = new List<GameObject>();
        PlayerCharacterMasterController currentActivator = null;
        Dictionary<PurchaseInteraction, List<PlayerCharacterMasterController>> whichStallsHaveBeenBoughtOnce = new Dictionary<PurchaseInteraction, List<PlayerCharacterMasterController>>();

        public override void Preload()
        {
            lunarShopBud = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
            LunarRerollEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarRecycler/LunarRerollEffect.prefab");
            LunarBudScaleSync.Preload();
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.PurchaseInteraction.ScaleCost += PurchaseInteraction_ScaleCost;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.DropPickup += ShopTerminalBehavior_DropPickup;
            On.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += ShopTerminalBehavior_GenerateNewPickupServer_bool;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }
        public override void RunStart()
        {
            RunEnd();
        }

        public override void RunEnd()
        {
            ObjectLunarShopTerminals_Spawn.Clear();
            whichStallsHaveBeenBoughtOnce.Clear();
            currentActivator = null;
        }

        private void SceneUnloaded(Scene scene)
        {
            if (scene.name == "bazaar")
            {
                RunEnd();
            }
        }
        public override void SetupBazaar()
        {
            lunarRecyclerRerolledCount = 0;
            if (ModConfig.LunarShopSectionEnabled.Value) {
                generateNewPickupIndex = 0;
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

        // Runs the game's purchase and reports whether it was accepted and paid
        private bool TryCompletePurchase(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator, PlayerCharacterMasterController player)
        {
            bool purchaseSucceeded = false;
            UnityEngine.Events.UnityAction<CostTypeDef.PayCostContext, CostTypeDef.PayCostResults> purchaseListener =
                (context, results) =>
                {
                    if (context.activator == activator)
                    {
                        purchaseSucceeded = true;
                    }
                };

            // keep the buyer available to DropPickup while the buy runs
            var previousActivator = currentActivator;
            self.onDetailedPurchaseServer.AddListener(purchaseListener);
            try
            {
                currentActivator = player;
                orig(self, activator);
            }
            finally
            {
                currentActivator = previousActivator;
                self.onDetailedPurchaseServer.RemoveListener(purchaseListener);
            }

            return purchaseSucceeded;
        }

        public void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar() && NetworkServer.active && (ModConfig.LunarShopSectionEnabled.Value || ModConfig.LunarRecyclerSectionEnabled.Value))
            {
                var body = activator ? activator.GetComponent<CharacterBody>() : null;
                var player = body && body.master ? body.master.playerCharacterMasterController : null;
                if (!player)
                {
                    orig(self, activator);
                    return;
                }

                // every spawned bud is tracked with the players who bought from it
                if (ModConfig.LunarShopSectionEnabled.Value && whichStallsHaveBeenBoughtOnce.TryGetValue(self, out var buyers))
                {
                    var playerStruct = Main.instance.GetPlayerStruct(player);
                    bool firstPurchase = !buyers.Contains(player);
                    int usesLeft = ModConfig.LunarShopBuyLimit.Value - playerStruct.LunarShopUseCount;

                    if (firstPurchase && ModConfig.LunarShopBuyLimit.Value >= 0 && usesLeft <= 0)
                    {
                        ChatHelper.LunarShopTerminalUsesLeft(player, usesLeft);
                        return;
                    }

                    if (!TryCompletePurchase(orig, self, activator, player))
                    {
                        return;
                    }

                    // swapping equipment at an already bought bud does not use another purchase
                    if (firstPurchase)
                    {
                        buyers.Add(player);
                        playerStruct.LunarShopUseCount++;
                        if (self.TryGetComponent(out InstancedPurchase instance))
                        {
                            instance.GetOrCreate(player).hasBeenPurchasedOnce = true;
                        }
                        if (ModConfig.LunarShopBuyLimit.Value >= 0)
                        {
                            ChatHelper.LunarShopTerminalUsesLeft(player, ModConfig.LunarShopBuyLimit.Value - playerStruct.LunarShopUseCount);
                        }
                    }
                    return;
                }

                if (self.name.StartsWith("LunarRecycler"))
                {
                    if (!self.available || (ModConfig.LunarRecyclerSectionEnabled.Value && ModConfig.LunarRecyclerRerollLimit.Value >= 0 && lunarRecyclerRerolledCount >= ModConfig.LunarRecyclerRerollLimit.Value))
                    {
                        return;
                    }

                    if (!TryCompletePurchase(orig, self, activator, player))
                    {
                        return;
                    }

                    if (ModConfig.LunarRecyclerSectionEnabled.Value)
                    {
                        lunarRecyclerRerolledCount++;
                        if (ModConfig.LunarRecyclerRerollLimit.Value >= 0)
                        {
                            int rerollsLeft = ModConfig.LunarRecyclerRerollLimit.Value - lunarRecyclerRerolledCount;
                            ChatHelper.LunarRecyclerUsesLeft(rerollsLeft);
                            if (rerollsLeft <= 0)
                            {
                                self.SetAvailable(false);
                            }
                        }
                    }

                    // the scene wires the recycler to the original buds only
                    if (ModConfig.LunarShopSectionEnabled.Value)
                    {
                        float delay = 0f;
                        foreach (var shop in ObjectLunarShopTerminals_Spawn)
                        {
                            Main.instance.StartCoroutine(DelayRerollEffect(shop, delay));
                            delay += 0.1f;
                        }
                    }
                    return;
                }
            }
            orig(self, activator);
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
                        // keep the game's cooldown, only block availability past the limit
                        newAvailable = newAvailable && lunarRecyclerRerolledCount < ModConfig.LunarRecyclerRerollLimit.Value;
                    }
                }
            }
            orig(self, newAvailable);
        }
        private void ShopTerminalBehavior_DropPickup(On.RoR2.ShopTerminalBehavior.orig_DropPickup orig, ShopTerminalBehavior self)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar() && NetworkServer.active && self.name.StartsWith("LunarShopTerminal"))
            {
                if (!ModConfig.LunarShopSectionEnabled.Value)
                {
                    // vanilla buds have the same client animation bug
                    orig(self);
                    SendPurchasedFlagToClients(self);
                    return;
                }
                if (ModConfig.LunarShopBuyToInventory.Value)
                {
                    var body = currentActivator && currentActivator.master ? currentActivator.master.GetBody() : null;
                    if (!body || !body.inventory)
                    {
                        // nowhere to put the item, so drop it instead of losing it
                        orig(self);
                        SendPurchasedFlagToClients(self);
                        self.SetNoPickup();
                        return;
                    }
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
                        SendPurchasedFlagToClients(self);
                        self.SetNoPickup();
                    }
                }
                else
                {
                    orig(self);
                    SendPurchasedFlagToClients(self);
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
                if (TryGenerateLunarShopPickup(out var pickup))
                {
                    self.SetPickup(pickup, newHidden);
                }
            }
            else
            {
                orig(self, newHidden);
            }
        }

        // Clients clear a bud's pickup before its purchased flag, so it never plays its open animation.
        // Send the purchased flag alone first. Instanced buds already send updates in this order.
        private void SendPurchasedFlagToClients(ShopTerminalBehavior shop)
        {
            if (!NetworkServer.active || shop.GetComponent<InstancedPurchase>() || !shop.hasBeenPurchased)
            {
                return;
            }
            var identity = shop.GetComponent<NetworkIdentity>();
            if (!identity || identity.observers == null)
            {
                return;
            }

            int channel = shop.GetNetworkChannel();
            var writer = new NetworkWriter();
            writer.StartMessage(MsgType.UpdateVars);
            writer.Write(identity.netId);
            // every component must be serialized in the order the client reads them
            foreach (var behaviour in identity.GetBehavioursOfSameChannel(channel, false))
            {
                uint originalDirtyBits = behaviour.m_SyncVarDirtyBits;
                try
                {
                    // dirty bit 4 is ShopTerminalBehavior.hasBeenPurchased; pickup is left out
                    behaviour.m_SyncVarDirtyBits = behaviour == shop ? 4u : 0u;
                    behaviour.OnSerialize(writer, false);
                }
                finally
                {
                    // leave pending changes to the normal update
                    behaviour.m_SyncVarDirtyBits = originalDirtyBits;
                }
            }
            writer.FinishMessage();

            foreach (var connection in identity.observers)
            {
                if (connection.isReady && !Util.ConnectionIsLocal(connection) && !connection.SendWriter(writer, channel))
                {
                    Log.LogWarning("Could not send the bud purchased flag.");
                }
            }
        }

        private bool TryGenerateLunarShopPickup(out UniquePickup pickup)
        {
            int itemIndex = ModConfig.LunarShopSequentialItems.Value ? generateNewPickupIndex : -1;

            var resolvedItems = new Dictionary<PickupIndex, int>();
            ItemStringParser.ItemStringParser.ParseItemString(ModConfig.LunarShopItemList.Value, resolvedItems, Log.GetSource(), false, itemIndex);
            foreach (var (pickupIndex, amount) in resolvedItems)
            {
                if (amount > 0 && PickupCatalog.GetPickupDef(pickupIndex) != null)
                {
                    pickup = new UniquePickup(pickupIndex);
                    generateNewPickupIndex++;
                    return true;
                }
            }

            pickup = UniquePickup.none;
            Log.LogError($"Could not get a pickup index from LunarShopItemList: {ModConfig.LunarShopItemList.Value}");
            return false;
        }

        private IEnumerator DelayRerollEffect(GameObject shop, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!NetworkServer.active || !shop)
            {
                yield break;
            }
            var terminal = shop.GetComponent<ShopTerminalBehavior>();
            var purchase = shop.GetComponent<PurchaseInteraction>();
            if (!terminal || !purchase)
            {
                yield break;
            }

            if (shop.TryGetComponent(out InstancedPurchase instance))
            {
                // check after the delay; a player may have bought this bud meanwhile
                bool anyEligiblePlayer = PlayerCharacterMasterController.instances.Any(pc => pc && instance.GetOrOriginal(pc).CanReroll);
                if (!anyEligiblePlayer || !TryGenerateLunarShopPickup(out var pickup))
                {
                    yield break;
                }

                // roll once per bud; every player who has not bought it gets the same new offer
                if (instance.original.CanReroll)
                {
                    instance.original.pickup = pickup;
                    instance.original.hidden = false;
                }
                foreach (var state in instance.purchases.Values)
                {
                    if (state.CanReroll)
                    {
                        state.pickup = pickup;
                        state.hidden = false;
                    }
                }

                // sends each client its own state, used buds too
                InstancedPurchases.UpdateAll(shop);
            }
            else
            {
                // keep equipment swapped into a bought bud
                if (whichStallsHaveBeenBoughtOnce.TryGetValue(purchase, out var buyers) && buyers.Count > 0)
                {
                    yield break;
                }
                if (!purchase.available || terminal.hasBeenPurchased || terminal.pickup.Equals(UniquePickup.none) || !TryGenerateLunarShopPickup(out var pickup))
                {
                    yield break;
                }
                terminal.SetPickup(pickup, false);
            }

            SpawnRerollEffect(shop, instance);
        }

        private void SpawnRerollEffect(GameObject shop, InstancedPurchase instance)
        {
            if (EffectManager.DisableAllEffectSpawning)
            {
                return;
            }

            var prefab = LunarRerollEffect.WaitForCompletion();
            var effectData = new EffectData
            {
                origin = shop.transform.position + Vector3.up * 2.5f,
                rotation = Quaternion.identity,
                scale = 2f,
                color = new Color32(255, 255, 255, 255)
            };

            if (!instance)
            {
                EffectManager.SpawnEffect(prefab, effectData, transmit: true);
                return;
            }

            // only players whose offer changed see the effect; the vanilla message works for unmodded clients
            var message = new EffectManager.EffectMessage
            {
                effectIndex = EffectCatalog.FindEffectIndexFromPrefab(prefab),
                effectData = effectData
            };
            var recipients = new HashSet<NetworkConnection>();
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (!player || !instance.GetOrOriginal(player).CanReroll)
                {
                    continue;
                }
                var connection = player.networkUser ? player.networkUser.connectionToClient : null;
                if (connection != null && connection.isReady && recipients.Add(connection))
                {
                    connection.SendByChannel(UmsgType.Effect, message, QosChannelIndex.effects.intVal);
                }
            }
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
                if (count >= 3 && count < 10)
                {
                    // widen the arc for few buds, so 5 buds sit roughly where the originals were
                    const float maximumExtraArc = 90f;
                    const float maximumCorrectedArc = 180f;

                    // full fix at 3 buds, gone at 10
                    float correctionWeight = (10f - count) / 7f;
                    float middleAngle = (tableStartAngleMiddle + tableEndAngleMiddle) / 2f;
                    float currentArc = tableEndAngleMiddle - tableStartAngleMiddle;
                    float correctedArc = Mathf.Min(currentArc + maximumExtraArc * correctionWeight, maximumCorrectedArc);

                    tableStartAngleMiddle = middleAngle - correctedArc / 2f;
                    tableEndAngleMiddle = middleAngle + correctedArc / 2f;
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
                // turn the buds so their price holograms face outward
                rotation = rotation * Quaternion.Euler(0, 180, 0);
                var position = new Vector3(lunarTablePosition.x + points[i].x, lunarTablePosition.y, lunarTablePosition.z + points[i].y);
                DicLunarShopTerminals.Add(i, new SpawnCardStruct(position, rotation.eulerAngles));
            }
        }

        // Buds clear their pickup after DropPickup. Our hook does that too, unless an equipment swap leaves the new equipment in the bud.
        private void DisableLunarBudPickupClearing(PurchaseInteraction purchaseInteraction, ShopTerminalBehavior shopTerminalBehavior)
        {
            // buds still add their SetNoPickup listener on this old event
#pragma warning disable CS0618
            var purchaseEvent = purchaseInteraction.onPurchase;
#pragma warning restore CS0618
            if (purchaseEvent == null)
            {
                Log.LogWarning("The bud's purchase event is missing");
                return;
            }
            for (int index = 0; index < purchaseEvent.GetPersistentEventCount(); index++)
            {
                if (purchaseEvent.GetPersistentTarget(index) == shopTerminalBehavior && purchaseEvent.GetPersistentMethodName(index) == nameof(shopTerminalBehavior.SetNoPickup))
                {
                    purchaseEvent.SetPersistentListenerState(index, UnityEngine.Events.UnityEventCallState.Off);
                }
            }
        }

        private void SpawnLunarShopTerminal()
        {
            // QoLChests would hide bought buds, breaking instanced purchases and rerolls
            if (ModCompatibilityQoLChests.enabled)
            {
                ModCompatibilityQoLChests.RegisterQoLChestsBlacklist(LunarShopObjectName);
            }

            ObjectLunarShopTerminals_Spawn.Clear();
            DicLunarShopTerminals.Clear();
            SetLunarShopTerminal();

            // replace the original buds with the set amount
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (obj.name.StartsWith("LunarShopTerminal"))
                {
                    NetworkServer.Destroy(obj);
                }
            }
            var gameObjects = DoSpawnGameObject(DicLunarShopTerminals, lunarShopBud, ModConfig.LunarShopAmount.Value);
            ObjectLunarShopTerminals_Spawn.AddRange(gameObjects);

            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject gameObject = gameObjects[i];
                gameObject.name = LunarShopObjectName;
                // the prefab is half scale; buds placed in the scene are not
                gameObject.transform.localScale = Vector3.one;
                var purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
                var shopTerminalBehavior = gameObject.GetComponent<ShopTerminalBehavior>();
                DisableLunarBudPickupClearing(purchaseInteraction, shopTerminalBehavior);
                // the scene wires the Newt's comment only to the original buds
                purchaseInteraction.onDetailedPurchaseServer.AddListener((context, results) =>
                {
                    if (BazaarController.instance)
                        BazaarController.instance.CommentOnLunarPurchase();
                });
                if (ModConfig.LunarShopInstancedPurchases.Value)
                {
                    var instancedPurchase = gameObject.AddComponent<InstancedPurchase>();
                    instancedPurchase.lunarShopIndex = i;
                    instancedPurchase.original.available = purchaseInteraction.available;
                    instancedPurchase.original.pickup = shopTerminalBehavior.pickup;
                    instancedPurchase.original.hasBeenPurchased = shopTerminalBehavior.hasBeenPurchased;
                    instancedPurchase.original.hidden = shopTerminalBehavior.hidden;
                }
                // purchaseInteraction.onPurchase.AddListener((interactor) => shopTerminalBehavior.SetNoPickup());

                whichStallsHaveBeenBoughtOnce.Add(purchaseInteraction, new List<PlayerCharacterMasterController>());
                //Main.instance.StartCoroutine(DelayRerollEffect(shopTerminalBehavior, 0.1f, false));
            }

            LunarBudScaleSync.Apply(gameObjects);
        }
    }
}
