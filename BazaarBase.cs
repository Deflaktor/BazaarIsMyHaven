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
    /// <summary>
    /// A section of the Bazaar, e.g. the printers or the Donation Altar. Other mods add their own with
    /// BazaarAPI.AddSection. On every visit, the director collects the cards of all sections, buys from them with its
    /// credits, and then each section spawns what the director bought. Everything but Preload and Hook runs on the host
    /// only, and only while the mod is enabled.
    /// </summary>
    public abstract class BazaarBase
    {
        protected System.Random RNG = new System.Random();

        /// <summary>Loads the section's assets, once, when it is added.</summary>
        public abstract void Preload();
        /// <summary>Spawns what the director bought from the section's cards, when the Bazaar is set up.</summary>
        public abstract void SetupBazaar();
        /// <summary>Adds the section's hooks, once, after the game loaded.</summary>
        public virtual void Hook() { }
        public virtual void RunStart() { }
        public virtual void RunEnd() { }
        /// <summary>Adds the cards the director can buy for the section, on every visit, before SetupBazaar.</summary>
        public virtual void AddDirectorCards(List<DirectorCard> cards) { }
        /// <summary>
        /// Runs a frame after setup, once the shops rolled their items, e.g. to remove a shop that has nothing to offer.
        /// Returns the credits to give back to the director, which spends them in a final pass.
        /// </summary>
        public virtual int CheckAfterSetup() { return 0; }
        /// <summary>Spawns what the director bought in its final pass.</summary>
        public virtual void SpawnLatePicks() { }

        // Spawns a prefab already rotated, so every client gets the rotation the host sees
        protected static GameObject SpawnAt(GameObject prefab, SpawnCardStruct slot)
        {
            GameObject gameObject = GameObject.Instantiate(prefab, slot.Position, Quaternion.Euler(slot.Rotation));
            NetworkServer.Spawn(gameObject);
            return gameObject;
        }

        // A kind of another mod, added through BazaarAPI; it spawns its spawn card's prefab
        private protected static DirectorKind AddedKind(string name, Func<SpawnCard> spawnCard, Func<int> cost, Func<float> weight, Func<bool> isAvailable)
        {
            return new DirectorKind
            {
                name = name,
                cost = cost,
                weight = weight,
                isAvailable = isAvailable,
                spawn = slot =>
                {
                    var card = spawnCard();
                    var prefab = card ? card.prefab : null;
                    if (prefab)
                        SpawnAt(prefab, slot);
                    else
                        Log.LogError($"{name} has no spawn card to spawn.");
                }
            };
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
