using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sets the font family of the text in one or more shapes. The Slides counterpart of DocsConnect's Font Family Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class FontFamilyTextComponent : TextTraitComponent<string>
    {
        public FontFamilyTextComponent()
          : base("Font Family Text", "FontFamily",
              "Sets the font family of the text in one or more shapes. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Font Family";
        protected override string Fallback => "Arial";
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Font Family", "FF", "Font family name — see the Font Family preset, or type any Google Fonts name.", GH_ParamAccess.list, "Arial");
        }

        protected override string BuildOne(string objectId, string value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { FontFamily = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_FontFamilyText;
        public override Guid ComponentGuid => new Guid("756237d5-fc39-4905-ae90-1b17168e68b8");
    }
}
