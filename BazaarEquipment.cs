using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHome
{
    public class BazaarEquipment : BazaarBase
    {
        AsyncOperationHandle<GameObject> multiShopEquipmentTerminal;

        Dictionary<int, SpawnCardStruct> DicEquipments = new Dictionary<int, SpawnCardStruct>();
        Dictionary<int, SpawnCardStruct> DicTriplEquipments = new Dictionary<int, SpawnCardStruct>();

        public override void Init()
        {
            multiShopEquipmentTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MultiShopEquipmentTerminal/MultiShopEquipmentTerminal.prefab");
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
        }

        public override void SetupBazaar()
        {
            SpawnEquipment();
        }

        public void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                if (ModConfig.EquipmentCount.Value > 0)
                {
                    // 主动装备
                    if (self.name.StartsWith("MultiShopEquipmentTerminal"))
                    {
                        //ChatHelper.Send("一台主动装备已修改");
                        self.cost = 0;
                        self.Networkcost = 0;
                        //self.costType = CostTypeIndex.PercentHealth;
                    }
                }
            }
        }

        private void SetEquipment()
        {
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            if (!ModConfig.SeerStationAvailable.Value && ModConfig.EquipmentCount.Value <= 2)
            {
                // left seer stand
                DicEquipments.Add(0, new SpawnCardStruct(new Vector3(-133.9731f, -23.4f, -10.71112f), new Vector3(0f, 120.0f, 0.0f)));
                // right seer stand
                DicEquipments.Add(1, new SpawnCardStruct(new Vector3(-128.0793f, -23.4f, -7.056283f), new Vector3(0f, 160.0f, 0.0f)));
            }
            else
            {
                DicEquipments.Add(random[0], new SpawnCardStruct(new Vector3(-128.9115f, -23.1756f, -24.6339f), new Vector3(350.0f, 90.0f, 0.0f)));
                DicEquipments.Add(random[1], new SpawnCardStruct(new Vector3(-131.3281f, -23.0673f, -21.9982f), new Vector3(353.0f, 0.0f, 0.0f)));
                DicEquipments.Add(random[2], new SpawnCardStruct(new Vector3(-132.8414f, -22.6963f, -26.6293f), new Vector3(353.0f, 220.0f, 0.0f)));
                DicEquipments.Add(random[3], new SpawnCardStruct(new Vector3(-141.3541f, -21.2761f, -10.9000f), new Vector3(358.0f, 180.0f, 0.0f)));
                DicEquipments.Add(random[4], new SpawnCardStruct(new Vector3(-138.9401f, -20.9378f, -8.87810f), new Vector3(355.0f, 100.0f, 0.0f)));
                DicEquipments.Add(random[5], new SpawnCardStruct(new Vector3(-139.9517f, -20.8648f, -5.79960f), new Vector3(353.0f, 30.0f, 0.0f)));
                DicTriplEquipments.Add(random[0], new SpawnCardStruct(new Vector3(-142f, -22.0f, 0.0f), new Vector3(0.0f, 72.0f, 0.0f)));
                DicTriplEquipments.Add(random[1], new SpawnCardStruct(new Vector3(-139f, -22.8f, -2.0f), new Vector3(0.0f, 72.0f, 0.0f)));
                DicTriplEquipments.Add(random[2], new SpawnCardStruct(new Vector3(-136f, -22.5f, 0.0f), new Vector3(0.0f, 72.0f, 0.0f)));
                DicTriplEquipments.Add(random[3], new SpawnCardStruct(new Vector3(-135f, -22.0f, 3.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            }
        }

        private void SpawnEquipment()
        {
            if (ModConfig.EquipmentCount.Value > 0)
            {
                // 主动装备
                DicEquipments.Clear();
                SetEquipment();
                DoSpawnGameObject(DicEquipments, multiShopEquipmentTerminal, ModConfig.EquipmentCount.Value);
            }
        }
    }
}
