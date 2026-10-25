using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BazaarIsMyHaven
{
    class ModConfig
    {
        // general
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> AlwaysSpawnShopPortal;
        public static ConfigEntry<DisableTransferMoneyMode> DisableTransferMoney;
        public static ConfigEntry<bool> ScaleMoneyCosts;
        public static ConfigEntry<bool> PreventDrifterBagging;
        public static ConfigEntry<bool> PreventDrifterJunk;
        public static ConfigEntry<bool> PreventDrifterTinker;
        public static ConfigEntry<int> DirectorExtraCredits;
        public static ConfigEntry<int> DirectorCreditsPerStage;
        public static ConfigEntry<int> DirectorCreditsCap;

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
        public static ConfigEntry<float> CauldronTranscendentWeight;
        public static ConfigEntry<int> CauldronTranscendentCost;

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
        public static ConfigEntry<float> DronesTripleDroneShopCommonWeight;
        public static ConfigEntry<float> DronesTripleDroneShopUncommonWeight;
        public static ConfigEntry<int> DronesTripleDroneShopCommonCost;
        public static ConfigEntry<int> DronesTripleDroneShopUncommonCost;
        public static ConfigEntry<CostTypeIndex> DronesTripleDroneShopCostType;
        public static ConfigEntry<bool> DronesTripleDroneShopBuyAll;
        public static ConfigEntry<bool> DronesTripleDroneShopInstancedPurchases;

        // command pedestal
        public static ConfigEntry<bool> CommandPedestalSectionEnabled;
        public static ConfigEntry<float> CommandPedestalDirectorWeight;
        public static readonly List<CommandPedestalTier> CommandPedestalTiers = new List<CommandPedestalTier>();
        public static ConfigEntry<CostTypeIndex> CommandPedestalCostType;
        public static ConfigEntry<bool> CommandPedestalInstancedPurchases;
        public static ConfigEntry<bool> CommandPedestalBuyToInventory;

        private const string itemTiersString = "Tier1 | Tier2 | Tier3 | Lunar | Boss | VoidTier1 | VoidTier2 | VoidTier3 | VoidBoss | FoodTier";
        private const string itemKeyWordsExplanation = $"Can use:\n - internal item names(see https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/)\n- item tier keywords ({itemTiersString})\n- droptable names (see README.md)\nFollows ItemStringParser format.";
        private static bool reloading;

        public static void InitConfig(ConfigFile config)
        {
            ConfigMigration.BindVersion(config);

            // 00 General
            EnableMod = ModCompatibilityConfigPresets.Bind(config, "00 General", "Enabled", true, "Enable Mod");
            AlwaysSpawnShopPortal = ModCompatibilityConfigPresets.Bind(config, "00 General", "AlwaysSpawnShopPortal", false, "Spawn a portal to the Bazaar after each teleporter event.");
            DisableTransferMoney = ModCompatibilityConfigPresets.Bind(config, "00 General", "DisableTransferMoney", ModCompatibilityBiggerBazaar.enabled ? DisableTransferMoneyMode.InheritFromBiggerBazaar : DisableTransferMoneyMode.Off, "Whether the gold players have when they take the portal to the Bazaar turns into experience, as in the base game, instead of them keeping it to spend there. Named after BiggerBazaar's own disableTransferMoney. Gold left when they leave the Bazaar turns into experience as usual. The default is InheritFromBiggerBazaar with BiggerBazaar installed, otherwise Off.\nOff: Players keep their gold. If BiggerBazaar transfers the gold itself, with its disableTransferMoney off, that is used instead.\nOn: The gold turns into experience. BiggerBazaar still transfers the gold if its disableTransferMoney is off.\nInheritFromBiggerBazaar: BiggerBazaar's disableTransferMoney decides: off, it transfers the gold; on, the gold turns into experience. Without BiggerBazaar, the same as Off.");
            DirectorExtraCredits = ModCompatibilityConfigPresets.Bind(config, "00 General", "DirectorExtraCredits", 15, "Credits the director gets on every visit to the Bazaar, on top of DirectorCreditsPerStage. Like the game's scene director, it spends them at random on the sections' interactables, by their DirectorCost and DirectorWeight.");
            DirectorCreditsPerStage = ModCompatibilityConfigPresets.Bind(config, "00 General", "DirectorCreditsPerStage", 25, "Credits the director gets for every stage cleared in the run.");
            DirectorCreditsCap = ModCompatibilityConfigPresets.Bind(config, "00 General", "DirectorCreditsCap", 200, "The most credits the director gets on a visit, DirectorExtraCredits included. -1 = No cap.");
            ScaleMoneyCosts = ModCompatibilityConfigPresets.Bind(config, "00 General", "ScaleMoneyCosts", true, "Costs paid in Money scale with difficulty, like the game's own shops. E.g. an Equipment Terminal with Cost 50 then costs what the game's triple equipment shop costs.");
            PreventDrifterBagging = ModCompatibilityConfigPresets.Bind(config, "00 General", "PreventDrifterBagging", true, "Drifter can't put anything into his bag in the Bazaar: no interactables, no Newt, no other players or drones. The bag stays empty, like when he grabs something he can't carry.");
            PreventDrifterJunk = ModCompatibilityConfigPresets.Bind(config, "00 General", "PreventDrifterJunk", true, "Drifter gains no Junk in the Bazaar, so Salvage can't farm temporary items there or on the next stage. Junk brought into the Bazaar can still be used.");
            PreventDrifterTinker = ModCompatibilityConfigPresets.Bind(config, "00 General", "PreventDrifterTinker", true, "Drifter's Tinker changes nothing in the Bazaar: it doesn't reroll Lunar Buds, terminals or printers, and doesn't turn item pickups into scrap and temporary items. It causes issues together with Instanced Purchases.");

            // 01 Newt
            NewtSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "01 Newt", "SectionEnabled", true, "Enables or disables the Newt section.");
            NewtNoKickFromShop = ModCompatibilityConfigPresets.Bind(config, "01 Newt", "NoKickFromShop", true, "Prevents being kicked out of the Bazaar after angering Newt.");
            NewtDeathBehavior = ModCompatibilityConfigPresets.Bind(config, "01 Newt", "DeathBehavior", ShopKeeper.DeathState.Tank, "Determines Newt's behavior after death.\nDefault: Vanilla - Unchanged behavior\nTank: Doubles Newt's health after each kill\nGhost: Newt respawns as a passive ghost\nHostile: Newt revives hostile and attacks players");
            NewtKarmaTrigger = ModCompatibilityConfigPresets.Bind(config, "01 Newt", "KarmaTrigger", ShopKeeper.KarmaTrigger.OnKill, "When angering Newt raises the run's karma, if karma is enabled for the run.\nOnDamage: Vanilla - When he would kick you out after being hit, also if NoKickFromShop keeps you in\nOnKill: Only when he is killed");
            NewtKarmaTriggerOncePerVisit = ModCompatibilityConfigPresets.Bind(config, "01 Newt", "KarmaTriggerOncePerVisit", true, "Angering Newt raises the run's karma at most once per visit to the Bazaar, even if he revives and is angered again.");
            NewtDialogue = ModCompatibilityConfigPresets.Bind(config, "01 Newt", "Dialogue", ShopKeeper.Dialogue.Default, "What Newt says in chat.\nOff: Nothing\nDefault: The game's own unused Newt lines when you donate, buy from a Lunar Bud or hit him, shown in each player's language. He also greets you, in English.\nBazaarIsMyHome: The greetings and remarks of the original BazaarIsMyHome mod, in the host's language");

            // 02 Printer
            PrinterSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "SectionEnabled", true, "Enables or disables the Printer section.");
            PrinterMaxAmount = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "MaxAmount", 9, new ConfigDescription("Maximum number of printers the director can spawn (max 9).", new AcceptableValueRange<int>(0, 9)));
            PrinterDirectorCostWhite = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "DirectorCostWhite", 5, "Director credits a printer for white (Tier 1) items costs.");
            PrinterDirectorCostGreen = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "DirectorCostGreen", 10, "Director credits a printer for green (Tier 2) items costs.");
            PrinterDirectorCostRed = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "DirectorCostRed", 15, "Director credits a printer for red (Tier 3) or Void items costs.");
            PrinterDirectorCostBoss = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "DirectorCostBoss", 10, "Director credits a printer for Boss items costs.");
            PrinterDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "DirectorWeight", 4f, "How likely the director picks a printer, compared to the DirectorWeight of the other interactables. Which tier it prints is rolled by the tier weights below.");
            PrinterTier1Weight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "Tier1Weight", 0.5f, "Weight for Tier 1 (white) items.");
            PrinterTier2Weight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "Tier2Weight", 0.25f, "Weight for Tier 2 (green) items.");
            PrinterTier3Weight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "Tier3Weight", 0.17f, "Weight for Tier 3 (red) items.");
            PrinterTierBossWeight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "BossWeight", 0.04f, "Weight for Boss-tier items.");
            PrinterTierVoid1Weight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "TierVoid1Weight", 0.015f, "Weight for Tier 1 Void items (traded from red).");
            PrinterTierVoid2Weight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "TierVoid2Weight", 0.015f, "Weight for Tier 2 Void items (traded from red).");
            PrinterTierVoid3Weight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "TierVoid3Weight", 0.006f, "Weight for Tier 3 Void items (traded from red).");
            PrinterTierVoidBossWeight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "TierVoidBossWeight", 0.003f, "Weight for Boss-tier Void items (traded from red).");
            PrinterTierVoidAnyWeight = ModCompatibilityConfigPresets.Bind(config, "02 Printer", "TierVoidAnyWeight", 0f, "Weight for Void items of any tier (traded from red).");

            // 03 Cauldron
            CauldronSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "SectionEnabled", true, "Enables or disables the Cauldron section.");
            CauldronMaxAmount = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "MaxAmount", 7, new ConfigDescription("Maximum number of cauldrons the director can spawn (max 7).", new AcceptableValueRange<int>(0, 7)));
            CauldronDirectorCost = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "DirectorCost", 10, "Director credits a cauldron costs.");
            CauldronDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "DirectorWeight", 2f, "How likely the director picks a cauldron, compared to the DirectorWeight of the other interactables. Which type it is is rolled by the weights below.");
            CauldronWhiteToGreenWeight = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "WhiteToGreenWeight", 0.75f, "Weight for White->Green cauldrons.");
            CauldronGreenToRedWeight = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "GreenToRedWeight", 0.25f, "Weight for Green->Red cauldrons.");
            CauldronRedToWhiteWeight = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "RedToWhiteWeight", 0f, "Weight for Red->White cauldrons.");
            CauldronTranscendentWeight = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "TranscendentWeight", 0f, "Weight for Transcendent Cauldrons from Hallowed Concepts, which turn a Hex into a cursed item. Only spawns if Hallowed Concepts is enabled for the run.");
            CauldronWhiteToGreenCost = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "WhiteToGreenCost", 3, "Number of White items required for Green conversion.");
            CauldronGreenToRedCost = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "GreenToRedCost", 5, "Number of Green items required for Red conversion.");
            CauldronRedToWhiteCost = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "RedToWhiteCost", 1, "Number of Red items required for White conversion.");
            CauldronTranscendentCost = ModCompatibilityConfigPresets.Bind(config, "03 Cauldron", "TranscendentCost", 1, "Number of Hexes a Transcendent Cauldron takes.");

            // 04 Scrapper
            ScrapperSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "04 Scrapper", "SectionEnabled", true, "Enables or disables the Scrapper section.");
            ScrapperMaxAmount = ModCompatibilityConfigPresets.Bind(config, "04 Scrapper", "MaxAmount", 2, new ConfigDescription("Number of Scrappers spawned when the director picks them (max 2).", new AcceptableValueRange<int>(0, 2)));
            ScrapperDirectorCost = ModCompatibilityConfigPresets.Bind(config, "04 Scrapper", "DirectorCost", 3, "Director credits the Scrappers cost. The director picks them only once, and then all of them spawn.");
            ScrapperDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "04 Scrapper", "DirectorWeight", 1f, "How likely the director picks the Scrappers, compared to the DirectorWeight of the other interactables.");

            // 05 Equipment
            EquipmentSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "SectionEnabled", true, "Enables or disables the Equipment section.");
            EquipmentMaxAmount = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "MaxAmount", 3, new ConfigDescription("Maximum number of Equipment Terminals the director can spawn (max 3 normally, 5 if replacing Lunar Seers).", new AcceptableValueRange<int>(0, 5)));
            EquipmentDirectorCost = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "DirectorCost", 2, "Director credits an Equipment Terminal costs.");
            EquipmentDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "DirectorWeight", 1f, "How likely the director picks an Equipment Terminal, compared to the DirectorWeight of the other interactables.");
            EquipmentReplaceLunarSeersWithEquipment = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "ReplaceLunarSeersWithEquipment", false, "Replaces Lunar Seers with Equipment Terminals (increases equipment max to 5). Makes the Lunar Seer section irrelevant.");
            EquipmentReplaceWithEliteChance = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "ReplaceWithEliteChance", 0.15f, "Chance for replacing the equipment with an elite equipment.");
            EquipmentReplaceWithEliteList = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "ReplaceWithEliteList", "EliteEarthEquipment | EliteFireEquipment | EliteIceEquipment | EliteLunarEquipment | EliteLightningEquipment", "With which elite equipments to replace. List in the format equipment1 | equipment2. Can use:\\n- internal item names (see https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/)\\n- item tier keywords ({itemTiersString})\\n- droptable names (see README.md). Follows ItemStringParser format.");
            EquipmentCost = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "Cost", 50, "Cost of an equipment purchase, paid in CostType.");
            EquipmentCostType = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "CostType", CostTypeIndex.Money, "What equipment purchases cost, e.g. Money, LunarCoin, WhiteItem or PercentHealth. The price sign shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");
            EquipmentInstancedPurchases = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "InstancedPurchases", true, "Each player can purchase equipment independently.");
            EquipmentBuyToInventory = ModCompatibilityConfigPresets.Bind(config, "05 Equipment", "BuyToInventory", true, "Purchased equipment goes directly into inventory instead of dropping to the ground. Equipment replaced by a purchase goes into the terminal, where swapping again is free for the buyer.");

            // 06 LunarShop
            LunarShopSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "SectionEnabled", true, "Enables or disables the Lunar Shop section.");
            LunarShopMaxAmount = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "MaxAmount", 5, new ConfigDescription("Maximum number of Lunar Buds the director can spawn (max 20).", new AcceptableValueRange<int>(0, 20)));
            LunarShopDirectorCost = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "DirectorCost", 0, "Director credits a Lunar Bud costs. With 0, all buds up to MaxAmount always spawn.");
            LunarShopDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "DirectorWeight", 1f, "How likely the director picks a Lunar Bud, compared to the DirectorWeight of the other interactables.");
            var items = "Tonic | AutoCastEquipment | RandomDamageZone | LunarDagger | HalfSpeedDoubleHealth | ShieldOnly | ShieldOnly | ShieldOnly | LunarPrimaryReplacement | LunarSecondaryReplacement | LunarSpecialReplacement | LunarBadLuck | LunarBadLuck | LunarBadLuck | LunarSun | HalfAttackSpeedHalfCooldowns | HalfAttackSpeedHalfCooldowns";
            LunarShopItemList = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "ItemList", "dtLunarChest", $"List of items available in Lunar Shop. {itemKeyWordsExplanation} Example: {items}");
            LunarShopSequentialItems = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "SequentialItems", false, "Picks items sequentially from the list instead of randomly.");
            LunarShopCost = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "Cost", 2, "Cost of a Lunar Bud purchase, paid in CostType.");
            LunarShopCostType = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "CostType", CostTypeIndex.LunarCoin, "What Lunar Bud purchases cost, e.g. LunarCoin, Money, WhiteItem or PercentHealth. The price tag shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");
            LunarShopBuyLimit = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "BuyLimit", 5, "Limit on Lunar Shop purchases each player can make per visit to the Bazaar. -1 = Unlimited.");
            LunarShopInstancedPurchases = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "InstancedPurchases", true, "Each player can buy independently from Lunar Shop.");
            LunarShopAmountDependingOnCharacter = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "AmountDependingOnCharacter", "", "Change the number of Lunar Buds by Character. Needs InstancedPurchases. Cannot exceed the number of spawned buds. Comma-separated <characterBody|survivorName>=<amount>.\nExample: Seeker=15, FalseSon=16");
            LunarShopBuyToInventory = ModCompatibilityConfigPresets.Bind(config, "06 LunarShop", "BuyToInventory", true, "Items go directly into inventory instead of dropping on ground. Equipment replaced by a purchase goes into the bud, where swapping again is free for the buyer.");

            // 07 LunarRecycler
            LunarRecyclerSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "07 LunarRecycler", "SectionEnabled", true, "Enables or disables the Lunar Recycler section.");
            LunarRecyclerAvailable = ModCompatibilityConfigPresets.Bind(config, "07 LunarRecycler", "Available", true, "If enabled, a Lunar Recycler is available in the Bazaar. Otherwise it will get removed. It is also removed when the director spawns no Lunar Buds.");
            LunarRecyclerCost = ModCompatibilityConfigPresets.Bind(config, "07 LunarRecycler", "Cost", 1, "Initial lunar coin cost to reroll.");
            LunarRecyclerCostMultiplier = ModCompatibilityConfigPresets.Bind(config, "07 LunarRecycler", "CostMultiplier", 2, "Cost multiplier applied after each reroll use.");
            LunarRecyclerRerollLimit = ModCompatibilityConfigPresets.Bind(config, "07 LunarRecycler", "RerollLimit", 3, "Limit the amount of rerolls allowed per visit to the Bazaar. -1 = Unlimited.");

            // 08 CleansingPool
            CleansingPoolSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "08 CleansingPool", "SectionEnabled", true, "Enables or disables the Cleansing Pool section.");
            CleansingPoolDirectorCost = ModCompatibilityConfigPresets.Bind(config, "08 CleansingPool", "DirectorCost", 0, "Director credits the Cleansing Pool costs. Free by default, since with RewardsLunarCoin it only gives back a Lunar Coin, so it always spawns unless its DirectorWeight is 0. Vanilla value: 5.");
            CleansingPoolDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "08 CleansingPool", "DirectorWeight", 1f, "How likely the director picks the Cleansing Pool, compared to the DirectorWeight of the other interactables.");
            CleansingPoolRewardsLunarCoins = ModCompatibilityConfigPresets.Bind(config, "08 CleansingPool", "RewardsLunarCoin", true, "Cleansing Pools reward a Lunar Coin instead of a Pearl.");

            // 09 LunarSeer
            LunarSeerSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "09 LunarSeer", "SectionEnabled", true, "Enables or disables the Lunar Seer Station section.");
            LunarSeerAvailable = ModCompatibilityConfigPresets.Bind(config, "09 LunarSeer", "Available", true, "If enabled, Seer Stations appear in the Bazaar.");
            LunarSeerCost = ModCompatibilityConfigPresets.Bind(config, "09 LunarSeer", "Cost", 3, "Lunar coin cost for using Seer Stations.");

            // 10 ShrineOfOrder
            ShrineOfOrderSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "10 ShrineOfOrder", "SectionEnabled", true, "Enables or disables the Shrine of Order section. Enabling lets the director spawn a Shrine of Order near the Newt.");
            ShrineOfOrderDirectorCost = ModCompatibilityConfigPresets.Bind(config, "10 ShrineOfOrder", "DirectorCost", 5, "Director credits the Shrine of Order costs.");
            ShrineOfOrderDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "10 ShrineOfOrder", "DirectorWeight", 1f, "How likely the director picks the Shrine of Order, compared to the DirectorWeight of the other interactables.");
            ShrineOfOrderCost = ModCompatibilityConfigPresets.Bind(config, "10 ShrineOfOrder", "Cost", 1, "Lunar coin cost of each Shrine of Order use.");
            ShrineOfOrderUseLimit = ModCompatibilityConfigPresets.Bind(config, "10 ShrineOfOrder", "UseLimit", -1, "Limit the amount of Shrine of Order uses per visit to the Bazaar. -1 = Unlimited.");

            // 11 Donation
            DonateSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "SectionEnabled", true, "Enables or disables the Donate section. Enabling lets the director spawn a Donation Altar near the Newt.");
            DonateDirectorCost = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "DirectorCost", 10, "Director credits the Donation Altar costs.");
            DonateDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "DirectorWeight", 1f, "How likely the director picks the Donation Altar, compared to the DirectorWeight of the other interactables.");
            DonateCost = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "Cost", 5, "Cost of a donation, paid in CostType.");
            DonateCostType = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "CostType", CostTypeIndex.LunarCoin, "What a donation costs, e.g. LunarCoin, Money, WhiteItem or PercentHealth. The price tag shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");
            DonateRewardLimitPerVisit = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardLimitPerVisit", 1, "Limit the number of rewards each player can get per visit to the Bazaar.");
            DonateRewardLimitPerRun = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardLimitPerRun", 1, "Limit the number of rewards each player can get in total over the course of the complete run. Must be equal or higher than RewardLimitPerVisit.");
            DonateSequentialRewardLists = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "SequentialRewardLists", false, "If enabled, the reward lists will be choosen sequentially, ordered from heighest weight to lowest weight. The current reward list is stored for each player per run and swaps around. Reward lists with weight = 0 will be ignored.");
            DonateRewardList1Weight = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardList1Weight", 0f, "Weight for choosing RewardList1.");
            DonateRewardList1 = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardList1", "2xdtChest1 | Tier2", $"Item reward pool. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation} Example: Hoof & Tier1");
            DonateRewardList2Weight = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardList2Weight", 0f, "Weight for choosing RewardList2.");
            DonateRewardList2 = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardList2", "Tier3 | Boss", $"Item reward pool. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation} Example: AlienHead | 2xTier2");
            DonateRewardList3Weight = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardList3Weight", 0f, "Weight for choosing RewardList3.");
            DonateRewardList3 = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardList3", "10xBoostAttackSpeed | 10xBoostDamage | 10xBoostEquipmentRecharge | 10xBoostHp | 10xCrippleWardOnLevel | EmpowerAlways | Ghost | 3xIncubator | 10xLevelBonus | 10xWarCryOnCombat | 10xTempestOnKill", $"Item reward pool. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation}");
            DonateRewardListCharacterWeight = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardListCharacterWeight", 1.0f, "Weight for choosing the reward list of the respective character.");
            string[] survivorNames = SurvivorCatalog.allSurvivorDefs.Select(survivor => survivor.cachedName).ToArray();
            DonateRewardListAvailableCharacters = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardListAvailableCharacters", string.Join(", ", survivorNames), "Available special reward lists for certain characters. Applies only if the donator is a character of the respective type. Comma-separated list of characters (see README.md)");
            DonateRewardListCharacterDefault = ModCompatibilityConfigPresets.Bind(config, "11 Donate", "RewardListCharacterDefault", "3xScrapGreen", $"Item reward pool for characters without a reward list of their own, or whose list gives no reward. Format: <amount1>x<item1> & <amount2>x<item2>, or | to pick one. See README.md for more detailed description. {itemKeyWordsExplanation}");

            // 12 Wandering Chef
            WanderingChefSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "12 WanderingChef", "SectionEnabled", true, "Enables or disables the Wandering Chef section. Only works if the Alloyed Collective is enabled for the run. Enabling lets the director spawn a Wandering Chef near the Lunar Shop.");
            WanderingChefDirectorCost = ModCompatibilityConfigPresets.Bind(config, "12 WanderingChef", "DirectorCost", 7, "Director credits the Wandering Chef costs.");
            WanderingChefDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "12 WanderingChef", "DirectorWeight", 0.5f, "How likely the director picks the Wandering Chef, compared to the DirectorWeight of the other interactables.");
            WanderingChefUnrestrictedCrafting = ModCompatibilityConfigPresets.Bind(config, "12 WanderingChef", "UnrestrictedCrafting", false, "Allows you to craft anything. Otherwise crafting is restricted to a single randomly selected target pickup.");
            WanderingChefCraftableRecipesOnly = ModCompatibilityConfigPresets.Bind(config, "12 WanderingChef", "CraftableRecipesOnly", true, "Only if UnrestrictedCrafting is false: The Wandering Chef only offers a recipe that a player can cook from their inventory and what the Bazaar offers (items in printers, Lunar Buds, Equipment Terminals and cauldrons, scrap if there are Scrappers, and one item from the Command Pedestal's choices). If there is none, he leaves and the director spends his credits on something else.");
            WanderingChefBuyToInventory = ModCompatibilityConfigPresets.Bind(config, "12 WanderingChef", "BuyToInventory", true, "Items go directly into inventory instead of dropping on ground. Cooked equipment replaces yours once your slots are full, and the old one is discarded.");

            // 13 Drones
            DronesSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "SectionEnabled", true, "Enables or disables the Drones section. Only works if the Alloyed Collective is enabled for the run. Enabling lets the director spawn the drone stations below, and the players' drones come along into the Bazaar like on any other stage.");
            DronesDroneCombinerStationDirectorCost = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "DroneCombinerStationDirectorCost", 1, "Director credits the Drone Combiner Station costs.");
            DronesDroneCombinerStationDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "DroneCombinerStationDirectorWeight", 1f, "How likely the director picks the Drone Combiner Station, compared to the DirectorWeight of the other interactables. 0 = never spawns.");
            DronesDroneUpgradeStationDirectorCost = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "DroneUpgradeStationDirectorCost", 5, "Director credits the Drone Upgrade Station costs.");
            DronesDroneUpgradeStationDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "DroneUpgradeStationDirectorWeight", 1f, "How likely the director picks the Drone Upgrade Station, compared to the DirectorWeight of the other interactables. 0 = never spawns.");
            DronesDroneScrapperDirectorCost = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "DroneScrapperDirectorCost", 3, "Director credits the Drone Scrapper costs.");
            DronesDroneScrapperDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "DroneScrapperDirectorWeight", 1f, "How likely the director picks the Drone Scrapper, compared to the DirectorWeight of the other interactables. 0 = never spawns.");
            DronesTripleDroneShopDirectorCost = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopDirectorCost", 8, "Director credits the Triple Drone Shop costs.");
            DronesTripleDroneShopDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopDirectorWeight", 1f, "How likely the director picks the Triple Drone Shop, compared to the DirectorWeight of the other interactables. 0 = never spawns.");
            DronesTripleDroneShopCommonWeight = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopCommonWeight", 0.8f, "How likely the Triple Drone Shop offers common drones, compared to TripleDroneShopUncommonWeight. Unlike on other stages, where the game offers only common drones before the third stage, this applies on every visit.");
            DronesTripleDroneShopUncommonWeight = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopUncommonWeight", 0.2f, "How likely the Triple Drone Shop offers uncommon drones, compared to TripleDroneShopCommonWeight. 0 = never.");
            DronesTripleDroneShopCommonCost = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopCommonCost", 40, "Cost of a common drone from the Triple Drone Shop, paid in TripleDroneShopCostType.");
            DronesTripleDroneShopUncommonCost = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopUncommonCost", 60, "Cost of an uncommon drone from the Triple Drone Shop, paid in TripleDroneShopCostType.");
            DronesTripleDroneShopBuyAll = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopBuyAll", false, "Lets you buy all three drones of the Triple Drone Shop, as with an Executive Card. Off = only one, like in the base game.");
            DronesTripleDroneShopInstancedPurchases = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopInstancedPurchases", true, "Each player can buy from the Triple Drone Shop independently: one drone, or all three with TripleDroneShopBuyAll. It stays full for the others.");
            DronesTripleDroneShopCostType = ModCompatibilityConfigPresets.Bind(config, "13 Drones", "TripleDroneShopCostType", CostTypeIndex.Money, "What a drone from the Triple Drone Shop costs, e.g. Money, LunarCoin, WhiteItem or PercentHealth. The price tag shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");

            // 14 CommandPedestal
            CommandPedestalSectionEnabled = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", "SectionEnabled", true, "Enables or disables the Command Pedestal section. It lets you pick an item for a price.");
            CommandPedestalTiers.Clear();
            // by how many items of each tier a typical stage gives; lunar and void boss items are off
            AddCommandPedestalTier("Tier1", "white items", run => run.availableTier1DropList, 50, 10, 0.5f);
            AddCommandPedestalTier("Tier2", "green items", run => run.availableTier2DropList, 100, 20, 0.25f);
            AddCommandPedestalTier("Tier3", "red items", run => run.availableTier3DropList, 800, 30, 0.05f);
            AddCommandPedestalTier("Boss", "boss items", run => run.availableBossDropList, 400, 20, 0.05f);
            AddCommandPedestalTier("Equipment", "equipment", run => run.availableEquipmentDropList, 100, 10, 0.125f);
            AddCommandPedestalTier("Lunar", "lunar items", run => run.availableLunarItemDropList, 100, 20, 0f);
            AddCommandPedestalTier("VoidTier1", "void white items", run => run.availableVoidTier1DropList, 50, 10, 0.05f);
            AddCommandPedestalTier("VoidTier2", "void green items", run => run.availableVoidTier2DropList, 100, 20, 0.025f);
            AddCommandPedestalTier("VoidTier3", "void red items", run => run.availableVoidTier3DropList, 800, 30, 0.0125f);
            AddCommandPedestalTier("VoidBoss", "void boss items", run => run.availableVoidBossDropList, 400, 20, 0f);
            // all the tiers' director costs, then the pedestal's own weight, then the tiers' weights and prices
            foreach (var tier in CommandPedestalTiers)
            {
                tier.directorCost = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", $"{tier.name}DirectorCost", tier.defaultDirectorCost, $"Director credits a Command Pedestal offering {tier.offers} costs.");
            }
            CommandPedestalDirectorWeight = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", "DirectorWeight", 0.2f, "How likely the director picks the Command Pedestal, compared to the DirectorWeight of the other interactables. Which tier it offers is rolled by the tier weights below.");
            foreach (var tier in CommandPedestalTiers)
            {
                tier.weight = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", $"{tier.name}Weight", tier.defaultWeight, $"Weight for a Command Pedestal offering {tier.offers}, compared to the other tier weights. 0 = never.");
            }
            foreach (var tier in CommandPedestalTiers)
            {
                tier.cost = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", $"{tier.name}Cost", tier.defaultCost, $"Cost of a Command Pedestal offering {tier.offers}, paid in CostType.");
            }
            CommandPedestalCostType = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", "CostType", CostTypeIndex.Money, "What opening the Command Pedestal costs, e.g. Money, LunarCoin, WhiteItem or PercentHealth. The price tag shows it too. Money scales with difficulty if ScaleMoneyCosts is on.");
            CommandPedestalInstancedPurchases = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", "InstancedPurchases", true, "Each player can buy from the Command Pedestal once, independently of the others. It stays ready for the others, and each buyer gets their own Command Essence.");
            CommandPedestalBuyToInventory = ModCompatibilityConfigPresets.Bind(config, "14 CommandPedestal", "BuyToInventory", true, "Using the Command Pedestal shows its item picker right away, and the price is only charged once you pick an item, which goes directly into your inventory. Equipment it replaces goes into the vault, where only you can swap it back, for free. No essence appears; closing the picker costs nothing, and using the Command Pedestal again reopens it. Otherwise buying drops a Command Essence that anyone can use.");

            RefreshDerivedData(config);
            ConfigMigration.Migrate();
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
                ConfigMigration.ReadOldValues(config);
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
            // e.g. an older config file put in place of the current one
            ConfigMigration.Migrate();
        }

        // One tier of Command Pedestal: once the director picks the pedestal, it rolls a tier it can afford by the tier weights
        public class CommandPedestalTier
        {
            public string name;
            public Func<Run, List<PickupIndex>> dropList;
            public ConfigEntry<int> directorCost;
            public ConfigEntry<float> weight;
            public ConfigEntry<int> cost;
            public string offers;
            public int defaultCost;
            public int defaultDirectorCost;
            public float defaultWeight;
        }

        // bound afterwards, kind by kind
        private static void AddCommandPedestalTier(string name, string offers, Func<Run, List<PickupIndex>> dropList, int cost, int directorCost, float weight)
        {
            CommandPedestalTiers.Add(new CommandPedestalTier
            {
                name = name,
                dropList = dropList,
                offers = offers,
                defaultCost = cost,
                defaultDirectorCost = directorCost,
                defaultWeight = weight,
            });
        }

        private static void RefreshDerivedData(ConfigFile config)
        {
            if (EquipmentMaxAmount.Value > 3 && !EquipmentReplaceLunarSeersWithEquipment.Value) EquipmentMaxAmount.Value = 3;
            if (EquipmentMaxAmount.Value > 5 && EquipmentReplaceLunarSeersWithEquipment.Value) EquipmentMaxAmount.Value = 5;
            // Count is not a cost type
            if (EquipmentCostType.Value >= CostTypeIndex.Count) EquipmentCostType.Value = CostTypeIndex.Money;
            if (LunarShopCostType.Value >= CostTypeIndex.Count) LunarShopCostType.Value = CostTypeIndex.LunarCoin;
            if (DonateCostType.Value >= CostTypeIndex.Count) DonateCostType.Value = CostTypeIndex.LunarCoin;
            if (CommandPedestalCostType.Value >= CostTypeIndex.Count) CommandPedestalCostType.Value = CostTypeIndex.Money;
            if (DronesTripleDroneShopCostType.Value >= CostTypeIndex.Count) DronesTripleDroneShopCostType.Value = CostTypeIndex.Money;
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
            if (!reloading && !addingModSurvivors)
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

        // the Donation Altar rewards other mods gave their survivors, see BazaarAPI, by survivor or body name
        private static readonly Dictionary<string, string> modDonationRewards = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public static void SetModDonationReward(string survivorOrBodyName, string rewardList)
        {
            modDonationRewards[survivorOrBodyName] = rewardList;
            // after the game loaded, the settings are there already
            if (DonateRewardListAvailableCharacters != null)
                RefreshDonationRewardLists(DonateRewardListAvailableCharacters.ConfigFile);
        }

        private static void RefreshDonationRewardLists(ConfigFile config)
        {
            DonateRewardListCharacters.Clear();
            var modRewards = new Dictionary<BodyIndex, string>();
            foreach (var (name, reward) in modDonationRewards)
            {
                BodyIndex bodyIndex = CatalogHelper.FindBody(name);
                if (bodyIndex == BodyIndex.None)
                {
                    Log.LogError($"A mod gave a Donation Altar reward to a survivor that doesn't exist: {name}");
                    continue;
                }
                modRewards[bodyIndex] = reward;
            }
            // A mod's survivor joins RewardListAvailableCharacters once, when its reward list is new to the config, e.g.
            // when the config is older than the mod. After that the setting decides, so players can take it out again.
            AddModSurvivors(ModCompatibilityConfigPresets.PlayerEntry(DonateRewardListAvailableCharacters), definition => HasEntry(config, definition));
            // a preset's list, unless the preset sets the survivor's reward list
            if (ModCompatibilityConfigPresets.IsPreset(DonateRewardListAvailableCharacters))
                AddModSurvivors(DonateRewardListAvailableCharacters, definition => ModCompatibilityConfigPresets.IsPreset(config, definition));
            var listed = DonateRewardListAvailableCharacters.Value.Split(",").Select(name => name.Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
            foreach (var name in listed)
            {
                BodyIndex bodyIndex = CatalogHelper.FindBody(name);
                if (bodyIndex == BodyIndex.None)
                {
                    Log.LogError($"RewardListAvailableCharacters: Could not find body or survivor: {name}");
                    continue;
                }
                if (DonateRewardListCharacters.ContainsKey(bodyIndex))
                    continue;
                string defaultReward = modRewards.TryGetValue(bodyIndex, out var modReward) ? modReward : DefaultDonationReward(name);
                ConfigEntry<string> donateRewardListCharacter = ModCompatibilityConfigPresets.Bind(config, "11 Donate", $"RewardList{name}", defaultReward, $"Item reward pool for {name} in the format <amount1>x<item1> & <amount2>x<item2>. See README.md for more detailed description. {itemKeyWordsExplanation}");
                DonateRewardListCharacters[bodyIndex] = donateRewardListCharacter;
            }
        }

        private static void AddModSurvivors(ConfigEntry<string> availableCharacters, Func<ConfigDefinition, bool> hasRewardList)
        {
            var listed = availableCharacters.Value.Split(",").Select(name => name.Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
            var listedBodies = new HashSet<BodyIndex>(listed.Select(CatalogHelper.FindBody));
            var added = new List<string>();
            foreach (var name in modDonationRewards.Keys)
            {
                BodyIndex bodyIndex = CatalogHelper.FindBody(name);
                if (bodyIndex != BodyIndex.None && !listedBodies.Contains(bodyIndex) && !hasRewardList(new ConfigDefinition("11 Donate", $"RewardList{name}")))
                {
                    added.Add(name);
                    listedBodies.Add(bodyIndex);
                }
            }
            if (added.Count == 0)
                return;
            listed.AddRange(added);
            addingModSurvivors = true;
            try
            {
                availableCharacters.Value = string.Join(", ", listed);
            }
            finally
            {
                addingModSurvivors = false;
            }
        }

        // while a mod's survivor is added to RewardListAvailableCharacters, which would refresh the lists again
        private static bool addingModSurvivors;

        // whether the config has the setting, bound or only in the file; BepInEx keeps the latter private
        private static bool HasEntry(ConfigFile config, ConfigDefinition definition)
        {
            if (config.ContainsKey(definition))
                return true;
            var orphaned = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(config) as Dictionary<ConfigDefinition, string>;
            return orphaned != null && orphaned.ContainsKey(definition);
        }

        private static string DefaultDonationReward(string name)
        {
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
            return defaultReward;
        }
    }

}
