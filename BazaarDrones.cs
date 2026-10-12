using RoR2;
using RoR2.CharacterAI;
using RoR2.Items;
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

        public override void Preload()
        {
            DroneCombinerStation = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneCombinerStation/DroneCombinerStation.prefab");
            DroneUpgradeStation = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneAssemblyStation/DroneAssemblyStation.prefab");
            DroneScrapper = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/DroneScrapper/DroneScrapper.prefab");
            TripleDroneShop = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/TripleDroneShop/TripleDroneShop.prefab");
        }

        public override void Hook()
        {
            On.RoR2.CharacterAI.BaseAI.ManagedFixedUpdate += BaseAI_ManagedFixedUpdate;
            On.RoR2.Items.MinionLeashBodyBehavior.FixedUpdate += MinionLeashBodyBehavior_FixedUpdate;
        }

        public override void SetupBazaar()
        {
            // clients only have the stations if the run has the Alloyed Collective
            if (!ModConfig.DronesSectionEnabled.Value || !DLC3Content.DLC3 || !Run.instance.IsExpansionEnabled(DLC3Content.DLC3))
            {
                return;
            }

            DroneSpot.Create();

            if (ModConfig.DronesSpawnDroneCombinerStation.Value)
            {
                SpawnStation(DroneCombinerStation, new Vector3(-89.4022f, -23.4619f, -5.0529f), new Vector3(0f, 150f, 350f));
            }
            if (ModConfig.DronesSpawnDroneUpgradeStation.Value)
            {
                SpawnStation(DroneUpgradeStation, new Vector3(-85.1751f, -22.264f, -3.539f), new Vector3(0f, 150f, 270f));
            }
            if (ModConfig.DronesSpawnDroneScrapper.Value)
            {
                SpawnStation(DroneScrapper, new Vector3(-91.9756f, -26.0923f, -48.0897f), new Vector3(0f, 315f, 0f));
            }
            if (ModConfig.DronesSpawnTripleDroneShop.Value)
            {
                SpawnStation(TripleDroneShop, new Vector3(-64.3101f, -23.0074f, -19.7406f), new Vector3(0f, 300f, 6f));
            }
        }

        private void BaseAI_ManagedFixedUpdate(On.RoR2.CharacterAI.BaseAI.orig_ManagedFixedUpdate orig, BaseAI self, float deltaTime)
        {
            if (!DroneSpot.instance || !DroneSpot.IsPlayerDrone(self.master))
            {
                orig(self, deltaTime);
                return;
            }

            // without inputs, a drone hovers where it is
            var inputBank = self.bodyInputBank;
            if (inputBank)
            {
                inputBank.skill1.PushState(false);
                inputBank.skill2.PushState(false);
                inputBank.skill3.PushState(false);
                inputBank.skill4.PushState(false);
                inputBank.jump.PushState(false);
                inputBank.sprint.PushState(false);
                inputBank.rawSprintInput.PushState(false);
                inputBank.activateEquipment.PushState(false);
                inputBank.moveVector = Vector3.zero;
            }
        }

        private void MinionLeashBodyBehavior_FixedUpdate(On.RoR2.Items.MinionLeashBodyBehavior.orig_FixedUpdate orig, MinionLeashBodyBehavior self)
        {
            // the leash would teleport waiting drones to their player
            if (DroneSpot.instance && self.body && DroneSpot.IsPlayerDrone(self.body.master))
            {
                return;
            }
            orig(self);
        }

        private void SpawnStation(AsyncOperationHandle<GameObject> prefab, Vector3 position, Vector3 rotation)
        {
            // clients only get the rotation the station is spawned with
            GameObject gameObject = Object.Instantiate(prefab.WaitForCompletion(), position, Quaternion.Euler(rotation));
            NetworkServer.Spawn(gameObject);
        }
    }
}
