namespace BazaarIsMyHaven
{
    /// <summary>
    /// What other mods can add to the Bazaar. Call it from your plugin's Awake, with
    /// [BepInDependency(BazaarIsMyHaven.Main.PluginGUID)], so that BazaarIsMyHaven is loaded first.
    /// </summary>
    public static class BazaarAPI
    {
        /// <summary>
        /// Adds a section to the Bazaar: its director cards join those of the Bazaar's own sections, for the same
        /// credits, and it spawns what the director bought, as the Bazaar's own sections do. See BazaarBase.
        /// </summary>
        public static void AddSection(BazaarBase section)
        {
            Main.instance.AddSection(section);
        }

        /// <summary>
        /// Gives a survivor its own Donation Altar reward, e.g. for a survivor your mod adds. It becomes the default of the
        /// survivor's RewardList&lt;name&gt; setting, which players can still change. The first time, e.g. when their config
        /// is older than your mod, the survivor is added to their RewardListAvailableCharacters; if they take it out
        /// again, it stays out.
        /// </summary>
        /// <param name="survivorOrBodyName">The survivor's name, e.g. "Commando", or its body's, e.g. "CommandoBody"</param>
        /// <param name="rewardList">The reward in ItemStringParser's format, e.g. "3xFeather" or "Tier3 | 2xTier2"</param>
        public static void SetSurvivorDonationReward(string survivorOrBodyName, string rewardList)
        {
            ModConfig.SetModDonationReward(survivorOrBodyName, rewardList);
        }
    }
}
