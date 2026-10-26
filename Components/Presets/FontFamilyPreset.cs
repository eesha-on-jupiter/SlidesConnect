using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlidesConnect.Presets
{
    // Curated shortcut list of common font families for Styled Text / Font Family Text — not a closed list, any Google Fonts name can be typed directly.
    public class FontFamilyPresetComponent : GH_ValueList
    {
        public FontFamilyPresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Font Family";
            NickName = "FF";
            Description = "Curated shortcut list of common font families for Styled Text / Font Family Text — not a closed list, any Google Fonts name can be typed directly.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Arial",            "Arial" },
            { "Calibri",          "Calibri" },
            { "Times New Roman",  "Times New Roman" },
            { "Georgia",          "Georgia" },
            { "Verdana",          "Verdana" },
            { "Courier New",      "Courier New" },
            { "Garamond",         "Garamond" },
            { "Trebuchet MS",     "Trebuchet MS" },
            { "Roboto",           "Roboto" },
            { "Open Sans",        "Open Sans" },
        };

        protected override Bitmap Icon => Properties.Resources.GS_FontFamilyPreset;
        public override Guid ComponentGuid => new Guid("024f49f1-3a29-41a3-a3f6-eed10630cafa");
    }
}
