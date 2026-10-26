using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Restyles one or more existing shapes with any combination of shape-level traits — fill colour
    // and transparency, outline colour / weight / dash, drop shadow, vertical text alignment, autofit
    // and hyperlink. Only the inputs you connect are applied. The single-purpose Shape Fill / Shape
    // Outline / Shape Shadow / Shape Link / Shape Text Fit components each cover one of these.
    public class UpdateShapePropertiesComponent : GH_Component
    {
        public UpdateShapePropertiesComponent()
          : base("Update Shape Style", "ShapeStyle",
              "Restyles one or more shapes with any combination of fill colour/transparency, outline colour/weight/dash, shadow, vertical text alignment, autofit and link. Only the inputs you connect are applied.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape to style.", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Fill Color", "C", "Fill colour(s) (connect to apply): a Colour Swatch or a panel like 255,0,0.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fill Transparency", "FT", "Fill transparency 0-1 (0 = opaque, 1 = invisible). Connect to apply.", GH_ParamAccess.list);
            pManager.AddColourParameter("Outline Color", "OC", "Outline colour (connect to apply).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Outline Weight", "OW", "Outline weight, in Unit (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Outline Dash", "OD", "Outline dash style: SOLID, DOT, DASH, DASH_DOT, LONG_DASH, LONG_DASH_DOT (blank to skip) — see the Dash Style preset.", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("Shadow", "SH", "True = add a soft outer drop shadow, false = remove the shadow. Connect to apply.", GH_ParamAccess.list);
            pManager.AddTextParameter("Content Alignment", "CA", "Vertical alignment of the shape's text: TOP, MIDDLE, BOTTOM (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Autofit", "AF", "NONE, TEXT_AUTOFIT (shrink text to fit) or SHAPE_AUTOFIT (grow shape to fit). Blank to skip.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Link URL", "LK", "Hyperlink the whole shape to this URL (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Unit", "U", "Unit for Outline Weight (pt, in, mm, cm, m, ft).", GH_ParamAccess.list, "pt");
            foreach (int i in new[] { 1, 2, 3, 6 }) pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateShapeProperties batchUpdate request JSON, one per shape.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var fills = new List<System.Drawing.Color>();
            var transparencies = new List<double>();
            var outlineColors = new List<System.Drawing.Color>();
            var outlineWeights = new List<double>();
            var dashes = new List<string>();
            var shadows = new List<bool>();
            var contentAlignments = new List<string>();
            var autofits = new List<string>();
            var links = new List<string>();
            var units = new List<string>();

            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, fills);
            DA.GetDataList(2, transparencies);
            DA.GetDataList(3, outlineColors);
            DA.GetDataList(4, outlineWeights);
            DA.GetDataList(5, dashes);
            DA.GetDataList(6, shadows);
            DA.GetDataList(7, contentAlignments);
            DA.GetDataList(8, autofits);
            DA.GetDataList(9, links);
            DA.GetDataList(10, units);

            var fillJsons = PluginUtilities.ToColorJsons(fills);
            var outlineJsons = PluginUtilities.ToColorJsons(outlineColors);

            int n = PluginUtilities.RecordCount(objectIds, fillJsons, transparencies, outlineJsons, outlineWeights, dashes, shadows, contentAlignments, autofits, links, units);
            PluginUtilities.WarnIfRagged(this, n,
                ("Object ID", objectIds.Count), ("Fill Color", fillJsons.Count), ("Fill Transparency", transparencies.Count),
                ("Outline Color", outlineJsons.Count), ("Outline Weight", outlineWeights.Count), ("Outline Dash", dashes.Count),
                ("Shadow", shadows.Count), ("Content Alignment", contentAlignments.Count), ("Autofit", autofits.Count),
                ("Link URL", links.Count), ("Unit", units.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            fillJsons = PluginUtilities.MatchLength(fillJsons, n, "");
            var transparenciesN = PluginUtilities.MatchLengthOrNull(transparencies, n);
            outlineJsons = PluginUtilities.MatchLength(outlineJsons, n, "");
            outlineWeights = PluginUtilities.MatchLength(outlineWeights, n, 0);
            dashes = PluginUtilities.MatchLength(dashes, n, "");
            var shadowsN = PluginUtilities.MatchLengthOrNull(shadows, n);
            contentAlignments = PluginUtilities.MatchLength(contentAlignments, n, "");
            autofits = PluginUtilities.MatchLength(autofits, n, "");
            links = PluginUtilities.MatchLength(links, n, "");
            units = PluginUtilities.MatchLength(units, n, "pt");

            var requests = new List<string>();
            int skipped = 0;
            for (int i = 0; i < n; i++)
            {
                var spec = new StyleBuilders.ShapeStyleSpec
                {
                    FillColorJson = fillJsons[i],
                    FillAlpha = transparenciesN[i].HasValue ? 1 - transparenciesN[i].Value : (double?)null,
                    OutlineColorJson = outlineJsons[i],
                    OutlineWeightEmu = outlineWeights[i] > 0 ? GeometryBuilders.ToEmu(outlineWeights[i], units[i]) : (double?)null,
                    OutlineDash = dashes[i],
                    Shadow = shadowsN[i],
                    ContentAlignment = contentAlignments[i],
                    Autofit = autofits[i],
                    LinkUrl = links[i]
                };
                string request = StyleBuilders.BuildUpdateShapeProperties(objectIds[i], spec);
                if (request == null) { skipped++; continue; }
                requests.Add(request);
            }
            if (skipped > 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, skipped + " record(s) had no style values set — nothing to update, skipped.");

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_UpdateShapeProperties;
        public override Guid ComponentGuid => new Guid("28818b85-77fb-44fe-9a3b-0957566ce29c");
    }
}
