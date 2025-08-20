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
        public static ConfigEntry<bool> EnableWelcomeWord;
        public static ConfigEntry<bool> EnableNoKickFromShop;
        public static ConfigEntry<ShopKeep.DeathState> NewtSecondLifeMode;
        public static ConfigEntry<bool> EnableNewtTrashTalk;
        public static ConfigEntry<bool> SpawnCountByStage;
        public static ConfigEntry<int> SpawnCountOffset;
        public static ConfigEntry<bool> EnableAutoOpenShop;
        public static ConfigEntry<bool> EnableDecorate;
        //public static ConfigEntry<string> HitWords;

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
        public static ConfigEntry<float> PrinterTierVoidAllWeight;

        // cauldron
        public static ConfigEntry<bool> CauldronSectionEnabled;
        public static ConfigEntry<int> CauldronCount;
        public static ConfigEntry<float> CauldronWhiteWeight;
        public static ConfigEntry<float> CauldronGreenWeight;
        public static ConfigEntry<float> CauldronRedWeight;
        public static ConfigEntry<float> CauldronYellowWeight;
        public static ConfigEntry<float> CauldronPurpleWeight;
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
        public static ConfigEntry<int> LunarRecyclerRerollCount;

        // cleansing pool
        public static ConfigEntry<bool> CleansingPoolSectionEnabled;
        public static ConfigEntry<bool> ShrineCleaseGivesLunarCoins;

        // lunarSeer
        public static ConfigEntry<bool> LunarSeerSectionEnabled;
        public static ConfigEntry<bool> SeerStationAvailable;
        public static ConfigEntry<int> SeerStationsCost;

        // restack shrine
        public static ConfigEntry<bool> ShrineRestackSectionEnabled;
        public static ConfigEntry<int> ShrineRestackMaxCount;
        public static ConfigEntry<int> ShrineRestackCost;
        public static ConfigEntry<int> ShrineRestackScalar;

        // prayer
        public static ConfigEntry<bool> PrayerSectionEnabled;
        public static ConfigEntry<int> PrayCost;
        public static ConfigEntry<int> PrayRewardCount;
        public static ConfigEntry<float> PrayNormalWeight;
        public static ConfigEntry<float> PrayEliteWeight;
        public static ConfigEntry<float> PrayPeculiarWeight;
        public static ConfigEntry<string> PrayPeculiarList;

        public static void InitConfig(ConfigFile config)
        {
            EnableMod = config.Bind("00 General", "Enabled", true, "Enable Mod");
            
            EnableWelcomeWord = config.Bind("00 General", "EnableWelcomeWord", true, "Enable welcome words by the Newt. Though if he has been killed before, he may be a bit cranky.");
            EnableNoKickFromShop = config.Bind("00 General", "EnableNoKickFromShop", true, "Enable no kick out from Bazaar.");
            NewtSecondLifeMode = config.Bind("00 General", "NewtDeath", ShopKeep.DeathState.Tank, "Change Newt Behavior on Death.\nDefault: Vanilla Behavior\nTank: Newt doubles health after each death\nGhost: Newt becomes a ghost\nEvil: Newt revives and tries to kill you.");
            EnableNewtTrashTalk = config.Bind("00 General", "EnableNewtTrashTalk", true, "Hitting the Newt, will make him reply with snarky comments.");
            EnableAutoOpenShop = config.Bind("00 General", "AlwaysSpawnShopPortal", false, "Spawn a portal to the Bazaar after every teleporter event.");
            SpawnCountByStage = config.Bind("00 General", "SpawnCountByStage", true, "Increase the number of devices based on stage. true = Yes\nFormula：Total Qty = stage + offset");
            SpawnCountOffset = config.Bind("00 General", "SpawnCountOffset", 0, "Offset on how many devices per stage.");
            EnableDecorate = config.Bind("00 General", "EnableDecorate", true, "Enable bazaar decoration, places some chests and a portal behind Newt.");

            PrinterSectionEnabled = config.Bind("01 Printer", "SectionEnabled", true, "Enables/Disables Printer Section");
            PrinterCount = config.Bind("01 Printer", "PrinterCount", 9, "Total number of printers, max is 9."); if (PrinterCount.Value > 9) PrinterCount.Value = 9;
            PrinterTier1Weight = config.Bind("01 Printer", "PrinterTier1Weight", 0.8f, "Weight of white items."); PrinterTier1Weight.Value = Math.Abs(PrinterTier1Weight.Value);
            PrinterTier2Weight = config.Bind("01 Printer", "PrinterTier2Weight", 0.2f, "Weight of green items."); PrinterTier2Weight.Value = Math.Abs(PrinterTier2Weight.Value);
            PrinterTier3Weight = config.Bind("01 Printer", "PrinterTier3Weight", 0.01f, "Weight of red items."); PrinterTier3Weight.Value = Math.Abs(PrinterTier3Weight.Value);
            PrinterTierBossWeight = config.Bind("01 Printer", "PrinterBossWeight", 0.01f, "Weight of boss items."); PrinterTierBossWeight.Value = Math.Abs(PrinterTierBossWeight.Value);
            PrinterTierVoid1Weight = config.Bind("01 Printer", "PrinterTierVoid1Weight", 0.02f, "Weight of void tier 1 items, exchange red for void."); PrinterTierVoid1Weight.Value = Math.Abs(PrinterTierVoid1Weight.Value);
            PrinterTierVoid2Weight = config.Bind("01 Printer", "PrinterTierVoid2Weight", 0.02f, "Weight of void tier 2 items, exchange red for void."); PrinterTierVoid2Weight.Value = Math.Abs(PrinterTierVoid2Weight.Value);
            PrinterTierVoid3Weight = config.Bind("01 Printer", "PrinterTierVoid3Weight", 0.008f, "Weight of void tier 3 items, exchange red for void."); PrinterTierVoid3Weight.Value = Math.Abs(PrinterTierVoid3Weight.Value);
            PrinterTierVoidBossWeight = config.Bind("01 Printer", "PrinterTierVoidBossWeight", 0.004f, "Weight of void tier boss items, exchange red for void."); PrinterTierVoidBossWeight.Value = Math.Abs(PrinterTierVoidBossWeight.Value);
            PrinterTierVoidAllWeight = config.Bind("01 Printer", "PrinterTierVoidAllWeight", 0.0f, "Weight of void items, exchange red for void."); PrinterTierVoidAllWeight.Value = Math.Abs(PrinterTierVoidAllWeight.Value);

            CauldronSectionEnabled = config.Bind("02 Cauldron", "SectionEnabled", true, "Enables/Disables Cauldron Section");
            CauldronCount = config.Bind("02 Cauldron", "CauldronCount", 7, "Total generated value of cauldrons, max is 7, below zero is not enabled."); if (CauldronCount.Value > 7) CauldronCount.Value = 7;
            CauldronWhiteWeight = config.Bind("02 Cauldron", "CauldronWhiteWeight", 0.3f, "Weight of white cauldron."); CauldronWhiteWeight.Value = Math.Abs(CauldronWhiteWeight.Value);
            CauldronGreenWeight = config.Bind("02 Cauldron", "CauldronGreenWeight", 0.6f, "Weight of green cauldron."); CauldronGreenWeight.Value = Math.Abs(CauldronGreenWeight.Value);
            CauldronRedWeight = config.Bind("02 Cauldron", "CauldronRedWeight", 0.1f, "Weight of red cauldron."); CauldronRedWeight.Value = Math.Abs(CauldronRedWeight.Value);
            CauldronYellowWeight = config.Bind("02 Cauldron", "CauldronYellowWeight", 0.33f, "Weight of yellow cauldron (costs green items)."); CauldronYellowWeight.Value = Math.Abs(CauldronYellowWeight.Value);
            CauldronPurpleWeight = config.Bind("02 Cauldron", "CauldronPurpleWeight", 0.33f, "Weight of purple cauldron (costs green items)."); CauldronPurpleWeight.Value = Math.Abs(CauldronPurpleWeight.Value); 
            CauldronWhiteToGreenCost = config.Bind("02 Cauldron", "CauldronWhiteToGreenCost", 3, "Green cauldron amount of required white items."); CauldronWhiteToGreenCost.Value = Math.Abs(CauldronWhiteToGreenCost.Value);
            CauldronGreenToRedCost = config.Bind("02 Cauldron", "CauldronGreenToRedCost", 5, "Red cauldron amount of required green items."); CauldronGreenToRedCost.Value = Math.Abs(CauldronGreenToRedCost.Value);
            CauldronRedToWhiteCost = config.Bind("02 Cauldron", "CauldronRedToWhiteCost", 1, "White cauldron amount of required red items."); CauldronRedToWhiteCost.Value = Math.Abs(CauldronRedToWhiteCost.Value);

            ScrapperSectionEnabled = config.Bind("03 Scrapper", "SectionEnabled", true, "Enables/Disables Scrapper Section");
            ScrapperCount = config.Bind("03 Scrapper", "ScrapperCount", 4, "Total generated amount of scrappers, max is 4, below zero is not enabled."); if (ScrapperCount.Value > 4) ScrapperCount.Value = 4;

            EquipmentSectionEnabled = config.Bind("04 Equipment", "SectionEnabled", true, "Enables/Disables Equipment Section");
            ReplaceLunarSeersWithEquipment = config.Bind("04 Equipment", "ReplaceLunarSeersWithEquipment", true, "Replaces the Lunar Seers with Equipment Terminals, bumping the Max Equipment Count to 5. Makes the Lunar Seer Section irrelevant.");
            EquipmentCount = config.Bind("04 Equipment", "EquipmentCount", 3, "Total generated value of equipments, max is 3, unless ReplaceLunarSeersWithEquipment is enabled.");
            EquipmentInstanced = config.Bind("04 Equipment", "EquipmentInstanced", true, "Allows each player to buy equipment independently.");
            EquipmentCost = config.Bind("04 Equipment", "EquipmentCost", 0, "How much an equipment costs money.");
            EquipmentBuyToInventory = config.Bind("04 Equipment", "EquipmentBuyToInventory", true, "Whether the items should go directly into the inventory instead of dropping on the ground first.");
            if (EquipmentCount.Value > 3 && !ReplaceLunarSeersWithEquipment.Value) EquipmentCount.Value = 3;
            if (EquipmentCount.Value > 5 && ReplaceLunarSeersWithEquipment.Value) EquipmentCount.Value = 5;

            LunarShopSectionEnabled = config.Bind("05 LunarShop", "SectionEnabled", true, "Enables/Disables LunarShop Section");
            LunarShopTerminalCount = config.Bind("05 LunarShop", "LunarShopTerminalCount", 5, "Places special LunarShopTerminals instead of the vanilla Lunar Buds. Max is 20. Below zero is vanilla behavior, ie. 5 Lunar Buds."); if (LunarShopTerminalCount.Value > 20) LunarShopTerminalCount.Value = 20;
            LunarShopTerminalCost = config.Bind("05 LunarShop", "LunarShopTerminalCost", 2, "Price of LunarShopTerminal/LunarBud"); LunarShopTerminalCost.Value = Math.Abs(LunarShopTerminalCost.Value);
            LunarShopBuyLimit = config.Bind("05 LunarShop", "LunarShopBuyLimit", 5, "With how many LunarShopTerminals/LunarBuds each player can interact during a single Bazaar visit."); LunarShopBuyLimit.Value = Math.Abs(LunarShopBuyLimit.Value);
            EnableLunarShopStaticItems = config.Bind("05 LunarShop", "EnableLunarShopStaticItems", true, "Enable LunarShop static items (non-randomized).");
            var items = "LunarPrimaryReplacement, LunarSecondaryReplacement, LunarSpecialReplacement, AutoCastEquipment, LunarDagger, HalfSpeedDoubleHealth, LunarSun, LunarBadLuck, LunarBadLuck, LunarBadLuck, ShieldOnly, ShieldOnly, ShieldOnly, HalfAttackSpeedHalfCooldowns, HalfAttackSpeedHalfCooldowns, RandomDamageZone, Tonic";
            var itemTiersString = "Tier1, Tier2, Tier3, Lunar, Boss, VoidTier1, VoidTier2, VoidTier3, VoidBoss";
            LunarShopItemsList = config.Bind("05 LunarShop", "LunarShopItems", items, $"List of items available at the LunarShop, separated by comma. Must be internal names as defined in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/ or item tiers ({itemTiersString}).");
            LunarShopInstanced = config.Bind("05 LunarShop", "LunarShopInstanced", true, "Allows each player to buy items from the LunarShop independently.");
            LunarShopBuyToInventory = config.Bind("05 LunarShop", "LunarShopBuyToInventory", true, "Whether the items should go directly into the inventory instead of dropping on the ground first.");

            LunarRecyclerSectionEnabled = config.Bind("06 LunarRecycler", "SectionEnabled", true, "Enables/Disables LunarRecycler Section");
            LunarRecyclerAvailable = config.Bind("06 LunarRecycler", "LunarRecyclerAvailable", true, "Enable Lunar Recycler.");
            LunarRecyclerRerollCount = config.Bind("06 LunarRecycler", "LunarRecyclerRerollCount", 10, "Recycler reroll max count.");
            LunarRecyclerRerollCost = config.Bind("06 LunarRecycler", "LunarRecyclerRerollCost", 1, "Recycler starting lunar coin cost.");
            LunarRecyclerRerollCostMultiplier = config.Bind("06 LunarRecycler", "LunarRecyclerRerollCostMultiplier", 2, "Multiplier to lunar coin cost after each use.");

            CleansingPoolSectionEnabled = config.Bind("07 CleansingPool", "SectionEnabled", true, "Enables/Disables CleansingPool Section");
            ShrineCleaseGivesLunarCoins = config.Bind("07 CleansingPool", "CleansingPoolGivesLunarCoins", true, "Instead of giving Pearls, the Cleansing Pool will reward Lunar Coins inside the Bazaar.");

            LunarSeerSectionEnabled = config.Bind("08 LunarSeer", "SectionEnabled", true, "Enables/Disables SeerStation Section");
            SeerStationAvailable = config.Bind("08 LunarSeer", "LunarSeerStationAvailable", true, "Enable seerstations.");
            SeerStationsCost = config.Bind("08 LunarSeer", "LunarSeerStationsCost", 3, "Seerstations lunar coins cost.");

            ShrineRestackSectionEnabled = config.Bind("09 ShrineOfOrder", "SectionEnabled", true, "Enables/Disables ShrineOfOrder Section");
            ShrineRestackMaxCount = config.Bind("09 ShrineOfOrder", "ShrineOfOrderMaxCount", 99, "The value of total restack times. Download this mod [BazaarLunarForEveryOne] can be limited to once per person.\n跌序可以次数，搭配BazaarForEveryOne模组可以限制只赌博一次");
            ShrineRestackCost = config.Bind("09 ShrineOfOrder", "ShrineOfOrderCost", 1, "Shrinerestack starting lunar coins cost.");
            ShrineRestackScalar = config.Bind("09 ShrineOfOrder", "ShrineOfOrderScalar", 2, "Multiple increases in lunar coins cost.");

            PrayerSectionEnabled = config.Bind("10 Prayer", "SectionEnabled", true, "Enables/Disables Prayer Section");
            PrayRewardCount = config.Bind("10 Prayer", "RewardCount", 3, "Pray to the Shrine, get a reward for every 10 prayers.");
            PrayCost = config.Bind("10 Prayer", "PrayCost", 2, "How many lunar coins it costs to pray once.");
            PrayNormalWeight = config.Bind("10 Prayer", "PrayNormalWeight", 0.5f, "Weight of normal items, like white, green, red items."); PrayNormalWeight.Value = Math.Abs(PrayNormalWeight.Value);
            PrayEliteWeight = config.Bind("10 Prayer", "PrayEliteWeight", 0.25f, "Weight of elite equipment."); PrayEliteWeight.Value = Math.Abs(PrayEliteWeight.Value);
            PrayPeculiarWeight = config.Bind("10 Prayer", "PrayPeculiarWeight", 0.25f, "Weight of peculiar items, some items don't drop in the game, just for fun."); PrayPeculiarWeight.Value = Math.Abs(PrayPeculiarWeight.Value);
            PrayPeculiarList = config.Bind("10 Prayer", "PrayPeculiarList", "BoostAttackSpeed,BoostDamage,BoostEquipmentRecharge,BoostHp,BurnNearby,CrippleWardOnLevel,EmpowerAlways,Ghost,Incubator,LevelBonus,WarCryOnCombat,TempestOnKill", "Peculiar items list.");

            if (ModCompatibilityInLobbyConfig.enabled)
            {
                ModCompatibilityInLobbyConfig.CreateFromBepInExConfigFile(config, Main.PluginName);
            }
        }
    }

}
