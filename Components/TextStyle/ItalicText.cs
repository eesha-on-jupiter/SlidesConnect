using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Makes the text in one or more shapes italic. The Slides counterpart of DocsConnect's Italic Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class ItalicTextComponent : TextTraitComponent<bool>
    {
        public ItalicTextComponent()
          : base("Italic Text", "Italic",
              "Makes the text in one or more shapes italic. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Italic";
        protected override bool Fallback => true;
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Italic", "I", "True = italic, false = upright.", GH_ParamAccess.list, true);
        }

        protected override string BuildOne(string objectId, bool value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { Italic = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ItalicText;
        public override Guid ComponentGuid => new Guid("6b357fc2-1690-4ee8-9333-2d9c80b83055");
    }
}
