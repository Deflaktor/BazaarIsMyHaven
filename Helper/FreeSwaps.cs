using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BazaarIsMyHaven
{
    // A purchase that puts the buyer's old equipment into a Lunar Bud or an Equipment Terminal buys that slot:
    // swapping there again is free for the buyer. Others still pay, so no one gets equipment someone else paid for.
    // The game's purchase still runs, only without its payment.
    public class FreeSwaps : MonoBehaviour
    {
        private static PurchaseInteraction freePurchase;

        private readonly HashSet<PlayerCharacterMasterController> owners = new HashSet<PlayerCharacterMasterController>();

        public static void Hook()
        {
            On.RoR2.CostTypeDef.PayCost_PayCostContext_PayCostResults += CostTypeDef_PayCost;
            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += PurchaseInteraction_CanBeAffordedByInteractor;
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
            freeSwaps.owners.Add(buyer);
        }

        public static bool IsFreeFor(Component slot, PlayerCharacterMasterController player)
        {
            return slot && player && slot.TryGetComponent(out FreeSwaps freeSwaps) && freeSwaps.owners.Contains(player);
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

        // The game checks the price before the purchase starts, too
        private static bool PurchaseInteraction_CanBeAffordedByInteractor(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
        {
            var body = activator ? activator.GetComponent<CharacterBody>() : null;
            if (body && body.master && IsFreeFor(self, body.master.playerCharacterMasterController))
            {
                return true;
            }
            return orig(self, activator);
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

        // The host's own view of an instanced bud's price; clients get theirs from InstancedPurchases.
        // Shared buds show everyone the same price.
        private static void PurchaseInteraction_UpdateHologramContent(On.RoR2.PurchaseInteraction.orig_UpdateHologramContent orig, PurchaseInteraction self, GameObject hologramContentObject, Transform viewer)
        {
            orig(self, hologramContentObject, viewer);
            if (self.GetComponent<InstancedPurchase>() && viewer && viewer.TryGetComponent(out CharacterBody body) && body.master && IsFreeFor(self, body.master.playerCharacterMasterController)
                && hologramContentObject.TryGetComponent(out CostHologramContent content))
            {
                content.displayValue = 0;
            }
        }
    }
}
