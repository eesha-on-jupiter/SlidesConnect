using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds insertText requests: places text into one or more shapes or table cells.
    public class InsertTextComponent : GH_Component
    {
        public InsertTextComponent()
          : base("Insert Text", "InsText",
              "Build requests that insert text into one or more shapes or placeholders.",
              PluginUtilities.TabName, PluginUtilities.CategoryAFText)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape to insert text into.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Text", "TX", "Text to insert.", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per insertion.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var texts = new List<string>();

            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, texts);

            int n = PluginUtilities.RecordCount(objectIds, texts);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Text", texts.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            texts = PluginUtilities.MatchLength(texts, n, "");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
                requests.Add(RequestBuilders.BuildInsertText(objectIds[i], texts[i], 0));

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_InsertText;
        public override Guid ComponentGuid => new Guid("76d894ae-5b36-4208-a423-f7e7a9e67716");
    }
}
