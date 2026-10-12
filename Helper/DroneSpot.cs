using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BazaarIsMyHaven
{
    // The drone stations only work with drones that have a body, but no drones enter the Bazaar.
    // The players' drones are spawned at one spot instead, where they wait without their AI and leash.
    // Drones that show up elsewhere, like bought or upgraded ones, are moved there.
    public class DroneSpot : MonoBehaviour
    {
        public static DroneSpot instance;

        private static readonly Vector3 Position = new Vector3(-79.9249f, -37.4302f, -17.9876f);
        // drones further away than this are moved to the spot
        private const float SpotRadius = 25f;
        // gives bought drones time to appear and upgraded ones time to leave the station
        private const float MoveDelay = 2f;
        private const float CheckInterval = 0.5f;

        private readonly Dictionary<CharacterBody, float> awaySince = new Dictionary<CharacterBody, float>();
        private float nextCheckTime;

        public static void Create()
        {
            instance = new GameObject("BazaarDroneSpot").AddComponent<DroneSpot>();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static bool IsPlayerDrone(CharacterMaster master)
        {
            if (!master || !master.bodyPrefab || !master.minionOwnership)
            {
                return false;
            }
            var owner = master.minionOwnership.ownerMaster;
            return owner && owner.playerCharacterMasterController
                && master.bodyPrefab.TryGetComponent(out CharacterBody body) && (body.bodyFlags & CharacterBody.BodyFlags.Drone) != 0;
        }

        // Runs once everything in the Bazaar has started
        private void Start()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            var masters = CharacterMaster.readOnlyInstancesList;
            for (int i = 0; i < masters.Count; i++)
            {
                var master = masters[i];
                // like the game's own respawn, only drones that come from the last stage
                if (IsPlayerDrone(master) && !master.GetBodyObject() && Util.IsDontDestroyOnLoad(master.gameObject))
                {
                    master.Respawn(Position, Quaternion.identity, wasRevivedMidStage: true);
                }
            }
        }

        private void Update()
        {
            if (!NetworkServer.active || Time.unscaledTime < nextCheckTime)
            {
                return;
            }
            nextCheckTime = Time.unscaledTime + CheckInterval;

            var seen = new HashSet<CharacterBody>();
            foreach (var master in CharacterMaster.readOnlyInstancesList)
            {
                if (!IsPlayerDrone(master))
                {
                    continue;
                }
                var body = master.GetBody();
                // a seated drone belongs to a station, e.g. while the Drone Combiner Station upgrades it
                if (!body || body.currentVehicle || (body.footPosition - Position).sqrMagnitude <= SpotRadius * SpotRadius)
                {
                    continue;
                }

                seen.Add(body);
                if (!awaySince.TryGetValue(body, out float since))
                {
                    awaySince[body] = Time.unscaledTime;
                }
                else if (Time.unscaledTime - since >= MoveDelay)
                {
                    MoveToSpot(body);
                    seen.Remove(body);
                }
            }

            // forget drones that were moved, came back or are gone
            var forgotten = new List<CharacterBody>();
            foreach (var body in awaySince.Keys)
            {
                if (!seen.Contains(body))
                {
                    forgotten.Add(body);
                }
            }
            foreach (var body in forgotten)
            {
                awaySince.Remove(body);
            }
        }

        private static void MoveToSpot(CharacterBody body)
        {
            TeleportHelper.TeleportBody(body, Position);
            var effect = Run.instance.GetTeleportEffectPrefab(body.gameObject);
            if (effect)
            {
                EffectManager.SimpleEffect(effect, Position, Quaternion.identity, transmit: true);
            }
        }
    }
}
