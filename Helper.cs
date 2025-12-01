using BepInEx;
using HG;
using RoR2;
using RoR2.Navigation;
using RoR2.Skills;
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
        private static int FindSkillSlotIndex(BodyIndex bodyIndex, SkillFamily skillFamily)
        {
            GenericSkill[] bodyPrefabSkillSlots = BodyCatalog.GetBodyPrefabSkillSlots(bodyIndex);
            for (int i = 0; i < bodyPrefabSkillSlots.Length; i++)
            {
                if (bodyPrefabSkillSlots[i].skillFamily == skillFamily)
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindVariantIndex(SkillFamily skillFamily, SkillDef skillDef)
        {
            SkillFamily.Variant[] variants = skillFamily.variants;
            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i].skillDef == skillDef)
                {
                    return i;
                }
            }
            return -1;
        }
        public static SkillFamily FindSkillByFamilyName(string skillFamilyName)
        {
            for (int i = 0; i < SkillCatalog._allSkillFamilies.Length; i++)
            {
                if (SkillCatalog.GetSkillFamilyName(SkillCatalog._allSkillFamilies[i].catalogIndex) == skillFamilyName)
                {
                    return SkillCatalog._allSkillFamilies[i];
                }
            }
            return null;
        }

        public static bool HasSkillVariantEnabled(Loadout loadout, BodyIndex bodyIndex, SkillFamily skillFamily, SkillDef skillDef)
        {
            int num = FindSkillSlotIndex(bodyIndex, skillFamily);
            int num2 = FindVariantIndex(skillFamily, skillDef);
            if (num == -1 || num2 == -1)
            {
                return false;
            }
            return loadout.bodyLoadoutManager.GetSkillVariant(bodyIndex, num) == num2;
        }

        public static bool IsToolbotWithSwapSkill(CharacterMaster master)
        {
            var body = master.bodyPrefab.GetComponent<CharacterBody>();
            var skillFamily = Helper.FindSkillByFamilyName("ToolbotBodySpecialFamily");
            var skillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Swap"));
            return Helper.HasSkillVariantEnabled(master.loadout, body.bodyIndex, skillFamily, skillDef);
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

        public static Dictionary<EquipmentIndex, int> GivePickups(CharacterBody characterBody, Dictionary<PickupIndex, int> itemsToGive, Vector3 itemOrbSource, bool dropReplacedEquipmentsAsPickupDroplets)
        {
            Dictionary<EquipmentIndex, int> droppedEquipments = new Dictionary<EquipmentIndex, int>();
            var inventory = characterBody.inventory;
            if (inventory == null)
                return null;
            var master = characterBody.master;
            if (master == null)
                return null;
            var itemTakenOrbs = 0;
            uint equipmentsGiven = 0;
            int equipSkip = 0;
            bool equipLoop = false;
            foreach (var (pickupIndex, itemAmount) in itemsToGive)
            {
                if (itemAmount <= 0 || pickupIndex == PickupIndex.none)
                    continue;
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                // handle items
                var itemIndex = pickupDef.itemIndex;
                var equipmentIndex = pickupDef.equipmentIndex;
                if (itemIndex != ItemIndex.None)
                {
                    if (itemTakenOrbs < 20 && itemOrbSource != null)
                    {
                        CreateItemTakenOrb(itemOrbSource, characterBody.gameObject, pickupIndex);
                        itemTakenOrbs++;
                    }
                    inventory.GiveItemPermanent(itemIndex, itemAmount);
                }
                else
                {
                    // handle equipments
                    var equipmentAmount = itemAmount;
                    int maxEquipmentSlots = Helper.IsToolbotWithSwapSkill(master) ? 2 : 1;
                    int maxEquipmentSets = inventory.GetItemCountEffective(DLC3Content.Items.ExtraEquipment.itemIndex) + 1;
                    int maxEquipmentCount = maxEquipmentSlots * maxEquipmentSets;
                    while (equipmentIndex != EquipmentIndex.None && equipmentAmount > 0 && equipmentsGiven < maxEquipmentCount)
                    {
                        var index = equipmentsGiven + equipSkip;
                        if (index >= maxEquipmentCount)
                        {
                            equipLoop = true;
                            index = 0;
                        }
                        uint slot = (uint)(index % maxEquipmentSlots);
                        uint set = (uint)(index / maxEquipmentSlots);
                        var equipmentState = inventory.GetEquipment(slot, set);
                        if (EquipmentState.empty.Equals(equipmentState))
                        {
                            // has no equipment in this slot -> set it
                            if (itemTakenOrbs < 20 && itemOrbSource != null)
                            {
                                CreateItemTakenOrb(itemOrbSource, characterBody.gameObject, pickupIndex);
                                itemTakenOrbs++;
                            }
                            inventory.SetEquipmentIndexForSlot(equipmentIndex, slot, set);
                            equipmentAmount--;
                            equipmentsGiven++;
                        }
                        else
                        {
                            if (!equipLoop)
                            {
                                // skip this slot, because it already has an equip
                                equipSkip++;
                            }
                            else
                            {
                                // we already looped once -> time to drop the old equipment
                                droppedEquipments[equipmentState.equipmentIndex] = droppedEquipments.GetValueOrDefault(equipmentIndex) + 1;
                                if (dropReplacedEquipmentsAsPickupDroplets) { 
                                    var oldEquipment = new UniquePickup(PickupCatalog.FindPickupIndex(equipmentState.equipmentIndex));
                                    PickupDropletController.CreatePickupDroplet(oldEquipment, characterBody.corePosition + Vector3.up * 1.5f, Vector3.up * 15f - characterBody.coreTransform.forward * 15f, false);
                                }
                                if (itemTakenOrbs < 20 && itemOrbSource != null)
                                {
                                    CreateItemTakenOrb(itemOrbSource, characterBody.gameObject, pickupIndex);
                                    itemTakenOrbs++;
                                }
                                inventory.SetEquipmentIndexForSlot(equipmentIndex, slot, set);
                                equipmentAmount--;
                                equipmentsGiven++;
                            }
                        }
                    }
                }
            }
            return droppedEquipments;
        }
    }
}
