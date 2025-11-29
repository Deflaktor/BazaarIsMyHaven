using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements.Collections;

namespace BazaarIsMyHaven
{
    public partial class InstancedPurchases
    {
        public PlayerCharacterMasterController currentInteractor;

        public void Hook()
        {
            IL.RoR2.PurchaseInteraction.OnSerialize += PurchaseInteraction_OnSerialize;
            IL.RoR2.ShopTerminalBehavior.OnSerialize += ShopTerminalBehavior_OnSerialize;
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.SetHasBeenPurchased += ShopTerminalBehavior_SetHasBeenPurchased;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ShopTerminalBehavior.CurrentPickupIndex += ShopTerminalBehavior_CurrentPickupIndex;
            On.RoR2.ShopTerminalBehavior.CurrentPickup += ShopTerminalBehavior_CurrentPickup;
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
                }
            }
            else
            {
                orig(self, newAvailable);
            }
        }


        private void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase) && NetworkServer.active)
            {
                if (currentInteractor != null)
                {
                    // someone is interacting with the shop terminal -> set the pickup index only for the interactor
                    instancedPurchase.GetOrCreate(currentInteractor).pickup = newPickup;
                    instancedPurchase.GetOrCreate(currentInteractor).hidden = newHidden;
                    UpdateShop(self.gameObject, currentInteractor);
                }
                else
                {
                    // no one is interacting with the shop terminal -> host sets the pickup index
                    instancedPurchase.original.pickup = newPickup;
                    instancedPurchase.original.hidden = newHidden;
                    orig(self, newPickup, newHidden);
                }
            }
            else
            {
                orig(self, newPickup, newHidden);
            }
        }

        private PickupIndex ShopTerminalBehavior_CurrentPickupIndex(On.RoR2.ShopTerminalBehavior.orig_CurrentPickupIndex orig, ShopTerminalBehavior self)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase) && NetworkServer.active)
            {
                return instancedPurchase.GetOrOriginal(currentInteractor).pickup.pickupIndex;
            }
            return orig(self);
        }
        private UniquePickup ShopTerminalBehavior_CurrentPickup(On.RoR2.ShopTerminalBehavior.orig_CurrentPickup orig, ShopTerminalBehavior self)
        {
            if (self.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase) && NetworkServer.active)
            {
                return instancedPurchase.GetOrOriginal(currentInteractor).pickup;
            }
            return orig(self);
        }

        private void ShopTerminalBehavior_OnSerialize(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(x => x.MatchLdfld<ShopTerminalBehavior>("pickup")))
            {
                c.Remove();
                c.EmitDelegate((ShopTerminalBehavior shopTerminalBehavior) =>
                {
                    if(shopTerminalBehavior.gameObject.TryGetComponent(out InstancedPurchase instancedPurchase))
                    {
                        return instancedPurchase.GetOrOriginal(instancedPurchase.pcClient).pickup;
                    }
                    // vanilla
                    return shopTerminalBehavior.pickup;
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
                    shopTerminalBehavior.pickup = instancedPurchase.GetOrOriginal(pc).pickup;
                    shopTerminalBehavior.UpdatePickupDisplayAndAnimations();
                }
            }
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
                    shopTerminalBehavior.m_SyncVarDirtyBits |= 1u; // pickup
                    shopTerminalBehavior.m_SyncVarDirtyBits |= 4u; // hasBeenPurchased
                }
                if (shop.TryGetComponent(out PurchaseInteraction purchaseInteraction))
                {
                    syncVarDirtyBitsBackups[purchaseInteraction] = purchaseInteraction.m_SyncVarDirtyBits;
                    purchaseInteraction.m_SyncVarDirtyBits |= 8u; // available
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
