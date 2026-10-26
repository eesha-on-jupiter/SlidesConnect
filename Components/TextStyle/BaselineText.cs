using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Raises or lowers the text in one or more shapes (superscript / subscript). The Slides counterpart of DocsConnect's Baseline Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class BaselineTextComponent : TextTraitComponent<string>
    {
        public BaselineTextComponent()
          : base("Baseline Text", "Baseline",
              "Raises or lowers the text in one or more shapes (superscript / subscript). One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Baseline";
        protected override string Fallback => "SUPERSCRIPT";
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Baseline", "BL", "Baseline offset: SUPERSCRIPT, SUBSCRIPT or NONE.", GH_ParamAccess.list, "SUPERSCRIPT");
        }

        protected override string BuildOne(string objectId, string value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { Baseline = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_BaselineText;
        public override Guid ComponentGuid => new Guid("1632d953-db8a-43cd-bfd3-a9ad5d7370c7");
    }
}
