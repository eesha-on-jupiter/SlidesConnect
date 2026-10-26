using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Vertical text alignment and autofit for existing shapes — the single-purpose slice of Update
    // Shape Style.
    public class ShapeTextFitComponent : GH_Component
    {
        public ShapeTextFitComponent()
          : base("Shape Text Fit", "TextFit",
              "Sets how text sits in one or more existing shapes: vertical alignment (TOP/MIDDLE/BOTTOM) and autofit (NONE/TEXT_AUTOFIT/SHAPE_AUTOFIT). Only the inputs you supply are applied.",
              PluginUtilities.TabName, PluginUtilities.CategoryAEShapes)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Content Alignment", "CA", "Vertical alignment of the text in the shape: TOP, MIDDLE, BOTTOM (blank to skip) — see the Content Alignment preset.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Autofit", "AF", "NONE, TEXT_AUTOFIT (shrink text to fit the shape) or SHAPE_AUTOFIT (grow the shape to fit the text). Blank to skip.", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateShapeProperties batchUpdate request JSON, one per shape.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var alignments = new List<string>();
            var autofits = new List<string>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, alignments);
            DA.GetDataList(2, autofits);

            int n = PluginUtilities.RecordCount(objectIds, alignments, autofits);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Content Alignment", alignments.Count), ("Autofit", autofits.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            alignments = PluginUtilities.MatchLength(alignments, n, "");
            autofits = PluginUtilities.MatchLength(autofits, n, "");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string request = StyleBuilders.BuildUpdateShapeProperties(objectIds[i], new StyleBuilders.ShapeStyleSpec { ContentAlignment = alignments[i], Autofit = autofits[i] });
                if (request != null) requests.Add(request);
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Supply a Content Alignment and/or Autofit."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_ShapeTextFit;
        public override Guid ComponentGuid => new Guid("340f8ce5-55e5-4ed3-a053-2b24f6ad771d");
    }
}
