using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlidesConnect.Presets
{
    // Bullet / numbering preset values for Bullet List / Styled Text Box (Google Slides BulletGlyphPreset names).
    public class BulletPresetComponent : GH_ValueList
    {
        public BulletPresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Bullet Preset";
            NickName = "BP";
            Description = "Bullet / numbering preset values for Bullet List / Styled Text Box (Google Slides BulletGlyphPreset names).";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Disc / Circle / Square",    "BULLET_DISC_CIRCLE_SQUARE" },
            { "Diamond / Arrow / Square",  "BULLET_DIAMONDX_ARROW3D_SQUARE" },
            { "Checkbox",                  "BULLET_CHECKBOX" },
            { "Arrow / Diamond / Disc",    "BULLET_ARROW_DIAMOND_DISC" },
            { "Star / Circle / Square",    "BULLET_STAR_CIRCLE_SQUARE" },
            { "Arrow / Circle / Square",   "BULLET_ARROW3D_CIRCLE_SQUARE" },
            { "Numbered (1, a, i)",        "NUMBERED_DIGIT_ALPHA_ROMAN" },
            { "Numbered (1), a), i)",      "NUMBERED_DIGIT_ALPHA_ROMAN_PARENS" },
            { "Numbered (nested 1.1)",     "NUMBERED_DIGIT_NESTED" },
            { "Numbered (A, a, i)",        "NUMBERED_UPPERALPHA_ALPHA_ROMAN" },
            { "Numbered (I, A, 1)",        "NUMBERED_UPPERROMAN_UPPERALPHA_DIGIT" },
            { "Remove bullets",            "NONE" },
        };

        protected override Bitmap Icon => Properties.Resources.GS_BulletPreset;
        public override Guid ComponentGuid => new Guid("7b68a3c1-b090-4bb2-b9bd-65b383aef059");
    }
}
