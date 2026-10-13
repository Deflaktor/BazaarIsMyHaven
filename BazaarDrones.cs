using BepInEx.Configuration;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    public class BazaarDrones : BazaarBase
    {
        AsyncOperationHandle<GameObject> DroneCombinerStation;
        AsyncOperationHandle<GameObject> DroneUpgradeStation;
        AsyncOperationHandle<GameObject> DroneScrapper;
        AsyncOperationHandle<GameObject> TripleDroneShop;

        private class Station
        {
            public DirectorCard card;
            public AsyncOperationHandle<GameObject> prefab;
            public SpawnCardStruct slot;
            public bool spawned;
            public GameObject instance;
        }
        private readonly List<Station> stations = new List<Station>();
        private Station upgradeStation;

        public override void Preload()
        {
            DroneCombinerStation = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneCombinerStation/DroneCombinerStation.prefab");
            DroneUpgradeStation = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneAssemblyStation/DroneAssemblyStation.prefab");
            DroneScrapper = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneScrapper/DroneScrapper.prefab");
            TripleDroneShop = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/TripleDroneShop/TripleDroneShop.prefab");
        }

        public override void Hook()
        {
            On.RoR2.Stage.RespawnAllNPCs += Stage_RespawnAllNPCs;
            On.RoR2.PickupPickerController.GetInteractability += PickupPickerController_GetInteractability;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
        }

        // clients only have the stations if the run has the Alloyed Collective
        private static bool IsSectionActive()
        {
            return ModConfig.DronesSectionEnabled.Value && DLC3Content.DLC3 && Run.instance && Run.instance.IsExpansionEnabled(DLC3Content.DLC3);
        }

        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            stations.Clear();
            upgradeStation = null;
            if (!IsSectionActive())
            {
                return;
            }

            AddStation(cards, "Drone Combiner Station", ModConfig.DronesDroneCombinerStationDirectorCost, ModConfig.DronesDroneCombinerStationDirectorWeight,
                DroneCombinerStation, new SpawnCardStruct(new Vector3(-89.4022f, -23.4619f, -5.0529f), new Vector3(0f, 150f, 350f)));
            upgradeStation = AddStation(cards, "Drone Upgrade Station", ModConfig.DronesDroneUpgradeStationDirectorCost, ModConfig.DronesDroneUpgradeStationDirectorWeight,
                DroneUpgradeStation, new SpawnCardStruct(new Vector3(-85.1751f, -22.264f, -3.539f), new Vector3(0f, 150f, 270f)));
            AddStation(cards, "Drone Scrapper", ModConfig.DronesDroneScrapperDirectorCost, ModConfig.DronesDroneScrapperDirectorWeight,
                DroneScrapper, new SpawnCardStruct(new Vector3(-91.9756f, -26.0923f, -48.0897f), new Vector3(0f, 315f, 0f)));
            AddStation(cards, "Triple Drone Shop", ModConfig.DronesTripleDroneShopDirectorCost, ModConfig.DronesTripleDroneShopDirectorWeight,
                TripleDroneShop, new SpawnCardStruct(new Vector3(-64.3101f, -23.0074f, -19.7406f), new Vector3(0f, 300f, 6f)));
        }

        private Station AddStation(List<DirectorCard> cards, string name, ConfigEntry<int> cost, ConfigEntry<float> weight,
            AsyncOperationHandle<GameObject> prefab, SpawnCardStruct slot)
        {
            var station = new Station { card = new DirectorCard(name, cost, weight, () => 1), prefab = prefab, slot = slot };
            stations.Add(station);
            cards.Add(station.card);
            return station;
        }

        public override void SetupBazaar()
        {
            SpawnLatePicks();
        }

        public override void SpawnLatePicks()
        {
            foreach (var station in stations)
            {
                if (station.card.Count == 0 || station.spawned)
                {
                    continue;
                }
                station.instance = SpawnAt(station.prefab.WaitForCompletion(), station.slot);
                station.spawned = true;
            }
        }

        // The Bazaar keeps all allies out, but the drone stations need the players' drones.
        // Bring the drones in the way other stages bring allies in.
        private void Stage_RespawnAllNPCs(On.RoR2.Stage.orig_RespawnAllNPCs orig, Stage self)
        {
            orig(self);
            if (!ModConfig.EnableMod.Value || !IsCurrentMapInBazaar() || !IsSectionActive() || !self.sceneDef || !self.sceneDef.suppressNpcEntry)
            {
                return;
            }

            var spawnTransform = self.GetPlayerSpawnTransform();
            var masters = CharacterMaster.readOnlyInstancesList;
            for (int i = 0; i < masters.Count; i++)
            {
                var master = masters[i];
                // only drones that come from the last stage
                if (!IsPlayerDrone(master) || master.GetBodyObject() || !Util.IsDontDestroyOnLoad(master.gameObject))
                {
                    continue;
                }

                var position = Vector3.zero;
                var rotation = Quaternion.identity;
                if (spawnTransform)
                {
                    position = spawnTransform.position;
                    rotation = spawnTransform.rotation;
                    // a random node near the players' spawn, like the game does
                    var ai = master.GetComponent<BaseAI>();
                    var body = master.bodyPrefab.GetComponent<CharacterBody>();
                    var nodeGraph = ai && body ? ai.GetDesiredSpawnNodeGraph() : null;
                    if (nodeGraph)
                    {
                        var nodes = new List<NodeGraph.NodeIndex>();
                        nodeGraph.FindNodesInRange(position, 10f, 100f, (HullMask)(1 << (int)body.hullClassification), nodes);
                        if (nodes.Count > 0)
                        {
                            nodeGraph.GetNodePosition(nodes[Random.Range(0, nodes.Count)], out position);
                        }
                    }
                }
                master.Respawn(position, rotation, wasRevivedMidStage: true);
            }
        }

        // The Drone Upgrade Station takes the item even from players without drones, who get nothing for it.
        // Refuse them like the Drone Combiner Station refuses players without drones to combine.
        private Interactability PickupPickerController_GetInteractability(On.RoR2.PickupPickerController.orig_GetInteractability orig, PickupPickerController self, Interactor activator)
        {
            return RefusesUpgrade(self.gameObject, activator) ? Interactability.ConditionsNotMet : orig(self, activator);
        }

        // the station's free purchase would otherwise still go through, and the player would hear no refusal
        private Interactability PurchaseInteraction_GetInteractability(On.RoR2.PurchaseInteraction.orig_GetInteractability orig, PurchaseInteraction self, Interactor activator)
        {
            return RefusesUpgrade(self.gameObject, activator) ? Interactability.ConditionsNotMet : orig(self, activator);
        }

        private bool RefusesUpgrade(GameObject interactable, Interactor activator)
        {
            if (upgradeStation == null || !upgradeStation.instance || interactable != upgradeStation.instance || !activator)
            {
                return false;
            }
            var body = activator.GetComponent<CharacterBody>();
            return !body || !body.master || !HasDrones(body.master);
        }

        // The drones the station would upgrade, found the way the game finds them
        private static bool HasDrones(CharacterMaster owner)
        {
            var group = MinionOwnership.MinionGroup.FindGroup(owner.netId);
            if (group == null)
            {
                return false;
            }
            foreach (var member in group.members)
            {
                var master = member ? member.GetComponent<CharacterMaster>() : null;
                var body = master ? master.GetBody() : null;
                if (body && (body.bodyFlags & CharacterBody.BodyFlags.Drone) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsPlayerDrone(CharacterMaster master)
        {
            if (!master || !master.bodyPrefab || !master.minionOwnership)
            {
                return false;
            }
            var owner = master.minionOwnership.ownerMaster;
            return owner && owner.playerCharacterMasterController
                && master.bodyPrefab.TryGetComponent(out CharacterBody body) && (body.bodyFlags & CharacterBody.BodyFlags.Drone) != 0;
        }
    }
}
