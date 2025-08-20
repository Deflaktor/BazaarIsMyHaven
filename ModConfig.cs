using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using R2API;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BazaarIsMyHaven
{
    class ModConfig
    {
        // general
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> SpawnCountByStage;
        public static ConfigEntry<int> SpawnCountOffset;
        public static ConfigEntry<bool> EnableAutoOpenShop;
        public static ConfigEntry<bool> EnableDecorate;
        //public static ConfigEntry<string> HitWords;

        // newt
        public static ConfigEntry<bool> NewtSectionEnabled;
        public static ConfigEntry<bool> EnableWelcomeWord;
        public static ConfigEntry<bool> EnableNoKickFromShop;
        public static ConfigEntry<ShopKeep.DeathState> NewtSecondLifeMode;
        public static ConfigEntry<bool> EnableNewtTrashTalk;

        // printer
        public static ConfigEntry<bool> PrinterSectionEnabled;
        public static ConfigEntry<int> PrinterCount;
        public static ConfigEntry<float> PrinterTier1Weight;
        public static ConfigEntry<float> PrinterTier2Weight;
        public static ConfigEntry<float> PrinterTier3Weight;
        public static ConfigEntry<float> PrinterTierBossWeight;
        public static ConfigEntry<float> PrinterTierVoid1Weight;
        public static ConfigEntry<float> PrinterTierVoid2Weight;
        public static ConfigEntry<float> PrinterTierVoid3Weight;
        public static ConfigEntry<float> PrinterTierVoidBossWeight;
        public static ConfigEntry<float> PrinterTierVoidAnyWeight;

        // cauldron
        public static ConfigEntry<bool> CauldronSectionEnabled;
        public static ConfigEntry<int> CauldronCount;
        public static ConfigEntry<float> CauldronWhiteToGreenWeight;
        public static ConfigEntry<float> CauldronGreenToRedWeight;
        public static ConfigEntry<float> CauldronRedToWhite;
        public static ConfigEntry<float> CauldronRedToYellowWeight;
        public static ConfigEntry<float> CauldronGreenToYellowWeight;
        public static ConfigEntry<float> CauldronGreenToPurpleWeight;
        public static ConfigEntry<float> CauldronRedToPurpleWeight;
        public static ConfigEntry<int> CauldronWhiteToGreenCost;
        public static ConfigEntry<int> CauldronGreenToRedCost;
        public static ConfigEntry<int> CauldronRedToWhiteCost;

        // scrapper
        public static ConfigEntry<bool> ScrapperSectionEnabled;
        public static ConfigEntry<int> ScrapperCount;

        // equipment
        public static ConfigEntry<bool> EquipmentSectionEnabled;
        public static ConfigEntry<int> EquipmentCount;
        public static ConfigEntry<bool> EquipmentInstanced;
        public static ConfigEntry<int> EquipmentCost;
        public static ConfigEntry<bool> EquipmentBuyToInventory;
        public static ConfigEntry<bool> ReplaceLunarSeersWithEquipment;

        // lunarShop
        public static ConfigEntry<bool> LunarShopSectionEnabled;
        public static ConfigEntry<int> LunarShopTerminalCount;
        public static ConfigEntry<int> LunarShopTerminalCost;
        public static ConfigEntry<int> LunarShopBuyLimit;
        public static ConfigEntry<bool> EnableLunarShopStaticItems;
        public static ConfigEntry<string> LunarShopItemsList;
        public static ConfigEntry<bool> LunarShopInstanced;
        public static ConfigEntry<bool> LunarShopBuyToInventory;

        // lunarRecycler
        public static ConfigEntry<bool> LunarRecyclerSectionEnabled;
        public static ConfigEntry<bool> LunarRecyclerAvailable;
        public static ConfigEntry<int> LunarRecyclerRerollCost;
        public static ConfigEntry<int> LunarRecyclerRerollCostMultiplier;
        public static ConfigEntry<int> LunarRecyclerRerollLimit;

        // cleansing pool
        public static ConfigEntry<bool> CleansingPoolSectionEnabled;
        public static ConfigEntry<bool> ShrineCleaseGivesLunarCoins;

        // lunarSeer
        public static ConfigEntry<bool> LunarSeerSectionEnabled;
        public static ConfigEntry<bool> SeerStationAvailable;
        public static ConfigEntry<int> SeerStationsCost;

        // restack shrine
        public static ConfigEntry<bool> ShrineRestackSectionEnabled;
        public static ConfigEntry<int> ShrineRestackUseLimit;
        public static ConfigEntry<int> ShrineRestackCost;
        public static ConfigEntry<int> ShrineRestackScalar;

        // prayer
        public static ConfigEntry<bool> PrayerSectionEnabled;
        public static ConfigEntry<int> PrayCost;
        public static ConfigEntry<int> PrayRewardLimit;
        public static ConfigEntry<float> PrayNormalWeight;
        public static ConfigEntry<float> PrayEliteWeight;
        public static ConfigEntry<float> PrayPeculiarWeight;
        public static ConfigEntry<string> PrayPeculiarList;

        public static void InitConfig(ConfigFile config)
        {
            // 00 General
            EnableMod = config.Bind("00 General", "Enabled", true, "Enable Mod");
            EnableAutoOpenShop = config.Bind("00 General", "AlwaysSpawnShopPortal", false, "Spawn a portal to the Bazaar after each teleporter event.");
            SpawnCountByStage = config.Bind("00 General", "SpawnCountByStage", true, "The amount of spawned interactables are based on stage completion count.\nFormula: Total = stage count + offset");
            SpawnCountOffset = config.Bind("00 General", "SpawnCountOffset", 0, "Offset value added to stage-based interactable spawns.");
            EnableDecorate = config.Bind("00 General", "EnableDecorate", true, "Adds Bazaar decorations such as chests and an extra portal behind Newt.");

            // 01 Newt
            NewtSectionEnabled = config.Bind("01 Newt", "SectionEnabled", true, "Enables or disables the Newt section.");
            EnableWelcomeWord = config.Bind("01 Newt", "EnableWelcomeWord", true, "Enables Newt's welcome messages. If he has been killed before, his tone may be hostile.");
            EnableNoKickFromShop = config.Bind("01 Newt", "EnableNoKickFromShop", true, "Prevents being kicked out of the Bazaar after angering Newt.");
            NewtSecondLifeMode = config.Bind("01 Newt", "NewtDeath", ShopKeep.DeathState.Tank, "Determines Newt's behavior after death.\nDefault: Vanilla - Unchanged behavior\nTank: Doubles Newt's health after each kill\nGhost: Newt respawns as a passive ghost\nHostile: Newt revives hostile and attacks players");
            EnableNewtTrashTalk = config.Bind("01 Newt", "EnableNewtTrashTalk", true, "When hit, Newt responds with sarcastic remarks.");

            // 02 Printer
            PrinterSectionEnabled = config.Bind("02 Printer", "SectionEnabled", true, "Enables or disables the Printer section.");
            PrinterCount = config.Bind("02 Printer", "PrinterCount", 9, "Number of printers spawned (max 9)."); if (PrinterCount.Value > 9) PrinterCount.Value = 9;
            PrinterTier1Weight = config.Bind("02 Printer", "PrinterTier1Weight", 0.5f, "Weight for Tier 1 (white) items."); PrinterTier1Weight.Value = Math.Abs(PrinterTier1Weight.Value);
            PrinterTier2Weight = config.Bind("02 Printer", "PrinterTier2Weight", 0.25f, "Weight for Tier 2 (green) items."); PrinterTier2Weight.Value = Math.Abs(PrinterTier2Weight.Value);
            PrinterTier3Weight = config.Bind("02 Printer", "PrinterTier3Weight", 0.17f, "Weight for Tier 3 (red) items."); PrinterTier3Weight.Value = Math.Abs(PrinterTier3Weight.Value);
            PrinterTierBossWeight = config.Bind("02 Printer", "PrinterBossWeight", 0.04f, "Weight for Boss-tier items."); PrinterTierBossWeight.Value = Math.Abs(PrinterTierBossWeight.Value);
            PrinterTierVoid1Weight = config.Bind("02 Printer", "PrinterTierVoid1Weight", 0.015f, "Weight for Tier 1 Void items (traded from red)."); PrinterTierVoid1Weight.Value = Math.Abs(PrinterTierVoid1Weight.Value);
            PrinterTierVoid2Weight = config.Bind("02 Printer", "PrinterTierVoid2Weight", 0.015f, "Weight for Tier 2 Void items (traded from red)."); PrinterTierVoid2Weight.Value = Math.Abs(PrinterTierVoid2Weight.Value);
            PrinterTierVoid3Weight = config.Bind("02 Printer", "PrinterTierVoid3Weight", 0.006f, "Weight for Tier 3 Void items (traded from red)."); PrinterTierVoid3Weight.Value = Math.Abs(PrinterTierVoid3Weight.Value);
            PrinterTierVoidBossWeight = config.Bind("02 Printer", "PrinterTierVoidBossWeight", 0.003f, "Weight for Boss-tier Void items (traded from red)."); PrinterTierVoidBossWeight.Value = Math.Abs(PrinterTierVoidBossWeight.Value);
            PrinterTierVoidAnyWeight = config.Bind("02 Printer", "PrinterTierVoidAnyWeight", 0f, "Weight for Void items of any tier (traded from red)."); PrinterTierVoidAnyWeight.Value = Math.Abs(PrinterTierVoidAnyWeight.Value);

            // 03 Cauldron
            CauldronSectionEnabled = config.Bind("03 Cauldron", "SectionEnabled", true, "Enables or disables the Cauldron section.");
            CauldronCount = config.Bind("03 Cauldron", "CauldronCount", 7, "Number of cauldrons spawned (max 7)."); if (CauldronCount.Value > 7) CauldronCount.Value = 7;
            CauldronWhiteToGreenWeight = config.Bind("03 Cauldron", "CauldronWhiteToGreenWeight", 0.75f, "Weight for White->Green cauldrons."); CauldronWhiteToGreenWeight.Value = Math.Abs(CauldronWhiteToGreenWeight.Value);
            CauldronWhiteToGreenCost = config.Bind("03 Cauldron", "CauldronWhiteToGreenCost", 3, "Number of White items required for Green conversion."); CauldronWhiteToGreenCost.Value = Math.Abs(CauldronWhiteToGreenCost.Value);
            CauldronGreenToRedWeight = config.Bind("03 Cauldron", "CauldronGreenToRedWeight", 0.25f, "Weight for Green->Red cauldrons."); CauldronGreenToRedWeight.Value = Math.Abs(CauldronGreenToRedWeight.Value);
            CauldronGreenToRedCost = config.Bind("03 Cauldron", "CauldronGreenToRedCost", 5, "Number of Green items required for Red conversion."); CauldronGreenToRedCost.Value = Math.Abs(CauldronGreenToRedCost.Value);
            CauldronRedToWhite = config.Bind("03 Cauldron", "CauldronRedToWhite", 0f, "Weight for Red->White cauldrons."); CauldronRedToWhite.Value = Math.Abs(CauldronRedToWhite.Value);
            CauldronRedToWhiteCost = config.Bind("03 Cauldron", "CauldronRedToWhiteCost", 1, "Number of Red items required for White conversion."); CauldronRedToWhiteCost.Value = Math.Abs(CauldronRedToWhiteCost.Value);
            // TODO
            //CauldronGreenToYellowWeight = config.Bind("03 Cauldron", "CauldronGreenToYellowWeight", 0.33f, "Spawn weight for Yellow cauldrons (uses Green items as cost)."); CauldronGreenToYellowWeight.Value = Math.Abs(CauldronGreenToYellowWeight.Value);
            //CauldronGreenToPurpleWeight = config.Bind("03 Cauldron", "CauldronGreenToPurpleWeight", 0.33f, "Spawn weight for Purple cauldrons (uses Green items as cost)."); CauldronGreenToPurpleWeight.Value = Math.Abs(CauldronGreenToPurpleWeight.Value);

            // 04 Scrapper
            ScrapperSectionEnabled = config.Bind("04 Scrapper", "SectionEnabled", true, "Enables or disables the Scrapper section.");
            ScrapperCount = config.Bind("04 Scrapper", "ScrapperCount", 4, "Number of Scrappers spawned (max 4)."); if (ScrapperCount.Value > 4) ScrapperCount.Value = 4;

            // 05 Equipment
            EquipmentSectionEnabled = config.Bind("05 Equipment", "SectionEnabled", true, "Enables or disables the Equipment section.");
            EquipmentCount = config.Bind("05 Equipment", "EquipmentCount", 3, "Number of Equipment Terminals (max 3 normally, 5 if replacing Lunar Seers).");
            ReplaceLunarSeersWithEquipment = config.Bind("05 Equipment", "ReplaceLunarSeersWithEquipment", true, "Replaces Lunar Seers with Equipment Terminals (increases equipment max to 5). Makes the Lunar Seer section irrelevant.");
            EquipmentInstanced = config.Bind("05 Equipment", "EquipmentInstanced", true, "Each player can purchase equipment independently.");
            EquipmentCost = config.Bind("05 Equipment", "EquipmentCost", 0, "Monetary cost for equipment purchases.");
            EquipmentBuyToInventory = config.Bind("05 Equipment", "EquipmentBuyToInventory", true, "Purchased equipment goes directly into inventory instead of dropping to the ground.");
            if (EquipmentCount.Value > 3 && !ReplaceLunarSeersWithEquipment.Value) EquipmentCount.Value = 3;
            if (EquipmentCount.Value > 5 && ReplaceLunarSeersWithEquipment.Value) EquipmentCount.Value = 5;

            // 06 LunarShop
            LunarShopSectionEnabled = config.Bind("06 LunarShop", "SectionEnabled", true, "Enables or disables the Lunar Shop section.");
            LunarShopTerminalCount = config.Bind("06 LunarShop", "LunarShopTerminalCount", 5, "Number of Lunar Shop Terminals (max 20). -1 = Vanilla Behavior (5 Lunar Buds)."); if (LunarShopTerminalCount.Value > 20) LunarShopTerminalCount.Value = 20;
            LunarShopTerminalCost = config.Bind("06 LunarShop", "LunarShopTerminalCost", 2, "Lunar coin cost per Lunar Shop Terminal or Lunar Bud use."); LunarShopTerminalCost.Value = Math.Abs(LunarShopTerminalCost.Value);
            LunarShopBuyLimit = config.Bind("06 LunarShop", "LunarShopBuyLimit", 5, "Limit on Lunar Shop purchases each player can make per visit to the Bazaar."); LunarShopBuyLimit.Value = Math.Abs(LunarShopBuyLimit.Value);
            EnableLunarShopStaticItems = config.Bind("06 LunarShop", "EnableLunarShopStaticItems", true, "Uses fixed/static items instead of randomized rolls.");
            var items = "LunarPrimaryReplacement, LunarSecondaryReplacement, LunarSpecialReplacement, AutoCastEquipment, LunarDagger, HalfSpeedDoubleHealth, LunarSun, LunarBadLuck, LunarBadLuck, LunarBadLuck, ShieldOnly, ShieldOnly, ShieldOnly, HalfAttackSpeedHalfCooldowns, HalfAttackSpeedHalfCooldowns, RandomDamageZone, Tonic";
            var itemTiersString = "Tier1, Tier2, Tier3, Lunar, Boss, VoidTier1, VoidTier2, VoidTier3, VoidBoss";
            LunarShopItemsList = config.Bind("06 LunarShop", "LunarShopItems", items, $"Comma-separated list of items available in Lunar Shop. Must use internal names (see https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/) or item tier keywords ({itemTiersString}).");
            LunarShopInstanced = config.Bind("06 LunarShop", "LunarShopInstanced", true, "Each player can buy independently from Lunar Shop.");
            LunarShopBuyToInventory = config.Bind("06 LunarShop", "LunarShopBuyToInventory", true, "Items go directly into inventory instead of dropping on ground.");

            // 07 LunarRecycler
            LunarRecyclerSectionEnabled = config.Bind("07 LunarRecycler", "SectionEnabled", true, "Enables or disables the Lunar Recycler section.");
            LunarRecyclerAvailable = config.Bind("07 LunarRecycler", "LunarRecyclerAvailable", true, "If enabled, a Lunar Recycler is available in the Bazaar. Otherwise it will get removed.");
            LunarRecyclerRerollLimit = config.Bind("07 LunarRecycler", "LunarRecyclerRerollLimit", 10, "Limit the amount of rerolls allowed per visit to the Bazaar.");
            LunarRecyclerRerollCost = config.Bind("07 LunarRecycler", "LunarRecyclerRerollCost", 1, "Initial lunar coin cost to reroll.");
            LunarRecyclerRerollCostMultiplier = config.Bind("07 LunarRecycler", "LunarRecyclerRerollCostMultiplier", 2, "Cost multiplier applied after each reroll use.");

            // 08 CleansingPool
            CleansingPoolSectionEnabled = config.Bind("08 CleansingPool", "SectionEnabled", true, "Enables or disables the Cleansing Pool section.");
            ShrineCleaseGivesLunarCoins = config.Bind("08 CleansingPool", "CleansingPoolGivesLunarCoins", true, "Cleansing Pools reward Lunar Coins instead of Pearls.");

            // 09 LunarSeer
            LunarSeerSectionEnabled = config.Bind("09 LunarSeer", "SectionEnabled", true, "Enables or disables the Lunar Seer Station section.");
            SeerStationAvailable = config.Bind("09 LunarSeer", "LunarSeerStationAvailable", true, "If enabled, Seer Stations appear in the Bazaar.");
            SeerStationsCost = config.Bind("09 LunarSeer", "LunarSeerStationsCost", 3, "Lunar coin cost for using Seer Stations.");

            // 10 ShrineOfOrder
            ShrineRestackSectionEnabled = config.Bind("10 ShrineOfOrder", "SectionEnabled", true, "Enables or disables the Shrine of Order section.");
            ShrineRestackUseLimit = config.Bind("10 ShrineOfOrder", "ShrineOfOrderUseLimit", 99, "Limit the amount of Shrine of Order uses per visit to the Bazaar.");
            ShrineRestackCost = config.Bind("10 ShrineOfOrder", "ShrineOfOrderCost", 1, "Initial lunar coin cost for Shrine of Order.");
            ShrineRestackScalar = config.Bind("10 ShrineOfOrder", "ShrineOfOrderScalar", 1, "Multiplier to Shrine of Order cost per use.");

            // 11 Prayer
            PrayerSectionEnabled = config.Bind("11 Prayer", "SectionEnabled", true, "Enables or disables the Prayer section.");
            PrayRewardLimit = config.Bind("11 Prayer", "RewardLimit", 3, "Limit the number of rewards each player can get per visit to the Bazaar. One reward is given every 11 Prayers.");
            PrayCost = config.Bind("11 Prayer", "PrayCost", 2, "Lunar coin cost per prayer. 11 Prayers need to be done to get a reward.");
            PrayNormalWeight = config.Bind("11 Prayer", "PrayNormalWeight", 0.5f, "Weight for regular item rewards (white, green, red items)."); PrayNormalWeight.Value = Math.Abs(PrayNormalWeight.Value);
            PrayEliteWeight = config.Bind("11 Prayer", "PrayEliteWeight", 0.25f, "Weight for elite equipment rewards."); PrayEliteWeight.Value = Math.Abs(PrayEliteWeight.Value);
            PrayPeculiarWeight = config.Bind("11 Prayer", "PrayPeculiarWeight", 0.25f, "Weight for odd or peculiar items (some unobtainable in normal gameplay)."); PrayPeculiarWeight.Value = Math.Abs(PrayPeculiarWeight.Value);
            PrayPeculiarList = config.Bind("11 Prayer", "PrayPeculiarList", "BoostAttackSpeed,BoostDamage,BoostEquipmentRecharge,BoostHp,BurnNearby,CrippleWardOnLevel,EmpowerAlways,Ghost,Incubator,LevelBonus,WarCryOnCombat,TempestOnKill", "Comma-separated peculiar items available as prayer rewards.");

            if (ModCompatibilityInLobbyConfig.enabled)
            {
                ModCompatibilityInLobbyConfig.CreateFromBepInExConfigFile(config, Main.PluginName);
            }
        }
    }

}
