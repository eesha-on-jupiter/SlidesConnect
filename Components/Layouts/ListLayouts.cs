using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Lists the Layout pages available in a presentation, so a hand-designed custom layout can be
    // found by name and wired straight into Create Slide's Layout ID input instead of hunting for
    // its object ID in the raw Get Presentation JSON.
    public class ListLayoutsComponent : GH_Component
    {
        public ListLayoutsComponent()
          : base("List Layouts", "Layouts",
              "List the Layout pages available in a presentation (object ID + display name).",
              PluginUtilities.TabName, PluginUtilities.CategoryADLayouts)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Presentation ID", "PID", "ID of the presentation to inspect.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Layout ID", "LID", "Object ID of each layout — wire straight into Create Slide's Layout ID.", GH_ParamAccess.list);
            pManager.AddTextParameter("Layout Name", "LN", "Human-readable display name of each layout.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string presentationId = "";

            DA.GetData(0, ref token);
            DA.GetData(1, ref presentationId);

            if (string.IsNullOrWhiteSpace(presentationId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Presentation ID is empty.");
                return;
            }

            var client = new SlidesConnectClient(token);
            var result = client.GetPresentationAsync(presentationId).GetAwaiter().GetResult();

            if (!result.Item1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Item3);
                return;
            }

            var ids = new List<string>();
            var names = new List<string>();

            var layouts = JObject.Parse(result.Item2)["layouts"] as JArray;
            if (layouts != null)
            {
                foreach (var layout in layouts)
                {
                    string id = (string)layout["objectId"] ?? "";
                    string name = (string)layout["layoutProperties"]?["displayName"] ?? "";
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    ids.Add(id);
                    names.Add(name);
                }
            }

            if (ids.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No layouts found on this presentation.");
                return;
            }

            DA.SetDataList(0, ids);
            DA.SetDataList(1, names);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ListLayouts;
        public override Guid ComponentGuid => new Guid("b02592f2-d095-441b-8549-93055e86c23d");
    }
}
