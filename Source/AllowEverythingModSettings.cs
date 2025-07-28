using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace AllowEverything
{
    public class AllowEverythingModSettings : ModSettings
    {
        public HashSet<string> allowByCategory, notifyByCategory;

        public override void ExposeData()
        {
            Validate();

            Scribe_Collections.Look(ref allowByCategory, nameof(allowByCategory), LookMode.Value);
            Scribe_Collections.Look(ref notifyByCategory, nameof(notifyByCategory), LookMode.Value);

            base.ExposeData();
        }

        public void Validate()
        {
            if (allowByCategory == null)
            {
                allowByCategory = new HashSet<string>();
                foreach (var cat in ThingCategoryDefOf.Root.childCategories)
                    AllowEverythingMod.SetCategoryAndDescendants(cat, true, allowByCategory);
            }

            if (notifyByCategory == null)
                notifyByCategory = new HashSet<string>();
        }

    }
}
