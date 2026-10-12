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
    public class BazaarPrinter : BazaarBase
    {
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicator;
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicatorLarge;
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicatorMilitary;
        AsyncOperationHandle<InteractableSpawnCard> iscDuplicatorWild;
        AsyncOperationHandle<InteractableSpawnCard>[] PrintersCode;

        Dictionary<int, SpawnCardStruct> DicPrinters = new Dictionary<int, SpawnCardStruct>();
        PrinterCard card;
        int spawned;
        public override void Preload()
        {
            iscDuplicator = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Duplicator/iscDuplicator.asset");
            iscDuplicatorLarge = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorLarge/iscDuplicatorLarge.asset");
            iscDuplicatorMilitary = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset");
            iscDuplicatorWild = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset");
            PrintersCode = [
                iscDuplicator,
                iscDuplicatorLarge,
                iscDuplicatorMilitary,
                iscDuplicatorWild
            ];
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
            if (ModConfig.PrinterSectionEnabled.Value)
            {
                if (GetTierChoices().Count == 0)
                {
                    Log.LogWarning("No printer tiers with a valid weight; skipping printers.");
                    return;
                }
                card = new PrinterCard();
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            if (card != null)
            {
                DicPrinters.Clear();
                SetPrinter();
                spawned = 0;
                SpawnLatePicks();
            }
        }

        public override void SpawnLatePicks()
        {
            if (card == null)
                return;
            for (; spawned < card.tiers.Count; spawned++)
            {
                SpawnPrinter(card.tiers[spawned], DicPrinters[spawned]);
            }
        }

        private void ShopTerminalBehavior_SetPickup(On.RoR2.ShopTerminalBehavior.orig_SetPickup orig, ShopTerminalBehavior self, UniquePickup newPickup, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.PrinterSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("Duplicator"))
                {
                    var nameWithoutDuplicatorPrefix = self.name.Substring("Duplicator".Length);
                    var endsWithItemTier = Enum.TryParse(nameWithoutDuplicatorPrefix, out ItemTier itemTier);
                    if (nameWithoutDuplicatorPrefix == "Void") // ItemTier.Void has no entry
                    {
                        itemTier = ItemTier.NoTier;
                        endsWithItemTier = true;
                    }

                    if (endsWithItemTier)
                    {
                        WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();

                        //Local helper to add choices without empty ones.
                        void AddDropList(List<PickupIndex> drops)
                        {
                            if (drops.Count > 0)
                                weightedSelection.AddChoice(drops, 25f);
                        }

                        switch (itemTier)
                        {
                            case ItemTier.VoidTier1:
                                AddDropList(Run.instance.availableVoidTier1DropList);
                                break;
                            case ItemTier.VoidTier2:
                                AddDropList(Run.instance.availableVoidTier2DropList);
                                break;
                            case ItemTier.VoidTier3:
                                AddDropList(Run.instance.availableVoidTier3DropList);
                                break;
                            case ItemTier.VoidBoss:
                                AddDropList(Run.instance.availableVoidBossDropList);
                                break;
                            case ItemTier.NoTier:
                                AddDropList(Run.instance.availableVoidTier1DropList);
                                AddDropList(Run.instance.availableVoidTier2DropList);
                                AddDropList(Run.instance.availableVoidTier3DropList);
                                AddDropList(Run.instance.availableVoidBossDropList);
                                break;
                        }

                        //Handle no eligible pool
                        if (weightedSelection.Count > 0)
                        {
                            List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                            newPickup.pickupIndex = list[UnityEngine.Random.Range(0, list.Count)];
                        }
                    }
                }
            }
            orig(self, newPickup, newHidden);
        }

        private void SpawnPrinter(ItemTier tier, SpawnCardStruct slot)
        {
            SpawnCard spawnCard = null;
            string nonDefaultName = null;
            switch (tier)
            {
                case ItemTier.Tier1:
                    spawnCard = iscDuplicator.WaitForCompletion();
                    break;
                case ItemTier.Tier2:
                    spawnCard = iscDuplicatorLarge.WaitForCompletion();
                    break;
                case ItemTier.Tier3:
                    spawnCard = iscDuplicatorMilitary.WaitForCompletion();
                    break;
                case ItemTier.Boss:
                    spawnCard = iscDuplicatorWild.WaitForCompletion();
                    break;
                case ItemTier.VoidTier1:
                case ItemTier.VoidTier2:
                case ItemTier.VoidTier3:
                case ItemTier.VoidBoss:
                    spawnCard = iscDuplicatorMilitary.WaitForCompletion();
                    nonDefaultName = "Duplicator" + tier.ToString();
                    break;
                case ItemTier.NoTier:
                    spawnCard = iscDuplicatorMilitary.WaitForCompletion();
                    nonDefaultName = "DuplicatorVoid";
                    break;
            }
            GameObject printer = SpawnAt(spawnCard.prefab, slot);
            if (nonDefaultName != null)
                printer.name = nonDefaultName;
        }

        // Void printers use the red printer, so they cost the same
        private static int GetTierCost(ItemTier tier)
        {
            switch (tier)
            {
                case ItemTier.Tier1:
                    return Math.Max(0, ModConfig.PrinterDirectorCostWhite.Value);
                case ItemTier.Tier2:
                    return Math.Max(0, ModConfig.PrinterDirectorCostGreen.Value);
                case ItemTier.Boss:
                    return Math.Max(0, ModConfig.PrinterDirectorCostBoss.Value);
                default:
                    return Math.Max(0, ModConfig.PrinterDirectorCostRed.Value);
            }
        }

        // The tier is rolled among the ones the director can afford, since printers cost by their kind
        private class PrinterCard : DirectorCard
        {
            public readonly List<ItemTier> tiers = new List<ItemTier>();

            public PrinterCard() : base("Printer", null, ModConfig.PrinterDirectorWeight, () => ModConfig.PrinterMaxAmount.Value) { }

            protected override int CheapestCost()
            {
                var choices = GetTierChoices();
                return choices.Count == 0 ? int.MaxValue : choices.Min(choice => GetTierCost(choice.tier));
            }

            protected override int PickCost(int credits)
            {
                WeightedSelection<ItemTier> weightedSelection = new WeightedSelection<ItemTier>();
                foreach (var choice in GetTierChoices())
                {
                    if (GetTierCost(choice.tier) <= credits)
                        weightedSelection.AddChoice(choice.tier, choice.weight);
                }
                ItemTier tier = weightedSelection.Evaluate(UnityEngine.Random.value);
                tiers.Add(tier);
                return GetTierCost(tier);
            }
        }

        private void SetPrinter()
        {
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicPrinters.Add(random[0], new SpawnCardStruct(new Vector3(-112f, -26.8f, -46.0f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[1], new SpawnCardStruct(new Vector3(-108f, -26.8f, -48.5f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[2], new SpawnCardStruct(new Vector3(-104f, -26.7f, -51.0f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[3], new SpawnCardStruct(new Vector3(-127f, -26.0f, -34.5f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[4], new SpawnCardStruct(new Vector3(-131f, -26.0f, -31.8f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[5], new SpawnCardStruct(new Vector3(-135f, -26.0f, -29.0f), new Vector3(0.0f, 32.2f, 0.0f)));
            DicPrinters.Add(random[6], new SpawnCardStruct(new Vector3(-144f, -24.7f, -24.0f), new Vector3(0.0f, 60.2f, 0.0f)));
            DicPrinters.Add(random[7], new SpawnCardStruct(new Vector3(-145f, -25.0f, -20.0f), new Vector3(0.0f, 80.0f, 0.0f)));
            DicPrinters.Add(random[8], new SpawnCardStruct(new Vector3(-146f, -25.3f, -16.0f), new Vector3(0.0f, 100.0f, 0.0f)));
        }

        private static List<(ItemTier tier, float weight)> GetTierChoices()
        {
            var choices = new List<(ItemTier tier, float weight)>();

            //Same localized helper to handle weights and counts and avoid empty choices.
            void AddTier(ItemTier choice, float weight, int availableCount)
            {
                if (availableCount > 0 && weight > 0)
                    choices.Add((choice, weight));
            }

            AddTier(ItemTier.Tier1, ModConfig.PrinterTier1Weight.Value, Run.instance.availableTier1DropList.Count);
            AddTier(ItemTier.Tier2, ModConfig.PrinterTier2Weight.Value, Run.instance.availableTier2DropList.Count);
            AddTier(ItemTier.Tier3, ModConfig.PrinterTier3Weight.Value, Run.instance.availableTier3DropList.Count);
            AddTier(ItemTier.Boss, ModConfig.PrinterTierBossWeight.Value, Run.instance.availableBossDropList.Count);
            AddTier(ItemTier.VoidTier1, ModConfig.PrinterTierVoid1Weight.Value, Run.instance.availableVoidTier1DropList.Count);
            AddTier(ItemTier.VoidTier2, ModConfig.PrinterTierVoid2Weight.Value, Run.instance.availableVoidTier2DropList.Count);
            AddTier(ItemTier.VoidTier3, ModConfig.PrinterTierVoid3Weight.Value, Run.instance.availableVoidTier3DropList.Count);
            AddTier(ItemTier.VoidBoss, ModConfig.PrinterTierVoidBossWeight.Value, Run.instance.availableVoidBossDropList.Count);
            int voidCount = Run.instance.availableVoidTier1DropList.Count + Run.instance.availableVoidTier2DropList.Count +
                Run.instance.availableVoidTier3DropList.Count + Run.instance.availableVoidBossDropList.Count;
            AddTier(ItemTier.NoTier, ModConfig.PrinterTierVoidAnyWeight.Value, voidCount);
            return choices;
        }
    }
}
