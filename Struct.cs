using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BazaarIsMyHaven
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
        public PlayerStruct(PlayerCharacterMasterController master)
        {
            Master = master;
            DonateCount = 0;
            RewardCount = 0;
            LunarShopUseCount = 0;
        }

        public PlayerCharacterMasterController Master { get; set; }
        public int DonateCount { get; set; }
        public int RewardCount { get; set; }
        public int LunarShopUseCount { get; set; }
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
    public class ShopKeep
    {
        public static bool DiedAtLeastOnce { get; set; }
        public static int DeathCount { get; set; }
        public static CharacterBody Body { get; set; }

        public enum DeathState
        {
            Default, Tank, Ghost, Evil
        }
    }
}
