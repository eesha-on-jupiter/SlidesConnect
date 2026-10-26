using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Hyperlinks whole shapes (not just their text) — the single-purpose slice of Update Shape Style.
    public class ShapeLinkComponent : GH_Component
    {
        public ShapeLinkComponent()
          : base("Shape Link", "ShapeLink",
              "Hyperlinks one or more existing shapes to a URL — the whole shape becomes clickable. For linking only the text, use Link Text.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("URL", "LK", "Hyperlink URL the whole shape opens when clicked.", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateShapeProperties batchUpdate request JSON, one per shape.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var urls = new List<string>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, urls);

            int n = PluginUtilities.RecordCount(objectIds, urls);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("URL", urls.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            urls = PluginUtilities.MatchLength(urls, n, "");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string request = StyleBuilders.BuildUpdateShapeProperties(objectIds[i], new StyleBuilders.ShapeStyleSpec { LinkUrl = urls[i] });
                if (request != null) requests.Add(request);
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Supply an Object ID and a URL."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ShapeLink;
        public override Guid ComponentGuid => new Guid("4f6c52b6-4f28-4076-82f3-cc20e64aa32b");
    }
}
