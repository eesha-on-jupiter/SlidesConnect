using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // mergeTableCells / unmergeTableCells over a rectangular range. Text in merged cells is kept
    // only from the top-left cell — insert the merged cell's text after merging.
    public class MergeCellsComponent : GH_Component
    {
        public MergeCellsComponent()
          : base("Merge Cells", "Merge",
              "Merges a rectangular range of cells in an existing table (e.g. a header spanning every column), or unmerges it with Unmerge = true. Merging keeps only the top-left cell's text.",
              PluginUtilities.TabName, PluginUtilities.CategoryAHTables)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the table.", GH_ParamAccess.list, "");
            pManager.AddIntegerParameter("Row", "R", "First row of the range (0-based).", GH_ParamAccess.list, 0);
            pManager.AddIntegerParameter("Column", "CL", "First column of the range (0-based).", GH_ParamAccess.list, 0);
            pManager.AddIntegerParameter("Row Span", "RS", "Number of rows to merge.", GH_ParamAccess.list, 1);
            pManager.AddIntegerParameter("Column Span", "CS", "Number of columns to merge.", GH_ParamAccess.list, 2);
            pManager.AddBooleanParameter("Unmerge", "UM", "True = split the range's merged cells back apart instead.", GH_ParamAccess.list, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "mergeTableCells / unmergeTableCells batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var rows = new List<int>();
            var cols = new List<int>();
            var rowSpans = new List<int>();
            var colSpans = new List<int>();
            var unmerges = new List<bool>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, rows);
            DA.GetDataList(2, cols);
            DA.GetDataList(3, rowSpans);
            DA.GetDataList(4, colSpans);
            DA.GetDataList(5, unmerges);

            int n = PluginUtilities.RecordCount(objectIds, rows, cols, rowSpans, colSpans, unmerges);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Row", rows.Count), ("Column", cols.Count),
                ("Row Span", rowSpans.Count), ("Column Span", colSpans.Count), ("Unmerge", unmerges.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            rows = PluginUtilities.MatchLength(rows, n, 0);
            cols = PluginUtilities.MatchLength(cols, n, 0);
            rowSpans = PluginUtilities.MatchLength(rowSpans, n, 1);
            colSpans = PluginUtilities.MatchLength(colSpans, n, 2);
            unmerges = PluginUtilities.MatchLength(unmerges, n, false);

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(objectIds[i])) continue;
                requests.Add(unmerges[i]
                    ? StyleBuilders.BuildUnmergeTableCells(objectIds[i], rows[i], cols[i], rowSpans[i], colSpans[i])
                    : StyleBuilders.BuildMergeTableCells(objectIds[i], rows[i], cols[i], rowSpans[i], colSpans[i]));
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Object ID supplied."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_MergeCells;
        public override Guid ComponentGuid => new Guid("81575627-b442-4775-9396-1fd54fadbebf");
    }
}
