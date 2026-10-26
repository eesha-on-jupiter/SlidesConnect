using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // AEC: turns a Grasshopper data tree into a full table — createTable plus one insertText per cell.
    // One branch per row, items in a branch are that row's cell values. Outputs a Request JSON list.
    public class DataTreeToTableComponent : GH_Component
    {
        public DataTreeToTableComponent()
          : base("Data Tree To Table", "Tree2Table",
              "Build createTable + per-cell insertText requests from a data tree (one branch per row).",
              PluginUtilities.TabName, PluginUtilities.CategoryAHTables)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID to place the table on.", GH_ParamAccess.item, "");
            pManager.AddTextParameter("Data", "DA", "Table data — one branch per row, items per branch are the row's cells.", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON sequence (createTable + cell inserts).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pageId = "";
            if (!DA.GetData(0, ref pageId)) return;

            if (!DA.GetDataTree(1, out GH_Structure<GH_String> tree) || tree.IsEmpty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Data tree is empty.");
                return;
            }

            var rows = new List<IList<string>>();
            foreach (var branch in tree.Branches)
            {
                var row = new List<string>();
                foreach (var cell in branch)
                    row.Add(cell?.Value ?? "");
                rows.Add(row);
            }

            var requests = GeometryBuilders.BuildTableRequests(pageId, rows);
            if (requests.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No table cells produced.");
                return;
            }

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_DataTreeToTable;
        public override Guid ComponentGuid => new Guid("a9856621-74e5-4844-b448-c8c83e80fd2c");
    }
}
