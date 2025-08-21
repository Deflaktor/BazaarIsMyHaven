using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BazaarIsMyHaven
{
    class ChatHelper
    {
        private static System.Random Random = new System.Random();

        private static string NewtName
        {
            get
            {
                return Language.GetString("SHOPKEEPER_BODY_NAME");
            }
        }

        private const string GrayColor = "7e91af";
        private const string RedColor = "ed4c40";
        private const string LunarColor = "5cb1ed";
        
        private static string GetPlayerColor(PlayerCharacterMasterController pc)
        {
            var userName = pc.GetDisplayName();
            var survivorDef = SurvivorCatalog.FindSurvivorDefFromBody(pc.master?.bodyPrefab);
            if (survivorDef != null && survivorDef.primaryColor != null) {
                return ColorUtility.ToHtmlStringRGB(survivorDef.primaryColor);
            }
            var body = pc.master?.GetBody();
            if (body != null && body.bodyColor != null)
            {
                return ColorUtility.ToHtmlStringRGB(body.bodyColor);
            }
            return "f27b0c";
        }

        private static string GetColoredPlayerName(PlayerCharacterMasterController playerCharacterMasterController)
        {
            var playerColor = GetPlayerColor(playerCharacterMasterController);
            var body = playerCharacterMasterController.master.GetBody();
            return $"<color=#{playerColor}>{body.GetUserName()}</color>";
        }

        public static void LunarShopTerminalUsesLeft(PlayerCharacterMasterController playerCharacterMasterController, int usesLeft)
        {
            var coloredPlayerName = GetColoredPlayerName(playerCharacterMasterController);
            if(usesLeft > 1)
            {
                Send($"{coloredPlayerName} <color=#{GrayColor}>can use a shop terminal</color> <color=#{RedColor}>{usesLeft}</color> <color=#{GrayColor}>more times.</color>");
            }
            else if (usesLeft == 1)
            {
                Send($"{coloredPlayerName}  <color=#{GrayColor}>can use a shop terminal</color> <color=#{RedColor}>{usesLeft}</color> <color=#{GrayColor}>more time.</color>");
            }
            else
            {
                Send($"{coloredPlayerName} <color=#{GrayColor}>can no longer use shop terminals.</color>");
            }
        }

        public static void LunarRecyclerUsesLeft(int usesLeft)
        {
            if (usesLeft > 1)
            {
                Send($"The <color=#{LunarColor}>Lunar Recycler</color> <color=#{GrayColor}>can be used</color> <color=#{RedColor}>{usesLeft}</color> <color=#{GrayColor}>more times.</color>");
            }
            else if (usesLeft == 1)
            {
                Send($"The <color=#{LunarColor}>Lunar Recycler</color> <color=#{GrayColor}>can be used</color> <color=#{RedColor}>{usesLeft}</color> <color=#{GrayColor}>more time.</color>");
            }
            else
            {
                Send($"The <color=#{LunarColor}>Lunar Recycler</color> <color=#{GrayColor}>can no longer be used.</color>");
            }
        }

        private static string GetDeathState() => ShopKeeper.DiedAtLeastOnce && ModConfig.NewtDeathBehavior.Value == ShopKeeper.DeathState.Ghost ? $" ({Language.GetString(LanguageAPI.NEWT_DEATH_STATE)})" : "";

        public static string GetItemNames(PickupIndex[] pickupIndexes)
        {
            var counts = pickupIndexes.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var itemNames = new List<string>();
            foreach (var entry in counts)
            {
                var pickupIndex = entry.Key;
                var count = entry.Value;
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                var itemName = pickupDef.internalName;
                if (pickupDef.itemIndex != ItemIndex.None)
                {
                    itemName = Language.GetString(ItemCatalog.GetItemDef(pickupDef.itemIndex).nameToken);
                }
                else if (pickupDef.equipmentIndex != EquipmentIndex.None)
                {
                    itemName = Language.GetString(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex).nameToken);
                }
                if (count > 1)
                {
                    itemNames.Add(string.Format("{0} x {1}", itemName, count));
                }
                else
                {
                    itemNames.Add(itemName);
                }
            }

            return string.Join(", ", itemNames);
        }

        public static void ThanksTip(NetworkUser networkUser, PlayerCharacterMasterController pc)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_FIRST_TIME, NewtName, GetDeathState(), GetColoredPlayerName(pc)));
        }

        public static void ThanksTipNormal(NetworkUser networkUser, PlayerCharacterMasterController pc, PickupIndex[] pickupIndexes)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_NORMAL, NewtName, GetDeathState(), GetColoredPlayerName(pc), GetItemNames(pickupIndexes)));
        }

        public static void ThanksTipElite(NetworkUser networkUser, PlayerCharacterMasterController pc, PickupIndex[] pickupIndexes)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_ELITE, NewtName, GetDeathState(), GetColoredPlayerName(pc), GetItemNames(pickupIndexes)));
        }

        public static void ThanksTipPeculiar(NetworkUser networkUser, PlayerCharacterMasterController pc, PickupIndex[] pickupIndexes)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_PECULIAR, NewtName, GetDeathState(), GetColoredPlayerName(pc), GetItemNames(pickupIndexes)));
        }

        public static void WelcomeWord()
        {
            int length = LanguageAPI.NEWT_WELCOME_WORD.Length;
            int r = Random.Next(length);
            if (r < length)
            {
                if (!ShopKeeper.DiedAtLeastOnce) Send(Language.GetStringFormatted(LanguageAPI.NEWT_WELCOME_WORD[r], NewtName));
                else Send(Language.GetStringFormatted(LanguageAPI.NEWT_ANGRY_WELCOME_WORD[r], NewtName, GetDeathState()));
            }
        }
        public static void ShowNewtDeath()
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_DEATH_INFO, NewtName, GetDeathState()));
        }

        public static void HitWord()
        {
            int length = LanguageAPI.NEWT_ATTACKED_WORD.Length;
            int r = Random.Next(length);
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_ATTACKED_WORD[r], NewtName, GetDeathState()));
        }
        public static void Send(string message)
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = message
            });
        }
       
    }
}
