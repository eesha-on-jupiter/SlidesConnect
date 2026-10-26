using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Strikes through the text in one or more shapes. The Slides counterpart of DocsConnect's Strikethrough Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class StrikethroughTextComponent : TextTraitComponent<bool>
    {
        public StrikethroughTextComponent()
          : base("Strikethrough Text", "Strike",
              "Strikes through the text in one or more shapes. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Strikethrough";
        protected override bool Fallback => true;
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Strikethrough", "SK", "True = strikethrough, false = remove.", GH_ParamAccess.list, true);
        }

        protected override string BuildOne(string objectId, bool value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { Strikethrough = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_StrikethroughText;
        public override Guid ComponentGuid => new Guid("242b1e05-468e-4356-b674-1acea7c090a7");
    }
}
