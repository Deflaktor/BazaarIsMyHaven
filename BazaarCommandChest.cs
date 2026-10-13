using RoR2;
using RoR2.Artifacts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    // The game's Command Chest drops a Command Essence, which offers every item of the tier the chest rolled
    public class BazaarCommandChest : BazaarBase
    {
        AsyncOperationHandle<GameObject> commandChest;
        DirectorCard card;
        bool spawned;
        GameObject chest;
        // the player opening the chest, while the purchase runs
        Interactor buyer;
        // who opened the chest without instanced purchases, until it gives its essence
        Interactor openedBy;
        // essences given to one player; only they can use it
        readonly Dictionary<PickupPickerController, CharacterMaster> essenceOwners = new Dictionary<PickupPickerController, CharacterMaster>();

        public override void Preload()
        {
            commandChest = Addressables.LoadAssetAsync<GameObject>("RoR2/CommandChest/CommandChest.prefab");
        }

        public override void Hook()
        {
            On.RoR2.ChestBehavior.Roll += ChestBehavior_Roll;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ChestBehavior.Open += ChestBehavior_Open;
            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
            On.RoR2.PickupPickerController.GetInteractability += PickupPickerController_GetInteractability;
            On.RoR2.PickupPickerController.CreatePickup_UniquePickup += PickupPickerController_CreatePickup;
        }

        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            card = null;
            if (ModConfig.CommandChestSectionEnabled.Value)
            {
                card = new DirectorCard("Command Chest", ModConfig.CommandChestDirectorCost, ModConfig.CommandChestDirectorWeight, () => 1);
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            spawned = false;
            chest = null;
            buyer = null;
            openedBy = null;
            essenceOwners.Clear();
            SpawnLatePicks();
        }

        public override void SpawnLatePicks()
        {
            if (card == null || card.Count == 0 || spawned)
                return;
            SpawnCommandChest();
            spawned = true;
        }

        private bool IsBazaarChest(Component component)
        {
            return chest && component && component.gameObject == chest && NetworkServer.active;
        }

        // Rolls the configured tier; the essence then offers every item of that tier
        private void ChestBehavior_Roll(On.RoR2.ChestBehavior.orig_Roll orig, ChestBehavior self)
        {
            orig(self);
            if (!IsBazaarChest(self))
                return;

            var resolvedItems = new Dictionary<PickupIndex, int>();
            ItemStringParser.ItemStringParser.ParseItemString(ModConfig.CommandChestTier.Value, resolvedItems, Log.GetSource(), false);
            bool rolled = false;
            foreach (var (pickupIndex, amount) in resolvedItems)
            {
                if (amount > 0 && PickupCatalog.GetPickupDef(pickupIndex) != null)
                {
                    self.currentPickup = new UniquePickup(pickupIndex);
                    rolled = true;
                    break;
                }
            }
            if (!rolled)
            {
                Log.LogError($"Could not get a pickup from the Command Chest Tier: {ModConfig.CommandChestTier.Value}; it keeps the game's own roll");
            }

            // the chest's own name token is missing from the game's language files
            if (self.TryGetComponent(out PurchaseInteraction purchaseInteraction))
            {
                purchaseInteraction.NetworkdisplayNameToken = GetEssenceNameToken(self.currentPickup.pickupIndex);
            }
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (!IsBazaarChest(self))
            {
                orig(self, activator);
                return;
            }
            var previousBuyer = buyer;
            buyer = activator;
            try
            {
                orig(self, activator);
            }
            finally
            {
                buyer = previousBuyer;
            }
        }

        // An instanced chest stays closed for the others and gives each buyer their own essence
        private void ChestBehavior_Open(On.RoR2.ChestBehavior.orig_Open orig, ChestBehavior self)
        {
            if (!IsBazaarChest(self))
            {
                orig(self);
                return;
            }
            if (!self.GetComponent<InstancedPurchase>())
            {
                // the opening chest gives its essence in ItemDrop
                openedBy = buyer;
                orig(self);
                return;
            }
            GiveEssence(self, buyer);
            self.Roll();
        }

        private void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (!IsBazaarChest(self) || !ModConfig.CommandChestBuyToInventory.Value || !openedBy)
            {
                orig(self);
                return;
            }
            SpawnEssenceFor(self, openedBy);
            openedBy = null;
            self.currentPickup = UniquePickup.none;
            self.NetworkisChestOpened = true;
        }

        private void GiveEssence(ChestBehavior chestBehavior, Interactor activator)
        {
            if (ModConfig.CommandChestBuyToInventory.Value && activator)
            {
                SpawnEssenceFor(chestBehavior, activator);
                return;
            }
            var dropTransform = chestBehavior.dropTransform ? chestBehavior.dropTransform : chestBehavior.transform;
            var pickupInfo = new GenericPickupController.CreatePickupInfo
            {
                pickup = chestBehavior.currentPickup,
                position = dropTransform.position + Vector3.up * 1.5f,
                chest = chestBehavior,
                artifactFlag = GenericPickupController.PickupArtifactFlag.Command
            };
            PickupDropletController.CreatePickupDroplet(pickupInfo, pickupInfo.position, Vector3.up * chestBehavior.dropUpVelocityStrength + dropTransform.forward * chestBehavior.dropForwardVelocityStrength);
        }

        // Spawns the essence a dropped pickup would turn into, but right away at the chest, and opens its picker for the buyer
        private void SpawnEssenceFor(ChestBehavior chestBehavior, Interactor activator)
        {
            var pickup = chestBehavior.currentPickup;
            var dropTransform = chestBehavior.dropTransform ? chestBehavior.dropTransform : chestBehavior.transform;
            var essence = Object.Instantiate(CommandArtifactManager.commandCubePrefab, dropTransform.position + Vector3.up * 1.5f, Quaternion.identity);
            essence.GetComponent<PickupIndexNetworker>().NetworkpickupState = pickup;
            var picker = essence.GetComponent<PickupPickerController>();
            picker.SetOptionsFromPickupForCommandArtifact(pickup);
            picker.chestGeneratedFrom = chestBehavior;
            picker.pickupThatGeneratedOptions = pickup;
            NetworkServer.Spawn(essence);

            var body = activator.GetComponent<CharacterBody>();
            if (body && body.master)
            {
                essenceOwners[picker] = body.master;
            }
            picker.OnInteractionBegin(activator);
        }

        private Interactability PickupPickerController_GetInteractability(On.RoR2.PickupPickerController.orig_GetInteractability orig, PickupPickerController self, Interactor activator)
        {
            if (NetworkServer.active && essenceOwners.TryGetValue(self, out var owner) && owner)
            {
                var body = activator ? activator.GetComponent<CharacterBody>() : null;
                if (!body || body.master != owner)
                {
                    return Interactability.Disabled;
                }
            }
            return orig(self, activator);
        }

        // The item chosen from the chest's essence goes to the player who chose it
        private void PickupPickerController_CreatePickup(On.RoR2.PickupPickerController.orig_CreatePickup_UniquePickup orig, PickupPickerController self, UniquePickup pickupState)
        {
            var master = self.networkUIPromptController ? self.networkUIPromptController.currentParticipantMaster : null;
            var body = master ? master.GetBody() : null;
            if (!ModConfig.CommandChestBuyToInventory.Value || !IsBazaarChest(self.chestGeneratedFrom) || !body || !body.inventory)
            {
                orig(self, pickupState);
                return;
            }
            Helper.GivePickup(body, pickupState.pickupIndex, self.transform.position, true);
        }

        private static string GetEssenceNameToken(PickupIndex pickupIndex)
        {
            var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            if (pickupDef == null)
                return "ARTIFACT_COMMAND_CUBE_UNKNOWN_NAME";
            if (pickupDef.equipmentIndex != EquipmentIndex.None)
                return pickupDef.isLunar ? "ARTIFACT_COMMAND_CUBE_BLUE_NAME" : "ARTIFACT_COMMAND_CUBE_ORANGE_NAME";
            switch (pickupDef.itemTier)
            {
                case ItemTier.Tier1:
                    return "ARTIFACT_COMMAND_CUBE_WHITE_NAME";
                case ItemTier.Tier2:
                    return "ARTIFACT_COMMAND_CUBE_GREEN_NAME";
                case ItemTier.Tier3:
                    return "ARTIFACT_COMMAND_CUBE_RED_NAME";
                case ItemTier.Boss:
                    return "ARTIFACT_COMMAND_CUBE_YELLOW_NAME";
                case ItemTier.Lunar:
                    return "ARTIFACT_COMMAND_CUBE_BLUE_NAME";
                default:
                    return "ARTIFACT_COMMAND_CUBE_UNKNOWN_NAME";
            }
        }

        private void SpawnCommandChest()
        {
            chest = SpawnAt(commandChest.WaitForCompletion(), new SpawnCardStruct(new Vector3(-63.4376f, -27.1783f, -73.8056f), new Vector3(0f, 305f, 10f)));
            var purchaseInteraction = chest.GetComponent<PurchaseInteraction>();
            var cost = Helper.GetCost(ModConfig.CommandChestCost.Value, ModConfig.CommandChestCostType.Value);
            purchaseInteraction.cost = cost;
            purchaseInteraction.Networkcost = cost;
            purchaseInteraction.NetworkcostType = ModConfig.CommandChestCostType.Value;
            // the chest's own context token is missing from the game's language files
            purchaseInteraction.NetworkcontextToken = "CHEST1_CONTEXT";

            if (ModConfig.CommandChestInstancedPurchases.Value)
            {
                var instancedPurchase = chest.AddComponent<InstancedPurchase>();
                instancedPurchase.original.available = purchaseInteraction.available;
            }
        }
    }
}
