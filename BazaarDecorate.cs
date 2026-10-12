using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    public class BazaarDecorate : BazaarBase
    {

        AsyncOperationHandle<GameObject> TeleporterBeaconEffect;

        public override void Preload()
        {
            TeleporterBeaconEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Teleporter/TeleporterBeaconEffect.prefab");
        }

        public override void Hook()
        {
            
        }
        public override void RunStart()
        {

        }
        public override void SetupBazaar()
        {
            SpawnEffect(TeleporterBeaconEffect, new Vector3(-73.5143f, -22.2897f, 9.1621f), Color.blue, 1f);
            SpawnEffect(TeleporterBeaconEffect, new Vector3(-57.0645f, -22.2698f, -0.5218f), Color.blue, 1f);
            SpawnEffect(TeleporterBeaconEffect, new Vector3(15.7063f, -2.1074f, 2.5406f), Color.blue, 1f);
            SpawnEffect(TeleporterBeaconEffect, new Vector3(2.5543f, -2.7093f, -8.7185f), Color.blue, 1f);
        }
    }
}
