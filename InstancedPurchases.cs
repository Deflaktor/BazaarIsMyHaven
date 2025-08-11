using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements.Collections;

namespace BazaarIsMyHome
{
    public partial class InstancedPurchases
    {
        public PlayerCharacterMasterController current;

        public void Hook()
        {
            IL.RoR2.PurchaseInteraction.OnSerialize += PurchaseInteraction_OnSerialize;
            IL.RoR2.ShopTerminalBehavior.OnSerialize += ShopTerminalBehavior_OnSerialize;
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.SetNoPickup += ShopTerminalBehavior_SetNoPickup;
            On.RoR2.ShopTerminalBehavior.SetHasBeenPurchased += ShopTerminalBehavior_SetHasBeenPurchased;
            IL.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
        }

        private Interactability PurchaseInteraction_GetInteractability(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            if (!activator.hasAuthority && self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                var purchased = instancedPurchase.purchases.Get(activator.GetComponent<CharacterBody>().master.playerCharacterMasterController, false);
                var availableBackup = self.available;
                self.available = !purchased;
                var interactability = orig(self, activator);
                self.available = availableBackup;
                return interactability;
            }
            return orig(self, activator);
        }

        private void PurchaseInteraction_OnInteractionBegin(ILContext il)
        {
            HookHelper.HookEndOfMethod(il, (PurchaseInteraction self, Interactor activator) =>
            {
                if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
                {
                    if (activator.hasAuthority)
                    {
                        CloseShopForServer(self.gameObject);
                    }
                    else
                    {
                        var playerCharacterMasterController = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
                        instancedPurchase.purchases[playerCharacterMasterController] = true;
                        instancedPurchase.playerCharacterMasterController = playerCharacterMasterController;

                        var purchaseInteractionDirtyBitsBackup = self.m_SyncVarDirtyBits;
                        self.m_SyncVarDirtyBits |= 4u; // available
                        if (self.gameObject.TryGetComponent(out ShopTerminalBehavior shopTerminalBehavior))
                        {
                            var shopTerminalBehaviorDirtyBitsBackup = shopTerminalBehavior.m_SyncVarDirtyBits;
                            shopTerminalBehavior.m_SyncVarDirtyBits |= 1u; // pickupIndex
                            shopTerminalBehavior.m_SyncVarDirtyBits |= 4u; // hasBeenPurchased

                            CloseShopForClient(self.gameObject, playerCharacterMasterController);

                            shopTerminalBehavior.m_SyncVarDirtyBits = shopTerminalBehaviorDirtyBitsBackup;
                        }
                        else
                        {
                            CloseShopForClient(self.gameObject, playerCharacterMasterController);
                        }
                        self.m_SyncVarDirtyBits = purchaseInteractionDirtyBitsBackup;
                        
                        instancedPurchase.playerCharacterMasterController = null;
                        //var purchased = instancedPurchase.purchases.Get(playerCharacterMasterController, false);
                        //var availableBackup = self.available;
                        //self.available = false;
                        //if (self.gameObject.TryGetComponent(out ShopTerminalBehavior shopTerminalBehavior))
                        //{
                        //    var hasBeenPurchasedBackup = shopTerminalBehavior.hasBeenPurchased;
                        //    var pickupIndexBackup = shopTerminalBehavior.pickupIndex;
                        //    shopTerminalBehavior.hasBeenPurchased = true;
                        //    shopTerminalBehavior.pickupIndex = PickupIndex.none;
                        //    CloseShopForClient(self.gameObject, playerCharacterMasterController);
                        //    shopTerminalBehavior.hasBeenPurchased = hasBeenPurchasedBackup;
                        //    shopTerminalBehavior.pickupIndex = pickupIndexBackup;
                        //}
                        //else
                        //{
                        //    CloseShopForClient(self.gameObject, playerCharacterMasterController);
                        //}
                        //self.available = availableBackup;
                    }
                }
            });
        }

        private void ShopTerminalBehavior_SetHasBeenPurchased(On.RoR2.ShopTerminalBehavior.orig_SetHasBeenPurchased orig, ShopTerminalBehavior self, bool newHasBeenPurchased)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                //if (!newHasBeenPurchased)
                //{
                //    orig(self, newHasBeenPurchased);
                //}
            }
            else
            {
                orig(self, newHasBeenPurchased);
            }
        }

        private void ShopTerminalBehavior_SetNoPickup(On.RoR2.ShopTerminalBehavior.orig_SetNoPickup orig, ShopTerminalBehavior self)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {

            }
            else
            {
                orig(self);
            }
        }

        private void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                //if(newAvailable)
                //{
                //    orig(self, newAvailable);
                //}
            }
            else
            {
                orig(self, newAvailable);
            }
        }

        private void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                if(newPickupIndex != PickupIndex.none) {
                    // changing pickup is fine, but we do not want to set the pickup to none for everyone
                    instancedPurchase.originalPickupIndex = newPickupIndex;
                }
            }
            orig(self, newPickupIndex, newHidden);
        }

        private void ShopTerminalBehavior_OnSerialize(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(x => x.MatchLdfld<ShopTerminalBehavior>("pickupIndex")))
            {
                c.Remove();
                c.EmitDelegate((ShopTerminalBehavior shopTerminalBehavior) =>
                {
                    if(shopTerminalBehavior.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
                    {
                        if(instancedPurchase.playerCharacterMasterController != null && instancedPurchase.purchases.Get(instancedPurchase.playerCharacterMasterController))
                        {
                            return PickupIndex.none;
                        }
                        else
                        {
                            return instancedPurchase.originalPickupIndex;
                        }
                    }
                    // vanilla
                    return shopTerminalBehavior.pickupIndex;
                });
            }
            c.Goto(0);
            while (c.TryGotoNext(x => x.MatchLdfld<ShopTerminalBehavior>("hasBeenPurchased")))
            {
                c.Remove();
                c.EmitDelegate((ShopTerminalBehavior shopTerminalBehavior) =>
                {
                    if (shopTerminalBehavior.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
                    {
                        if (instancedPurchase.playerCharacterMasterController != null && instancedPurchase.purchases.Get(instancedPurchase.playerCharacterMasterController))
                        {
                            return true;
                        }
                        else
                        {
                            return instancedPurchase.originalHasBeenPurchased;
                        }
                    }
                    // vanilla
                    return shopTerminalBehavior.hasBeenPurchased;
                });
            }
        }

        private void PurchaseInteraction_OnSerialize(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while(c.TryGotoNext(x => x.MatchLdfld<PurchaseInteraction>("available")))
            {
                c.Remove();
                c.EmitDelegate((PurchaseInteraction purchaseInteraction) =>
                {
                    if (purchaseInteraction.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
                    {
                        if (instancedPurchase.playerCharacterMasterController != null && instancedPurchase.purchases.Get(instancedPurchase.playerCharacterMasterController))
                        {
                            return false;
                        }
                        else
                        {
                            return instancedPurchase.originalAvailable;
                        }
                    }
                    // vanilla
                    return purchaseInteraction.available;
                });
            }
        }


        private void CloseShopForServer(GameObject gameObject)
        {
            var purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
            var shopTerminalBehavior = gameObject.GetComponent<ShopTerminalBehavior>();

            purchaseInteraction.available = false;
            var pickupIndexBackup = shopTerminalBehavior.pickupIndex;
            shopTerminalBehavior.pickupIndex = PickupIndex.none;
            shopTerminalBehavior.hasBeenPurchased = true;
            shopTerminalBehavior.UpdatePickupDisplayAndAnimations();
            shopTerminalBehavior.pickupIndex = pickupIndexBackup;

            if (shopTerminalBehavior.pickupDisplay != null)
            {
                shopTerminalBehavior.pickupDisplay.SetPickupIndex(PickupIndex.none, shopTerminalBehavior.hidden);
            }
        }

        private void CloseShopForClient(GameObject shop, PlayerCharacterMasterController playerCharacterMasterController)
        {
            if (shop.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                NetworkWriter updateWriter = new NetworkWriter();
                instancedPurchase.playerCharacterMasterController = playerCharacterMasterController;

                var shopNetworkIdentity = shop.GetComponent<NetworkIdentity>();
                for (int j = 0; j < NetworkServer.numChannels; j++)
                {
                    updateWriter.StartMessage(MsgType.UpdateVars);
                    updateWriter.Write(shopNetworkIdentity.netId);
                    bool flag = false;
                    NetworkBehaviour[] behavioursOfSameChannel = shopNetworkIdentity.GetBehavioursOfSameChannel(j, initialState: false);
                    for (int k = 0; k < behavioursOfSameChannel.Length; k++)
                    {
                        NetworkBehaviour networkBehaviour = behavioursOfSameChannel[k];
                        if (networkBehaviour.OnSerialize(updateWriter, initialState: false))
                        {
                            // networkBehaviour.ClearAllDirtyBits();
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        updateWriter.FinishMessage();
                        playerCharacterMasterController.networkUser.connectionToClient.SendWriter(updateWriter, j);
                    }
                }
                instancedPurchase.playerCharacterMasterController = null;
            }
        }
    }
}
