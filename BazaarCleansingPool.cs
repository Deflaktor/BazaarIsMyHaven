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
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
        }

        public override void SetupBazaar()
        {
            SpawnShrineCleanse(); // 月池
        }

        private void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.ShrineCleaseGivesLunarCoins.Value && IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("ShrineCleanse"))
                {
                    newPickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex);
                }
            }
            orig(self, newPickupIndex, newHidden);
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
            DicLunarPools.Add(0, new SpawnCardStruct(new Vector3(-72.4183f, -24.4958f, -28.9289f), new Vector3(6.4123f, 272.6046f, 356.459f)));
            //DicLunarPools.Add(0, new SpawnCardStruct(new Vector3(-115.420f, -9.55f, -50.3600f), new Vector3(90.0f, 30.0f, 0.0f)));
            //DicLunarPools.Add(1, new SpawnCardStruct(new Vector3(-129.891f, -9.55f, -42.6537f), new Vector3(90.0f, 30.0f, 0.0f)));
        }
    }
}
