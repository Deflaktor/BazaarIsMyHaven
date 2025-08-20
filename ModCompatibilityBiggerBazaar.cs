using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static ak.wwise.core;

namespace BazaarIsMyHaven
{
    public static class ModCompatibilityBiggerBazaar
    {
        private static bool? _enabled;
        private static BaseUnityPlugin _plugin;
        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.MagnusMagnuson.BiggerBazaar");
                }
                return (bool)_enabled;
            }
        }
        public static BaseUnityPlugin plugin
        {
            get
            {
                if (_plugin == null)
                {
                    foreach (var plugin in Chainloader.PluginInfos)
                    {
                        if (plugin.Key == "com.MagnusMagnuson.BiggerBazaar")
                        {
                            _plugin = plugin.Value.Instance;
                        }
                    }
                }
                return _plugin;
            }
        }


    }
}
