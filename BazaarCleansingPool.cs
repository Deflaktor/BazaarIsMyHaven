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

        Dictionary<int, SpawnCardStruct> DicLunarPools = new Dictionary<int, SpawnCardStruct>();
        DirectorCard card;
        bool spawned;
        // only this mod's pool and its coin give Lunar Coins, not pools other mods spawn
        GameObject pool;
        GameObject hoveringCoin;

        public override void Preload()
        {
            iscShrineCleanse = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset");
            lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
        }

        public override void Hook()
        {
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
        }

        public override void RunStart()
        {

        }
        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            card = null;
            if (ModConfig.CleansingPoolSectionEnabled.Value)
            {
                card = new DirectorCard("Cleansing Pool", ModConfig.CleansingPoolDirectorCost, ModConfig.CleansingPoolDirectorWeight, () => 1);
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            spawned = false;
            pool = null;
            hoveringCoin = null;
            SpawnLatePicks();
        }

        public override void SpawnLatePicks()
        {
            if (card == null || card.Count == 0 || spawned)
                return;
            SpawnShrineCleanse();
            spawned = true;
        }

        private void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (ModConfig.CleansingPoolRewardsLunarCoins.Value && NetworkServer.active && self)
            {
                if (pool && self.gameObject == pool)
                {
                    newPickup.pickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.MiscPickups.LunarCoin.miscPickupIndex);
                }
                if (hoveringCoin && self.gameObject == hoveringCoin)
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
            // both pick their pickup in Start, after these references are set
            pool = SpawnAt(iscShrineCleanse.WaitForCompletion().prefab, DicLunarPools[0]);

            if (ModConfig.CleansingPoolRewardsLunarCoins.Value)
            {
                GameObject gameObject = GameObject.Instantiate(lunarShopTerminal.WaitForCompletion(), DicLunarPools[0].Position, Quaternion.identity);
                gameObject.transform.localPosition = new Vector3(DicLunarPools[0].Position.x, DicLunarPools[0].Position.y + 0.5f, DicLunarPools[0].Position.z);
                gameObject.transform.eulerAngles = DicLunarPools[0].Rotation;
                gameObject.name = "HoveringLunarCoin";
                hoveringCoin = gameObject;
                NetworkServer.Spawn(gameObject);
            }
        }

        private void SetLunarPool()
        {
            DicLunarPools.Add(0, new SpawnCardStruct(new Vector3(-71.5092f, -25.5622f, -44.2288f), new Vector3(6.4123f, 0f, 356.459f)));
        }
    }
}
