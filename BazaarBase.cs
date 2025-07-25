using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace BazaarIsMyHome
{
    public abstract class BazaarBase
    {
        protected System.Random RNG = new System.Random();
        protected readonly DirectorPlacementRule DirectPlacement = new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.Direct
        };

        public abstract void Init();
        public abstract void Hook();
        public abstract void SetupBazaar();

        //protected void DoSpawnCard(string name, Vector3 vector)
        //{
        //    SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>(name);
        //    DirectorPlacementRule placementRule = new DirectorPlacementRule
        //    {
        //        placementMode = DirectorPlacementRule.PlacementMode.Random
        //    };
        //    GameObject obj = spawnCard.DoSpawn(vector, Quaternion.identity, new DirectorSpawnRequest(spawnCard, placementRule, Run.instance.runRNG)).spawnedInstance;
        //    obj.transform.eulerAngles = default;
        //}

        protected void DoSpawnCard(Dictionary<int, SpawnCardStruct> keyValuePairs, AsyncOperationHandle<InteractableSpawnCard> card, int max)
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
                    Log.LogDebug($"{card} 出现问题了");
                }
            }
        }
        protected virtual void DoSpawnGameObject(Dictionary<int, SpawnCardStruct> keyValuePairs, AsyncOperationHandle<GameObject> card, int max)
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
            for (int i = 0; i < count; i++)
            {
                GameObject gameObject = GameObject.Instantiate(card.WaitForCompletion(), keyValuePairs[i].Position, Quaternion.identity);
                gameObject.transform.eulerAngles = keyValuePairs[i].Rotation;
                gameObject.transform.localScale = (Vector3)keyValuePairs[i].Scale;
                NetworkServer.Spawn(gameObject);
            }
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
            int stageCount = Run.instance.stageClearCount + 1;
            int set = stageCount + offset;
            if (set > max)
            {
                set = max;
            }
            return set;
        }

        protected bool IsCurrentMapInBazaar()
        {
            return SceneManager.GetActiveScene().name == "bazaar";
        }
        protected bool IsMultiplayer()
        {
            return PlayerCharacterMasterController.instances.Count > 1;
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
