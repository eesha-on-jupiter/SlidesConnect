using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Builds createTable requests: adds one or more empty tables to a page.
    public class CreateTableComponent : GH_Component
    {
        public CreateTableComponent()
          : base("Create Table", "Table",
              "Build requests that add one or more empty tables of given rows and columns to a page.",
              PluginUtilities.TabName, PluginUtilities.CategoryAHTables)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID(s) to place the table on.", GH_ParamAccess.list, "");
            pManager.AddIntegerParameter("Rows", "RW", "Number of rows.", GH_ParamAccess.list, 2);
            pManager.AddIntegerParameter("Columns", "CL", "Number of columns.", GH_ParamAccess.list, 2);
            pManager.AddTextParameter("Object ID", "OID", "Optional: your own ID(s) for the new table. Leave blank to auto-generate; the resolved ID comes out of the Object ID output either way.", GH_ParamAccess.list, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "batchUpdate request JSON, one per table.", GH_ParamAccess.list);
            pManager.AddTextParameter("Object ID", "OID", "Resolved object ID of each new table (supplied or auto-generated).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pageIds = new List<string>();
            var rows = new List<int>();
            var columns = new List<int>();
            var objectIds = new List<string>();

            DA.GetDataList(0, pageIds);
            DA.GetDataList(1, rows);
            DA.GetDataList(2, columns);
            DA.GetDataList(3, objectIds);

            int n = PluginUtilities.RecordCount(pageIds, rows, columns, objectIds);
            PluginUtilities.WarnIfRagged(this, n,
                ("Slide ID", pageIds.Count), ("Rows", rows.Count), ("Columns", columns.Count), ("Object ID", objectIds.Count));

            pageIds = PluginUtilities.MatchLength(pageIds, n, "");
            rows = PluginUtilities.MatchLength(rows, n, 2);
            columns = PluginUtilities.MatchLength(columns, n, 2);
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");

            var requests = new List<string>();
            var resolvedIds = new List<string>();
            for (int i = 0; i < n; i++)
            {
                string tableId = RequestBuilders.EnsureId(objectIds[i], "table");
                resolvedIds.Add(tableId);
                requests.Add(RequestBuilders.BuildCreateTable(pageIds[i], rows[i], columns[i], tableId));
            }

            DA.SetDataList(0, requests);
            DA.SetDataList(1, resolvedIds);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_CreateTable;
        public override Guid ComponentGuid => new Guid("bc5fe494-fde9-4016-9c48-1d7b9c31e84b");
    }
}
