using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Turns the paragraphs already in a shape (or table cell) into a bulleted / numbered list — the
    // Slides counterpart of DocsConnect's Bullet List. Each line of the shape's text becomes one item;
    // put the items in with Insert Text (one string with newlines, or Styled Text Box's Text list).
    // Bullet Preset "NONE" removes bullets instead.
    public class BulletListComponent : GH_Component
    {
        public BulletListComponent()
          : base("Bullet List", "BulletList",
              "Turns the paragraphs in one or more shapes or table cells into a bulleted or numbered list (one line = one item). Bullet Preset NONE removes existing bullets. Row/Column target a table cell instead of a shape.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGTextStyle)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape (or table) whose paragraphs to bullet.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Bullet Preset", "BP", "Bullet preset, e.g. BULLET_DISC_CIRCLE_SQUARE or NUMBERED_DIGIT_ALPHA_ROMAN — see the Bullet Preset dropdown. NONE removes bullets.", GH_ParamAccess.list, "BULLET_DISC_CIRCLE_SQUARE");
            pManager.AddIntegerParameter("Row", "R", "Table only: row index of the cell (-1 = Object ID is a shape).", GH_ParamAccess.list, -1);
            pManager.AddIntegerParameter("Column", "CL", "Table only: column index of the cell.", GH_ParamAccess.list, -1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "createParagraphBullets (or deleteParagraphBullets) batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var presets = new List<string>();
            var rows = new List<int>();
            var cols = new List<int>();

            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, presets);
            DA.GetDataList(2, rows);
            DA.GetDataList(3, cols);

            int n = PluginUtilities.RecordCount(objectIds, presets, rows, cols);
            PluginUtilities.WarnIfRagged(this, n,
                ("Object ID", objectIds.Count), ("Bullet Preset", presets.Count),
                ("Row", rows.Count), ("Column", cols.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            presets = PluginUtilities.MatchLength(presets, n, "BULLET_DISC_CIRCLE_SQUARE");
            rows = PluginUtilities.MatchLength(rows, n, -1);
            cols = PluginUtilities.MatchLength(cols, n, -1);

            var requests = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(objectIds[i])) continue;
                string preset = (presets[i] ?? "").Trim().ToUpperInvariant();
                if (preset != "" && !preset.StartsWith("BULLET_") && !preset.StartsWith("NUMBERED_") && preset != "NONE")
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        "Bullet Preset \"" + presets[i] + "\" is not a Google Slides bullet preset (e.g. BULLET_DISC_CIRCLE_SQUARE, BULLET_CHECKBOX, NUMBERED_DIGIT_ALPHA_ROMAN — use the Bullet Preset dropdown).");
                    continue;
                }
                requests.Add(preset == "NONE"
                    ? StyleBuilders.BuildDeleteParagraphBullets(objectIds[i], -1, -1, rows[i], cols[i])
                    : StyleBuilders.BuildCreateParagraphBullets(objectIds[i], preset, -1, -1, rows[i], cols[i]));
            }

            if (requests.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Object ID supplied.");
                return;
            }
            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_BulletList;
        public override Guid ComponentGuid => new Guid("0b4d04ab-fc79-4978-b0ec-05e5fd833920");
    }
}
