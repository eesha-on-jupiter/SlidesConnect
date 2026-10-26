using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Pulls one column out of Read CSV's Columns tree by header name (case-insensitive) instead of
    // by branch index — robust to the CSV's column order changing. Output is already the flat list
    // Build Slides From Layout / Build Simple Slide want for Title/Subtitle/Body/Photo URL.
    public class GetCsvColumnComponent : GH_Component
    {
        public GetCsvColumnComponent()
          : base("Get CSV Column", "CsvColumn",
              "Pull one column out of Read CSV's output by header name.",
              PluginUtilities.TabName, PluginUtilities.CategoryADLayouts)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Columns", "C", "Column tree from Read CSV.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Headers", "H", "Header names from Read CSV, parallel to Columns branches.", GH_ParamAccess.list);
            pManager.AddTextParameter("Header Name", "N", "Name of the column to pull out (case-insensitive).", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Values", "V", "That column's values, one per row.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Found", "F", "True if a header matching Header Name was found.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var headers = new List<string>();
            string headerName = "";

            if (!DA.GetDataTree(0, out GH_Structure<GH_String> columns)) return;
            DA.GetDataList(1, headers);
            DA.GetData(2, ref headerName);

            int index = -1;
            for (int i = 0; i < headers.Count; i++)
            {
                if (string.Equals(headers[i]?.Trim(), headerName?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"No column header matching \"{headerName}\" found.");
                DA.SetDataList(0, new List<string>());
                DA.SetData(1, false);
                return;
            }

            // branches by position, not by path {index} — the tree keeps working after a Graft /
            // Simplify / Flatten-then-Partition upstream changes the path shape
            if (index >= columns.Branches.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Header \"{headerName}\" is column {index} but the Columns tree only has {columns.Branches.Count} branch(es).");
                DA.SetDataList(0, new List<string>());
                DA.SetData(1, false);
                return;
            }

            var values = new List<string>();
            foreach (var item in columns.Branches[index])
                values.Add((item as GH_String)?.Value ?? item?.ToString() ?? "");

            DA.SetDataList(0, values);
            DA.SetData(1, true);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_GetCsvColumn;
        public override Guid ComponentGuid => new Guid("dd9e9d25-883a-46ca-b860-a8915fef8f00");
    }
}
