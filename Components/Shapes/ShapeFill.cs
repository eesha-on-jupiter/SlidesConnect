using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sets the fill colour and/or transparency of existing shapes — the single-purpose slice of
    // Update Shape Style. Transparency alone keeps the current colour.
    public class ShapeFillComponent : GH_Component
    {
        public ShapeFillComponent()
          : base("Shape Fill", "Fill",
              "Sets the fill colour and/or transparency of one or more existing shapes. Only the inputs you connect are applied.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape to fill.", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Fill Color", "C", "Fill colour (connect to apply): a Colour Swatch or a panel like 255,0,0.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fill Transparency", "FT", "Transparency 0-1 (0 = opaque, 1 = invisible). Connect to apply.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateShapeProperties batchUpdate request JSON, one per shape.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var colors = new List<System.Drawing.Color>();
            var transparencies = new List<double>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, colors);
            DA.GetDataList(2, transparencies);
            var colorJsons = PluginUtilities.ToColorJsons(colors);

            int n = PluginUtilities.RecordCount(objectIds, colorJsons, transparencies);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Fill Color", colorJsons.Count), ("Fill Transparency", transparencies.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            colorJsons = PluginUtilities.MatchLength(colorJsons, n, "");
            var transparenciesN = PluginUtilities.MatchLengthOrNull(transparencies, n);

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string request = StyleBuilders.BuildUpdateShapeProperties(objectIds[i], new StyleBuilders.ShapeStyleSpec
                {
                    FillColorJson = colorJsons[i],
                    FillAlpha = transparenciesN[i].HasValue ? 1 - transparenciesN[i].Value : (double?)null
                });
                if (request != null) requests.Add(request);
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connect a Fill Color and/or Fill Transparency."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ShapeFill;
        public override Guid ComponentGuid => new Guid("a571be93-d78f-49ce-8c53-76121d594deb");
    }
}
