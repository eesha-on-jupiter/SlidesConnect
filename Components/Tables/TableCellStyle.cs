using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Cell fill and vertical content alignment over a range of an existing table (or the whole
    // table). For the cell *text* (bold, size, colour, alignment) use Styled Text / Styled Paragraph
    // with their Row/Column inputs.
    public class TableCellStyleComponent : GH_Component
    {
        public TableCellStyleComponent()
          : base("Table Cell Style", "CellStyle",
              "Sets the fill colour and/or vertical content alignment of a range of cells in an existing table (Row/Column -1 = whole table). For the cell text itself use Styled Text / Styled Paragraph with Row/Column.",
              PluginUtilities.TabName, PluginUtilities.CategoryAHTables)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the table (from Create Table / Data Tree To Table).", GH_ParamAccess.list, "");
            pManager.AddIntegerParameter("Row", "R", "First row of the range (0-based). -1 = the whole table.", GH_ParamAccess.list, -1);
            pManager.AddIntegerParameter("Column", "CL", "First column of the range (0-based). -1 = the whole table.", GH_ParamAccess.list, -1);
            pManager.AddIntegerParameter("Row Span", "RS", "Number of rows in the range.", GH_ParamAccess.list, 1);
            pManager.AddIntegerParameter("Column Span", "CS", "Number of columns in the range.", GH_ParamAccess.list, 1);
            pManager.AddColourParameter("Fill Color", "C", "Cell fill colour (connect to apply).", GH_ParamAccess.list);
            pManager.AddTextParameter("Content Alignment", "CA", "Vertical alignment of the cell text: TOP, MIDDLE, BOTTOM (blank to skip).", GH_ParamAccess.list, "");
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateTableCellProperties batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var rows = new List<int>();
            var cols = new List<int>();
            var rowSpans = new List<int>();
            var colSpans = new List<int>();
            var colors = new List<System.Drawing.Color>();
            var alignments = new List<string>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, rows);
            DA.GetDataList(2, cols);
            DA.GetDataList(3, rowSpans);
            DA.GetDataList(4, colSpans);
            DA.GetDataList(5, colors);
            DA.GetDataList(6, alignments);
            var colorJsons = PluginUtilities.ToColorJsons(colors);

            int n = PluginUtilities.RecordCount(objectIds, rows, cols, rowSpans, colSpans, colorJsons, alignments);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Row", rows.Count), ("Column", cols.Count),
                ("Row Span", rowSpans.Count), ("Column Span", colSpans.Count), ("Fill Color", colorJsons.Count), ("Content Alignment", alignments.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            rows = PluginUtilities.MatchLength(rows, n, -1);
            cols = PluginUtilities.MatchLength(cols, n, -1);
            rowSpans = PluginUtilities.MatchLength(rowSpans, n, 1);
            colSpans = PluginUtilities.MatchLength(colSpans, n, 1);
            colorJsons = PluginUtilities.MatchLength(colorJsons, n, "");
            alignments = PluginUtilities.MatchLength(alignments, n, "");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string request = StyleBuilders.BuildUpdateTableCellProperties(objectIds[i], rows[i], cols[i], rowSpans[i], colSpans[i], colorJsons[i], null, alignments[i]);
                if (request != null) requests.Add(request);
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connect a Fill Color and/or Content Alignment."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_TableCellStyle;
        public override Guid ComponentGuid => new Guid("d13c343c-69cf-4487-be37-b51c0bbbd73e");
    }
}
