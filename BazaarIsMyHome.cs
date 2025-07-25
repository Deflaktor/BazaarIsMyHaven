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
        public static BazaarIsMyHome instance;
        private System.Random RNG = new System.Random();
        private readonly DirectorPlacementRule DirectPlacement = new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.Direct
        };

        Dictionary<int, SpawnCardStruct> DicEquipments = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicLunarPools = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicLunarShopTerminals = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicGlodChests = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicBigChests = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicSmallChests = new Dictionary<int, SpawnCardStruct>();

        int LunarShopTerminalTotalCount = 0;
        List<PurchaseInteraction> ObjectLunarShopTerminals = new List<PurchaseInteraction>();
        List<PurchaseInteraction> ObjectLunarShopTerminals_Spawn = new List<PurchaseInteraction>();

        AsyncOperationHandle<InteractableSpawnCard> iscChest1;
        AsyncOperationHandle<InteractableSpawnCard> iscChest2;
        AsyncOperationHandle<InteractableSpawnCard> iscGoldChest;
        
        AsyncOperationHandle<InteractableSpawnCard> iscShopPortal;
        AsyncOperationHandle<InteractableSpawnCard> iscShrineCleanse;
        AsyncOperationHandle<InteractableSpawnCard> iscDeepVoidPortalBattery;
        AsyncOperationHandle<GameObject> lunarShopTerminal;
        AsyncOperationHandle<GameObject> multiShopEquipmentTerminal;

        AsyncOperationHandle<GameObject> LunarRerollEffect;
        AsyncOperationHandle<GameObject> TeleporterBeaconEffect;

        BazaarCauldron bazaarCauldron;
        BazaarPrinter bazaarPrinter;
        BazaarRestack bazaarRestack;
        BazaarPrayer bazaarPrayer;
        BazaarScrapper bazaarScrapper;


        public void Awake()
        {
            instance = this;
            Log.Init(Logger);
            ModConfig.InitConfig(Config);

            bazaarCauldron = new BazaarCauldron();
            bazaarCauldron.Init();
            bazaarPrinter = new BazaarPrinter();
            bazaarPrinter.Init();
            bazaarRestack = new BazaarRestack();
            bazaarRestack.Init();
            bazaarPrayer = new BazaarPrayer();
            bazaarPrayer.Init();
            bazaarScrapper = new BazaarScrapper();
            bazaarScrapper.Init();

            bazaarCauldron.Hook();
            bazaarPrinter.Hook();
            bazaarRestack.Hook();
            bazaarPrayer.Hook();
            bazaarScrapper.Hook();

            // --- preload stuff ---

            iscChest1 = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Chest1/iscChest1.asset");
            iscChest2 = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Chest2/iscChest2.asset");
            iscGoldChest = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/GoldChest/iscGoldChest.asset");
            
            iscShopPortal = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/PortalShop/iscShopPortal.asset");
            iscShrineCleanse = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset");

            lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
            multiShopEquipmentTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MultiShopEquipmentTerminal/MultiShopEquipmentTerminal.prefab");

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

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            ShopKeep.IsDeath = false;
            ShopKeep.Body = null;
            ItemHandler = new ItemHandler();
            orig(self);
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {

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
                ShopKeep.SpawnTime_Record = 0;
                ModConfig.RerolledCount = 0;

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
                    bazaarPrinter.EnterBazaar(); // 打印机
                    bazaarCauldron.EnterBazaar(); // 大锅
                    bazaarScrapper.EnterBazaar(); // 收割机
                    SpawnEquipment(); // 主动装备
                    SpawnLunarShopTerminal(); // 月球蓓蕾
                    SpawnShrineCleanse(); // 月池
                    bazaarRestack.EnterBazaar(); // 跌序
                    bazaarPrayer.EnterBazaar();
                    SpawnDecorate(); // 装饰
                    if (isEnableSacrifice) RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, true);
                    #endregion
                }
            }
            orig.Invoke(self);
        }


        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            //ChatHelper.Send("PurchaseInteraction_Awake");
            orig(self);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
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

        private void SetEquipment()
        {
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
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
            const float droneTableOrientation = 160f;

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
                int index = RNG.Next(total.Count);
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
                int index = RNG.Next(total.Count);
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
                int index = RNG.Next(total.Count);
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
                GameObject gameObject = spawnCard.DoSpawn(new Vector3(-135f, -23f, -60f), Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectPlacement, Run.instance.runRNG)).spawnedInstance;
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
                    GameObject gameObject = spawnCard.DoSpawn(keyValuePairs[i].Position, Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectPlacement, Run.instance.runRNG)).spawnedInstance;
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
                GameObject gameObject = GameObject.Instantiate(card.WaitForCompletion(), keyValuePairs[i].Position, Quaternion.identity);
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
        private void SpawnEffect(AsyncOperationHandle<GameObject> effect, Vector3 position, Color32 color, float scale = 1f)
        {
            EffectManager.SpawnEffect(effect.WaitForCompletion(), new EffectData()
            {
                origin = position,
                rotation = Quaternion.identity,
                scale = scale,
                color = color
            }, true);
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
        private bool IsCurrentMapInBazaar()
        {
            return SceneManager.GetActiveScene().name == "bazaar";
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
    }
}