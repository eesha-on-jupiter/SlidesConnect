using Grasshopper.Kernel;
using System;

namespace SlidesConnect
{
    // Retrieves the full JSON of a presentation by ID, or by title via a Drive lookup.
    public class GetPresentationComponent : GH_Component
    {
        public GetPresentationComponent()
          : base("Get Presentation", "GetPres",
              "Retrieve the full JSON of a presentation by ID, or by title.",
              PluginUtilities.TabName, PluginUtilities.CategoryABPresentation)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token (scope https://www.googleapis.com/auth/presentations; add https://www.googleapis.com/auth/drive.readonly too if using Name).", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Presentation ID", "PID", "ID of the presentation to fetch. Takes priority over Name when both are supplied.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Name", "N", "Presentation title to look up via Drive when Presentation ID is blank. Most-recently-modified match wins if the title isn't unique.", GH_ParamAccess.item, "");
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "R", "Raw presentation JSON.", GH_ParamAccess.item);
            pManager.AddTextParameter("Presentation ID", "PID", "Resolved presentation ID (supplied, or found by Name).", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Operation status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string presentationId = "";
            string name = "";

            DA.GetData(0, ref token);
            DA.GetData(1, ref presentationId);
            DA.GetData(2, ref name);

            var client = new SlidesConnectClient(token);

            if (string.IsNullOrWhiteSpace(presentationId))
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Supply a Presentation ID or a Name.");
                    return;
                }

                var lookup = client.FindPresentationIdByNameAsync(name).GetAwaiter().GetResult();
                if (!lookup.Item1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, lookup.Item3);
                    DA.SetData(2, "Failed: " + lookup.Item3);
                    return;
                }
                presentationId = lookup.Item2;
            }

            var result = client.GetPresentationAsync(presentationId).GetAwaiter().GetResult();

            if (!result.Item1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Item3);
                DA.SetData(0, result.Item3);
                DA.SetData(1, presentationId);
                DA.SetData(2, "Failed");
                return;
            }

            DA.SetData(0, result.Item2);
            DA.SetData(1, presentationId);
            DA.SetData(2, "OK");
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_GetPresentation;
        public override Guid ComponentGuid => new Guid("02ce8adc-f1fb-4fc4-942c-15ad86bd5eb3");
    }
}
