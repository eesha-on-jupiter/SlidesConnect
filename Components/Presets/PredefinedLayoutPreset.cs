using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlidesConnect.Presets
{
    // Provides predefined slide layout values for Create Slide.
    public class PredefinedLayoutPresetComponent : GH_ValueList
    {
        public PredefinedLayoutPresetComponent()
        {
            Category = PluginUtilities.TabName;
            SubCategory = PluginUtilities.CategoryZYPresets;
            Name = "Predefined Layout";
            NickName = "Layout";
            Description = "Predefined Google Slides slide layouts.";

            ListItems.Clear();
            foreach (var kvp in Items)
                ListItems.Add(new GH_ValueListItem(kvp.Key, $"\"{kvp.Value}\""));
        }

        private static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            { "Blank",                       "BLANK"                          },
            { "Title",                       "TITLE"                          },
            { "Title and Body",              "TITLE_AND_BODY"                 },
            { "Title Only",                  "TITLE_ONLY"                     },
            { "Section Header",              "SECTION_HEADER"                 },
            { "Section Title and Desc",      "SECTION_TITLE_AND_DESCRIPTION"  },
            { "One Column Text",             "ONE_COLUMN_TEXT"                },
            { "Main Point",                  "MAIN_POINT"                     },
            { "Caption Only",                "CAPTION_ONLY"                   },
            { "Big Number",                  "BIG_NUMBER"                     },
        };

        protected override Bitmap Icon => Properties.Resources.GS_PredefinedLayoutPreset;
        public override Guid ComponentGuid => new Guid("7ad788bc-b61a-4be5-9ed3-f85e72e08221");
    }
}
