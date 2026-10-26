using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Adds space below the paragraphs in one or more shapes. The Slides counterpart of DocsConnect's Space Below Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class SpaceBelowTextComponent : TextTraitComponent<double>
    {
        public SpaceBelowTextComponent()
          : base("Space Below Text", "SpaceBelow",
              "Adds space below the paragraphs in one or more shapes. One updateParagraphStyle request per record.")
        { }

        protected override string TraitName => "Space Below";
        protected override double Fallback => 6;
        protected override string RequestTypeName => "updateParagraphStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Space Below", "SB", "Space below each paragraph in points.", GH_ParamAccess.list, 6);
        }

        protected override string BuildOne(string objectId, double value)
            => StyleBuilders.BuildUpdateParagraphStyle(objectId, new StyleBuilders.ParagraphStyleSpec { SpaceBelowPt = PluginUtilities.Positive(value) });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_SpaceBelowText;
        public override Guid ComponentGuid => new Guid("63f40a9a-df9d-48ed-9b66-c8576f34ac30");
    }
}
