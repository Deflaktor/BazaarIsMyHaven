using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements.Collections;

namespace BazaarIsMyHome
{
    public partial class InstancedPurchases
    {
        public PlayerCharacterMasterController currentInteractor;

        public void Hook()
        {
            IL.RoR2.PurchaseInteraction.OnSerialize += PurchaseInteraction_OnSerialize;
            IL.RoR2.ShopTerminalBehavior.OnSerialize += ShopTerminalBehavior_OnSerialize;
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.SetHasBeenPurchased += ShopTerminalBehavior_SetHasBeenPurchased;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ShopTerminalBehavior.CurrentPickupIndex += ShopTerminalBehavior_CurrentPickupIndex;
        }

        private Interactability PurchaseInteraction_GetInteractability(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            if (!activator.hasAuthority && self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                PlayerCharacterMasterController pc = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
                var instancedPurchaseAvailable = instancedPurchase.GetOrOriginal(pc).available;
                var availableBackup = self.available;
                self.available = instancedPurchaseAvailable;
                var interactability = orig(self, activator);
                self.available = availableBackup;
                return interactability;
            }
            return orig(self, activator);
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            try { 
                currentInteractor = activator.GetComponent<CharacterBody>().master.playerCharacterMasterController;
                orig(self, activator);

                if (currentInteractor.hasAuthority)
                {
                    UpdateShopForServer(self.gameObject, currentInteractor);
                }
                else
                {
                    UpdateShopForClient(self.gameObject, currentInteractor);
                }

            } finally {
                currentInteractor = null;
            }
        }

        private void ShopTerminalBehavior_SetHasBeenPurchased(On.RoR2.ShopTerminalBehavior.orig_SetHasBeenPurchased orig, ShopTerminalBehavior self, bool newHasBeenPurchased)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                if (currentInteractor != null)
                {
                    instancedPurchase.GetOrCreate(currentInteractor).hasBeenPurchased = newHasBeenPurchased;
                    UpdateShop(self.gameObject, currentInteractor);
                }
                else
                {
                    instancedPurchase.original.hasBeenPurchased = newHasBeenPurchased;
                    orig(self, newHasBeenPurchased);
                    return;
                    // self.hasBeenPurchased = newHasBeenPurchased;
                }
            }
            else
            {
                orig(self, newHasBeenPurchased);
            }
        }

        //private void ShopTerminalBehavior_SetNoPickup(On.RoR2.ShopTerminalBehavior.orig_SetNoPickup orig, ShopTerminalBehavior self)
        //{
        //    if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
        //    {
        //        if(currentInteractor != null)
        //        {
        //            instancedPurchase.GetOrOriginal(currentInteractor).pickupIndex = PickupIndex.none;
        //        }
        //        else
        //        {
        //            self.pickupIndex = PickupIndex.none;
        //        }
        //    }
        //    else
        //    {
        //        orig(self);
        //    }
        //}

        private void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                if (currentInteractor != null) {
                    instancedPurchase.GetOrCreate(currentInteractor).available = newAvailable;
                    UpdateShop(self.gameObject, currentInteractor);
                }
                else
                {
                    instancedPurchase.original.available = newAvailable;
                    orig(self, newAvailable);
                    return;
                    //self.available = newAvailable;
                }
            }
            else
            {
                orig(self, newAvailable);
            }
        }

        private void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase) && NetworkServer.active)
            {
                if (currentInteractor != null)
                {
                    instancedPurchase.GetOrCreate(currentInteractor).pickupIndex = newPickupIndex;
                    instancedPurchase.GetOrCreate(currentInteractor).hidden = newHidden;
                    UpdateShop(self.gameObject, currentInteractor);
                }
                else
                {
                    instancedPurchase.original.pickupIndex = newPickupIndex;
                    instancedPurchase.original.hidden = newHidden;
                    orig(self, newPickupIndex, newHidden);
                    return;
                    //self.pickupIndex = newPickupIndex;
                    //self.hidden = newHidden;
                }
            }
            else
            {
                orig(self, newPickupIndex, newHidden);
            }
        }
        private PickupIndex ShopTerminalBehavior_CurrentPickupIndex(On.RoR2.ShopTerminalBehavior.orig_CurrentPickupIndex orig, ShopTerminalBehavior self)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase) && NetworkServer.active)
            {
                return instancedPurchase.GetOrOriginal(currentInteractor).pickupIndex;
            }
            return orig(self);
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
                        return instancedPurchase.GetOrOriginal(instancedPurchase.pcClient).pickupIndex;
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
                        return instancedPurchase.GetOrOriginal(instancedPurchase.pcClient).hasBeenPurchased;
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
                        return instancedPurchase.GetOrOriginal(instancedPurchase.pcClient).available;
                    }
                    // vanilla
                    return purchaseInteraction.available;
                });
            }
        }

        private void UpdateShop(GameObject gameObject, PlayerCharacterMasterController pc)
        {
            if (currentInteractor == null)
            {
                Log.LogError("Internal Error: During UpdateShop it is expected that currentInteractor is not null. Either the game code has changed or there is an incompatibility with another mod.");
            }
            if (currentInteractor.hasAuthority)
            {
                UpdateShopForServer(gameObject, currentInteractor);
            }
            //else
            //{
            //    UpdateShopForClient(gameObject, currentInteractor);
            //}
        }

        private void UpdateShopForServer(GameObject shop, PlayerCharacterMasterController pc)
        {
            if (shop.TryGetComponent(out InstancedPurchase instancedPurchase)) {
                if (shop.TryGetComponent(out PurchaseInteraction purchaseInteraction))
                {
                    purchaseInteraction.available = instancedPurchase.GetOrOriginal(pc).available;
                }
                if (shop.TryGetComponent(out ShopTerminalBehavior shopTerminalBehavior))
                {
                    shopTerminalBehavior.hasBeenPurchased = instancedPurchase.GetOrOriginal(pc).hasBeenPurchased;
                    shopTerminalBehavior.pickupIndex = instancedPurchase.GetOrOriginal(pc).pickupIndex;
                    shopTerminalBehavior.UpdatePickupDisplayAndAnimations();
                }
            }
            //if (shopTerminalBehavior.pickupDisplay != null)
            //{
            //    shopTerminalBehavior.pickupDisplay.SetPickupIndex(PickupIndex.none, shopTerminalBehavior.hidden);
            //}
            // shopTerminalBehavior.pickupIndex = instancedPurchase.original.pickupIndex;
        }

        private void UpdateShopForClient(GameObject shop, PlayerCharacterMasterController pc)
        {
            if (shop.TryGetComponent(out InstancedPurchase instancedPurchase))
            {
                // set which properties we want to sync
                var syncVarDirtyBitsBackups = new Dictionary<NetworkBehaviour, uint>();
                if (shop.TryGetComponent(out ShopTerminalBehavior shopTerminalBehavior))
                {
                    syncVarDirtyBitsBackups[shopTerminalBehavior] = shopTerminalBehavior.m_SyncVarDirtyBits;
                    shopTerminalBehavior.m_SyncVarDirtyBits |= 1u; // pickupIndex
                    shopTerminalBehavior.m_SyncVarDirtyBits |= 4u; // hasBeenPurchased
                }
                if (shop.TryGetComponent(out PurchaseInteraction purchaseInteraction))
                {
                    syncVarDirtyBitsBackups[purchaseInteraction] = purchaseInteraction.m_SyncVarDirtyBits;
                    purchaseInteraction.m_SyncVarDirtyBits |= 4u; // available
                }

                NetworkWriter updateWriter = new NetworkWriter();
                instancedPurchase.pcClient = pc;

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
                            // restore m_SyncVarDirtyBits
                            if(syncVarDirtyBitsBackups.TryGetValue(networkBehaviour, out uint syncVarDirtyBitsBackup)) {
                                networkBehaviour.m_SyncVarDirtyBits = syncVarDirtyBitsBackup;
                            }
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        updateWriter.FinishMessage();
                        pc.networkUser.connectionToClient.SendWriter(updateWriter, j);
                    }
                }
                instancedPurchase.pcClient = null;
            }
        }
    }
}
