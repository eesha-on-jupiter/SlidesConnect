using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SlidesConnect
{
    // One component from "a string and a place on the slide" to a finished, styled text box: creates
    // a TEXT_BOX per record, inserts the text, and bundles any character / paragraph / box traits
    // you connect — the Slides counterpart of DocsConnect's Styled Text, which in Docs can just
    // insert at an index but here needs a shape to live in. Newlines inside a Text item become
    // paragraphs (so Bullet Preset makes a list). Same Box = true is the per-fragment mode: every Text
    // item becomes one run inside ONE box, keeping its own Font Size / Text Color / etc. — sample a
    // sine into Font Size with one letter per item for a wave, or a Gradient into Text Color. The
    // character ranges are computed by the builder from the fragment lengths, never by the user.
    // The Object ID output feeds the Text Style tab for anything not covered here.
    public class StyledTextBoxComponent : GH_Component
    {
        public StyledTextBoxComponent()
          : base("Styled Text Box", "TxtBox",
              "Creates a text box per record, inserts the text, and applies any combination of styles — bold, italic, underline, strikethrough, size, font, color, highlight, link, baseline, alignment, line spacing, bullets, fill, vertical alignment, autofit, rotation, flip, skew. Only the inputs you connect are applied. Newlines in Text make paragraphs. Same Box = true joins every Text item into ONE box, each item keeping its own style (font-size wave, colour gradient across a sentence).",
              PluginUtilities.TabName, PluginUtilities.CategoryAFText)
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Slide ID", "SID", "Slide ID(s) to place the text box on.", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Text", "TX", "Text for each box. Newlines make separate paragraphs within the box.", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("X", "X", "Left position, in Unit.", GH_ParamAccess.list, 0.5);
            pManager.AddNumberParameter("Y", "Y", "Top position, in Unit.", GH_ParamAccess.list, 0.5);
            pManager.AddNumberParameter("Width", "W", "Box width, in Unit.", GH_ParamAccess.list, 4.0);
            pManager.AddNumberParameter("Height", "H", "Box height, in Unit.", GH_ParamAccess.list, 1.0);
            pManager.AddTextParameter("Unit", "U", "Unit for X/Y/Width/Height (in, pt, mm, cm, m, ft, emu). Page is 10 x 5.625 in.", GH_ParamAccess.list, "in");
            pManager.AddTextParameter("Object ID", "OID", "Optional: your own ID(s) for the new box. Leave blank to auto-generate; the resolved ID comes out of the Object ID output either way.", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("Bold", "B", "Bold (connect to apply).", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Italic", "I", "Italic (connect to apply).", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Underline", "U", "Underline (connect to apply).", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Strikethrough", "SK", "Strikethrough (connect to apply).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Font Size", "FS", "Font size in points (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Font Family", "FF", "Font family name (blank to skip) — see the Font Family preset.", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Text Color", "FC", "Text colour (connect to apply).", GH_ParamAccess.list);
            pManager.AddColourParameter("Highlight", "HC", "Highlight (background) colour behind the text (connect to apply).", GH_ParamAccess.list);
            pManager.AddTextParameter("Link URL", "LK", "Hyperlink URL (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Baseline", "BL", "Baseline offset: SUPERSCRIPT, SUBSCRIPT or NONE (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Alignment", "AL", "Paragraph alignment: START, CENTER, END, JUSTIFIED (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Line Spacing", "LS", "Line spacing percent, e.g. 115 (>0 to apply).", GH_ParamAccess.list, 0);
            pManager.AddTextParameter("Bullet Preset", "BP", "Bullet preset to list the paragraphs, e.g. BULLET_DISC_CIRCLE_SQUARE (blank = no bullets).", GH_ParamAccess.list, "");
            pManager.AddColourParameter("Fill Color", "BG", "Box background fill colour (connect to apply).", GH_ParamAccess.list);
            pManager.AddTextParameter("Content Alignment", "CA", "Vertical alignment of the text in the box: TOP, MIDDLE, BOTTOM (blank to skip).", GH_ParamAccess.list, "");
            pManager.AddTextParameter("Autofit", "AF", "Autofit: NONE, TEXT_AUTOFIT (shrink text to fit) or SHAPE_AUTOFIT (grow box to fit). Blank to skip.", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Rotation", "RO", "Rotation in degrees, clockwise, about the box's centre (0 = none).", GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("Flip X", "FX", "Mirror the box left-right (about its centre).", GH_ParamAccess.list, false);
            pManager.AddBooleanParameter("Flip Y", "FY", "Mirror the box top-bottom (about its centre).", GH_ParamAccess.list, false);
            pManager.AddNumberParameter("Skew X", "KX", "Lean the box's top edge sideways, in degrees (positive = right).", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Skew Y", "KY", "Lean the box's left edge, in degrees (positive = down).", GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("Same Box", "SB", "False (default): one box per Text item. True: all Text items join into ONE box (separated by Separator), each keeping its own style — e.g. one letter per item with a sampled Font Size list for a wave, or a Gradient into Text Color.", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Separator", "SEP", "Only used when Same Box is true. Inserted between items — \"\" for letter-by-letter, \" \" for word-by-word.", GH_ParamAccess.item, "");
            foreach (int i in new[] { 8, 9, 10, 11, 14, 15, 21 }) pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Request JSON", "RQ", "createShape + insertText + style requests for every box, in order — wire into Requests Aggregator / Batch Update.", GH_ParamAccess.list);
            pManager.AddTextParameter("Object ID", "OID", "Resolved object ID of each new text box — feed the Text Style tab for further styling.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var slideIds = new List<string>();
            var texts = new List<string>();
            var xs = new List<double>();
            var ys = new List<double>();
            var widths = new List<double>();
            var heights = new List<double>();
            var units = new List<string>();
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
            var alignments = new List<string>();
            var lineSpacings = new List<double>();
            var bulletPresets = new List<string>();
            var fillColors = new List<System.Drawing.Color>();
            var contentAlignments = new List<string>();
            var autofits = new List<string>();
            var rotations = new List<double>();
            var flipXs = new List<bool>();
            var flipYs = new List<bool>();
            var skewXs = new List<double>();
            var skewYs = new List<double>();
            bool sameBox = false;
            string separator = "";

            DA.GetDataList(0, slideIds);
            DA.GetDataList(1, texts);
            DA.GetDataList(2, xs);
            DA.GetDataList(3, ys);
            DA.GetDataList(4, widths);
            DA.GetDataList(5, heights);
            DA.GetDataList(6, units);
            DA.GetDataList(7, objectIds);
            DA.GetDataList(8, bolds);
            DA.GetDataList(9, italics);
            DA.GetDataList(10, underlines);
            DA.GetDataList(11, strikes);
            DA.GetDataList(12, fontSizes);
            DA.GetDataList(13, fontFamilies);
            DA.GetDataList(14, textColors);
            DA.GetDataList(15, highlights);
            DA.GetDataList(16, links);
            DA.GetDataList(17, baselines);
            DA.GetDataList(18, alignments);
            DA.GetDataList(19, lineSpacings);
            DA.GetDataList(20, bulletPresets);
            DA.GetDataList(21, fillColors);
            DA.GetDataList(22, contentAlignments);
            DA.GetDataList(23, autofits);
            DA.GetDataList(24, rotations);
            DA.GetDataList(25, flipXs);
            DA.GetDataList(26, flipYs);
            DA.GetDataList(27, skewXs);
            DA.GetDataList(28, skewYs);
            DA.GetData(29, ref sameBox);
            DA.GetData(30, ref separator);

            var textColorJsons = PluginUtilities.ToColorJsons(textColors);
            var highlightJsons = PluginUtilities.ToColorJsons(highlights);
            var fillJsons = PluginUtilities.ToColorJsons(fillColors);

            // Same Box: the per-fragment traits are matched to the Text list (one entry per fragment);
            // everything about the box itself comes from record 0. Otherwise: one record = one box.
            int n = sameBox
                ? Math.Max(1, texts.Count)
                : PluginUtilities.RecordCount(slideIds, texts, xs, ys, widths, heights, units, objectIds, bolds, italics, underlines, strikes,
                    fontSizes, fontFamilies, textColorJsons, highlightJsons, links, baselines, alignments, lineSpacings, bulletPresets,
                    fillJsons, contentAlignments, autofits, rotations, flipXs, flipYs, skewXs, skewYs);
            if (!sameBox)
            PluginUtilities.WarnIfRagged(this, n,
                ("Slide ID", slideIds.Count), ("Text", texts.Count), ("X", xs.Count), ("Y", ys.Count), ("Width", widths.Count), ("Height", heights.Count),
                ("Unit", units.Count), ("Object ID", objectIds.Count), ("Bold", bolds.Count), ("Italic", italics.Count), ("Underline", underlines.Count),
                ("Strikethrough", strikes.Count), ("Font Size", fontSizes.Count), ("Font Family", fontFamilies.Count), ("Text Color", textColorJsons.Count),
                ("Highlight", highlightJsons.Count), ("Link URL", links.Count), ("Baseline", baselines.Count), ("Alignment", alignments.Count),
                ("Line Spacing", lineSpacings.Count), ("Bullet Preset", bulletPresets.Count), ("Fill Color", fillJsons.Count),
                ("Content Alignment", contentAlignments.Count), ("Autofit", autofits.Count), ("Rotation", rotations.Count),
                ("Flip X", flipXs.Count), ("Flip Y", flipYs.Count), ("Skew X", skewXs.Count), ("Skew Y", skewYs.Count));

            slideIds = PluginUtilities.MatchLength(slideIds, n, "");
            texts = PluginUtilities.MatchLength(texts, n, "");
            xs = PluginUtilities.MatchLength(xs, n, 0.5);
            ys = PluginUtilities.MatchLength(ys, n, 0.5);
            widths = PluginUtilities.MatchLength(widths, n, 4.0);
            heights = PluginUtilities.MatchLength(heights, n, 1.0);
            units = PluginUtilities.MatchLength(units, n, "in");
            objectIds = PluginUtilities.MatchLength(objectIds, n, "");
            var boldsN = PluginUtilities.MatchLengthOrNull(bolds, n);
            var italicsN = PluginUtilities.MatchLengthOrNull(italics, n);
            var underlinesN = PluginUtilities.MatchLengthOrNull(underlines, n);
            var strikesN = PluginUtilities.MatchLengthOrNull(strikes, n);
            fontSizes = PluginUtilities.MatchLength(fontSizes, n, 0);
            fontFamilies = PluginUtilities.MatchLength(fontFamilies, n, "");
            textColorJsons = PluginUtilities.MatchLength(textColorJsons, n, "");
            highlightJsons = PluginUtilities.MatchLength(highlightJsons, n, "");
            links = PluginUtilities.MatchLength(links, n, "");
            baselines = PluginUtilities.MatchLength(baselines, n, "");
            alignments = PluginUtilities.MatchLength(alignments, n, "");
            lineSpacings = PluginUtilities.MatchLength(lineSpacings, n, 0);
            bulletPresets = PluginUtilities.MatchLength(bulletPresets, n, "");
            fillJsons = PluginUtilities.MatchLength(fillJsons, n, "");
            contentAlignments = PluginUtilities.MatchLength(contentAlignments, n, "");
            autofits = PluginUtilities.MatchLength(autofits, n, "");
            rotations = PluginUtilities.MatchLength(rotations, n, 0);
            flipXs = PluginUtilities.MatchLength(flipXs, n, false);
            flipYs = PluginUtilities.MatchLength(flipYs, n, false);
            skewXs = PluginUtilities.MatchLength(skewXs, n, 0);
            skewYs = PluginUtilities.MatchLength(skewYs, n, 0);

            var requests = new List<string>();
            var resolvedIds = new List<string>();

            if (sameBox)
            {
                if (string.IsNullOrWhiteSpace(slideIds[0]))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Slide ID is blank — nothing built.");
                    return;
                }
                string boxId = RequestBuilders.EnsureId(objectIds[0], "textbox");
                var runStyles = new List<StyleBuilders.TextStyleSpec>();
                for (int i = 0; i < n; i++)
                    runStyles.Add(new StyleBuilders.TextStyleSpec
                    {
                        Bold = boldsN[i], Italic = italicsN[i], Underline = underlinesN[i], Strikethrough = strikesN[i],
                        FontSizePt = PluginUtilities.Positive(fontSizes[i]), FontFamily = fontFamilies[i],
                        TextColorJson = textColorJsons[i], HighlightColorJson = highlightJsons[i],
                        LinkUrl = links[i], Baseline = baselines[i]
                    });
                requests.AddRange(StyleBuilders.AssembleStyledTextRuns(slideIds[0], texts, separator,
                    GeometryBuilders.ToEmu(widths[0], units[0]), GeometryBuilders.ToEmu(heights[0], units[0]),
                    GeometryBuilders.ToEmu(xs[0], units[0]), GeometryBuilders.ToEmu(ys[0], units[0]),
                    boxId, runStyles,
                    new StyleBuilders.ParagraphStyleSpec { Alignment = alignments[0], LineSpacingPercent = PluginUtilities.Positive(lineSpacings[0]) },
                    bulletPresets[0],
                    new StyleBuilders.ShapeStyleSpec { FillColorJson = fillJsons[0], ContentAlignment = contentAlignments[0], Autofit = autofits[0] },
                    new RequestBuilders.Placement { RotationDegrees = rotations[0], FlipX = flipXs[0], FlipY = flipYs[0], SkewXDegrees = skewXs[0], SkewYDegrees = skewYs[0] }));
                resolvedIds.Add(boxId);
                DA.SetDataList(0, requests);
                DA.SetDataList(1, resolvedIds);
                return;
            }

            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(slideIds[i]))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"[{i}] Slide ID is blank — skipped.");
                    continue;
                }
                string boxId = RequestBuilders.EnsureId(objectIds[i], "textbox");
                resolvedIds.Add(boxId);

                var textStyle = new StyleBuilders.TextStyleSpec
                {
                    Bold = boldsN[i], Italic = italicsN[i], Underline = underlinesN[i], Strikethrough = strikesN[i],
                    FontSizePt = PluginUtilities.Positive(fontSizes[i]), FontFamily = fontFamilies[i],
                    TextColorJson = textColorJsons[i], HighlightColorJson = highlightJsons[i],
                    LinkUrl = links[i], Baseline = baselines[i]
                };
                var paragraphStyle = new StyleBuilders.ParagraphStyleSpec
                {
                    Alignment = alignments[i], LineSpacingPercent = PluginUtilities.Positive(lineSpacings[i])
                };
                var shapeStyle = new StyleBuilders.ShapeStyleSpec
                {
                    FillColorJson = fillJsons[i], ContentAlignment = contentAlignments[i], Autofit = autofits[i]
                };

                requests.AddRange(StyleBuilders.AssembleStyledTextBox(slideIds[i], texts[i],
                    GeometryBuilders.ToEmu(widths[i], units[i]), GeometryBuilders.ToEmu(heights[i], units[i]),
                    GeometryBuilders.ToEmu(xs[i], units[i]), GeometryBuilders.ToEmu(ys[i], units[i]),
                    boxId, textStyle, paragraphStyle, bulletPresets[i], shapeStyle,
                    new RequestBuilders.Placement { RotationDegrees = rotations[i], FlipX = flipXs[i], FlipY = flipYs[i], SkewXDegrees = skewXs[i], SkewYDegrees = skewYs[i] }));
            }

            DA.SetDataList(0, requests);
            DA.SetDataList(1, resolvedIds);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GS_StyledTextBox;
        public override Guid ComponentGuid => new Guid("dee04f68-4559-4241-9927-208f46e684b7");
    }
}
