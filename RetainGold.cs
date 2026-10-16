using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BazaarIsMyHaven
{
    // Players keep their gold when they take the portal to the Bazaar, instead of the portal turning it into experience,
    // so they can spend it there. Gold left when they leave the Bazaar turns into experience as usual.
    public static class RetainGold
    {
        // ShareSuite resets its shared gold when a stage starts
        private static int? sharedGoldToRestore;

        public static void Hook()
        {
            On.RoR2.SceneExitController.SetState += SceneExitController_SetState;
            SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private static void SceneExitController_SetState(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            bool extractsExperience = newState == SceneExitController.ExitState.ExtractExp && self.exitState != newState;
            orig(self, newState);
            if (!extractsExperience || !self.experienceCollector || !IsRetainingGold(self))
            {
                return;
            }
            int? sharedGold = null;
            if (ModCompatibilityShareSuite.enabled && !ModCompatibilityShareSuite.TryGetSharedGold(out sharedGold))
            {
                // without knowing ShareSuite's gold, the gold turns into experience as usual instead of getting lost
                return;
            }
            sharedGoldToRestore = sharedGold;
            // the exit moves on to teleporting once the converter is gone
            self.experienceCollector.enabled = false;
            Object.Destroy(self.experienceCollector);
        }

        private static bool IsRetainingGold(SceneExitController exit)
        {
            return NetworkServer.active && ModConfig.EnableMod.Value && ModConfig.RetainGold.Value
                && exit.destinationScene && exit.destinationScene.cachedName == "bazaar"
                // BiggerBazaar brings the gold over itself
                && !ModCompatibilityBiggerBazaar.TransfersGold();
        }

        private static void SceneDirector_onPrePopulateSceneServer(SceneDirector sceneDirector)
        {
            // ShareSuite has just reset its shared gold for the new stage
            if (sharedGoldToRestore.HasValue && SceneManager.GetActiveScene().name == "bazaar")
            {
                ModCompatibilityShareSuite.SetSharedGold(sharedGoldToRestore.Value);
            }
            sharedGoldToRestore = null;
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            sharedGoldToRestore = null;
        }
    }
}
