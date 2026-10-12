using RoR2;
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

        public override void SetupBazaar()
        {
            // clients only have the stations if the run has the Alloyed Collective
            if (!ModConfig.DronesSectionEnabled.Value || !DLC3Content.DLC3 || !Run.instance.IsExpansionEnabled(DLC3Content.DLC3))
            {
                return;
            }

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

        private void SpawnStation(AsyncOperationHandle<GameObject> prefab, Vector3 position, Vector3 rotation)
        {
            // clients only get the rotation the station is spawned with
            GameObject gameObject = Object.Instantiate(prefab.WaitForCompletion(), position, Quaternion.Euler(rotation));
            NetworkServer.Spawn(gameObject);
        }
    }
}
