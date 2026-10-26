using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Underlines the text in one or more shapes. The Slides counterpart of DocsConnect's Underline Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class UnderlineTextComponent : TextTraitComponent<bool>
    {
        public UnderlineTextComponent()
          : base("Underline Text", "Underline",
              "Underlines the text in one or more shapes. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Underline";
        protected override bool Fallback => true;
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Underline", "U", "True = underline, false = remove.", GH_ParamAccess.list, true);
        }

        protected override string BuildOne(string objectId, bool value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { Underline = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_UnderlineText;
        public override Guid ComponentGuid => new Guid("7c496f33-707d-4d5f-81bd-7b927c88e351");
    }
}
