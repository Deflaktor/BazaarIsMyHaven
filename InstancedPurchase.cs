using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarIsMyHome
{
    public class InstancedPurchase : MonoBehaviour
    {
        public readonly InstancedPurchaseStruct original = new InstancedPurchaseStruct();
        public Dictionary<PlayerCharacterMasterController, InstancedPurchaseStruct> purchases = new Dictionary<PlayerCharacterMasterController, InstancedPurchaseStruct>();
        public PlayerCharacterMasterController pcClient;

        public InstancedPurchaseStruct GetOrCreate(PlayerCharacterMasterController pc)
        {
            if(!purchases.ContainsKey(pc))
            {
                purchases[pc] = new InstancedPurchaseStruct();
                purchases[pc].available = original.available;
                purchases[pc].pickupIndex = original.pickupIndex;
                purchases[pc].hasBeenPurchased = original.hasBeenPurchased;
                purchases[pc].hidden = original.hidden;
            }
            return purchases[pc];
        }

        public InstancedPurchaseStruct GetOrOriginal(PlayerCharacterMasterController pc)
        {
            if (pc == null || !purchases.ContainsKey(pc))
            {
                return original;
            }
            return purchases[pc];
        }
    }

    public class InstancedPurchaseStruct
    {
        public bool available = false;
        public PickupIndex pickupIndex = PickupIndex.none;
        public bool hasBeenPurchased = false;
        public bool hidden = false;
        public bool hasBeenPurchasedOnce = false;
    }
}
