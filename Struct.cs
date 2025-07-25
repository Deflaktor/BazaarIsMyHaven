using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BazaarIsMyHome
{
    public class SpawnCardStruct
    {
        public SpawnCardStruct(Vector3 position, Vector3 rotation, Vector3? scale = null)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale ?? new Vector3(1, 1, 1);
        }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3? Scale { get; set; }
    }
    public class PlayerStruct
    {
        public PlayerStruct(NetworkUser networkUser, int donateCount, int rewardCount = 0)
        {
            NetworkUser = networkUser;
            DonateCount = donateCount;
            RewardCount = rewardCount;
        }

        public NetworkUser NetworkUser { get; set; }
        public int DonateCount { get; set; }
        public int RewardCount { get; set; }
    }
    public class SpecialItemStruct
    {
        public SpecialItemStruct(string name, int count, bool isUse = false)
        {
            Name = name;
            Count = count;
            IsUse = isUse;
        }

        public string Name { get; set; }
        public int Count { get; set; }
        public bool IsUse { get; set; }
    }
    public class CauldronHackedStruct
    {
        public CauldronHackedStruct(string name, int cost, CostTypeIndex costTypeIndex)
        {
            Name = name;
            Cost = cost;
            CostTypeIndex = costTypeIndex;
        }

        public string Name { get; set; }
        public int Cost { get; set; }
        public CostTypeIndex CostTypeIndex { get; set; }
    }
}
