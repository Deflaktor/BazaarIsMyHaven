using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using R2API;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BazaarIsMyHome
{
    class ModConfig
    {
        // general
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> EnableWelcomeWord;
        public static ConfigEntry<bool> EnableNoKickFromShop;
        public static ConfigEntry<ShopKeep.DeathState> NewtSecondLifeMode;
        public static ConfigEntry<bool> EnableLines;
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

        // lunarShop
        public static ConfigEntry<bool> LunarShopSectionEnabled;
        public static ConfigEntry<int> LunarShopTerminalCount;
        public static ConfigEntry<int> LunarShopTerminalCost;
        public static ConfigEntry<int> LunarShopBuyLimit;
        public static ConfigEntry<bool> EnableLunarShopStaticItems;
        public static ConfigEntry<string> LunarShopItemsList;

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
        public static ConfigEntry<bool> ReplaceLunarSeersWithEquipment;

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
            EnableMod = config.Bind("00 General", "Enabled", true, "Enable Mod\n启用模组");
            
            EnableWelcomeWord = config.Bind("00 General", "EnableWelcomeWord", true, "Enable welcome words.\n启用商人欢迎语");
            EnableNoKickFromShop = config.Bind("00 General", "EnableNoKickFromShop", true, "Enable no shoper kick out.\n启用商店防踢");
            NewtSecondLifeMode = config.Bind("00 General", "NewtDeath", ShopKeep.DeathState.Tank, "Change Newt Behavior on Death.\nDefault: Vanilla Behavior\nTank: Newt doubles health after each death\nGhost: Newt becomes a ghost\nEvil: Newt revives and tries to kill you.");
            EnableLines = config.Bind("00 General", "EnableLines", true, "Enable Newt speak function.\n启用商人台词");

            EnableAutoOpenShop = config.Bind("00 General", "AlwaysSpawnShopPortal", false, "Spawn portal to the Bazaar after every teleporter event.");
            SpawnCountByStage = config.Bind("00 General", "SpawnCountByStage", true, "Increase the number of devices based on stage. true = Yes\n是否随关卡数增加所有设备数量? true = 是\nFormula公式：Total Qty = stage + offset");
            SpawnCountOffset = config.Bind("00 General", "SpawnCountOffset", 0, "Associate the above parameters, value of offset.\n关联上面参数，增加设备数量的位移数");
            EnableDecorate = config.Bind("00 General", "EnableDecorate", true, "Enable bazaar decoration, someting behide newt like blue portal, chest...This might be my next mod idea.\n启用装饰，就是商人后面的装饰品，如果还活着会是下个mod的彩蛋吧");

            PrinterSectionEnabled = config.Bind("01 Printer", "SectionEnabled", true, "Enables/Disables Printer Section");
            PrinterCount = config.Bind("01 Printer", "PrinterCount", 9, "Total generated value of printers, max is 9, below zero is not enabled.\n月店3D打印机的数量，最多9台，小于0不启用"); if (PrinterCount.Value > 9) PrinterCount.Value = 9;
            PrinterTier1Weight = config.Bind("01 Printer", "PrinterTier1Weight", 0.8f, "Weight of white items. \n打印机出现白色物品的比重"); PrinterTier1Weight.Value = Math.Abs(PrinterTier1Weight.Value);
            PrinterTier2Weight = config.Bind("01 Printer", "PrinterTier2Weight", 0.2f, "Weight of green items. \n打印机出现绿色物品的比重"); PrinterTier2Weight.Value = Math.Abs(PrinterTier2Weight.Value);
            PrinterTier3Weight = config.Bind("01 Printer", "PrinterTier3Weight", 0.01f, "Weight of red items. \n打印机出现红色物品的比重"); PrinterTier3Weight.Value = Math.Abs(PrinterTier3Weight.Value);
            PrinterTierBossWeight = config.Bind("01 Printer", "PrinterBossWeight", 0.01f, "Weight of boss items. \n打印机出现黄色物品的比重"); PrinterTierBossWeight.Value = Math.Abs(PrinterTierBossWeight.Value);
            PrinterTierVoid1Weight = config.Bind("01 Printer", "PrinterTierVoid1Weight", 0.02f, "Weight of void tier 1 items, exchange red for void. \n打印机出现紫色物品的比重，用红色物品打印"); PrinterTierVoid1Weight.Value = Math.Abs(PrinterTierVoid1Weight.Value);
            PrinterTierVoid2Weight = config.Bind("01 Printer", "PrinterTierVoid2Weight", 0.02f, "Weight of void tier 2 items, exchange red for void. \n打印机出现紫色物品的比重，用红色物品打印"); PrinterTierVoid2Weight.Value = Math.Abs(PrinterTierVoid2Weight.Value);
            PrinterTierVoid3Weight = config.Bind("01 Printer", "PrinterTierVoid3Weight", 0.008f, "Weight of void tier 3 items, exchange red for void. \n打印机出现紫色物品的比重，用红色物品打印"); PrinterTierVoid3Weight.Value = Math.Abs(PrinterTierVoid3Weight.Value);
            PrinterTierVoidBossWeight = config.Bind("01 Printer", "PrinterTierVoidBossWeight", 0.004f, "Weight of void tier boss items, exchange red for void. \n打印机出现紫色物品的比重，用红色物品打印"); PrinterTierVoidBossWeight.Value = Math.Abs(PrinterTierVoidBossWeight.Value);
            PrinterTierVoidAllWeight = config.Bind("01 Printer", "PrinterTierVoidAllWeight", 0.0f, "Weight of void items, exchange red for void. \n打印机出现紫色物品的比重，用红色物品打印"); PrinterTierVoidAllWeight.Value = Math.Abs(PrinterTierVoidAllWeight.Value);

            CauldronSectionEnabled = config.Bind("02 Cauldron", "SectionEnabled", true, "Enables/Disables Cauldron Section");
            CauldronCount = config.Bind("02 Cauldron", "CauldronCount", 7, "Total generated value of cauldrons, max is 7, below zero is not enabled."); if (CauldronCount.Value > 7) CauldronCount.Value = 7;
            CauldronWhiteWeight = config.Bind("02 Cauldron", "CauldronWhiteWeight", 0.3f, "Weight of white cauldron."); CauldronWhiteWeight.Value = Math.Abs(CauldronWhiteWeight.Value);
            CauldronGreenWeight = config.Bind("02 Cauldron", "CauldronGreenWeight", 0.6f, "Weight of green cauldron."); CauldronGreenWeight.Value = Math.Abs(CauldronGreenWeight.Value);
            CauldronRedWeight = config.Bind("02 Cauldron", "CauldronRedWeight", 0.1f, "Weight of red cauldron."); CauldronRedWeight.Value = Math.Abs(CauldronRedWeight.Value);
            CauldronYellowWeight = config.Bind("02 Cauldron", "CauldronYellowWeight", 0.33f, "Weight of yellow cauldron (costs green items)."); CauldronYellowWeight.Value = Math.Abs(CauldronYellowWeight.Value);
            CauldronPurpleWeight = config.Bind("02 Cauldron", "CauldronPurpleWeight", 0.33f, "Weight of purple cauldron (costs green items)."); CauldronPurpleWeight.Value = Math.Abs(CauldronPurpleWeight.Value); 
            CauldronWhiteToGreenCost = config.Bind("02 Cauldron", "CauldronWhiteToGreenCost", 3, "Green cauldron requires the number of white items.\n(被黑后)绿锅需要物品数量"); CauldronWhiteToGreenCost.Value = Math.Abs(CauldronWhiteToGreenCost.Value);
            CauldronGreenToRedCost = config.Bind("02 Cauldron", "CauldronGreenToRedCost", 5, "Red cauldron requires the number of green items.\n(被黑后)红锅需要物品数量"); CauldronGreenToRedCost.Value = Math.Abs(CauldronGreenToRedCost.Value);
            CauldronRedToWhiteCost = config.Bind("02 Cauldron", "CauldronRedToWhiteCost", 1, "White cauldron requires the number of red items."); CauldronRedToWhiteCost.Value = Math.Abs(CauldronRedToWhiteCost.Value);

            ScrapperSectionEnabled = config.Bind("03 Scrapper", "SectionEnabled", true, "Enables/Disables Scrapper Section");
            ScrapperCount = config.Bind("03 Scrapper", "ScrapperCount", 4, "Total generated value of scrappers, max is 4, below zero is not enabled. \n月店收割机的数量，最多4台，小于0不启用"); if (ScrapperCount.Value > 4) ScrapperCount.Value = 4;

            EquipmentSectionEnabled = config.Bind("04 Equipment", "SectionEnabled", true, "Enables/Disables Equipment Section");
            EquipmentCount = config.Bind("04 Equipment", "EquipmentCount", 3, "Total generated value of equipments, max is 3, below zero is not enabled. \n月店主动装备的数量，最多6台，小于0不启用"); if (EquipmentCount.Value > 3) EquipmentCount.Value = 3;

            LunarShopSectionEnabled = config.Bind("05 LunarShop", "SectionEnabled", true, "Enables/Disables LunarShop Section");
            LunarShopTerminalCount = config.Bind("05 LunarShop", "LunarShopTerminalCount", 5, "Total generated value of LunarShopTerminal, max is 20, below zero is not enabled. \n月店月球装备的数量，最多11个，包括原有的5个，小于0不启用"); if (LunarShopTerminalCount.Value > 20) LunarShopTerminalCount.Value = 20;
            LunarShopTerminalCost = config.Bind("05 LunarShop", "LunarShopTerminalCost", 2, "Price of LunarShopTerminal. \n月球装备价格"); LunarShopTerminalCost.Value = Math.Abs(LunarShopTerminalCost.Value);
            LunarShopBuyLimit = config.Bind("05 LunarShop", "LunarShopBuyLimit", 5, "With how many LunarShopTerminals each player can interact during a single Bazaar visit."); LunarShopBuyLimit.Value = Math.Abs(LunarShopBuyLimit.Value);
            EnableLunarShopStaticItems = config.Bind("05 LunarShop", "EnableLunarShopStaticItems", true, "Enable LunarShop static items (non-randomized).");
            var items = "LunarPrimaryReplacement, LunarSecondaryReplacement, LunarSpecialReplacement, AutoCastEquipment, LunarDagger, HalfSpeedDoubleHealth, LunarSun, LunarBadLuck, LunarBadLuck, LunarBadLuck, ShieldOnly, ShieldOnly, ShieldOnly, HalfAttackSpeedHalfCooldowns, HalfAttackSpeedHalfCooldowns, RandomDamageZone, Tonic";
            var itemTiersString = "Tier1, Tier2, Tier3, Lunar, Boss, VoidTier1, VoidTier2, VoidTier3, VoidBoss";
            LunarShopItemsList = config.Bind("05 LunarShop", "LunarShopItems", items, $"List of items available at the LunarShop, separated by comma. Must be internal names as defined in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/ or item tiers ({itemTiersString}).");

            LunarRecyclerSectionEnabled = config.Bind("06 LunarRecycler", "SectionEnabled", true, "Enables/Disables LunarRecycler Section");
            LunarRecyclerAvailable = config.Bind("06 LunarRecycler", "LunarRecyclerAvailable", true, "Enable Lunar Recycler.\n切片能否购买？");
            LunarRecyclerRerollCount = config.Bind("06 LunarRecycler", "LunarRecyclerRerollCount", 10, "Recycler reroll max count.\n切片翻滚次数");
            LunarRecyclerRerollCost = config.Bind("06 LunarRecycler", "LunarRecyclerRerollCost", 1, "Recycler starting lunar coin cost.\n切片翻滚起步价格");
            LunarRecyclerRerollCostMultiplier = config.Bind("06 LunarRecycler", "LunarRecyclerRerollCostMultiplier", 2, "Multiplier to lunar coin cost after each use.\n切片翻滚增长倍数");

            CleansingPoolSectionEnabled = config.Bind("07 CleansingPool", "SectionEnabled", true, "Enables/Disables CleansingPool Section");
            ShrineCleaseGivesLunarCoins = config.Bind("07 CleansingPool", "CleansingPoolGivesLunarCoins", true, "Instead of giving Pearls, the Cleansing Pool will reward Lunar Coins inside the Bazaar.");

            LunarSeerSectionEnabled = config.Bind("08 LunarSeer", "SectionEnabled", true, "Enables/Disables SeerStation Section");
            SeerStationAvailable = config.Bind("08 LunarSeer", "LunarSeerStationAvailable", true, "Enable seerstations.\n预言地图能否购买");
            SeerStationsCost = config.Bind("08 LunarSeer", "LunarSeerStationsCost", 3, "Seerstations lunar coins cost.\n预言地图价格");
            ReplaceLunarSeersWithEquipment = config.Bind("08 LunarSeer", "ReplaceLunarSeersWithEquipment", true, "Replaces the Lunar Seers with Equipment Terminals. Makes the other options irrelevant.");

            ShrineRestackSectionEnabled = config.Bind("09 ShrineOfOrder", "SectionEnabled", true, "Enables/Disables ShrineOfOrder Section");
            ShrineRestackMaxCount = config.Bind("09 ShrineOfOrder", "ShrineOfOrderMaxCount", 99, "The value of total restack times. Download this mod [BazaarLunarForEveryOne] can be limited to once per person.\n跌序可以次数，搭配BazaarForEveryOne模组可以限制只赌博一次");
            ShrineRestackCost = config.Bind("09 ShrineOfOrder", "ShrineOfOrderCost", 1, "Shrinerestack starting lunar coins cost.\n跌序起步价");
            ShrineRestackScalar = config.Bind("09 ShrineOfOrder", "ShrineOfOrderScalar", 2, "Multiple increases in lunar coins cost.\n跌序价格增长倍数");

            PrayerSectionEnabled = config.Bind("10 Prayer", "SectionEnabled", true, "Enables/Disables Prayer Section");
            PrayRewardCount = config.Bind("10 Prayer", "RewardCount", 3, "Pray to the Shrinehealing, get a reward for every 10 prayers.\n祈祷获得物品的次数，10次祈祷获得一次");
            PrayCost = config.Bind("10 Prayer", "PrayCost", 10, "How many lunar coins does it take to pray once.\n祈祷一次需要多少月币");
            PrayNormalWeight = config.Bind("10 Prayer", "PrayNormalWeight", 0.5f, "Weight of normal items, like white, green, red items.\n祈祷出现一般物品的比重"); PrayNormalWeight.Value = Math.Abs(PrayNormalWeight.Value);
            PrayEliteWeight = config.Bind("10 Prayer", "PrayEliteWeight", 0.25f, "Weight of elite equipment.\n祈祷出现精英主动装备的比重"); PrayEliteWeight.Value = Math.Abs(PrayEliteWeight.Value);
            PrayPeculiarWeight = config.Bind("10 Prayer", "PrayPeculiarWeight", 0.25f, "Weight of peculiar items, some items don't drop in the game, just for fun.\n祈祷出现独特物品的比重，一些在游戏不会掉落的物品"); PrayPeculiarWeight.Value = Math.Abs(PrayPeculiarWeight.Value);
            PrayPeculiarList = config.Bind("10 Prayer", "PrayPeculiarList", "BoostAttackSpeed,BoostDamage,BoostEquipmentRecharge,BoostHp,BurnNearby,CrippleWardOnLevel,CooldownOnCrit,EmpowerAlways,Ghost,Incubator,InvadingDoppelganger,LevelBonus,WarCryOnCombat,TempestOnKill", "Peculiar items list.\n祈祷出现独特物品列表");

            if (ModCompatibilityInLobbyConfig.enabled)
            {
                ModCompatibilityInLobbyConfig.CreateFromBepInExConfigFile(config, Main.PluginName);
            }
        }
    }

}
