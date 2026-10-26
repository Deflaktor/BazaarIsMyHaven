using RoR2;
using System;

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
        /// Adds a kind of printer to the Bazaar's printers. When the director buys a printer, it rolls the kind among
        /// the ones it can afford, by weight, so yours competes with the printer tiers' weights (Tier1Weight and so on)
        /// and spawns in one of the printer spots.
        /// </summary>
        /// <param name="name">The name in the log</param>
        /// <param name="spawnCard">The printer's spawn card; its prefab is spawned</param>
        /// <param name="directorCost">The credits one costs, e.g. 5 like a white printer by default</param>
        /// <param name="weight">How likely the director rolls it, compared to the printer tiers' weights</param>
        /// <param name="isAvailable">Whether the director can roll it on this visit</param>
        public static void AddPrinter(string name, Func<SpawnCard> spawnCard, Func<int> directorCost, Func<float> weight, Func<bool> isAvailable = null)
        {
            BazaarPrinter.AddKind(name, spawnCard, directorCost, weight, isAvailable);
        }

        /// <summary>
        /// Adds a kind of cauldron to the Bazaar's cauldrons, as AddPrinter does for printers: yours competes with the
        /// cauldron types' weights (WhiteToGreenWeight and so on) and spawns in one of the cauldron spots.
        /// </summary>
        /// <param name="name">The name in the log</param>
        /// <param name="spawnCard">The cauldron's spawn card; its prefab is spawned</param>
        /// <param name="directorCost">The credits one costs, e.g. 10 like the Bazaar's own cauldrons by default</param>
        /// <param name="weight">How likely the director rolls it, compared to the cauldron types' weights</param>
        /// <param name="isAvailable">Whether the director can roll it on this visit</param>
        public static void AddCauldron(string name, Func<SpawnCard> spawnCard, Func<int> directorCost, Func<float> weight, Func<bool> isAvailable = null)
        {
            BazaarCauldron.AddKind(name, spawnCard, directorCost, weight, isAvailable);
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
