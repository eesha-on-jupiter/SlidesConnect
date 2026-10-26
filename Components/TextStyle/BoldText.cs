using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Makes the text in one or more shapes bold (or un-bold). The Slides counterpart of DocsConnect's Bold Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class BoldTextComponent : TextTraitComponent<bool>
    {
        public BoldTextComponent()
          : base("Bold Text", "Bold",
              "Makes the text in one or more shapes bold (or un-bold). One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Bold";
        protected override bool Fallback => true;
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Bold", "B", "True = bold, false = un-bold.", GH_ParamAccess.list, true);
        }

        protected override string BuildOne(string objectId, bool value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { Bold = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_BoldText;
        public override Guid ComponentGuid => new Guid("86731df2-fdde-4b07-9ac8-d961edb134cd");
    }
}
