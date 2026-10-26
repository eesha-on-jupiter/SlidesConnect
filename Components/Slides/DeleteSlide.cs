using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds deleteObject requests: removes one or more slides or page elements by object ID.
    public class DeleteSlideComponent : GH_Component
    {
        public DeleteSlideComponent()
          : base("Delete Slide", "DeleteSlide",
              "Build requests that delete one or more slides or page elements by object ID.",
              PluginUtilities.TabName, PluginUtilities.CategoryACSlides)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID(s) to delete. (Any element's Object ID works too; the API's deleteObject removes shapes, images and tables the same way.)", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per object.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            DA.GetDataList(0, objectIds);

            var requests = new List<string>();
            foreach (var objectId in objectIds)
            {
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Skipped an empty Slide ID.");
                    continue;
                }
                requests.Add(RequestBuilders.BuildDeleteObject(objectId));
            }

            if (requests.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No valid Slide IDs supplied.");
                return;
            }

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_DeleteSlide;
        public override Guid ComponentGuid => new Guid("f9cd81e7-31b7-4802-9e9d-fa13de39665f");
    }
}
