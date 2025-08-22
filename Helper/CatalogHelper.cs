using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace BazaarIsMyHaven
{
    public class CatalogHelper
    {
        public static BodyIndex FindBody(string survivorOrBodyName)
        {
            // stolen from DropInMultiplayer, thanks niwith :)
            return BodyCatalog.FindBodyIndexCaseInsensitive(survivorOrBodyName.EndsWith("Body", StringComparison.InvariantCultureIgnoreCase) ? survivorOrBodyName : survivorOrBodyName + "Body");
        }

    }
}
