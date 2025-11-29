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
    }
}
