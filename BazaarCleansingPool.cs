using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    public class BazaarCleansingPool : BazaarBase
    {
        AsyncOperationHandle<InteractableSpawnCard> iscShrineCleanse;
        AsyncOperationHandle<GameObject> lunarShopTerminal;
        AsyncOperationHandle<GameObject> duplicator;
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicator;
        AsyncOperationHandle<GameObject> SingleLunarShop;

        Dictionary<int, SpawnCardStruct> DicLunarPools = new Dictionary<int, SpawnCardStruct>();

        public override void Preload()
        {
            iscDuplicator = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset");
            duplicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Duplicator/Duplicator.prefab");
            iscShrineCleanse = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset");
            lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
            SingleLunarShop = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/SingleLunarShop.prefab");
        }

        public override void Hook()
        {
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
        }

        public override void RunStart()
        {

        }
        public override void SetupBazaar()
        {
            if (ModConfig.CleansingPoolSectionEnabled.Value) { 
                SpawnShrineCleanse();
            }
        }

        private void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.CleansingPoolSectionEnabled.Value && ModConfig.CleansingPoolRewardsLunarCoins.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("ShrineCleanse"))
                {
                    newPickup.pickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex);
                }
                if (self.name.StartsWith("HoveringLunarCoin"))
                {
                    newPickup.pickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex);
                    self.gameObject.GetComponent<PurchaseInteraction>().SetAvailable(false);
                }
            }
            orig(self, newPickup, newHidden);
        }

        private void SpawnShrineCleanse()
        {
            DicLunarPools.Clear();
            SetLunarPool();
            DoSpawnCard(DicLunarPools, iscShrineCleanse, DicLunarPools.Count);

            if (ModConfig.CleansingPoolRewardsLunarCoins.Value)
            {
                GameObject gameObject = GameObject.Instantiate(lunarShopTerminal.WaitForCompletion(), DicLunarPools[0].Position, Quaternion.identity);
                gameObject.transform.localPosition = new Vector3(DicLunarPools[0].Position.x, DicLunarPools[0].Position.y + 0.5f, DicLunarPools[0].Position.z);
                gameObject.transform.eulerAngles = DicLunarPools[0].Rotation;
                gameObject.name = "HoveringLunarCoin";
                NetworkServer.Spawn(gameObject);
            }
        }

        private void SetLunarPool()
        {
            DicLunarPools.Add(0, new SpawnCardStruct(new Vector3(-71.5092f, -25.5622f, -44.2288f), new Vector3(6.4123f, 0f, 356.459f)));
        }
    }
}
