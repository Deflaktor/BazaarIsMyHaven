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

        public abstract void Preload();
        public abstract void SetupBazaar();
        public virtual void Hook() { }
        public virtual void RunStart() { }
        public virtual void RunEnd() { }
        // Adds the cards the director can pick for this section; SetupBazaar spawns the picks
        public virtual void AddDirectorCards(List<DirectorCard> cards) { }
        // Runs a frame after setup, once the shops rolled their items. Returns credits to give back to the director.
        public virtual int CheckAfterSetup() { return 0; }
        // Spawns what the director picked in its final pass
        public virtual void SpawnLatePicks() { }

        // Spawns a prefab already rotated, so every client gets the rotation the host sees
        protected static GameObject SpawnAt(GameObject prefab, SpawnCardStruct slot)
        {
            GameObject gameObject = GameObject.Instantiate(prefab, slot.Position, Quaternion.Euler(slot.Rotation));
            NetworkServer.Spawn(gameObject);
            return gameObject;
        }

        protected static List<GameObject> SpawnInSlots(Dictionary<int, SpawnCardStruct> slots, GameObject prefab, int start, int count)
        {
            List<GameObject> result = new List<GameObject>();
            for (int i = start; i < start + count; i++)
            {
                result.Add(SpawnAt(prefab, slots[i]));
            }
            return result;
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
                if (!gameObject)
                {
                    Log.LogWarning($"Could not spawn {spawnCard.name} at slot {i}.");
                    continue;
                }
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
