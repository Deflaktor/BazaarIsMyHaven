using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BazaarIsMyHaven
{
    class LanguageAPI
    {
        public static string NEWT_DONATE_LIST1 = "NEWT_DONATE_LIST1";
        public static string NEWT_DONATE_LIST2 = "NEWT_DONATE_LIST2";
        public static string NEWT_DONATE_LIST3 = "NEWT_DONATE_LIST3";
        public static string NEWT_DONATE_LIST_CHARACTER = "NEWT_DONATE_LIST_CHARACTER";
        public static string NEWT_DEATH_STATE = "NEWT_DEATH_STATE";
        public static string NEWT_DEATH_INFO = "NEWT_DEATH_INFO";
        public static string[] NEWT_WELCOME_WORD = new string[]
        {
            "NEWT_WELCOME_WORD_1",
            "NEWT_WELCOME_WORD_2",
            "NEWT_WELCOME_WORD_3",
            "NEWT_WELCOME_WORD_4",
            "NEWT_WELCOME_WORD_5",
        };
        public static string[] NEWT_ANGRY_WELCOME_WORD = new string[]
        {
            "NEWT_ANGRY_WELCOME_WORD_1",
            "NEWT_ANGRY_WELCOME_WORD_2",
            "NEWT_ANGRY_WELCOME_WORD_3",
            "NEWT_ANGRY_WELCOME_WORD_4",
            "NEWT_ANGRY_WELCOME_WORD_5",
        };
        // new lines in the style of the game's own; clients don't have them
        public static string[] NEWT_WELCOME = Tokens("NEWT_WELCOME_", 5);
        public static string[] NEWT_ANGRY_WELCOME = Tokens("NEWT_ANGRY_WELCOME_", 5);
        // the game's own lines, which every client can translate
        public static string[] NEWT_UPGRADE = Tokens("NEWT_UPGRADE_", 3);
        public static string[] NEWT_LUNAR_PURCHASE = Tokens("NEWT_LUNAR_PURCHASE_", 8);
        public static string[] NEWT_ANNOY = Tokens("NEWT_ANNOY_", 6);

        private static string[] Tokens(string prefix, int count)
        {
            return Enumerable.Range(1, count).Select(number => prefix + number).ToArray();
        }

        public static string[] NEWT_ATTACKED_WORD = new string[]
        {
            "NEWT_ATTACKED_WORD_1",
            "NEWT_ATTACKED_WORD_2",
            "NEWT_ATTACKED_WORD_3",
            "NEWT_ATTACKED_WORD_4",
            "NEWT_ATTACKED_WORD_5",
            "NEWT_ATTACKED_WORD_6",
            "NEWT_ATTACKED_WORD_7",
            "NEWT_ATTACKED_WORD_8",
            "NEWT_ATTACKED_WORD_9",
            "NEWT_ATTACKED_WORD_10",
        };
    }
}
