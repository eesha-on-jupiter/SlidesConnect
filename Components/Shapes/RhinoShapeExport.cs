using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // AEC: converts one or more Rhino rectangles (+ unit) into Slides X/Y/Width/Height/Rotation in
    // that same unit, the bridge that drives CreateShape / CreateImage / UpdateTransform from real
    // geometry. Rhino's origin is the page's bottom-left and Y is flipped (Slides' origin is
    // top-left, Y down), so a Top-view layout lands on the slide the way it looks in Rhino. Outputs
    // are in the model unit — never raw EMU — so they wire straight into the Unit-taking inputs.
    public class RhinoShapeExportComponent : GH_Component
    {
        public RhinoShapeExportComponent()
          : base("Rhino Shape Export", "ShapeExp",
              "Convert one or more Rhino rectangles into Slides position, size and rotation (in the model unit, Y flipped to Slides' top-left origin).",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddRectangleParameter("Rectangle", "RC", "Rectangle(s) defining the element frame.", GH_ParamAccess.list);
            pManager.AddTextParameter("Unit", "U", "Model unit of the rectangle (in, mm, cm, m, ft, pt). Outputs are in this same unit.", GH_ParamAccess.list, "in");
            pManager.AddBooleanParameter("Fit To Page", "FP", "Scale and center the frame to fit the standard 10x5.625 in page.", GH_ParamAccess.list, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("X", "X", "Left position on the slide, in Unit.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Y", "Y", "Top position on the slide, in Unit (Y already flipped: Rhino origin = page bottom-left).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Width", "W", "Width, in Unit.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "H", "Height, in Unit.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rotation", "RO", "Rotation in degrees, clockwise (Slides convention; Rhino's counter-clockwise angle negated).", GH_ParamAccess.list);
            pManager.AddTextParameter("Unit", "U", "The unit the outputs are in — wire into Create Shape / Create Image / Update Shape Transform's Unit.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var rects = new List<Rectangle3d>();
            var units = new List<string>();
            var fits = new List<bool>();

            if (!DA.GetDataList(0, rects)) return;
            DA.GetDataList(1, units);
            DA.GetDataList(2, fits);

            int n = PluginUtilities.RecordCount(rects, units, fits);
            PluginUtilities.WarnIfRagged(this, n, ("Rectangle", rects.Count), ("Unit", units.Count), ("Fit To Page", fits.Count));

            rects = PluginUtilities.MatchLength(rects, n, Rectangle3d.Unset);
            units = PluginUtilities.MatchLength(units, n, "in");
            fits = PluginUtilities.MatchLength(fits, n, false);

            var xs = new List<double>();
            var ys = new List<double>();
            var ws = new List<double>();
            var hs = new List<double>();
            var rotations = new List<double>();
            var unitsOut = new List<string>();

            for (int i = 0; i < n; i++)
            {
                Rectangle3d rect = rects[i];
                if (!rect.IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rectangle at index " + i + " is invalid — skipped.");
                    continue;
                }

                // Corner(3) = origin + height along the rectangle's Y axis: the visually top-left
                // corner in a Y-up view, which is where a Slides element's transform origin sits.
                // Rhino rotates counter-clockwise, Slides clockwise (screen Y points down) — negate.
                Point3d topLeft = rect.Corner(3);
                double rotation = -Math.Atan2(rect.Plane.XAxis.Y, rect.Plane.XAxis.X) * 180.0 / Math.PI;
                double[] frame = GeometryBuilders.BuildFrame(topLeft.X, topLeft.Y, rect.Width, rect.Height, units[i], fits[i]);

                xs.Add(GeometryBuilders.FromEmu(frame[0], units[i]));
                ys.Add(GeometryBuilders.FromEmu(frame[1], units[i]));
                ws.Add(GeometryBuilders.FromEmu(frame[2], units[i]));
                hs.Add(GeometryBuilders.FromEmu(frame[3], units[i]));
                rotations.Add(rotation);
                unitsOut.Add(units[i]);
            }

            DA.SetDataList(0, xs);
            DA.SetDataList(1, ys);
            DA.SetDataList(2, ws);
            DA.SetDataList(3, hs);
            DA.SetDataList(4, rotations);
            DA.SetDataList(5, unitsOut);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_RhinoShapeExport;
        public override Guid ComponentGuid => new Guid("b4609c97-de5a-48cf-b707-d761f6214184");
    }
}
