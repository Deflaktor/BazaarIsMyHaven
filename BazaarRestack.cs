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
    public class BazaarRestack : BazaarBase
    {
        AsyncOperationHandle<InteractableSpawnCard> iscShrineRestack;
        DirectorCard card;
        bool spawned;
        public override void Preload()
        {
            iscShrineRestack = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/ShrineRestack/iscShrineRestack.asset");
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.ScaleCost += PurchaseInteraction_ScaleCost;
        }
        public override void RunStart()
        {

        }

        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            card = null;
            if (ModConfig.ShrineOfOrderSectionEnabled.Value)
            {
                card = new DirectorCard("Shrine of Order", ModConfig.ShrineOfOrderDirectorCost, ModConfig.ShrineOfOrderDirectorWeight, () => 1);
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            spawned = false;
            SpawnLatePicks();
        }

        public override void SpawnLatePicks()
        {
            if (card == null || card.Count == 0 || spawned)
                return;
            SpawnShrineRestack();
            spawned = true;
        }

        private void PurchaseInteraction_ScaleCost(On.RoR2.PurchaseInteraction.orig_ScaleCost orig, PurchaseInteraction self, float scalar)
        {
            if (ModConfig.EnableMod.Value && ModConfig.ShrineOfOrderSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("ShrineRestack"))
                {
                    scalar = (float)ModConfig.ShrineOfOrderCostMultiplier.Value;
                }
            }
            orig(self, scalar);
        }

        private void SpawnShrineRestack()
        {
            GameObject shrinerestackOne = SpawnAt(iscShrineRestack.WaitForCompletion().prefab, new SpawnCardStruct(new Vector3(-130f, -24f, -40f), new Vector3(0.0f, 220f, 0.0f)));
            if (ModConfig.ShrineOfOrderUseLimit.Value >= 0)
            {
                shrinerestackOne.GetComponent<ShrineRestackBehavior>().maxPurchaseCount = ModConfig.ShrineOfOrderUseLimit.Value;
            }
            else
            {
                shrinerestackOne.GetComponent<ShrineRestackBehavior>().maxPurchaseCount = int.MaxValue;
            }
            shrinerestackOne.GetComponent<PurchaseInteraction>().cost = ModConfig.ShrineOfOrderCost.Value;
            shrinerestackOne.GetComponent<PurchaseInteraction>().Networkcost = ModConfig.ShrineOfOrderCost.Value;
        }
    }
}
