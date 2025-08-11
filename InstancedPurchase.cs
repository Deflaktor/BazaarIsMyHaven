using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarIsMyHome
{
    public class InstancedPurchase : MonoBehaviour
    {
        public PickupIndex originalPickupIndex;
        public bool originalHasBeenPurchased;
        public bool originalAvailable;

        public Dictionary<PlayerCharacterMasterController, bool> purchases = new Dictionary<PlayerCharacterMasterController, bool>();
        public PlayerCharacterMasterController playerCharacterMasterController;
    }
}
