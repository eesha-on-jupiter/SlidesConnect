using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds updatePageElementTransform requests: positions, scales and rotates one or more elements.
    // Absolute mode replaces the element's whole matrix, so Scale 1 resizes any element whose current
    // scale isn't 1 (most layout placeholders are stored as base size x scale). Relative mode is a
    // pure move-by-delta that leaves size and rotation exactly as they are.
    public class UpdateShapeTransformComponent : GH_Component
    {
        public UpdateShapeTransformComponent()
          : base("Update Shape Transform", "ShapeTransform",
              "Build requests that set one or more elements' position, scale, rotation, flip and skew (absolute, about the centre when Width/Height are given), or move them by a delta (Relative).",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the element to transform.", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("X", "X", "Left position, in Unit.", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Y", "Y", "Top position, in Unit.", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Scale X", "SX", "Horizontal scale factor.", GH_ParamAccess.list, 1);
            pManager.AddNumberParameter("Scale Y", "SY", "Vertical scale factor.", GH_ParamAccess.list, 1);
            pManager.AddNumberParameter("Rotation", "RO", "Rotation in degrees (clockwise).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Unit", "U", "Unit for X/Y (in, pt, mm, cm, m, ft, emu).", GH_ParamAccess.list, "in");
            pManager.AddBooleanParameter("Flip X", "FX", "Absolute mode: mirror left-right.", GH_ParamAccess.list, false);
            pManager.AddBooleanParameter("Flip Y", "FY", "Absolute mode: mirror top-bottom.", GH_ParamAccess.list, false);
            pManager.AddNumberParameter("Skew X", "KX", "Absolute mode: lean the top edge sideways, in degrees.", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Skew Y", "KY", "Absolute mode: lean the left edge, in degrees.", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Width", "W", "Absolute mode, optional: the element's base width, in Unit. When Width and Height are given the rotation/flip/skew pivot about the element's centre (like Create Shape); when 0 they pivot about its top-left corner, the API default.", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Height", "H", "Absolute mode, optional: the element's base height, in Unit (see Width).", GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("Relative", "REL", "False = absolute: X/Y/Scale/Rotation replace the element's transform (Scale must match the element's current scale or it resizes — check Get Slide's JSON). True = move by X/Y as a delta; size and rotation are left untouched, Scale/Rotation ignored.", GH_ParamAccess.list, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per element.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var xs = new List<double>();
            var ys = new List<double>();
            var scaleXs = new List<double>();
            var scaleYs = new List<double>();
            var rotations = new List<double>();
            var units = new List<string>();
            var relatives = new List<bool>();
            var flipXs = new List<bool>();
            var flipYs = new List<bool>();
            var skewXs = new List<double>();
            var skewYs = new List<double>();
            var widths = new List<double>();
            var heights = new List<double>();

            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, xs);
            DA.GetDataList(2, ys);
            DA.GetDataList(3, scaleXs);
            DA.GetDataList(4, scaleYs);
            DA.GetDataList(5, rotations);
            DA.GetDataList(6, units);
            DA.GetDataList(7, flipXs);
            DA.GetDataList(8, flipYs);
            DA.GetDataList(9, skewXs);
            DA.GetDataList(10, skewYs);
            DA.GetDataList(11, widths);
            DA.GetDataList(12, heights);
            DA.GetDataList(13, relatives);

            int n = PluginUtilities.RecordCount(objectIds, xs, ys, scaleXs, scaleYs, rotations, units, flipXs, flipYs, skewXs, skewYs, widths, heights, relatives);
            PluginUtilities.WarnIfRagged(this, n,
                ("Object ID", objectIds.Count), ("X", xs.Count), ("Y", ys.Count),
                ("Scale X", scaleXs.Count), ("Scale Y", scaleYs.Count), ("Rotation", rotations.Count), ("Unit", units.Count),
                ("Flip X", flipXs.Count), ("Flip Y", flipYs.Count), ("Skew X", skewXs.Count), ("Skew Y", skewYs.Count),
                ("Width", widths.Count), ("Height", heights.Count), ("Relative", relatives.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            xs = PluginUtilities.MatchLength(xs, n, 0);
            ys = PluginUtilities.MatchLength(ys, n, 0);
            scaleXs = PluginUtilities.MatchLength(scaleXs, n, 1);
            scaleYs = PluginUtilities.MatchLength(scaleYs, n, 1);
            rotations = PluginUtilities.MatchLength(rotations, n, 0);
            units = PluginUtilities.MatchLength(units, n, "in");
            relatives = PluginUtilities.MatchLength(relatives, n, false);
            flipXs = PluginUtilities.MatchLength(flipXs, n, false);
            flipYs = PluginUtilities.MatchLength(flipYs, n, false);
            skewXs = PluginUtilities.MatchLength(skewXs, n, 0);
            skewYs = PluginUtilities.MatchLength(skewYs, n, 0);
            widths = PluginUtilities.MatchLength(widths, n, 0);
            heights = PluginUtilities.MatchLength(heights, n, 0);

            var requests = new List<string>();
            bool ignoredScaleOrRotation = false;
            for (int i = 0; i < n; i++)
            {
                double x = GeometryBuilders.ToEmu(xs[i], units[i]);
                double y = GeometryBuilders.ToEmu(ys[i], units[i]);
                if (relatives[i] && (scaleXs[i] != 1 || scaleYs[i] != 1 || rotations[i] != 0 || flipXs[i] || flipYs[i] || skewXs[i] != 0 || skewYs[i] != 0)) ignoredScaleOrRotation = true;
                var placement = new RequestBuilders.Placement
                {
                    ScaleX = scaleXs[i], ScaleY = scaleYs[i], RotationDegrees = rotations[i],
                    FlipX = flipXs[i], FlipY = flipYs[i], SkewXDegrees = skewXs[i], SkewYDegrees = skewYs[i]
                };
                double w = widths[i] > 0 ? GeometryBuilders.ToEmu(widths[i], units[i]) : 0;
                double h = heights[i] > 0 ? GeometryBuilders.ToEmu(heights[i], units[i]) : 0;
                requests.Add(RequestBuilders.BuildUpdateTransform(objectIds[i], x, y, placement, w, h, relatives[i]));
            }
            if (ignoredScaleOrRotation)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Relative mode moves only — Scale, Rotation, Flip and Skew were ignored for the relative record(s). Use absolute mode for those.");

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_UpdateShapeTransform;
        public override Guid ComponentGuid => new Guid("0871517d-3ff8-4219-9ce7-17416c1b3299");
    }
}
