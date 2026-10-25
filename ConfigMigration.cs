using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace BazaarIsMyHaven
{
    // The version of the config's layout, and the steps that bring an older config up to it. Each step runs once, for the
    // configs written before its version: a setting still at its old default moves to its new default, and one the
    // player changed is converted, e.g. into a new unit.
    public static class ConfigMigration
    {
        public const int CurrentVersion = 1;
        // the mod's own bookkeeping, which players don't set
        public const string MetaSection = "99 Meta";
        public const string VersionKey = "ConfigVersion";
        public static ConfigEntry<int> ConfigVersion;

        // the step to each version, from the one before
        private static readonly SortedDictionary<int, Action> steps = new SortedDictionary<int, Action>();
        // The values in the config file as they were written, by section and key. A setting whose type changed can't
        // read its old value, which the conversion needs.
        private static readonly Dictionary<(string section, string key), string> oldValues = new Dictionary<(string section, string key), string>();

        // Before anything else is bound, so that a config file from before the version was introduced, which has none,
        // can be told apart from a new one, which starts at the current version
        public static void BindVersion(ConfigFile config)
        {
            bool existingFile = File.Exists(config.ConfigFilePath);
            ReadOldValues(config);
            ConfigVersion = config.Bind(MetaSection, VersionKey, existingFile ? 0 : CurrentVersion, "The version of this config's layout, which the mod uses to update older configs to new settings and defaults. Don't change it.");
        }

        public static void Migrate()
        {
            if (ConfigVersion.Value >= CurrentVersion)
            {
                return;
            }
            foreach (var (version, step) in steps)
            {
                if (version > ConfigVersion.Value)
                {
                    step();
                    Log.LogInfo($"Updated the config to version {version}.");
                }
            }
            ConfigVersion.Value = CurrentVersion;
        }

        // before the file is read into the settings
        public static void ReadOldValues(ConfigFile config)
        {
            oldValues.Clear();
            if (!File.Exists(config.ConfigFilePath))
            {
                return;
            }
            string section = "";
            foreach (var rawLine in File.ReadAllLines(config.ConfigFilePath))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2);
                    continue;
                }
                int separator = line.IndexOf('=');
                if (separator > 0)
                {
                    oldValues[(section, line.Substring(0, separator).Trim())] = line.Substring(separator + 1).Trim();
                }
            }
        }

        // For a setting whose type changed: the new default if its old value was the old default, otherwise the old value
        // converted. Without an old value, it stays. Only the config file is updated, not a preset.
        private static void MigrateChangedType<T>(ConfigEntry<T> entry, string oldDefault, Func<string, T> convert)
        {
            entry = ModCompatibilityConfigPresets.PlayerEntry(entry);
            if (oldValues.TryGetValue((entry.Definition.Section, entry.Definition.Key), out var oldValue))
            {
                entry.Value = string.Equals(oldValue, oldDefault, StringComparison.OrdinalIgnoreCase) ? (T)entry.DefaultValue : convert(oldValue);
            }
        }

        // the new default if the setting still has the old one, otherwise its value converted; in the config file, not a preset
        private static void Migrate<T>(ConfigEntry<T> entry, T oldDefault, Func<T, T> convert)
        {
            entry = ModCompatibilityConfigPresets.PlayerEntry(entry);
            entry.Value = EqualityComparer<T>.Default.Equals(entry.Value, oldDefault) ? (T)entry.DefaultValue : convert(entry.Value);
        }
    }
}
