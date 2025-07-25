using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BazaarIsMyHome
{
    public static class ModCompatibilityInLobbyConfig
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.InLobbyConfig");
                }
                return (bool)_enabled;
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void CreateFromBepInExConfigFile(ConfigFile config, string displayName)
        {
            var configEntry = InLobbyConfig.Fields.ConfigFieldUtilities.CreateFromBepInExConfigFile(config, displayName);
            InLobbyConfig.ModConfigCatalog.Add(configEntry);
        }
    }
}
