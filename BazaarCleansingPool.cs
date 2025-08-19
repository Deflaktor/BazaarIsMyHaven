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

        public override void Init()
        {
            iscDuplicator = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset");
            duplicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Duplicator/Duplicator.prefab");
            iscShrineCleanse = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset");
            lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
            SingleLunarShop = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/SingleLunarShop.prefab");
        }

        public override void Hook()
        {
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;

            if (ModCompatibilityShareSuite.enabled)
            {
                // ModCompatibilityShareSuite.AddPickupEventHandler(NonShareableItemCheck);
            }
        }

        public override void SetupBazaar()
        {
            if (ModConfig.CleansingPoolSectionEnabled.Value) { 
                SpawnShrineCleanse();
            }
        }

        private void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.CleansingPoolSectionEnabled.Value && ModConfig.ShrineCleaseGivesLunarCoins.Value && IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("ShrineCleanse"))
                {
                    newPickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex);
                }
                if (self.name.StartsWith("HoveringLunarCoin"))
                {
                    newPickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex);
                    self.gameObject.GetComponent<PurchaseInteraction>().SetAvailable(false);
                }
            }
            orig(self, newPickupIndex, newHidden);
        }


        private bool NonShareableItemCheck(GenericPickupController pickup, CharacterBody picker)
        {
            //if (ModConfig.EnableMod.Value && ModConfig.ShrineCleaseGivesLunarCoins.Value && IsCurrentMapInBazaar())
            // return !pickup.TryGetComponent<NonShareableItem>(out _);
            return pickup.pickupIndex != PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex);
            // item shareable
            return true;
        }

        private void SpawnShrineCleanse()
        {
            DicLunarPools.Clear();
            SetLunarPool();
            DoSpawnCard(DicLunarPools, iscShrineCleanse, DicLunarPools.Count);

            if (ModConfig.ShrineCleaseGivesLunarCoins.Value)
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
            DicLunarPools.Add(0, new SpawnCardStruct(new Vector3(-72.4183f, -24.4958f, -28.9289f), new Vector3(6.4123f, 272.6046f, 356.459f)));
        }
    }
}
