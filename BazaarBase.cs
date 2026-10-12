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
