using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BazaarIsMyHome
{

    public static class Tokens
    {
        internal static string LanguageRoot
        {
            get
            {
                return System.IO.Path.Combine(AssemblyDir, "Language");
            }
        }

        internal static string AssemblyDir
        {
            get
            {
                return System.IO.Path.GetDirectoryName(BazaarIsMyHome.PluginInfo.Location);
            }
        }
        public static void RegisterLanguageTokens()
        {
            On.RoR2.Language.SetFolders += Language_SetFolders;
        }

        private static void Language_SetFolders(On.RoR2.Language.orig_SetFolders orig, Language self, IEnumerable<string> newFolders)
        {
            if (Directory.Exists(LanguageRoot))
            {
                IEnumerable<string> second = Directory.EnumerateDirectories(System.IO.Path.Combine(new string[]
                {
                    LanguageRoot
                }), self.name);
                orig(self, newFolders.Union(second));
            }
            else
            {
                orig(self, newFolders);
            }
        }
    }

    class LanguageAPI
    {
        public static string NEWT_PRAY_FIRST_TIME = "NEWT_PRAY_FIRST_TIME";
        public static string NEWT_PRAY_NORMAL = "NEWT_PRAY_NORMAL";
        public static string NEWT_PRAY_ELITE = "NEWT_PRAY_ELITE";
        public static string NEWT_PRAY_PECULIAR = "NEWT_PRAY_PECULIAR";
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
