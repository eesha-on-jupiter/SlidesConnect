using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // updateTableColumnProperties — one request per record (table, column, width), so a list of
    // widths sets each column independently; broadcast a single width to size many columns alike.
    public class TableColumnWidthComponent : GH_Component
    {
        public TableColumnWidthComponent()
          : base("Table Column Width", "ColWidth",
              "Sets the width of columns of an existing table — one record per column (Column Index + Width, index-matched); a single Width broadcasts across every listed column.",
              PluginUtilities.TabName, PluginUtilities.CategoryAHTables)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the table.", GH_ParamAccess.list, "");
            pManager.AddIntegerParameter("Column Index", "CI", "Column index per record (0-based).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Width", "W", "Column width per record, in Unit.", GH_ParamAccess.list, 1.5);
            pManager.AddTextParameter("Unit", "U", "Unit for Width (in, pt, mm, cm, m, ft).", GH_ParamAccess.list, "in");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateTableColumnProperties batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var indices = new List<int>();
            var widths = new List<double>();
            var units = new List<string>();
            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, indices);
            DA.GetDataList(2, widths);
            DA.GetDataList(3, units);

            int n = PluginUtilities.RecordCount(objectIds, indices, widths, units);
            PluginUtilities.WarnIfRagged(this, n, ("Object ID", objectIds.Count), ("Column Index", indices.Count), ("Width", widths.Count), ("Unit", units.Count));
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            indices = PluginUtilities.MatchLength(indices, n, 0);
            widths = PluginUtilities.MatchLength(widths, n, 1.5);
            units = PluginUtilities.MatchLength(units, n, "in");

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(objectIds[i]) || widths[i] <= 0) continue;
                requests.Add(StyleBuilders.BuildUpdateTableColumnProperties(objectIds[i], new[] { indices[i] }, GeometryBuilders.ToEmu(widths[i], units[i])));
            }
            if (requests.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Supply an Object ID and a Width > 0."); return; }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_TableColumnWidth;
        public override Guid ComponentGuid => new Guid("768ebf47-bcf2-4416-ac0c-f290d5fb31ae");
    }
}
