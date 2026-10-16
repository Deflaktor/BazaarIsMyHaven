using BazaarIsMyHaven;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using ShareSuite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BazaarIsMyHaven
{
    public static class ModCompatibilityShareSuite
    {
        private static bool? _enabled;
        private static BaseUnityPlugin _plugin;
        private static ConfigEntry<bool> printerCauldronFix;
        private static bool fieldChecked;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.funkfrog_sipondo.sharesuite");
                }
                return (bool)_enabled;
            }
        }

        public static BaseUnityPlugin plugin
        {
            get
            {
                if(_plugin == null)
                {
                    foreach (var plugin in Chainloader.PluginInfos)
                    {
                        if (plugin.Key == "com.funkfrog_sipondo.sharesuite")
                        {
                            _plugin = plugin.Value.Instance;
                        }
                    }
                }
                return _plugin;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool IsShareSuite_PrinterCauldronFixEnabled()
        {
            // Read the setting once so a renamed field cannot break cauldron buys
            if (!fieldChecked)
            {
                fieldChecked = true;
                try
                {
                    if (plugin != null)
                        printerCauldronFix = plugin.GetFieldValue<ConfigEntry<bool>>("PrinterCauldronFixEnabled");
                }
                catch (Exception exception)
                {
                    Log.LogWarning($"Could not read ShareSuite's cauldron setting - {exception.Message}");
                }
            }

            return printerCauldronFix != null && printerCauldronFix.Value;
        }

        // In multiplayer, ShareSuite can keep all players' gold at one shared value.
        // sharedGold is null if it doesn't; false if ShareSuite's state can't be read.
        public static bool TryGetSharedGold(out int? sharedGold)
        {
            try
            {
                sharedGold = ReadSharedGold();
                return true;
            }
            catch (Exception exception)
            {
                Log.LogWarning($"Could not read ShareSuite's shared gold - {exception.Message}");
                sharedGold = null;
                return false;
            }
        }

        public static void SetSharedGold(int sharedGold)
        {
            try
            {
                WriteSharedGold(sharedGold);
            }
            catch (Exception exception)
            {
                Log.LogWarning($"Could not set ShareSuite's shared gold - {exception.Message}");
            }
        }

        // Kept apart, so a changed ShareSuite fails inside the try blocks above
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static int? ReadSharedGold()
        {
            if (!ShareSuite.ShareSuite.ModIsEnabled.Value || !ShareSuite.ShareSuite.MoneyIsShared.Value || !GeneralHooks.IsMultiplayer())
            {
                return null;
            }
            return MoneySharingHooks.SharedMoneyValue;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void WriteSharedGold(int sharedGold)
        {
            MoneySharingHooks.SharedMoneyValue = sharedGold;
        }
    }
}
