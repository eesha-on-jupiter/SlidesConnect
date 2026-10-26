using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Adds space above the paragraphs in one or more shapes. The Slides counterpart of DocsConnect's Space Above Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class SpaceAboveTextComponent : TextTraitComponent<double>
    {
        public SpaceAboveTextComponent()
          : base("Space Above Text", "SpaceAbove",
              "Adds space above the paragraphs in one or more shapes. One updateParagraphStyle request per record.")
        { }

        protected override string TraitName => "Space Above";
        protected override double Fallback => 6;
        protected override string RequestTypeName => "updateParagraphStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Space Above", "SA", "Space above each paragraph in points.", GH_ParamAccess.list, 6);
        }

        protected override string BuildOne(string objectId, double value)
            => StyleBuilders.BuildUpdateParagraphStyle(objectId, new StyleBuilders.ParagraphStyleSpec { SpaceAbovePt = PluginUtilities.Positive(value) });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_SpaceAboveText;
        public override Guid ComponentGuid => new Guid("2fc1522b-1b4b-4656-9043-be0dd7c3a06d");
    }
}
