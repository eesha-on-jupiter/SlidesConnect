using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds createShape requests: adds one or more shapes of a given type, size and position to
    // a slide. Slide ID repeated = many shapes on one slide; varied = spread across slides.
    // Optional Color / Outline Weight bundle an updateShapeProperties request right after
    // each shape's createShape request — the Slides API has no color field on createShape itself.
    public class CreateShapeComponent : GH_Component
    {
        public CreateShapeComponent()
          : base("Create Shape", "Shape",
              "Build requests that add one or more shapes at a given size and position, optionally styled.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID(s) to place the shape on (from Create Slide's Slide ID output, or an existing slide).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Shape Type", "ST", "Shape type (e.g. TEXT_BOX, RECTANGLE, ELLIPSE).", GH_ParamAccess.list, "TEXT_BOX");
            pManager.AddNumberParameter("Width", "W", "Width, in Unit.", GH_ParamAccess.list, 3.0);
            pManager.AddNumberParameter("Height", "H", "Height, in Unit.", GH_ParamAccess.list, 1.0);
            pManager.AddNumberParameter("X", "X", "Left position, in Unit.", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Y", "Y", "Top position, in Unit.", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Unit", "U", "Unit for Width/Height/X/Y/Outline Weight (in, pt, mm, cm, m, ft, emu). Page is 10 x 5.625 in.", GH_ParamAccess.list, "in");
            pManager.AddTextParameter("Object ID", "OID", "Optional: your own ID(s) for the new shape, so later requests can target it by a name you chose. Leave blank to auto-generate; the resolved ID comes out of the Object ID output either way.", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Color", "C", "Optional fill colour(s): a Colour Swatch or a panel like 255,0,0. Unconnected = keep the shape's default fill.", GH_ParamAccess.list);
            pManager[8].Optional = true;
            pManager.AddNumberParameter("Outline Weight", "OW", "Optional outline weight, in Unit (0 = no outline set).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Rotation", "RO", "Rotation in degrees, clockwise, about the shape's centre (0 = none). Rhino Shape Export's Rotation wires straight in.", GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("Flip X", "FX", "Mirror left-right (about the centre).", GH_ParamAccess.list, false);
            pManager.AddBooleanParameter("Flip Y", "FY", "Mirror top-bottom (about the centre).", GH_ParamAccess.list, false);
            pManager.AddNumberParameter("Skew X", "KX", "Lean the top edge sideways, in degrees (italic-style shear; positive = right).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Skew Y", "KY", "Lean the left edge, in degrees (positive = down).", GH_ParamAccess.list, 0);
            pManager[7].Optional = true;
            pManager[8].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON — createShape, plus updateShapeProperties per shape where Color/Outline Weight is supplied.", GH_ParamAccess.list);
            pManager.AddTextParameter("Object ID", "OID", "Resolved object ID of each new shape (supplied or auto-generated).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pageIds = new List<string>();
            var shapeTypes = new List<string>();
            var widths = new List<double>();
            var heights = new List<double>();
            var xs = new List<double>();
            var ys = new List<double>();
            var units = new List<string>();
            var objectIds = new List<string>();
            var colors = new List<System.Drawing.Color>();
            var outlineWeights = new List<double>();
            var rotations = new List<double>();
            var flipXs = new List<bool>();
            var flipYs = new List<bool>();
            var skewXs = new List<double>();
            var skewYs = new List<double>();

            DA.GetDataList(0, pageIds);
            DA.GetDataList(1, shapeTypes);
            DA.GetDataList(2, widths);
            DA.GetDataList(3, heights);
            DA.GetDataList(4, xs);
            DA.GetDataList(5, ys);
            DA.GetDataList(6, units);
            DA.GetDataList(7, objectIds);
            DA.GetDataList(8, colors);
            DA.GetDataList(9, outlineWeights);
            DA.GetDataList(10, rotations);
            DA.GetDataList(11, flipXs);
            DA.GetDataList(12, flipYs);
            DA.GetDataList(13, skewXs);
            DA.GetDataList(14, skewYs);
            var colorJsons = PluginUtilities.ToColorJsons(colors);

            int n = PluginUtilities.RecordCount(pageIds, shapeTypes, widths, heights, xs, ys, units, objectIds, colorJsons, outlineWeights, rotations, flipXs, flipYs, skewXs, skewYs);
            PluginUtilities.WarnIfRagged(this, n,
                ("Slide ID", pageIds.Count), ("Shape Type", shapeTypes.Count), ("Width", widths.Count),
                ("Height", heights.Count), ("X", xs.Count), ("Y", ys.Count), ("Unit", units.Count),
                ("Object ID", objectIds.Count), ("Color", colorJsons.Count), ("Outline Weight", outlineWeights.Count), ("Rotation", rotations.Count), ("Flip X", flipXs.Count), ("Flip Y", flipYs.Count), ("Skew X", skewXs.Count), ("Skew Y", skewYs.Count));

            pageIds = PluginUtilities.MatchLength(pageIds, n, "");
            shapeTypes = PluginUtilities.MatchLength(shapeTypes, n, "TEXT_BOX");
            widths = PluginUtilities.MatchLength(widths, n, 3.0);
            heights = PluginUtilities.MatchLength(heights, n, 1.0);
            xs = PluginUtilities.MatchLength(xs, n, 0);
            ys = PluginUtilities.MatchLength(ys, n, 0);
            units = PluginUtilities.MatchLength(units, n, "in");
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            colorJsons = PluginUtilities.MatchLength(colorJsons, n, "");
            outlineWeights = PluginUtilities.MatchLength(outlineWeights, n, 0);
            rotations = PluginUtilities.MatchLength(rotations, n, 0);
            flipXs = PluginUtilities.MatchLength(flipXs, n, false);
            flipYs = PluginUtilities.MatchLength(flipYs, n, false);
            skewXs = PluginUtilities.MatchLength(skewXs, n, 0);
            skewYs = PluginUtilities.MatchLength(skewYs, n, 0);

            var requests = new List<string>();
            var resolvedIds = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string shapeId = RequestBuilders.EnsureId(objectIds[i], "shape");
                resolvedIds.Add(shapeId);

                double w = GeometryBuilders.ToEmu(widths[i], units[i]);
                double h = GeometryBuilders.ToEmu(heights[i], units[i]);
                double x = GeometryBuilders.ToEmu(xs[i], units[i]);
                double y = GeometryBuilders.ToEmu(ys[i], units[i]);
                requests.Add(RequestBuilders.BuildCreateShape(pageIds[i], shapeTypes[i], w, h, x, y, shapeId,
                    new RequestBuilders.Placement { RotationDegrees = rotations[i], FlipX = flipXs[i], FlipY = flipYs[i], SkewXDegrees = skewXs[i], SkewYDegrees = skewYs[i] }));

                double? outlineEmu = outlineWeights[i] > 0 ? GeometryBuilders.ToEmu(outlineWeights[i], units[i]) : (double?)null;
                string style = RequestBuilders.BuildUpdateShapeProperties(shapeId, colorJsons[i], outlineEmu);
                if (style != null) requests.Add(style);
            }

            DA.SetDataList(0, requests);
            DA.SetDataList(1, resolvedIds);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_CreateShape;
        public override Guid ComponentGuid => new Guid("b5d73488-159f-4836-98f3-15493ab33423");
    }
}
