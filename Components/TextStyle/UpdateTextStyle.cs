using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // Styles the text already in one or more shapes (or table cells) with any combination of
    // character traits — the Slides counterpart of DocsConnect's Styled Text. Only the inputs you
    // connect are applied: an unconnected Bold/Italic/etc. leaves the existing weight alone, a 0
    // Font Size or blank Font Family is skipped. Everything is list-access and index-matched; Row/Column
    // target a table cell instead of a shape.
    public class UpdateTextStyleComponent : GH_Component
    {
        public UpdateTextStyleComponent()
          : base("Styled Text", "TxtStyle",
              "Styles the text in one or more shapes or table cells with any combination of traits — bold, italic, underline, strikethrough, size, font, color, highlight, link, baseline, small caps. Only the inputs you connect are applied. Row/Column target a table cell instead of a shape.",
              PluginUtilities.TabName, PluginUtilities.CategoryAGTextStyle)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Object ID", "OID", "Object ID(s) of the shape (or table) whose text to style.", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("Bold", "B", "Bold (connect to apply; unconnected = leave as is).", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Italic", "I", "Italic (connect to apply).", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Underline", "U", "Underline (connect to apply).", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Strikethrough", "SK", "Strikethrough (connect to apply).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Font Size", "FS", "Font size in points (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Font Family", "FF", "Font family name (blank to skip) — see the Font Family preset.", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Text Color", "FC", "Text colour (connect to apply): a Colour Swatch or a panel like 255,0,0.", GH_ParamAccess.list);
            pManager.AddColourParameter("Highlight", "HC", "Highlight (background) colour behind the text (connect to apply).", GH_ParamAccess.list);
            pManager.AddTextParameter("Link URL", "LK", "Hyperlink URL (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Baseline", "BL", "Baseline offset: SUPERSCRIPT, SUBSCRIPT or NONE (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("Small Caps", "SC", "Small caps (connect to apply).", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Row", "R", "Table only: row index of the cell whose text to style (-1 = Object ID is a shape, not a table).", GH_ParamAccess.list, -1);
            pManager.AddIntegerParameter("Column", "CL", "Table only: column index of the cell whose text to style.", GH_ParamAccess.list, -1);
            for (int i = 1; i <= 4; i++) pManager[i].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[11].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "updateTextStyle batchUpdate request JSON, one per record.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectIds = new List<string>();
            var bolds = new List<bool>();
            var italics = new List<bool>();
            var underlines = new List<bool>();
            var strikes = new List<bool>();
            var fontSizes = new List<double>();
            var fontFamilies = new List<string>();
            var textColors = new List<System.Drawing.Color>();
            var highlights = new List<System.Drawing.Color>();
            var links = new List<string>();
            var baselines = new List<string>();
            var smallCaps = new List<bool>();
            var rows = new List<int>();
            var cols = new List<int>();

            DA.GetDataList(0, objectIds);
            DA.GetDataList(1, bolds);
            DA.GetDataList(2, italics);
            DA.GetDataList(3, underlines);
            DA.GetDataList(4, strikes);
            DA.GetDataList(5, fontSizes);
            DA.GetDataList(6, fontFamilies);
            DA.GetDataList(7, textColors);
            DA.GetDataList(8, highlights);
            DA.GetDataList(9, links);
            DA.GetDataList(10, baselines);
            DA.GetDataList(11, smallCaps);
            DA.GetDataList(12, rows);
            DA.GetDataList(13, cols);

            var textColorJsons = PluginUtilities.ToColorJsons(textColors);
            var highlightJsons = PluginUtilities.ToColorJsons(highlights);

            int n = PluginUtilities.RecordCount(objectIds, bolds, italics, underlines, strikes, fontSizes, fontFamilies,
                textColorJsons, highlightJsons, links, baselines, smallCaps, rows, cols);
            PluginUtilities.WarnIfRagged(this, n,
                ("Object ID", objectIds.Count), ("Bold", bolds.Count), ("Italic", italics.Count), ("Underline", underlines.Count),
                ("Strikethrough", strikes.Count), ("Font Size", fontSizes.Count), ("Font Family", fontFamilies.Count),
                ("Text Color", textColorJsons.Count), ("Highlight", highlightJsons.Count), ("Link URL", links.Count),
                ("Baseline", baselines.Count), ("Small Caps", smallCaps.Count),
                ("Row", rows.Count), ("Column", cols.Count));

            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            var boldsN = PluginUtilities.MatchLengthOrNull(bolds, n);
            var italicsN = PluginUtilities.MatchLengthOrNull(italics, n);
            var underlinesN = PluginUtilities.MatchLengthOrNull(underlines, n);
            var strikesN = PluginUtilities.MatchLengthOrNull(strikes, n);
            var smallCapsN = PluginUtilities.MatchLengthOrNull(smallCaps, n);
            fontSizes = PluginUtilities.MatchLength(fontSizes, n, 0);
            fontFamilies = PluginUtilities.MatchLength(fontFamilies, n, "");
            textColorJsons = PluginUtilities.MatchLength(textColorJsons, n, "");
            highlightJsons = PluginUtilities.MatchLength(highlightJsons, n, "");
            links = PluginUtilities.MatchLength(links, n, "");
            baselines = PluginUtilities.MatchLength(baselines, n, "");
            rows = PluginUtilities.MatchLength(rows, n, -1);
            cols = PluginUtilities.MatchLength(cols, n, -1);

            var requests = new List<string>();
            int skipped = 0;
            for (int i = 0; i < n; i++)
            {
                var spec = new StyleBuilders.TextStyleSpec
                {
                    Bold = boldsN[i], Italic = italicsN[i], Underline = underlinesN[i], Strikethrough = strikesN[i], SmallCaps = smallCapsN[i],
                    FontSizePt = PluginUtilities.Positive(fontSizes[i]),
                    FontFamily = fontFamilies[i],
                    TextColorJson = textColorJsons[i],
                    HighlightColorJson = highlightJsons[i],
                    LinkUrl = links[i],
                    Baseline = baselines[i]
                };
                string request = StyleBuilders.BuildUpdateTextStyle(objectIds[i], spec, -1, -1, rows[i], cols[i]);
                if (request == null) { skipped++; continue; }
                requests.Add(request);
            }
            if (skipped > 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, skipped + " record(s) had no style values set — nothing to update, skipped.");

            DA.SetDataList(0, requests);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_UpdateTextStyle;
        public override Guid ComponentGuid => new Guid("89a13a00-1a70-402f-aa36-ac5db888e070");
    }
}
