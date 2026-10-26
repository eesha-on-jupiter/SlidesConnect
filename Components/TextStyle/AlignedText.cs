using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sets the paragraph alignment of the text in one or more shapes. The Slides counterpart of DocsConnect's Aligned Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class AlignedTextComponent : TextTraitComponent<string>
    {
        public AlignedTextComponent()
          : base("Aligned Text", "Aligned",
              "Sets the paragraph alignment of the text in one or more shapes. One updateParagraphStyle request per record.")
        { }

        protected override string TraitName => "Alignment";
        protected override string Fallback => "CENTER";
        protected override string RequestTypeName => "updateParagraphStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Alignment", "AL", "Alignment: START, CENTER, END or JUSTIFIED — see the Alignment preset.", GH_ParamAccess.list, "CENTER");
        }

        protected override string BuildOne(string objectId, string value)
            => StyleBuilders.BuildUpdateParagraphStyle(objectId, new StyleBuilders.ParagraphStyleSpec { Alignment = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_AlignedText;
        public override Guid ComponentGuid => new Guid("66618543-fbff-4ae3-af3d-26d77bb21586");
    }
}
