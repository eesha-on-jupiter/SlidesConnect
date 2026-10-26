using Grasshopper.Kernel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // The one-component path from "a layout + flat lists" to "a finished deck's worth of requests."
    // Give it a Layout ID and up to four flat lists (Title/Subtitle/Body/Photo URL) — it fetches the
    // layout once, finds its TITLE-or-CENTERED_TITLE/SUBTITLE/BODY/PICTURE placeholders itself, and
    // for each record builds a createSlide (with placeholderIdMappings) plus the matching
    // insertText/replaceImage requests, in order. No List Placeholders, no ID-threading, no
    // text-token matching. Wire the output straight into Batch Update (skip Requests Aggregator
    // unless combining with other content).
    public class BuildSlidesFromLayoutComponent : GH_Component
    {
        public BuildSlidesFromLayoutComponent()
          : base("Build Slides From Layout", "BuildSlides",
              "Build a full deck's requests from a Layout and Title/Subtitle/Body/Photo URL lists — one slide per record.",
              PluginUtilities.TabName, PluginUtilities.CategoryADLayouts)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Presentation ID", "PID", "ID of the presentation containing the layout.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Layout ID", "LID", "Object ID of the Layout to build slides from (from List Layouts).", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Title", "TI", "Title text, one per slide.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Subtitle", "SU", "Subtitle text, one per slide.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Body", "BD", "Body text, one per slide.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Photo URL", "PH", "Public image URL, one per slide.", GH_ParamAccess.list, "");
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON for the whole deck — wire straight into Batch Update.", GH_ParamAccess.list);
            pManager.AddTextParameter("Slide ID", "SID", "Object ID of each new slide.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string token = "";
            string presentationId = "";
            string layoutId = "";
            var titles = new List<string>();
            var subtitles = new List<string>();
            var bodies = new List<string>();
            var photoUrls = new List<string>();

            DA.GetData(0, ref token);
            DA.GetData(1, ref presentationId);
            DA.GetData(2, ref layoutId);
            DA.GetDataList(3, titles);
            DA.GetDataList(4, subtitles);
            DA.GetDataList(5, bodies);
            DA.GetDataList(6, photoUrls);

            if (string.IsNullOrWhiteSpace(presentationId) || string.IsNullOrWhiteSpace(layoutId))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Presentation ID and Layout ID are both required.");
                return;
            }

            // one live call: find this layout's placeholders itself, so nothing downstream needs to
            var client = new SlidesConnectClient(token);
            var pageResult = client.GetPageAsync(presentationId, layoutId).GetAwaiter().GetResult();
            if (!pageResult.Item1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, pageResult.Item3);
                return;
            }

            string titlePhId = "", subtitlePhId = "", bodyPhId = "", picturePhId = "";
            var pageElements = JObject.Parse(pageResult.Item2)["pageElements"] as JArray;
            if (pageElements != null)
            {
                foreach (var element in pageElements)
                {
                    var placeholder = element["shape"]?["placeholder"] ?? element["image"]?["placeholder"];
                    if (placeholder == null) continue;
                    string id = (string)element["objectId"] ?? "";
                    string type = (string)placeholder["type"] ?? "";
                    if (string.IsNullOrWhiteSpace(id)) continue;

                    if ((type == "TITLE" || type == "CENTERED_TITLE") && titlePhId == "") titlePhId = id;
                    else if (type == "SUBTITLE" && subtitlePhId == "") subtitlePhId = id;
                    else if (type == "BODY" && bodyPhId == "") bodyPhId = id;
                    else if (type == "PICTURE" && picturePhId == "") picturePhId = id;
                }
            }

            if (PluginUtilities.HasAny(titles) && titlePhId == "") AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Layout has no TITLE placeholder — Title values ignored.");
            if (PluginUtilities.HasAny(subtitles) && subtitlePhId == "") AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Layout has no SUBTITLE placeholder — Subtitle values ignored.");
            if (PluginUtilities.HasAny(bodies) && bodyPhId == "") AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Layout has no BODY placeholder — Body values ignored.");
            if (PluginUtilities.HasAny(photoUrls) && picturePhId == "") AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Layout has no PICTURE placeholder — Photo URL values ignored.");

            int n = PluginUtilities.RecordCount(titles, subtitles, bodies, photoUrls);
            titles = PluginUtilities.MatchLength(titles, n, "");
            subtitles = PluginUtilities.MatchLength(subtitles, n, "");
            bodies = PluginUtilities.MatchLength(bodies, n, "");
            photoUrls = PluginUtilities.MatchLength(photoUrls, n, "");

            var requests = new List<string>();
            var slideIds = new List<string>();

            for (int i = 0; i < n; i++)
            {
                string slideId = RequestBuilders.EnsureId("", "slide");
                var layoutPhIds = new List<string>();
                var resolvedPhIds = new List<string>();

                string titleOid = "", subtitleOid = "", bodyOid = "", pictureOid = "";
                if (titlePhId != "" && !string.IsNullOrWhiteSpace(titles[i]))
                {
                    titleOid = RequestBuilders.EnsureId("", "title");
                    layoutPhIds.Add(titlePhId); resolvedPhIds.Add(titleOid);
                }
                if (subtitlePhId != "" && !string.IsNullOrWhiteSpace(subtitles[i]))
                {
                    subtitleOid = RequestBuilders.EnsureId("", "subtitle");
                    layoutPhIds.Add(subtitlePhId); resolvedPhIds.Add(subtitleOid);
                }
                if (bodyPhId != "" && !string.IsNullOrWhiteSpace(bodies[i]))
                {
                    bodyOid = RequestBuilders.EnsureId("", "body");
                    layoutPhIds.Add(bodyPhId); resolvedPhIds.Add(bodyOid);
                }
                if (picturePhId != "" && !string.IsNullOrWhiteSpace(photoUrls[i]))
                {
                    pictureOid = RequestBuilders.EnsureId("", "pic");
                    layoutPhIds.Add(picturePhId); resolvedPhIds.Add(pictureOid);
                }

                requests.Add(RequestBuilders.BuildCreateSlide("BLANK", null, slideId, layoutId,
                    layoutPhIds.Count > 0 ? layoutPhIds : null, resolvedPhIds.Count > 0 ? resolvedPhIds : null));

                if (titleOid != "") requests.Add(RequestBuilders.BuildInsertText(titleOid, titles[i], 0));
                if (subtitleOid != "") requests.Add(RequestBuilders.BuildInsertText(subtitleOid, subtitles[i], 0));
                if (bodyOid != "") requests.Add(RequestBuilders.BuildInsertText(bodyOid, bodies[i], 0));
                if (pictureOid != "") requests.Add(RequestBuilders.BuildReplaceImage(pictureOid, photoUrls[i]));

                slideIds.Add(slideId);
            }

            DA.SetDataList(0, requests);
            DA.SetDataList(1, slideIds);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_BuildSlidesFromLayout;
        public override Guid ComponentGuid => new Guid("8b4fade9-118e-45c3-8c48-219e8d2e1950");
    }
}
