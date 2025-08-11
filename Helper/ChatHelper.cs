using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BazaarIsMyHome
{
    class ChatHelper
    {
        private static System.Random Random = new System.Random();

        private static int RandomCapacity = 500;

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

        public static void ItemAlreadyBought()
        {
            Send(Language.GetString("LUNAR_POD_ALREADY_PURCHASED"));
        }

        public static void LunarShopTerminalUsesLeft(PlayerCharacterMasterController playerCharacterMasterController, int usesLeft)
        {
            var playerColor = GetPlayerColor(playerCharacterMasterController);
            var body = playerCharacterMasterController.master.GetBody();
            if(usesLeft > 1)
            {
                Send($"<color=#{playerColor}>{body.GetUserName()}</color> <color=#{GrayColor}>can use a shop terminal</color> <color=#{RedColor}>{usesLeft}</color> <color=#{GrayColor}>more times.</color>");
            }
            else if (usesLeft == 1)
            {
                Send($"<color=#{playerColor}>{body.GetUserName()}</color> <color=#{GrayColor}>can use a shop terminal</color> <color=#{RedColor}>{usesLeft}</color> <color=#{GrayColor}>more time.</color>");
            }
            else
            {
                Send($"<color=#{playerColor}>{body.GetUserName()}</color> <color=#{GrayColor}>can no longer use shop terminals.</color>");
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

        private static string GetDeathState() => ShopKeep.DiedAtLeastOnce ? $"({Language.GetString(LanguageAPI.NEWT_DEATH_STATE)})" : "";

        // 不知道是否有更好的写法
        public static void ThanksTip(NetworkUser networkUser, CharacterBody characterBody)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_FIRST_TIME, NewtName, GetDeathState(), string.Format("[{0}]{1}", characterBody.GetDisplayName(), networkUser.userName)));
        }
        public static void ThanksTip(NetworkUser networkUser, CharacterBody characterBody, EquipmentDef equipmentDef)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_NORMAL, NewtName, GetDeathState(), string.Format("[{0}]{1}", characterBody.GetDisplayName(), networkUser.userName), Language.GetString(equipmentDef.nameToken)));
        }

        public static void ThanksTip(NetworkUser networkUser, CharacterBody characterBody, PickupDef pickupDef)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_ELITE, NewtName, GetDeathState(), string.Format("[{0}]{1}", characterBody.GetDisplayName(), networkUser.userName), Language.GetString(pickupDef.nameToken)));
        }

        public static void ThanksTip(NetworkUser networkUser, CharacterBody characterBody, ItemDef itemDef, int count)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_PRAY_PECULIAR, NewtName, GetDeathState(), string.Format("[{0}]{1}", characterBody.GetDisplayName(), networkUser.userName), string.Format("{0} x {1}", Language.GetString(itemDef.nameToken), count)));
        }

        public static void WelcomeWord()
        {
            int length = LanguageAPI.NEWT_WELCOME_WORD.Length;
            int r = Random.Next(length);
            if (r < length)
            {
                if (!ShopKeep.DiedAtLeastOnce) Send(Language.GetStringFormatted(LanguageAPI.NEWT_WELCOME_WORD[r], NewtName));
                else Send(Language.GetStringFormatted(LanguageAPI.NEWT_ANGRY_WELCOME_WORD[r], NewtName, GetDeathState()));
            }
        }
        public static void ShowNewtDeath()
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_DEATH_INFO, NewtName));
        }

        public static void HitWord()
        {
            int length = LanguageAPI.NEWT_ATTACKED_WORD.Length;
            int r = Random.Next(length * RandomCapacity);
            if (r < length)
            {
                if (!ShopKeep.DiedAtLeastOnce) Send(Language.GetStringFormatted(LanguageAPI.NEWT_ATTACKED_WORD[r], NewtName));
            }
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
