using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarIsMyHaven
{
    // A purchase that puts the buyer's old equipment into a Lunar Bud or an Equipment Terminal buys that slot:
    // swapping there again is free, for the buyer at an instanced slot and for everyone otherwise.
    // The game's purchase still runs, only without its payment.
    public class FreeSwaps : MonoBehaviour
    {
        private static PurchaseInteraction freePurchase;

        private readonly HashSet<PlayerCharacterMasterController> owners = new HashSet<PlayerCharacterMasterController>();
        private bool freeForEveryone;

        public static void Hook()
        {
            On.RoR2.CostTypeDef.PayCost_PayCostContext_PayCostResults += CostTypeDef_PayCost;
            On.RoR2.PurchaseInteraction.UpdateHologramContent += PurchaseInteraction_UpdateHologramContent;
        }

        public static void BuySlot(GameObject slot, PlayerCharacterMasterController buyer)
        {
            if (!slot || !buyer)
            {
                return;
            }
            if (!slot.TryGetComponent(out FreeSwaps freeSwaps))
            {
                freeSwaps = slot.AddComponent<FreeSwaps>();
            }
            // each player has their own instanced slot
            if (slot.GetComponent<InstancedPurchase>())
            {
                freeSwaps.owners.Add(buyer);
                return;
            }
            freeSwaps.freeForEveryone = true;
            // everyone sees the price drop to nothing; price signs go away at 0
            if (slot.TryGetComponent(out PurchaseInteraction purchase))
            {
                purchase.Networkcost = 0;
            }
        }

        public static bool IsFreeFor(Component slot, PlayerCharacterMasterController player)
        {
            return slot && slot.TryGetComponent(out FreeSwaps freeSwaps) && (freeSwaps.freeForEveryone || (player && freeSwaps.owners.Contains(player)));
        }

        // Runs the game's purchase without charging anything
        public static T RunFree<T>(PurchaseInteraction purchase, Func<T> purchaseAction)
        {
            var previousFreePurchase = freePurchase;
            int cost = purchase.cost;
            // the field only, so everyone else keeps the price; the game then sees the swap as affordable
            purchase.cost = 0;
            freePurchase = purchase;
            try
            {
                return purchaseAction();
            }
            finally
            {
                purchase.cost = cost;
                freePurchase = previousFreePurchase;
            }
        }

        // A free swap charges nothing, not even a hit of zero damage for PercentHealth
        private static void CostTypeDef_PayCost(On.RoR2.CostTypeDef.orig_PayCost_PayCostContext_PayCostResults orig, CostTypeDef self, CostTypeDef.PayCostContext context, CostTypeDef.PayCostResults result)
        {
            if (freePurchase && context.purchaseInteraction == freePurchase)
            {
                return;
            }
            orig(self, context, result);
        }

        // The host's own view of a bud's price; clients get theirs from InstancedPurchases
        private static void PurchaseInteraction_UpdateHologramContent(On.RoR2.PurchaseInteraction.orig_UpdateHologramContent orig, PurchaseInteraction self, GameObject hologramContentObject, Transform viewer)
        {
            orig(self, hologramContentObject, viewer);
            if (viewer && viewer.TryGetComponent(out CharacterBody body) && body.master && IsFreeFor(self, body.master.playerCharacterMasterController)
                && hologramContentObject.TryGetComponent(out CostHologramContent content))
            {
                content.displayValue = 0;
            }
        }
    }
}
