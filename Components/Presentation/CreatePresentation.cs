using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using System;

namespace SlidesConnect
{
    // Creates a new blank presentation and returns its ID. Press the button to run.
    public class CreatePresentationComponent : ButtonComponent
    {
        public CreatePresentationComponent()
          : base("Create Presentation", "Create",
              "Create a new blank Google Slides presentation with the given title.",
              PluginUtilities.TabName, PluginUtilities.CategoryABPresentation)
        { }

        public override string ButtonLabel => "Create";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Title", "N", "Presentation title.", GH_ParamAccess.item, "Untitled presentation");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Presentation ID", "PID", "ID of the new presentation.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "S", "Operation status.", GH_ParamAccess.item);
            pManager.AddTextParameter("Response", "R", "Raw API response.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered) return;

            string token = "";
            string title = "Untitled presentation";

            DA.GetData(0, ref token);
            DA.GetData(1, ref title);

            var client = new SlidesConnectClient(token);
            var result = client.CreatePresentationAsync(title).GetAwaiter().GetResult();

            if (!result.Item1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Item3);
                DA.SetData(1, "Failed");
                DA.SetData(2, result.Item3);
                return;
            }

            string presentationId = "";
            try { presentationId = (string)JObject.Parse(result.Item2)["presentationId"] ?? ""; } catch { }

            DA.SetData(0, presentationId);
            DA.SetData(1, "Created");
            DA.SetData(2, result.Item2);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_CreatePresentation;
        public override Guid ComponentGuid => new Guid("ae583e89-41eb-42ee-9788-24c7575c6b30");
    }
}
