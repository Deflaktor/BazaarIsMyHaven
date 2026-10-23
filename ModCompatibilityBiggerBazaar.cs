using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace BazaarIsMyHaven
{
    public static class ModCompatibilityBiggerBazaar
    {
        private static BaseUnityPlugin _plugin;

        public static BaseUnityPlugin plugin
        {
            get
            {
                if (_plugin == null && Chainloader.PluginInfos.TryGetValue("com.MagnusMagnuson.BiggerBazaar", out var info))
                {
                    _plugin = info.Instance;
                }

                return _plugin;
            }
        }

        public static bool enabled => Chainloader.PluginInfos.ContainsKey("com.MagnusMagnuson.BiggerBazaar");

        // BiggerBazaar takes the players' gold at the portal to the Bazaar and gives it back there, unless its disableTransferMoney is on
        public static bool TransfersGold()
        {
            if (plugin == null)
            {
                return false;
            }
            return !plugin.Config.TryGetEntry("9. Other", "disableTransferMoney", out ConfigEntry<bool> disableTransferMoney) || !disableTransferMoney.Value;
        }
    }
}
