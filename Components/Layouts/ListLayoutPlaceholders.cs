using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Lists the placeholder page elements on a Layout (or Master) page — their own object ID and
    // placeholder type (TITLE, BODY, PICTURE, etc.). Scans both shape and image elements — a picture
    // placeholder that already holds an image comes back as an image element, not a shape. Feed a
    // Placeholder ID straight into Create
    // Slide's Title/Body/Picture Placeholder ID inputs to populate that placeholder deterministically
    // at slide-creation time, instead of relying on replaceAllText token matching.
    public class ListLayoutPlaceholdersComponent : GH_Component
    {
        public ListLayoutPlaceholdersComponent()
          : base("List Layout Placeholders", "Placeholders",
              "List the placeholder page elements on a Layout page (object ID + placeholder type).",
              PluginUtilities.TabName, PluginUtilities.CategoryADLayouts)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Presentation ID", "PID", "ID of the presentation.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Layout ID", "LID", "Object ID of the Layout page to inspect (from List Layouts).", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Placeholder ID", "PHID", "Object ID of each placeholder on the layout — wire into Create Slide's Title/Body/Picture Placeholder ID.", GH_ParamAccess.list);
            pManager.AddTextParameter("Placeholder Type", "PHT", "Placeholder type of each entry (TITLE, BODY, PICTURE, SUBTITLE, etc).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string presentationId = "";
            string layoutId = "";

            DA.GetData(0, ref token);
            DA.GetData(1, ref presentationId);
            DA.GetData(2, ref layoutId);

            if (string.IsNullOrWhiteSpace(presentationId) || string.IsNullOrWhiteSpace(layoutId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Presentation ID and Layout ID are both required.");
                return;
            }

            var client = new SlidesConnectClient(token);
            var result = client.GetPageAsync(presentationId, layoutId).GetAwaiter().GetResult();

            if (!result.Item1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Item3);
                return;
            }

            var ids = new List<string>();
            var types = new List<string>();

            var pageElements = JObject.Parse(result.Item2)["pageElements"] as JArray;
            if (pageElements != null)
            {
                foreach (var element in pageElements)
                {
                    var placeholder = element["shape"]?["placeholder"] ?? element["image"]?["placeholder"];
                    if (placeholder == null) continue;

                    string id = (string)element["objectId"] ?? "";
                    string type = (string)placeholder["type"] ?? "";
                    if (string.IsNullOrWhiteSpace(id)) continue;

                    ids.Add(id);
                    types.Add(type);
                }
            }

            if (ids.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No placeholders found on this layout.");
                return;
            }

            DA.SetDataList(0, ids);
            DA.SetDataList(1, types);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ListLayoutPlaceholders;
        public override Guid ComponentGuid => new Guid("262c5c64-aaf8-4b36-be14-1160f755d855");
    }
}
