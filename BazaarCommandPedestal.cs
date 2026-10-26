using EntityStates.Missions.RepurposedCrater;
using PerClientSync;
using RoR2;
using RoR2.Artifacts;
using RoR2.Networking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    // A Command Pedestal gives a Command Essence, which offers every item of the tier it rolled.
    // Unmodded clients can't spawn the game's own Command Chest, so it is built from the Alloyed Collective's Ocular Vault:
    // the host takes its interaction, charges the price and plays the vault's own opening or refusal.
    // With BuyToInventory the vault only opens a private item picker, and the price is charged once an item is picked.
    // Equipment the pick replaces goes into the vault, where only the buyer can swap it back, for free.
    public class BazaarCommandPedestal : BazaarBase
    {
        // a price sign shows this high above the vault
        private const float SignHeight = 4.5f;
        // with BuyToInventory, the essence stays this far below the vault, out of sight but within the picker's 10 m range
        private const float HiddenEssenceDepth = 5f;
        // the essence shown above the vault rests on whatever is below this point
        private const float DisplayDropHeight = 5f;
        // GenericInteraction's dirty bit for interactability
        private const uint InteractabilityBit = 1u;

        private class Essence
        {
            public CharacterMaster owner;
            // the chosen item flies from here instead of from the hidden essence
            public Vector3 vaultPosition;
        }

        // Picks one of a tier's items. As a drop table, mods that change what drop tables give change this pick too
        private class TierDropTable : PickupDropTable
        {
            public List<PickupIndex> pickups;

            public override int GetPickupCount()
            {
                return pickups != null ? pickups.Count : 0;
            }

            public override UniquePickup GeneratePickupPreReplacement(Xoroshiro128Plus rng)
            {
                return pickups != null && pickups.Count > 0 ? new UniquePickup(rng.NextElementUniform(pickups)) : UniquePickup.none;
            }
        }

        // The director picks the pedestal by its DirectorWeight, then a tier it can afford by the tier weights; the tier sets the essence and the price
        private class CommandPedestalCard : DirectorCard
        {
            public ModConfig.CommandPedestalTier tier;

            public CommandPedestalCard() : base("Command Pedestal", null, ModConfig.CommandPedestalDirectorWeight, () => 1) { }

            private static List<ModConfig.CommandPedestalTier> GetTiers()
            {
                return ModConfig.CommandPedestalTiers.Where(tier => tier.weight.Value > 0f && tier.dropList(Run.instance).Count > 0).ToList();
            }

            // without a tier to offer, there is no pedestal
            public override float Weight => GetTiers().Count > 0 ? base.Weight : 0f;

            protected override int CheapestCost()
            {
                var tiers = GetTiers();
                return tiers.Count == 0 ? int.MaxValue : tiers.Min(tier => System.Math.Max(0, tier.directorCost.Value));
            }

            protected override int PickCost(int credits)
            {
                var weightedSelection = new WeightedSelection<ModConfig.CommandPedestalTier>();
                foreach (var choice in GetTiers())
                {
                    if (System.Math.Max(0, choice.directorCost.Value) <= credits)
                        weightedSelection.AddChoice(choice, choice.weight.Value);
                }
                tier = weightedSelection.Evaluate(UnityEngine.Random.value);
                return System.Math.Max(0, tier.directorCost.Value);
            }
        }

        AsyncOperationHandle<GameObject> vaultPrefab;
        AsyncOperationHandle<GameObject> shrineUseEffect;
        CommandPedestalCard card;
        bool spawned;
        GameObject pedestal;
        Xoroshiro128Plus rng;
        TierDropTable dropTable;
        // bought without instanced purchases; nobody can buy it any more
        bool sold;
        // rolled once per visit, so the essence shown above the vault is the one everyone gets
        UniquePickup offer;
        bool hasOffer;
        GameObject displayEssence;
        // players who bought from the pedestal with instanced purchases
        readonly HashSet<PlayerCharacterMasterController> buyers = new HashSet<PlayerCharacterMasterController>();
        // hidden essences waiting for their owner's choice; interacting with the vault again opens them
        readonly Dictionary<PickupPickerController, Essence> essences = new Dictionary<PickupPickerController, Essence>();
        // equipment a buyer's pick replaced; using the vault again swaps it back
        readonly Dictionary<PlayerCharacterMasterController, EquipmentIndex> storedEquipment = new Dictionary<PlayerCharacterMasterController, EquipmentIndex>();

        public static BazaarCommandPedestal instance;

        public BazaarCommandPedestal()
        {
            instance = this;
        }

        // The items this player could still choose from the pedestal's essence, e.g. for the Wandering CHEF's recipes
        public IEnumerable<PickupIndex> GetChoicesFor(PlayerCharacterMasterController player)
        {
            if (!pedestal || !hasOffer || sold || (ModConfig.CommandPedestalInstancedPurchases.Value && buyers.Contains(player)))
            {
                return Enumerable.Empty<PickupIndex>();
            }
            return PickupTransmutationManager.GetAvailableGroupFromPickupIndex(offer.pickupIndex) ?? Enumerable.Empty<PickupIndex>();
        }

        public override void Preload()
        {
            vaultPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/PowerOrbPedestal/PowerOrbVault.prefab");
            shrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab");
            PriceSigns.Preload();
        }

        public override void Hook()
        {
            On.EntityStates.Missions.RepurposedCrater.PowerOrbPedestalManager.DoOnInteract += PowerOrbPedestalManager_DoOnInteract;
            On.RoR2.GenericInteraction.RoR2_IInteractable_GetInteractability += GenericInteraction_GetInteractability;
            On.RoR2.PickupPickerController.GetInteractability += PickupPickerController_GetInteractability;
            On.RoR2.PickupPickerController.CreatePickup_UniquePickup += PickupPickerController_CreatePickup;
        }

        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            card = null;
            if (ModConfig.CommandPedestalSectionEnabled.Value)
            {
                card = new CommandPedestalCard();
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            spawned = false;
            pedestal = null;
            displayEssence = null;
            hasOffer = false;
            sold = false;
            buyers.Clear();
            essences.Clear();
            storedEquipment.Clear();
            SpawnLatePicks();
        }

        public override void SpawnLatePicks()
        {
            if (card == null || card.Count == 0 || spawned)
                return;
            SpawnCommandPedestal();
            spawned = true;
        }

        private bool IsBazaarChest(Component component)
        {
            return pedestal && component && component.gameObject == pedestal && NetworkServer.active;
        }

        private static PlayerCharacterMasterController GetPlayer(GameObject bodyObject)
        {
            var body = bodyObject ? bodyObject.GetComponent<CharacterBody>() : null;
            return body && body.master ? body.master.playerCharacterMasterController : null;
        }

        // Replaces the vault's check for an Ocular Battery
        private void PowerOrbPedestalManager_DoOnInteract(On.EntityStates.Missions.RepurposedCrater.PowerOrbPedestalManager.orig_DoOnInteract orig, PowerOrbPedestalManager self, GameObject interactorObject)
        {
            if (!IsBazaarChest(self))
            {
                orig(self, interactorObject);
                return;
            }

            var activator = interactorObject ? interactorObject.GetComponent<Interactor>() : null;
            var body = interactorObject ? interactorObject.GetComponent<CharacterBody>() : null;
            var player = GetPlayer(interactorObject);
            if (!activator || !body || !player)
            {
                return;
            }
            if (storedEquipment.ContainsKey(player))
            {
                SwapStoredEquipment(self, player, body);
                return;
            }
            if (sold || (ModConfig.CommandPedestalInstancedPurchases.Value && buyers.Contains(player)))
            {
                return;
            }

            var costType = ModConfig.CommandPedestalCostType.Value;
            int cost = GetPrice();
            if (!Helper.CanAfford(costType, cost, activator, body))
            {
                Refuse(self);
                return;
            }

            if (ModConfig.CommandPedestalBuyToInventory.Value)
            {
                // only opens the picker; the price is charged once an item is picked
                if (TryGetWaitingEssence(body.master, out var waitingEssence))
                {
                    waitingEssence.OnInteractionBegin(activator);
                }
                else if (hasOffer)
                {
                    SpawnHiddenEssence(self, offer, activator);
                }
                else
                {
                    Refuse(self);
                }
                return;
            }

            if (!hasOffer)
            {
                Refuse(self);
                return;
            }
            Helper.PayCost(costType, cost, activator, body, pedestal, rng);
            CompletePurchase(self, player);
            DropEssence(self, offer);
        }

        // The vault's own refusal
        private static void Refuse(PowerOrbPedestalManager vault)
        {
            vault.DoInteractResult(false);
            vault.CallRpcInteractResult(false);
        }

        // Counts a paid purchase: an instanced pedestal stays ready for the others, otherwise the vault opens for everyone
        private void CompletePurchase(PowerOrbPedestalManager vault, PlayerCharacterMasterController player)
        {
            if (ModConfig.CommandPedestalInstancedPurchases.Value)
            {
                buyers.Add(player);
                UpdateSwapAvailability(player);
                HideDisplayFor(player);
                PriceSigns.HideFor(pedestal, player);
                PlayBuyEffect(vault);
                return;
            }

            sold = true;
            if (displayEssence)
            {
                NetworkServer.Destroy(displayEssence);
            }
            vault.DoInteractResult(true);
            vault.CallRpcInteractResult(true);
            // the opening disables the vault for everyone, the buyer's client included; the vault's update after it gives a
            // buyer with stored equipment the swap back
            UpdateSwapAvailability(player);
            // nobody else can buy it now, so their open pickers close
            foreach (var picker in essences.Keys)
            {
                if (picker)
                {
                    NetworkServer.Destroy(picker.gameObject);
                }
            }
            essences.Clear();
        }

        // An instanced pedestal stays closed, so a purchase shows the vault's burst and the shrines' use effect with its sound, in the essence's color
        private void PlayBuyEffect(PowerOrbPedestalManager vault)
        {
            var position = vault.activationEffectAttach ? vault.activationEffectAttach.position : vault.transform.position;
            if (vault.activationEffectPrefab)
            {
                EffectManager.SpawnEffect(vault.activationEffectPrefab, new EffectData { origin = position }, transmit: true);
            }
            var pickupDef = PickupCatalog.GetPickupDef(offer.pickupIndex);
            SpawnEffect(shrineUseEffect, position, pickupDef != null ? pickupDef.baseColor : Color.white);
        }

        private bool TryGetWaitingEssence(CharacterMaster master, out PickupPickerController picker)
        {
            foreach (var entry in essences)
            {
                if (entry.Key && entry.Value.owner == master)
                {
                    picker = entry.Key;
                    return true;
                }
            }
            picker = null;
            return false;
        }

        // Players with a waiting essence can use the vault to open it; buyers of an instanced pedestal can't use it again.
        // This covers the host's own player and the server's checks.
        private Interactability GenericInteraction_GetInteractability(On.RoR2.GenericInteraction.orig_RoR2_IInteractable_GetInteractability orig, GenericInteraction self, Interactor activator)
        {
            if (IsBazaarChest(self) && activator)
            {
                var body = activator.GetComponent<CharacterBody>();
                if (body && body.master && TryGetWaitingEssence(body.master, out _))
                {
                    return Interactability.Available;
                }
                var player = GetPlayer(activator.gameObject);
                if (player && storedEquipment.ContainsKey(player))
                {
                    return Interactability.Available;
                }
                if (buyers.Contains(player))
                {
                    return Interactability.Disabled;
                }
            }
            return orig(self, activator);
        }

        // The vault as each player's client shows it: usable while it holds their equipment, used once they bought from an
        // instanced pedestal. The host's own player and the server's checks go through GetInteractability.
        private class VaultView : ClientView<GenericInteraction>
        {
            public BazaarCommandPedestal owner;
            private Interactability realInteractability;

            protected override void Apply(NetworkConnection client)
            {
                var interaction = Component;
                realInteractability = interaction.interactability;
                var player = PerClient.GetPlayer(client);
                if (player && owner.storedEquipment.ContainsKey(player))
                {
                    interaction.interactability = Interactability.Available;
                }
                else if (player && owner.buyers.Contains(player))
                {
                    interaction.interactability = Interactability.Disabled;
                }
            }

            protected override void Restore()
            {
                Component.interactability = realInteractability;
            }
        }

        // The tier's price, scaled with difficulty if it is paid in money
        private int GetPrice()
        {
            return card != null && card.tier != null ? Helper.GetCost(card.tier.cost.Value, ModConfig.CommandPedestalCostType.Value) : 0;
        }

        // Rolls an item of the director's tier, which mods that change what drop tables give may change; the essence then offers every item of the rolled item's group
        private bool TryRollPickup(out UniquePickup pickup)
        {
            var dropList = card != null && card.tier != null ? card.tier.dropList(Run.instance) : null;
            if (dropList == null || dropList.Count == 0)
            {
                pickup = UniquePickup.none;
                return false;
            }
            if (!dropTable)
            {
                dropTable = ScriptableObject.CreateInstance<TierDropTable>();
                // the tier's item, not a lunar one from Eulogy Zero
                dropTable.canDropBeReplaced = false;
            }
            dropTable.pickups = dropList;
            pickup = dropTable.GeneratePickup(rng);
            return true;
        }

        private static Vector3 GetEssencePosition(PowerOrbPedestalManager vault)
        {
            var origin = vault.activationEffectAttach ? vault.activationEffectAttach : vault.transform;
            return origin.position + Vector3.up;
        }

        // A Command Essence drops out of the vault; anyone can use it, and the chosen item drops
        private static void DropEssence(PowerOrbPedestalManager vault, UniquePickup pickup)
        {
            var position = GetEssencePosition(vault);
            var pickupInfo = new GenericPickupController.CreatePickupInfo
            {
                pickup = pickup,
                position = position,
                artifactFlag = GenericPickupController.PickupArtifactFlag.Command
            };
            PickupDropletController.CreatePickupDroplet(pickupInfo, position, Vector3.up * 20f + vault.transform.forward * 2f);
        }

        // Spawns the essence a dropped pickup would turn into, hidden below the vault for good, and opens its picker for the player
        private void SpawnHiddenEssence(PowerOrbPedestalManager vault, UniquePickup pickup, Interactor activator)
        {
            var essence = Object.Instantiate(CommandArtifactManager.commandCubePrefab, vault.transform.position + Vector3.down * HiddenEssenceDepth, Quaternion.identity);
            essence.GetComponent<PickupIndexNetworker>().NetworkpickupState = pickup;
            var picker = essence.GetComponent<PickupPickerController>();
            picker.SetOptionsFromPickupForCommandArtifact(pickup);
            picker.pickupThatGeneratedOptions = pickup;
            // the host closes the picker once its player is 10 m from this copy, so it must not fall; clients' copies fall unseen
            if (essence.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.isKinematic = true;
            }
            NetworkServer.Spawn(essence);

            var body = activator.GetComponent<CharacterBody>();
            essences[picker] = new Essence { owner = body ? body.master : null, vaultPosition = GetEssencePosition(vault) };
            picker.OnInteractionBegin(activator);
        }

        private Interactability PickupPickerController_GetInteractability(On.RoR2.PickupPickerController.orig_GetInteractability orig, PickupPickerController self, Interactor activator)
        {
            if (NetworkServer.active && essences.TryGetValue(self, out var essence) && essence.owner)
            {
                var body = activator ? activator.GetComponent<CharacterBody>() : null;
                if (!body || body.master != essence.owner)
                {
                    return Interactability.Disabled;
                }
            }
            return orig(self, activator);
        }

        // Picking an item from a hidden essence buys it: the price is charged and the item goes to the player who picked it
        private void PickupPickerController_CreatePickup(On.RoR2.PickupPickerController.orig_CreatePickup_UniquePickup orig, PickupPickerController self, UniquePickup pickupState)
        {
            if (!NetworkServer.active || !essences.TryGetValue(self, out var essence))
            {
                orig(self, pickupState);
                return;
            }
            // any choice uses the essence up
            essences.Remove(self);

            var master = self.networkUIPromptController ? self.networkUIPromptController.currentParticipantMaster : null;
            var body = master ? master.GetBody() : null;
            var activator = body ? body.GetComponent<Interactor>() : null;
            var player = master ? master.playerCharacterMasterController : null;
            var vault = pedestal ? pedestal.GetComponent<PowerOrbPedestalManager>() : null;
            var costType = ModConfig.CommandPedestalCostType.Value;
            int cost = GetPrice();
            if (!vault || !body || !body.inventory || !activator || !player || sold
                || (ModConfig.CommandPedestalInstancedPurchases.Value && buyers.Contains(player))
                || !Helper.CanAfford(costType, cost, activator, body))
            {
                // nothing is charged or given; the vault can be used again
                if (vault)
                {
                    Refuse(vault);
                }
                return;
            }

            Helper.PayCost(costType, cost, activator, body, pedestal, rng);
            // the item flies from the vault, not from where the essence hides
            GiveAndStore(body, player, pickupState.pickupIndex, essence.vaultPosition);
            CompletePurchase(vault, player);
        }

        // Gives the pickup; equipment it pushes out flies into the vault, where only this player can swap it back
        private void GiveAndStore(CharacterBody body, PlayerCharacterMasterController player, PickupIndex pickupIndex, Vector3 vaultPosition)
        {
            var replaced = Helper.GivePickup(body, pickupIndex, vaultPosition, false);
            if (replaced == EquipmentIndex.None)
            {
                storedEquipment.Remove(player);
                return;
            }
            storedEquipment[player] = replaced;
            Helper.CreateItemTakenOrb(body.corePosition, pedestal, PickupCatalog.FindPickupIndex(replaced));
        }

        // Swaps the buyer's stored equipment back in, for free; the equipment it replaces goes into the vault in turn
        private void SwapStoredEquipment(PowerOrbPedestalManager vault, PlayerCharacterMasterController player, CharacterBody body)
        {
            var stored = storedEquipment[player];
            storedEquipment.Remove(player);
            GiveAndStore(body, player, PickupCatalog.FindPickupIndex(stored), GetEssencePosition(vault));
            UpdateSwapAvailability(player);
        }

        // A buyer can use the vault while it holds their equipment; their client gets it with the vault's next update
        private void UpdateSwapAvailability(PlayerCharacterMasterController player)
        {
            if (pedestal && pedestal.TryGetComponent(out VaultView view))
            {
                view.SetDirty(PerClient.GetConnection(player), InteractabilityBit);
            }
        }

        // The buyer of an instanced pedestal no longer sees the essence it gives; the others still do
        private void HideDisplayFor(PlayerCharacterMasterController player)
        {
            var identity = displayEssence ? displayEssence.GetComponent<NetworkIdentity>() : null;
            var connection = PerClient.GetConnection(player);
            if (!identity || connection == null)
            {
                return;
            }
            if (PerClient.IsLocal(connection))
            {
                // the host sees the server's own essence, so only its visuals go
                var visuals = displayEssence.transform.Find("PickupDisplay");
                if (visuals)
                {
                    visuals.gameObject.SetActive(false);
                }
                return;
            }
            PerClient.Hide(identity, connection);
        }

        // Shows the essence the vault gives above it, as a Command Essence nobody can use.
        // Clients run its physics themselves, so it is placed where it comes to rest.
        private void SpawnDisplayEssence()
        {
            var prefab = CommandArtifactManager.commandCubePrefab;
            var collider = prefab ? prefab.GetComponent<SphereCollider>() : null;
            if (!collider)
            {
                return;
            }
            int layerMask = 0;
            for (int layer = 0; layer < 32; layer++)
            {
                if (!Physics.GetIgnoreLayerCollision(prefab.layer, layer))
                {
                    layerMask |= 1 << layer;
                }
            }
            var origin = pedestal.transform.position + Vector3.up * DisplayDropHeight;
            if (!Physics.SphereCast(origin, collider.radius, Vector3.down, out var hit, DisplayDropHeight * 2f, layerMask, QueryTriggerInteraction.Ignore))
            {
                Log.LogWarning("Found nothing for the Command Pedestal's essence to rest on.");
                return;
            }

            displayEssence = Object.Instantiate(prefab, origin + Vector3.down * hit.distance, Quaternion.identity);
            displayEssence.GetComponent<PickupIndexNetworker>().NetworkpickupState = offer;
            displayEssence.GetComponent<PickupPickerController>().SetAvailable(false);
            if (displayEssence.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.isKinematic = true;
            }
            NetworkServer.Spawn(displayEssence);
            Log.LogDebug($"The Command Pedestal's essence rests on {hit.collider.name}, {displayEssence.transform.position.y - pedestal.transform.position.y:0.00} m above the pedestal.");
        }

        private void SpawnCommandPedestal()
        {
            rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            pedestal = Object.Instantiate(vaultPrefab.WaitForCompletion(), new Vector3(-63.9613f, -26.3518f, -73.8056f), Quaternion.Euler(0f, 300f, 0f));
            pedestal.AddComponent<VaultView>().owner = this;
            NetworkServer.Spawn(pedestal);

            hasOffer = TryRollPickup(out offer);
            if (hasOffer)
            {
                SpawnDisplayEssence();
            }

            Log.LogDebug($"The Command Pedestal offers {card.tier.name}.");
            PriceSigns.Apply(new List<GameObject> { pedestal }, GetPrice(), ModConfig.CommandPedestalCostType.Value, SignHeight);
        }
    }
}
