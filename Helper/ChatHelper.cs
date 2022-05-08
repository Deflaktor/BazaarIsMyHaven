using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

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
        private static string GetDeathState() => ShopKeep.IsDeath ? $"({Language.GetString(LanguageAPI.NEWT_DEATH_STATE)})" : "";

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
                if (!ShopKeep.IsDeath) Send(Language.GetStringFormatted(LanguageAPI.NEWT_WELCOME_WORD[r], NewtName));
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
                if (!ShopKeep.IsDeath) Send(Language.GetStringFormatted(LanguageAPI.NEWT_ATTACKED_WORD[r], NewtName));
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
    class ShopKeep
    {
        public static bool IsDeath { get; set; }
        public static int SpawnTime_Record { get; set; }
        public static CharacterBody Body { get; set; }

        public enum DeathState
        {
            Ghost, Tank, Evil
        }
    }
}
