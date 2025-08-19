using RoR2;
using System;
using System.Collections.Generic;
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
        public override void Init()
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
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
        }

        public override void SetupBazaar()
        {
            if(ModConfig.PrinterSectionEnabled.Value)
            {
                SpawnPrinters();
            }
        }

        private void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && ModConfig.PrinterSectionEnabled.Value && IsCurrentMapInBazaar() && NetworkServer.active)
            {
                if (self.name.StartsWith("Duplicator"))
                {
                    var nameWithoutDuplicatorPrefix = self.name.Substring("Duplicator".Length);
                    var endsWithItemTier = Enum.TryParse(nameWithoutDuplicatorPrefix, out ItemTier itemTier);
                    if (endsWithItemTier)
                    {
                        WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();
                        switch (itemTier)
                        {
                            case ItemTier.VoidTier1:
                                weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 25f);
                                break;
                            case ItemTier.VoidTier2:
                                weightedSelection.AddChoice(Run.instance.availableVoidTier2DropList, 25f);
                                break;
                            case ItemTier.VoidTier3:
                                weightedSelection.AddChoice(Run.instance.availableVoidTier3DropList, 25f);
                                break;
                            case ItemTier.VoidBoss:
                                weightedSelection.AddChoice(Run.instance.availableVoidBossDropList, 25f);
                                break;
                            case ItemTier.NoTier:
                                weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 25f);
                                weightedSelection.AddChoice(Run.instance.availableVoidTier2DropList, 25f);
                                weightedSelection.AddChoice(Run.instance.availableVoidTier3DropList, 25f);
                                weightedSelection.AddChoice(Run.instance.availableVoidBossDropList, 25f);
                                break;
                        }
                        List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                        newPickupIndex = list[UnityEngine.Random.Range(0, list.Count)];
                    }
                }
            }
            orig(self, newPickupIndex, newHidden);
        }

        private void SpawnPrinters()
        {
            if (ModConfig.PrinterCount.Value > 0)
            {
                // 打印机
                DicPrinters.Clear();
                SetPrinter();
                int count = 0;
                if (ModConfig.SpawnCountByStage.Value)
                    count = SetCountbyGameStage(ModConfig.PrinterCount.Value, ModConfig.SpawnCountOffset.Value);
                else
                    count = ModConfig.PrinterCount.Value;
                for (int i = 0; i < count; i++)
                {
                    var tier = GetRandomPrinterTier();
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
                    GameObject printer = spawnCard.DoSpawn(DicPrinters[i].Position, Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectPlacement, Run.instance.runRNG)).spawnedInstance;
                    if (nonDefaultName != null)
                        printer.name = nonDefaultName;
                    printer.transform.eulerAngles = DicPrinters[i].Rotation;
                }
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

        private ItemTier GetRandomPrinterTier()
        {
            WeightedSelection<ItemTier> weightedSelection = new WeightedSelection<ItemTier>();
            weightedSelection.AddChoice(ItemTier.Tier1, ModConfig.PrinterTier1Weight.Value);
            weightedSelection.AddChoice(ItemTier.Tier2, ModConfig.PrinterTier2Weight.Value);
            weightedSelection.AddChoice(ItemTier.Tier3, ModConfig.PrinterTier3Weight.Value);
            weightedSelection.AddChoice(ItemTier.Boss, ModConfig.PrinterTierBossWeight.Value);
            weightedSelection.AddChoice(ItemTier.VoidTier1, ModConfig.PrinterTierVoid1Weight.Value);
            weightedSelection.AddChoice(ItemTier.VoidTier2, ModConfig.PrinterTierVoid2Weight.Value);
            weightedSelection.AddChoice(ItemTier.VoidTier3, ModConfig.PrinterTierVoid3Weight.Value);
            weightedSelection.AddChoice(ItemTier.VoidBoss, ModConfig.PrinterTierVoidBossWeight.Value);
            weightedSelection.AddChoice(ItemTier.NoTier, ModConfig.PrinterTierVoidAllWeight.Value);
            var tier = weightedSelection.Evaluate(UnityEngine.Random.value);
            return tier;
        }
    }
}
