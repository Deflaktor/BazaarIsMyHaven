using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHome
{
    internal class Common
    {
        public static System.Random RNG = new System.Random();
        public static readonly DirectorPlacementRule DirectPlacement = new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.Direct
        };


        public static void SpawnEffect(AsyncOperationHandle<GameObject> effect, Vector3 position, Color32 color, float scale = 1f)
        {
            EffectManager.SpawnEffect(effect.WaitForCompletion(), new EffectData()
            {
                origin = position,
                rotation = Quaternion.identity,
                scale = scale,
                color = color
            }, true);
        }
    }
}
