using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlidesConnect
{
    // Text / paragraph / shape / table styling requests — the Slides equivalents of DocsConnect's
    // Text and Paragraph tabs. Pure Newtonsoft + BCL only, like RequestBuilders. Every "spec" here is
    // a bag of nullable traits: a null trait is left out of the field mask entirely, so the API leaves
    // that property as it is rather than resetting it. A request with nothing set returns null and
    // the calling component skips it.
    public static class StyleBuilders
    {
        #region Specs

        public sealed class TextStyleSpec
        {
            public bool? Bold, Italic, Underline, Strikethrough, SmallCaps;
            public double? FontSizePt;
            public string FontFamily;
            public string TextColorJson;      // OpaqueColor JSON ({"rgbColor":...}) — RequestBuilders.ColorJson
            public string HighlightColorJson; // OpaqueColor JSON
            public string LinkUrl;
            public string Baseline;           // SUPERSCRIPT | SUBSCRIPT | NONE

            public bool IsEmpty =>
                !Bold.HasValue && !Italic.HasValue && !Underline.HasValue && !Strikethrough.HasValue && !SmallCaps.HasValue
                && !FontSizePt.HasValue && string.IsNullOrWhiteSpace(FontFamily)
                && string.IsNullOrWhiteSpace(TextColorJson) && string.IsNullOrWhiteSpace(HighlightColorJson)
                && string.IsNullOrWhiteSpace(LinkUrl) && string.IsNullOrWhiteSpace(Baseline);
        }

        public sealed class ParagraphStyleSpec
        {
            public string Alignment;          // START | CENTER | END | JUSTIFIED
            public double? LineSpacingPercent; // 100 = normal
            public double? IndentStartPt, IndentEndPt, IndentFirstLinePt, SpaceAbovePt, SpaceBelowPt;
            public string Direction;          // LEFT_TO_RIGHT | RIGHT_TO_LEFT

            public bool IsEmpty =>
                string.IsNullOrWhiteSpace(Alignment) && !LineSpacingPercent.HasValue
                && !IndentStartPt.HasValue && !IndentEndPt.HasValue && !IndentFirstLinePt.HasValue
                && !SpaceAbovePt.HasValue && !SpaceBelowPt.HasValue && string.IsNullOrWhiteSpace(Direction);
        }

        public sealed class ShapeStyleSpec
        {
            public string FillColorJson;
            public double? FillAlpha;         // 1 = opaque, 0 = fully transparent
            public string OutlineColorJson;
            public double? OutlineWeightEmu;
            public string OutlineDash;        // SOLID | DOT | DASH | DASH_DOT | LONG_DASH | LONG_DASH_DOT
            public bool? Shadow;              // true = render an outer shadow, false = remove it
            public string ShadowColorJson;
            public double? ShadowAlpha;
            public double? ShadowBlurEmu;
            public double? ShadowOffsetXEmu, ShadowOffsetYEmu;
            public string ShadowAlignment;    // BOTTOM_RIGHT etc.
            public string ContentAlignment;   // TOP | MIDDLE | BOTTOM
            public string Autofit;            // NONE | TEXT_AUTOFIT | SHAPE_AUTOFIT
            public string LinkUrl;

            public bool IsEmpty =>
                string.IsNullOrWhiteSpace(FillColorJson) && !FillAlpha.HasValue
                && string.IsNullOrWhiteSpace(OutlineColorJson) && !OutlineWeightEmu.HasValue && string.IsNullOrWhiteSpace(OutlineDash)
                && !Shadow.HasValue && string.IsNullOrWhiteSpace(ContentAlignment)
                && string.IsNullOrWhiteSpace(Autofit) && string.IsNullOrWhiteSpace(LinkUrl);
        }

        #endregion

        #region Shared JSON pieces

        private static JObject Pt(double magnitude) => new JObject { ["magnitude"] = magnitude, ["unit"] = "PT" };
        private static JObject Emu(double magnitude) => new JObject { ["magnitude"] = magnitude, ["unit"] = "EMU" };

        // start/end < 0 mean "unbounded": (-1,-1) = ALL, (s,-1) = FROM_START_INDEX, (s,e) = FIXED_RANGE
        public static JObject TextRange(int startIndex, int endIndex)
        {
            if (startIndex < 0) return new JObject { ["type"] = "ALL" };
            if (endIndex < 0) return new JObject { ["type"] = "FROM_START_INDEX", ["startIndex"] = startIndex };
            return new JObject { ["type"] = "FIXED_RANGE", ["startIndex"] = startIndex, ["endIndex"] = Math.Max(endIndex, startIndex) };
        }

        // row/col < 0 = not a table cell; otherwise the request targets that cell of a table object
        private static void PutCellLocation(JObject inner, int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || columnIndex < 0) return;
            inner["cellLocation"] = new JObject { ["rowIndex"] = rowIndex, ["columnIndex"] = columnIndex };
        }

        // a subset of a table: null when row/col < 0 (= the whole table, the API default)
        public static JObject TableRange(int rowIndex, int columnIndex, int rowSpan, int columnSpan)
        {
            if (rowIndex < 0 || columnIndex < 0) return null;
            return new JObject
            {
                ["location"] = new JObject { ["rowIndex"] = rowIndex, ["columnIndex"] = columnIndex },
                ["rowSpan"] = Math.Max(1, rowSpan),
                ["columnSpan"] = Math.Max(1, columnSpan)
            };
        }

        private static JObject SolidFill(string colorJson, double? alpha)
        {
            var fill = new JObject { ["color"] = RequestBuilders.Parse(colorJson) };
            if (alpha.HasValue) fill["alpha"] = Clamp01(alpha.Value);
            return new JObject { ["solidFill"] = fill };
        }

        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        private static string Upper(string s) => (s ?? "").Trim().ToUpperInvariant();

        #endregion

        #region Text style

        public static string BuildUpdateTextStyle(string objectId, TextStyleSpec spec, int startIndex = -1, int endIndex = -1, int rowIndex = -1, int columnIndex = -1)
        {
            if (spec == null || spec.IsEmpty) return null;

            var style = new JObject();
            var fields = new List<string>();

            if (spec.Bold.HasValue) { style["bold"] = spec.Bold.Value; fields.Add("bold"); }
            if (spec.Italic.HasValue) { style["italic"] = spec.Italic.Value; fields.Add("italic"); }
            if (spec.Underline.HasValue) { style["underline"] = spec.Underline.Value; fields.Add("underline"); }
            if (spec.Strikethrough.HasValue) { style["strikethrough"] = spec.Strikethrough.Value; fields.Add("strikethrough"); }
            if (spec.SmallCaps.HasValue) { style["smallCaps"] = spec.SmallCaps.Value; fields.Add("smallCaps"); }
            if (spec.FontSizePt.HasValue && spec.FontSizePt.Value > 0) { style["fontSize"] = Pt(spec.FontSizePt.Value); fields.Add("fontSize"); }
            if (!string.IsNullOrWhiteSpace(spec.FontFamily)) { style["fontFamily"] = spec.FontFamily.Trim(); fields.Add("fontFamily"); }

            var fg = RequestBuilders.Parse(spec.TextColorJson);
            if (fg != null) { style["foregroundColor"] = new JObject { ["opaqueColor"] = fg }; fields.Add("foregroundColor"); }
            var bg = RequestBuilders.Parse(spec.HighlightColorJson);
            if (bg != null) { style["backgroundColor"] = new JObject { ["opaqueColor"] = bg }; fields.Add("backgroundColor"); }

            if (!string.IsNullOrWhiteSpace(spec.LinkUrl)) { style["link"] = new JObject { ["url"] = spec.LinkUrl.Trim() }; fields.Add("link"); }
            if (!string.IsNullOrWhiteSpace(spec.Baseline)) { style["baselineOffset"] = Upper(spec.Baseline); fields.Add("baselineOffset"); }

            if (fields.Count == 0) return null;

            var inner = new JObject { ["objectId"] = objectId ?? "" };
            PutCellLocation(inner, rowIndex, columnIndex);
            inner["style"] = style;
            inner["textRange"] = TextRange(startIndex, endIndex);
            inner["fields"] = string.Join(",", fields);
            return new JObject { ["updateTextStyle"] = inner }.ToString(Formatting.None);
        }

        #endregion

        #region Paragraph style

        public static string BuildUpdateParagraphStyle(string objectId, ParagraphStyleSpec spec, int startIndex = -1, int endIndex = -1, int rowIndex = -1, int columnIndex = -1)
        {
            if (spec == null || spec.IsEmpty) return null;

            var style = new JObject();
            var fields = new List<string>();

            if (!string.IsNullOrWhiteSpace(spec.Alignment)) { style["alignment"] = Upper(spec.Alignment); fields.Add("alignment"); }
            if (spec.LineSpacingPercent.HasValue && spec.LineSpacingPercent.Value > 0) { style["lineSpacing"] = spec.LineSpacingPercent.Value; fields.Add("lineSpacing"); }
            if (spec.IndentStartPt.HasValue) { style["indentStart"] = Pt(spec.IndentStartPt.Value); fields.Add("indentStart"); }
            if (spec.IndentEndPt.HasValue) { style["indentEnd"] = Pt(spec.IndentEndPt.Value); fields.Add("indentEnd"); }
            if (spec.IndentFirstLinePt.HasValue) { style["indentFirstLine"] = Pt(spec.IndentFirstLinePt.Value); fields.Add("indentFirstLine"); }
            if (spec.SpaceAbovePt.HasValue) { style["spaceAbove"] = Pt(spec.SpaceAbovePt.Value); fields.Add("spaceAbove"); }
            if (spec.SpaceBelowPt.HasValue) { style["spaceBelow"] = Pt(spec.SpaceBelowPt.Value); fields.Add("spaceBelow"); }
            if (!string.IsNullOrWhiteSpace(spec.Direction)) { style["direction"] = Upper(spec.Direction); fields.Add("direction"); }

            if (fields.Count == 0) return null;

            var inner = new JObject { ["objectId"] = objectId ?? "" };
            PutCellLocation(inner, rowIndex, columnIndex);
            inner["style"] = style;
            inner["textRange"] = TextRange(startIndex, endIndex);
            inner["fields"] = string.Join(",", fields);
            return new JObject { ["updateParagraphStyle"] = inner }.ToString(Formatting.None);
        }

        // create paragraph bullets — bulletPreset is a Slides BulletGlyphPreset (BULLET_DISC_CIRCLE_SQUARE,
        // NUMBERED_DIGIT_ALPHA_ROMAN, ...). Note the Slides enum names differ from Docs' (DIGIT vs DECIMAL).
        public static string BuildCreateParagraphBullets(string objectId, string bulletPreset, int startIndex = -1, int endIndex = -1, int rowIndex = -1, int columnIndex = -1)
        {
            var inner = new JObject { ["objectId"] = objectId ?? "" };
            PutCellLocation(inner, rowIndex, columnIndex);
            inner["textRange"] = TextRange(startIndex, endIndex);
            inner["bulletPreset"] = string.IsNullOrWhiteSpace(bulletPreset) ? "BULLET_DISC_CIRCLE_SQUARE" : Upper(bulletPreset);
            return new JObject { ["createParagraphBullets"] = inner }.ToString(Formatting.None);
        }

        public static string BuildDeleteParagraphBullets(string objectId, int startIndex = -1, int endIndex = -1, int rowIndex = -1, int columnIndex = -1)
        {
            var inner = new JObject { ["objectId"] = objectId ?? "" };
            PutCellLocation(inner, rowIndex, columnIndex);
            inner["textRange"] = TextRange(startIndex, endIndex);
            return new JObject { ["deleteParagraphBullets"] = inner }.ToString(Formatting.None);
        }

        #endregion

        #region Shape style

        public static string BuildUpdateShapeProperties(string objectId, ShapeStyleSpec spec)
        {
            if (spec == null || spec.IsEmpty) return null;

            var props = new JObject();
            var fields = new List<string>();

            // fill: colour and/or transparency. Alpha alone is allowed (keeps the current colour).
            bool hasFillColor = !string.IsNullOrWhiteSpace(spec.FillColorJson);
            if (hasFillColor || spec.FillAlpha.HasValue)
            {
                var fill = new JObject();
                if (hasFillColor) { fill["color"] = RequestBuilders.Parse(spec.FillColorJson); fields.Add("shapeBackgroundFill.solidFill.color"); }
                if (spec.FillAlpha.HasValue) { fill["alpha"] = Clamp01(spec.FillAlpha.Value); fields.Add("shapeBackgroundFill.solidFill.alpha"); }
                props["shapeBackgroundFill"] = new JObject { ["solidFill"] = fill };
            }

            // outline: any of colour / weight / dash
            bool hasOutlineColor = !string.IsNullOrWhiteSpace(spec.OutlineColorJson);
            bool hasOutlineWeight = spec.OutlineWeightEmu.HasValue && spec.OutlineWeightEmu.Value > 0;
            bool hasOutlineDash = !string.IsNullOrWhiteSpace(spec.OutlineDash);
            if (hasOutlineColor || hasOutlineWeight || hasOutlineDash)
            {
                var outline = new JObject();
                if (hasOutlineColor) { outline["outlineFill"] = SolidFill(spec.OutlineColorJson, 1); fields.Add("outline.outlineFill.solidFill.color"); }
                if (hasOutlineWeight) { outline["weight"] = Emu(spec.OutlineWeightEmu.Value); fields.Add("outline.weight"); }
                if (hasOutlineDash) { outline["dashStyle"] = Upper(spec.OutlineDash); fields.Add("outline.dashStyle"); }
                props["outline"] = outline;
            }

            // shadow: true renders an outer shadow with the given look; false hides it
            if (spec.Shadow.HasValue)
            {
                if (spec.Shadow.Value)
                {
                    var shadow = new JObject
                    {
                        ["type"] = "OUTER",
                        ["alignment"] = string.IsNullOrWhiteSpace(spec.ShadowAlignment) ? "BOTTOM_RIGHT" : Upper(spec.ShadowAlignment),
                        ["transform"] = new JObject
                        {
                            ["scaleX"] = 1, ["scaleY"] = 1,
                            ["translateX"] = spec.ShadowOffsetXEmu ?? 38100.0, // 3 pt
                            ["translateY"] = spec.ShadowOffsetYEmu ?? 38100.0,
                            ["unit"] = "EMU"
                        },
                        ["blurRadius"] = Emu(spec.ShadowBlurEmu ?? 38100.0),
                        ["color"] = RequestBuilders.Parse(spec.ShadowColorJson) ?? new JObject { ["rgbColor"] = new JObject { ["red"] = 0, ["green"] = 0, ["blue"] = 0 } },
                        ["alpha"] = Clamp01(spec.ShadowAlpha ?? 0.4),
                        ["rotateWithShape"] = false,
                        ["propertyState"] = "RENDERED"
                    };
                    props["shadow"] = shadow;
                    fields.Add("shadow");
                }
                else
                {
                    props["shadow"] = new JObject { ["propertyState"] = "NOT_RENDERED" };
                    fields.Add("shadow.propertyState");
                }
            }

            if (!string.IsNullOrWhiteSpace(spec.ContentAlignment)) { props["contentAlignment"] = Upper(spec.ContentAlignment); fields.Add("contentAlignment"); }
            if (!string.IsNullOrWhiteSpace(spec.Autofit)) { props["autofit"] = new JObject { ["autofitType"] = Upper(spec.Autofit) }; fields.Add("autofit.autofitType"); }
            if (!string.IsNullOrWhiteSpace(spec.LinkUrl)) { props["link"] = new JObject { ["url"] = spec.LinkUrl.Trim() }; fields.Add("link"); }

            if (fields.Count == 0) return null;

            var inner = new JObject
            {
                ["objectId"] = objectId ?? "",
                ["shapeProperties"] = props,
                ["fields"] = string.Join(",", fields)
            };
            return new JObject { ["updateShapeProperties"] = inner }.ToString(Formatting.None);
        }

        #endregion

        #region Tables

        // cell fill and/or vertical content alignment over a range (whole table when row/col < 0)
        public static string BuildUpdateTableCellProperties(string objectId, int rowIndex, int columnIndex, int rowSpan, int columnSpan,
            string fillColorJson, double? fillAlpha, string contentAlignment)
        {
            var props = new JObject();
            var fields = new List<string>();

            if (!string.IsNullOrWhiteSpace(fillColorJson))
            {
                props["tableCellBackgroundFill"] = SolidFill(fillColorJson, fillAlpha);
                fields.Add("tableCellBackgroundFill.solidFill.color");
                if (fillAlpha.HasValue) fields.Add("tableCellBackgroundFill.solidFill.alpha");
            }
            if (!string.IsNullOrWhiteSpace(contentAlignment)) { props["contentAlignment"] = Upper(contentAlignment); fields.Add("contentAlignment"); }
            if (fields.Count == 0) return null;

            var inner = new JObject { ["objectId"] = objectId ?? "" };
            var range = TableRange(rowIndex, columnIndex, rowSpan, columnSpan);
            if (range != null) inner["tableRange"] = range;
            inner["tableCellProperties"] = props;
            inner["fields"] = string.Join(",", fields);
            return new JObject { ["updateTableCellProperties"] = inner }.ToString(Formatting.None);
        }

        // border colour / weight / dash on one side (ALL, OUTER, INNER, TOP, BOTTOM, LEFT, RIGHT,
        // INNER_HORIZONTAL, INNER_VERTICAL) of a range (whole table when row/col < 0)
        public static string BuildUpdateTableBorderProperties(string objectId, int rowIndex, int columnIndex, int rowSpan, int columnSpan,
            string borderPosition, string colorJson, double? weightEmu, string dashStyle)
        {
            var props = new JObject();
            var fields = new List<string>();

            if (!string.IsNullOrWhiteSpace(colorJson)) { props["tableBorderFill"] = SolidFill(colorJson, 1); fields.Add("tableBorderFill.solidFill.color"); }
            if (weightEmu.HasValue && weightEmu.Value > 0) { props["weight"] = Emu(weightEmu.Value); fields.Add("weight"); }
            if (!string.IsNullOrWhiteSpace(dashStyle)) { props["dashStyle"] = Upper(dashStyle); fields.Add("dashStyle"); }
            if (fields.Count == 0) return null;

            var inner = new JObject { ["objectId"] = objectId ?? "" };
            var range = TableRange(rowIndex, columnIndex, rowSpan, columnSpan);
            if (range != null) inner["tableRange"] = range;
            inner["borderPosition"] = string.IsNullOrWhiteSpace(borderPosition) ? "ALL" : Upper(borderPosition);
            inner["tableBorderProperties"] = props;
            inner["fields"] = string.Join(",", fields);
            return new JObject { ["updateTableBorderProperties"] = inner }.ToString(Formatting.None);
        }

        public static string BuildMergeTableCells(string objectId, int rowIndex, int columnIndex, int rowSpan, int columnSpan)
            => new JObject
            {
                ["mergeTableCells"] = new JObject
                {
                    ["objectId"] = objectId ?? "",
                    ["tableRange"] = TableRange(Math.Max(0, rowIndex), Math.Max(0, columnIndex), rowSpan, columnSpan)
                }
            }.ToString(Formatting.None);

        public static string BuildUnmergeTableCells(string objectId, int rowIndex, int columnIndex, int rowSpan, int columnSpan)
            => new JObject
            {
                ["unmergeTableCells"] = new JObject
                {
                    ["objectId"] = objectId ?? "",
                    ["tableRange"] = TableRange(Math.Max(0, rowIndex), Math.Max(0, columnIndex), rowSpan, columnSpan)
                }
            }.ToString(Formatting.None);

        public static string BuildUpdateTableColumnProperties(string objectId, IEnumerable<int> columnIndices, double columnWidthEmu)
        {
            var inner = new JObject
            {
                ["objectId"] = objectId ?? "",
                ["columnIndices"] = new JArray(columnIndices ?? new int[0]),
                ["tableColumnProperties"] = new JObject { ["columnWidth"] = Emu(columnWidthEmu) },
                ["fields"] = "columnWidth"
            };
            return new JObject { ["updateTableColumnProperties"] = inner }.ToString(Formatting.None);
        }

        public static string BuildUpdateTableRowProperties(string objectId, IEnumerable<int> rowIndices, double minRowHeightEmu)
        {
            var inner = new JObject
            {
                ["objectId"] = objectId ?? "",
                ["rowIndices"] = new JArray(rowIndices ?? new int[0]),
                ["tableRowProperties"] = new JObject { ["minRowHeight"] = Emu(minRowHeightEmu) },
                ["fields"] = "minRowHeight"
            };
            return new JObject { ["updateTableRowProperties"] = inner }.ToString(Formatting.None);
        }

        #endregion

        #region Styled text box assembly

        // One box, many fragments: joins the fragments (with separator) into one insertText and then
        // styles each fragment's character range with its own TextStyleSpec — the ranges are computed
        // here from the fragment lengths, so a Grasshopper user never touches an index. Feed one
        // letter per fragment with a sampled Font Size list for a wave, or one word per fragment with
        // a Gradient into Text Color. Consecutive fragments with identical styles are still emitted
        // separately (simple, and the API doesn't mind). Paragraph/bullet/shape styles apply to the
        // whole box.
        public static List<string> AssembleStyledTextRuns(string pageId, IList<string> fragments, string separator,
            double widthEmu, double heightEmu, double xEmu, double yEmu, string objectId,
            IList<TextStyleSpec> runStyles, ParagraphStyleSpec paragraphStyle, string bulletPreset, ShapeStyleSpec shapeStyle,
            RequestBuilders.Placement placement = null)
        {
            var requests = new List<string>
            {
                RequestBuilders.BuildCreateShape(pageId, "TEXT_BOX", widthEmu, heightEmu, xEmu, yEmu, objectId, placement)
            };
            separator = separator ?? "";
            var joined = new System.Text.StringBuilder();
            var starts = new List<int>();
            var ends = new List<int>();
            for (int i = 0; i < (fragments?.Count ?? 0); i++)
            {
                string f = fragments[i] ?? "";
                if (i > 0) joined.Append(separator);
                starts.Add(joined.Length);
                joined.Append(f);
                ends.Add(joined.Length);
            }
            string text = joined.ToString();
            if (text.Length == 0)
            {
                string onlyShape = BuildUpdateShapeProperties(objectId, shapeStyle);
                if (onlyShape != null) requests.Add(onlyShape);
                return requests;
            }
            requests.Add(RequestBuilders.BuildInsertText(objectId, text, 0));

            for (int i = 0; i < starts.Count; i++)
            {
                if (ends[i] <= starts[i]) continue;                    // empty fragment: nothing to style
                var spec = runStyles != null && i < runStyles.Count ? runStyles[i] : null;
                string ts = BuildUpdateTextStyle(objectId, spec, starts[i], ends[i]);
                if (ts != null) requests.Add(ts);
            }
            string ps = BuildUpdateParagraphStyle(objectId, paragraphStyle);
            if (ps != null) requests.Add(ps);
            if (!string.IsNullOrWhiteSpace(bulletPreset)) requests.Add(BuildCreateParagraphBullets(objectId, bulletPreset));
            string ss = BuildUpdateShapeProperties(objectId, shapeStyle);
            if (ss != null) requests.Add(ss);
            return requests;
        }

        // createShape(TEXT_BOX) + insertText + optional updateTextStyle / updateParagraphStyle /
        // createParagraphBullets / updateShapeProperties, in the order the API needs them (the text
        // must exist before it can be styled). The Slides counterpart to DocsConnect's Styled Text —
        // one component from "a string and a place on the slide" to a finished, styled text box.
        public static List<string> AssembleStyledTextBox(string pageId, string text, double widthEmu, double heightEmu, double xEmu, double yEmu,
            string objectId, TextStyleSpec textStyle, ParagraphStyleSpec paragraphStyle, string bulletPreset, ShapeStyleSpec shapeStyle,
            RequestBuilders.Placement placement = null)
        {
            var requests = new List<string>
            {
                RequestBuilders.BuildCreateShape(pageId, "TEXT_BOX", widthEmu, heightEmu, xEmu, yEmu, objectId, placement)
            };
            if (!string.IsNullOrEmpty(text)) requests.Add(RequestBuilders.BuildInsertText(objectId, text, 0));

            string ts = BuildUpdateTextStyle(objectId, textStyle);
            if (ts != null && !string.IsNullOrEmpty(text)) requests.Add(ts);
            string ps = BuildUpdateParagraphStyle(objectId, paragraphStyle);
            if (ps != null && !string.IsNullOrEmpty(text)) requests.Add(ps);
            if (!string.IsNullOrWhiteSpace(bulletPreset) && !string.IsNullOrEmpty(text)) requests.Add(BuildCreateParagraphBullets(objectId, bulletPreset));
            string ss = BuildUpdateShapeProperties(objectId, shapeStyle);
            if (ss != null) requests.Add(ss);
            return requests;
        }

        #endregion
    }
}
