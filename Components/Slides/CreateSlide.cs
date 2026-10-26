using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds createSlide requests: adds one or more new slides. Each slide can inherit a
    // predefined layout or an existing Layout by ID (mutually exclusive; Layout ID wins).
    // Optional Title/Body/Picture Placeholder ID (from List Placeholders) map that layout
    // placeholder to a fresh object ID at creation time — the deterministic alternative to
    // Slide Template Fill's text-token matching for populating a layout's placeholders.
    public class CreateSlideComponent : GH_Component
    {
        public CreateSlideComponent()
          : base("Create Slide", "Slide",
              "Build requests that add one or more new slides, optionally populating layout placeholders.",
              PluginUtilities.TabName, PluginUtilities.CategoryACSlides)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Layout", "LO", "Predefined layout (e.g. BLANK, TITLE, TITLE_AND_BODY). Ignored when Layout ID is supplied.", GH_ParamAccess.list, "BLANK");
            pManager.AddIntegerParameter("Insertion Index", "IX", "Optional 0-based slide insertion index (-1 = append).", GH_ParamAccess.list, -1);
            pManager.AddTextParameter("Object ID", "OID", "Optional object ID to assign the new slide. Auto-generated when blank.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Layout ID", "LID", "Optional object ID of an existing Layout page to inherit placeholders/background from.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Title Placeholder ID", "TPH", "Optional Title placeholder object ID on the Layout (from List Placeholders).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Body Placeholder ID", "BPH", "Optional Body placeholder object ID on the Layout (from List Placeholders).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Picture Placeholder ID", "PPH", "Optional Picture placeholder object ID on the Layout (from List Placeholders).", GH_ParamAccess.list, "");
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per slide.", GH_ParamAccess.list);
            pManager.AddTextParameter("Slide ID", "SID", "Resolved object ID of each new slide (supplied or auto-generated).", GH_ParamAccess.list);
            pManager.AddTextParameter("Title Object ID", "TOID", "Resolved object ID of the Title placeholder copy on each slide (blank where not mapped).", GH_ParamAccess.list);
            pManager.AddTextParameter("Body Object ID", "BOID", "Resolved object ID of the Body placeholder copy on each slide (blank where not mapped).", GH_ParamAccess.list);
            pManager.AddTextParameter("Picture Object ID", "POID", "Resolved object ID of the Picture placeholder copy on each slide (blank where not mapped).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var layouts = new List<string>();
            var indices = new List<int>();
            var objectIds = new List<string>();
            var layoutIds = new List<string>();
            var titlePhIds = new List<string>();
            var bodyPhIds = new List<string>();
            var picturePhIds = new List<string>();

            DA.GetDataList(0, layouts);
            DA.GetDataList(1, indices);
            DA.GetDataList(2, objectIds);
            DA.GetDataList(3, layoutIds);
            DA.GetDataList(4, titlePhIds);
            DA.GetDataList(5, bodyPhIds);
            DA.GetDataList(6, picturePhIds);

            int n = PluginUtilities.RecordCount(layouts, indices, objectIds, layoutIds, titlePhIds, bodyPhIds, picturePhIds);
            PluginUtilities.WarnIfRagged(this, n,
                ("Layout", layouts.Count), ("Insertion Index", indices.Count),
                ("Object ID", objectIds.Count), ("Layout ID", layoutIds.Count),
                ("Title Placeholder ID", titlePhIds.Count), ("Body Placeholder ID", bodyPhIds.Count), ("Picture Placeholder ID", picturePhIds.Count));

            layouts = PluginUtilities.MatchLength(layouts, n, "BLANK");
            indices = PluginUtilities.MatchLength(indices, n, -1);
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            layoutIds = PluginUtilities.MatchLength(layoutIds, n, "");
            titlePhIds = PluginUtilities.MatchLength(titlePhIds, n, "");
            bodyPhIds = PluginUtilities.MatchLength(bodyPhIds, n, "");
            picturePhIds = PluginUtilities.MatchLength(picturePhIds, n, "");

            var requests = new List<string>();
            var slideIds = new List<string>();
            var titleObjectIds = new List<string>();
            var bodyObjectIds = new List<string>();
            var pictureObjectIds = new List<string>();

            for (int i = 0; i < n; i++)
            {
                string slideId = RequestBuilders.EnsureId(objectIds[i], "slide");
                slideIds.Add(slideId);

                var layoutPhIds = new List<string>();
                var resolvedPhIds = new List<string>();

                string titleOid = "";
                if (!string.IsNullOrWhiteSpace(titlePhIds[i]))
                {
                    titleOid = RequestBuilders.EnsureId("", "title");
                    layoutPhIds.Add(titlePhIds[i]);
                    resolvedPhIds.Add(titleOid);
                }
                titleObjectIds.Add(titleOid);

                string bodyOid = "";
                if (!string.IsNullOrWhiteSpace(bodyPhIds[i]))
                {
                    bodyOid = RequestBuilders.EnsureId("", "body");
                    layoutPhIds.Add(bodyPhIds[i]);
                    resolvedPhIds.Add(bodyOid);
                }
                bodyObjectIds.Add(bodyOid);

                string pictureOid = "";
                if (!string.IsNullOrWhiteSpace(picturePhIds[i]))
                {
                    pictureOid = RequestBuilders.EnsureId("", "pic");
                    layoutPhIds.Add(picturePhIds[i]);
                    resolvedPhIds.Add(pictureOid);
                }
                pictureObjectIds.Add(pictureOid);

                requests.Add(RequestBuilders.BuildCreateSlide(
                    layouts[i], indices[i] < 0 ? (int?)null : indices[i], slideId,
                    string.IsNullOrWhiteSpace(layoutIds[i]) ? null : layoutIds[i],
                    layoutPhIds.Count > 0 ? layoutPhIds : null,
                    resolvedPhIds.Count > 0 ? resolvedPhIds : null));
            }

            DA.SetDataList(0, requests);
            DA.SetDataList(1, slideIds);
            DA.SetDataList(2, titleObjectIds);
            DA.SetDataList(3, bodyObjectIds);
            DA.SetDataList(4, pictureObjectIds);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_CreateSlide;
        public override Guid ComponentGuid => new Guid("39e41419-7f23-42aa-8aba-4aae6ede1543");
    }
}
