using PerClientSync;
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
        internal const uint PickupMask = 1u;
        internal const uint HiddenMask = 2u;
        internal const uint PurchasedMask = 4u;
        internal const uint AvailableMask = 8u;
        internal const uint CostMask = 16u;
        // what a player's state changes in the terminal and in the purchase
        internal const uint TerminalMask = PickupMask | HiddenMask | PurchasedMask;
        internal const uint PurchaseMask = AvailableMask | CostMask;
        // a Triple Drone Shop terminal's pickup and purchased flag
        internal const uint DroneTerminalMask = PickupMask | PurchasedMask;

        public static void Hook()
        {
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.SetHasBeenPurchased += ShopTerminalBehavior_SetHasBeenPurchased;
            On.RoR2.DroneVendorTerminalBehavior.SetHasBeenPurchased += DroneVendorTerminalBehavior_SetHasBeenPurchased;
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

        // the drone terminal's own purchase event marks it as bought
        private static void DroneVendorTerminalBehavior_SetHasBeenPurchased(On.RoR2.DroneVendorTerminalBehavior.orig_SetHasBeenPurchased orig, DroneVendorTerminalBehavior self, bool newHasBeenPurchased)
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

        private static void ApplyState(GameObject shop, InstancedPurchaseStruct state, bool refreshAnimation)
        {
            var purchase = shop.GetComponent<PurchaseInteraction>();
            purchase.available = state.available;
            if (shop.TryGetComponent(out DroneVendorTerminalBehavior droneTerminal))
            {
                droneTerminal.currentPickup = state.pickup;
                droneTerminal.hasBeenPurchased = state.hasBeenPurchased;
                if (refreshAnimation && NetworkClient.active)
                {
                    droneTerminal.UpdateVisuals();
                }
                return;
            }
            // chests have no terminal; only their availability is per player
            var terminal = shop.GetComponent<ShopTerminalBehavior>();
            if (!terminal)
            {
                return;
            }
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
            var droneTerminal = shop.GetComponent<DroneVendorTerminalBehavior>();
            bool changed = terminal && (!terminal.pickup.Equals(state.pickup) || terminal.hidden != state.hidden || terminal.hasBeenPurchased != state.hasBeenPurchased)
                || droneTerminal && !droneTerminal.currentPickup.Equals(state.pickup);
            ApplyState(shop, state, forceAnimation || changed);
        }

        public static void UpdateAll(GameObject shop)
        {
            if (!NetworkServer.active || !shop.TryGetComponent(out InstancedPurchase instance))
            {
                return;
            }

            RestoreHostView(shop);
            instance.QueueUpdateForAll();
        }

        // Shows a changed state to its player only; resending an empty bud would replay its closing sound
        public static void UpdatePlayer(GameObject shop, PlayerCharacterMasterController pc)
        {
            if (!NetworkServer.active || !pc || !shop.TryGetComponent(out InstancedPurchase instance))
            {
                return;
            }

            if (pc.hasAuthority)
            {
                RestoreHostView(shop);
            }
            else
            {
                instance.QueueUpdate(pc);
            }
        }

        // Plays the shop's buy sound when a purchase leaves an item in it, e.g. when equipment swapped in replaces the bought one.
        // Clients play it only when the shop's pickup clears, so they get it cleared and back; instanced shops play it for the buyer only.
        public static void PlayBuySound(ShopTerminalBehavior shop, PlayerCharacterMasterController buyer)
        {
            var identity = shop ? shop.GetComponent<NetworkIdentity>() : null;
            if (!NetworkServer.active || !identity)
            {
                return;
            }
            shop.TryGetComponent(out InstancedPurchase instance);
            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                if (!pc || (instance && pc != buyer))
                {
                    continue;
                }
                if (pc.hasAuthority)
                {
                    // the host's own view
                    Util.PlaySound("Play_UI_tripleChestShutter", shop.gameObject);
                    continue;
                }
                var connection = PerClient.GetConnection(pc);
                if (connection == null || !connection.isReady || identity.observers == null || !identity.observers.Contains(connection))
                {
                    continue;
                }

                var state = instance ? instance.GetOrOriginal(pc) : null;
                var pickup = state != null ? state.pickup : shop.pickup;
                byte[] clearUpdate;
                try
                {
                    SetPickup(shop, state, UniquePickup.none);
                    clearUpdate = PerClient.BuildUpdate(connection, shop, PickupMask);
                }
                finally
                {
                    SetPickup(shop, state, pickup);
                }
                var restoreUpdate = PerClient.BuildUpdate(connection, shop, PickupMask);
                var updates = Concat(clearUpdate, restoreUpdate);
                if (!connection.SendBytes(updates, updates.Length, shop.GetNetworkChannel()))
                {
                    Log.LogWarning("Could not play a shop's buy sound for a client.");
                }
            }
        }

        // The player's own state of an instanced shop, the shop's fields otherwise
        private static void SetPickup(ShopTerminalBehavior shop, InstancedPurchaseStruct state, UniquePickup pickup)
        {
            if (state != null)
            {
                state.pickup = pickup;
            }
            else
            {
                shop.pickup = pickup;
            }
        }

        private static void SetHidden(ShopTerminalBehavior shop, InstancedPurchaseStruct state, bool hidden)
        {
            if (state != null)
            {
                state.hidden = hidden;
            }
            else
            {
                shop.hidden = hidden;
            }
        }

        private static byte[] Concat(params byte[][] updates)
        {
            var writer = new NetworkWriter();
            foreach (var update in updates)
            {
                writer.Write(update, update.Length);
            }
            return writer.ToArray();
        }

        // Rebuilds a shop's model on one client (e.g. after fixing its scale) without changing stock or purchase state
        internal static byte[] CreatePickupDisplayRefresh(NetworkIdentity identity, NetworkConnection connection)
        {
            var terminal = identity.GetComponent<ShopTerminalBehavior>();
            var instance = identity.GetComponent<InstancedPurchase>();
            var player = PerClient.GetPlayer(connection);

            // A late join may see shops before the player is ready. Retry instead of
            // sending the host's stock or another player's personalized offer.
            if (instance && !player)
            {
                return null;
            }

            var state = instance ? instance.GetOrOriginal(player) : null;
            var pickup = state != null ? state.pickup : terminal.pickup;
            if (pickup.Equals(UniquePickup.none))
            {
                return Array.Empty<byte>();
            }

            // Hidden alone does not refresh the display. Send the temp value first;
            // this change only applies while serializing this client's update.
            bool originalHidden = state != null ? state.hidden : terminal.hidden;
            byte[] toggleHiddenUpdate;
            try
            {
                SetHidden(terminal, state, !originalHidden);
                toggleHiddenUpdate = PerClient.BuildUpdate(connection, terminal, HiddenMask);
            }
            finally
            {
                SetHidden(terminal, state, originalHidden);
            }

            // Vanilla reads pickup before hidden: rebuild with the temp hidden value,
            // then put the real value back in the same update. The final pickup update
            // rebuilds the intended display without changing its item or purchase state.
            var restoreHiddenUpdate = PerClient.BuildUpdate(connection, terminal, PickupMask | HiddenMask);
            var restoreModelUpdate = PerClient.BuildUpdate(connection, terminal, PickupMask);
            return Concat(toggleHiddenUpdate, restoreHiddenUpdate, restoreModelUpdate);
        }
    }
}
