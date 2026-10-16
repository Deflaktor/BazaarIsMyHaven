using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BazaarIsMyHaven
{
    // ProperSave restarts a run when it loads a save, so state that lasts the whole run goes into its save files
    public static class ModCompatibilityProperSave
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");
                }
                return (bool)_enabled;
            }
        }

        // ProperSave saves at the start of each stage; load gets the saved value once a save has loaded
        public static void Register(string key, Func<object> save, Action<object> load)
        {
            if (!enabled)
            {
                return;
            }
            try
            {
                RegisterWithProperSave(key, save, load);
            }
            catch (Exception exception)
            {
                Log.LogWarning($"Could not register {key} with ProperSave - {exception.Message}");
            }
        }

        // Kept apart, so a changed ProperSave fails inside the try block above
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RegisterWithProperSave(string key, Func<object> save, Action<object> load)
        {
            ProperSave.SaveFile.OnGatherSaveData += data => data[key] = save();
            ProperSave.Loading.OnLoadingEnded += saveFile =>
            {
                if (saveFile == null)
                {
                    return;
                }
                object value;
                try
                {
                    value = saveFile.GetModdedData<object>(key);
                }
                catch (KeyNotFoundException)
                {
                    // saved without this data, e.g. by an older version of this mod
                    return;
                }
                load(value);
            };
        }
    }
}
