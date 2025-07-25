using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace BazaarIsMyHome
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Lunzir.BazaarIsMyHome", "BazaarIsMyHome", "1.3.1")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class BazaarIsMyHome : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo;
        public static ItemHandler ItemHandler;
        System.Random Random = new System.Random();
        readonly DirectorPlacementRule DirectorPlacementRule = new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.Direct
        };

        Dictionary<int, SpawnCardStruct> DicPrinters = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicEquipments = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicScrapers = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicLunarPools = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicCauldrons = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicLunarShopTerminals = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicGlodChests = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicBigChests = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicSmallChests = new Dictionary<int, SpawnCardStruct>();

        int LunarShopTerminalTotalCount = 0;
        List<PurchaseInteraction> ObjectLunarShopTerminals = new List<PurchaseInteraction>();
        List<PurchaseInteraction> ObjectLunarShopTerminals_Spawn = new List<PurchaseInteraction>();

        Dictionary<NetworkUserId, PlayerStruct> PlayerStructs = new Dictionary<NetworkUserId, PlayerStruct>();
        // 如果游戏更新，要检查一下
        List<string> EquipmentCodes = new List<string>
        {
            "EliteEarthEquipment" ,
            "EliteFireEquipment",
            "EliteHauntedEquipment",
            "EliteIceEquipment",
            "EliteLightningEquipment",
            "ElitePoisonEquipment",
            "EliteVoidEquipment",
            "EliteLunarEquipment",
            "LunarPortalOnUse",
        };
        List<SpecialItemStruct> SpecialCodes = new List<SpecialItemStruct>
        {
            new SpecialItemStruct("BoostAttackSpeed", 10),
            new SpecialItemStruct("BoostDamage", 10),
            new SpecialItemStruct("BoostEquipmentRecharge", 10),
            new SpecialItemStruct("BoostHp", 10),
            new SpecialItemStruct("BurnNearby", 1),
            new SpecialItemStruct("CrippleWardOnLevel", 10),
            new SpecialItemStruct("CooldownOnCrit", 1),
            new SpecialItemStruct("EmpowerAlways", 1),
            new SpecialItemStruct("Ghost", 1),
            new SpecialItemStruct("Incubator", 3),
            new SpecialItemStruct("InvadingDoppelganger", 1),
            new SpecialItemStruct("LevelBonus", 10),
            new SpecialItemStruct("WarCryOnCombat", 10),
            new SpecialItemStruct("TempestOnKill", 10),
        };
        List<CauldronHackedStruct> CauldronHackedStructs = new List<CauldronHackedStruct>();

        AsyncOperationHandle<InteractableSpawnCard> iscDuplicator;
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicatorLarge;
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicatorMilitary;
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicatorWild;
        AsyncOperationHandle<InteractableSpawnCard> iscChest1;
        AsyncOperationHandle<InteractableSpawnCard> iscChest2;
        AsyncOperationHandle<InteractableSpawnCard> iscGoldChest;
        AsyncOperationHandle<InteractableSpawnCard> iscShrineRestack;
        AsyncOperationHandle<InteractableSpawnCard> iscShrineHealing;
        AsyncOperationHandle<InteractableSpawnCard> iscShopPortal;
        AsyncOperationHandle<InteractableSpawnCard> iscShrineCleanse;
        AsyncOperationHandle<InteractableSpawnCard> iscScrapper;
        AsyncOperationHandle<InteractableSpawnCard> iscDeepVoidPortalBattery;
        AsyncOperationHandle<GameObject> lunarShopTerminal;
        AsyncOperationHandle<GameObject> multiShopEquipmentTerminal;
        AsyncOperationHandle<GameObject> lunarCauldronWhiteToGreen;
        AsyncOperationHandle<GameObject> lunarCauldronGreenToRed;
        AsyncOperationHandle<GameObject> lunarCauldronRedToWhite;

        AsyncOperationHandle<InteractableSpawnCard>[] PrintersCode;
        AsyncOperationHandle<GameObject>[] LunarCauldronsCode;

        AsyncOperationHandle<GameObject> ShrineUseEffect;
        AsyncOperationHandle<GameObject> LevelUpEffect;
        AsyncOperationHandle<GameObject> MoneyPackPickupEffect;
        AsyncOperationHandle<GameObject> TeamWarCryActivation;
        AsyncOperationHandle<GameObject> LunarRerollEffect;
        AsyncOperationHandle<GameObject> TeleporterBeaconEffect;

        public void Awake()
        {
            ModConfig.InitConfig(Config);

            // --- preload stuff ---

            iscDuplicator = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset");
            iscDuplicatorLarge = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset");
            iscDuplicatorMilitary = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset");
            iscDuplicatorWild = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset");
            PrintersCode = [
                iscDuplicator,
                iscDuplicatorLarge,
                iscDuplicatorMilitary,
                iscDuplicatorWild
            ];

            iscChest1 = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Chest1/iscChest1.asset");
            iscChest2 = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Chest2/iscChest2.asset");
            iscGoldChest = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/GoldChest/iscGoldChest.asset");

            iscShrineRestack = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineRestack/iscShrineRestack.asset");
            iscShrineHealing = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineHealing/iscShrineHealing.asset");
            iscShopPortal = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/PortalShop/iscShopPortal.asset");
            iscShrineCleanse = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset");
            iscScrapper = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset");

            lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
            multiShopEquipmentTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MultiShopEquipmentTerminal/MultiShopEquipmentTerminal.prefab");

            lunarCauldronWhiteToGreen = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, WhiteToGreen.prefab");
            lunarCauldronGreenToRed = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, GreenToRed Variant.prefab");
            lunarCauldronRedToWhite = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, RedToWhite Variant.prefab");
            LunarCauldronsCode = [
                lunarCauldronWhiteToGreen,
                lunarCauldronGreenToRed,
                lunarCauldronRedToWhite
            ];

            ShrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab");
            LevelUpEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LevelUpEffect.prefab");
            MoneyPackPickupEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BonusGoldPackOnKill/MoneyPackPickupEffect.prefab");
            TeamWarCryActivation = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamWarCry/TeamWarCryActivation.prefab");
            LunarRerollEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarRecycler/LunarRerollEffect.prefab");
            TeleporterBeaconEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Teleporter/TeleporterBeaconEffect.prefab");

            PluginInfo = Info;
            Tokens.RegisterLanguageTokens();
            ItemHandler = new ItemHandler();
            On.RoR2.Run.Start += Run_Start;
            // 祈祷
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            // 大锅、打印机注入、主动装备、月球装备、切片
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
            // 切片使用次数
            On.RoR2.PurchaseInteraction.ScaleCost += PurchaseInteraction_ScaleCost;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            // 预言地图
            On.RoR2.BazaarController.SetUpSeerStations += BazaarController_SetUpSeerStations;
            // 生成设施
            On.RoR2.BazaarController.Awake += BazaarController_Awake;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            // 自动商店
            On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;
            // 防商人踢
            On.EntityStates.NewtMonster.KickFromShop.FixedUpdate += KickFromShop_FixedUpdate;
            // 打商人
            On.RoR2.GlobalEventManager.OnHitAll += GlobalEventManager_OnHitAll;
            // 记录商人死亡状态
            On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;
            On.EntityStates.NewtMonster.SpawnState.OnEnter += SpawnState_OnEnter;
            // 物品重写
            On.RoR2.GlobalEventManager.OnCrit += ItemHandler.GlobalEventManager_OnCrit;
            On.RoR2.CharacterBody.RecalculateStats += ItemHandler.CharacterBody_RecalculateStats;
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
        }

        private GameObject testEquip;
        private float rotate = 0f;
        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F4))
            {
                foreach (PlayerCharacterMasterController pc in PlayerCharacterMasterController.instances)
                {
                    var user = pc.networkUser;
                    var pos = pc.body.transform.position;
                    var rot = pc.body.transform.eulerAngles;

                    

                    if (testEquip == null)
                    {
                        testEquip = Instantiate(multiShopEquipmentTerminal.WaitForCompletion(), pos, Quaternion.identity);
                        testEquip.transform.eulerAngles = new Vector3(0f, rotate, 0f);
                        testEquip.transform.localScale = new Vector3(1, 1, 1);
                        NetworkServer.Spawn(testEquip);
                    } else
                    {
                        testEquip.transform.position = new Vector3(pos.x, -23.56834f, pos.z);
                        testEquip.transform.eulerAngles = new Vector3(0f, rotate, 0f);
                        testEquip.transform.localScale = new Vector3(1, 1, 1);
                        rotate += 10f;
                        if (rotate >= 360)
                            rotate = 0f;

                        Logger.LogDebug($"Player pressed F4: (" + testEquip.transform.position.x + "," + testEquip.transform.position.y + "," + testEquip.transform.position.z + ") (" + testEquip.transform.eulerAngles.x + "," + testEquip.transform.eulerAngles.y + "," + testEquip.transform.eulerAngles.z + ")");
                    }
                }
            }
#endif
        }

        public void Start()
        {
            if (ModConfig.EnableMod.Value)
            {
                ShopKeep.IsDeath = false;
                ShopKeep.Body = null;
                
                foreach (var plugin in Chainloader.PluginInfos)
                {
                    if (plugin.Key == "com.funkfrog_sipondo.sharesuite")
                    {
                        ModConfig.ShareSuite = plugin.Value.Instance;
                    }
                    if (plugin.Key == "com.MagnusMagnuson.BiggerBazaar")
                    {
                        ModConfig.BiggerBazaar = plugin.Value.Instance;
                    }
                } 
            }
        }
        public void OnDestroy()
        {
            //On.RoR2.BazaarController.Awake -= BazaarController_Awake;
            //ChatHelper.IsNewtDeath = false;
            //ChatHelper.ShopKeeper = null;
            //HasBeenDonate = false;
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            if (ModConfig.EnableMod.Value)
            {
                ShopKeep.IsDeath = false;
                ShopKeep.Body = null;
                ItemHandler = new ItemHandler();
            }
            orig(self);
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("ShrineHealing"))
                {
                    //HasBeenDonate = true;
                    NetworkUser networkUser = Util.LookUpBodyNetworkUser(activator.gameObject);
                    CharacterMaster characterMaster = activator.GetComponent<CharacterBody>().master;
                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    Inventory inventory = characterBody.inventory;

                    PlayerStructs.TryGetValue(networkUser.id, out PlayerStruct playerStruct);
                    if (playerStruct is null)
                    {
                        playerStruct = new PlayerStruct(networkUser, 1);
                        PlayerStructs.Add(networkUser.id, playerStruct);
                        ChatHelper.ThanksTip(networkUser, characterBody);
                    }
                    else
                    {
                        playerStruct.DonateCount += 1; // 加一次捐赠次数
                        if (playerStruct.DonateCount % 10 == 0) // 每满10次捐赠
                        {
                            playerStruct.RewardCount += 1;
                            if (playerStruct.RewardCount <= ModConfig.RewardCount.Value)
                            {
                                GiftReward(self, networkUser, characterBody, inventory); // 给奖励
                            }
                        }
                    }
                    if (playerStruct.DonateCount <= (ModConfig.RewardCount.Value * 10))
                    {
                        // 购买特效
                        SpawnEffect(ShrineUseEffect, self.transform.position, new Color32(64, 127, 255, 255), 5f);
                        networkUser.DeductLunarCoins((uint)self.Networkcost);
                    }
                    //ChatHelper.Send($"DonateCount = {playerStruct.DonateCount }, RewardCount = {playerStruct.RewardCount}");
                    return;
                }
                // 修复白色大锅
                if (self.name.StartsWith("LunarCauldron, RedToWhite Variant"))
                {
                    if (IsMultiplayer() && ModConfig.IsShareSuite_PrinterCauldronFixEnabled())
                    {
                        Inventory inventory = activator.GetComponent<CharacterBody>().inventory;
                        ShopTerminalBehavior shop = self.GetComponent<ShopTerminalBehavior>();
                        inventory.GiveItem(PickupCatalog.GetPickupDef(shop.CurrentPickupIndex()).itemIndex, 2);
                    }

                }
                if (self.name.StartsWith("LunarRecycler"))
                {
                    float time = 1f;
                    foreach (PurchaseInteraction interaction in ObjectLunarShopTerminals_Spawn)
                    {
                        StartCoroutine(DelayEffect(self, interaction, time));
                        time = time + 0.1f;
                    }
                }
            }
            orig(self, activator);
        }
        private void GiftReward(PurchaseInteraction self, NetworkUser networkUser, CharacterBody characterBody, Inventory inventory)
        {
            float w1 = ModConfig.PrayNormalWeight.Value, w2 = ModConfig.PrayEliteWeight.Value, w3 = ModConfig.PrayPeculiarWeight.Value;
            double random = Random.NextDouble() * (w1 + w2 + w3);
            if (random <= w1)
            {
                WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                weightedSelection.AddChoice(Run.instance.availableTier1DropList, 100f);
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, 60f);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, 10f);
                List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                PickupDef pickupDef = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]);
                inventory.GiveItem((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None, 1);
                // 特效
                PurchaseInteraction.CreateItemTakenOrb(self.gameObject.transform.position, characterBody.gameObject, pickupDef.itemIndex);
                ChatHelper.ThanksTip(networkUser, characterBody, pickupDef);
            }
            else if (random <= w1 + w2)
            {
                string equipCode = EquipmentCodes[Random.Next(EquipmentCodes.Count)];
                EquipmentIndex equipIndex = EquipmentCatalog.FindEquipmentIndex(equipCode);
                EquipmentIndex IsHasEquip = inventory.GetEquipmentIndex();
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipIndex);

                if (IsHasEquip != EquipmentIndex.None) // 如果玩家身上有主动装备，掉落出来
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(IsHasEquip), characterBody.gameObject.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + self.transform.forward * 2f);
                inventory.SetEquipmentIndex(equipIndex); // 为了获得文本，其实可以用GiveEquipmentString()

                ChatHelper.ThanksTip(networkUser, characterBody, equipmentDef);
            }
            else
            {
                SpecialItemStruct specialItemStruct = SpecialCodes[Random.Next(SpecialCodes.Count)];
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(specialItemStruct.Name);
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                inventory.GiveItem(itemDef, specialItemStruct.Count);
                // 特效
                PurchaseInteraction.CreateItemTakenOrb(self.gameObject.transform.position, characterBody.gameObject, itemIndex);
                ChatHelper.ThanksTip(networkUser, characterBody, itemDef, specialItemStruct.Count);
            }
            SpawnEffect(LevelUpEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(MoneyPackPickupEffect, self.transform.position, new Color32(255, 255, 255, 255), 3f);
            SpawnEffect(TeamWarCryActivation, self.transform.position, new Color32(255, 255, 255, 255), 3f);
        }
        IEnumerator DelayEffect(PurchaseInteraction self, PurchaseInteraction interaction, float time)
        {
            yield return new WaitForSeconds(time);
            RerollLunarShopTerminal(self, interaction);
        }
        private void RerollLunarShopTerminal(PurchaseInteraction self, PurchaseInteraction interaction)
        {
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
            weightedSelection.AddChoice(Run.instance.availableLunarItemDropList, 50f);
            weightedSelection.AddChoice(Run.instance.availableLunarEquipmentDropList, 50f);
            List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
            PickupDef pickupDef = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]);

            ShopTerminalBehavior shopTerminal = interaction.gameObject.GetComponent<ShopTerminalBehavior>();
            shopTerminal.SetPickupIndex(pickupDef.pickupIndex);

            SpawnEffect(LunarRerollEffect, interaction.gameObject.transform.position + Vector3.up * 1f, new Color32(255, 255, 255, 255), 2f);
        }

        private void PurchaseInteraction_ScaleCost(On.RoR2.PurchaseInteraction.orig_ScaleCost orig, PurchaseInteraction self, float scalar)
        {
            if (ModConfig.EnableMod.Value)
            {
                if (self.name.StartsWith("LunarRecycler"))
                {
                    if (ModConfig.EnableLunarRecyclerInjection.Value)
                    {
                        if (ModConfig.LunarRecyclerAvailable.Value)
                        {
                            scalar = (float)ModConfig.LunarRecyclerRerollScalar.Value; 
                        }
                    } 
                }
                if (self.name.StartsWith("ShrineRestack"))
                {
                    if (ModConfig.EnableShrineRestack.Value)
                    {
                        scalar = (float)ModConfig.ShrineRestackScalar.Value;
                    }
                }
            }
            orig.Invoke(self, scalar);
        }
        private void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            if (ModConfig.EnableMod.Value)
            {
                if (self.name.StartsWith("LunarRecycler"))
                {
                    if (ModConfig.EnableLunarRecyclerInjection.Value)
                    {
                        if (ModConfig.LunarRecyclerAvailable.Value)
                        {
                            //ChatHelper.Send($"RerolledCount = {ModConfig.RerolledCount}, Reroll = {ModConfig.LunarRecyclerRerollCount.Value}");
                            if (ModConfig.RerolledCount < ModConfig.LunarRecyclerRerollCount.Value)
                            {
                                orig.Invoke(self, true);
                            }
                            else
                            {
                                orig.Invoke(self, false);
                            }
                            ModConfig.RerolledCount++;
                            return; 
                        }
                    }
                }

            }
            orig.Invoke(self, newAvailable);
        }

        private void BazaarController_SetUpSeerStations(On.RoR2.BazaarController.orig_SetUpSeerStations orig, BazaarController self)
        {
            if (ModConfig.EnableMod.Value)
            {
                if ((ModConfig.EnableSeerStationsInjection.Value || ModConfig.PenaltyCoefficient_Temp != 1))
                {
                    foreach (SeerStationController seerStationController in self.seerStations)
                    {
                        seerStationController.GetComponent<PurchaseInteraction>().available = ModConfig.SeerStationAvailable.Value;
                        if (ModConfig.SeerStationAvailable.Value)
                        {
                            seerStationController.GetComponent<PurchaseInteraction>().cost = ModConfig.SeerStationsCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            seerStationController.GetComponent<PurchaseInteraction>().Networkcost = ModConfig.SeerStationsCost.Value * ModConfig.PenaltyCoefficient_Temp; 
                        }
                    }
                }
                else
                {
                    if (ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        foreach (SeerStationController seerStationController in self.seerStations)
                        {
                            seerStationController.GetComponent<PurchaseInteraction>().cost *= ModConfig.PenaltyCoefficient_Temp;
                            seerStationController.GetComponent<PurchaseInteraction>().Networkcost *= ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
            }
            orig(self);
        }

        private void BazaarController_Awake(On.RoR2.BazaarController.orig_Awake orig, BazaarController self)
        {
            //ChatHelper.Send("BazaarController_Awake");
            if (ModConfig.EnableMod.Value)
            {
                Config.Reload();
                ModConfig.InitConfig(Config);
                SetCaudronList_Hacked();
                ShopKeep.SpawnTime_Record = 0;
                ModConfig.RerolledCount = 0;

                if (ModConfig.EnableShrineHealing.Value)
                {
                    InitPrayData(); 
                }

                if (NetworkServer.active)
                {
                    #region 开始生成设备
                    ArtifactDef artifactDef = ArtifactCatalog.FindArtifactDef("Sacrifice");
                    bool isEnableSacrifice = false;
                    if (RunArtifactManager.instance.IsArtifactEnabled(artifactDef))
                    {
                        isEnableSacrifice = true;
                        RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, false);
                    }
                    PlayerStructs.Clear();
                    SpawnPrinters(); // 打印机
                    SpawnLunarCauldron(); // 大锅
                    SpawnScrapper(); // 收割机
                    SpawnEquipment(); // 主动装备
                    SpawnLunarShopTerminal(); // 月球蓓蕾
                    SpawnShrineCleanse(); // 月池
                    SpawnShrineRestack(); // 跌序
                    SpawnShrineHealing(); // 祈祷
                    SpawnDecorate(); // 装饰
                    if (isEnableSacrifice) RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, true);
                    #endregion
                }
            }
            orig.Invoke(self);
        }

        private void InitPrayData()
        {
            //SpecialCodes 
            SpecialCodes.ForEach(x => x.IsUse = false);
            string[] codes = ModConfig.PrayPeculiarList.Value.Split(',');
            for (int i = 0; i < codes.Length; i++)
            {
                string code = codes[i].Trim().ToLower();
                SpecialItemStruct result = SpecialCodes.FirstOrDefault(x => x.Name.ToLower() == code);
                result.IsUse = true;
            }
        }

        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            //ChatHelper.Send("PurchaseInteraction_Awake");
            orig(self);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                // 大锅
                if (ModConfig.CauldronCount.Value > 0)
                {
                    if (ModConfig.EnableCauldronHacking.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        double random = Random.NextDouble();
                        if (self.name.StartsWith("LunarCauldron, WhiteToGreen")) // 绿锅
                        {
                            self.cost = ModConfig.CauldronWhiteToGreenCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.CauldronWhiteToGreenCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            if (random <= ModConfig.CauldronGreenHackedChance.Value && ModConfig.EnableCauldronHacking.Value) // 被黑概率
                            {
                                //ChatHelper.Send("一台绿锅被黑");
                                CauldronHacked_Start(self, "LunarCauldronGreen"); // 变特定锅
                            }
                        }
                        if (self.name.StartsWith("LunarCauldron, GreenToRed")) // 红锅
                        {
                            self.cost = ModConfig.CauldronGreenToRedCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.CauldronGreenToRedCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            if (random <= ModConfig.CauldronRedHackedChance.Value && ModConfig.EnableCauldronHacking.Value)
                            {
                                //ChatHelper.Send("一台红锅被黑");
                                CauldronHacked_Start(self, "LunarCauldronRed");
                            }
                        }
                        if (self.name.StartsWith("LunarCauldron, RedToWhite")) // 白锅
                        {
                            self.cost = ModConfig.CauldronRedToWhiteCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.CauldronRedToWhiteCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            if (ModConfig.CauldronWhiteCostTypeChange.Value)
                            {
                                self.costType = CostTypeIndex.GreenItem;
                            }
                            if (random <= ModConfig.CauldronWhiteHackedChance.Value && ModConfig.EnableCauldronHacking.Value)
                            {
                                //ChatHelper.Send("一台白锅被黑");
                                CauldronHacked_Start(self, "LunarCauldronWhite");
                            }
                        }
                    }
                }
                else
                {
                    if (ModConfig.PenaltyCoefficient_Temp != 1) // 如果商人死了，关了数量，开了惩罚系数
                    {
                        if (self.name.StartsWith("LunarCauldron, WhiteToGreen") 
                            || self.name.StartsWith("LunarCauldron, GreenToRed")
                            || self.name.StartsWith("LunarCauldron, RedToWhite")) 
                        {
                            self.cost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                        } 
                    }
                }

                // 打印机
                if (ModConfig.PrinterCount.Value > 0)
                {
                    if (self.name.StartsWith("Duplicator")
                                || self.name.StartsWith("DuplicatorLarge")
                                || self.name.StartsWith("DuplicatorMilitary")
                                || self.name.StartsWith("DuplicatorWild"))
                    {
                        float w1, w2, w3, w4, w5, w6, total;
                        w1 = ModConfig.PrinterTier1Weight.Value;
                        w2 = ModConfig.PrinterTier2Weight.Value;
                        w3 = ModConfig.PrinterTier3Weight.Value;
                        w4 = ModConfig.PrinterTierBossWeight.Value;
                        w5 = ModConfig.PrinterTierLunarWeight.Value;
                        w6 = ModConfig.PrinterTierVoidWeight.Value;
                        total = w1 + w2 + w3 + w4 + w5 + w6;
                        if (total != 0)
                        {
                            double random = Random.NextDouble() * total;
                            if (random <= w1) { }
                            else if (random <= w1 + w2) { }
                            else if (random <= w1 + w2 + w3) { }
                            else if (random <= w1 + w2 + w3 + w4) { }
                            else if (random <= w1 + w2 + w3 + w4 + w5)
                            {
                                self.name = "DuplicatorBlue";
                                self.costType = CostTypeIndex.LunarItemOrEquipment;
                            }
                            else
                            {
                                self.name = "DuplicatorPurple";
                                self.costType = CostTypeIndex.RedItem;
                            }
                        }
                    }
                }
                if (ModConfig.EquipmentCount.Value > 0)
                {
                    // 主动装备
                    if (self.name.StartsWith("MultiShopEquipmentTerminal"))
                    {
                        //ChatHelper.Send("一台主动装备已修改");
                        self.cost = 0;
                        self.Networkcost = 0;
                        //self.costType = CostTypeIndex.PercentHealth;
                    } 
                }
                // 月球装备
                if (ModConfig.LunarShopTerminalCount.Value > 0)
                {
                    if (ModConfig.EnableLunarShopTerminalInjection.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        if (self.name.StartsWith("LunarShopTerminal") || self.name.StartsWith("MyLunarBud")) 
                        {
                            self.cost = ModConfig.LunarShopTerminalCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.LunarShopTerminalCost.Value * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
                else
                {
                    if (ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        if (self.name.StartsWith("LunarShopTerminal") || self.name.StartsWith("MyLunarBud"))
                        {
                            self.cost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
                if (self.name.StartsWith("LunarShopTerminal"))
                {
                    if (LunarShopTerminalTotalCount < 5)
                    {
                        ObjectLunarShopTerminals.Add(self);
                    }
                }

                // 切片
                if (ModConfig.EnableLunarRecyclerInjection.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                {
                    if (self.name.StartsWith("LunarRecycler"))
                    {
                        // 失效 不能购买
                        self.available = ModConfig.LunarRecyclerAvailable.Value;
                        self.Networkavailable = ModConfig.LunarRecyclerAvailable.Value;

                        if (ModConfig.LunarRecyclerAvailable.Value)
                        {
                            self.cost = ModConfig.LunarRecyclerRerollCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.LunarRecyclerRerollCost.Value * ModConfig.PenaltyCoefficient_Temp; 
                        }
                    }
                }
                else
                {
                    if (ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        if (self.name.StartsWith("LunarRecycler"))
                        {
                            self.cost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
            }
        }
        private void CauldronHacked_Start(PurchaseInteraction self, string newName)
        {
            float w1 = ModConfig.CauldronYellowWeight.Value, w2 = ModConfig.CauldronBlueWeight.Value, w3 = ModConfig.CauldronPurpleWeight.Value;
            float total = w1 + w2 + w3;
            double random = Random.NextDouble() * total;
            CauldronHackedStruct cauldronHacked = null;
            if (random <= w1)
            {
                //ChatHelper.Send("被黑成黄色");
                cauldronHacked = CauldronHackedStructs.FirstOrDefault(x => x.Name.StartsWith(newName) && x.Name.EndsWith("Yellow"));
                self.name = cauldronHacked.Name;
                self.cost = cauldronHacked.Cost;
                self.Networkcost = cauldronHacked.Cost;
                self.costType = cauldronHacked.CostTypeIndex;
            }
            else if (random <= w1 + w2)
            {
                //ChatHelper.Send("被黑成蓝色");
                cauldronHacked = CauldronHackedStructs.FirstOrDefault(x => x.Name.StartsWith(newName) && x.Name.EndsWith("Blue"));
                self.name = cauldronHacked.Name;
                self.cost = cauldronHacked.Cost;
                self.Networkcost = cauldronHacked.Cost;
                self.costType = cauldronHacked.CostTypeIndex;
            }
            else
            {
                //ChatHelper.Send("被黑成紫色");
                cauldronHacked = CauldronHackedStructs.FirstOrDefault(x => x.Name.StartsWith(newName) && x.Name.EndsWith("Purple"));
                self.name = cauldronHacked.Name;
                self.cost = cauldronHacked.Cost;
                self.Networkcost = cauldronHacked.Cost;
                self.costType = cauldronHacked.CostTypeIndex;
            }
        }

        private void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("LunarCauldronGreen"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[Random.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronRed"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[Random.Next(0, list.Count)];
                }
                if (self.name.StartsWith("LunarCauldronWhite"))
                {
                    CauldronHacked_SetPickupIndex(self, out List<PickupIndex> list);
                    newPickupIndex = list[Random.Next(0, list.Count)];
                }
                if (self.name.StartsWith("DuplicatorBlue"))
                {
                    List<PickupIndex> listLunarItem = Run.instance.availableLunarItemDropList;
                    newPickupIndex = listLunarItem[UnityEngine.Random.Range(0, listLunarItem.Count)];
                }
                if (self.name.StartsWith("DuplicatorPurple"))
                {
                    WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                    weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 25f);
                    weightedSelection.AddChoice(Run.instance.availableVoidTier2DropList, 25f);
                    weightedSelection.AddChoice(Run.instance.availableVoidTier3DropList, 25f);
                    weightedSelection.AddChoice(Run.instance.availableVoidBossDropList, 25f);
                    List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                    newPickupIndex = list[UnityEngine.Random.Range(0, list.Count)];
                }
            }
            orig(self, newPickupIndex, newHidden);
        }
        private void CauldronHacked_SetPickupIndex(ShopTerminalBehavior self, out List<PickupIndex> listLunarItem)
        {
            listLunarItem = new List<PickupIndex>();
            if (self.name.EndsWith("Yellow"))
            {
                listLunarItem.AddRange(Run.instance.availableBossDropList);
            }
            if (self.name.EndsWith("Blue"))
            {
                listLunarItem.AddRange(Run.instance.availableLunarItemDropList);
            }
            if (self.name.EndsWith("Purple"))
            {
                listLunarItem.AddRange(Run.instance.availableVoidTier1DropList);
                listLunarItem.AddRange(Run.instance.availableVoidTier2DropList);
                listLunarItem.AddRange(Run.instance.availableVoidTier3DropList);
                listLunarItem.AddRange(Run.instance.availableVoidBossDropList);
            }
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                if (NetworkServer.active)
                {
                    ShopKeep.Body = null;
                }
            }
            orig(self);
        }
        private void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, TeleporterInteraction self)
        {
            if (ModConfig.EnableMod.Value && ModConfig.EnableAutoOpenShop.Value)
            {
                orig.Invoke(self);
                self.shouldAttemptToSpawnShopPortal = true;
            }
            else
            {
                orig.Invoke(self);
            }
        }
        private void KickFromShop_FixedUpdate(On.EntityStates.NewtMonster.KickFromShop.orig_FixedUpdate orig, EntityStates.NewtMonster.KickFromShop self)
        {
            if (ModConfig.EnableMod.Value && ModConfig.EnableNoKickFromShop.Value)
            {
                self.outer.SetNextStateToMain(); 
            }
        }
        private void GlobalEventManager_OnHitAll(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            if (ModConfig.EnableMod.Value && ModConfig.EnableLines.Value)
            {
                if (IsCurrentMapInBazaar())
                {
                    try
                    {
                        if (hitObject.name.StartsWith("ShopkeeperBody") && damageInfo.attacker)
                        {
                            ChatHelper.HitWord();
                            orig.Invoke(self, damageInfo, hitObject);
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                    finally
                    {

                    }
                    return;
                }
            }
            orig.Invoke(self, damageInfo, hitObject);

        }
        private void CharacterMaster_OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            orig.Invoke(self, body);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                //var attack = self.name;
                var victim = body.name;
                //ChatHelper.Send($"victim = {victim}");
                if (victim.Contains("ShopkeeperBody"))
                {
                    ShopKeep.IsDeath = true;
                    //ChatHelper.SpawnTime_Record++;
                    ShopKeep.SpawnTime_Record++;
                    if (ModConfig.EnableNewtNoDie.Value)
                    {
                        //Stage.instance.RespawnCharacter(body.master);
                        //body.name = "ShopkeeperBody";
                        body.master.Respawn(body.footPosition, Quaternion.identity);
                        AddItemToShopKeeper(body);
                    }
                    else
                    {
                        ChatHelper.ShowNewtDeath();
                        //string card = "Spawncards/InteractableSpawncard/iscScavLunarbackpack";
                        //DoSpawnCard(card, new Vector3(-122.7888f, -22.3505f, -45.7878f));
                        //DoSpawnCard(card, new Vector3(-125.9958f, -22.4272f, -42.6213f));
                        //Vector3 vector3 = body.footPosition;
                        //ChatHelper.Send($"死亡位置：{vector3.x},{vector3.y},{vector3.z}");
                    }
                    
                }
            }
        }
        private void AddItemToShopKeeper(CharacterBody body)
        {
            //body.teamComponent.teamIndex = TeamIndex.Monster;
            
            if (ModConfig.NewtSecondLifeMode.Value == ShopKeep.DeathState.Tank)
            {
                //body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("HealingPotion")), 1000);
                //body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Clover")), 20);
                //body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Medkit")), 100);
                //body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ParentEgg")), 10000);
                //body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("IncreaseHealing")), 100);

            }
            if (ModConfig.NewtSecondLifeMode.Value == ShopKeep.DeathState.Evil)
            {
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("HealingPotion")), 1000); // 强力万能药
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Thorns")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Clover")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Medkit")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("NearbyDamageBonus")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StickyBomb")), 50);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StunChanceOnHit")), 50);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("CaptainDefenseMatrix")), 1);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ExplodeOnDeath")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SlowOnHit")), 10);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StrengthenBurn")), 1);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ImmuneToDebuff")), 1);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BearVoid")), 1);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BurnNearby")), 1);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("InvadingDoppelganger")), 1);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("AutoCastEquipment")), 5);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("EquipmentMagazine")), 10);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("CritGlasses")), 20);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Syringe")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Phasing")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ParentEgg")), 10000);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BarrierOnOverHeal")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("IncreaseHealing")), 100);
                body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Crowbar")), 10);
                body.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteVoidEquipment"));
            }
        }

        private void SpawnState_OnEnter(On.EntityStates.NewtMonster.SpawnState.orig_OnEnter orig, EntityStates.NewtMonster.SpawnState self)
        {
            try
            {
                if (ModConfig.EnableMod.Value)
                {
                    //ChatHelper.Send("进入SpawnState_OnEnter");
                    // 传送门
                    if (ModConfig.EnableDecorate.Value)
                    {
                        SpawnBluePortal();
                    }
                    // 门口特效 火把
                    if (ModConfig.EnableDecorate.Value)
                    {
                        // 门口
                        SpawnEffect(TeleporterBeaconEffect, new Vector3(-73.5143f, -22.2897f, 9.1621f), Color.blue, 1f);
                        SpawnEffect(TeleporterBeaconEffect, new Vector3(-57.0645f, -22.2698f, -0.5218f), Color.blue, 1f);
                        // 传送门位置
                        SpawnEffect(TeleporterBeaconEffect, new Vector3(15.7063f, -2.1074f, 2.5406f), Color.blue, 1f);
                        SpawnEffect(TeleporterBeaconEffect, new Vector3(2.5543f, -2.7093f, -8.7185f), Color.blue, 1f);
                    }
                    // 欢迎语
                    StartCoroutine(ShopWelcomeWord());

                    if (ShopKeep.SpawnTime_Record == 0)
                    {
                        if (ShopKeep.Body is null) FindShopkeeper();
                        //ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BoostHp")), 10000 * 10000);
                        //ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Pearl")), 10000 * 10000);
                        //ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ParentEgg")), 34 * 1000);
                        //ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ShieldOnly")), 100*1000);
                        //ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Medkit")), 10000);
                        // 死亡状态 添加物品
                        if (ShopKeep.IsDeath)
                        {
                            if (ModConfig.EnableNewtNoDie.Value)
                            {
                                switch (ModConfig.NewtSecondLifeMode.Value)
                                {
                                    case ShopKeep.DeathState.Ghost:
                                        ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Ghost")), 1);
                                        break;
                                    case ShopKeep.DeathState.Tank:
                                        ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BoostHp")), 10);
                                        break;
                                    case ShopKeep.DeathState.Evil:
                                        AddItemToShopKeeper(ShopKeep.Body);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        } 
                    }

                    // 重写月球装备
                    if (LunarShopTerminalTotalCount < 5)
                    {
                        // 先改成禁用
                        ObjectLunarShopTerminals.ForEach(x => x.SetAvailable(false));
                        // 打乱List
                        ObjectLunarShopTerminals = DisorderList(ObjectLunarShopTerminals);
                        // 逐个启用
                        for (int i = 0; i < LunarShopTerminalTotalCount; i++)
                        {
                            ObjectLunarShopTerminals[i].SetAvailable(true);
                        }
                        // 没启用的删除
                        foreach (PurchaseInteraction interaction in ObjectLunarShopTerminals)
                        {
                            if (!interaction.Networkavailable)
                            {
                                UnityEngine.Object.Destroy(interaction.gameObject);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {

            }
            orig.Invoke(self);
        }
        IEnumerator ShopWelcomeWord()
        {
            yield return new WaitForSeconds(0.5f);
            if (ModConfig.EnableWelcomeWord.Value)
            {
                ChatHelper.WelcomeWord();
            }
        }
        private void FindShopkeeper()
        {
            TeamIndex team = TeamIndex.Neutral;
            foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
            {
                if (cm.teamIndex == team)
                {
                    CharacterBody cb = cm.GetBody();
                    if (cb && cb.name.StartsWith("ShopkeeperBody"))
                    {
                        ShopKeep.Body = cb;
                        break;
                    }
                }
            }
        }
        // 打乱 泛型列表项目
        private List<t> DisorderList<t>(List<t> TList)  
        {
            List<t> NewList = new List<t>();
            System.Random Rand = new System.Random();
            foreach (var item in TList)
            {
                NewList.Insert(Rand.Next(NewList.Count()), item);
            }
            return NewList;
        }

        #region 初始化设备
        private void SetPrinter()
        {
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicPrinters.Add(random[0], new SpawnCardStruct(new Vector3(-112f, -26.8f, -46.0f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[1], new SpawnCardStruct(new Vector3(-108f, -26.8f, -48.5f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[2], new SpawnCardStruct(new Vector3(-104f, -26.7f, -51.0f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[3], new SpawnCardStruct(new Vector3(-127f, -26.0f, -34.5f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[4], new SpawnCardStruct(new Vector3(-131f, -26.0f, -31.8f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[5], new SpawnCardStruct(new Vector3(-135f, -26.0f, -29.0f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[6], new SpawnCardStruct(new Vector3(-144f, -24.7f, -24.0f), new Vector3(0.0f, 60.2f, 0.0f)));
            DicPrinters.Add(random[7], new SpawnCardStruct(new Vector3(-145f, -25.0f, -20.0f), new Vector3(0.0f, 80.0f, 0.0f)));
            DicPrinters.Add(random[8], new SpawnCardStruct(new Vector3(-146f, -25.3f, -16.0f), new Vector3(0.0f, 100.0f, 0.0f)));
        }
        private void SetScraper()
        {
            List<int> total = new List<int> { 0, 1, 2, 3 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicScrapers.Add(random[0], new SpawnCardStruct(new Vector3(-95.0f, -25.5f, -45.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            DicScrapers.Add(random[1], new SpawnCardStruct(new Vector3(-100.0f, -25.0f, -40.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            DicScrapers.Add(random[2], new SpawnCardStruct(new Vector3(-90.0f, -25.5f, -40.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            DicScrapers.Add(random[3], new SpawnCardStruct(new Vector3(-95.0f, -25.5f, -35.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            //DicScrapers.Add(random[2], new SpawnCardStruct(new Vector3(-105f, -26.0f, -35.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            //DicScrapers.Add(random[5], new SpawnCardInteface(new Vector3(-100.0f, -26.5f, -30.0f), new Vector3(0.0f, 72.0f, 0.0f)));
        }
        private void SetCauldron()
        {
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            List<int> random = new List<int>();

            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicCauldrons.Add(random[0], new SpawnCardStruct(new Vector3(-115.9816f, -24.1175f, -6.2091f), new Vector3(0.0f, 120.0f, 0.0f)));
            DicCauldrons.Add(random[1], new SpawnCardStruct(new Vector3(-119.9280f, -24.1238f, -7.0865f), new Vector3(0.0f, 140.0f, 0.0f)));
            DicCauldrons.Add(random[2], new SpawnCardStruct(new Vector3(-123.4725f, -23.7951f, -5.4690f), new Vector3(0.0f, 160.0f, 0.0f)));
            DicCauldrons.Add(random[3], new SpawnCardStruct(new Vector3(-107.8159f, -23.8448f, -4.5170f), new Vector3(0.0f, 130.0f, 0.0f)));
            DicCauldrons.Add(random[4], new SpawnCardStruct(new Vector3(-101.2425f, -24.8612f, -9.1464f), new Vector3(0.0f, 160.0f, 0.0f)));
            DicCauldrons.Add(random[5], new SpawnCardStruct(new Vector3(-98.5219f, -25.6548f, -12.3659f), new Vector3(0.0f, 155.0f, 0.0f)));
            DicCauldrons.Add(random[6], new SpawnCardStruct(new Vector3(-94.6071f, -25.8717f, -13.6159f), new Vector3(0.0f, 135.0f, 0.0f)));
            //DicCauldrons.Add(random[6], new SpawnCardStruct(new Vector3(-91.1582f, -25.0957f, -10.9174f), new Vector3(0.0f, 80.0f, 0.0f)));
            //DicCauldrons.Add(random[7], new SpawnCardStruct(new Vector3(-89.8054f, -24.0894f, -7.2084f), new Vector3(0.0f, 80.0f, 0.0f)));
            //DicCauldrons.Add(random[8], new SpawnCardStruct(new Vector3(-85.7223f, -23.6673f, -4.8544f), new Vector3(0.0f, 85.0f, 0.0f)));
        }
        private void SetEquipment()
        {
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            if (!ModConfig.SeerStationAvailable.Value && ModConfig.EquipmentCount.Value <= 2)
            {
                // left seer stand
                DicEquipments.Add(0, new SpawnCardStruct(new Vector3(-133.9731f, -23.4f, -10.71112f), new Vector3(0f, 120.0f, 0.0f)));
                // right seer stand
                DicEquipments.Add(1, new SpawnCardStruct(new Vector3(-128.0793f, -23.4f, -7.056283f), new Vector3(0f, 160.0f, 0.0f)));
            }
            else
            {
                DicEquipments.Add(random[0], new SpawnCardStruct(new Vector3(-128.9115f, -23.1756f, -24.6339f), new Vector3(350.0f, 90.0f, 0.0f)));
                DicEquipments.Add(random[1], new SpawnCardStruct(new Vector3(-131.3281f, -23.0673f, -21.9982f), new Vector3(353.0f, 0.0f, 0.0f)));
                DicEquipments.Add(random[2], new SpawnCardStruct(new Vector3(-132.8414f, -22.6963f, -26.6293f), new Vector3(353.0f, 220.0f, 0.0f)));
                DicEquipments.Add(random[3], new SpawnCardStruct(new Vector3(-141.3541f, -21.2761f, -10.9000f), new Vector3(358.0f, 180.0f, 0.0f)));
                DicEquipments.Add(random[4], new SpawnCardStruct(new Vector3(-138.9401f, -20.9378f, -8.87810f), new Vector3(355.0f, 100.0f, 0.0f)));
                DicEquipments.Add(random[5], new SpawnCardStruct(new Vector3(-139.9517f, -20.8648f, -5.79960f), new Vector3(353.0f, 30.0f, 0.0f)));
                //DicTriplEquipments.Add(random[0], new SpawnCardStruct(new Vector3(-142f, -22.0f, 0.0f), new Vector3(0.0f, 72.0f, 0.0f)));
                //DicTriplEquipments.Add(random[1], new SpawnCardStruct(new Vector3(-139f, -22.8f, -2.0f), new Vector3(0.0f, 72.0f, 0.0f)));
                //DicTriplEquipments.Add(random[2], new SpawnCardStruct(new Vector3(-136f, -22.5f, 0.0f), new Vector3(0.0f, 72.0f, 0.0f)));
                //DicTriplEquipments.Add(random[3], new SpawnCardStruct(new Vector3(-135f, -22.0f, 3.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            }
        }


        private void SetLunarShopTerminal()
        {
            /*
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5 };
            List<int> random = new List<int>();

            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicLunarShopTerminals.Add(random[0], new SpawnCardStruct(new Vector3(-90.8748f, -22.3210f, -49.7166f), new Vector3(0.0f, 250.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[1], new SpawnCardStruct(new Vector3(-90.7317f, -22.1151f, -53.4639f), new Vector3(0.0f, 240.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[2], new SpawnCardStruct(new Vector3(-87.8854f, -22.1132f, -53.3190f), new Vector3(0.0f, 180.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[3], new SpawnCardStruct(new Vector3(-86.6861f, -22.9508f, -50.5742f), new Vector3(0.0f, 100.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[4], new SpawnCardStruct(new Vector3(-70.2474f, -24.1325f, -51.1947f), new Vector3(0.0f, 230.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[5], new SpawnCardStruct(new Vector3(-76.9623f, -25.8940f, -41.4813f), new Vector3(0.0f, 120.0f, 0.0f)));*/

            Vector3 lunarTablePosition = new Vector3(-76.6438f, -24.0468f, -41.6449f);
            const float orientation = 280f;
            Vector3 lunarTableDroneShopPosition = new Vector3(-139.8156f, -21.8568f, 2.9263f);
            const float droneTableOrientation = 16 0f

            const float tableRadiusInner = 3.0f;
            const float tableRadiusMiddle = 4.0f;
            const float tableRadiusOuter = 5.0f;
            const float tableStartAngleInner = 0f;
            const float tableStartAngleMiddle = -10f;
            const float tableStartAngleOuter = 8f;
            const float tableEndAngleInner = 215f;
            const float tableEndAngleMiddle = 230f;
            const float tableEndAngleOuter = 234f;
            const float minDistance = 19f;
            const float innerCapacity = 5;//(int)(2 * Math.PI * tableRadiusInner * (tableEndAngleInner - tableStartAngleInner) / 360f / minDistance);
            const float middleCapacity = 8;//(int)(2 * Math.PI * tableRadiusMiddle * (tableEndAngleMiddle - tableStartAngleMiddle) / 360f / minDistance);
            const float outerCapacity = 10;//(int)(2 * Math.PI * tableRadiusOuter * (tableEndAngleOuter - tableStartAngleOuter) / 360f / minDistance);

            List<Vector2> points = new List<Vector2>();
            if (ModConfig.LunarShopTerminalCount.Value <= middleCapacity)
            {
                points = Lloyd.GenerateCirclePoints(tableRadiusMiddle, tableStartAngleMiddle, tableEndAngleMiddle, orientation, ModConfig.LunarShopTerminalCount.Value);
            }
            else
            {
                points.AddRange(Lloyd.GenerateCirclePoints(tableRadiusInner, tableStartAngleInner, tableEndAngleInner, orientation, 100));
                points.AddRange(Lloyd.GenerateCirclePoints(tableRadiusOuter, tableStartAngleOuter, tableEndAngleOuter, orientation, 100));
                points = Lloyd.LloydsAlgorithm(points, ModConfig.LunarShopTerminalCount.Value);
            }
            float scale = 1.0f;
            switch(ModConfig.LunarShopTerminalCount.Value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    scale = 1f;
                    break;
                case 5:
                    scale = 0.95f;
                    break;
                case 6:
                    scale = 0.9f;
                    break;
                case 7:
                    scale = 0.85f;
                    break;
                case 8:
                    scale = 0.8f;
                    break;
                default:
                    scale = 0.75f;
                    break;
            }
            for (int i = 0; i < points.Count; i++)
            {
                DicLunarShopTerminals.Add(i, new SpawnCardStruct(new Vector3(lunarTablePosition.x + points[i].x, lunarTablePosition.y, lunarTablePosition.z + points[i].y), new Vector3(0.0f, 250.0f + i * 10f / points.Count, 0.0f), new Vector3(scale, scale, scale)));
            }
        }
        private void SetLunarPool()
        {
            DicLunarPools.Add(0, new SpawnCardStruct(new Vector3(-115.420f, -9.55f, -50.3600f), new Vector3(90.0f, 30.0f, 0.0f)));
            DicLunarPools.Add(1, new SpawnCardStruct(new Vector3(-129.891f, -9.55f, -42.6537f), new Vector3(90.0f, 30.0f, 0.0f)));
        }
        private void SetDecorate()
        {
            List<int> total = new List<int> { 0, 1, 2 };
            List<int> random = new List<int>();

            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicGlodChests.Add(random[0], new SpawnCardStruct(new Vector3(-127.7178f, -23.5346f, -60.0732f), new Vector3(350.0f, 0.0f, 0.0f)));
            DicGlodChests.Add(random[1], new SpawnCardStruct(new Vector3(-129.2000f, -22.7723f, -63.2481f), new Vector3(350.0f, 150.0f, 350.0f)));
            DicGlodChests.Add(random[2], new SpawnCardStruct(new Vector3(-126.5934f, -22.8150f, -62.1658f), new Vector3(357.8401f, 341.0997f, 330f)));

            total = new List<int> { 0, 1, 2, 3 };
            random = new List<int>();
            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicBigChests.Add(random[0], new SpawnCardStruct(new Vector3(-134.5802f, -25.3546f, -68.1701f), new Vector3(333.3292f, 136.2832f, 335.803f)));
            DicBigChests.Add(random[1], new SpawnCardStruct(new Vector3(-134.7337f, -26.2325f, -67.6419f), new Vector3(333.3292f, 136.2832f, 335.803f)));
            DicBigChests.Add(random[2], new SpawnCardStruct(new Vector3(-132.4877f, -25.2405f, -66.5167f), new Vector3(22.3718f, 143.4671f, 327.6613f)));
            DicBigChests.Add(random[3], new SpawnCardStruct(new Vector3(-132.7968f, -25.409f, -65.5423f), new Vector3(22.3718f, 143.4671f, 327.6613f)));

            total = new List<int> { 0, 1, 2, 3, 4, 5 };
            random = new List<int>();
            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicSmallChests.Add(random[0], new SpawnCardStruct(new Vector3(-133.4579f, -25.3303f, -68.7228f), new Vector3(9.6318f, 138.4325f, 358.917f)));
            DicSmallChests.Add(random[1], new SpawnCardStruct(new Vector3(-135.1821f, -25.4367f, -70.568f), new Vector3(18.5215f, 90.3934f, 352.1391f)));
            DicSmallChests.Add(random[2], new SpawnCardStruct(new Vector3(-136.5309f, -25.5333f, -70.1181f), new Vector3(2.2523f, 116.0182f, 18.9676f)));
            DicSmallChests.Add(random[3], new SpawnCardStruct(new Vector3(-137.627f, -25.5271f, -69.0357f), new Vector3(15.987f, 225.1928f, 1.5787f)));
            DicSmallChests.Add(random[4], new SpawnCardStruct(new Vector3(-137.2342f, -25.4021f, -66.4729f), new Vector3(28.129f, 313.1049f, 21.9359f)));
            DicSmallChests.Add(random[5], new SpawnCardStruct(new Vector3(-140.2227f, -25.6923f, -67.7525f), new Vector3(9.4325f, 192.0851f, 17.5428f)));
        }
        #endregion

        #region 生成设备
        private void SpawnPrinters()
        {
            if (ModConfig.PrinterCount.Value > 0)
            {
                // 打印机
                DicPrinters.Clear();
                SetPrinter();
                int count = 0;
                if (ModConfig.SpawnCountByStage.Value) count = SetCountbyGameStage(ModConfig.PrinterCount.Value, ModConfig.SpawnCountOffset.Value);
                else count = ModConfig.PrinterCount.Value;
                for (int i = 0; i < count; i++)
                {
                    AsyncOperationHandle<InteractableSpawnCard> randomPrinter = GetRandomPrinter();
                    SpawnCard spawnCard = randomPrinter.WaitForCompletion();
                    GameObject printerOne = spawnCard.DoSpawn(DicPrinters[i].Position, Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectorPlacementRule, Run.instance.runRNG)).spawnedInstance;
                    printerOne.transform.eulerAngles = DicPrinters[i].Rotation;
                } 
            }
        }
        private void SpawnLunarCauldron()
        {
            if (ModConfig.CauldronCount.Value > 0)
            {
                // 大锅
                DicCauldrons.Clear();
                SetCauldron();
                int count = 0;
                if (ModConfig.SpawnCountByStage.Value) count = SetCountbyGameStage(ModConfig.CauldronCount.Value, ModConfig.SpawnCountOffset.Value);
                else count = ModConfig.CauldronCount.Value;
                for (int i = 0; i < count; i++)
                {
                    AsyncOperationHandle<GameObject> randomCauldron = GetRandomLunarCauldron();
                    GameObject gameObject = randomCauldron.WaitForCompletion();
                    gameObject = UnityEngine.Object.Instantiate<GameObject>(gameObject, DicCauldrons[i].Position, Quaternion.identity);
                    gameObject.transform.eulerAngles = DicCauldrons[i].Rotation;
                    NetworkServer.Spawn(gameObject);
                } 
            }
        }
        private void SpawnScrapper()
        {
            if (ModConfig.ScrapperCount.Value > 0)
            {
                // 收割机
                DicScrapers.Clear();
                SetScraper();
                DoSpawnCard(DicScrapers, iscScrapper, ModConfig.ScrapperCount.Value); 
            }
        }
        private void SpawnEquipment()
        {
            if (ModConfig.EquipmentCount.Value > 0)
            {
                // 主动装备
                DicEquipments.Clear();
                SetEquipment();
                DoSpawnGameObject(DicEquipments, multiShopEquipmentTerminal, ModConfig.EquipmentCount.Value); 
            }
        }
        private void SpawnLunarShopTerminal()
        {
            ObjectLunarShopTerminals.Clear();
            ObjectLunarShopTerminals_Spawn.Clear();
            LunarShopTerminalTotalCount = 0;
            if (ModConfig.LunarShopTerminalCount.Value > 0)
            {

                //foreach (GameObject @object in UnityEngine.Object.FindObjectsOfType<GameObject>())
                //{
                //    if (@object.name.StartsWith("LunarShopTerminal"))
                //    {
                //        PickupDisplay[] pickupDisplays = @object.GetComponentsInChildren<PickupDisplay>();
                //        foreach (PickupDisplay pickup in pickupDisplays)
                //        {
                //            pickup.modelRenderer.enabled = false;
                //        }
                //    }
                //}


                // 月球蓓蕾
                DicLunarShopTerminals.Clear();
                SetLunarShopTerminal();
                DoSpawnGameObject(DicLunarShopTerminals, lunarShopTerminal, ModConfig.LunarShopTerminalCount.Value);
            }
        }
        private void SpawnShrineRestack()
        {
            if (ModConfig.EnableShrineRestack.Value)
            {
                // 跌序
                SpawnCard spawnCard = iscShrineRestack.WaitForCompletion();
                GameObject shrinerestackOne = spawnCard.DoSpawn(new Vector3(-130f, -24f, -40f), Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectorPlacementRule, Run.instance.runRNG)).spawnedInstance;
                shrinerestackOne.transform.eulerAngles = new Vector3(0.0f, 220f, 0.0f);
                shrinerestackOne.GetComponent<ShrineRestackBehavior>().maxPurchaseCount = ModConfig.ShrineRestackMaxCount.Value;
                shrinerestackOne.GetComponent<PurchaseInteraction>().cost = ModConfig.ShrineRestackCost.Value;
                shrinerestackOne.GetComponent<PurchaseInteraction>().Networkcost = ModConfig.ShrineRestackCost.Value;
                if (ModConfig.PenaltyCoefficient_Temp != 1)
                {
                    shrinerestackOne.GetComponent<PurchaseInteraction>().cost *= ModConfig.PenaltyCoefficient_Temp;
                    shrinerestackOne.GetComponent<ShrineRestackBehavior>().costMultiplierPerPurchase = ModConfig.PenaltyCoefficient_Temp;
                }
            }
        }
        private void SpawnShrineCleanse()
        {
            if (ModConfig.EnableShrineCleanse.Value)
            {
                // 月池
                DicLunarPools.Clear();
                SetLunarPool();
                DoSpawnCard(DicLunarPools, iscShrineCleanse, DicLunarPools.Count); 
            }
        }
        private void SpawnShrineHealing()
        {
            if (ModConfig.EnableShrineHealing.Value)
            {
                // 木灵
                SpawnCard spawnCard = iscShrineHealing.WaitForCompletion();
                GameObject gameObject = spawnCard.DoSpawn(new Vector3(-119f, -23f, -52f), Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectorPlacementRule, Run.instance.runRNG)).spawnedInstance;
                gameObject.transform.eulerAngles = new Vector3(0.0f, 210f, 0.0f);
                gameObject.GetComponent<PurchaseInteraction>().costType = CostTypeIndex.LunarCoin;
                gameObject.GetComponent<PurchaseInteraction>().cost = ModConfig.PrayCost.Value * ModConfig.PenaltyCoefficient_Temp;
                gameObject.GetComponent<PurchaseInteraction>().Networkcost = ModConfig.PrayCost.Value * ModConfig.PenaltyCoefficient_Temp;

                //gameObject.GetComponent<ShrineHealingBehavior>().baseRadius = 80;
                //gameObject.GetComponent<ShrineHealingBehavior>().radiusBonusPerPurchase = 0;
                gameObject.GetComponent<ShrineHealingBehavior>().costMultiplierPerPurchase = 1;
                gameObject.GetComponent<ShrineHealingBehavior>().maxPurchaseCount = int.MaxValue; 
            }
        }
        private void SpawnDecorate()
        {
            if (ModConfig.EnableDecorate.Value)
            {
                // 箱子
                DicGlodChests.Clear();
                DicBigChests.Clear();
                DicSmallChests.Clear();
                SetDecorate();
                DoSpawnCard(DicGlodChests, iscGoldChest, DicGlodChests.Count);
                DoSpawnCard(DicBigChests, iscChest2, DicBigChests.Count);
                DoSpawnCard(DicSmallChests, iscChest1, DicSmallChests.Count); 
            }

        }
        private void SpawnBluePortal()
        {
            if (ModConfig.EnableDecorate.Value)
            {
                // 传送门
                SpawnCard spawnCard = iscShopPortal.WaitForCompletion();
                GameObject gameObject = spawnCard.DoSpawn(new Vector3(-135f, -23f, -60f), Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectorPlacementRule, Run.instance.runRNG)).spawnedInstance;
                gameObject.transform.eulerAngles = new Vector3(0.0f, 220f, 0.0f); 
            }
        }
        #endregion

        private void DoSpawnCard(Dictionary<int, SpawnCardStruct> keyValuePairs, AsyncOperationHandle<InteractableSpawnCard> card, int max)
        {
            int count = 0;
            if (ModConfig.SpawnCountByStage.Value) count = SetCountbyGameStage(max, ModConfig.SpawnCountOffset.Value);
            else count = max;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    SpawnCard spawnCard = card.WaitForCompletion();
                    GameObject gameObject = spawnCard.DoSpawn(keyValuePairs[i].Position, Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectorPlacementRule, Run.instance.runRNG)).spawnedInstance;
                    gameObject.transform.eulerAngles = keyValuePairs[i].Rotation;
                    gameObject.transform.localScale = (Vector3)keyValuePairs[i].Scale;

                    if (card.LocationName.EndsWith("iscGoldChest.asset") || card.LocationName.EndsWith("iscChest1.asset") || card.LocationName.EndsWith("iscChest2.asset"))
                    {
                        gameObject.GetComponent<PurchaseInteraction>().SetAvailable(false);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"{card} 出现问题了");
                }
            }
        }
        private void DoSpawnCard(string card, Vector3 vector)
        {
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>(name);
            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random
            };
            GameObject obj = spawnCard.DoSpawn(vector, Quaternion.identity, new DirectorSpawnRequest(spawnCard, placementRule, Run.instance.runRNG)).spawnedInstance;
            obj.transform.eulerAngles = default; 

        }
        private void DoSpawnGameObject(Dictionary<int, SpawnCardStruct> keyValuePairs, AsyncOperationHandle<GameObject> card, int max)
        {

            int count = 0;
            if (ModConfig.SpawnCountByStage.Value)
            {
                count = SetCountbyGameStage(max, ModConfig.SpawnCountOffset.Value);
                if (card.LocationName.EndsWith("LunarShopTerminal.prefab"))
                {
                    LunarShopTerminalTotalCount = count;
                    if (count <= 5)
                    {
                        return;
                    }
                    else
                    {
                        count = LunarShopTerminalTotalCount - 5;
                    }
                }
            }
            else
            {
                count = max;
                if (card.LocationName.EndsWith("LunarShopTerminal.prefab"))
                {
                    LunarShopTerminalTotalCount = max;
                    count = count - 5;
                }
            }
            for (int i = 0; i < count; i++)
            {
                GameObject gameObject = Instantiate(card.WaitForCompletion(), keyValuePairs[i].Position, Quaternion.identity);
                gameObject.transform.eulerAngles = keyValuePairs[i].Rotation;
                gameObject.transform.localScale = (Vector3)keyValuePairs[i].Scale;
                if (card.LocationName.EndsWith("LunarShopTerminal.prefab"))
                {
                    ObjectLunarShopTerminals_Spawn.Add(gameObject.GetComponent<PurchaseInteraction>());
                    if (ModConfig.EnableLunarShopTerminalInjection.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        int cost = 2;
                        int total = cost * ModConfig.PenaltyCoefficient_Temp;
                        if (ModConfig.EnableLunarShopTerminalInjection.Value)
                        {
                            total = ModConfig.LunarShopTerminalCost.Value * ModConfig.PenaltyCoefficient_Temp;
                        }
                        //gameObject.name = "MyLunarBud";
                        gameObject.GetComponent<PurchaseInteraction>().cost = total;
                        gameObject.GetComponent<PurchaseInteraction>().Networkcost = total; 
                    }
                }
                NetworkServer.Spawn(gameObject);
            }
        }

        [ConCommand(commandName = "spawn_card", flags = ConVarFlags.ExecuteOnServer, helpText = "生成实物")]
        private static void Command_SpawnCard(ConCommandArgs args)
        {
            //Inventory inventory = args.sender?.master.inventory;
            string name = args.GetArgString(0);
            NetworkUser player = PlayerCharacterMasterController.instances[0].networkUser;
            ChatHelper.Send($"name = {name}, DisplayName = {player.masterController.GetDisplayName()}");
            Vector3 vector = player.GetCurrentBody().footPosition;

            SpawnCard card = LegacyResourcesAPI.Load<SpawnCard>(name);
            DirectorPlacementRule pr2 = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct
            };
            GameObject obj = card.DoSpawn(vector, Quaternion.identity, new DirectorSpawnRequest(card, pr2, Run.instance.runRNG)).spawnedInstance;
            obj.transform.eulerAngles = new Vector3(0.0f, 220f, 0.0f);
        }
        [ConCommand(commandName = "spawn_object", flags = ConVarFlags.ExecuteOnServer, helpText = "生成实体")]
        private static void Command_GameObject(ConCommandArgs args)
        {
            //Inventory inventory = args.sender?.master.inventory;
            string name = args.GetArgString(0);
            NetworkUser player = PlayerCharacterMasterController.instances[0].networkUser;
            ChatHelper.Send($"name = {name}, DisplayName = {player.masterController.GetDisplayName()}");
            Vector3 vector = player.GetCurrentBody().footPosition;

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>(name), vector, Quaternion.identity); ;
            gameObject.transform.eulerAngles = new Vector3(0.0f, 220f, 0.0f);
            NetworkServer.Spawn(gameObject);
        }
        [ConCommand(commandName = "play_effect", flags = ConVarFlags.ExecuteOnServer, helpText = "生成特效")]
        private static void Command_PlayEffect(ConCommandArgs args)
        {
            string name = args.GetArgString(0);
            NetworkUser player = PlayerCharacterMasterController.instances[0].networkUser;
            Vector3 vector = player.GetCurrentBody().corePosition;

            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>(name), new EffectData()
            {
                origin = vector,
                rotation = Quaternion.identity,
                scale = 1f,
                color = Color.yellow
            }, true);
        }

        private bool IsMultiplayer()
        {
            return PlayerCharacterMasterController.instances.Count > 1;
        }
        private bool IsCurrentMapInBazaar()
        {
            return SceneManager.GetActiveScene().name == "bazaar";
        }
        private AsyncOperationHandle<InteractableSpawnCard> GetRandomPrinter()
        {
            float tier1 = ModConfig.PrinterTier1Weight.Value;
            float tier2 = ModConfig.PrinterTier2Weight.Value;
            float tier3 = ModConfig.PrinterTier3Weight.Value;
            float boss = ModConfig.PrinterTierBossWeight.Value;
            float total = tier1 + tier2 + tier3 + boss;
            double d = Random.NextDouble() * total;
            if (d <= tier1) return PrintersCode[0];
            else if (d <= tier1 + tier2) return PrintersCode[1];
            else if (d <= tier1 + tier2 + tier3) return PrintersCode[2];
            else return PrintersCode[3];
        }
        private AsyncOperationHandle<GameObject> GetRandomLunarCauldron()
        {
            float w_g = ModConfig.CauldronGreenWeight.Value;
            float g_r = ModConfig.CauldronRedWeight.Value;
            float g_w = ModConfig.CauldronWhiteWeight.Value;
            float total = w_g + g_r + g_w;
            double d = Random.NextDouble() * total;
            if (d <= w_g) return LunarCauldronsCode[0];
            else if (d <= w_g + g_r) return LunarCauldronsCode[1];
            else { return LunarCauldronsCode[2]; }
        }
        private void GetRandomLunarCauldron_DLC1()
        {
            int total = 0;
            if (ModConfig.CauldronGreenWeight.Value != 0f) total++;
            if (ModConfig.CauldronRedWeight.Value != 0f) total++;
            if (ModConfig.CauldronWhiteWeight.Value != 0f) total++;
            if (ModConfig.CauldronYellowWeight.Value != 0f) total++;
            if (ModConfig.CauldronBlueWeight.Value != 0f) total++;
            if (ModConfig.CauldronPurpleWeight.Value != 0f) total++;

        }

        private int SetCountbyGameStage(int max, int offset = 0)
        {
            int stageCount = Run.instance.stageClearCount + 1;
            int set = stageCount + offset;
            if (set > max)
            {
                set = max;
            }
            return set;
        }
        private void SetCaudronList_Hacked()
        {
            if (ModConfig.CauldronCount.Value > 0)
            {
                if (ModConfig.EnableCauldronHacking.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                {
                    CauldronHackedStructs.Clear();
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronGreen-Yellow", ModConfig.CauldronWhiteToGreenCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.WhiteItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronGreen-Blue", ModConfig.CauldronWhiteToGreenCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.WhiteItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronGreen-Purple", ModConfig.CauldronWhiteToGreenCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.WhiteItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronRed-Yellow", ModConfig.CauldronGreenToRedCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.GreenItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronRed-Blue", ModConfig.CauldronGreenToRedCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.GreenItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronRed-Purple", ModConfig.CauldronGreenToRedCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.GreenItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronWhite-Yellow", ModConfig.CauldronRedToWhiteCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.BossItem));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronWhite-Blue", ModConfig.CauldronRedToWhiteCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.LunarItemOrEquipment));
                    CauldronHackedStructs.Add(new CauldronHackedStruct("LunarCauldronWhite-Purple", ModConfig.CauldronRedToWhiteCost_Hacked.Value * ModConfig.PenaltyCoefficient_Temp, CostTypeIndex.RedItem));
                } 
            }
        }

        private static void SpawnEffect(AsyncOperationHandle<GameObject> effect, Vector3 position, Color32 color, float scale = 1f)
        {
            EffectManager.SpawnEffect(effect.WaitForCompletion(), new EffectData()
            {
                origin = position,
                rotation = Quaternion.identity,
                scale = scale,
                color = color
            }, true);
        }

        internal class SpawnCardStruct
        {
            public SpawnCardStruct(Vector3 position, Vector3 rotation, Vector3? scale = null)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale ?? new Vector3(1, 1, 1);
            }

            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }
            public Vector3? Scale { get; set; }
        }
        internal class PlayerStruct
        {
            public PlayerStruct(NetworkUser networkUser, int donateCount, int rewardCount = 0)
            {
                NetworkUser = networkUser;
                DonateCount = donateCount;
                RewardCount = rewardCount;
            }

            public NetworkUser NetworkUser { get; set; }
            public int DonateCount { get; set; }
            public int RewardCount { get; set; }
        }
        internal class SpecialItemStruct
        {
            public SpecialItemStruct(string name, int count, bool isUse = false)
            {
                Name = name;
                Count = count;
                IsUse = isUse;
            }

            public string Name { get; set; }
            public int Count { get; set; }
            public bool IsUse { get; set; }
        }
        internal class CauldronHackedStruct
        {
            public CauldronHackedStruct(string name, int cost, CostTypeIndex costTypeIndex)
            {
                Name = name;
                Cost = cost;
                CostTypeIndex = costTypeIndex;
            }

            public string Name { get; set; }
            public int Cost { get; set; }
            public CostTypeIndex CostTypeIndex { get; set; }
        }
    }
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

        public static BaseUnityPlugin ShareSuite;
        public static BaseUnityPlugin BiggerBazaar;

        public static void InitConfig(ConfigFile config)
        {
            EnableMod = config.Bind("00 Setting设置", "EnableMod", true, "Enable Mod\n启用模组");
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

        public static bool IsShareSuite_PrinterCauldronFixEnabled()
        {
            if (!(ShareSuite is null))
            {
                return ShareSuite.GetFieldValue<ConfigEntry<bool>>("PrinterCauldronFixEnabled").Value;
            }
            return false;
        }

    }

    public static class Tokens
    {
        internal static string LanguageRoot
        {
            get
            {
                return System.IO.Path.Combine(AssemblyDir, "Language");
            }
        }

        internal static string AssemblyDir
        {
            get
            {
                return System.IO.Path.GetDirectoryName(BazaarIsMyHome.PluginInfo.Location);
            }
        }
        public static void RegisterLanguageTokens()
        {
            On.RoR2.Language.SetFolders += Language_SetFolders;
        }

        private static void Language_SetFolders(On.RoR2.Language.orig_SetFolders orig, Language self, IEnumerable<string> newFolders)
        {
            if (Directory.Exists(LanguageRoot))
            {
                IEnumerable<string> second = Directory.EnumerateDirectories(System.IO.Path.Combine(new string[]
                {
                    LanguageRoot
                }), self.name);
                orig.Invoke(self, newFolders.Union(second));
            }
            else
            {
                orig.Invoke(self, newFolders);
            }
        }
    }
}