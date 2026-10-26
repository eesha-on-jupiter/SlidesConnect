using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Indents the paragraphs in one or more shapes. The Slides counterpart of DocsConnect's Indent Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class IndentTextComponent : TextTraitComponent<double>
    {
        public IndentTextComponent()
          : base("Indent Text", "Indent",
              "Indents the paragraphs in one or more shapes. One updateParagraphStyle request per record.")
        { }

        protected override string TraitName => "Indent";
        protected override double Fallback => 18;
        protected override string RequestTypeName => "updateParagraphStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Indent", "IN", "Start indent in points.", GH_ParamAccess.list, 18);
        }

        protected override string BuildOne(string objectId, double value)
            => StyleBuilders.BuildUpdateParagraphStyle(objectId, new StyleBuilders.ParagraphStyleSpec { IndentStartPt = PluginUtilities.Positive(value) });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_IndentText;
        public override Guid ComponentGuid => new Guid("345c356e-3ab9-40f6-89d6-985b6b2a16da");
    }
}
