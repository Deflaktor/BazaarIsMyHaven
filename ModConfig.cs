using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BazaarIsMyHaven
{
    class ModConfig
    {
        // general
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> AlwaysSpawnShopPortal;
        public static ConfigEntry<bool> ScaleMoneyCosts;
        public static ConfigEntry<bool> PreventDrifterBagging;
        public static ConfigEntry<bool> PreventDrifterJunk;
        public static ConfigEntry<bool> PreventDrifterTinker;
        public static ConfigEntry<int> DirectorExtraCredits;
        public static ConfigEntry<int> DirectorCreditsPerStage;

        // newt
        public static ConfigEntry<bool> NewtSectionEnabled;
        public static ConfigEntry<bool> NewtNoKickFromShop;
        public static ConfigEntry<ShopKeeper.KarmaTrigger> NewtKarmaTrigger;
        public static ConfigEntry<bool> NewtKarmaTriggerOncePerVisit;
        public static ConfigEntry<ShopKeeper.DeathState> NewtDeathBehavior;
        public static ConfigEntry<ShopKeeper.Dialogue> NewtDialogue;

        // printer
        public static ConfigEntry<bool> PrinterSectionEnabled;
        public static ConfigEntry<int> PrinterMaxAmount;
        public static ConfigEntry<float> PrinterDirectorWeight;
        public static ConfigEntry<int> PrinterDirectorCostWhite;
        public static ConfigEntry<int> PrinterDirectorCostGreen;
        public static ConfigEntry<int> PrinterDirectorCostRed;
        public static ConfigEntry<int> PrinterDirectorCostBoss;
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
        public static ConfigEntry<int> CauldronMaxAmount;
        public static ConfigEntry<int> CauldronDirectorCost;
        public static ConfigEntry<float> CauldronDirectorWeight;
        public static ConfigEntry<float> CauldronWhiteToGreenWeight;
        public static ConfigEntry<float> CauldronGreenToRedWeight;
        public static ConfigEntry<float> CauldronRedToWhiteWeight;
        public static ConfigEntry<int> CauldronWhiteToGreenCost;
        public static ConfigEntry<int> CauldronGreenToRedCost;
        public static ConfigEntry<int> CauldronRedToWhiteCost;

        // scrapper
        public static ConfigEntry<bool> ScrapperSectionEnabled;
        public static ConfigEntry<int> ScrapperMaxAmount;
        public static ConfigEntry<int> ScrapperDirectorCost;
        public static ConfigEntry<float> ScrapperDirectorWeight;

        // equipment
        public static ConfigEntry<bool> EquipmentSectionEnabled;
        public static ConfigEntry<int> EquipmentMaxAmount;
        public static ConfigEntry<int> EquipmentDirectorCost;
        public static ConfigEntry<float> EquipmentDirectorWeight;
        public static ConfigEntry<bool> EquipmentInstancedPurchases;
        public static ConfigEntry<int> EquipmentCost;
        public static ConfigEntry<CostTypeIndex> EquipmentCostType;
        public static ConfigEntry<bool> EquipmentBuyToInventory;
        public static ConfigEntry<bool> EquipmentReplaceLunarSeersWithEquipment;
        public static ConfigEntry<float> EquipmentReplaceWithEliteChance;
        public static ConfigEntry<string> EquipmentReplaceWithEliteList;

        // lunarShop
        public static ConfigEntry<bool> LunarShopSectionEnabled;
        public static ConfigEntry<int> LunarShopMaxAmount;
        public static ConfigEntry<int> LunarShopDirectorCost;
        public static ConfigEntry<float> LunarShopDirectorWeight;
        public static ConfigEntry<int> LunarShopCost;
        public static ConfigEntry<CostTypeIndex> LunarShopCostType;
        public static ConfigEntry<int> LunarShopBuyLimit;
        public static ConfigEntry<bool> LunarShopSequentialItems;
        public static ConfigEntry<string> LunarShopItemList;
        public static ConfigEntry<bool> LunarShopInstancedPurchases;
        public static ConfigEntry<bool> LunarShopBuyToInventory;
        public static ConfigEntry<string> LunarShopAmountDependingOnCharacter;
        public static Dictionary<BodyIndex, int> LunarShopAmountDependingOnCharacterParsed = new Dictionary<BodyIndex, int>();

        // lunarRecycler
        public static ConfigEntry<bool> LunarRecyclerSectionEnabled;
        public static ConfigEntry<bool> LunarRecyclerAvailable;
        public static ConfigEntry<int> LunarRecyclerCost;
        public static ConfigEntry<int> LunarRecyclerCostMultiplier;
        public static ConfigEntry<int> LunarRecyclerRerollLimit;

        // cleansing pool
        public static ConfigEntry<bool> CleansingPoolSectionEnabled;
        public static ConfigEntry<bool> CleansingPoolRewardsLunarCoins;
        public static ConfigEntry<int> CleansingPoolDirectorCost;
        public static ConfigEntry<float> CleansingPoolDirectorWeight;

        // lunarSeer
        public static ConfigEntry<bool> LunarSeerSectionEnabled;
        public static ConfigEntry<bool> LunarSeerAvailable;
        public static ConfigEntry<int> LunarSeerCost;

        // restack shrine
        public static ConfigEntry<bool> ShrineOfOrderSectionEnabled;
        public static ConfigEntry<int> ShrineOfOrderUseLimit;
        public static ConfigEntry<int> ShrineOfOrderCost;
        public static ConfigEntry<int> ShrineOfOrderDirectorCost;
        public static ConfigEntry<float> ShrineOfOrderDirectorWeight;

        // donate
        public static ConfigEntry<bool> DonateSectionEnabled;
        public static ConfigEntry<int> DonateCost;
        public static ConfigEntry<CostTypeIndex> DonateCostType;
        public static ConfigEntry<int> DonateDirectorCost;
        public static ConfigEntry<float> DonateDirectorWeight;
        public static ConfigEntry<int> DonateRewardLimitPerVisit;
        public static ConfigEntry<int> DonateRewardLimitPerRun;
        public static ConfigEntry<bool> DonateSequentialRewardLists;
        public static ConfigEntry<float> DonateRewardList1Weight;
        public static ConfigEntry<string> DonateRewardList1;
        public static ConfigEntry<float> DonateRewardList2Weight;
        public static ConfigEntry<string> DonateRewardList2;
        public static ConfigEntry<float> DonateRewardList3Weight;
        public static ConfigEntry<string> DonateRewardList3;
        public static ConfigEntry<string> DonateRewardListAvailableCharacters;
        public static ConfigEntry<float> DonateRewardListCharacterWeight;
        public static ConfigEntry<string> DonateRewardListCharacterDefault;
        public static Dictionary<BodyIndex, ConfigEntry<string>> DonateRewardListCharacters = new Dictionary<BodyIndex, ConfigEntry<string>>();

        // wandering chef
        public static ConfigEntry<bool> WanderingChefSectionEnabled;
        public static ConfigEntry<bool> WanderingChefUnrestrictedCrafting;
        public static ConfigEntry<bool> WanderingChefCraftableRecipesOnly;
        public static ConfigEntry<bool> WanderingChefBuyToInventory;
        public static ConfigEntry<int> WanderingChefDirectorCost;
        public static ConfigEntry<float> WanderingChefDirectorWeight;

        // drones
        public static ConfigEntry<bool> DronesSectionEnabled;
        public static ConfigEntry<int> DronesDroneCombinerStationDirectorCost;
        public static ConfigEntry<float> DronesDroneCombinerStationDirectorWeight;
        public static ConfigEntry<int> DronesDroneUpgradeStationDirectorCost;
        public static ConfigEntry<float> DronesDroneUpgradeStationDirectorWeight;
        public static ConfigEntry<int> DronesDroneScrapperDirectorCost;
        public static ConfigEntry<float> DronesDroneScrapperDirectorWeight;
        public static ConfigEntry<int> DronesTripleDroneShopDirectorCost;
        public static ConfigEntry<float> DronesTripleDroneShopDirectorWeight;

        private const string itemTiersString = "Tier1 | Tier2 | Tier3 | Lunar | Boss | VoidTier1 | VoidTier2 | VoidTier3 | VoidBoss | FoodTier";
        private const string itemKeyWordsExplanation = $"Can use:\n - internal item names(see https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/)\n- item tier keywords ({itemTiersString})\n- droptable names (see README.md)\nFollows ItemStringParser format.";
        private static bool reloading;

        public static void InitConfig(ConfigFile config)
        {
            // 00 General
            EnableMod = config.Bind("00 General", "Enabled", true, "Enable Mod");
            AlwaysSpawnShopPortal = config.Bind("00 General", "AlwaysSpawnShopPortal", false, "Spawn a portal to the Bazaar after each teleporter event.");
            ScaleMoneyCosts = config.Bind("00 General", "ScaleMoneyCosts", true, "Costs paid in Money scale with difficulty, like the game's own shops. E.g. an Equipment Terminal with Cost 50 then costs what the game's triple equipment shop costs.");
            PreventDrifterBagging = config.Bind("00 General", "PreventDrifterBagging", true, "Drifter can't put anything into his bag in the Bazaar: no interactables, no Newt, no other players or drones. The bag stays empty, like when he grabs something he can't carry.");
            PreventDrifterJunk = config.Bind("00 General", "PreventDrifterJunk", true, "Drifter gains no Junk in the Bazaar, so Salvage can't farm temporary items there or on the next stage. Junk brought into the Bazaar can still be used.");
            PreventDrifterTinker = config.Bind("00 General", "PreventDrifterTinker", true, "Drifter's Tinker changes nothing in the Bazaar: it doesn't reroll Lunar Buds, terminals or printers, and doesn't turn item pickups into scrap and temporary items. The shockwave still hits.");
            DirectorExtraCredits = config.Bind("00 General", "DirectorExtraCredits", 15, "Credits the director gets on every visit to the Bazaar, on top of DirectorCreditsPerStage. Like the game's scene director, it spends them at random on the sections' interactables, by their DirectorCost and DirectorWeight.");
            DirectorCreditsPerStage = config.Bind("00 General", "DirectorCreditsPerStage", 25, "Credits the director gets for every stage cleared in the run.");

            // 01 Newt
            NewtSectionEnabled = config.Bind("01 Newt", "SectionEnabled", true, "Enables or disables the Newt section.");
            NewtDialogue = config.Bind("01 Newt", "Dialogue", ShopKeeper.Dialogue.Default, "What Newt says in chat.\nOff: Nothing\nDefault: The game's own unused Newt lines when you donate, buy from a Lunar Bud or hit him, shown in each player's language. He also greets you, in English.\nBazaarIsMyHome: The greetings and remarks of the original BazaarIsMyHome mod, in the host's language");
            NewtNoKickFromShop = config.Bind("01 Newt", "NoKickFromShop", true, "Prevents being kicked out of the Bazaar after angering Newt.");
            NewtKarmaTrigger = config.Bind("01 Newt", "KarmaTrigger", ShopKeeper.KarmaTrigger.OnKill, "When angering Newt raises the run's karma, if karma is enabled for the run.\nOnDamage: Vanilla - When he would kick you out after being hit, also if NoKickFromShop keeps you in\nOnKill: Only when he is killed");
            NewtKarmaTriggerOncePerVisit = config.Bind("01 Newt", "KarmaTriggerOncePerVisit", true, "Angering Newt raises the run's karma at most once per visit to the Bazaar, even if he revives and is angered again.");
            NewtDeathBehavior = config.Bind("01 Newt", "DeathBehavior", ShopKeeper.DeathState.Tank, "Determines Newt's behavior after death.\nDefault: Vanilla - Unchanged behavior\nTank: Doubles Newt's health after each kill\nGhost: Newt respawns as a passive ghost\nHostile: Newt revives hostile and attacks players");

            // 02 Printer
            PrinterSectionEnabled = config.Bind("02 Printer", "SectionEnabled", true, "Enables or disables the Printer section.");
            PrinterMaxAmount = config.Bind("02 Printer", "MaxAmount", 9, new ConfigDescription("Maximum number of printers the director can spawn (max 9).", new AcceptableValueRange<int>(0, 9)));
            PrinterDirectorWeight = config.Bind("02 Printer", "DirectorWeight", 4f, "How likely the director picks a printer, compared to the DirectorWeight of the other interactables. Which tier it prints is rolled by the tier weights below.");
            PrinterDirectorCostWhite = config.Bind("02 Printer", "DirectorCostWhite", 5, "Director credits a printer for white (Tier 1) items costs.");
            PrinterDirectorCostGreen = config.Bind("02 Printer", "DirectorCostGreen", 10, "Director credits a printer for green (Tier 2) items costs.");
            PrinterDirectorCostRed = config.Bind("02 Printer", "DirectorCostRed", 15, "Director credits a printer for red (Tier 3) or Void items costs.");
            PrinterDirectorCostBoss = config.Bind("02 Printer", "DirectorCostBoss", 10, "Director credits a printer for Boss items costs.");
            PrinterTier1Weight = config.Bind("02 Printer", "Tier1Weight", 0.5f, "Weight for Tier 1 (white) items.");
            PrinterTier2Weight = config.Bind("02 Printer", "Tier2Weight", 0.25f, "Weight for Tier 2 (green) items.");
            PrinterTier3Weight = config.Bind("02 Printer", "Tier3Weight", 0.17f, "Weight for Tier 3 (red) items.");
            PrinterTierBossWeight = config.Bind("02 Printer", "BossWeight", 0.04f, "Weight for Boss-tier items.");
            PrinterTierVoid1Weight = config.Bind("02 Printer", "TierVoid1Weight", 0.015f, "Weight for Tier 1 Void items (traded from red).");
            PrinterTierVoid2Weight = config.Bind("02 Printer", "TierVoid2Weight", 0.015f, "Weight for Tier 2 Void items (traded from red).");
            PrinterTierVoid3Weight = config.Bind("02 Printer", "TierVoid3Weight", 0.006f, "Weight for Tier 3 Void items (traded from red).");
            PrinterTierVoidBossWeight = config.Bind("02 Printer", "TierVoidBossWeight", 0.003f, "Weight for Boss-tier Void items (traded from red).");
            PrinterTierVoidAnyWeight = config.Bind("02 Printer", "TierVoidAnyWeight", 0f, "Weight for Void items of any tier (traded from red).");

            // 03 Cauldron
            CauldronSectionEnabled = config.Bind("03 Cauldron", "SectionEnabled", true, "Enables or disables the Cauldron section.");
            CauldronMaxAmount = config.Bind("03 Cauldron", "MaxAmount", 7, new ConfigDescription("Maximum number of cauldrons the director can spawn (max 7).", new AcceptableValueRange<int>(0, 7)));
            CauldronDirectorCost = config.Bind("03 Cauldron", "DirectorCost", 10, "Director credits a cauldron costs.");
            CauldronDirectorWeight = config.Bind("03 Cauldron", "DirectorWeight", 2f, "How likely the director picks a cauldron, compared to the DirectorWeight of the other interactables. Which type it is is rolled by the weights below.");
            CauldronWhiteToGreenWeight = config.Bind("03 Cauldron", "WhiteToGreenWeight", 0.75f, "Weight for White->Green cauldrons.");
            CauldronWhiteToGreenCost = config.Bind("03 Cauldron", "WhiteToGreenCost", 3, "Number of White items required for Green conversion.");
            CauldronGreenToRedWeight = config.Bind("03 Cauldron", "GreenToRedWeight", 0.25f, "Weight for Green->Red cauldrons.");
            CauldronGreenToRedCost = config.Bind("03 Cauldron", "GreenToRedCost", 5, "Number of Green items required for Red conversion.");
            CauldronRedToWhiteWeight = config.Bind("03 Cauldron", "RedToWhiteWeight", 0f, "Weight for Red->White cauldrons.");
            CauldronRedToWhiteCost = config.Bind("03 Cauldron", "RedToWhiteCost", 1, "Number of Red items required for White conversion.");

            // 04 Scrapper
            ScrapperSectionEnabled = config.Bind("04 Scrapper", "SectionEnabled", true, "Enables or disables the Scrapper section.");
            ScrapperMaxAmount = config.Bind("04 Scrapper", "MaxAmount", 2, new ConfigDescription("Number of Scrappers spawned when the director picks them (max 2).", new AcceptableValueRange<int>(0, 2)));
            ScrapperDirectorCost = config.Bind("04 Scrapper", "DirectorCost", 5, "Director credits the Scrappers cost. The director picks them only once, and then all of them spawn.");
            ScrapperDirectorWeight = config.Bind("04 Scrapper", "DirectorWeight", 1f, "How likely the director picks the Scrappers, compared to the DirectorWeight of the other interactables.");

            // 05 Equipment
            EquipmentSectionEnabled = config.Bind("05 Equipment", "SectionEnabled", true, "Enables or disables the Equipment section.");
            EquipmentMaxAmount = config.Bind("05 Equipment", "MaxAmount", 3, new ConfigDescription("Maximum number of Equipment Terminals the director can spawn (max 3 normally, 5 if replacing Lunar Seers).", new AcceptableValueRange<int>(0, 5)));
            EquipmentDirectorCost = config.Bind("05 Equipment", "DirectorCost", 2, "Director credits an Equipment Terminal costs.");
            EquipmentDirectorWeight = config.Bind("05 Equipment", "DirectorWeight", 1f, "How likely the director picks an Equipment Terminal, compared to the DirectorWeight of the other interactables.");
            EquipmentReplaceLunarSeersWithEquipment = config.Bind("05 Equipment", "ReplaceLunarSeersWithEquipment", false, "Replaces Lunar Seers with Equipment Terminals (increases equipment max to 5). Makes the Lunar Seer section irrelevant.");
            EquipmentReplaceWithEliteChance = config.Bind("05 Equipment", "ReplaceWithEliteChance", 0.15f, "Chance for replacing the equipment with an elite equipment.");
            EquipmentReplaceWithEliteList = config.Bind("05 Equipment", "ReplaceWithEliteList", "EliteEarthEquipment | EliteFireEquipment | EliteIceEquipment | EliteLunarEquipment | EliteLightningEquipment", "With which elite equipments to replace. List in the format equipment1 | equipment2. Can use:\\n- internal item names (see https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/)\\n- item tier keywords ({itemTiersString})\\n- droptable names (see README.md). Follows ItemStringParser format.");
            EquipmentInstancedPurchases = config.Bind("05 Equipment", "InstancedPurchases", true, "Each player can purchase equipment independently.");
            EquipmentCost = config.Bind("05 Equipment", "Cost", 0, "Cost of an equipment purchase, paid in CostType.");
            EquipmentCostType = config.Bind("05 Equipment", "CostType", CostTypeIndex.Money, "What equipment purchases cost, e.g. Money, LunarCoin, WhiteItem or PercentHealth. The price sign shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");
            EquipmentBuyToInventory = config.Bind("05 Equipment", "BuyToInventory", true, "Purchased equipment goes directly into inventory instead of dropping to the ground.");

            // 06 LunarShop
            LunarShopSectionEnabled = config.Bind("06 LunarShop", "SectionEnabled", true, "Enables or disables the Lunar Shop section.");
            LunarShopMaxAmount = config.Bind("06 LunarShop", "MaxAmount", 5, new ConfigDescription("Maximum number of Lunar Buds the director can spawn (max 20).", new AcceptableValueRange<int>(0, 20)));
            LunarShopDirectorCost = config.Bind("06 LunarShop", "DirectorCost", 0, "Director credits a Lunar Bud costs. With 0, all buds up to MaxAmount always spawn.");
            LunarShopDirectorWeight = config.Bind("06 LunarShop", "DirectorWeight", 1f, "How likely the director picks a Lunar Bud, compared to the DirectorWeight of the other interactables.");
            LunarShopCost = config.Bind("06 LunarShop", "Cost", 1, "Cost of a Lunar Bud purchase, paid in CostType.");
            LunarShopCostType = config.Bind("06 LunarShop", "CostType", CostTypeIndex.LunarCoin, "What Lunar Bud purchases cost, e.g. LunarCoin, Money, WhiteItem or PercentHealth. The price tag shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");
            LunarShopBuyLimit = config.Bind("06 LunarShop", "BuyLimit", 5, "Limit on Lunar Shop purchases each player can make per visit to the Bazaar. -1 = Unlimited.");
            LunarShopSequentialItems = config.Bind("06 LunarShop", "SequentialItems", false, "Picks items sequentially from the list instead of randomly.");
            var items = "Tonic | AutoCastEquipment | RandomDamageZone | LunarDagger | HalfSpeedDoubleHealth | ShieldOnly | ShieldOnly | ShieldOnly | LunarPrimaryReplacement | LunarSecondaryReplacement | LunarSpecialReplacement | LunarBadLuck | LunarBadLuck | LunarBadLuck | LunarSun | HalfAttackSpeedHalfCooldowns | HalfAttackSpeedHalfCooldowns";
            LunarShopItemList = config.Bind("06 LunarShop", "ItemList", "dtLunarChest", $"List of items available in Lunar Shop. {itemKeyWordsExplanation} Example: {items}");
            LunarShopInstancedPurchases = config.Bind("06 LunarShop", "InstancedPurchases", true, "Each player can buy independently from Lunar Shop.");
            LunarShopBuyToInventory = config.Bind("06 LunarShop", "BuyToInventory", true, "Items go directly into inventory instead of dropping on ground.");
            LunarShopAmountDependingOnCharacter = config.Bind("06 LunarShop", "AmountDependingOnCharacter", "", "Change the number of Lunar Buds by Character. Needs InstancedPurchases. Cannot exceed the number of spawned buds. Comma-separated <characterBody|survivorName>=<amount>.\nExample: Seeker=15, FalseSon=16");

            // 07 LunarRecycler
            LunarRecyclerSectionEnabled = config.Bind("07 LunarRecycler", "SectionEnabled", true, "Enables or disables the Lunar Recycler section.");
            LunarRecyclerAvailable = config.Bind("07 LunarRecycler", "Available", true, "If enabled, a Lunar Recycler is available in the Bazaar. Otherwise it will get removed. It is also removed when the director spawns no Lunar Buds.");
            LunarRecyclerRerollLimit = config.Bind("07 LunarRecycler", "RerollLimit", 3, "Limit the amount of rerolls allowed per visit to the Bazaar. -1 = Unlimited.");
            LunarRecyclerCost = config.Bind("07 LunarRecycler", "Cost", 1, "Initial lunar coin cost to reroll.");
            LunarRecyclerCostMultiplier = config.Bind("07 LunarRecycler", "CostMultiplier", 2, "Cost multiplier applied after each reroll use.");

            // 08 CleansingPool
            CleansingPoolSectionEnabled = config.Bind("08 CleansingPool", "SectionEnabled", true, "Enables or disables the Cleansing Pool section.");
            CleansingPoolRewardsLunarCoins = config.Bind("08 CleansingPool", "RewardsLunarCoin", true, "Cleansing Pools reward a Lunar Coin instead of a Pearl.");
            CleansingPoolDirectorCost = config.Bind("08 CleansingPool", "DirectorCost", 5, "Director credits the Cleansing Pool costs.");
            CleansingPoolDirectorWeight = config.Bind("08 CleansingPool", "DirectorWeight", 1f, "How likely the director picks the Cleansing Pool, compared to the DirectorWeight of the other interactables.");

            // 09 LunarSeer
            LunarSeerSectionEnabled = config.Bind("09 LunarSeer", "SectionEnabled", true, "Enables or disables the Lunar Seer Station section.");
            LunarSeerAvailable = config.Bind("09 LunarSeer", "Available", true, "If enabled, Seer Stations appear in the Bazaar.");
            LunarSeerCost = config.Bind("09 LunarSeer", "Cost", 3, "Lunar coin cost for using Seer Stations.");

            // 10 ShrineOfOrder
            ShrineOfOrderSectionEnabled = config.Bind("10 ShrineOfOrder", "SectionEnabled", true, "Enables or disables the Shrine of Order section. Enabling lets the director spawn a Shrine of Order near the Newt.");
            ShrineOfOrderUseLimit = config.Bind("10 ShrineOfOrder", "UseLimit", -1, "Limit the amount of Shrine of Order uses per visit to the Bazaar. -1 = Unlimited.");
            ShrineOfOrderCost = config.Bind("10 ShrineOfOrder", "Cost", 1, "Initial lunar coin cost for Shrine of Order.");
            ShrineOfOrderDirectorCost = config.Bind("10 ShrineOfOrder", "DirectorCost", 5, "Director credits the Shrine of Order costs.");
            ShrineOfOrderDirectorWeight = config.Bind("10 ShrineOfOrder", "DirectorWeight", 1f, "How likely the director picks the Shrine of Order, compared to the DirectorWeight of the other interactables.");

            // 11 Donation
            DonateSectionEnabled = config.Bind("11 Donate", "SectionEnabled", true, "Enables or disables the Donate section. Enabling lets the director spawn a Donation Altar near the Newt.");
            DonateRewardLimitPerVisit = config.Bind("11 Donate", "RewardLimitPerVisit", 1, "Limit the number of rewards each player can get per visit to the Bazaar.");
            DonateRewardLimitPerRun = config.Bind("11 Donate", "RewardLimitPerRun", 1, "Limit the number of rewards each player can get in total over the course of the complete run. Must be equal or higher than RewardLimitPerVisit.");
            DonateCost = config.Bind("11 Donate", "Cost", 5, "Cost of a donation, paid in CostType.");
            DonateCostType = config.Bind("11 Donate", "CostType", CostTypeIndex.LunarCoin, "What a donation costs, e.g. LunarCoin, Money, WhiteItem or PercentHealth. The price tag shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");
            DonateDirectorCost = config.Bind("11 Donate", "DirectorCost", 10, "Director credits the Donation Altar costs.");
            DonateDirectorWeight = config.Bind("11 Donate", "DirectorWeight", 1f, "How likely the director picks the Donation Altar, compared to the DirectorWeight of the other interactables.");
            DonateSequentialRewardLists = config.Bind("11 Donate", "SequentialRewardLists", false, "If enabled, the reward lists will be choosen sequentially, ordered from heighest weight to lowest weight. The current reward list is stored for each player per run and swaps around. Reward lists with weight = 0 will be ignored.");
            DonateRewardList1Weight = config.Bind("11 Donate", "RewardList1Weight", 0.60f, "Weight for choosing RewardList1.");
            DonateRewardList1 = config.Bind("11 Donate", "RewardList1", "2xdtChest1 | Tier2", $"Item reward pool. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation} Example: Hoof & Tier1");
            DonateRewardList2Weight = config.Bind("11 Donate", "RewardList2Weight", 0.10f, "Weight for choosing RewardList2.");
            DonateRewardList2 = config.Bind("11 Donate", "RewardList2", "Tier3 | Boss", $"Item reward pool. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation} Example: AlienHead | 2xTier2");
            DonateRewardList3Weight = config.Bind("11 Donate", "RewardList3Weight", 0f, "Weight for choosing RewardList3.");
            DonateRewardList3 = config.Bind("11 Donate", "RewardList3", "10xBoostAttackSpeed | 10xBoostDamage | 10xBoostEquipmentRecharge | 10xBoostHp | 10xCrippleWardOnLevel | EmpowerAlways | Ghost | 3xIncubator | 10xLevelBonus | 10xWarCryOnCombat | 10xTempestOnKill", $"Item reward pool. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation}");

            DonateRewardListCharacterWeight = config.Bind("11 Donate", "RewardListCharacterWeight", 0.30f, "Weight for choosing the reward list of the respective character.");
            DonateRewardListCharacterDefault = config.Bind("11 Donate", "RewardListCharacterDefault", "3xScrapGreen", $"Item reward pool for characters without a reward list of their own, or whose list gives no reward. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation}");
            string[] survivorNames = SurvivorCatalog.allSurvivorDefs.Select(survivor => survivor.cachedName).ToArray();
            DonateRewardListAvailableCharacters = config.Bind("11 Donate", "RewardListAvailableCharacters", string.Join(", ", survivorNames), "Available special reward lists for certain characters. Applies only if the donator is a character of the respective type. Comma-separated list of characters (see README.md)");
            
            // 12 Wandering Chef
            WanderingChefSectionEnabled = config.Bind("12 WanderingChef", "SectionEnabled", true, "Enables or disables the Wandering Chef section. Only works if the Alloyed Collective is enabled for the run. Enabling lets the director spawn a Wandering Chef near the Lunar Shop.");
            WanderingChefUnrestrictedCrafting = config.Bind("12 WanderingChef", "UnrestrictedCrafting", false, "Allows you to craft anything. Otherwise crafting is restricted to a single randomly selected target pickup.");
            WanderingChefCraftableRecipesOnly = config.Bind("12 WanderingChef", "CraftableRecipesOnly", true, "Only if UnrestrictedCrafting is false: The Wandering Chef only offers a recipe that a player can cook from their inventory and what the Bazaar offers (items in printers, Lunar Buds, Equipment Terminals and cauldrons, and scrap if there are Scrappers). If there is none, he leaves and the director spends his credits on something else.");
            WanderingChefBuyToInventory = config.Bind("12 WanderingChef", "BuyToInventory", true, "Items go directly into inventory instead of dropping on ground.");
            WanderingChefDirectorCost = config.Bind("12 WanderingChef", "DirectorCost", 7, "Director credits the Wandering Chef costs.");
            WanderingChefDirectorWeight = config.Bind("12 WanderingChef", "DirectorWeight", 0.5f, "How likely the director picks the Wandering Chef, compared to the DirectorWeight of the other interactables.");

            // 13 Drones
            DronesSectionEnabled = config.Bind("13 Drones", "SectionEnabled", true, "Enables or disables the Drones section. Only works if the Alloyed Collective is enabled for the run. Enabling lets the director spawn the drone stations below, and the players' drones come along into the Bazaar like on any other stage.");
            DronesDroneCombinerStationDirectorCost = config.Bind("13 Drones", "DroneCombinerStationDirectorCost", 1, "Director credits the Drone Combiner Station costs.");
            DronesDroneCombinerStationDirectorWeight = config.Bind("13 Drones", "DroneCombinerStationDirectorWeight", 1f, "How likely the director picks the Drone Combiner Station, compared to the DirectorWeight of the other interactables. 0 = never spawns.");
            DronesDroneUpgradeStationDirectorCost = config.Bind("13 Drones", "DroneUpgradeStationDirectorCost", 5, "Director credits the Drone Upgrade Station costs.");
            DronesDroneUpgradeStationDirectorWeight = config.Bind("13 Drones", "DroneUpgradeStationDirectorWeight", 1f, "How likely the director picks the Drone Upgrade Station, compared to the DirectorWeight of the other interactables. 0 = never spawns.");
            DronesDroneScrapperDirectorCost = config.Bind("13 Drones", "DroneScrapperDirectorCost", 3, "Director credits the Drone Scrapper costs.");
            DronesDroneScrapperDirectorWeight = config.Bind("13 Drones", "DroneScrapperDirectorWeight", 1f, "How likely the director picks the Drone Scrapper, compared to the DirectorWeight of the other interactables. 0 = never spawns.");
            DronesTripleDroneShopDirectorCost = config.Bind("13 Drones", "TripleDroneShopDirectorCost", 8, "Director credits the Triple Drone Shop costs.");
            DronesTripleDroneShopDirectorWeight = config.Bind("13 Drones", "TripleDroneShopDirectorWeight", 1f, "How likely the director picks the Triple Drone Shop, compared to the DirectorWeight of the other interactables. 0 = never spawns.");

            RefreshDerivedData(config);
            LunarShopAmountDependingOnCharacter.SettingChanged += LunarShopCharacterAmountsChanged;
            DonateRewardListAvailableCharacters.SettingChanged += DonationCharactersChanged;

            if (ModCompatibilityInLobbyConfig.enabled)
            {
                ModCompatibilityInLobbyConfig.CreateFromBepInExConfigFile(config, Main.PluginName);
            }
        }

        // Re-reads the file, so edits apply from the next run without restarting the game.
        public static void ReloadConfig(ConfigFile config)
        {
            bool saveOnConfigSet = config.SaveOnConfigSet;
            reloading = true;
            try
            {
                // Reload sets the entries one at a time; saving after each would rewrite the file mid-reload
                config.SaveOnConfigSet = false;
                config.Reload();
            }
            catch (Exception exception)
            {
                Log.LogError($"Could not reload the config: {exception}");
            }
            finally
            {
                config.SaveOnConfigSet = saveOnConfigSet;
                reloading = false;
            }
            RefreshDerivedData(config);
        }

        private static void RefreshDerivedData(ConfigFile config)
        {
            if (EquipmentMaxAmount.Value > 3 && !EquipmentReplaceLunarSeersWithEquipment.Value) EquipmentMaxAmount.Value = 3;
            if (EquipmentMaxAmount.Value > 5 && EquipmentReplaceLunarSeersWithEquipment.Value) EquipmentMaxAmount.Value = 5;
            // Count is not a cost type
            if (EquipmentCostType.Value >= CostTypeIndex.Count) EquipmentCostType.Value = CostTypeIndex.Money;
            if (LunarShopCostType.Value >= CostTypeIndex.Count) LunarShopCostType.Value = CostTypeIndex.LunarCoin;
            if (DonateCostType.Value >= CostTypeIndex.Count) DonateCostType.Value = CostTypeIndex.LunarCoin;
            RefreshLunarShopCharacterAmounts();
            RefreshDonationRewardLists(config);
        }

        private static void LunarShopCharacterAmountsChanged(object sender, EventArgs args)
        {
            if (!reloading)
                RefreshLunarShopCharacterAmounts();
        }

        private static void DonationCharactersChanged(object sender, EventArgs args)
        {
            if (!reloading)
                RefreshDonationRewardLists(DonateRewardListAvailableCharacters.ConfigFile);
        }

        private static void RefreshLunarShopCharacterAmounts()
        {
            LunarShopAmountDependingOnCharacterParsed.Clear();
            if (!string.IsNullOrWhiteSpace(LunarShopAmountDependingOnCharacter.Value))
            {
                string[] characterStrings = LunarShopAmountDependingOnCharacter.Value.Split(",");
                foreach (string characterString in characterStrings)
                {
                    var nameAmountStr = characterString.Split("=");
                    if (nameAmountStr.Length != 2) {
                        Log.LogWarning($"Could not parse character string {characterString} for AmountDependingOnCharacter");
                        continue;
                    }
                    var name = nameAmountStr[0].Trim();
                    var amountStr = nameAmountStr[1].Trim();
                    if(int.TryParse(amountStr, out var amount))
                    {
                        if (string.IsNullOrWhiteSpace(name)) {
                            Log.LogWarning($"Could not parse character string name in {characterString} for AmountDependingOnCharacter");
                            continue;
                        }
                        BodyIndex bodyIndex = CatalogHelper.FindBody(name);
                        if (bodyIndex == BodyIndex.None)
                        {
                            Log.LogError($"AmountDependingOnCharacter: Could not find body or survivor: {name}");
                            continue;
                        }
                        LunarShopAmountDependingOnCharacterParsed[bodyIndex] = amount;
                    }
                    else
                    {
                        Log.LogWarning($"Could not parse character string amount {amountStr} in {characterString} for AmountDependingOnCharacter");
                    }
                }
            }
        }

        private static void RefreshDonationRewardLists(ConfigFile config)
        {
            DonateRewardListCharacters.Clear();
            string[] availableCharacters = DonateRewardListAvailableCharacters.Value.Split(",");
            foreach (string availableCharacter in availableCharacters)
            {
                var name = availableCharacter.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                BodyIndex bodyIndex = CatalogHelper.FindBody(name);
                if (bodyIndex == BodyIndex.None)
                {
                    Log.LogError($"RewardListAvailableCharacters: Could not find body or survivor: {name}");
                    continue;
                }
                string defaultReward;
                switch (name)
                {
                    case "Bandit2":
                        defaultReward = "BleedOnHitAndExplode";
                        break;
                    case "Captain":
                        defaultReward = "3xBleedOnHit";
                        break;
                    case "Commando":
                        defaultReward = "5xStickyBomb";
                        break;
                    case "Croco":
                        defaultReward = "TriggerEnemyDebuffs";
                        break;
                    case "Engi":
                        defaultReward = "HealingPotion";
                        break;
                    case "Heretic":
                        defaultReward = "2xHealWhileSafe";
                        break;
                    case "Huntress":
                        defaultReward = "2xAttackSpeedOnCrit";
                        break;
                    case "Loader":
                        defaultReward = "2xIceRing";
                        break;
                    case "Mage":
                        defaultReward = "StrengthenBurn";
                        break;
                    case "Merc":
                        defaultReward = "AlienHead";
                        break;
                    case "Toolbot":
                        defaultReward = "5xSecondarySkillMagazine";
                        break;
                    case "Treebot":
                        defaultReward = "3xMedkit";
                        break;
                    case "Railgunner":
                        defaultReward = "CritDamage";
                        break;
                    case "VoidSurvivor":
                        defaultReward = "3xSlowOnHitVoid";
                        break;
                    case "Chef":
                        defaultReward = "5xAttackSpeedPerNearbyAllyOrEnemy";
                        break;
                    case "FalseSon":
                        defaultReward = "3xInfusion";
                        break;
                    case "Seeker":
                        defaultReward = "5xArmorPlate";
                        break;
                    case "DroneTech":
                        defaultReward = "3xBarrierOnCooldown";
                        break;
                    case "Drifter":
                        defaultReward = "SpeedOnPickup";
                        break;
                    case "Acolyte":
                        defaultReward = "2xSeed";
                        break;
                    case "Enforcer2":
                        defaultReward = "3xBarrierOnKill";
                        break;
                    default:
                        defaultReward = "3xScrapGreen";
                        break;
                }   
                ConfigEntry<string> donateRewardListCharacter = config.Bind("11 Donate", $"RewardList{name}", defaultReward, $"Item reward pool for {name} in the format <amount1>x<item1> & <amount2>x<item2>. See README.md for more detailed description. {itemKeyWordsExplanation}");
                DonateRewardListCharacters[bodyIndex] = donateRewardListCharacter;
            }
        }
    }

}
