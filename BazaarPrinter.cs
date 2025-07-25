using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHome
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
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake; ;
            On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
        }

        public override void SetupBazaar()
        {
            SpawnPrinters();
        }

        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, RoR2.PurchaseInteraction self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                // 打印机
                if (ModConfig.PrinterCount.Value > 0)
                {
                    if (self.name.StartsWith("Duplicator")
                                || self.name.StartsWith("DuplicatorLarge")
                                || self.name.StartsWith("DuplicatorMilitary")
                                || self.name.StartsWith("DuplicatorWild"))
                    {
                        float w1, w2, w3, w4, w5, w6, total;
                        w1 = ModConfig.PrinterTier1Weight.Value;
                        w2 = ModConfig.PrinterTier2Weight.Value;
                        w3 = ModConfig.PrinterTier3Weight.Value;
                        w4 = ModConfig.PrinterTierBossWeight.Value;
                        w5 = ModConfig.PrinterTierLunarWeight.Value;
                        w6 = ModConfig.PrinterTierVoidWeight.Value;
                        total = w1 + w2 + w3 + w4 + w5 + w6;
                        if (total != 0)
                        {
                            double random = RNG.NextDouble() * total;
                            if (random <= w1) { }
                            else if (random <= w1 + w2) { }
                            else if (random <= w1 + w2 + w3) { }
                            else if (random <= w1 + w2 + w3 + w4) { }
                            else if (random <= w1 + w2 + w3 + w4 + w5)
                            {
                                self.name = "DuplicatorBlue";
                                self.costType = CostTypeIndex.LunarItemOrEquipment;
                            }
                            else
                            {
                                self.name = "DuplicatorPurple";
                                self.costType = CostTypeIndex.RedItem;
                            }
                        }
                    }
                }
            }
        }

        private void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("DuplicatorBlue"))
                {
                    List<PickupIndex> listLunarItem = Run.instance.availableLunarItemDropList;
                    newPickupIndex = listLunarItem[UnityEngine.Random.Range(0, listLunarItem.Count)];
                }
                if (self.name.StartsWith("DuplicatorPurple"))
                {
                    WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                    weightedSelection.AddChoice(Run.instance.availableVoidTier1DropList, 25f);
                    weightedSelection.AddChoice(Run.instance.availableVoidTier2DropList, 25f);
                    weightedSelection.AddChoice(Run.instance.availableVoidTier3DropList, 25f);
                    weightedSelection.AddChoice(Run.instance.availableVoidBossDropList, 25f);
                    List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                    newPickupIndex = list[UnityEngine.Random.Range(0, list.Count)];
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
                    AsyncOperationHandle<InteractableSpawnCard> randomPrinter = GetRandomPrinter();
                    SpawnCard spawnCard = randomPrinter.WaitForCompletion();
                    GameObject printerOne = spawnCard.DoSpawn(DicPrinters[i].Position, Quaternion.identity, new DirectorSpawnRequest(spawnCard, DirectPlacement, Run.instance.runRNG)).spawnedInstance;
                    printerOne.transform.eulerAngles = DicPrinters[i].Rotation;
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


        private AsyncOperationHandle<InteractableSpawnCard> GetRandomPrinter()
        {
            float tier1 = ModConfig.PrinterTier1Weight.Value;
            float tier2 = ModConfig.PrinterTier2Weight.Value;
            float tier3 = ModConfig.PrinterTier3Weight.Value;
            float boss = ModConfig.PrinterTierBossWeight.Value;
            float total = tier1 + tier2 + tier3 + boss;
            double d = RNG.NextDouble() * total;
            if (d <= tier1) return PrintersCode[0];
            else if (d <= tier1 + tier2) return PrintersCode[1];
            else if (d <= tier1 + tier2 + tier3) return PrintersCode[2];
            else return PrintersCode[3];
        }
    }
}
