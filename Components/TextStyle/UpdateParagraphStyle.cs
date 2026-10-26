using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Styles the paragraphs already in one or more shapes (or table cells) with any combination of
    // paragraph traits — the Slides counterpart of DocsConnect's Styled Paragraph. Only supplied
    // properties are applied (blank alignment / 0 spacing = leave as is). Row/Column target a table
    // cell instead of a shape.
    public class UpdateParagraphStyleComponent : GH_Component
    {
        public UpdateParagraphStyleComponent()
          : base("Styled Paragraph", "ParaStyle",
              "Styles the paragraphs in one or more shapes or table cells with any combination of paragraph-level traits — alignment, line spacing, indent, space above/below, direction. Only supplied properties are applied. Row/Column target a table cell instead of a shape.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGTextStyle)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape (or table) whose paragraphs to style.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Alignment", "AL", "Alignment: START, CENTER, END, JUSTIFIED (blank to skip) — see the Alignment preset.", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Line Spacing", "LS", "Line spacing percent, e.g. 115 (>0 to apply; 100 = normal).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Indent", "IN", "Start indent in points (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Space Above", "SA", "Space above each paragraph in points (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Space Below", "SB", "Space below each paragraph in points (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Indent End", "IE", "End indent in points (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("First Line Indent", "FI", "First-line indent in points (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Direction", "DR", "Text direction: LEFT_TO_RIGHT or RIGHT_TO_LEFT (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddIntegerParameter("Row", "R", "Table only: row index of the cell whose paragraphs to style (-1 = Object ID is a shape).", GH_ParamAccess.list, -1);
            pManager.AddIntegerParameter("Column", "CL", "Table only: column index of the cell.", GH_ParamAccess.list, -1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateParagraphStyle batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var alignments = new List<string>();
            var lineSpacings = new List<double>();
            var indents = new List<double>();
            var spaceAboves = new List<double>();
            var spaceBelows = new List<double>();
            var indentEnds = new List<double>();
            var firstLines = new List<double>();
            var directions = new List<string>();
            var rows = new List<int>();
            var cols = new List<int>();

            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, alignments);
            DA.GetDataList(2, lineSpacings);
            DA.GetDataList(3, indents);
            DA.GetDataList(4, spaceAboves);
            DA.GetDataList(5, spaceBelows);
            DA.GetDataList(6, indentEnds);
            DA.GetDataList(7, firstLines);
            DA.GetDataList(8, directions);
            DA.GetDataList(9, rows);
            DA.GetDataList(10, cols);

            int n = PluginUtilities.RecordCount(objectIds, alignments, lineSpacings, indents, spaceAboves, spaceBelows,
                indentEnds, firstLines, directions, rows, cols);
            PluginUtilities.WarnIfRagged(this, n,
                ("Object ID", objectIds.Count), ("Alignment", alignments.Count), ("Line Spacing", lineSpacings.Count),
                ("Indent", indents.Count), ("Space Above", spaceAboves.Count), ("Space Below", spaceBelows.Count),
                ("Indent End", indentEnds.Count), ("First Line Indent", firstLines.Count), ("Direction", directions.Count),
                ("Row", rows.Count), ("Column", cols.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            alignments = PluginUtilities.MatchLength(alignments, n, "");
            lineSpacings = PluginUtilities.MatchLength(lineSpacings, n, 0);
            indents = PluginUtilities.MatchLength(indents, n, 0);
            spaceAboves = PluginUtilities.MatchLength(spaceAboves, n, 0);
            spaceBelows = PluginUtilities.MatchLength(spaceBelows, n, 0);
            indentEnds = PluginUtilities.MatchLength(indentEnds, n, 0);
            firstLines = PluginUtilities.MatchLength(firstLines, n, 0);
            directions = PluginUtilities.MatchLength(directions, n, "");
            rows = PluginUtilities.MatchLength(rows, n, -1);
            cols = PluginUtilities.MatchLength(cols, n, -1);

            var requests = new List<string>();
            int skipped = 0;
            for (int i = 0; i < n; i++)
            {
                var spec = new StyleBuilders.ParagraphStyleSpec
                {
                    Alignment = alignments[i],
                    LineSpacingPercent = PluginUtilities.Positive(lineSpacings[i]),
                    IndentStartPt = PluginUtilities.Positive(indents[i]),
                    SpaceAbovePt = PluginUtilities.Positive(spaceAboves[i]),
                    SpaceBelowPt = PluginUtilities.Positive(spaceBelows[i]),
                    IndentEndPt = PluginUtilities.Positive(indentEnds[i]),
                    IndentFirstLinePt = PluginUtilities.Positive(firstLines[i]),
                    Direction = directions[i]
                };
                string request = StyleBuilders.BuildUpdateParagraphStyle(objectIds[i], spec, -1, -1, rows[i], cols[i]);
                if (request == null) { skipped++; continue; }
                requests.Add(request);
            }
            if (skipped > 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, skipped + " record(s) had no paragraph values set — nothing to update, skipped.");

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_UpdateParagraphStyle;
        public override Guid ComponentGuid => new Guid("103b0303-2ecc-4719-98e7-323940489d08");
    }
}
