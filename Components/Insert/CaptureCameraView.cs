using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // AEC: repositions the Rhino viewport camera from a Plane (origin = camera location, Z axis =
    // view direction, Y axis = up) and captures a PNG per plane — e.g. one per floor, "sliding" the
    // camera up the building. Unlike Capture Rhino View (which only switches between pre-named,
    // already-framed viewports), this one moves the camera itself before each shot. Does not upload
    // anywhere — host the files yourself and feed the resulting URLs into Create Image / Build
    // Slides From Layout.
    public class CaptureCameraViewComponent : ButtonComponent
    {
        public CaptureCameraViewComponent()
          : base("Capture Camera View", "CaptureCam",
              "Move the Rhino camera to one or more Planes and capture a PNG at each — e.g. one per floor.",
              PluginUtilities.TabName, PluginUtilities.CategoryAIInsert)
        { }

        public override string ButtonLabel => "Capture";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Camera Plane", "CP", "Camera plane(s) — origin = camera location, Z axis = view direction, Y axis = up.", GH_ParamAccess.list);
            pManager.AddTextParameter("Projection", "PR", "Perspective or Parallel.", GH_ParamAccess.list, "Perspective");
            pManager.AddTextParameter("View Name", "VN", "Named Rhino viewport to move and capture (blank = the currently active viewport).", GH_ParamAccess.item, "");
            pManager.AddTextParameter("File Path", "FP", "Where to save each PNG — a full file path, or a folder to save an auto-named file inside it (blank = a temp folder).", GH_ParamAccess.list, "");
            pManager.AddIntegerParameter("Width", "W", "Capture width in pixels (0 = current viewport size).", GH_ParamAccess.list, 0);
            pManager.AddIntegerParameter("Height", "H", "Capture height in pixels (0 = current viewport size).", GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("Restore View", "RV", "Put the viewport's camera back where it was after the last capture (true), or leave it on the last plane (false).", GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "FP", "Path of each saved PNG file.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered) return;

            var planes = new List<Plane>();
            var projections = new List<string>();
            string viewName = "";
            var filePaths = new List<string>();
            var widths = new List<int>();
            var heights = new List<int>();
            bool restoreView = true;

            if (!DA.GetDataList(0, planes)) return;
            DA.GetDataList(1, projections);
            DA.GetData(2, ref viewName);
            DA.GetDataList(3, filePaths);
            DA.GetDataList(4, widths);
            DA.GetDataList(5, heights);
            DA.GetData(6, ref restoreView);

            var doc = Rhino.RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active Rhino document.");
                return;
            }

            var view = string.IsNullOrWhiteSpace(viewName) ? doc.Views.ActiveView : doc.Views.Find(viewName, false);
            if (view == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.IsNullOrWhiteSpace(viewName)
                    ? "No active Rhino viewport found."
                    : $"No viewport named \"{viewName}\" found.");
                return;
            }

            int n = planes.Count;
            projections = PluginUtilities.MatchLength(projections, n, "Perspective");
            filePaths = PluginUtilities.MatchLength(filePaths, n, "");
            widths = PluginUtilities.MatchLength(widths, n, 0);
            heights = PluginUtilities.MatchLength(heights, n, 0);

            // Push/PopViewProjection save and restore the full ViewportInfo (camera + projection
            // type), so the user's framing survives the capture series. Available on both the Rhino 7
            // and Rhino 8 RhinoCommon builds referenced by this project.
            var vp = view.ActiveViewport;
            if (restoreView) vp.PushViewProjection();

            var results = new List<string>();
            try
            {
                for (int i = 0; i < n; i++)
                {
                    string savedPath = CaptureOne(doc, view, planes[i], projections[i], filePaths[i], widths[i], heights[i], i);
                    if (savedPath != null) results.Add(savedPath);
                }
            }
            finally
            {
                if (restoreView)
                {
                    vp.PopViewProjection();
                    doc.Views.Redraw();
                }
            }

            DA.SetDataList(0, results);
        }

        // moves the camera to one plane, captures, and saves one PNG; returns the saved path, or
        // null (with a runtime message already added) on failure — never aborts the rest of the batch
        private string CaptureOne(Rhino.RhinoDoc doc, Rhino.Display.RhinoView view, Plane cameraPlane, string projection, string filePath, int width, int height, int index)
        {
            if (!cameraPlane.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Index {index}: camera plane is invalid — skipped.");
                return null;
            }

            var vp = view.ActiveViewport;

            if (string.Equals(projection?.Trim(), "Parallel", StringComparison.OrdinalIgnoreCase))
                vp.ChangeToParallelProjection(true);
            else
                vp.ChangeToPerspectiveProjection(true, 50.0);

            vp.SetCameraDirection(cameraPlane.ZAxis, false);
            vp.SetCameraLocation(cameraPlane.Origin, false);
            vp.CameraUp = cameraPlane.YAxis;

            doc.Views.Redraw();

            var bitmap = (width > 0 && height > 0)
                ? view.CaptureToBitmap(new System.Drawing.Size(width, height))
                : view.CaptureToBitmap();

            if (bitmap == null || bitmap.Width <= 0 || bitmap.Height <= 0)
            {
                bitmap?.Dispose();
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Index {index}: viewport capture failed (empty or zero-size result).");
                return null;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    string folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SlidesConnect");
                    System.IO.Directory.CreateDirectory(folder);
                    filePath = System.IO.Path.Combine(folder, $"camera_{index}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                }
                else if (System.IO.Directory.Exists(filePath))
                {
                    filePath = System.IO.Path.Combine(filePath, $"camera_{index}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                }
                else
                {
                    string dir = System.IO.Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrWhiteSpace(dir)) System.IO.Directory.CreateDirectory(dir);
                }

                // see CaptureRhinoView.cs for why: re-drawing onto a clean 32bppArgb bitmap and
                // encoding to a MemoryStream first avoids GDI+ errors on some captured bitmaps.
                using (var clean = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (var g = System.Drawing.Graphics.FromImage(clean))
                        g.DrawImageUnscaled(bitmap, 0, 0);

                    using (var ms = new System.IO.MemoryStream())
                    {
                        clean.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        System.IO.File.WriteAllBytes(filePath, ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Index {index}: could not save image: {ex.Message}");
                return null;
            }
            finally
            {
                bitmap.Dispose();
            }

            return filePath;
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_CaptureCameraView;
        public override Guid ComponentGuid => new Guid("139edf55-bbf4-43e5-85e7-aff58f3ab72e");
    }
}
