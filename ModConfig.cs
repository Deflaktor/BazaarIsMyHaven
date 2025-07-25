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
        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> EnableAutoOpenShop;
        public static ConfigEntry<bool> EnableNoKickFromShop;
        public static ConfigEntry<bool> EnableNewtNoDie;
        public static ConfigEntry<ShopKeep.DeathState> NewtSecondLifeMode;
        //public static ConfigEntry<bool> EnableNewtNoDie_HP;
        public static ConfigEntry<bool> EnableWelcomeWord;
        public static ConfigEntry<bool> SpawnCountByStage;
        public static ConfigEntry<int> SpawnCountOffset;
        public static ConfigEntry<int> PenaltyCoefficient;
        public static int PenaltyCoefficient_Temp;

        public static ConfigEntry<int> PrinterCount;
        public static ConfigEntry<int> CauldronCount;
        public static ConfigEntry<int> ScrapperCount;
        public static ConfigEntry<int> EquipmentCount;
        public static ConfigEntry<int> LunarShopTerminalCount;
        public static ConfigEntry<int> RewardCount;

        public static ConfigEntry<float> PrinterTier1Weight;
        public static ConfigEntry<float> PrinterTier2Weight;
        public static ConfigEntry<float> PrinterTier3Weight;
        public static ConfigEntry<float> PrinterTierBossWeight;
        public static ConfigEntry<float> PrinterTierLunarWeight;
        public static ConfigEntry<float> PrinterTierVoidWeight;

        public static ConfigEntry<float> CauldronWhiteWeight;
        public static ConfigEntry<float> CauldronGreenWeight;
        public static ConfigEntry<float> CauldronRedWeight;
        public static ConfigEntry<bool> EnableCauldronHacking;
        public static ConfigEntry<int> CauldronWhiteToGreenCost;
        public static ConfigEntry<int> CauldronGreenToRedCost;
        public static ConfigEntry<int> CauldronRedToWhiteCost;
        public static ConfigEntry<int> CauldronWhiteToGreenCost_Hacked;
        public static ConfigEntry<int> CauldronGreenToRedCost_Hacked;
        public static ConfigEntry<int> CauldronRedToWhiteCost_Hacked;
        public static ConfigEntry<bool> CauldronWhiteCostTypeChange;
        public static ConfigEntry<float> CauldronWhiteHackedChance;
        public static ConfigEntry<float> CauldronGreenHackedChance;
        public static ConfigEntry<float> CauldronRedHackedChance;
        public static ConfigEntry<float> CauldronYellowWeight;
        public static ConfigEntry<float> CauldronBlueWeight;
        public static ConfigEntry<float> CauldronPurpleWeight;

        public static ConfigEntry<bool> EnableShrineCleanse;
        public static ConfigEntry<bool> EnableShrineHealing;

        public static ConfigEntry<bool> EnableShrineRestack;
        public static ConfigEntry<int> ShrineRestackMaxCount;
        public static ConfigEntry<int> ShrineRestackCost;
        public static ConfigEntry<int> ShrineRestackScalar;

        public static ConfigEntry<int> PrayCost;
        public static ConfigEntry<float> PrayNormalWeight;
        public static ConfigEntry<float> PrayEliteWeight;
        public static ConfigEntry<float> PrayPeculiarWeight;
        public static ConfigEntry<string> PrayPeculiarList;

        public static ConfigEntry<bool> EnableDecorate;

        public static ConfigEntry<bool> EnableLunarRecyclerInjection;
        public static ConfigEntry<bool> LunarRecyclerAvailable;
        public static ConfigEntry<int> LunarRecyclerRerollCost;
        public static ConfigEntry<int> LunarRecyclerRerollScalar;
        public static ConfigEntry<int> LunarRecyclerRerollCount;
        public static int RerolledCount;

        public static ConfigEntry<bool> EnableLunarShopTerminalInjection;
        public static ConfigEntry<int> LunarShopTerminalCost;
        public static ConfigEntry<bool> EnableLunarShopStaticItems;
        public static ConfigEntry<string> LunarShopTerminal1Item;
        public static ConfigEntry<string> LunarShopTerminal2Item;
        public static ConfigEntry<string> LunarShopTerminal3Item;
        public static ConfigEntry<string> LunarShopTerminal4Item;
        public static ConfigEntry<string> LunarShopTerminal5Item;
        public static ConfigEntry<string> LunarShopTerminal6Item;
        public static ConfigEntry<string> LunarShopTerminal7Item;
        public static ConfigEntry<string> LunarShopTerminal8Item;
        public static ConfigEntry<string> LunarShopTerminal9Item;
        public static ConfigEntry<string> LunarShopTerminal10Item;
        public static ConfigEntry<string> LunarShopTerminal11Item;
        public static ConfigEntry<string> LunarShopTerminal12Item;
        public static ConfigEntry<string> LunarShopTerminal13Item;
        public static ConfigEntry<string> LunarShopTerminal14Item;
        public static ConfigEntry<string> LunarShopTerminal15Item;
        public static ConfigEntry<string> LunarShopTerminal16Item;

        public static ConfigEntry<bool> EnableSeerStationsInjection;
        public static ConfigEntry<bool> SeerStationAvailable;
        public static ConfigEntry<int> SeerStationsCost;

        public static ConfigEntry<bool> EnableLines;
        //public static ConfigEntry<string> HitWords;

        public static void InitConfig(ConfigFile config)
        {
            EnableMod = config.Bind("00 Setting设置", "Enabled", true, "Enable Mod\n启用模组");
            if (EnableMod.Value)
            {
                EnableAutoOpenShop = config.Bind("00 Setting设置", "EnableAutoOpenShop", true, "Enable auto open shop portal. \n启用自动开商店传送门");
                EnableNoKickFromShop = config.Bind("00 Setting设置", "EnableNoKickFromShop", true, "Enable no shoper kick out.\n启用商店防踢");
                EnableNewtNoDie = config.Bind("00 Setting设置", "EnableShoperWontDie", false, "Enable Newt will no longer die, maybe...you will die instead.\n启用商人没那么容易死，如果开了注意安全");
                if (EnableNewtNoDie.Value)
                {
                    NewtSecondLifeMode = config.Bind("00 Setting设置", "NewtSecondLifeMode", ShopKeep.DeathState.Evil, "Enable Newt will no longer die, maybe...you will die instead.\n商人死亡复活后的形态, Tank = 纯肉, Eveil = 恶魔"); 
                }
                EnableWelcomeWord = config.Bind("00 Setting设置", "EnableWelcomeWord", true, "Enable welcome words.\n启用商人欢迎语");
                PenaltyCoefficient = config.Bind("00 Setting设置", "PenaltyCoefficient", 10, "If Newt is die, there is a price penalty.\n商人死亡后所有价格、兑换惩罚系数"); PenaltyCoefficient.Value = Math.Abs(PenaltyCoefficient.Value);
                if (ShopKeep.IsDeath) PenaltyCoefficient_Temp = PenaltyCoefficient.Value;
                else PenaltyCoefficient_Temp = 1;
                SpawnCountByStage = config.Bind("00 Setting设置", "SpawnCountByStage", true, "Increase the number of devices based on stage. true = Yes\n是否随关卡数增加所有设备数量? true = 是\nFormula公式：Total Qty = stage + offset");
                if (SpawnCountByStage.Value)
                {
                    SpawnCountOffset = config.Bind("00 Setting设置", "SpawnCountOffset", 0, "Associate the above parameters, value of offset.\n关联上面参数，增加设备数量的位移数");
                }

                PrinterCount = config.Bind("01 Printer打印机", "PrinterCount", 9, "Total generated value of printers, max is 9, below zero is not enabled.\n月店3D打印机的数量，最多9台，小于0不启用"); if (PrinterCount.Value > 9) PrinterCount.Value = 9;
                if (PrinterCount.Value > 0)
                {
                    PrinterTier1Weight = config.Bind("01 Printer打印机", "PrinterTier1Weight", 0.8f, "Weight of white items. \n打印机出现白色物品的比重"); PrinterTier1Weight.Value = Math.Abs(PrinterTier1Weight.Value);
                    PrinterTier2Weight = config.Bind("01 Printer打印机", "PrinterTier2Weight", 0.2f, "Weight of green items. \n打印机出现绿色物品的比重"); PrinterTier2Weight.Value = Math.Abs(PrinterTier2Weight.Value);
                    PrinterTier3Weight = config.Bind("01 Printer打印机", "PrinterTier3Weight", 0.01f, "Weight of red items. \n打印机出现红色物品的比重"); PrinterTier3Weight.Value = Math.Abs(PrinterTier3Weight.Value);
                    PrinterTierBossWeight = config.Bind("01 Printer打印机", "PrinterBossWeight", 0.01f, "Weight of boss items. \n打印机出现黄色物品的比重"); PrinterTierBossWeight.Value = Math.Abs(PrinterTierBossWeight.Value);

                    PrinterTierLunarWeight = config.Bind("01 Printer打印机", "PrinterTierLunarHackChance", 0.05f, "Weight of blue items, exchange blue for blue.\n打印机出现蓝色物品的比重"); PrinterTierLunarWeight.Value = Math.Abs(PrinterTierLunarWeight.Value);
                    PrinterTierVoidWeight = config.Bind("01 Printer打印机", "PrinterTierVoidHackChance", 0.05f, "Weight of purple items, exchange red for purple. \n打印机出现紫色物品的比重，用红色物品打印"); PrinterTierVoidWeight.Value = Math.Abs(PrinterTierVoidWeight.Value);
                }

                CauldronCount = config.Bind("02 Cauldron大锅", "CauldronCount", 7, "Total generated value of cauldrons, max is 7, below zero is not enabled. \n月店大锅的数量，最多7锅，小于0不启用"); if (CauldronCount.Value > 7) CauldronCount.Value = 7;
                if (CauldronCount.Value > 0)
                {
                    CauldronWhiteWeight = config.Bind("02 Cauldron大锅", "CauldronWhiteWeight", 0.3f, "Weight of white cauldron. \n大锅出现白色物品的比重"); CauldronWhiteWeight.Value = Math.Abs(CauldronWhiteWeight.Value);
                    CauldronGreenWeight = config.Bind("02 Cauldron大锅", "CauldronGreenWeight", 0.6f, "Weight of green cauldron. \n大锅出现绿色物品的比重"); CauldronGreenWeight.Value = Math.Abs(CauldronGreenWeight.Value);
                    CauldronRedWeight = config.Bind("02 Cauldron大锅", "CauldronRedWeight", 0.1f, "Weight of red cauldron. \n大锅出现红色物品的比重"); CauldronRedWeight.Value = Math.Abs(CauldronRedWeight.Value);

                    EnableCauldronHacking = config.Bind("02.1 Cauldron Hack大锅数据修改", "EnableCauldronHacking", true, "Enable cauldron data hacking.\n启用大锅数据修改");
                    if (EnableCauldronHacking.Value)
                    {
                        CauldronWhiteToGreenCost = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronWhiteToGreenCost", 3, "Green cauldron requires the number of white items.\n绿锅需要白色物品的数量"); CauldronWhiteToGreenCost.Value = Math.Abs(CauldronWhiteToGreenCost.Value);
                        CauldronGreenToRedCost = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronGreenToRedCost", 5, "Red cauldron requires the number of green items.\n红锅需要绿色物品的数量"); CauldronGreenToRedCost.Value = Math.Abs(CauldronGreenToRedCost.Value);
                        CauldronRedToWhiteCost = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronRedToWhiteCost", 1, "White cauldron requires the number of red items.\n白锅需要红色物品的数量"); CauldronRedToWhiteCost.Value = Math.Abs(CauldronRedToWhiteCost.Value);
                        CauldronWhiteCostTypeChange = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronWhiteCostTypeChange", true, "Enable white cauldron change the cost type demand for red to green.\n白锅把需求红色物品改成绿色物品，默认1红3白太亏了");
                        CauldronWhiteHackedChance = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronWhiteHackedChance", 0.2f, "Probability of a white cauldron being hacked. After being hacked, exchange with the same color, yellow to yellow, blue to blue. Except for purple exchange with red. \n白色大锅被黑的机率，被黑后用同等颜色兑换，黄换黄，蓝换蓝，而紫色用红色兑换"); CauldronWhiteHackedChance.Value = Math.Abs(CauldronWhiteHackedChance.Value);
                        CauldronGreenHackedChance = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronGreenHackedChance", 0.2f, "Probability of a green cauldron being hacked. After being hacked, still use white items.\n绿色大锅被黑的机率，被黑后仍然使用白色物品兑换。"); CauldronGreenHackedChance.Value = Math.Abs(CauldronGreenHackedChance.Value);
                        CauldronRedHackedChance = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronRedHackedChance", 0.2f, "Probability of a red cauldron being hacked. After being hacked, still use green items.\n红色大锅被黑的机率，被黑后仍然使用绿色物品交换"); CauldronRedHackedChance.Value = Math.Abs(CauldronRedHackedChance.Value);
                        CauldronYellowWeight = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronYellowWeight", 0.33f, "(After being hacked)Weight of yellow items. \n(被黑后)大锅出现黄色物品的比重"); CauldronYellowWeight.Value = Math.Abs(CauldronYellowWeight.Value);
                        CauldronBlueWeight = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronBlueWeight", 0.33f, "(After being hacked)Weight of blue items.\n(被黑后)大锅出现蓝色物品的比重"); CauldronBlueWeight.Value = Math.Abs(CauldronBlueWeight.Value);
                        CauldronPurpleWeight = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronPurpleWeight", 0.33f, "(After being hacked)Weight of purple items, yep, that's right, still use the red item exchange.\n(被黑后)大锅出现紫色物品的比重，用红色兑换"); CauldronPurpleWeight.Value = Math.Abs(CauldronPurpleWeight.Value); 
                        CauldronWhiteToGreenCost_Hacked = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronWhiteToGreenCost_Hacked", 3, "(After being hacked)Green cauldron requires the number of white items.\n(被黑后)绿锅需要物品数量"); CauldronWhiteToGreenCost_Hacked.Value = Math.Abs(CauldronWhiteToGreenCost_Hacked.Value);
                        CauldronGreenToRedCost_Hacked = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronGreenToRedCost_Hacked", 5, "(After being hacked)Red cauldron requires the number of green items.\n(被黑后)红锅需要物品数量"); CauldronGreenToRedCost_Hacked.Value = Math.Abs(CauldronGreenToRedCost_Hacked.Value);
                        CauldronRedToWhiteCost_Hacked = config.Bind("02.1 Cauldron Hack大锅数据修改", "CauldronRedToWhiteCost_Hacked", 4, "(After being hacked)White cauldron requires the number of red items.\n(被黑后)白锅需要物品数量，为什么是4，因为白色大锅原本掉3件物品，为了平衡，被黑后会改成使用相同颜色装备兑换，紫色除外用红色兑换"); CauldronRedToWhiteCost_Hacked.Value = Math.Abs(CauldronRedToWhiteCost_Hacked.Value);
                    }
                }

                ScrapperCount = config.Bind("03 Scrapper收割机", "ScrapperCount", 4, "Total generated value of scrappers, max is 4, below zero is not enabled. \n月店收割机的数量，最多4台，小于0不启用"); if (ScrapperCount.Value > 4) ScrapperCount.Value = 4;

                EquipmentCount = config.Bind("04 Equipment主动装备", "EquipmentCount", 6, "Total generated value of equipments, max is 6, below zero is not enabled. \n月店主动装备的数量，最多6台，小于0不启用"); if (EquipmentCount.Value > 6) EquipmentCount.Value = 6;

                LunarShopTerminalCount = config.Bind("05 Lunar月球装备", "LunarShopTerminalCount", 15, "Total generated value of LunarShopTerminal, max is 15, below zero is not enabled. \n月店月球装备的数量，最多11个，包括原有的5个，小于0不启用"); if (LunarShopTerminalCount.Value > 15) LunarShopTerminalCount.Value = 15;
                EnableLunarShopTerminalInjection = config.Bind("05 Lunar月球装备", "EnableLunarShopTerminalInjection", true, "Enable LunarShopTerminal data modification.\n启用月球装备修改");
                if (EnableLunarShopTerminalInjection.Value)
                {
                    LunarShopTerminalCost = config.Bind("05 Lunar月球装备", "LunarShopTerminalCost", 2, "Price of Lunar\n月球装备价格"); LunarShopTerminalCost.Value = Math.Abs(LunarShopTerminalCost.Value);
                }
                EnableLunarShopStaticItems = config.Bind("05 Lunar月球装备", "EnableLunarShopStaticItems", true, "Enable LunarShop static items (non-randomized).");
                if (EnableLunarShopStaticItems.Value)
                {
                    LunarShopTerminal1Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem1", "LunarPrimaryReplacement", "LunarShop static item 1");
                    LunarShopTerminal2Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem2", "LunarSecondaryReplacement", "LunarShop static item 2");
                    LunarShopTerminal3Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem3", "LunarSpecialReplacement", "LunarShop static item 3");
                    LunarShopTerminal4Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem4", "AutoCastEquipment", "LunarShop static item 4");
                    LunarShopTerminal5Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem5", "LunarDagger", "LunarShop static item 5");
                    LunarShopTerminal6Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem6", "HalfSpeedDoubleHealth", "LunarShop static item 6");
                    LunarShopTerminal7Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem7", "LunarSun", "LunarShop static item 7");
                    LunarShopTerminal8Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem8", "LunarBadLuck", "LunarShop static item 8");
                    LunarShopTerminal9Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem9", "LunarBadLuck", "LunarShop static item 9");
                    LunarShopTerminal10Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem10", "LunarBadLuck", "LunarShop static item 10");
                    LunarShopTerminal11Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem11", "ShieldOnly", "LunarShop static item 11");
                    LunarShopTerminal12Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem12", "ShieldOnly", "LunarShop static item 12");
                    LunarShopTerminal13Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem13", "ShieldOnly", "LunarShop static item 13");
                    LunarShopTerminal14Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem14", "Light Flux Pauldron", "LunarShop static item 14");
                    LunarShopTerminal15Item = config.Bind("05 Lunar月球装备", "LunarShopStaticItem15", "Light Flux Pauldron", "LunarShop static item 15");

                }

                EnableShrineRestack = config.Bind("06 ShrineRestack跌序", "EnableShrineRestack", true, "Enable shrinerestack.\n启用跌序");
                if (EnableShrineRestack.Value)
                {
                    ShrineRestackMaxCount = config.Bind("06 ShrineRestack跌序", "ShrineRestackMaxCount", 99, "The value of total restack times. Download this mod [BazaarLunarForEveryOne] can be limited to once per person.\n跌序可以次数，搭配BazaarForEveryOne模组可以限制只赌博一次");
                    ShrineRestackCost = config.Bind("06 ShrineRestack跌序", "ShrineRestackCost", 1, "Shrinerestack starting lunar coins cost.\n跌序起步价");
                    ShrineRestackScalar = config.Bind("06 ShrineRestack跌序", "ShrineRestackScalar", 2, "Multiple increases in lunar coins cost.\n跌序价格增长倍数");
                }

                EnableShrineCleanse = config.Bind("07 CleanPool净化池", "EnableShrineCleanse", true, "Enable clean pool.(It's over Newt's head.)\n启用净化池，在纽特头上面");

                EnableShrineHealing = config.Bind("08 Pray祈祷", "EnablePray", true, "Enable pray.(The model is Shrinehealing)\n启用祈祷，模型是那个树灵神龛。");
                if (EnableShrineHealing.Value)
                {
                    RewardCount = config.Bind("08 Pray祈祷", "RewardCount", 3, "Pray to the Shrinehealing, get a reward for every 10 prayers.\n祈祷获得物品的次数，10次祈祷获得一次");
                    PrayCost = config.Bind("08 Pray祈祷", "PrayCost", 10, "How many lunar coins does it take to pray once.\n祈祷一次需要多少月币");
                    PrayNormalWeight = config.Bind("08 Pray祈祷", "PrayNormalWeight", 0.5f, "Weight of normal items, like white, green, red items.\n祈祷出现一般物品的比重"); PrayNormalWeight.Value = Math.Abs(PrayNormalWeight.Value);
                    PrayEliteWeight = config.Bind("08 Pray祈祷", "PrayEliteWeight", 0.25f, "Weight of elite equipment.\n祈祷出现精英主动装备的比重"); PrayEliteWeight.Value = Math.Abs(PrayEliteWeight.Value);
                    PrayPeculiarWeight = config.Bind("08 Pray祈祷", "PrayPeculiarWeight", 0.25f, "Weight of peculiar items, some items don't drop in the game, just for fun.\n祈祷出现独特物品的比重，一些在游戏不会掉落的物品"); PrayPeculiarWeight.Value = Math.Abs(PrayPeculiarWeight.Value);
                    PrayPeculiarList = config.Bind("08 Pray祈祷", "PrayPeculiarList", "BoostAttackSpeed,BoostDamage,BoostEquipmentRecharge,BoostHp,BurnNearby,CrippleWardOnLevel,CooldownOnCrit,EmpowerAlways,Ghost,Incubator,InvadingDoppelganger,LevelBonus,WarCryOnCombat,TempestOnKill", "Peculiar items list.\n祈祷出现独特物品列表");
                }

                EnableLunarRecyclerInjection = config.Bind("09 LunarRecycler切片", "EnableLunarRecyclerInjection", true, "Enable recycler data modification.\n启用切片修改");
                if (EnableLunarRecyclerInjection.Value)
                {
                    LunarRecyclerAvailable = config.Bind("09 LunarRecycler切片", "LunarRecyclerAvailable", true, "Enable recycler.\n切片能否购买？");
                    if (LunarRecyclerAvailable.Value)
                    {
                        LunarRecyclerRerollCount = config.Bind("09 LunarRecycler切片", "LunarRecyclerRerollCount", 10, "Recycler reroll max count.\n切片翻滚次数");
                        LunarRecyclerRerollCost = config.Bind("09 LunarRecycler切片", "LunarRecyclerRerollCost", 1, "Recycler reroll starting lunar coins cost.\n切片翻滚起步价格");
                        LunarRecyclerRerollScalar = config.Bind("09 LunarRecycler切片", "LunarRecyclerRerollScalar", 2, "Multiple increases in lunar coins cost.\n切片翻滚增长倍数");
                    }
                }

                EnableSeerStationsInjection = config.Bind("10 预言地图", "EnableSeerStationsInjection", true, "Enable seerstations data modification.\n启用预言地图修改");
                if (EnableSeerStationsInjection.Value)
                {
                    SeerStationAvailable = config.Bind("10 预言地图", "SeerStationAvailable", true, "Enable seerstations.\n预言地图能否购买");
                    if (SeerStationAvailable.Value)
                    {
                        SeerStationsCost = config.Bind("10 预言地图", "LunarSeerStationsCost", 3, "Seerstations lunar coins cost.\n预言地图价格");  
                    }
                }
                EnableLines = config.Bind("98 Newts lines商人台词", "EnableLines", true, "Enable Newt speak function.\n启用商人台词");

                EnableDecorate = config.Bind("99 Decorate装饰", "EnableDecorate", true, "Enable bazaar decoration, someting behide newt like blue portal, chest...This might be my next mod idea.\n启用装饰，就是商人后面的装饰品，如果还活着会是下个mod的彩蛋吧");
            }
        }
    }

}
