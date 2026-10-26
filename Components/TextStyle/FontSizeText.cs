using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sets the font size of the text in one or more shapes. The Slides counterpart of DocsConnect's Font Size Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class FontSizeTextComponent : TextTraitComponent<double>
    {
        public FontSizeTextComponent()
          : base("Font Size Text", "FontSize",
              "Sets the font size of the text in one or more shapes. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Font Size";
        protected override double Fallback => 18;
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Font Size", "FS", "Font size in points.", GH_ParamAccess.list, 18);
        }

        protected override string BuildOne(string objectId, double value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { FontSizePt = PluginUtilities.Positive(value) });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_FontSizeText;
        public override Guid ComponentGuid => new Guid("76e40e43-aaef-4163-82ba-293a46f1bd19");
    }
}
