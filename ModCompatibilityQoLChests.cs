using BazaarIsMyHaven;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BazaarIsMyHaven
{
    public static class ModCompatibilityQoLChests
    {
        private static bool? _enabled;
        private static BaseUnityPlugin _plugin;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = Chainloader.PluginInfos.ContainsKey("Faust.QoLChests");
                }
                return (bool)_enabled;
            }
        }

        public static BaseUnityPlugin plugin
        {
            get
            {
                if (_plugin == null && Chainloader.PluginInfos.TryGetValue("Faust.QoLChests", out var info))
                {
                    _plugin = info.Instance;
                }

                return _plugin;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RegisterQoLChestsBlacklist(string LunarShopObjectName)
        {
            if (plugin == null)
                return;

            try
            {
                // Find the API in the plugin without referencing its DLL.
                var registryType = plugin.GetType().Assembly.GetType("Faust.QoLChests.Handlers.InteractableRegistry");

                var blacklistMethod = registryType?.GetMethod("BlackList", new[] { typeof(string) });

                if (blacklistMethod == null)
                {
                    Log.LogWarning("Could not find QoLChests' blacklist API; buds may get hidden.");
                    return;
                }

                // BlackList is static, so its target is null.
                blacklistMethod.Invoke(null, new object[] { LunarShopObjectName });
            }
            catch (Exception exception)
            {
                Log.LogWarning($"Could not exempt buds from QoLChests: {exception}");
            }
        }
    }
}
