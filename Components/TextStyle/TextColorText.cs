using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sets the colour of the text in one or more shapes. The Slides counterpart of DocsConnect's Text Color Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    // the trait is a native Colour; it is converted to Color JSON before the base class sees it
    public class TextColorTextComponent : TextTraitComponent<string>
    {
        public TextColorTextComponent()
          : base("Text Color Text", "TextColor",
              "Sets the colour of the text in one or more shapes. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "Text Color";
        protected override string Fallback => "";
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Text Color", "FC", "Text colour: a Colour Swatch or a panel like 255,0,0.", GH_ParamAccess.list);
        }

        protected override List<string> ReadTrait(IGH_DataAccess DA)
        {
            var colors = new List<System.Drawing.Color>();
            DA.GetDataList(1, colors);
            return PluginUtilities.ToColorJsons(colors);
        }

        protected override string BuildOne(string objectId, string value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { TextColorJson = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_TextColorText;
        public override Guid ComponentGuid => new Guid("eee92b79-88f1-4a5f-bb1c-5778b575e726");
    }
}
