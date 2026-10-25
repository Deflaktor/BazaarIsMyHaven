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
    }
}
