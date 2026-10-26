using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Adds or removes an outer drop shadow on existing shapes — the single-purpose slice of Update
    // Shape Style, with the shadow's look exposed (colour, transparency, blur, offset).
    public class ShapeShadowComponent : GH_Component
    {
        public ShapeShadowComponent()
          : base("Shape Shadow", "Shadow",
              "Adds a soft outer drop shadow to one or more existing shapes (colour, transparency, blur, offset), or removes it with Shadow = false.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape.", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("Shadow", "SH", "True = add the shadow, false = remove it.", GH_ParamAccess.list, true);
            pManager.AddColourParameter("Shadow Color", "SC", "Shadow colour (default black).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Transparency", "ST", "Shadow transparency 0-1 (0 = solid, 1 = invisible).", GH_ParamAccess.list, 0.6);
            pManager.AddNumberParameter("Blur", "BR", "Blur radius, in Unit.", GH_ParamAccess.list, 3);
            pManager.AddNumberParameter("Offset X", "OX", "Horizontal offset, in Unit (positive = right).", GH_ParamAccess.list, 3);
            pManager.AddNumberParameter("Offset Y", "OY", "Vertical offset, in Unit (positive = down).", GH_ParamAccess.list, 3);
            pManager.AddTextParameter("Unit", "U", "Unit for Blur / Offset (pt, in, mm, cm, m, ft).", GH_ParamAccess.list, "pt");
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateShapeProperties batchUpdate request JSON, one per shape.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var ons = new List<bool>();
            var colors = new List<System.Drawing.Color>();
            var transparencies = new List<double>();
            var blurs = new List<double>();
            var oxs = new List<double>();
            var oys = new List<double>();
            var units = new List<string>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, ons);
            DA.GetDataList(2, colors);
            DA.GetDataList(3, transparencies);
            DA.GetDataList(4, blurs);
            DA.GetDataList(5, oxs);
            DA.GetDataList(6, oys);
            DA.GetDataList(7, units);
            var colorJsons = PluginUtilities.ToColorJsons(colors);

            int n = PluginUtilities.RecordCount(objectIds, ons, colorJsons, transparencies, blurs, oxs, oys, units);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Shadow", ons.Count), ("Shadow Color", colorJsons.Count),
                ("Transparency", transparencies.Count), ("Blur", blurs.Count), ("Offset X", oxs.Count), ("Offset Y", oys.Count), ("Unit", units.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            ons = PluginUtilities.MatchLength(ons, n, true);
            colorJsons = PluginUtilities.MatchLength(colorJsons, n, "");
            transparencies = PluginUtilities.MatchLength(transparencies, n, 0.6);
            blurs = PluginUtilities.MatchLength(blurs, n, 3);
            oxs = PluginUtilities.MatchLength(oxs, n, 3);
            oys = PluginUtilities.MatchLength(oys, n, 3);
            units = PluginUtilities.MatchLength(units, n, "pt");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(objectIds[i])) continue;
                requests.Add(StyleBuilders.BuildUpdateShapeProperties(objectIds[i], new StyleBuilders.ShapeStyleSpec
                {
                    Shadow = ons[i],
                    ShadowColorJson = colorJsons[i],
                    ShadowAlpha = 1 - transparencies[i],
                    ShadowBlurEmu = GeometryBuilders.ToEmu(blurs[i], units[i]),
                    ShadowOffsetXEmu = GeometryBuilders.ToEmu(oxs[i], units[i]),
                    ShadowOffsetYEmu = GeometryBuilders.ToEmu(oys[i], units[i])
                }));
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Object ID supplied."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ShapeShadow;
        public override Guid ComponentGuid => new Guid("baeb3e5f-4c34-4add-aad3-06a3011baf50");
    }
}
