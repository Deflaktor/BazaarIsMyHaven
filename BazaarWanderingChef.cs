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

        Dictionary<PickupIndex, List<CraftableCatalog.RecipeEntry>> allAvailableRecipes = null;
        PickupIndex[] allAvailableTargetPickupIndexes = null;
        int randomTargetPickupIndex = 0;

        List<CraftableCatalog.RecipeEntry> possibleRecipes = null;
        ShopTerminalBehavior cauldron = null;
        GameObject chef = null;
        DirectorCard card;

        public override void Preload()
        {
            mealPrep = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab");
            ChefWok_WhitesAndGreens = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/ChefWok, WhitesAndGreens.prefab");
        }

        public override void Hook()
        {
            On.RoR2.ShopTerminalBehavior.SetPickup += ShopTerminalBehavior_SetPickup;
            On.RoR2.CraftingController.AttemptFindPossibleRecipes += CraftingController_AttemptFindPossibleRecipes;
            On.RoR2.CraftingController.GetGeneratedOptionsFromInteractor += CraftingController_GetGeneratedOptionsFromInteractor;
            On.RoR2.CraftingController.CanTakePickupAfterUseByIngredientSlots += CraftingController_CanTakePickupAfterUseByIngredientSlots;
            On.RoR2.MealPrepController.BeginCookingServer += MealPrepController_BeginCookingServer;
        }

        public override void RunStart()
        {
            RunEnd();
            if (ModConfig.WanderingChefSectionEnabled.Value && IsAlloyedCollectiveEnabled() && !ModConfig.WanderingChefUnrestrictedCrafting.Value)
            {
                allAvailableRecipes = GetAvailableRecipes();
                allAvailableTargetPickupIndexes = allAvailableRecipes.Keys.ToArray();
                randomTargetPickupIndex = RNG.Next(allAvailableTargetPickupIndexes.Length);
            }
        }

        public override void RunEnd()
        {
            allAvailableRecipes = null;
            allAvailableTargetPickupIndexes = null;
            possibleRecipes = null;
            cauldron = null;
        }
        // the Wandering CHEF is Alloyed Collective content
        private static bool IsAlloyedCollectiveEnabled()
        {
            return DLC3Content.DLC3 && Run.instance && Run.instance.IsExpansionEnabled(DLC3Content.DLC3);
        }

        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            card = null;
            if (ModConfig.WanderingChefSectionEnabled.Value && IsAlloyedCollectiveEnabled())
            {
                if (!ModConfig.WanderingChefUnrestrictedCrafting.Value &&
                    (allAvailableTargetPickupIndexes == null || allAvailableTargetPickupIndexes.Length == 0))
                {
                    Log.LogWarning("No recipes; skipping the Wandering Chef this visit.");
                    return;
                }
                // with CraftableRecipesOnly, the director's final pass may still send him away
                card = new DirectorCard("Wandering CHEF", ModConfig.WanderingChefDirectorCost, ModConfig.WanderingChefDirectorWeight, () => 1, allowLatePicks: false);
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            chef = null;
            cauldron = null;
            if (card != null && card.Count > 0)
            {
                SpawnWanderingChef();
            }
        }

        // Shops roll their items in their own Start, so the Bazaar's stock is only known now
        public override int CheckAfterSetup()
        {
            if (!chef || ModConfig.WanderingChefUnrestrictedCrafting.Value || !ModConfig.WanderingChefCraftableRecipesOnly.Value)
            {
                return 0;
            }
            if (TryChooseCraftableRecipe())
            {
                return 0;
            }

            Log.LogInfo("No craftable recipe for the Wandering Chef; the director spends his credits on something else.");
            NetworkServer.Destroy(chef);
            if (cauldron)
            {
                NetworkServer.Destroy(cauldron.gameObject);
            }
            chef = null;
            cauldron = null;
            return card.Spent;
        }
        private void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.WanderingChefSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("CookingCauldron"))
                {
                    if(possibleRecipes == null || possibleRecipes.Count == 0)
                    {
                        newPickup.pickupIndex = PickupIndex.none;
                    }
                    else
                    {
                        newPickup.pickupIndex = possibleRecipes[0].result;
                    }
                }
            }
            orig(self, newPickup, newHidden);
        }

        private void CraftingController_AttemptFindPossibleRecipes(On.RoR2.CraftingController.orig_AttemptFindPossibleRecipes orig, CraftingController self)
        {
            if (ModConfig.EnableMod.Value && ModConfig.WanderingChefSectionEnabled.Value && !ModConfig.WanderingChefUnrestrictedCrafting.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("WanderingChef"))
                {
                    if (possibleRecipes != null && possibleRecipes.Count > 0)
                    {
                        self._possibleRecipes = possibleRecipes.ToArray();
                        if (!self.AllSlotsEmpty())
                        {
                            self._possibleRecipes = FindRecipesThatCanAcceptIngredients(self.ingredients);
                        }
                        CraftableCatalog.FilterByEntitlement(ref self._possibleRecipes);
                        if (self.AllSlotsFilled())
                        {
                            self.bestFitRecipe = null;
                            for (int i = 0; i < self._possibleRecipes.Length; i++)
                            {
                                CraftableCatalog.RecipeEntry recipeEntry = self._possibleRecipes[i];
                                if (recipeEntry != null && CraftableCatalog.ValidateRecipe(recipeEntry, self.ingredients))
                                {
                                    self.bestFitRecipe = recipeEntry;
                                }
                            }
                            if (self.bestFitRecipe != null)
                            {
                                self.result = self.bestFitRecipe.result;
                                self.amountToDrop = self.bestFitRecipe.amountToDrop;
                                self.SetDirtyBit(CraftingController.resultDirtyBit);
                            }
                        }
                        return;
                    }
                }
            }
            orig(self);
        }
        private List<PickupPickerController.Option> CraftingController_GetGeneratedOptionsFromInteractor(On.RoR2.CraftingController.orig_GetGeneratedOptionsFromInteractor orig, CraftingController self, Interactor activator)
        {
            var options = orig(self, activator);
            if (ModConfig.EnableMod.Value && ModConfig.WanderingChefSectionEnabled.Value && !ModConfig.WanderingChefUnrestrictedCrafting.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("WanderingChef"))
                {
                    if (possibleRecipes != null && possibleRecipes.Count > 0)
                    {
                        CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                        if (characterBody != null)
                        {
                            List<PickupPickerController.Option> list = new List<PickupPickerController.Option>();
                            // if only a single recipe is allowed then only show ingredients for this recipe regardless if the interactor has them in their inventory or not
                            HashSet<PickupIndex> possibleIngredients = new HashSet<PickupIndex>();
                            foreach (var recipe in possibleRecipes)
                            {
                                foreach (var ingredient in recipe.possibleIngredients)
                                {
                                    foreach (var pickupIndex in ingredient.pickups)
                                    {
                                        possibleIngredients.Add(pickupIndex);
                                    }
                                }
                            }
                            foreach (var pickupIndex in possibleIngredients)
                            {
                                list.Add(new PickupPickerController.Option
                                {
                                    available = InventoryContainsPickup(characterBody.inventory, pickupIndex),
                                    pickup = new UniquePickup(pickupIndex)
                                });
                            }
                            return list;
                        }
                    }
                }
            }
            return options;
        }

        private bool CraftingController_CanTakePickupAfterUseByIngredientSlots(On.RoR2.CraftingController.orig_CanTakePickupAfterUseByIngredientSlots orig, CraftingController self, PickupIndex pickupIndex, PickupIndex[] consumedIngredients)
        {
            if (ModConfig.EnableMod.Value && ModConfig.WanderingChefSectionEnabled.Value && !ModConfig.WanderingChefUnrestrictedCrafting.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("WanderingChef"))
                {
                    CharacterMaster currentParticipantMaster = self.networkUIPromptController.currentParticipantMaster;
                    if ((bool)currentParticipantMaster)
                    {
                        PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                        if (pickupDef != null)
                        {
                            Inventory inventory = currentParticipantMaster.inventory;
                            if (pickupDef.itemIndex != ItemIndex.None)
                            {
                                int num = consumedIngredients.Count((PickupIndex p) => p == pickupIndex);
                                if ((bool)inventory)
                                {
                                    return num < inventory.GetItemCountPermanent(pickupDef.itemIndex);
                                }
                            }
                            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
                            {
                                int used = consumedIngredients.Count(pickup => pickup == pickupIndex);
                                return used < InventoryPickupCount(inventory, pickupDef.pickupIndex);
                            }
                        }
                    }
                    return false;
                }
            }
            return orig(self, pickupIndex, consumedIngredients);
        }

        private void MealPrepController_BeginCookingServer(On.RoR2.MealPrepController.orig_BeginCookingServer orig, MealPrepController self, Interactor activator, PickupIndex[] itemsToTake, PickupIndex reward, int count)
        {
            if (ModConfig.EnableMod.Value && ModConfig.WanderingChefSectionEnabled.Value && ModConfig.WanderingChefBuyToInventory.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("WanderingChef"))
                {
                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    Inventory inventory = characterBody.inventory;
                    // like the game's own cooking; equipment needs an empty slot, unless the recipe takes equipment
                    bool takesEquipment = itemsToTake.Any(pickup => PickupCatalog.GetPickupDef(pickup)?.equipmentIndex != EquipmentIndex.None);
                    if (!self.IsAffordable(itemsToTake, characterBody) || (!takesEquipment && !Helper.HasRoomFor(characterBody, reward)))
                    {
                        return;
                    }
                    // take items
                    for (int i = 0; i < itemsToTake.Length; i++)
                    {
                        PickupDef pickupDef = PickupCatalog.GetPickupDef(itemsToTake[i]);
                        ItemIndex itemIndex = pickupDef.itemIndex;
                        EquipmentIndex equipmentIndex = pickupDef.equipmentIndex;
                        self.CreateItemTakenOrb(activator.transform.position, self.gameObject, pickupDef.pickupIndex);
                        if (itemIndex != ItemIndex.None)
                        {
                            inventory.RemoveItemPermanent(itemIndex);
                        }
                        else if (equipmentIndex != EquipmentIndex.None)
                        {
                            inventory.RemoveEquipment(equipmentIndex);
                        }
                    }
                    // give items
                    Dictionary<PickupIndex, int> itemsToGive = new Dictionary<PickupIndex, int>();
                    itemsToGive[reward] = count;
                    Helper.GivePickups(characterBody, itemsToGive, null, true);
                    // skip original method
                    return;
                }
            }
            orig(self, activator, itemsToTake, reward, count);
        }

        private bool InventoryContainsPickup(Inventory inventory, PickupIndex pickup)
        {
            return InventoryPickupCount(inventory, pickup) > 0;
        }

        private int InventoryPickupCount(Inventory inventory, PickupIndex pickup)
        {
            var pickupDef = pickup.pickupDef;
            if (!inventory || pickupDef == null)
                return 0;

            if(pickupDef.itemIndex != ItemIndex.None)
            {
                return inventory.GetItemCountPermanent(pickupDef.itemIndex);
            }
            int count = 0;
            if(pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                for (uint slot = 0; slot < inventory.GetEquipmentSlotCount(); slot++)
                {
                    for (uint set = 0; set < inventory.GetEquipmentSetCount(); set++)
                    {
                        var equipmentState = inventory.GetEquipment(new EquipmentLocation { slot = slot, set = set });
                        if (equipmentState.equipmentIndex == pickupDef.equipmentIndex)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        public CraftableCatalog.RecipeEntry[] FindRecipesThatCanAcceptIngredients(PickupIndex[] choices)
        {
            var allRecipes = CraftableCatalog.allRecipes;
            if (possibleRecipes != null && possibleRecipes.Count > 0)
                allRecipes = possibleRecipes;
            if (choices.Length == 0)
            {
                return allRecipes.ToArray();
            }
            List<CraftableCatalog.RecipeEntry> list = new List<CraftableCatalog.RecipeEntry>();
            foreach (CraftableCatalog.RecipeEntry allRecipe in allRecipes)
            {
                foreach (PickupIndex pickupIndex in choices)
                {
                    if (allRecipe.recipe.CheckIfValidIngredient(pickupIndex))
                    {
                        list.Add(allRecipe);
                        break;
                    }
                }
            }
            return list.ToArray();
        }

        private Dictionary<PickupIndex, List<CraftableCatalog.RecipeEntry>> GetAvailableRecipes()
        {
            Dictionary<PickupIndex, List<CraftableCatalog.RecipeEntry>> selection = new Dictionary<PickupIndex, List<CraftableCatalog.RecipeEntry>>();
            var possibleRecipes = CraftableCatalog.GetAllRecipes();
            foreach (var possibleRecipe in possibleRecipes)
            {
                var available = false;
                if(possibleRecipe.result.pickupDef.itemIndex != ItemIndex.None)
                {
                    // this does not include consumed items, but oh well. They just wont appear then.
                    available = Run.instance.availableItems.Contains(possibleRecipe.result.pickupDef.itemIndex);
                    var objectiveRelated = ItemCatalog.GetItemDef(possibleRecipe.result.pickupDef.itemIndex).ContainsTag(ItemTag.ObjectiveRelated);
                    if (objectiveRelated) {
                        available = false;
                    }
                }
                if (possibleRecipe.result.pickupDef.equipmentIndex != EquipmentIndex.None)
                {
                    // this does not include consumed items, but oh well. They just wont appear then.
                    available = Run.instance.availableEquipment.Contains(possibleRecipe.result.pickupDef.equipmentIndex);
                }
                // filter out objective related items
                if (available)
                {
                    // Check that at least one set of ingredients is enabled in this run
                    var allIngredientsAvailable = true;
                    foreach (var ingredient in possibleRecipe.possibleIngredients)
                    {
                        var missingIngredients = new HashSet<PickupIndex>();
                        var atLeastOnePickupForIngredientAvailable = false;
                        foreach (var pickupIndex in ingredient.pickups)
                        {
                            if (Run.instance.IsPickupAvailable(pickupIndex))
                            {
                                atLeastOnePickupForIngredientAvailable = true;
                                break;
                            }
                            else
                            {
                                missingIngredients.Add(pickupIndex); 
                            }
                        }
                        if (!atLeastOnePickupForIngredientAvailable) { 
                            allIngredientsAvailable = false;
                            Log.LogDebug($"Recipe for {possibleRecipe.result} cannot be added because of missing ingredients: {string.Join(", ", missingIngredients)}");
                            break;
                        }
                    }
                    if(allIngredientsAvailable)
                    {
                        // can be added
                        var list = selection.GetValueOrDefault(possibleRecipe.result, new List<CraftableCatalog.RecipeEntry>());
                        list.Add(possibleRecipe);
                        selection[possibleRecipe.result] = list;
                    }
                }
                else
                {
                    Log.LogDebug($"Recipe for {possibleRecipe.result} cannot be added because target not in available items.");
                }
            }
            return selection;
        }

        public void NextRecipe()
        {
            if (allAvailableTargetPickupIndexes != null && allAvailableTargetPickupIndexes.Length > 0 && allAvailableRecipes != null)
            {
                randomTargetPickupIndex = (randomTargetPickupIndex + 1) % allAvailableTargetPickupIndexes.Length;
                var pickupIndex = allAvailableTargetPickupIndexes[randomTargetPickupIndex];
                var recipeResult = allAvailableRecipes[pickupIndex];
                if (pickupIndex != PickupIndex.none)
                {
                    possibleRecipes = allAvailableRecipes[pickupIndex];
                }
                if (cauldron != null)
                {
                    cauldron.GenerateNewPickupServer();
                }
            }
        }

        // Continues the recipe rotation with the next target that a player can cook from the Bazaar's stock and their inventory
        private bool TryChooseCraftableRecipe()
        {
            if (allAvailableTargetPickupIndexes == null || allAvailableTargetPickupIndexes.Length == 0 || allAvailableRecipes == null)
            {
                return false;
            }

            var players = GetPlayerIngredients();
            for (int step = 1; step <= allAvailableTargetPickupIndexes.Length; step++)
            {
                int index = (randomTargetPickupIndex + step) % allAvailableTargetPickupIndexes.Length;
                var craftableRecipes = allAvailableRecipes[allAvailableTargetPickupIndexes[index]].Where(recipe => IsCraftable(recipe, players)).ToList();
                if (craftableRecipes.Count > 0)
                {
                    randomTargetPickupIndex = index;
                    possibleRecipes = craftableRecipes;
                    if (cauldron)
                    {
                        cauldron.GenerateNewPickupServer();
                    }
                    return true;
                }
            }
            return false;
        }

        // Needing two of the same ingredient is not checked
        private bool IsCraftable(CraftableCatalog.RecipeEntry recipe, List<(Inventory inventory, HashSet<PickupIndex> bazaarStock)> players)
        {
            return players.Any(player => recipe.possibleIngredients.All(slot => slot.pickups != null &&
                slot.pickups.Any(pickup => player.bazaarStock.Contains(pickup) || InventoryContainsPickup(player.inventory, pickup))));
        }

        // What each player can get in the Bazaar: the shops' items they can still buy, and scrap if a Scrapper is there
        private List<(Inventory inventory, HashSet<PickupIndex> bazaarStock)> GetPlayerIngredients()
        {
            var terminals = UnityEngine.Object.FindObjectsOfType<ShopTerminalBehavior>();
            bool hasScrapper = UnityEngine.Object.FindObjectOfType<ScrapperController>();
            var players = new List<(Inventory inventory, HashSet<PickupIndex> bazaarStock)>();
            foreach (var pc in PlayerCharacterMasterController.instances)
            {
                var inventory = pc && pc.master ? pc.master.inventory : null;
                if (!inventory)
                {
                    continue;
                }

                var bazaarStock = new HashSet<PickupIndex>();
                foreach (var terminal in terminals)
                {
                    var pickup = terminal != cauldron ? GetBuyablePickup(terminal, pc) : PickupIndex.none;
                    if (pickup != PickupIndex.none)
                    {
                        bazaarStock.Add(pickup);
                    }
                }
                if (hasScrapper)
                {
                    AddScrap(bazaarStock, inventory, ItemTier.Tier1, RoR2Content.Items.ScrapWhite);
                    AddScrap(bazaarStock, inventory, ItemTier.Tier2, RoR2Content.Items.ScrapGreen);
                    AddScrap(bazaarStock, inventory, ItemTier.Tier3, RoR2Content.Items.ScrapRed);
                    AddScrap(bazaarStock, inventory, ItemTier.Boss, RoR2Content.Items.ScrapYellow);
                }
                players.Add((inventory, bazaarStock));
            }
            return players;
        }

        // Instanced shops have their own stock for each player; display-only shops are never available
        private static PickupIndex GetBuyablePickup(ShopTerminalBehavior terminal, PlayerCharacterMasterController pc)
        {
            if (terminal.TryGetComponent(out InstancedPurchase instance))
            {
                var state = instance.GetOrOriginal(pc);
                return state.available && !state.hasBeenPurchased ? state.pickup.pickupIndex : PickupIndex.none;
            }
            var purchaseInteraction = terminal.GetComponent<PurchaseInteraction>();
            return purchaseInteraction && purchaseInteraction.available && !terminal.hasBeenPurchased ? terminal.pickup.pickupIndex : PickupIndex.none;
        }

        // A Scrapper turns an item into the scrap of its tier
        private static void AddScrap(HashSet<PickupIndex> bazaarStock, Inventory inventory, ItemTier tier, ItemDef scrap)
        {
            if (scrap && inventory.GetTotalItemCountOfTier(tier) > 0)
            {
                bazaarStock.Add(PickupCatalog.FindPickupIndex(scrap.itemIndex));
            }
        }

        private void SpawnWanderingChef()
        {
            var chefPos = new SpawnCardStruct(new Vector3(-70.7306f, -23.7171f, -30.4022f), new Vector3(0f, 220f, 0f));
            var cookingPlacePos = new SpawnCardStruct(new Vector3(-72.4183f, -24.4958f, -28.9289f), new Vector3(0f, 220f, 0f));

            if (!ModConfig.WanderingChefUnrestrictedCrafting.Value)
            {
                if (ModConfig.WanderingChefCraftableRecipesOnly.Value)
                {
                    // chosen in CheckAfterSetup; until then the wok shows nothing
                    possibleRecipes = null;
                }
                else
                {
                    NextRecipe();
                }
            }

            // Wandering Chef
            GameObject gameObject = GameObject.Instantiate(mealPrep.WaitForCompletion(), chefPos.Position, Quaternion.identity);
            gameObject.transform.eulerAngles = chefPos.Rotation;
            gameObject.name = "WanderingChef";
            NetworkServer.Spawn(gameObject);
            chef = gameObject;

            // Cooking Cauldron
            gameObject = GameObject.Instantiate(ChefWok_WhitesAndGreens.WaitForCompletion(), cookingPlacePos.Position, Quaternion.identity);
            gameObject.transform.eulerAngles = cookingPlacePos.Rotation;
            gameObject.name = "CookingCauldron";
            var purchaseInteraction = gameObject.GetComponent<PurchaseInteraction>();
            purchaseInteraction.NetworkcostType = CostTypeIndex.None;
            purchaseInteraction.Networkavailable = false;
            cauldron = gameObject.GetComponent<ShopTerminalBehavior>();
            NetworkServer.Spawn(gameObject);
        }
    }
}
