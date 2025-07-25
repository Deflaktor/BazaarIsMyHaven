using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHome
{
    public class BazaarLunarShop : BazaarBase
    {
        AsyncOperationHandle<GameObject> lunarShopTerminal;
        AsyncOperationHandle<GameObject> LunarRerollEffect;

        Dictionary<int, SpawnCardStruct> DicLunarShopTerminals = new Dictionary<int, SpawnCardStruct>();
        int LunarShopTerminalTotalCount = 0;
        List<PurchaseInteraction> ObjectLunarShopTerminals = new List<PurchaseInteraction>();
        List<PurchaseInteraction> ObjectLunarShopTerminals_Spawn = new List<PurchaseInteraction>();

        public override void Init()
        {
            lunarShopTerminal = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab");
            LunarRerollEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarRecycler/LunarRerollEffect.prefab");
        }

        public override void Hook()
        {
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.EntityStates.NewtMonster.SpawnState.OnEnter += SpawnState_OnEnter;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.PurchaseInteraction.ScaleCost += PurchaseInteraction_ScaleCost;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
        }

        public override void SetupBazaar()
        {
            SpawnLunarShopTerminal();
        }

        public void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                // 月球装备
                if (ModConfig.LunarShopTerminalCount.Value > 0)
                {
                    if (ModConfig.EnableLunarShopTerminalInjection.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        if (self.name.StartsWith("LunarShopTerminal") || self.name.StartsWith("MyLunarBud"))
                        {
                            self.cost = ModConfig.LunarShopTerminalCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.LunarShopTerminalCost.Value * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
                else
                {
                    if (ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        if (self.name.StartsWith("LunarShopTerminal") || self.name.StartsWith("MyLunarBud"))
                        {
                            self.cost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
                if (self.name.StartsWith("LunarShopTerminal"))
                {
                    if (LunarShopTerminalTotalCount < 5)
                    {
                        ObjectLunarShopTerminals.Add(self);
                    }
                }

                // 切片
                if (ModConfig.EnableLunarRecyclerInjection.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                {
                    if (self.name.StartsWith("LunarRecycler"))
                    {
                        // 失效 不能购买
                        self.available = ModConfig.LunarRecyclerAvailable.Value;
                        self.Networkavailable = ModConfig.LunarRecyclerAvailable.Value;

                        if (ModConfig.LunarRecyclerAvailable.Value)
                        {
                            self.cost = ModConfig.LunarRecyclerRerollCost.Value * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = ModConfig.LunarRecyclerRerollCost.Value * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
                else
                {
                    if (ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        if (self.name.StartsWith("LunarRecycler"))
                        {
                            self.cost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                            self.Networkcost = self.cost * ModConfig.PenaltyCoefficient_Temp;
                        }
                    }
                }
            }
        }

        private void SpawnState_OnEnter(On.EntityStates.NewtMonster.SpawnState.orig_OnEnter orig, EntityStates.NewtMonster.SpawnState self)
        {
            if (ModConfig.EnableMod.Value)
            {
                // 重写月球装备
                if (LunarShopTerminalTotalCount < 5)
                {
                    // 先改成禁用
                    ObjectLunarShopTerminals.ForEach(x => x.SetAvailable(false));
                    // 打乱List
                    ObjectLunarShopTerminals = DisorderList(ObjectLunarShopTerminals);
                    // 逐个启用
                    for (int i = 0; i < LunarShopTerminalTotalCount; i++)
                    {
                        ObjectLunarShopTerminals[i].SetAvailable(true);
                    }
                    // 没启用的删除
                    foreach (PurchaseInteraction interaction in ObjectLunarShopTerminals)
                    {
                        if (!interaction.Networkavailable)
                        {
                            UnityEngine.Object.Destroy(interaction.gameObject);
                        }
                    }
                }
            }
        }

        public void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (ModConfig.EnableMod.Value && IsCurrentMapInBazaar())
            {
                if (self.name.StartsWith("LunarRecycler"))
                {
                    float time = 1f;
                    foreach (PurchaseInteraction interaction in ObjectLunarShopTerminals_Spawn)
                    {
                        BazaarIsMyHome.instance.StartCoroutine(DelayEffect(self, interaction, time));
                        time = time + 0.1f;
                    }
                }
            }
            orig(self, activator);
        }

        private void PurchaseInteraction_ScaleCost(On.RoR2.PurchaseInteraction.orig_ScaleCost orig, PurchaseInteraction self, float scalar)
        {
            if (ModConfig.EnableMod.Value)
            {
                if (self.name.StartsWith("LunarRecycler"))
                {
                    if (ModConfig.EnableLunarRecyclerInjection.Value)
                    {
                        if (ModConfig.LunarRecyclerAvailable.Value)
                        {
                            scalar = (float)ModConfig.LunarRecyclerRerollScalar.Value;
                        }
                    }
                }
            }
            orig(self, scalar);
        }
        private void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            if (ModConfig.EnableMod.Value)
            {
                if (self.name.StartsWith("LunarRecycler"))
                {
                    if (ModConfig.EnableLunarRecyclerInjection.Value)
                    {
                        if (ModConfig.LunarRecyclerAvailable.Value)
                        {
                            //ChatHelper.Send($"RerolledCount = {ModConfig.RerolledCount}, Reroll = {ModConfig.LunarRecyclerRerollCount.Value}");
                            if (ModConfig.RerolledCount < ModConfig.LunarRecyclerRerollCount.Value)
                            {
                                orig(self, true);
                            }
                            else
                            {
                                orig(self, false);
                            }
                            ModConfig.RerolledCount++;
                            return;
                        }
                    }
                }

            }
            orig(self, newAvailable);
        }

        IEnumerator DelayEffect(PurchaseInteraction self, PurchaseInteraction interaction, float time)
        {
            yield return new WaitForSeconds(time);
            RerollLunarShopTerminal(self, interaction);
        }

        private void RerollLunarShopTerminal(PurchaseInteraction self, PurchaseInteraction interaction)
        {
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
            weightedSelection.AddChoice(Run.instance.availableLunarItemDropList, 50f);
            weightedSelection.AddChoice(Run.instance.availableLunarEquipmentDropList, 50f);
            List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
            PickupDef pickupDef = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]);

            ShopTerminalBehavior shopTerminal = interaction.gameObject.GetComponent<ShopTerminalBehavior>();
            shopTerminal.SetPickupIndex(pickupDef.pickupIndex);

            SpawnEffect(LunarRerollEffect, interaction.gameObject.transform.position + Vector3.up * 1f, new Color32(255, 255, 255, 255), 2f);
        }

        private void SetLunarShopTerminal()
        {
            /*
            List<int> total = new List<int> { 0, 1, 2, 3, 4, 5 };
            List<int> random = new List<int>();

            while (total.Count > 0)
            {
                int index = Random.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicLunarShopTerminals.Add(random[0], new SpawnCardStruct(new Vector3(-90.8748f, -22.3210f, -49.7166f), new Vector3(0.0f, 250.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[1], new SpawnCardStruct(new Vector3(-90.7317f, -22.1151f, -53.4639f), new Vector3(0.0f, 240.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[2], new SpawnCardStruct(new Vector3(-87.8854f, -22.1132f, -53.3190f), new Vector3(0.0f, 180.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[3], new SpawnCardStruct(new Vector3(-86.6861f, -22.9508f, -50.5742f), new Vector3(0.0f, 100.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[4], new SpawnCardStruct(new Vector3(-70.2474f, -24.1325f, -51.1947f), new Vector3(0.0f, 230.0f, 0.0f)));
            DicLunarShopTerminals.Add(random[5], new SpawnCardStruct(new Vector3(-76.9623f, -25.8940f, -41.4813f), new Vector3(0.0f, 120.0f, 0.0f)));*/

            Vector3 lunarTablePosition = new Vector3(-76.6438f, -24.0468f, -41.6449f);
            const float orientation = 280f;
            Vector3 lunarTableDroneShopPosition = new Vector3(-139.8156f, -21.8568f, 2.9263f);
            const float droneTableOrientation = 160f;

            const float tableRadiusInner = 3.0f;
            const float tableRadiusMiddle = 4.0f;
            const float tableRadiusOuter = 5.0f;
            const float tableStartAngleInner = 0f;
            const float tableStartAngleMiddle = -10f;
            const float tableStartAngleOuter = 8f;
            const float tableEndAngleInner = 215f;
            const float tableEndAngleMiddle = 230f;
            const float tableEndAngleOuter = 234f;
            const float minDistance = 19f;
            const float innerCapacity = 5;//(int)(2 * Math.PI * tableRadiusInner * (tableEndAngleInner - tableStartAngleInner) / 360f / minDistance);
            const float middleCapacity = 8;//(int)(2 * Math.PI * tableRadiusMiddle * (tableEndAngleMiddle - tableStartAngleMiddle) / 360f / minDistance);
            const float outerCapacity = 10;//(int)(2 * Math.PI * tableRadiusOuter * (tableEndAngleOuter - tableStartAngleOuter) / 360f / minDistance);

            List<Vector2> points = new List<Vector2>();
            if (ModConfig.LunarShopTerminalCount.Value <= middleCapacity)
            {
                points = Lloyd.GenerateCirclePoints(tableRadiusMiddle, tableStartAngleMiddle, tableEndAngleMiddle, orientation, ModConfig.LunarShopTerminalCount.Value);
            }
            else
            {
                points.AddRange(Lloyd.GenerateCirclePoints(tableRadiusInner, tableStartAngleInner, tableEndAngleInner, orientation, 100));
                points.AddRange(Lloyd.GenerateCirclePoints(tableRadiusOuter, tableStartAngleOuter, tableEndAngleOuter, orientation, 100));
                points = Lloyd.LloydsAlgorithm(points, ModConfig.LunarShopTerminalCount.Value);
            }
            float scale = 1.0f;
            switch (ModConfig.LunarShopTerminalCount.Value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    scale = 1f;
                    break;
                case 5:
                    scale = 0.95f;
                    break;
                case 6:
                    scale = 0.9f;
                    break;
                case 7:
                    scale = 0.85f;
                    break;
                case 8:
                    scale = 0.8f;
                    break;
                default:
                    scale = 0.75f;
                    break;
            }
            for (int i = 0; i < points.Count; i++)
            {
                DicLunarShopTerminals.Add(i, new SpawnCardStruct(new Vector3(lunarTablePosition.x + points[i].x, lunarTablePosition.y, lunarTablePosition.z + points[i].y), new Vector3(0.0f, 250.0f + i * 10f / points.Count, 0.0f), new Vector3(scale, scale, scale)));
            }
        }

        private void SpawnLunarShopTerminal()
        {
            ObjectLunarShopTerminals.Clear();
            ObjectLunarShopTerminals_Spawn.Clear();
            LunarShopTerminalTotalCount = 0;
            if (ModConfig.LunarShopTerminalCount.Value > 0)
            {

                //foreach (GameObject @object in UnityEngine.Object.FindObjectsOfType<GameObject>())
                //{
                //    if (@object.name.StartsWith("LunarShopTerminal"))
                //    {
                //        PickupDisplay[] pickupDisplays = @object.GetComponentsInChildren<PickupDisplay>();
                //        foreach (PickupDisplay pickup in pickupDisplays)
                //        {
                //            pickup.modelRenderer.enabled = false;
                //        }
                //    }
                //}


                // 月球蓓蕾
                DicLunarShopTerminals.Clear();
                SetLunarShopTerminal();
                DoSpawnGameObject(DicLunarShopTerminals, lunarShopTerminal, ModConfig.LunarShopTerminalCount.Value);
            }
        }
        protected override void DoSpawnGameObject(Dictionary<int, SpawnCardStruct> keyValuePairs, AsyncOperationHandle<GameObject> card, int max)
        {
            int count = 0;
            if (ModConfig.SpawnCountByStage.Value)
            {
                count = SetCountbyGameStage(max, ModConfig.SpawnCountOffset.Value);
                if (card.LocationName.EndsWith("LunarShopTerminal.prefab"))
                {
                    LunarShopTerminalTotalCount = count;
                    if (count <= 5)
                    {
                        return;
                    }
                    else
                    {
                        count = LunarShopTerminalTotalCount - 5;
                    }
                }
            }
            else
            {
                count = max;
                if (card.LocationName.EndsWith("LunarShopTerminal.prefab"))
                {
                    LunarShopTerminalTotalCount = max;
                    count = count - 5;
                }
            }
            for (int i = 0; i < count; i++)
            {
                GameObject gameObject = GameObject.Instantiate(card.WaitForCompletion(), keyValuePairs[i].Position, Quaternion.identity);
                gameObject.transform.eulerAngles = keyValuePairs[i].Rotation;
                gameObject.transform.localScale = (Vector3)keyValuePairs[i].Scale;
                if (card.LocationName.EndsWith("LunarShopTerminal.prefab"))
                {
                    ObjectLunarShopTerminals_Spawn.Add(gameObject.GetComponent<PurchaseInteraction>());
                    if (ModConfig.EnableLunarShopTerminalInjection.Value || ModConfig.PenaltyCoefficient_Temp != 1)
                    {
                        int cost = 2;
                        int total = cost * ModConfig.PenaltyCoefficient_Temp;
                        if (ModConfig.EnableLunarShopTerminalInjection.Value)
                        {
                            total = ModConfig.LunarShopTerminalCost.Value * ModConfig.PenaltyCoefficient_Temp;
                        }
                        //gameObject.name = "MyLunarBud";
                        gameObject.GetComponent<PurchaseInteraction>().cost = total;
                        gameObject.GetComponent<PurchaseInteraction>().Networkcost = total;
                    }
                }
                NetworkServer.Spawn(gameObject);
            }
        }
    }
}
