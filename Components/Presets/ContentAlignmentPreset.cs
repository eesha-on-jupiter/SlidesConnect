using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlidesConnect.Presets
{
    // Vertical text alignment values for Shape Text Fit / Table Cell Style / Styled Text Box.
    public class ContentAlignmentPresetComponent : GH_ValueList
    {
        public ContentAlignmentPresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Content Alignment";
            NickName = "CA";
            Description = "Vertical text alignment values for Shape Text Fit / Table Cell Style / Styled Text Box.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Top",     "TOP" },
            { "Middle",  "MIDDLE" },
            { "Bottom",  "BOTTOM" },
        };

        protected override Bitmap Icon => Properties.Resources.GS_ContentAlignmentPreset;
        public override Guid ComponentGuid => new Guid("a5b05e18-32c4-48a2-8776-fc9c10aedf91");
    }
}
