using PerClientSync;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BazaarIsMyHaven
{
    public class InstancedPurchase : MonoBehaviour
    {
        public readonly InstancedPurchaseStruct original = new InstancedPurchaseStruct();
        public Dictionary<PlayerCharacterMasterController, InstancedPurchaseStruct> purchases = new Dictionary<PlayerCharacterMasterController, InstancedPurchaseStruct>();
        public int lunarShopIndex = -1;

        // each client sees its player's state in the shop's own fields
        private InstancedTerminalView terminalView;
        private InstancedPurchaseView purchaseView;

        private void Awake()
        {
            // chests have no terminal; only their availability is per player
            if (GetComponent<ShopTerminalBehavior>())
            {
                terminalView = gameObject.AddComponent<InstancedTerminalView>();
            }
            if (GetComponent<PurchaseInteraction>())
            {
                purchaseView = gameObject.AddComponent<InstancedPurchaseView>();
            }
        }

        // A shop that spawned for clients before it got its per-player states, e.g. one the director picked late, showed them
        // the original state; players with their own from the start, like a character limit, get theirs
        private void Start()
        {
            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                if (pc && GetOrOriginal(pc) != original)
                {
                    QueueUpdate(pc);
                }
            }
        }

        public InstancedPurchaseStruct GetOrCreate(PlayerCharacterMasterController pc)
        {
            if (!pc)
            {
                return original;
            }

            if (!purchases.TryGetValue(pc, out var state))
            {
                state = new InstancedPurchaseStruct
                {
                    available = original.available,
                    pickup = original.pickup,
                    hasBeenPurchased = original.hasBeenPurchased,
                    hidden = original.hidden,
                    hasBeenPurchasedOnce = original.hasBeenPurchasedOnce
                };
                if (IsShopDisabledFor(pc))
                {
                    state.available = false;
                    state.pickup = UniquePickup.none;
                    state.hasBeenPurchased = true;
                }
                purchases.Add(pc, state);
            }

            return state;
        }

        // Per-character shop amounts
        private bool IsShopDisabledFor(PlayerCharacterMasterController pc)
        {
            if (lunarShopIndex < 0 || !pc.master || !pc.master.bodyPrefab)
                return false;

            var bodyIndex = BodyCatalog.FindBodyIndex(pc.master.bodyPrefab);
            if (!ModConfig.LunarShopAmountDependingOnCharacterParsed.TryGetValue(bodyIndex, out int amount))
                return false;

            return amount >= 0 && lunarShopIndex >= amount;
        }

        public InstancedPurchaseStruct GetOrOriginal(PlayerCharacterMasterController pc)
        {
            // Apply character limits when a player is first seen, late joins too.
            if (pc && !purchases.ContainsKey(pc) && IsShopDisabledFor(pc))
                return GetOrCreate(pc);

            return pc && purchases.TryGetValue(pc, out var state) ? state : original;
        }

        // Sends this player's state to their client with the shop's next update
        public void QueueUpdate(PlayerCharacterMasterController pc)
        {
            var client = PerClient.GetConnection(pc);
            if (terminalView)
            {
                terminalView.SetDirty(client, InstancedPurchases.TerminalMask);
            }
            if (purchaseView)
            {
                purchaseView.SetDirty(client, InstancedPurchases.PurchaseMask);
            }
        }

        // Sends every client its player's state with the shop's next update
        public void QueueUpdateForAll()
        {
            if (terminalView)
            {
                terminalView.SetDirtyForAll(InstancedPurchases.TerminalMask);
            }
            if (purchaseView)
            {
                purchaseView.SetDirtyForAll(InstancedPurchases.PurchaseMask);
            }
        }
    }

    // The shop terminal as a client's player sees it: their item, whether it is hidden and whether they bought it
    public class InstancedTerminalView : ClientView<ShopTerminalBehavior>
    {
        private InstancedPurchase instance;
        private UniquePickup realPickup;
        private bool realHidden;
        private bool realPurchased;

        // the real fields hold the host's player's state, so only a player's own updates carry theirs
        public override uint PrivateBits => InstancedPurchases.TerminalMask;
        // clients would clear a bought bud's item before its purchased flag, and close the bud instead of opening it
        public override uint LeadingBits => InstancedPurchases.PurchasedMask;

        protected override void Apply(NetworkConnection client)
        {
            var terminal = Component;
            realPickup = terminal.pickup;
            realHidden = terminal.hidden;
            realPurchased = terminal.hasBeenPurchased;
            instance = instance ? instance : GetComponent<InstancedPurchase>();
            var state = instance.GetOrOriginal(PerClient.GetPlayer(client));
            terminal.pickup = state.pickup;
            terminal.hidden = state.hidden;
            terminal.hasBeenPurchased = state.hasBeenPurchased;
        }

        protected override void Restore()
        {
            var terminal = Component;
            terminal.pickup = realPickup;
            terminal.hidden = realHidden;
            terminal.hasBeenPurchased = realPurchased;
        }
    }

    // The purchase as a client's player sees it: whether they can buy, and its price, which is free for swaps at a slot they bought
    public class InstancedPurchaseView : ClientView<PurchaseInteraction>
    {
        private InstancedPurchase instance;
        private bool realAvailable;
        private int realCost;

        // the real field holds the host's player's state, so only a player's own updates carry theirs
        public override uint PrivateBits => InstancedPurchases.AvailableMask;

        protected override void Apply(NetworkConnection client)
        {
            var purchase = Component;
            realAvailable = purchase.available;
            realCost = purchase.cost;
            instance = instance ? instance : GetComponent<InstancedPurchase>();
            var player = PerClient.GetPlayer(client);
            purchase.available = instance.GetOrOriginal(player).available;
            if (FreeSwaps.IsFreeFor(purchase, player))
            {
                purchase.cost = 0;
            }
        }

        protected override void Restore()
        {
            var purchase = Component;
            purchase.available = realAvailable;
            purchase.cost = realCost;
        }
    }

    public class InstancedPurchaseStruct
    {
        public bool available = false;
        public UniquePickup pickup = UniquePickup.none;
        public bool hasBeenPurchased = false;
        public bool hidden = false;
        public bool hasBeenPurchasedOnce = false;

        // Used shops and terminals disabled for a character must stay unavailable.
        public bool CanReroll => available && !hasBeenPurchased && !hasBeenPurchasedOnce && !pickup.Equals(UniquePickup.none);
    }
}
