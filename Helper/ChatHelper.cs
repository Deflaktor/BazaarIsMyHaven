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
                Send($"{coloredPlayerName} <color=#{GrayColor}>can use a shop terminal</color> <color=#{RedColor}>{usesLeft}</color> <color=#{GrayColor}>more time.</color>");
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

        public static string GetItemNames(Dictionary<PickupIndex, int> pickupIndexes)
        {
            var itemNames = new List<string>();
            foreach (var (pickupIndex, count) in pickupIndexes)
            {
                if (count <= 0)
                    continue;
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                var itemName = pickupDef.internalName;
                ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.Tier1Item;
                if (pickupDef.itemIndex != ItemIndex.None)
                {
                    var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                    if (string.IsNullOrWhiteSpace(itemDef.nameToken))
                        itemName = itemDef.name;
                    else
                        itemName = Language.GetString(itemDef.nameToken);
                    if (string.IsNullOrWhiteSpace(itemName))
                        itemName = itemDef.name;
                    if (itemDef.tier != ItemTier.NoTier)
                        colorIndex = ItemTierCatalog.GetItemTierDef(itemDef.tier).colorIndex;
                }
                else if (pickupDef.equipmentIndex != EquipmentIndex.None)
                {
                    var equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                    if (string.IsNullOrWhiteSpace(equipmentDef.nameToken))
                        itemName = equipmentDef.name;
                    else
                        itemName = Language.GetString(equipmentDef.nameToken);
                    if (string.IsNullOrWhiteSpace(itemName))
                        itemName = equipmentDef.name;
                    colorIndex = equipmentDef.colorIndex;
                }
                var colorHexString = ColorCatalog.GetColorHexString(colorIndex);
                if (count > 1)
                {
                    itemNames.Add($"<color=#{colorHexString}>{itemName}</color> x {count}");
                }
                else
                {
                    itemNames.Add($"<color=#{colorHexString}>{itemName}</color>");
                }
            }

            return string.Join(", ", itemNames);
        }

        public static void ThanksTipNormal(NetworkUser networkUser, PlayerCharacterMasterController pc, Dictionary<PickupIndex, int> pickupIndexes)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_DONATE_LIST1, NewtName, GetDeathState(), GetColoredPlayerName(pc), GetItemNames(pickupIndexes)));
        }

        public static void ThanksTipElite(NetworkUser networkUser, PlayerCharacterMasterController pc, Dictionary<PickupIndex, int> pickupIndexes)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_DONATE_LIST2, NewtName, GetDeathState(), GetColoredPlayerName(pc), GetItemNames(pickupIndexes)));
        }

        public static void ThanksTipPeculiar(NetworkUser networkUser, PlayerCharacterMasterController pc, Dictionary<PickupIndex, int> pickupIndexes)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_DONATE_LIST3, NewtName, GetDeathState(), GetColoredPlayerName(pc), GetItemNames(pickupIndexes)));
        }

        public static void ThanksTipCharacter(NetworkUser networkUser, PlayerCharacterMasterController pc, Dictionary<PickupIndex, int> pickupIndexes)
        {
            Send(Language.GetStringFormatted(LanguageAPI.NEWT_DONATE_LIST_CHARACTER, NewtName, GetDeathState(), GetColoredPlayerName(pc), GetItemNames(pickupIndexes)));
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
