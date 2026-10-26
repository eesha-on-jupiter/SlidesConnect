using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Sets the outline colour / weight / dash of existing shapes — the single-purpose slice of
    // Update Shape Style.
    public class ShapeOutlineComponent : GH_Component
    {
        public ShapeOutlineComponent()
          : base("Shape Outline", "Outline",
              "Sets the outline colour, weight and/or dash style of one or more existing shapes. Only the inputs you connect are applied.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape to outline.", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Outline Color", "OC", "Outline colour (connect to apply).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Outline Weight", "OW", "Outline weight, in Unit (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Outline Dash", "OD", "SOLID, DOT, DASH, DASH_DOT, LONG_DASH or LONG_DASH_DOT (blank to skip) — see the Dash Style preset.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Unit", "U", "Unit for Outline Weight (pt, in, mm, cm, m, ft).", GH_ParamAccess.list, "pt");
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateShapeProperties batchUpdate request JSON, one per shape.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var colors = new List<System.Drawing.Color>();
            var weights = new List<double>();
            var dashes = new List<string>();
            var units = new List<string>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, colors);
            DA.GetDataList(2, weights);
            DA.GetDataList(3, dashes);
            DA.GetDataList(4, units);
            var colorJsons = PluginUtilities.ToColorJsons(colors);

            int n = PluginUtilities.RecordCount(objectIds, colorJsons, weights, dashes, units);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Outline Color", colorJsons.Count), ("Outline Weight", weights.Count), ("Outline Dash", dashes.Count), ("Unit", units.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            colorJsons = PluginUtilities.MatchLength(colorJsons, n, "");
            weights = PluginUtilities.MatchLength(weights, n, 0);
            dashes = PluginUtilities.MatchLength(dashes, n, "");
            units = PluginUtilities.MatchLength(units, n, "pt");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string request = StyleBuilders.BuildUpdateShapeProperties(objectIds[i], new StyleBuilders.ShapeStyleSpec
                {
                    OutlineColorJson = colorJsons[i],
                    OutlineWeightEmu = weights[i] > 0 ? GeometryBuilders.ToEmu(weights[i], units[i]) : (double?)null,
                    OutlineDash = dashes[i]
                });
                if (request != null) requests.Add(request);
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connect an Outline Color, Outline Weight and/or Outline Dash."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ShapeOutline;
        public override Guid ComponentGuid => new Guid("50e23488-90d4-4b33-81eb-0f3b149161fe");
    }
}
