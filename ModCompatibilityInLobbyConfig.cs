using BepInEx.Configuration;
using InLobbyConfig.Fields;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BazaarIsMyHaven
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
            var configuration = InLobbyConfig.Fields.ConfigFieldUtilities.CreateFromBepInExConfigFile(config, displayName);
           
            foreach (var section in configuration.SectionFields)
            {
                var entriesToRemove = new List<IConfigField>();

                foreach (var configEntry in section.Value)
                {
                    if (configEntry.DisplayName.Equals("SectionEnabled", StringComparison.InvariantCultureIgnoreCase))
                    {
                        entriesToRemove.Add(configEntry);
                    }
                }

                // Remove the entries after the iteration
                foreach (var entry in entriesToRemove)
                {
                    (section.Value as List<IConfigField>).Remove(entry);
                }
            }
            InLobbyConfig.ModConfigCatalog.Add(configuration);
        }
    }
}
