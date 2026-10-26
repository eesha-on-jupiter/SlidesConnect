using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlidesConnect.Presets
{
    // Which borders of a table range Table Borders styles.
    public class BorderPositionPresetComponent : GH_ValueList
    {
        public BorderPositionPresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Border Position";
            NickName = "BP";
            Description = "Which borders of a table range Table Borders styles.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "All",               "ALL" },
            { "Outer",             "OUTER" },
            { "Inner",             "INNER" },
            { "Inner Horizontal",  "INNER_HORIZONTAL" },
            { "Inner Vertical",    "INNER_VERTICAL" },
            { "Top",               "TOP" },
            { "Bottom",            "BOTTOM" },
            { "Left",              "LEFT" },
            { "Right",             "RIGHT" },
        };

        protected override Bitmap Icon => Properties.Resources.GS_BorderPositionPreset;
        public override Guid ComponentGuid => new Guid("243aaf2c-ccee-481f-92ef-825931e9eff6");
    }
}
