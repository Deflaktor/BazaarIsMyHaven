using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace BazaarIsMyHaven
{
    public abstract class BazaarBase
    {
        protected System.Random RNG = new System.Random();
        protected readonly DirectorPlacementRule DirectPlacement = new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.Direct
        };

        protected static Dictionary<string, AsyncOperationHandle<BasicPickupDropTable>> dropTables = new Dictionary<string, AsyncOperationHandle<BasicPickupDropTable>>();

        public static void InitBazaarBaseDropTables()
        {
            dropTables.Clear();
            dropTables.Add("dtMonsterTeamTier1Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/MonsterTeamGainsItems/dtMonsterTeamTier1Item.asset"));
            dropTables.Add("dtMonsterTeamTier2Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/MonsterTeamGainsItems/dtMonsterTeamTier2Item.asset"));
            dropTables.Add("dtMonsterTeamTier3Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/MonsterTeamGainsItems/dtMonsterTeamTier3Item.asset"));
            dropTables.Add("dtSacrificeArtifact", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Sacrifice/dtSacrificeArtifact.asset"));
            dropTables.Add("dtAISafeTier1Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtAISafeTier1Item.asset"));
            dropTables.Add("dtAISafeTier2Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtAISafeTier2Item.asset"));
            dropTables.Add("dtAISafeTier3Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtAISafeTier3Item.asset"));
            dropTables.Add("dtEquipment", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtEquipment.asset"));
            dropTables.Add("dtTier1Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier1Item.asset"));
            dropTables.Add("dtTier2Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier2Item.asset"));
            dropTables.Add("dtTier3Item", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier3Item.asset"));
            dropTables.Add("dtVoidChest", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtVoidChest.asset"));
            dropTables.Add("dtCasinoChest", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/CasinoChest/dtCasinoChest.asset"));
            dropTables.Add("dtSmallChestDamage", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/CategoryChest/dtSmallChestDamage.asset"));
            dropTables.Add("dtSmallChestHealing", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/CategoryChest/dtSmallChestHealing.asset"));
            dropTables.Add("dtSmallChestUtility", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/CategoryChest/dtSmallChestUtility.asset"));
            dropTables.Add("dtChest1", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Chest1/dtChest1.asset"));
            dropTables.Add("dtChest2", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Chest2/dtChest2.asset"));
            dropTables.Add("dtDuplicatorTier1", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Duplicator/dtDuplicatorTier1.asset"));
            dropTables.Add("dtDuplicatorTier2", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/DuplicatorLarge/dtDuplicatorTier2.asset"));
            dropTables.Add("dtDuplicatorTier3", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/DuplicatorMilitary/dtDuplicatorTier3.asset"));
            dropTables.Add("dtDuplicatorWild", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/DuplicatorWild/dtDuplicatorWild.asset"));
            dropTables.Add("dtGoldChest", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/GoldChest/dtGoldChest.asset"));
            dropTables.Add("dtLunarChest", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/LunarChest/dtLunarChest.asset"));
            dropTables.Add("dtShrineChance", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/ShrineChance/dtShrineChance.asset"));
            dropTables.Add("dtLockbox", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/TreasureCache/dtLockbox.asset"));
            dropTables.Add("dtITBossWave", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/dtITBossWave.asset"));
            dropTables.Add("dtITDefaultWave", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/dtITDefaultWave.asset"));
            dropTables.Add("dtITLunar", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/dtITLunar.asset"));
            dropTables.Add("dtITSpecialBossWave", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/dtITSpecialBossWave.asset"));
            dropTables.Add("dtITVoid", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/dtITVoid.asset"));
            dropTables.Add("dtCategoryChest2Damage", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/CategoryChest2/dtCategoryChest2Damage.asset"));
            dropTables.Add("dtCategoryChest2Healing", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/CategoryChest2/dtCategoryChest2Healing.asset"));
            dropTables.Add("dtCategoryChest2Utility", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/CategoryChest2/dtCategoryChest2Utility.asset"));
            dropTables.Add("dtVoidCamp", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/VoidCamp/dtVoidCamp.asset"));
            dropTables.Add("dtVoidTriple", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/VoidTriple/dtVoidTriple.asset"));
            dropTables.Add("dtVoidLockbox", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/TreasureCacheVoid/dtVoidLockbox.asset"));
            dropTables.Add("AurelioniteHeartPickupDropTable", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC2/AurelioniteHeartPickupDropTable.asset"));
            dropTables.Add("GeodeRewardDropTable", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC2/GeodeRewardDropTable.asset"));
            dropTables.Add("dtShrineHalcyoniteTier1", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC2/dtShrineHalcyoniteTier1.asset"));
            dropTables.Add("dtShrineHalcyoniteTier2", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC2/dtShrineHalcyoniteTier2.asset"));
            dropTables.Add("dtShrineHalcyoniteTier3", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC2/dtShrineHalcyoniteTier3.asset"));
            dropTables.Add("dtChanceDoll", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC2/Items/ExtraShrineItem/dtChanceDoll.asset"));
            dropTables.Add("dtSonorousEcho", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC2/Items/ItemDropChanceOnKill/dtSonorousEcho.asset"));
            dropTables.Add("dtCommandChest", Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/CommandChest/dtCommandChest.asset"));
        }

        public static void WriteDropTablesMarkdownFile()
        {
#if DEBUG
            string filePath = $"{Main.PluginName}_droptables.md";

            // This will write it next to the RiskOfRain2.exe file
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine("|                                 | canDropBeReplaced | requiredItemTags | bannedItemTags | tier1Weight | tier2Weight | tier3Weight | bossWeight | lunarEquipmentWeight | lunarItemWeight | lunarCombinedWeight | equipmentWeight | voidTier1Weight | voidTier2Weight | voidTier3Weight | voidBossWeight |");
                writer.WriteLine("|---------------------------------|-------------------|------------------|----------------|-------------|-------------|-------------|------------|----------------------|-----------------|---------------------|-----------------|-----------------|-----------------|-----------------|----------------|");
                foreach (var entry in dropTables)
                {
                    var dropTableName = entry.Key;
                    var dropTable = entry.Value.WaitForCompletion();
                    string canDropBeReplaced = dropTable.canDropBeReplaced.ToString();
                    string requiredItemTags = string.Join(", ", dropTable.requiredItemTags.Select(e => e.ToString()));
                    string bannedItemTags = string.Join(", ", dropTable.bannedItemTags.Select(e => e.ToString()));
                    string tier1Weight = dropTable.tier1Weight.ToString();
                    string tier2Weight = dropTable.tier2Weight.ToString();
                    string tier3Weight = dropTable.tier3Weight.ToString();
                    string bossWeight = dropTable.bossWeight.ToString();
                    string lunarEquipmentWeight = dropTable.lunarEquipmentWeight.ToString();
                    string lunarItemWeight = dropTable.lunarItemWeight.ToString();
                    string lunarCombinedWeight = dropTable.lunarCombinedWeight.ToString();
                    string equipmentWeight = dropTable.equipmentWeight.ToString();
                    string voidTier1Weight = dropTable.voidTier1Weight.ToString();
                    string voidTier2Weight = dropTable.voidTier2Weight.ToString();
                    string voidTier3Weight = dropTable.voidTier3Weight.ToString();
                    string voidBossWeight = dropTable.voidBossWeight.ToString();

                    writer.WriteLine($"| {dropTableName} | {canDropBeReplaced} | {requiredItemTags} | {bannedItemTags} | {tier1Weight} | {tier2Weight} | {tier3Weight} | {bossWeight} | {lunarEquipmentWeight} | {lunarItemWeight} | {lunarCombinedWeight} | {equipmentWeight} | {voidTier1Weight} | {voidTier2Weight} | {voidTier3Weight} | {voidBossWeight} |");
                }
            }
#endif
        }

        public abstract void Preload();
        public abstract void Hook();
        public abstract void SetupBazaar();

        /// <summary>
        /// Takes as input a string consisting of a comma-separated list of itemkey=amount. Resolves and chooses one of them into a pickupindex and count struct.
        /// If an itemkey cannot be resolved to a pickupindex, it will be skipped and next viable one will be picked.
        /// </summary>
        /// <param name="itemlist">Comma-separated list of itemkey=amount</param>
        /// <param name="index">If index is given, the concrete index of the list is given. If not given or negative, a random one will be picked.</param>
        /// <returns>null if there was an issue resolving to a proper pickup index. ItemRewardStruct otherwise.</returns>
        protected PickupIndex[] ResolveItemRewardFromStringList(string itemlist, int index = -1)
        {
            if(itemlist.IsNullOrWhiteSpace())
            {
                return null;
            }
            List<string> itemKeyCountStrings = itemlist.Split(',').ToList();
            PickupIndex pickupIndex = PickupIndex.none;
            int amount = 1;

            TryAgain:

            if (itemKeyCountStrings.Count > 0)
            {
                index = index % itemKeyCountStrings.Count;
                if(index < 0)
                {
                    index = RNG.Next(itemKeyCountStrings.Count);
                }
                var entry = itemKeyCountStrings[index].Trim().Split("=");
                string itemkey;
                if (entry.Length == 1)
                {
                    itemkey = entry[0];
                }
                else if (entry.Length == 2)
                {
                    itemkey = entry[0].Trim();
                    if(!int.TryParse(entry[1].Trim(), out amount))
                    {
                        Log.LogError($"Could not properly parse item amount: {entry}");
                    }
                }
                else
                {
                    Log.LogError($"Could not properly parse item key: {entry}");
                    itemkey = entry[0];
                }

                var isItemTier = true;
                switch (itemkey.ToLower())
                {
                    case "tier1":
                        pickupIndex = GetRandom(Run.instance.availableTier1DropList, PickupIndex.none);
                        break;
                    case "tier2":
                        pickupIndex = GetRandom(Run.instance.availableTier2DropList, PickupIndex.none);
                        break;
                    case "tier3":
                        pickupIndex = GetRandom(Run.instance.availableTier3DropList, PickupIndex.none);
                        break;
                    case "boss":
                        pickupIndex = GetRandom(Run.instance.availableBossDropList, PickupIndex.none);
                        break;
                    case "lunar":
                        pickupIndex = GetRandom(Run.instance.availableLunarCombinedDropList, PickupIndex.none);
                        break;
                    case "voidtier1":
                        pickupIndex = GetRandom(Run.instance.availableVoidTier1DropList, PickupIndex.none);
                        break;
                    case "voidtier2":
                        pickupIndex = GetRandom(Run.instance.availableVoidTier2DropList, PickupIndex.none);
                        break;
                    case "voidtier3":
                        pickupIndex = GetRandom(Run.instance.availableVoidTier3DropList, PickupIndex.none);
                        break;
                    case "voidboss":
                        pickupIndex = GetRandom(Run.instance.availableVoidBossDropList, PickupIndex.none);
                        break;
                    default:
                        isItemTier = false;
                        break;
                }
                if (isItemTier)
                {
                    if (pickupIndex == PickupIndex.none)
                    {
                        Log.LogWarning($"LunarShopItemsList: Could not get pickup from item tier: {itemkey}, skipping it.");
                        itemKeyCountStrings.RemoveAt(index);
                        goto TryAgain;
                    }
                }
                else
                {
                    if (dropTables.ContainsKey(itemkey))
                    {
                        var dropTable = dropTables[itemkey].WaitForCompletion();
                        return dropTable.GenerateUniqueDrops(amount, new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));
                    }
                    else
                    {
                        ItemIndex itemIndex = ItemCatalog.FindItemIndex(itemkey);
                        if (itemIndex != ItemIndex.None)
                        {
                            pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);
                        }
                        else
                        {
                            EquipmentIndex equipmentIndex = EquipmentCatalog.FindEquipmentIndex(itemkey);
                            if (equipmentIndex != EquipmentIndex.None)
                            {
                                pickupIndex = PickupCatalog.FindPickupIndex(equipmentIndex);
                            }
                            else
                            {
                                pickupIndex = PickupIndex.none;
                                Log.LogError($"LunarShopItemsList: Could not find item key: {itemkey}");
                            }
                        }
                    }
                }
            }
            else
            {
                return null;
            }

            if (pickupIndex == PickupIndex.none)
                return null;

            PickupIndex[] rewards = new PickupIndex[amount];
            Array.Fill(rewards, pickupIndex);
            return rewards;
        }

        protected List<GameObject> DoSpawnCard(Dictionary<int, SpawnCardStruct> keyValuePairs, AsyncOperationHandle<InteractableSpawnCard> card, int max)
        {
            int count = 0;
            if (ModConfig.SpawnCountByStage.Value) count = SetCountbyGameStage(max, ModConfig.SpawnCountOffset.Value);
            else count = max;
            List<GameObject> result = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                SpawnCard spawnCard = card.WaitForCompletion();
                GameObject gameObject = spawnCard.DoSpawn(keyValuePairs[i].Position, Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectPlacement, Run.instance.runRNG)).spawnedInstance;
                gameObject.transform.eulerAngles = keyValuePairs[i].Rotation;
                result.Add(gameObject);
            }
            return result;
        }
        protected virtual List<GameObject> DoSpawnGameObject(Dictionary<int, SpawnCardStruct> keyValuePairs, AsyncOperationHandle<GameObject> card, int max)
        {
            int count = 0;
            if (ModConfig.SpawnCountByStage.Value)
            {
                count = SetCountbyGameStage(max, ModConfig.SpawnCountOffset.Value);
            }
            else
            {
                count = max;
            }
            List<GameObject> result = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject gameObject = GameObject.Instantiate(card.WaitForCompletion(), keyValuePairs[i].Position, Quaternion.identity);
                gameObject.transform.eulerAngles = keyValuePairs[i].Rotation;
                NetworkServer.Spawn(gameObject);
                result.Add(gameObject);
            }
            return result;
        }

        public static void SpawnEffect(AsyncOperationHandle<GameObject> effect, Vector3 position, Color32 color, float scale = 1f)
        {
            EffectManager.SpawnEffect(effect.WaitForCompletion(), new EffectData()
            {
                origin = position,
                rotation = Quaternion.identity,
                scale = scale,
                color = color
            }, true);
        }
        protected int SetCountbyGameStage(int max, int offset = 0)
        {
            int stageCount = Run.instance.stageClearCount;
            int count = Math.Min(stageCount + offset, max);
            return Math.Max(0, count);
        }

        protected bool IsCurrentMapInBazaar()
        {
            return SceneManager.GetActiveScene().name == "bazaar";
        }
        protected bool IsMultiplayer()
        {
            return PlayerCharacterMasterController.instances.Count > 1;
        }
        protected T GetRandom<T>(List<T> list, T defaultValue)
        {
            if (list == null || list.Count == 0)
            {
                return defaultValue;
            }
            return list[RNG.Next(list.Count)];
        }

        protected List<t> DisorderList<t>(List<t> TList)
        {
            List<t> NewList = new List<t>();
            foreach (var item in TList)
            {
                NewList.Insert(RNG.Next(NewList.Count()), item);
            }
            return NewList;
        }

    }
}
