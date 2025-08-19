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

namespace BazaarIsMyHaven
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.KingEnderBrine.InLobbyConfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.funkfrog_sipondo.sharesuite", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInIncompatibility("com.Lunzir.BazaarLunarForEveryone")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Main : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo;
        public static ItemHandler ItemHandler;
        public static Main instance;

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Def";
        public const string PluginName = "BazaarIsMyHaven";
        public const string PluginVersion = "1.0.0";

        List<BazaarBase> bazaarMods = new List<BazaarBase>();
        private readonly Dictionary<PlayerCharacterMasterController, PlayerStruct> playerStructs_ = new Dictionary<PlayerCharacterMasterController, PlayerStruct>();

        public PlayerStruct GetPlayerStruct(PlayerCharacterMasterController master)
        {
            if(!playerStructs_.ContainsKey(master)) {
                playerStructs_.Add(master, new PlayerStruct(master));
            }
            return playerStructs_[master];
         }

        public void Awake()
        {
            PluginInfo = Info;
            instance = this;
            Log.Init(Logger);
            ModConfig.InitConfig(Config);
            Tokens.RegisterLanguageTokens();

            bazaarMods.Add(new BazaarCauldron());
            bazaarMods.Add(new BazaarPrinter());
            bazaarMods.Add(new BazaarRestack());
            bazaarMods.Add(new BazaarPrayer());
            bazaarMods.Add(new BazaarScrapper());
            bazaarMods.Add(new BazaarEquipment());
            bazaarMods.Add(new BazaarLunarShop());
            bazaarMods.Add(new BazaarCleansingPool());

            foreach (var bazaarMod in bazaarMods)
            {
                bazaarMod.Init();
            }
            foreach (var bazaarMod in bazaarMods)
            {
                bazaarMod.Hook();
            }

            var instancedPurchases = new InstancedPurchases();
            instancedPurchases.Hook();

            ItemHandler = new ItemHandler();
            On.RoR2.Run.Start += Run_Start;
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
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            ShopKeep.DiedAtLeastOnce = false;
            ShopKeep.Body = null;
            ItemHandler = new ItemHandler();
            orig(self);
        }


        private void BazaarController_SetUpSeerStations(On.RoR2.BazaarController.orig_SetUpSeerStations orig, BazaarController self)
        {
            if (ModConfig.EnableMod.Value)
            {
                if (ModConfig.LunarSeerSectionEnabled.Value)
                {
                    foreach (SeerStationController seerStationController in self.seerStations)
                    {
                        seerStationController.GetComponent<PurchaseInteraction>().available = ModConfig.SeerStationAvailable.Value && !ModConfig.ReplaceLunarSeersWithEquipment.Value;
                        if (ModConfig.SeerStationAvailable.Value)
                        {
                            seerStationController.GetComponent<PurchaseInteraction>().cost = ModConfig.SeerStationsCost.Value;
                            seerStationController.GetComponent<PurchaseInteraction>().Networkcost = ModConfig.SeerStationsCost.Value; 
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
                ShopKeep.DeathCount = 0;
                playerStructs_.Clear();

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
                    foreach (var bazaarMod in bazaarMods)
                    {
                        bazaarMod.SetupBazaar();
                    }
                    if (isEnableSacrifice) RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, true);
                    #endregion
                }
            }
            orig(self);
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
            orig(self);
            if (ModConfig.EnableMod.Value && ModConfig.EnableAutoOpenShop.Value)
            {
                self.shouldAttemptToSpawnShopPortal = true;
            }
        }
        private void KickFromShop_FixedUpdate(On.EntityStates.NewtMonster.KickFromShop.orig_FixedUpdate orig, EntityStates.NewtMonster.KickFromShop self)
        {
            if (ModConfig.EnableMod.Value && ModConfig.EnableNoKickFromShop.Value)
            {
                if (ModConfig.NewtSecondLifeMode.Value == ShopKeep.DeathState.Evil)
                {
                    // TODO
                }
                else
                {
                }
                self.outer.SetNextStateToMain();
            }
            else
            {
                orig(self);
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
                            orig(self, damageInfo, hitObject);
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                    return;
                }
            }
            orig(self, damageInfo, hitObject);

        }
        private void CharacterMaster_OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                //var attack = self.name;
                var victim = body.name;
                //ChatHelper.Send($"victim = {victim}");
                if (victim.Contains("ShopkeeperBody"))
                {
                    ShopKeep.DiedAtLeastOnce = true;
                    //ChatHelper.SpawnTime_Record++;
                    ShopKeep.DeathCount++;
                    if (ModConfig.NewtSecondLifeMode.Value == ShopKeep.DeathState.Default)
                    {
                        if(ModConfig.EnableLines.Value)
                        {
                            ChatHelper.ShowNewtDeath();
                        }
                        //string card = "Spawncards/InteractableSpawncard/iscScavLunarbackpack";
                        //DoSpawnCard(card, new Vector3(-122.7888f, -22.3505f, -45.7878f));
                        //DoSpawnCard(card, new Vector3(-125.9958f, -22.4272f, -42.6213f));
                        //Vector3 vector3 = body.footPosition;
                        //ChatHelper.Send($"死亡位置：{vector3.x},{vector3.y},{vector3.z}");
                    }
                    else
                    {
                        AddItemToShopKeeper(body);
                    }
                }
            }
        }
        private void AddItemToShopKeeper(CharacterBody body)
        {
            switch (ModConfig.NewtSecondLifeMode.Value)
            {
                case ShopKeep.DeathState.Ghost:
                    ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Ghost")), 1);
                    ShopKeep.Body.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                    break;
                case ShopKeep.DeathState.Tank:
                    var healthBoost = 10 * (int)Math.Pow(2, ShopKeep.DeathCount) - 10 * (int)Math.Pow(2, ShopKeep.DeathCount - 1);
                    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BoostHp")), healthBoost);
                    break;
                case ShopKeep.DeathState.Evil:
                    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Thorns")), 1);
                    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BurnNearby")), 1);
                    break;
                default:
                    break;
            }
            //if (ModConfig.NewtSecondLifeMode.Value == ShopKeep.DeathState.Evil)
            //{
            
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StrengthenBurn")), 1);


            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("HealingPotion")), 1000);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Thorns")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Clover")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Medkit")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("NearbyDamageBonus")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StickyBomb")), 50);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("StunChanceOnHit")), 50);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("CaptainDefenseMatrix")), 1);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ExplodeOnDeath")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("SlowOnHit")), 10);
                
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ImmuneToDebuff")), 1);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BearVoid")), 1);
                
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("InvadingDoppelganger")), 1);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("AutoCastEquipment")), 5);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("EquipmentMagazine")), 10);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("CritGlasses")), 20);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Syringe")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Phasing")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ParentEgg")), 10000);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("BarrierOnOverHeal")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("IncreaseHealing")), 100);
            //    body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Crowbar")), 10);
            //    body.inventory.SetEquipmentIndex(EquipmentCatalog.FindEquipmentIndex("EliteVoidEquipment"));
            //}
        }

        private void SpawnState_OnEnter(On.EntityStates.NewtMonster.SpawnState.orig_OnEnter orig, EntityStates.NewtMonster.SpawnState self)
        {
            try
            {
                if (ModConfig.EnableMod.Value)
                {
                    //ChatHelper.Send("进入SpawnState_OnEnter");
                    // 欢迎语
                    StartCoroutine(ShopWelcomeWord());

                    if (ShopKeep.Body is null) FindShopkeeper();

                    if (ModConfig.NewtSecondLifeMode.Value != ShopKeep.DeathState.Default)
                    {
                        ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ExtraLife")), 1000);
                        ShopKeep.Body.inventory.GiveItem(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("CutHp")), 200);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex);
            }
            orig(self);
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