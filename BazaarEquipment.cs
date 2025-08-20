using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    public class BazaarEquipment : BazaarBase
    {
        AsyncOperationHandle<GameObject> multiShopEquipmentTerminal;
        //AsyncOperationHandle<InteractableSpawnCard> iscTripleShopEquipment;

        Dictionary<int, SpawnCardStruct> DicEquipments = new Dictionary<int, SpawnCardStruct>();
        PlayerCharacterMasterController currentActivator = null;
        PickupIndex placeEquipment = PickupIndex.none;

        public override void Init()
        {
            multiShopEquipmentTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MultiShopEquipmentTerminal/MultiShopEquipmentTerminal.prefab");
            //iscTripleShopEquipment = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/TripleShopEquipment/iscTripleShopEquipment.asset");
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ShopTerminalBehavior.DropPickup += ShopTerminalBehavior_DropPickup;
        }

        public override void SetupBazaar()
        {
            if(ModConfig.EquipmentSectionEnabled.Value) {
                SpawnEquipment();
            }
        }

        public void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value && ModConfig.EquipmentSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (ModConfig.EquipmentCount.Value > 0)
                {
                    // 主动装备
                    if (self.name.StartsWith("MultiShopEquipmentTerminal"))
                    {
                        //ChatHelper.Send("一台主动装备已修改");
                        self.cost = ModConfig.EquipmentCost.Value;
                        self.Networkcost = ModConfig.EquipmentCost.Value;
                        //self.costType = CostTypeIndex.PercentHealth;
                    }
                }
            }
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && ModConfig.EquipmentSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("MultiShopEquipmentTerminal"))
                {
                    var playerCharacterMasterController = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
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

        private void ShopTerminalBehavior_DropPickup(On.RoR2.ShopTerminalBehavior.orig_DropPickup orig, ShopTerminalBehavior self)
        {
            if (ModConfig.EnableMod.Value && ModConfig.EquipmentSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active && self.name.StartsWith("MultiShopEquipmentTerminal"))
            {
                if(ModConfig.EquipmentBuyToInventory.Value) { 
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
                            if(currentActivator.master.inventory.GetEquipmentIndex() != EquipmentIndex.None)
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

        private void SetEquipment()
        {
            List<int> total = new List<int> { 0, 1, 2 };
            if(ModConfig.ReplaceLunarSeersWithEquipment.Value)
            { 
                total = new List<int> { 2, 3, 4 };

                // left seer stand
                DicEquipments.Add(0, new SpawnCardStruct(new Vector3(-133.9731f, -23.4f, -10.71112f), new Vector3(0f, 120.0f, 0.0f)));
                // right seer stand
                DicEquipments.Add(1, new SpawnCardStruct(new Vector3(-128.0793f, -23.4f, -7.056283f), new Vector3(0f, 160.0f, 0.0f)));
            }
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }

            DicEquipments.Add(random[0], new SpawnCardStruct(new Vector3(-139.5818f, -23.561f, -1.7491f), new Vector3(0.0f, 175.0f, 0.0f)));
            DicEquipments.Add(random[1], new SpawnCardStruct(new Vector3(-136.5639f, -23.7163f, -0.5618f), new Vector3(0.0f, 145.0f, 0.0f)));
            DicEquipments.Add(random[2], new SpawnCardStruct(new Vector3(-134.82f, -23.36f, 1.85f), new Vector3(0.0f, 105.0f, 0.0f)));
        }

        private void SpawnEquipment()
        {
            if (ModConfig.EquipmentCount.Value > 0)
            {
                // 主动装备
                DicEquipments.Clear();
                SetEquipment();

                var count = ModConfig.EquipmentCount.Value;
                if (!ModConfig.ReplaceLunarSeersWithEquipment.Value && count > 3)
                {
                    count = 3;
                }
                if (ModConfig.ReplaceLunarSeersWithEquipment.Value && count > 5) {
                    count = 5;
                }

                DoSpawnGameObject(DicEquipments, multiShopEquipmentTerminal, count).ForEach(gameObject => {
                    var purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
                    var shopTerminalBehavior = gameObject.GetComponent<ShopTerminalBehavior>();
                    if(ModConfig.EquipmentInstanced.Value) {
                        var instancedPurchase = gameObject.AddComponent<InstancedPurchase>();
                        instancedPurchase.original.available = purchaseInteraction.available;
                        instancedPurchase.original.pickupIndex = shopTerminalBehavior.pickupIndex;
                        instancedPurchase.original.hasBeenPurchased = shopTerminalBehavior.hasBeenPurchased;
                    }
                });
            }
        }
    }
}
