using EntityStates.Missions.RepurposedCrater;
using RoR2;
using RoR2.Artifacts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    // A Command Chest gives a Command Essence, which offers every item of the tier it rolled.
    // Unmodded clients can't spawn the game's own Command Chest, so it is built from the Alloyed Collective's Ocular Vault:
    // the host takes its interaction, charges the price and plays the vault's own opening or refusal.
    public class BazaarCommandChest : BazaarBase
    {
        // a price sign shows this high above the vault
        private const float SignHeight = 4.5f;
        // with BuyToInventory, the essence stays this far below the vault, out of sight but within the picker's 10 m range
        private const float HiddenEssenceDepth = 5f;

        private class Essence
        {
            public CharacterMaster owner;
            // the chosen item flies from here instead of from the hidden essence
            public Vector3 vaultPosition;
        }

        AsyncOperationHandle<GameObject> vaultPrefab;
        DirectorCard card;
        bool spawned;
        GameObject chest;
        Xoroshiro128Plus rng;
        // players who bought from the chest with instanced purchases
        readonly HashSet<PlayerCharacterMasterController> buyers = new HashSet<PlayerCharacterMasterController>();
        // hidden essences waiting for their owner's choice; interacting with the vault again opens them
        readonly Dictionary<PickupPickerController, Essence> essences = new Dictionary<PickupPickerController, Essence>();

        public override void Preload()
        {
            vaultPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/PowerOrbPedestal/PowerOrbVault.prefab");
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
            buyers.Clear();
            essences.Clear();
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
            bool instanced = ModConfig.CommandChestInstancedPurchases.Value;
            if (!activator || !body || !player)
            {
                return;
            }
            if (TryGetWaitingEssence(body.master, out var waitingEssence))
            {
                // reopens the picker its owner closed
                waitingEssence.OnInteractionBegin(activator);
                return;
            }
            if (instanced && buyers.Contains(player))
            {
                return;
            }

            var costType = ModConfig.CommandChestCostType.Value;
            int cost = Helper.GetCost(ModConfig.CommandChestCost.Value, costType);
            if (!Helper.CanAfford(costType, cost, activator, body) || !TryRollPickup(out var pickup))
            {
                // the vault's own refusal
                self.DoInteractResult(false);
                self.CallRpcInteractResult(false);
                return;
            }

            Helper.PayCost(costType, cost, activator, body, chest, rng);
            if (instanced)
            {
                // the vault stays ready for the others
                buyers.Add(player);
                if (!ModConfig.CommandChestBuyToInventory.Value)
                {
                    SetInteractabilityForClient(player, Interactability.Disabled);
                }
                if (self.activationEffectPrefab && self.activationEffectAttach)
                {
                    EffectManager.SpawnEffect(self.activationEffectPrefab, new EffectData { origin = self.activationEffectAttach.position }, transmit: true);
                }
            }
            else
            {
                // opens the vault for everyone and disables it
                self.DoInteractResult(true);
                self.CallRpcInteractResult(true);
                if (ModConfig.CommandChestBuyToInventory.Value)
                {
                    // the buyer can still reopen their essence's picker at the vault
                    SetInteractabilityForClient(player, Interactability.Available);
                }
            }
            GiveEssence(self, pickup, activator);
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

        // Players with a waiting essence can use the vault to open it; buyers of an instanced chest can't use it again.
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
                if (buyers.Contains(GetPlayer(activator.gameObject)))
                {
                    return Interactability.Disabled;
                }
            }
            return orig(self, activator);
        }

        // Shows the vault as usable or used to this player's client only, with the vault's own interactability field
        private void SetInteractabilityForClient(PlayerCharacterMasterController player, Interactability interactability)
        {
            var connection = player.networkUser ? player.networkUser.connectionToClient : null;
            var identity = chest.GetComponent<NetworkIdentity>();
            var interaction = chest.GetComponent<GenericInteraction>();
            if (connection == null || !connection.isReady || Util.ConnectionIsLocal(connection) || !identity || !interaction)
            {
                return;
            }

            int channel = interaction.GetNetworkChannel();
            var writer = new NetworkWriter();
            writer.StartMessage(MsgType.UpdateVars);
            writer.Write(identity.netId);
            var previousInteractability = interaction.interactability;
            try
            {
                interaction.interactability = interactability;
                // every component must be serialized in the order the client reads them
                foreach (var behaviour in identity.GetBehavioursOfSameChannel(channel, false))
                {
                    uint dirtyBits = behaviour.m_SyncVarDirtyBits;
                    try
                    {
                        // dirty bit 1 is GenericInteraction.interactability
                        behaviour.m_SyncVarDirtyBits = behaviour == interaction ? 1u : 0u;
                        behaviour.OnSerialize(writer, false);
                    }
                    finally
                    {
                        behaviour.m_SyncVarDirtyBits = dirtyBits;
                    }
                }
            }
            finally
            {
                interaction.interactability = previousInteractability;
            }
            writer.FinishMessage();
            if (!connection.SendWriter(writer, channel))
            {
                Log.LogWarning("Could not update the Command Chest for a client.");
            }
        }

        // Rolls the configured tier; the essence then offers every item of that tier
        private static bool TryRollPickup(out UniquePickup pickup)
        {
            var resolvedItems = new Dictionary<PickupIndex, int>();
            ItemStringParser.ItemStringParser.ParseItemString(ModConfig.CommandChestTier.Value, resolvedItems, Log.GetSource(), false);
            foreach (var (pickupIndex, amount) in resolvedItems)
            {
                if (amount > 0 && PickupCatalog.GetPickupDef(pickupIndex) != null)
                {
                    pickup = new UniquePickup(pickupIndex);
                    return true;
                }
            }
            Log.LogError($"Could not get a pickup from the Command Chest Tier: {ModConfig.CommandChestTier.Value}");
            pickup = UniquePickup.none;
            return false;
        }

        private static Vector3 GetEssencePosition(PowerOrbPedestalManager vault)
        {
            var origin = vault.activationEffectAttach ? vault.activationEffectAttach : vault.transform;
            return origin.position + Vector3.up;
        }

        private void GiveEssence(PowerOrbPedestalManager vault, UniquePickup pickup, Interactor activator)
        {
            var position = GetEssencePosition(vault);
            if (ModConfig.CommandChestBuyToInventory.Value)
            {
                SpawnEssenceFor(vault, pickup, activator);
                return;
            }
            var pickupInfo = new GenericPickupController.CreatePickupInfo
            {
                pickup = pickup,
                position = position,
                artifactFlag = GenericPickupController.PickupArtifactFlag.Command
            };
            PickupDropletController.CreatePickupDroplet(pickupInfo, position, Vector3.up * 20f + vault.transform.forward * 2f);
        }

        // Spawns the essence a dropped pickup would turn into, hidden below the vault for good, and opens its picker for the buyer
        private void SpawnEssenceFor(PowerOrbPedestalManager vault, UniquePickup pickup, Interactor activator)
        {
            var essence = Object.Instantiate(CommandArtifactManager.commandCubePrefab, vault.transform.position + Vector3.down * HiddenEssenceDepth, Quaternion.identity);
            essence.GetComponent<PickupIndexNetworker>().NetworkpickupState = pickup;
            var picker = essence.GetComponent<PickupPickerController>();
            picker.SetOptionsFromPickupForCommandArtifact(pickup);
            picker.pickupThatGeneratedOptions = pickup;
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

        // The item chosen from an essence the chest gave goes to the player who chose it
        private void PickupPickerController_CreatePickup(On.RoR2.PickupPickerController.orig_CreatePickup_UniquePickup orig, PickupPickerController self, UniquePickup pickupState)
        {
            var master = self.networkUIPromptController ? self.networkUIPromptController.currentParticipantMaster : null;
            var body = master ? master.GetBody() : null;
            if (!NetworkServer.active || !essences.TryGetValue(self, out var essence) || !body || !body.inventory)
            {
                orig(self, pickupState);
                return;
            }
            // the item flies from the vault, not from where the essence hides
            Helper.GivePickup(body, pickupState.pickupIndex, essence.vaultPosition, true);

            // the essence is used up; the vault no longer opens it
            essences.Remove(self);
            var player = master.playerCharacterMasterController;
            if (player && chest)
            {
                SetInteractabilityForClient(player, Interactability.Disabled);
            }
        }

        private void SpawnCommandChest()
        {
            rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            chest = Object.Instantiate(vaultPrefab.WaitForCompletion(), new Vector3(-63.4376f, -27.1783f, -73.8056f), Quaternion.Euler(0f, 305f, 10f));
            // outside its mission the vault stays retracted; spawning it extended raises its container for every client
            chest.GetComponent<PowerOrbPedestalManager>().Network_hasExtended = true;
            NetworkServer.Spawn(chest);

            var costType = ModConfig.CommandChestCostType.Value;
            PriceSigns.Apply(new List<GameObject> { chest }, Helper.GetCost(ModConfig.CommandChestCost.Value, costType), costType, SignHeight);
        }
    }
}
