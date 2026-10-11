using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace BazaarIsMyHaven
{
    public partial class InstancedPurchases
    {
        public static PlayerCharacterMasterController currentInteractor;

        // SyncVar masks used by the game's serializers.
        private const uint PickupMask = 1u;
        private const uint HiddenMask = 2u;
        private const uint PurchasedMask = 4u;
        private const uint AvailableMask = 8u;

        public static void Hook()
        {
            On.RoR2.PurchaseInteraction.OnSerialize += PurchaseInteraction_OnSerialize;
            On.RoR2.ShopTerminalBehavior.OnSerialize += ShopTerminalBehavior_OnSerialize;
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.SetHasBeenPurchased += ShopTerminalBehavior_SetHasBeenPurchased;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ShopTerminalBehavior.CurrentPickupIndex += ShopTerminalBehavior_CurrentPickupIndex;
            On.RoR2.ShopTerminalBehavior.CurrentPickup += ShopTerminalBehavior_CurrentPickup;
        }

        private static PlayerCharacterMasterController GetPlayer(Interactor activator)
        {
            var body = activator ? activator.GetComponent<CharacterBody>() : null;
            return body && body.master ? body.master.playerCharacterMasterController : null;
        }

        private static Interactability PurchaseInteraction_GetInteractability(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            if (!NetworkServer.active || !self.TryGetComponent(out InstancedPurchase instance))
            {
                return orig(self, activator);
            }

            bool previousAvailable = self.available;
            try
            {
                self.available = instance.GetOrOriginal(GetPlayer(activator)).available;
                return orig(self, activator);
            }
            finally
            {
                self.available = previousAvailable;
            }
        }

        private static void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (!NetworkServer.active)
            {
                orig(self, activator);
                return;
            }

            var previousInteractor = currentInteractor;
            var player = GetPlayer(activator);
            self.TryGetComponent(out InstancedPurchase instance);
            currentInteractor = player;

            try
            {
                if (instance)
                {
                    // Vanilla DropPickup reads the raw pickup field, so give it the buyer's state
                    ApplyState(self.gameObject, instance.GetOrOriginal(player), false);
                }

                orig(self, activator);
            }
            finally
            {
                currentInteractor = previousInteractor;
                if (instance)
                {
                    RestoreHostView(self.gameObject, player && player.hasAuthority);
                    if (player && !player.hasAuthority)
                    {
                        instance.QueueUpdate(player);
                    }

                }
            }
        }

        private static void ShopTerminalBehavior_SetHasBeenPurchased(On.RoR2.ShopTerminalBehavior.orig_SetHasBeenPurchased orig, ShopTerminalBehavior self, bool newHasBeenPurchased)
        {
            if (!NetworkServer.active || !self.TryGetComponent(out InstancedPurchase instance))
            {
                orig(self, newHasBeenPurchased);
                return;
            }

            instance.GetOrCreate(currentInteractor).hasBeenPurchased = newHasBeenPurchased;
            if (currentInteractor)
            {
                self.hasBeenPurchased = newHasBeenPurchased; 
            }
            else
            {
                UpdateAll(self.gameObject);
            }
        }

        private static void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            if (!NetworkServer.active || !self.TryGetComponent(out InstancedPurchase instance))
            {
                orig(self, newAvailable);
                return;
            }

            instance.GetOrCreate(currentInteractor).available = newAvailable;
            if (currentInteractor)
            {
                self.available = newAvailable;

            }
            else
            {
                UpdateAll(self.gameObject);
            }
        }


        private static void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (!NetworkServer.active || !self.TryGetComponent(out InstancedPurchase instance))
            {
                orig(self, newPickup, newHidden);
                return;
            }

            var state = instance.GetOrCreate(currentInteractor);
            state.pickup = newPickup;
            state.hidden = newHidden;
            if (currentInteractor)
            {
                self.pickup = newPickup;
                self.hidden = newHidden;

            }
            else
            {
                UpdateAll(self.gameObject);
            }
        }

        private static PickupIndex ShopTerminalBehavior_CurrentPickupIndex(On.RoR2.ShopTerminalBehavior.orig_CurrentPickupIndex orig, ShopTerminalBehavior self)
        {
            return NetworkServer.active && self.TryGetComponent(out InstancedPurchase instance) ? instance.GetOrOriginal(currentInteractor).pickup.pickupIndex : orig(self);
        }

        private static UniquePickup ShopTerminalBehavior_CurrentPickup(On.RoR2.ShopTerminalBehavior.orig_CurrentPickup orig, ShopTerminalBehavior self)
        {
            return NetworkServer.active && self.TryGetComponent(out InstancedPurchase instance) ? instance.GetOrOriginal(currentInteractor).pickup : orig(self);
        }

        private static bool ShopTerminalBehavior_OnSerialize(On.RoR2.ShopTerminalBehavior.orig_OnSerialize orig, ShopTerminalBehavior self, NetworkWriter writer, bool initialState)
        {

            if (!self.TryGetComponent(out InstancedPurchase instance))
            {
                return orig(self, writer, initialState);
            }

            var previousPickup = self.pickup;
            bool previousHidden = self.hidden;
            bool previousPurchased = self.hasBeenPurchased;
            var state = instance.GetOrOriginal(instance.pcClient);
            
            // Personalized fields use targeted messages, not the broadcast; other fields like prices sync normally
            if (!initialState && !instance.pcClient)
            {
                self.m_SyncVarDirtyBits &= ~(PickupMask | HiddenMask | PurchasedMask);
            }

            try
            {
                self.pickup = state.pickup;
                self.hidden = state.hidden;
                self.hasBeenPurchased = state.hasBeenPurchased;
                return orig(self, writer, initialState);
            }
            finally
            {
                self.pickup = previousPickup;
                self.hidden = previousHidden;
                self.hasBeenPurchased = previousPurchased;
            }
        }

        private static bool PurchaseInteraction_OnSerialize(On.RoR2.PurchaseInteraction.orig_OnSerialize orig, PurchaseInteraction self, NetworkWriter writer, bool initialState)
        {
            if (!self.TryGetComponent(out InstancedPurchase instance))
            {
                return orig(self, writer, initialState);
            }

            bool previousAvailable = self.available;
            if (!initialState && !instance.pcClient)
            {
                self.m_SyncVarDirtyBits &= ~AvailableMask;
            }

            try
            {
                self.available = instance.GetOrOriginal(instance.pcClient).available;
                return orig(self, writer, initialState);

            }
            finally
            {
                self.available = previousAvailable;
            }
        }

        private static void ApplyState(GameObject shop, InstancedPurchaseStruct state, bool refreshAnimation)
        {
            var purchase = shop.GetComponent<PurchaseInteraction>();
            var terminal = shop.GetComponent<ShopTerminalBehavior>();
            purchase.available = state.available;
            terminal.pickup = state.pickup;
            terminal.hidden = state.hidden;
            terminal.hasBeenPurchased = state.hasBeenPurchased;

            if (refreshAnimation && NetworkClient.active)
            {
                terminal.UpdatePickupDisplayAndAnimations();
            }
        }

        private static void RestoreHostView(GameObject shop, bool forceAnimation = false)
        {
            var instance = shop.GetComponent<InstancedPurchase>();
            PlayerCharacterMasterController host = null;
            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                if (pc && pc.hasAuthority)
                {
                    host = pc;
                    break;
                }
            }

            var state = instance.GetOrOriginal(host);
            var terminal = shop.GetComponent<ShopTerminalBehavior>();
            bool changed = !terminal.pickup.Equals(state.pickup) || terminal.hidden != state.hidden || terminal.hasBeenPurchased != state.hasBeenPurchased;
            ApplyState(shop, state, forceAnimation || changed);
        }

        public static void UpdateAll(GameObject shop)
        {
            if (!NetworkServer.active || !shop.TryGetComponent(out InstancedPurchase instance))
            {
                return;
            }

            RestoreHostView(shop);
            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                if(pc && !pc.hasAuthority)
                {
                    instance.QueueUpdate(pc);
                }
            }
        }

        private static byte[] CreateUpdateMessage(NetworkIdentity identity, int channel, uint shopMask, uint purchaseMask)
        {
            var writer = new NetworkWriter();
            writer.StartMessage(MsgType.UpdateVars);
            writer.Write(identity.netId);

            foreach (var behavior in identity.GetBehavioursOfSameChannel(channel, false))
            {
                uint previousDirtyBits = behavior.m_SyncVarDirtyBits;
                try
                {
                    // Serialize only the requested fields; other components write nothing
                    behavior.m_SyncVarDirtyBits = behavior is ShopTerminalBehavior ? shopMask : behavior is PurchaseInteraction ? purchaseMask : 0u;
                    behavior.OnSerialize(writer, false);
                }
                finally
                {
                    behavior.m_SyncVarDirtyBits = previousDirtyBits;
                }
            }

            writer.FinishMessage();
            return writer.ToArray();
        }

        // Rebuilds a shop's model on one client (e.g. after fixing its scale) without changing stock or purchase state
        internal static byte[] CreatePickupDisplayRefresh(NetworkIdentity identity, NetworkConnection connection, int channel)
        {
            var terminal = identity.GetComponent<ShopTerminalBehavior>();
            var instance = identity.GetComponent<InstancedPurchase>();
            PlayerCharacterMasterController player = null;
            InstancedPurchaseStruct state = null;

            if (instance)
            {
                foreach (var pc in PlayerCharacterMasterController.instances)
                {
                    if (pc && pc.networkUser && pc.networkUser.connectionToClient == connection)
                    {
                        player = pc;
                        break;
                    }
                }

                // A late join may see shops before the player is ready. Retry instead of
                // sending the host's stock or another player's personalized offer.
                if (!player)
                {
                    return null;
                }

                state = instance.GetOrOriginal(player);
            }

            var pickup = state != null ? state.pickup : terminal.pickup;
            if (pickup.Equals(UniquePickup.none))
            {
                return Array.Empty<byte>();
            }

            bool originalHidden = state != null ? state.hidden : terminal.hidden;
            var previousRecipient = instance ? instance.pcClient : null;
            try
            {
                if (instance)
                {
                    instance.pcClient = player;
                    state.hidden = !originalHidden;
                }
                else
                {
                    terminal.hidden = !originalHidden;
                }

                // Hidden alone does not refresh the display. Send the temp value first;
                // this change only applies while serializing this client's messages.
                var toggleHiddenMessage = CreateUpdateMessage(identity, channel, HiddenMask, 0u);

                if (state != null)
                {
                    state.hidden = originalHidden;
                }
                else
                {
                    terminal.hidden = originalHidden;
                }

                // Vanilla reads pickup before hidden: rebuild with the temp hidden value,
                // then put the real value back in the same message. The final pickup update
                // rebuilds the intended display without changing its item or purchase state.
                var restoreHiddenMessage = CreateUpdateMessage(identity, channel, PickupMask | HiddenMask, 0u);
                var restoreModelMessage = CreateUpdateMessage(identity, channel, PickupMask, 0u);
                var writer = new NetworkWriter();
                writer.Write(toggleHiddenMessage, toggleHiddenMessage.Length);
                writer.Write(restoreHiddenMessage, restoreHiddenMessage.Length);
                writer.Write(restoreModelMessage, restoreModelMessage.Length);

                return writer.ToArray();
            }
            finally
            {
                if (instance)
                {
                    state.hidden = originalHidden;
                    instance.pcClient = previousRecipient;
                }
                else
                {
                    terminal.hidden = originalHidden;
                }
            }
        }

        internal static bool SendUpdateToClient(InstancedPurchase instance, PlayerCharacterMasterController pc)
        {
            var identity = instance.GetComponent<NetworkIdentity>();
            var connection = pc.networkUser ? pc.networkUser.connectionToClient : null;

            if (connection == null || !connection.isReady || !identity || identity.observers == null || !identity.observers.Contains(connection) || Util.ConnectionIsLocal(connection))
            {
                return false;
            }

            var previousRecipient = instance.pcClient;
            try
            {
                instance.pcClient = pc;
                var terminal = instance.GetComponent<ShopTerminalBehavior>();
                int channel = terminal.GetNetworkChannel();

                //The first message sets the flag without triggering the item animation
                // the second updates stock/availability with that flag already set
                var purchaseMessage = CreateUpdateMessage(identity, channel, PurchasedMask, 0u);
                var stockMessage = CreateUpdateMessage(identity, channel, PickupMask | HiddenMask, AvailableMask);
                var messages = new byte[purchaseMessage.Length + stockMessage.Length];

                Buffer.BlockCopy(purchaseMessage, 0, messages, 0, purchaseMessage.Length);
                Buffer.BlockCopy(stockMessage, 0, messages, purchaseMessage.Length, stockMessage.Length);

                if (connection.SendBytes(messages, messages.Length, channel))
                {
                    return true;
                }

                Log.LogWarning($"Could not update instanced shop for {connection.connectionId}; retrying");
                return false;
            }
            catch (Exception exception)
            {
                Log.LogError($"Instanced shop sync failed: {exception}");
                return false;
            }
            finally
            {
                instance.pcClient = previousRecipient;
            }
        }
    }
}
