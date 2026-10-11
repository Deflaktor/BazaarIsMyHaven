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
        public PlayerCharacterMasterController pcClient;
        public int lunarShopIndex = -1;

        private readonly HashSet<PlayerCharacterMasterController> pendingUpdates = new HashSet<PlayerCharacterMasterController>();
        private readonly Dictionary<PlayerCharacterMasterController, NetworkConnection> initializedClients = new Dictionary<PlayerCharacterMasterController, NetworkConnection>();
        private float nextRetryTime;

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

        public void QueueUpdate(PlayerCharacterMasterController pc)
        {
            if (pc)
            {
                pendingUpdates.Add(pc);
            }
        }

        private void LateUpdate()
        {
            if (!NetworkServer.active || Time.unscaledTime < nextRetryTime)
            {
                return;
            }

            var identity = GetComponent<NetworkIdentity>();
            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                if (!pc || pc.hasAuthority)
                {
                    continue;
                }

                var connection = pc.networkUser ? pc.networkUser.connectionToClient : null;
                if (connection == null || !connection.isReady || !identity || identity.observers == null || !identity.observers.Contains(connection))
                {
                    initializedClients.Remove(pc);
                    continue;
                }

                //Initial updates must follow spawning; also covers slow clients.
                bool initialized = initializedClients.TryGetValue(pc, out var previousConnection) && previousConnection == connection;
                if (initialized && !pendingUpdates.Contains(pc))
                {
                    continue;
                }

                if (InstancedPurchases.SendUpdateToClient(this, pc))
                {
                    initializedClients[pc] = connection;
                    pendingUpdates.Remove(pc);
                }
                else
                {
                    nextRetryTime = Time.unscaledTime + 1f;
                    break;
                }
            }
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
