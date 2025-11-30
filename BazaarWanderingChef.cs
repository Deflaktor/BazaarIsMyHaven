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
    public class BazaarWanderingChef : BazaarBase
    {
        AsyncOperationHandle<GameObject> mealPrep;
        AsyncOperationHandle<GameObject> ChefWok_WhitesAndGreens;
        CraftableCatalog.RecipeEntry recipe = null;

        public override void Preload()
        {
            mealPrep = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab");
            ChefWok_WhitesAndGreens = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/ChefWok, WhitesAndGreens.prefab");
        }

        public override void Hook()
        {
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
            On.RoR2.CraftingController.AttemptFindPossibleRecipes += CraftingController_AttemptFindPossibleRecipes;
        }


        public override void RunStart()
        {

        }
        public override void SetupBazaar()
        {
            if (ModConfig.WanderingChefSectionEnabled.Value)
            {
                SpawnWanderingChef();
            }
        }

        private void CraftingController_AttemptFindPossibleRecipes(On.RoR2.CraftingController.orig_AttemptFindPossibleRecipes orig, CraftingController self)
        {
            if (ModConfig.EnableMod.Value && ModConfig.WanderingChefSectionEnabled.Value && ModConfig.WanderingChefSingleRecipe.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("WanderingChef"))
                {
                    if (recipe != null)
                    {
                        self._possibleRecipes = new CraftableCatalog.RecipeEntry[] { recipe };
                        self.bestFitRecipe = recipe;
                        self.result = recipe.result;
                        self.amountToDrop = recipe.amountToDrop;
                        self.SetDirtyBit(CraftingController.resultDirtyBit);
                        return;
                    }
                }
            }
            orig(self);
        }

        private void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.WanderingChefSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("CookingCauldron"))
                {
                    if (recipe == null || !ModConfig.WanderingChefSingleRecipe.Value)
                    {
                        newPickup.pickupIndex = PickupIndex.none;
                    } 
                    else
                    {
                        newPickup.pickupIndex = recipe.result;
                    }
                }
            }
            orig(self, newPickup, newHidden);
        }

        private List<CraftableCatalog.RecipeEntry> GetAvailableRecipes()
        {
            List<CraftableCatalog.RecipeEntry> selection = new List<CraftableCatalog.RecipeEntry>();
            var possibleRecipes = CraftableCatalog.GetAllRecipes();
            foreach (var possibleRecipe in possibleRecipes)
            {
                // TODO: Are FoodTier items also in availableItems? Check later
                // Check that the target item is available in this run
                if (Run.instance.availableItems.Contains(possibleRecipe.result.pickupDef.itemIndex))
                {
                    // Check that at least one set of ingredients is enabled in this run
                    var atLeastOneIngredientSetAvailable = false;
                    foreach (var ingredient in possibleRecipe.possibleIngredients)
                    {
                        var ingredientMissing = false;
                        foreach (var pickup in ingredient.pickups)
                        {
                            if (!Run.instance.IsPickupAvailable(pickup))
                            {
                                ingredientMissing = true;
                                break;
                            }
                        }
                        if (!ingredientMissing)
                        {
                            atLeastOneIngredientSetAvailable = true;
                            break;
                        }
                    }
                    if (atLeastOneIngredientSetAvailable) {
                        // can be added
                        selection.Add(possibleRecipe);
                    }
                }
            }
            return selection;
        }

        private void SpawnWanderingChef()
        {
            var chefPos = new SpawnCardStruct(new Vector3(-70.7306f, -23.7171f, -30.4022f), new Vector3(0f, 220f, 0f));
            var cookingPlacePos = new SpawnCardStruct(new Vector3(-72.4183f, -24.4958f, -28.9289f), new Vector3(0f, 220f, 0f));

            int count = 0;
            if (ModConfig.SpawnCountByStage.Value)
            {
                count = SetCountbyGameStage(1, ModConfig.SpawnCountOffset.Value);
            }
            else
            {
                count = 1;
            }
            if (count > 0)
            {
                recipe = GetRandom(GetAvailableRecipes(), null);

                // Wandering Chef
                GameObject gameObject = GameObject.Instantiate(mealPrep.WaitForCompletion(), chefPos.Position, Quaternion.identity);
                gameObject.transform.eulerAngles = chefPos.Rotation;
                gameObject.name = "WanderingChef";
                NetworkServer.Spawn(gameObject);

                // Cooking Cauldron
                gameObject = GameObject.Instantiate(ChefWok_WhitesAndGreens.WaitForCompletion(), cookingPlacePos.Position, Quaternion.identity);
                gameObject.transform.eulerAngles = cookingPlacePos.Rotation;
                gameObject.name = "CookingCauldron";
                NetworkServer.Spawn(gameObject);
            }
        }
    }
}
