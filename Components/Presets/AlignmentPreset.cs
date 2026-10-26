using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlidesConnect.Presets
{
    // Paragraph alignment values for Styled Paragraph / Aligned Text / Styled Text Box.
    public class AlignmentPresetComponent : GH_ValueList
    {
        public AlignmentPresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Alignment";
            NickName = "AL";
            Description = "Paragraph alignment values for Styled Paragraph / Aligned Text / Styled Text Box.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Start (Left)",  "START" },
            { "Center",        "CENTER" },
            { "End (Right)",   "END" },
            { "Justified",     "JUSTIFIED" },
        };

        protected override Bitmap Icon => Properties.Resources.GS_AlignmentPreset;
        public override Guid ComponentGuid => new Guid("93b1c72f-3c44-4c44-832b-b0af085a48f5");
    }
}
