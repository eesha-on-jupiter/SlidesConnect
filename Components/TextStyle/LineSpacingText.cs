using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sets the line spacing of the paragraphs in one or more shapes. The Slides counterpart of DocsConnect's Line Spacing Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class LineSpacingTextComponent : TextTraitComponent<double>
    {
        public LineSpacingTextComponent()
          : base("Line Spacing Text", "LineSpacing",
              "Sets the line spacing of the paragraphs in one or more shapes. One updateParagraphStyle request per record.")
        { }

        protected override string TraitName => "Line Spacing";
        protected override double Fallback => 115;
        protected override string RequestTypeName => "updateParagraphStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Line Spacing", "LS", "Line spacing as a percent of normal (100 = single).", GH_ParamAccess.list, 115);
        }

        protected override string BuildOne(string objectId, double value)
            => StyleBuilders.BuildUpdateParagraphStyle(objectId, new StyleBuilders.ParagraphStyleSpec { LineSpacingPercent = PluginUtilities.Positive(value) });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_LineSpacingText;
        public override Guid ComponentGuid => new Guid("a24823f9-7328-4b6c-a87c-b23fbc990042");
    }
}
