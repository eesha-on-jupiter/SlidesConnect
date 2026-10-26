using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds createImage requests: embeds one or more images by public URL at a given size and position.
    public class CreateImageComponent : GH_Component
    {
        public CreateImageComponent()
          : base("Create Image", "Image",
              "Build requests that embed one or more images by URL at a given size and position.",
              PluginUtilities.TabName, PluginUtilities.CategoryAIInsert)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID(s) to place the image on (from Create Slide's Slide ID output, or an existing slide).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Image URL", "URL", "Public image URL(s) (PNG/JPG/GIF, <= 50 MB / 25 MP).", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Width", "W", "Width, in Unit.", GH_ParamAccess.list, 3.0);
            pManager.AddNumberParameter("Height", "H", "Height, in Unit.", GH_ParamAccess.list, 3.0);
            pManager.AddNumberParameter("X", "X", "Left position, in Unit.", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Y", "Y", "Top position, in Unit.", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Unit", "U", "Unit for Width/Height/X/Y (in, pt, mm, cm, m, ft, emu). Page is 10 x 5.625 in.", GH_ParamAccess.list, "in");
            pManager.AddTextParameter("Object ID", "OID", "Optional: your own ID(s) for the new image. Leave blank to auto-generate; the resolved ID comes out of the Object ID output either way.", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Rotation", "RO", "Rotation in degrees, clockwise, about the image's centre (0 = none).", GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("Flip X", "FX", "Mirror left-right (about the centre).", GH_ParamAccess.list, false);
            pManager.AddBooleanParameter("Flip Y", "FY", "Mirror top-bottom (about the centre).", GH_ParamAccess.list, false);
            pManager.AddNumberParameter("Skew X", "KX", "Lean the top edge sideways, in degrees (italic-style shear; positive = right).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Skew Y", "KY", "Lean the left edge, in degrees (positive = down).", GH_ParamAccess.list, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per image.", GH_ParamAccess.list);
            pManager.AddTextParameter("Object ID", "OID", "Resolved object ID of each new image (supplied or auto-generated).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pageIds = new List<string>();
            var imageUrls = new List<string>();
            var widths = new List<double>();
            var heights = new List<double>();
            var xs = new List<double>();
            var ys = new List<double>();
            var units = new List<string>();
            var objectIds = new List<string>();
            var rotations = new List<double>();
            var flipXs = new List<bool>();
            var flipYs = new List<bool>();
            var skewXs = new List<double>();
            var skewYs = new List<double>();

            DA.GetDataList(0, pageIds);
            DA.GetDataList(1, imageUrls);
            DA.GetDataList(2, widths);
            DA.GetDataList(3, heights);
            DA.GetDataList(4, xs);
            DA.GetDataList(5, ys);
            DA.GetDataList(6, units);
            DA.GetDataList(7, objectIds);
            DA.GetDataList(8, rotations);
            DA.GetDataList(9, flipXs);
            DA.GetDataList(10, flipYs);
            DA.GetDataList(11, skewXs);
            DA.GetDataList(12, skewYs);

            int n = PluginUtilities.RecordCount(pageIds, imageUrls, widths, heights, xs, ys, units, objectIds, rotations, flipXs, flipYs, skewXs, skewYs);
            PluginUtilities.WarnIfRagged(this, n,
                ("Slide ID", pageIds.Count), ("Image URL", imageUrls.Count), ("Width", widths.Count),
                ("Height", heights.Count), ("X", xs.Count), ("Y", ys.Count), ("Unit", units.Count), ("Object ID", objectIds.Count), ("Rotation", rotations.Count), ("Flip X", flipXs.Count), ("Flip Y", flipYs.Count), ("Skew X", skewXs.Count), ("Skew Y", skewYs.Count));

            pageIds = PluginUtilities.MatchLength(pageIds, n, "");
            imageUrls = PluginUtilities.MatchLength(imageUrls, n, "");
            widths = PluginUtilities.MatchLength(widths, n, 3.0);
            heights = PluginUtilities.MatchLength(heights, n, 3.0);
            xs = PluginUtilities.MatchLength(xs, n, 0);
            ys = PluginUtilities.MatchLength(ys, n, 0);
            units = PluginUtilities.MatchLength(units, n, "in");
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            rotations = PluginUtilities.MatchLength(rotations, n, 0);
            flipXs = PluginUtilities.MatchLength(flipXs, n, false);
            flipYs = PluginUtilities.MatchLength(flipYs, n, false);
            skewXs = PluginUtilities.MatchLength(skewXs, n, 0);
            skewYs = PluginUtilities.MatchLength(skewYs, n, 0);

            var requests = new List<string>();
            var resolvedIds = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string imageId = RequestBuilders.EnsureId(objectIds[i], "image");
                resolvedIds.Add(imageId);

                double w = GeometryBuilders.ToEmu(widths[i], units[i]);
                double h = GeometryBuilders.ToEmu(heights[i], units[i]);
                double x = GeometryBuilders.ToEmu(xs[i], units[i]);
                double y = GeometryBuilders.ToEmu(ys[i], units[i]);
                requests.Add(RequestBuilders.BuildCreateImage(pageIds[i], imageUrls[i], w, h, x, y, imageId,
                    new RequestBuilders.Placement { RotationDegrees = rotations[i], FlipX = flipXs[i], FlipY = flipYs[i], SkewXDegrees = skewXs[i], SkewYDegrees = skewYs[i] }));
            }

            DA.SetDataList(0, requests);
            DA.SetDataList(1, resolvedIds);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_CreateImage;
        public override Guid ComponentGuid => new Guid("e6e40c88-f0a6-4ba7-857b-da247cdcf9a1");
    }
}
