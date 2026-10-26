using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Turns the text in one or more shapes into a hyperlink. The Slides counterpart of DocsConnect's Link Text: Docs inserts the text
    // at an index, Slides styles the text already in an object (Object ID). One request per record.
    public class LinkTextComponent : TextTraitComponent<string>
    {
        public LinkTextComponent()
          : base("Link Text", "Link",
              "Turns the text in one or more shapes into a hyperlink. One updateTextStyle request per record.")
        { }

        protected override string TraitName => "URL";
        protected override string Fallback => "";
        protected override string RequestTypeName => "updateTextStyle";

        protected override void RegisterTraitParam(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("URL", "LK", "Hyperlink URL.", GH_ParamAccess.list, "");
        }

        protected override string BuildOne(string objectId, string value)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec { LinkUrl = value });

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_LinkText;
        public override Guid ComponentGuid => new Guid("5f844f60-2947-414d-928e-946cdcb1048f");
    }
}
