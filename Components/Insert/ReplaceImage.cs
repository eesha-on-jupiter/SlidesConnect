using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds replaceImage requests: swaps the image content of one or more EXISTING image/picture-
    // placeholder elements (e.g. Create Slide's Picture Object ID output) with a new image URL.
    // Different from Create Image, which places a brand new image element.
    public class ReplaceImageComponent : GH_Component
    {
        public ReplaceImageComponent()
          : base("Replace Image", "ReplaceImg",
              "Build requests that swap the image content of one or more existing image elements (e.g. a filled Picture placeholder).",
              PluginUtilities.TabName, PluginUtilities.CategoryAIInsert)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the existing image element to replace.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Image URL", "URL", "Public image URL(s) (PNG/JPG/GIF, <= 50 MB / 25 MP).", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per image.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var imageUrls = new List<string>();

            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, imageUrls);

            int n = PluginUtilities.RecordCount(objectIds, imageUrls);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Image URL", imageUrls.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            imageUrls = PluginUtilities.MatchLength(imageUrls, n, "");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(objectIds[i]) || string.IsNullOrWhiteSpace(imageUrls[i])) continue;
                requests.Add(RequestBuilders.BuildReplaceImage(objectIds[i], imageUrls[i]));
            }

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ReplaceImage;
        public override Guid ComponentGuid => new Guid("b2fdce05-3cfe-41b1-9f31-a9ed80f4d2cf");
    }
}
