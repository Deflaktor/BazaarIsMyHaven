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
        public static ConfigEntry<bool> SpawnCountByStage;
        public static ConfigEntry<int> SpawnCountOffset;
        public static ConfigEntry<bool> AlwaysSpawnShopPortal;
        public static ConfigEntry<bool> DecorateBazaar;
        //public static ConfigEntry<string> HitWords;

        // newt
        public static ConfigEntry<bool> NewtSectionEnabled;
        public static ConfigEntry<bool> NewtGreeting;
        public static ConfigEntry<bool> NewtNoKickFromShop;
        public static ConfigEntry<ShopKeeper.DeathState> NewtDeathBehavior;
        public static ConfigEntry<bool> NewtTrashTalk;

        // printer
        public static ConfigEntry<bool> PrinterSectionEnabled;
        public static ConfigEntry<int> PrinterAmount;
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
        public static ConfigEntry<int> CauldronAmount;
        public static ConfigEntry<float> CauldronWhiteToGreenWeight;
        public static ConfigEntry<float> CauldronGreenToRedWeight;
        public static ConfigEntry<float> CauldronRedToWhiteWeight;
        public static ConfigEntry<float> CauldronRedToYellowWeight;
        public static ConfigEntry<float> CauldronGreenToYellowWeight;
        public static ConfigEntry<float> CauldronGreenToPurpleWeight;
        public static ConfigEntry<float> CauldronRedToPurpleWeight;
        public static ConfigEntry<int> CauldronWhiteToGreenCost;
        public static ConfigEntry<int> CauldronGreenToRedCost;
        public static ConfigEntry<int> CauldronRedToWhiteCost;

        // scrapper
        public static ConfigEntry<bool> ScrapperSectionEnabled;
        public static ConfigEntry<int> ScrapperAmount;

        // equipment
        public static ConfigEntry<bool> EquipmentSectionEnabled;
        public static ConfigEntry<int> EquipmentAmount;
        public static ConfigEntry<bool> EquipmentInstancedPurchases;
        public static ConfigEntry<int> EquipmentCost;
        public static ConfigEntry<bool> EquipmentBuyToInventory;
        public static ConfigEntry<bool> EquipmentReplaceLunarSeersWithEquipment;
        public static ConfigEntry<float> EquipmentReplaceWithEliteChance;
        public static ConfigEntry<string> EquipmentReplaceWithEliteList;

        // lunarShop
        public static ConfigEntry<bool> LunarShopSectionEnabled;
        public static ConfigEntry<bool> LunarShopReplaceLunarBudsWithTerminals;
        public static ConfigEntry<int> LunarShopAmount;
        public static ConfigEntry<int> LunarShopCost;
        public static ConfigEntry<int> LunarShopBuyLimit;
        public static ConfigEntry<bool> LunarShopSequentialItems;
        public static ConfigEntry<string> LunarShopItemList;
        public static ConfigEntry<bool> LunarShopInstancedPurchases;
        public static ConfigEntry<bool> LunarShopBuyToInventory;

        // lunarRecycler
        public static ConfigEntry<bool> LunarRecyclerSectionEnabled;
        public static ConfigEntry<bool> LunarRecyclerAvailable;
        public static ConfigEntry<int> LunarRecyclerCost;
        public static ConfigEntry<int> LunarRecyclerCostMultiplier;
        public static ConfigEntry<int> LunarRecyclerRerollLimit;

        // cleansing pool
        public static ConfigEntry<bool> CleansingPoolSectionEnabled;
        public static ConfigEntry<bool> CleansingPoolRewardsLunarCoins;

        // lunarSeer
        public static ConfigEntry<bool> LunarSeerSectionEnabled;
        public static ConfigEntry<bool> LunarSeerAvailable;
        public static ConfigEntry<int> LunarSeerCost;

        // restack shrine
        public static ConfigEntry<bool> ShrineOfOrderSectionEnabled;
        public static ConfigEntry<int> ShrineOfOrderUseLimit;
        public static ConfigEntry<int> ShrineOfOrderCost;
        public static ConfigEntry<int> ShrineOfOrderCostMultiplier;

        // donate
        public static ConfigEntry<bool> DonateSectionEnabled;
        public static ConfigEntry<int> DonateCost;
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

        public static ConfigEntry<bool> WanderingChefSectionEnabled;
        public static ConfigEntry<bool> WanderingChefSingleRecipe;

        public static void InitConfig(ConfigFile config)
        {
            // 00 General
            EnableMod = config.Bind("00 General", "Enabled", true, "Enable Mod");
            AlwaysSpawnShopPortal = config.Bind("00 General", "AlwaysSpawnShopPortal", false, "Spawn a portal to the Bazaar after each teleporter event.");
            SpawnCountByStage = config.Bind("00 General", "SpawnCountByStage", true, "The amount of spawned interactables are based on stage completion count, capped by the amount.\nFormula: Total = stage count + offset");
            SpawnCountOffset = config.Bind("00 General", "SpawnCountOffset", 0, "Offset value added to stage-based interactable spawns.");
            DecorateBazaar = config.Bind("00 General", "DecorateBazaar", true, "Adds Bazaar decorations such as chests and an extra portal behind Newt.");

            // 01 Newt
            NewtSectionEnabled = config.Bind("01 Newt", "SectionEnabled", true, "Enables or disables the Newt section.");
            NewtGreeting = config.Bind("01 Newt", "Greeting", true, "Enables Newt's welcome messages. If he has been killed before, his tone may be hostile.");
            NewtNoKickFromShop = config.Bind("01 Newt", "NoKickFromShop", true, "Prevents being kicked out of the Bazaar after angering Newt.");
            NewtDeathBehavior = config.Bind("01 Newt", "DeathBehavior", ShopKeeper.DeathState.Tank, "Determines Newt's behavior after death.\nDefault: Vanilla - Unchanged behavior\nTank: Doubles Newt's health after each kill\nGhost: Newt respawns as a passive ghost\nHostile: Newt revives hostile and attacks players");
            NewtTrashTalk = config.Bind("01 Newt", "TrashTalk", true, "When hit, Newt responds with sarcastic remarks.");

            // 02 Printer
            PrinterSectionEnabled = config.Bind("02 Printer", "SectionEnabled", true, "Enables or disables the Printer section.");
            PrinterAmount = config.Bind("02 Printer", "Amount", 9, "Number of printers spawned (max 9)."); if (PrinterAmount.Value > 9) PrinterAmount.Value = 9;
            PrinterTier1Weight = config.Bind("02 Printer", "Tier1Weight", 0.5f, "Weight for Tier 1 (white) items."); PrinterTier1Weight.Value = Math.Abs(PrinterTier1Weight.Value);
            PrinterTier2Weight = config.Bind("02 Printer", "Tier2Weight", 0.25f, "Weight for Tier 2 (green) items."); PrinterTier2Weight.Value = Math.Abs(PrinterTier2Weight.Value);
            PrinterTier3Weight = config.Bind("02 Printer", "Tier3Weight", 0.17f, "Weight for Tier 3 (red) items."); PrinterTier3Weight.Value = Math.Abs(PrinterTier3Weight.Value);
            PrinterTierBossWeight = config.Bind("02 Printer", "BossWeight", 0.04f, "Weight for Boss-tier items."); PrinterTierBossWeight.Value = Math.Abs(PrinterTierBossWeight.Value);
            PrinterTierVoid1Weight = config.Bind("02 Printer", "TierVoid1Weight", 0.015f, "Weight for Tier 1 Void items (traded from red)."); PrinterTierVoid1Weight.Value = Math.Abs(PrinterTierVoid1Weight.Value);
            PrinterTierVoid2Weight = config.Bind("02 Printer", "TierVoid2Weight", 0.015f, "Weight for Tier 2 Void items (traded from red)."); PrinterTierVoid2Weight.Value = Math.Abs(PrinterTierVoid2Weight.Value);
            PrinterTierVoid3Weight = config.Bind("02 Printer", "TierVoid3Weight", 0.006f, "Weight for Tier 3 Void items (traded from red)."); PrinterTierVoid3Weight.Value = Math.Abs(PrinterTierVoid3Weight.Value);
            PrinterTierVoidBossWeight = config.Bind("02 Printer", "TierVoidBossWeight", 0.003f, "Weight for Boss-tier Void items (traded from red)."); PrinterTierVoidBossWeight.Value = Math.Abs(PrinterTierVoidBossWeight.Value);
            PrinterTierVoidAnyWeight = config.Bind("02 Printer", "TierVoidAnyWeight", 0f, "Weight for Void items of any tier (traded from red)."); PrinterTierVoidAnyWeight.Value = Math.Abs(PrinterTierVoidAnyWeight.Value);

            // 03 Cauldron
            CauldronSectionEnabled = config.Bind("03 Cauldron", "SectionEnabled", true, "Enables or disables the Cauldron section.");
            CauldronAmount = config.Bind("03 Cauldron", "Amount", 7, "Number of cauldrons spawned (max 7)."); if (CauldronAmount.Value > 7) CauldronAmount.Value = 7;
            CauldronWhiteToGreenWeight = config.Bind("03 Cauldron", "WhiteToGreenWeight", 0.75f, "Weight for White->Green cauldrons."); CauldronWhiteToGreenWeight.Value = Math.Abs(CauldronWhiteToGreenWeight.Value);
            CauldronWhiteToGreenCost = config.Bind("03 Cauldron", "WhiteToGreenCost", 3, "Number of White items required for Green conversion."); CauldronWhiteToGreenCost.Value = Math.Abs(CauldronWhiteToGreenCost.Value);
            CauldronGreenToRedWeight = config.Bind("03 Cauldron", "GreenToRedWeight", 0.25f, "Weight for Green->Red cauldrons."); CauldronGreenToRedWeight.Value = Math.Abs(CauldronGreenToRedWeight.Value);
            CauldronGreenToRedCost = config.Bind("03 Cauldron", "GreenToRedCost", 5, "Number of Green items required for Red conversion."); CauldronGreenToRedCost.Value = Math.Abs(CauldronGreenToRedCost.Value);
            CauldronRedToWhiteWeight = config.Bind("03 Cauldron", "RedToWhiteWeight", 0f, "Weight for Red->White cauldrons."); CauldronRedToWhiteWeight.Value = Math.Abs(CauldronRedToWhiteWeight.Value);
            CauldronRedToWhiteCost = config.Bind("03 Cauldron", "RedToWhiteCost", 1, "Number of Red items required for White conversion."); CauldronRedToWhiteCost.Value = Math.Abs(CauldronRedToWhiteCost.Value);
            // TODO
            //CauldronGreenToYellowWeight = config.Bind("03 Cauldron", "CauldronGreenToYellowWeight", 0.33f, "Spawn weight for Yellow cauldrons (uses Green items as cost)."); CauldronGreenToYellowWeight.Value = Math.Abs(CauldronGreenToYellowWeight.Value);
            //CauldronGreenToPurpleWeight = config.Bind("03 Cauldron", "CauldronGreenToPurpleWeight", 0.33f, "Spawn weight for Purple cauldrons (uses Green items as cost)."); CauldronGreenToPurpleWeight.Value = Math.Abs(CauldronGreenToPurpleWeight.Value);

            // 04 Scrapper
            ScrapperSectionEnabled = config.Bind("04 Scrapper", "SectionEnabled", true, "Enables or disables the Scrapper section.");
            ScrapperAmount = config.Bind("04 Scrapper", "Amount", 4, "Number of Scrappers spawned (max 4)."); if (ScrapperAmount.Value > 4) ScrapperAmount.Value = 4;

            // 05 Equipment
            EquipmentSectionEnabled = config.Bind("05 Equipment", "SectionEnabled", true, "Enables or disables the Equipment section.");
            EquipmentAmount = config.Bind("05 Equipment", "Amount", 3, "Number of Equipment Terminals (max 3 normally, 5 if replacing Lunar Seers).");
            EquipmentReplaceLunarSeersWithEquipment = config.Bind("05 Equipment", "ReplaceLunarSeersWithEquipment", false, "Replaces Lunar Seers with Equipment Terminals (increases equipment max to 5). Makes the Lunar Seer section irrelevant.");
            EquipmentReplaceWithEliteChance = config.Bind("05 Equipment", "ReplaceWithEliteChance", 0.15f, "Chance for replacing the equipment with an elite equipment."); EquipmentReplaceWithEliteChance.Value = Math.Abs(EquipmentReplaceWithEliteChance.Value);
            EquipmentReplaceWithEliteList = config.Bind("05 Equipment", "ReplaceWithEliteList", "EliteEarthEquipment | EliteFireEquipment | EliteIceEquipment | EliteLunarEquipment | EliteLightningEquipment", "With which elite equipments to replace. List in the format equipment1 | equipment2. Can use:\\n- internal item names (see https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/)\\n- item tier keywords ({itemTiersString})\\n- droptable names (see README.md). Follows ItemStringParser format.");
            EquipmentInstancedPurchases = config.Bind("05 Equipment", "InstancedPurchases", true, "Each player can purchase equipment independently.");
            EquipmentCost = config.Bind("05 Equipment", "Cost", 0, "Monetary cost for equipment purchases."); EquipmentCost.Value = Math.Abs(EquipmentCost.Value);
            EquipmentBuyToInventory = config.Bind("05 Equipment", "BuyToInventory", true, "Purchased equipment goes directly into inventory instead of dropping to the ground.");
            if (EquipmentAmount.Value > 3 && !EquipmentReplaceLunarSeersWithEquipment.Value) EquipmentAmount.Value = 3;
            if (EquipmentAmount.Value > 5 && EquipmentReplaceLunarSeersWithEquipment.Value) EquipmentAmount.Value = 5;

            // 06 LunarShop
            LunarShopSectionEnabled = config.Bind("06 LunarShop", "SectionEnabled", true, "Enables or disables the Lunar Shop section.");
            LunarShopReplaceLunarBudsWithTerminals = config.Bind("06 LunarShop", "ReplaceLunarBudsWithTerminals", true, "Required so that Amount works. Otherwise it will always be 5 Lunar Buds. Looks also better in combination with InstancedPurchases.");
            LunarShopAmount = config.Bind("06 LunarShop", "Amount", 5, "Number of Lunar Shop Terminals (max 20)."); if (LunarShopAmount.Value > 20) LunarShopAmount.Value = 20;
            LunarShopCost = config.Bind("06 LunarShop", "Cost", 1, "Lunar coin cost per Lunar Shop Terminal or Lunar Bud use."); LunarShopCost.Value = Math.Abs(LunarShopCost.Value);
            LunarShopBuyLimit = config.Bind("06 LunarShop", "BuyLimit", 5, "Limit on Lunar Shop purchases each player can make per visit to the Bazaar. -1 = Unlimited.");
            LunarShopSequentialItems = config.Bind("06 LunarShop", "SequentialItems", false, "Picks items sequentially from the list instead of randomly.");
            var items = "Tonic | AutoCastEquipment | RandomDamageZone | LunarDagger | HalfSpeedDoubleHealth | ShieldOnly | ShieldOnly | ShieldOnly | LunarPrimaryReplacement | LunarSecondaryReplacement | LunarSpecialReplacement | LunarBadLuck | LunarBadLuck | LunarBadLuck | HalfAttackSpeedHalfCooldowns | HalfAttackSpeedHalfCooldowns | LunarSun";
            var itemTiersString = "Tier1 | Tier2 | Tier3 | Lunar | Boss | VoidTier1 | VoidTier2 | VoidTier3 | VoidBoss | FoodTier";
            var itemKeyWordsExplanation = $"Can use:\n - internal item names(see https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/)\n- item tier keywords ({itemTiersString})\n- droptable names (see README.md)\nFollows ItemStringParser format.";
            LunarShopItemList = config.Bind("06 LunarShop", "ItemList", "dtLunarChest", $"List of items available in Lunar Shop. {itemKeyWordsExplanation} Example: {items}");
            LunarShopInstancedPurchases = config.Bind("06 LunarShop", "InstancedPurchases", true, "Each player can buy independently from Lunar Shop.");
            LunarShopBuyToInventory = config.Bind("06 LunarShop", "BuyToInventory", true, "Items go directly into inventory instead of dropping on ground.");

            // 07 LunarRecycler
            LunarRecyclerSectionEnabled = config.Bind("07 LunarRecycler", "SectionEnabled", true, "Enables or disables the Lunar Recycler section.");
            LunarRecyclerAvailable = config.Bind("07 LunarRecycler", "Available", true, "If enabled, a Lunar Recycler is available in the Bazaar. Otherwise it will get removed.");
            LunarRecyclerRerollLimit = config.Bind("07 LunarRecycler", "RerollLimit", 3, "Limit the amount of rerolls allowed per visit to the Bazaar. -1 = Unlimited.");
            LunarRecyclerCost = config.Bind("07 LunarRecycler", "Cost", 1, "Initial lunar coin cost to reroll."); LunarRecyclerCost.Value = Math.Abs(LunarRecyclerCost.Value);
            LunarRecyclerCostMultiplier = config.Bind("07 LunarRecycler", "CostMultiplier", 2, "Cost multiplier applied after each reroll use."); LunarRecyclerCostMultiplier.Value = Math.Abs(LunarRecyclerCostMultiplier.Value);

            // 08 CleansingPool
            CleansingPoolSectionEnabled = config.Bind("08 CleansingPool", "SectionEnabled", true, "Enables or disables the Cleansing Pool section.");
            CleansingPoolRewardsLunarCoins = config.Bind("08 CleansingPool", "RewardsLunarCoin", true, "Cleansing Pools reward a Lunar Coin instead of a Pearl.");

            // 09 LunarSeer
            LunarSeerSectionEnabled = config.Bind("09 LunarSeer", "SectionEnabled", true, "Enables or disables the Lunar Seer Station section.");
            LunarSeerAvailable = config.Bind("09 LunarSeer", "Available", true, "If enabled, Seer Stations appear in the Bazaar.");
            LunarSeerCost = config.Bind("09 LunarSeer", "Cost", 3, "Lunar coin cost for using Seer Stations."); LunarSeerCost.Value = Math.Abs(LunarSeerCost.Value);

            // 10 ShrineOfOrder
            ShrineOfOrderSectionEnabled = config.Bind("10 ShrineOfOrder", "SectionEnabled", true, "Enables or disables the Shrine of Order section. Enabling spawns a Shrine of Order near the Newt.");
            ShrineOfOrderUseLimit = config.Bind("10 ShrineOfOrder", "UseLimit", -1, "Limit the amount of Shrine of Order uses per visit to the Bazaar. -1 = Unlimited.");
            ShrineOfOrderCost = config.Bind("10 ShrineOfOrder", "Cost", 1, "Initial lunar coin cost for Shrine of Order."); ShrineOfOrderCost.Value = Math.Abs(ShrineOfOrderCost.Value);
            ShrineOfOrderCostMultiplier = config.Bind("10 ShrineOfOrder", "CostMultiplier", 1, "Cost multiplier applied after each Shrine of Order use."); ShrineOfOrderCostMultiplier.Value = Math.Abs(ShrineOfOrderCostMultiplier.Value);

            // 11 Donation
            DonateSectionEnabled = config.Bind("11 Donate", "SectionEnabled", true, "Enables or disables the Donate section. Enabling spawns a Donation Altar near the Newt.");
            DonateRewardLimitPerVisit = config.Bind("11 Donate", "RewardLimitPerVisit", 1, "Limit the number of rewards each player can get per visit to the Bazaar."); DonateRewardLimitPerVisit.Value = Math.Abs(DonateRewardLimitPerVisit.Value);
            DonateRewardLimitPerRun = config.Bind("11 Donate", "RewardLimitPerRun", 1, "Limit the number of rewards each player can get in total over the course of the complete run. Must be equal or higher than RewardLimitPerVisit."); DonateRewardLimitPerRun.Value = Math.Abs(DonateRewardLimitPerRun.Value);
            DonateCost = config.Bind("11 Donate", "Cost", 5, "Lunar coin cost for reward."); DonateCost.Value = Math.Abs(DonateCost.Value);
            DonateSequentialRewardLists = config.Bind("11 Donate", "SequentialRewardLists", false, "If enabled, the reward lists will be choosen sequentially, ordered from heighest weight to lowest weight. The current reward list is stored for each player per run and swaps around. Reward lists with weight = 0 will be ignored.");
            DonateRewardList1Weight = config.Bind("11 Donate", "RewardList1Weight", 0.60f, "Weight for choosing RewardList1."); DonateRewardList1Weight.Value = Math.Abs(DonateRewardList1Weight.Value);
            DonateRewardList1 = config.Bind("11 Donate", "RewardList1", "2xdtChest1 | Tier2", $"Item reward pool. Comma-separated list in the format keyword=amount for rewarding multiple of the item, or just the keyword for single reward. {itemKeyWordsExplanation} Example: Hoof & Tier1");
            DonateRewardList2Weight = config.Bind("11 Donate", "RewardList2Weight", 0.10f, "Weight for choosing RewardList2."); DonateRewardList2Weight.Value = Math.Abs(DonateRewardList2Weight.Value);
            DonateRewardList2 = config.Bind("11 Donate", "RewardList2", "Tier3 | Boss", $"Item reward pool. Comma-separated list in the format keyword=amount for rewarding multiple of the item, or just the keyword for single reward. {itemKeyWordsExplanation} Example: AlienHead | 2xTier2");
            DonateRewardList3Weight = config.Bind("11 Donate", "RewardList3Weight", 0f, "Weight for choosing RewardList3."); DonateRewardList3Weight.Value = Math.Abs(DonateRewardList3Weight.Value);
            DonateRewardList3 = config.Bind("11 Donate", "RewardList3", "10xBoostAttackSpeed | 10xBoostDamage | 10xBoostEquipmentRecharge | 10xBoostHp | BurnNearby | 10xCrippleWardOnLevel | EmpowerAlways | Ghost | 3xIncubator | 10xLevelBonus | 10xWarCryOnCombat | 10xTempestOnKill", $"Item reward pool. Comma-separated list in the format keyword=amount for rewarding multiple of the item, or just the keyword for single reward. {itemKeyWordsExplanation}");

            DonateRewardListCharacterWeight = config.Bind("11 Donate", "RewardListCharacterWeight", 0.30f, "Weight for choosing the reward list of the respective character."); DonateRewardListCharacterWeight.Value = Math.Abs(DonateRewardListCharacterWeight.Value);
            DonateRewardListCharacterDefault = config.Bind("11 Donate", "RewardListCharacterDefault", "3xScrapGreen", $"Item reward pool if the donating character does not have a reward list specified. Comma-separated list in the format keyword=amount for rewarding multiple of the item, or just the keyword for single reward. {itemKeyWordsExplanation}");
            string[] survivorNames = SurvivorCatalog.allSurvivorDefs.Select(survivor => survivor.cachedName).ToArray();
            // string[] bodyNames = BodyCatalog.allBodyPrefabs.Select(prefab => prefab.name).ToArray();
            DonateRewardListAvailableCharacters = config.Bind("11 Donate", "RewardListAvailableCharacters", string.Join(", ", survivorNames), "Available special reward lists for certain characters. Applies only if the donator is a character of the respective type. Comma-separated list of characters (see README.md)");
            
            DonateRewardListCharacters.Clear();
            string[] availableCharacters = DonateRewardListAvailableCharacters.Value.Split(",");
            foreach (string availableCharacter in availableCharacters)
            {
                var name = availableCharacter.Trim();
                if (name.IsNullOrWhiteSpace())
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
                    default:
                        defaultReward = "3xScrapGreen";
                        break;
                }   
                ConfigEntry<string> donateRewardListCharacter = config.Bind("11 Donate", $"RewardList{name}", defaultReward, $"Item reward pool for {name} in the format <amount1>x<item1> & <amount2>x<item2>. See README.md for more detailed description. {itemKeyWordsExplanation}");
                DonateRewardListCharacters.Add(bodyIndex, donateRewardListCharacter);
            }

            // 12 Wandering Chef
            WanderingChefSectionEnabled = config.Bind("12 WanderingChef", "SectionEnabled", true, "Enables or disables the Wandering Chef section. Enabling spawns a Wandering Chef near the Lunar Shop.");
            WanderingChefSingleRecipe = config.Bind("12 WanderingChef", "SingleRecipe", true, "Limits the Wandering Chef inside the Bazaar to allow cooking only a single randomly selected target item type.");

            if (ModCompatibilityInLobbyConfig.enabled)
            {
                ModCompatibilityInLobbyConfig.CreateFromBepInExConfigFile(config, Main.PluginName);
            }
        }
    }

}
