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
        private Station tripleDroneShop;

        public override void Preload()
        {
            DroneCombinerStation = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneCombinerStation/DroneCombinerStation.prefab");
            DroneUpgradeStation = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneAssemblyStation/DroneAssemblyStation.prefab");
            DroneScrapper = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneScrapper/DroneScrapper.prefab");
            TripleDroneShop = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/TripleDroneShop/TripleDroneShop.prefab");
            PriceSigns.Preload();
        }

        public override void Hook()
        {
            On.RoR2.Stage.RespawnAllNPCs += Stage_RespawnAllNPCs;
            On.RoR2.PickupPickerController.GetInteractability += PickupPickerController_GetInteractability;
            On.RoR2.PurchaseInteraction.GetInteractability += PurchaseInteraction_GetInteractability;
            On.RoR2.DroneVendorMultiShopController.Start += DroneVendorMultiShopController_Start;
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
            tripleDroneShop = null;
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
            tripleDroneShop = AddStation(cards, "Triple Drone Shop", ModConfig.DronesTripleDroneShopDirectorCost, ModConfig.DronesTripleDroneShopDirectorWeight,
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

        // The Triple Drone Shop picks its drones and price when it starts; this one uses the section's weights, costs and cost type
        private void DroneVendorMultiShopController_Start(On.RoR2.DroneVendorMultiShopController.orig_Start orig, DroneVendorMultiShopController self)
        {
            if (!NetworkServer.active || tripleDroneShop == null || !tripleDroneShop.instance || self.gameObject != tripleDroneShop.instance || !Run.instance)
            {
                orig(self);
                return;
            }

            float commonWeight = Mathf.Max(0f, ModConfig.DronesTripleDroneShopCommonWeight.Value);
            float uncommonWeight = Mathf.Max(0f, ModConfig.DronesTripleDroneShopUncommonWeight.Value);
            if (commonWeight + uncommonWeight <= 0f)
            {
                commonWeight = 1f;
            }
            self.tier1Chance = new RangeFloat { min = commonWeight, max = commonWeight };
            self.tier2Chance = new RangeFloat { min = uncommonWeight, max = uncommonWeight };
            self.tier3Chance = new RangeFloat { min = 0f, max = 0f };
            self.baseCostTier1 = ModConfig.DronesTripleDroneShopCommonCost.Value;
            self.baseCostTier2 = ModConfig.DronesTripleDroneShopUncommonCost.Value;
            var costType = ModConfig.DronesTripleDroneShopCostType.Value;
            self.costType = costType;

            // the game offers only common drones before the third stage; the weights apply on every visit.
            // The field is set without its network setter and restored right after, so clients never see it.
            var run = Run.instance;
            int stageClearCount = run.stageClearCount;
            run.stageClearCount = Mathf.Max(stageClearCount, 3);
            try
            {
                orig(self);
            }
            finally
            {
                run.stageClearCount = stageClearCount;
            }

            // the game always scales the price as money
            int cost = Helper.GetCost(self.baseCost, costType);
            self.Networkcost = cost;
            foreach (var terminal in self._terminals ?? new DroneVendorTerminalBehavior[0])
            {
                if (terminal && terminal.purchaseInteraction)
                {
                    terminal.purchaseInteraction.Networkcost = cost;
                }
            }

            // clients show the shop's price in the prefab's money, since its cost type is not synced, so a price sign replaces it
            if (costType != CostTypeIndex.Money)
            {
                var projector = self.GetComponent<RoR2.Hologram.HologramProjector>();
                var pivot = projector && projector.hologramPivot ? projector.hologramPivot : self.transform;
                self.Networkavailable = false;
                PriceSigns.Apply(self.gameObject, pivot.position, self.transform.rotation * Quaternion.Euler(0f, 180f, 0f), cost, costType);
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
