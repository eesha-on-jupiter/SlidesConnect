using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Border colour / weight / dash on one position of a table range (or the whole table).
    public class TableBordersComponent : GH_Component
    {
        public TableBordersComponent()
          : base("Table Borders", "Borders",
              "Sets border colour, weight and/or dash on one side (or ALL / OUTER / INNER) of a range of cells in an existing table. Row/Column -1 = whole table. Only the inputs you connect are applied.",
              PluginUtilities.TabName, PluginUtilities.CategoryAHTables)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the table.", GH_ParamAccess.list, "");
            pManager.AddIntegerParameter("Row", "R", "First row of the range (0-based). -1 = the whole table.", GH_ParamAccess.list, -1);
            pManager.AddIntegerParameter("Column", "CL", "First column of the range (0-based). -1 = the whole table.", GH_ParamAccess.list, -1);
            pManager.AddIntegerParameter("Row Span", "RS", "Number of rows in the range.", GH_ParamAccess.list, 1);
            pManager.AddIntegerParameter("Column Span", "CS", "Number of columns in the range.", GH_ParamAccess.list, 1);
            pManager.AddTextParameter("Border Position", "BP", "ALL, OUTER, INNER, INNER_HORIZONTAL, INNER_VERTICAL, TOP, BOTTOM, LEFT or RIGHT — see the Border Position preset.", GH_ParamAccess.list, "ALL");
            pManager.AddColourParameter("Border Color", "BC", "Border colour (connect to apply).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Border Weight", "BW", "Border weight, in Unit (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Border Dash", "BD", "SOLID, DOT, DASH, DASH_DOT, LONG_DASH, LONG_DASH_DOT (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Unit", "U", "Unit for Border Weight (pt, in, mm, cm, m, ft).", GH_ParamAccess.list, "pt");
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateTableBorderProperties batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var rows = new List<int>();
            var cols = new List<int>();
            var rowSpans = new List<int>();
            var colSpans = new List<int>();
            var positions = new List<string>();
            var colors = new List<System.Drawing.Color>();
            var weights = new List<double>();
            var dashes = new List<string>();
            var units = new List<string>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, rows);
            DA.GetDataList(2, cols);
            DA.GetDataList(3, rowSpans);
            DA.GetDataList(4, colSpans);
            DA.GetDataList(5, positions);
            DA.GetDataList(6, colors);
            DA.GetDataList(7, weights);
            DA.GetDataList(8, dashes);
            DA.GetDataList(9, units);
            var colorJsons = PluginUtilities.ToColorJsons(colors);

            int n = PluginUtilities.RecordCount(objectIds, rows, cols, rowSpans, colSpans, positions, colorJsons, weights, dashes, units);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Row", rows.Count), ("Column", cols.Count),
                ("Row Span", rowSpans.Count), ("Column Span", colSpans.Count), ("Border Position", positions.Count),
                ("Border Color", colorJsons.Count), ("Border Weight", weights.Count), ("Border Dash", dashes.Count), ("Unit", units.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            rows = PluginUtilities.MatchLength(rows, n, -1);
            cols = PluginUtilities.MatchLength(cols, n, -1);
            rowSpans = PluginUtilities.MatchLength(rowSpans, n, 1);
            colSpans = PluginUtilities.MatchLength(colSpans, n, 1);
            positions = PluginUtilities.MatchLength(positions, n, "ALL");
            colorJsons = PluginUtilities.MatchLength(colorJsons, n, "");
            weights = PluginUtilities.MatchLength(weights, n, 0);
            dashes = PluginUtilities.MatchLength(dashes, n, "");
            units = PluginUtilities.MatchLength(units, n, "pt");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                double? weightEmu = weights[i] > 0 ? GeometryBuilders.ToEmu(weights[i], units[i]) : (double?)null;
                string request = StyleBuilders.BuildUpdateTableBorderProperties(objectIds[i], rows[i], cols[i], rowSpans[i], colSpans[i], positions[i], colorJsons[i], weightEmu, dashes[i]);
                if (request != null) requests.Add(request);
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Connect a Border Color, Border Weight and/or Border Dash."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_TableBorders;
        public override Guid ComponentGuid => new Guid("edead704-3e64-43fa-9025-709a7cce2c01");
    }
}
