using Grasshopper.Kernel;
using System;

namespace SlidesConnect
{
    // Retrieves a single slide by its object ID.
    public class GetSlideComponent : GH_Component
    {
        public GetSlideComponent()
          : base("Get Slide", "GetSlide",
              "Retrieve a single slide by its object ID.",
              PluginUtilities.TabName, PluginUtilities.CategoryACSlides)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Presentation ID", "PID", "ID of the presentation.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID to fetch. A Layout or Master ID works too: it returns that page's raw JSON, handy for inspecting placeholders.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "R", "Raw page JSON.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Operation status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string presentationId = "";
            string pageId = "";

            DA.GetData(0, ref token);
            DA.GetData(1, ref presentationId);
            DA.GetData(2, ref pageId);

            if (string.IsNullOrWhiteSpace(presentationId) || string.IsNullOrWhiteSpace(pageId)) return;

            var client = new SlidesConnectClient(token);
            var result = client.GetPageAsync(presentationId, pageId).GetAwaiter().GetResult();

            if (!result.Item1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Item3);
                DA.SetData(0, result.Item3);
                DA.SetData(1, "Failed");
                return;
            }

            DA.SetData(0, result.Item2);
            DA.SetData(1, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_GetSlide;
        public override Guid ComponentGuid => new Guid("0128240a-dc45-4fda-8a27-5fc31e7f7b40");
    }
}
