using BepInEx;
using RoR2;
using ShareSuite.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace BazaarIsMyHaven
{
    public class BazaarLunarSeer : BazaarBase
    {
        AsyncOperationHandle<GameObject> LunarRecycler;
        AsyncOperationHandle<GameObject> LunarRerollEffect;

        public override void Preload()
        {
            LunarRecycler = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarRecycler/LunarRecycler.prefab");
            LunarRerollEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarRecycler/LunarRerollEffect.prefab");
        }

        public override void Hook()
        {
        }
        public override void RunStart()
        {

        }
        public override void SetupBazaar()
        {
            SpawnLunarSeerRecycler();
        }

        private void SpawnLunarSeerRecycler()
        {

            if (ModConfig.LunarSeerSectionEnabled.Value)
            {
                GameObject gameObject = GameObject.Instantiate(LunarRecycler.WaitForCompletion(), new Vector3(-138.9266f, -25.4444f, -12.0989f), Quaternion.identity);
                gameObject.transform.eulerAngles = new Vector3(277f, 13.8f, 94.85f);
                NetworkServer.Spawn(gameObject);
            }
        }
    }
}
