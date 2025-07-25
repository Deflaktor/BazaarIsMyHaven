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
    public class BazaarCleansingPool : BazaarBase
    {
        AsyncOperationHandle<InteractableSpawnCard> iscShrineCleanse;

        Dictionary<int, SpawnCardStruct> DicLunarPools = new Dictionary<int, SpawnCardStruct>();

        public override void Init()
        {
            iscShrineCleanse = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset");
        }

        public override void Hook()
        {
            
        }

        public override void SetupBazaar()
        {
            SpawnShrineCleanse(); // 月池
        }
        private void SpawnShrineCleanse()
        {
            if (ModConfig.EnableShrineCleanse.Value)
            {
                // 月池
                DicLunarPools.Clear();
                SetLunarPool();
                DoSpawnCard(DicLunarPools, iscShrineCleanse, DicLunarPools.Count);
            }
        }

        private void SetLunarPool()
        {
            DicLunarPools.Add(0, new SpawnCardStruct(new Vector3(-115.420f, -9.55f, -50.3600f), new Vector3(90.0f, 30.0f, 0.0f)));
            DicLunarPools.Add(1, new SpawnCardStruct(new Vector3(-129.891f, -9.55f, -42.6537f), new Vector3(90.0f, 30.0f, 0.0f)));
        }
    }
}
