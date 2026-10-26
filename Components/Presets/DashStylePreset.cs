using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlidesConnect.Presets
{
    // Line dash values for Shape Outline / Table Borders.
    public class DashStylePresetComponent : GH_ValueList
    {
        public DashStylePresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Dash Style";
            NickName = "DS";
            Description = "Line dash values for Shape Outline / Table Borders.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Solid",          "SOLID" },
            { "Dot",            "DOT" },
            { "Dash",           "DASH" },
            { "Dash Dot",       "DASH_DOT" },
            { "Long Dash",      "LONG_DASH" },
            { "Long Dash Dot",  "LONG_DASH_DOT" },
        };

        protected override Bitmap Icon => Properties.Resources.GS_DashStylePreset;
        public override Guid ComponentGuid => new Guid("e749af3f-1719-47e8-a14a-10654137bbe7");
    }
}
