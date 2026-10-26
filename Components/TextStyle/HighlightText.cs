using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Puts a highlight colour behind the text in one or more shapes. The Slides counterpart of DocsConnect's Highlight Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    // the trait is a native Colour; it is converted to Color JSON before the base class sees it
    public class HighlightTextComponent : TextTraitComponent<string>
    {
        public HighlightTextComponent()
          : base("Highlight Text", "Highlight",
              "Puts a highlight colour behind the text in one or more shapes. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Highlight";
        protected override string Fallback => "";
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Highlight", "HC", "Highlight (background) colour behind the text.", GH_ParamAccess.list);
        }

        protected override List<string> ReadTrait(IGH_DataAccess DA)
        {
            var colors = new List<System.Drawing.Color>();
            DA.GetDataList(1, colors);
            return PluginUtilities.ToColorJsons(colors);
        }

        protected override string BuildOne(string objectId, string value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { HighlightColorJson = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_HighlightText;
        public override Guid ComponentGuid => new Guid("626372f6-2683-44d1-abdb-3aa992bfbcbe");
    }
}
