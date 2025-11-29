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

        Dictionary<int, SpawnCardStruct> DicGlodChests = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicBigChests = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicSmallChests = new Dictionary<int, SpawnCardStruct>();

        AsyncOperationHandle<InteractableSpawnCard> iscChest1;
        AsyncOperationHandle<InteractableSpawnCard> iscChest2;
        AsyncOperationHandle<InteractableSpawnCard> iscGoldChest;

        AsyncOperationHandle<InteractableSpawnCard> iscShopPortal;

        AsyncOperationHandle<InteractableSpawnCard> iscDeepVoidPortalBattery;

        AsyncOperationHandle<GameObject> TeleporterBeaconEffect;

        public override void Preload()
        {
            iscChest1 = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Chest1/iscChest1.asset");
            iscChest2 = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Chest2/iscChest2.asset");
            iscGoldChest = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/GoldChest/iscGoldChest.asset");

            iscShopPortal = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/PortalShop/iscShopPortal.asset");

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
            if (ModConfig.DecorateBazaar.Value)
            {
                SpawnDecorate();
                SpawnBluePortal();
                SpawnEffect(TeleporterBeaconEffect, new Vector3(-73.5143f, -22.2897f, 9.1621f), Color.blue, 1f);
                SpawnEffect(TeleporterBeaconEffect, new Vector3(-57.0645f, -22.2698f, -0.5218f), Color.blue, 1f);
                SpawnEffect(TeleporterBeaconEffect, new Vector3(15.7063f, -2.1074f, 2.5406f), Color.blue, 1f);
                SpawnEffect(TeleporterBeaconEffect, new Vector3(2.5543f, -2.7093f, -8.7185f), Color.blue, 1f);
            }
        }

        private void SpawnBluePortal()
        {
            SpawnCard spawnCard = iscShopPortal.WaitForCompletion();
            GameObject gameObject = spawnCard.DoSpawn(new Vector3(-135f, -23f, -60f), Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectPlacement, Run.instance.runRNG)).spawnedInstance;
            gameObject.transform.eulerAngles = new Vector3(0.0f, 220f, 0.0f);
        }

        private void SetDecorate()
        {
            List<int> total = new List<int> { 0, 1, 2 };
            List<int> random = new List<int>();

            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicGlodChests.Add(random[0], new SpawnCardStruct(new Vector3(-127.7178f, -23.5346f, -60.0732f), new Vector3(350.0f, 0.0f, 0.0f)));
            DicGlodChests.Add(random[1], new SpawnCardStruct(new Vector3(-129.2000f, -22.7723f, -63.2481f), new Vector3(350.0f, 150.0f, 350.0f)));
            DicGlodChests.Add(random[2], new SpawnCardStruct(new Vector3(-126.5934f, -22.8150f, -62.1658f), new Vector3(357.8401f, 341.0997f, 330f)));

            total = new List<int> { 0, 1, 2, 3 };
            random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicBigChests.Add(random[0], new SpawnCardStruct(new Vector3(-134.5802f, -25.3546f, -68.1701f), new Vector3(333.3292f, 136.2832f, 335.803f)));
            DicBigChests.Add(random[1], new SpawnCardStruct(new Vector3(-134.7337f, -26.2325f, -67.6419f), new Vector3(333.3292f, 136.2832f, 335.803f)));
            DicBigChests.Add(random[2], new SpawnCardStruct(new Vector3(-132.4877f, -25.2405f, -66.5167f), new Vector3(22.3718f, 143.4671f, 327.6613f)));
            DicBigChests.Add(random[3], new SpawnCardStruct(new Vector3(-132.7968f, -25.409f, -65.5423f), new Vector3(22.3718f, 143.4671f, 327.6613f)));

            total = new List<int> { 0, 1, 2, 3, 4, 5 };
            random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicSmallChests.Add(random[0], new SpawnCardStruct(new Vector3(-133.4579f, -25.3303f, -68.7228f), new Vector3(9.6318f, 138.4325f, 358.917f)));
            DicSmallChests.Add(random[1], new SpawnCardStruct(new Vector3(-135.1821f, -25.4367f, -70.568f), new Vector3(18.5215f, 90.3934f, 352.1391f)));
            DicSmallChests.Add(random[2], new SpawnCardStruct(new Vector3(-136.5309f, -25.5333f, -70.1181f), new Vector3(2.2523f, 116.0182f, 18.9676f)));
            DicSmallChests.Add(random[3], new SpawnCardStruct(new Vector3(-137.627f, -25.5271f, -69.0357f), new Vector3(15.987f, 225.1928f, 1.5787f)));
            DicSmallChests.Add(random[4], new SpawnCardStruct(new Vector3(-137.2342f, -25.4021f, -66.4729f), new Vector3(28.129f, 313.1049f, 21.9359f)));
            DicSmallChests.Add(random[5], new SpawnCardStruct(new Vector3(-140.2227f, -25.6923f, -67.7525f), new Vector3(9.4325f, 192.0851f, 17.5428f)));
        }

        private void SpawnDecorate()
        {
            DicGlodChests.Clear();
            DicBigChests.Clear();
            DicSmallChests.Clear();
            SetDecorate();

            DoSpawnCard(DicGlodChests, iscGoldChest, DicGlodChests.Count).ForEach((gameObject) => gameObject.GetComponent<PurchaseInteraction>().SetAvailable(false));
            DoSpawnCard(DicBigChests, iscChest2, DicBigChests.Count).ForEach((gameObject) => gameObject.GetComponent<PurchaseInteraction>().SetAvailable(false));
            DoSpawnCard(DicSmallChests, iscChest1, DicSmallChests.Count).ForEach((gameObject) => gameObject.GetComponent<PurchaseInteraction>().SetAvailable(false));
        }
    }
}
