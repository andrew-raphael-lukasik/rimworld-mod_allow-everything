using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace AllowEverything
{
    public class AllowEverythingMod : Mod
    {
        Vector2 _scrollPositionAllow, _scrollPositionNotify;
        AllowEverythingModSettings _settings;

        public AllowEverythingMod(ModContentPack content)
            : base(content)
        {
            _settings = GetSettings<AllowEverythingModSettings>();
        }

        public override string SettingsCategory() => "Allow Everything";

        public override void DoSettingsWindowContents(Rect rect)
        {
            _settings.Validate();

            List<ThingCategoryDef> rootCategories = ThingCategoryDefOf.Root.childCategories.OrderBy(c => c.label).ToList();
            int numCategories = 0;
            foreach (var cat in rootCategories)
                count_descendants(cat, ref numCategories);

            var listing = new Listing_Standard();
            listing.Begin(rect);
            {
                listing.GapLine();
                draw_category_list(listing, $"[{_settings.allowByCategory.Count()}/{numCategories}]\tAllow By Category:", ref _scrollPositionAllow, _settings.allowByCategory);
                listing.GapLine();
                draw_category_list(listing, $"[{_settings.notifyByCategory.Count()}/{numCategories}]\tNotify By Category:", ref _scrollPositionNotify, _settings.notifyByCategory);
                listing.GapLine();
            }
            listing.End();

            base.DoSettingsWindowContents(rect);


            // local methods
            void draw_category_list(Listing_Standard parentListing, string title, ref Vector2 scrollPosition, HashSet<string> categorySet)
            {
                parentListing.Label(title);

                Rect scrollViewRect = parentListing.GetRect(216f);
                Rect viewRect = new Rect(0f, 0f, scrollViewRect.width - 16f, (Text.LineHeight + parentListing.verticalSpacing) * numCategories);

                Widgets.BeginScrollView(scrollViewRect, ref scrollPosition, viewRect);
                {
                    var categoryListing = new Listing_Standard();
                    categoryListing.Begin(viewRect);

                    for (int i = 0; i < rootCategories.Count; i++)
                    {
                        var child = rootCategories[i];
                        bool isLast = i == rootCategories.Count - 1;
                        draw_category(categoryListing, child, "", isLast, categorySet);
                    }

                    categoryListing.End();
                }
                Widgets.EndScrollView();

                // parentListing.Gap(4f);
                Rect buttonRect = parentListing.GetRect(30f);
                Rect leftButtonRect = buttonRect.LeftHalf().ContractedBy(2f);
                Rect rightButtonRect = buttonRect.RightHalf().ContractedBy(2f);
                if (Widgets.ButtonText(leftButtonRect, "Select everything"))
                {
                    foreach (var catDef in rootCategories)
                        SetCategoryAndDescendants(catDef, true, categorySet);
                }
                if (Widgets.ButtonText(rightButtonRect, "Select nothing"))
                {
                    categorySet.Clear();
                }
            }
            void draw_category(Listing_Standard lst, ThingCategoryDef catDef, string parentPrefix, bool isLast, HashSet<string> categorySet)
            {
                bool isEnabled = categorySet.Contains(catDef.defName);
                bool tempIsEnabled = isEnabled;

                var linePrefix = new System.Text.StringBuilder(parentPrefix);
                linePrefix.Append(isLast ? "└─ " : "├─ ");

                string label = linePrefix.ToString() + catDef.LabelCap;
                lst.CheckboxLabeled(label, ref tempIsEnabled, catDef.defName);

                if (tempIsEnabled != isEnabled)
                    SetCategoryAndDescendants(catDef, tempIsEnabled, categorySet);

                string childPrefix = parentPrefix + (isLast ? "      " : "│   ");

                var children = catDef.childCategories.OrderBy(c => c.label).ToList();
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    bool isLastChild = i == children.Count - 1;
                    draw_category(lst, child, childPrefix, isLastChild, categorySet);
                }
            }
            void count_descendants(ThingCategoryDef cat, ref int count)
            {
                count++;
                foreach (var child in cat.childCategories.OrderBy(c => c.label))
                    count_descendants(child, ref count);
            }
        }

        public static void SetCategoryAndDescendants(ThingCategoryDef cat, bool b, HashSet<string> set)
        {
            if (b) set.Add(cat.defName);
            else set.Remove(cat.defName);

            foreach (var child in cat.childCategories)
                SetCategoryAndDescendants(child, b, set);
        }

    }
}
