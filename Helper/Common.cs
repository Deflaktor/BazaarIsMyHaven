using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace BazaarIsMyHome
{
    internal class Common
    {
        public static System.Random random = new System.Random();
        public static readonly DirectorPlacementRule directPlacement = new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.Direct
        };

    }
}
