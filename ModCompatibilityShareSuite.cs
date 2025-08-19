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
            return plugin.GetFieldValue<ConfigEntry<bool>>("PrinterCauldronFixEnabled").Value;
        }

    }
}
