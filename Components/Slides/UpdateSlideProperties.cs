using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds updatePageProperties requests: sets one or more slides' background color.
    public class UpdateSlidePropertiesComponent : GH_Component
    {
        public UpdateSlidePropertiesComponent()
          : base("Update Slide Background", "SlideBg",
              "Build requests that set one or more slides' background color.",
              PluginUtilities.TabName, PluginUtilities.CategoryACSlides)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID(s) to recolor.", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Color", "C", "Background colour(s): a Colour Swatch or a panel like 255,0,0.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per slide.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pageIds = new List<string>();
            var colors = new List<System.Drawing.Color>();

            DA.GetDataList(0, pageIds);
            DA.GetDataList(1, colors);
            var colorJsons = PluginUtilities.ToColorJsons(colors);

            int n = PluginUtilities.RecordCount(pageIds, colorJsons);
            PluginUtilities.WarnIfRagged(this, n, ("Slide ID", pageIds.Count), ("Color", colorJsons.Count));

            pageIds = PluginUtilities.MatchLength(pageIds, n, "");
            colorJsons = PluginUtilities.MatchLength(colorJsons, n, "");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
                requests.Add(RequestBuilders.BuildUpdatePageProperties(pageIds[i], colorJsons[i]));

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_UpdateSlideProperties;
        public override Guid ComponentGuid => new Guid("ff682de9-4b54-4b7f-87f8-7c9971e3d9c8");
    }
}
