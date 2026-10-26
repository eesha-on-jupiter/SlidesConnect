using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SlidesConnect
{
    // Resolves images to the public URLs that Create Image / Replace Image / Build Slides From Layout
    // need. Slides fetches image URLs unauthenticated, so a local Bitmap or File Path (e.g. from
    // Capture Rhino View / Capture Camera View) has no way onto a slide without being hosted first —
    // this uploads each one to the user's Google Drive and makes it link-public via
    // SlidesConnectClient.UploadImageAndGetUrlAsync (drive.file scope). Already-public URLs pass
    // through untouched. List access throughout so a list of captures becomes a list of URLs, one
    // per slide. Priority per record: URL > Bitmap > File Path; the three lists are zipped by index.
    public class GetImageUrlComponent : ButtonComponent
    {
        public GetImageUrlComponent()
          : base("Get Image URL", "ImgURL",
              "Resolves images to public URLs for Create Image / Build Slides From Layout. URLs pass through; Bitmaps and local File Paths are uploaded to Google Drive and made link-public (needs drive.file scope).",
              PluginUtilities.TabName, PluginUtilities.CategoryAIInsert)
        { }

        public override string ButtonLabel => "Get URLs";

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Token", "T", "Google OAuth 2.0 access token with drive.file scope (only needed when uploading a Bitmap or File Path; unused for URL passthrough).", GH_ParamAccess.item, "");
            pManager.AddGenericParameter("Bitmap", "BM", "In-memory image(s) to host. Ignored for any record that also has a URL.", GH_ParamAccess.list);
            pManager.AddTextParameter("File Path", "FP", "Local image path(s) to host (e.g. from Capture Rhino View / Capture Camera View). Ignored for any record that also has a URL or Bitmap.", GH_ParamAccess.list);
            pManager.AddTextParameter("URL", "U", "Already-public image URL(s) — passed through unchanged with no upload.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Image URL", "IU", "Publicly fetchable URL per record — wire into Create Image's Image URL, Replace Image's Image URL, or Build Slides From Layout's Photo URL.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsTriggered) return;

            string token = "";
            var bitmapObjs = new List<object>();
            var filePaths = new List<string>();
            var urls = new List<string>();
            DA.GetData(0, ref token);
            DA.GetDataList(1, bitmapObjs);
            DA.GetDataList(2, filePaths);
            DA.GetDataList(3, urls);

            // records are zipped by index across the three sources; a source shorter than the longest
            // simply contributes nothing for the missing indices (unlike request builders, a blank
            // here is not padding — it just means "try the next source")
            int n = Math.Max(bitmapObjs.Count, Math.Max(filePaths.Count, urls.Count));
            if (n == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Supply a Bitmap, File Path, or URL.");
                return;
            }

            var client = new SlidesConnectClient(token);
            var results = new List<string>();
            int uploaded = 0, failed = 0;

            for (int i = 0; i < n; i++)
            {
                string url = i < urls.Count ? urls[i] : "";
                if (!string.IsNullOrWhiteSpace(url))
                {
                    results.Add(url.Trim());
                    continue;
                }

                byte[] imageBytes = null;
                string fileName = "";

                object bitmapObj = i < bitmapObjs.Count ? bitmapObjs[i] : null;
                var bitmap = bitmapObj as Bitmap ?? (bitmapObj as GH_ObjectWrapper)?.Value as Bitmap;
                string filePath = i < filePaths.Count ? filePaths[i] : "";

                if (bitmap != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        imageBytes = ms.ToArray();
                    }
                    fileName = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}_{i}.png";
                }
                else if (!string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = filePath.Trim();
                    if (!File.Exists(filePath))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"[{i}] File not found: {filePath}");
                        results.Add(""); failed++;
                        continue;
                    }
                    try { imageBytes = File.ReadAllBytes(filePath); }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"[{i}] Could not read file: {ex.Message}");
                        results.Add(""); failed++;
                        continue;
                    }
                    fileName = Path.GetFileName(filePath);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"[{i}] No URL, Bitmap, or File Path for this record.");
                    results.Add("");
                    continue;
                }

                var result = client.UploadImageAndGetUrlAsync(imageBytes, fileName, MimeTypeFor(fileName)).GetAwaiter().GetResult();
                if (result.Item1) { results.Add(result.Item2); uploaded++; }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"[{i}] {result.Item3}");
                    results.Add(""); failed++;
                }
            }

            if (uploaded > 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"{uploaded} image(s) uploaded to Drive and made link-public.");

            DA.SetDataList(0, results);
        }

        private static string MimeTypeFor(string fileName)
        {
            switch (Path.GetExtension(fileName).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                default: return "image/png";
            }
        }

        protected override Bitmap Icon => Properties.Resources.GS_GetImageUrl;
        public override Guid ComponentGuid => new Guid("4f1c7a2e-9b3d-4e6f-8a15-2c7d9e0b3f61");
    }
}
