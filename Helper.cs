using BepInEx;
using HG;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    public class Helper
    {
        // Money scales with difficulty like the game's own shops, if ScaleMoneyCosts is on
        public static int GetCost(int cost, CostTypeIndex costType)
        {
            if (ModConfig.ScaleMoneyCosts.Value && costType == CostTypeIndex.Money && Run.instance)
            {
                return Run.instance.GetDifficultyScaledCost(cost);
            }
            return cost;
        }

        public static void CreateItemTakenOrb(Vector3 effectOrigin, GameObject targetObject, PickupIndex pickupIndex)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            EffectData effectData = new EffectData
            {
                origin = effectOrigin,
                genericFloat = 1.5f,
                genericUInt = (uint)(pickupIndex.value + 1)
            };
            effectData.SetNetworkedObjectReference(targetObject);
            EffectManager.SpawnEffect(Main.instance.pickupTakenOrbPrefab.WaitForCompletion(), effectData, transmit: true);
        }

        public static EquipmentIndex GivePickup(CharacterBody characterBody, PickupIndex pickupToGive, Vector3 itemOrbSource, bool dropReplacedEquipmentAsPickupDroplet)
        {
            Dictionary<PickupIndex, int> itemsToGive = new Dictionary<PickupIndex, int>();
            itemsToGive[pickupToGive] = 1;
            var droppedEquipments = Helper.GivePickups(characterBody, itemsToGive, itemOrbSource, dropReplacedEquipmentAsPickupDroplet);
            foreach(var (droppedEquipment, amount) in droppedEquipments)
            {
                if (amount > 0)
                    return droppedEquipment;
            }
            return EquipmentIndex.None;
        }

        // Gives the pickups like ItemStringParser does. Equipment that was replaced or did not fit is returned, and dropped if asked.
        public static Dictionary<EquipmentIndex, int> GivePickups(CharacterBody characterBody, Dictionary<PickupIndex, int> itemsToGive, Vector3? itemOrbSource, bool dropReplacedEquipmentsAsPickupDroplets)
        {
            Dictionary<EquipmentIndex, int> droppedEquipments = new Dictionary<EquipmentIndex, int>();
            if (!characterBody || itemsToGive == null)
                return droppedEquipments;

            if (itemOrbSource.HasValue)
            {
                var itemTakenOrbs = 0;
                foreach (var (pickupIndex, itemAmount) in itemsToGive)
                {
                    if (itemAmount <= 0 || itemTakenOrbs >= 20)
                        continue;
                    CreateItemTakenOrb(itemOrbSource.Value, characterBody.gameObject, pickupIndex);
                    itemTakenOrbs++;
                }
            }

            foreach (var leftOver in ItemStringParser.ItemStringParser.GivePickups(characterBody, itemsToGive))
            {
                var equipmentIndex = PickupCatalog.GetPickupDef(leftOver.pickupIndex)?.equipmentIndex ?? EquipmentIndex.None;
                if (equipmentIndex != EquipmentIndex.None)
                    droppedEquipments[equipmentIndex] = droppedEquipments.GetValueOrDefault(equipmentIndex) + 1;
                if (dropReplacedEquipmentsAsPickupDroplets)
                    PickupDropletController.CreatePickupDroplet(leftOver, characterBody.corePosition + Vector3.up * 1.5f, Vector3.up * 15f - characterBody.coreTransform.forward * 15f, false);
            }
            return droppedEquipments;
        }
    }
}
