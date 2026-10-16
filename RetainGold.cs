using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BazaarIsMyHaven
{
    // Players keep their gold when they take the portal to the Bazaar, instead of the portal turning it into experience,
    // so they can spend it there. Gold left when they leave the Bazaar turns into experience as usual.
    public static class RetainGold
    {
        public static void Hook()
        {
            On.RoR2.SceneExitController.SetState += SceneExitController_SetState;
        }

        private static void SceneExitController_SetState(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            bool extractsExperience = newState == SceneExitController.ExitState.ExtractExp && self.exitState != newState;
            orig(self, newState);
            if (!extractsExperience || !self.experienceCollector || !IsRetainingGold(self))
            {
                return;
            }
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
    }
}
