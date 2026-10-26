using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SlidesConnect;
using Xunit;

namespace SlidesConnect.Tests
{
    // StyleBuilders: the text / paragraph / shape / table styling requests behind the Text Style,
    // Shapes and Tables tabs. Field names and enum spellings here were checked against the Slides
    // API reference (presentations/request, pages/text, pages/other, pages/tables) on 2026-09-15.
    public class StyleBuilderTests
    {
        private static JObject J(string s) => JObject.Parse(s);

        [Fact]
        public void TextRange_AllFromStartFixed()
        {
            Assert.Equal("ALL", (string)StyleBuilders.TextRange(-1, -1)["type"]!);
            var from = StyleBuilders.TextRange(3, -1);
            Assert.Equal("FROM_START_INDEX", (string)from["type"]!); Assert.Equal(3, (int)from["startIndex"]!);
            var fixedR = StyleBuilders.TextRange(2, 7);
            Assert.Equal("FIXED_RANGE", (string)fixedR["type"]!); Assert.Equal(7, (int)fixedR["endIndex"]!);
        }

        [Fact]
        public void TextStyle_AllTraits_ExactFieldNames()
        {
            var spec = new StyleBuilders.TextStyleSpec
            {
                Bold = true, Italic = false, Underline = true, Strikethrough = true, SmallCaps = true,
                FontSizePt = 24, FontFamily = "Roboto",
                TextColorJson = RequestBuilders.ColorJson(255, 0, 0), HighlightColorJson = RequestBuilders.ColorJson(255, 255, 0),
                LinkUrl = "https://example.com", Baseline = "superscript"
            };
            var o = J(StyleBuilders.BuildUpdateTextStyle("s1", spec, 2, 5, 1, 0)!);
            var inner = o["updateTextStyle"]!;
            Assert.Equal("s1", (string)inner["objectId"]!);
            Assert.Equal(1, (int)inner["cellLocation"]!["rowIndex"]!);
            Assert.Equal("FIXED_RANGE", (string)inner["textRange"]!["type"]!);
            Assert.Equal("bold,italic,underline,strikethrough,smallCaps,fontSize,fontFamily,foregroundColor,backgroundColor,link,baselineOffset", (string)inner["fields"]!);
            Assert.Equal(24.0, (double)inner["style"]!["fontSize"]!["magnitude"]!);
            Assert.Equal("PT", (string)inner["style"]!["fontSize"]!["unit"]!);
            Assert.Equal(1.0, (double)inner["style"]!["foregroundColor"]!["opaqueColor"]!["rgbColor"]!["red"]!);
            Assert.Equal(1.0, (double)inner["style"]!["backgroundColor"]!["opaqueColor"]!["rgbColor"]!["green"]!);
            Assert.Equal("https://example.com", (string)inner["style"]!["link"]!["url"]!);
            Assert.Equal("SUPERSCRIPT", (string)inner["style"]!["baselineOffset"]!);
        }

        [Fact]
        public void TextStyle_OnlyConnectedTraitsInMask_EmptyReturnsNull()
        {
            var o = J(StyleBuilders.BuildUpdateTextStyle("s", new StyleBuilders.TextStyleSpec { Underline = true })!);
            Assert.Equal("underline", (string)o["updateTextStyle"]!["fields"]!);
            Assert.Null(o["updateTextStyle"]!["cellLocation"]);
            Assert.Null(StyleBuilders.BuildUpdateTextStyle("s", new StyleBuilders.TextStyleSpec()));
            Assert.Null(StyleBuilders.BuildUpdateTextStyle("s", new StyleBuilders.TextStyleSpec { FontSizePt = 0 }));
        }

        [Fact]
        public void ParagraphStyle_AllTraits_LineSpacingIsPlainNumber()
        {
            var spec = new StyleBuilders.ParagraphStyleSpec
            {
                Alignment = "center", LineSpacingPercent = 115, IndentStartPt = 18, IndentEndPt = 6, IndentFirstLinePt = 12,
                SpaceAbovePt = 4, SpaceBelowPt = 8, Direction = "RIGHT_TO_LEFT"
            };
            var inner = J(StyleBuilders.BuildUpdateParagraphStyle("s", spec)!)["updateParagraphStyle"]!;
            Assert.Equal("CENTER", (string)inner["style"]!["alignment"]!);
            Assert.Equal(115.0, (double)inner["style"]!["lineSpacing"]!);          // number, not a Dimension
            Assert.Equal("PT", (string)inner["style"]!["indentStart"]!["unit"]!);
            Assert.Equal("alignment,lineSpacing,indentStart,indentEnd,indentFirstLine,spaceAbove,spaceBelow,direction", (string)inner["fields"]!);
            Assert.Equal("ALL", (string)inner["textRange"]!["type"]!);
            Assert.Null(StyleBuilders.BuildUpdateParagraphStyle("s", new StyleBuilders.ParagraphStyleSpec()));
        }

        [Fact]
        public void Bullets_CreateAndDelete()
        {
            var c = J(StyleBuilders.BuildCreateParagraphBullets("s", "numbered_digit_alpha_roman", -1, -1, 0, 2))["createParagraphBullets"]!;
            Assert.Equal("NUMBERED_DIGIT_ALPHA_ROMAN", (string)c["bulletPreset"]!);
            Assert.Equal(2, (int)c["cellLocation"]!["columnIndex"]!);
            var d = J(StyleBuilders.BuildDeleteParagraphBullets("s"))["deleteParagraphBullets"]!;
            Assert.Equal("ALL", (string)d["textRange"]!["type"]!);
        }

        [Fact]
        public void ShapeStyle_FullSpec_MasksEverySetProperty()
        {
            var spec = new StyleBuilders.ShapeStyleSpec
            {
                FillColorJson = RequestBuilders.ColorJson(0, 0, 255), FillAlpha = 0.5,
                OutlineColorJson = RequestBuilders.ColorJson(0, 0, 0), OutlineWeightEmu = 12700, OutlineDash = "dash",
                Shadow = true, ShadowAlpha = 0.3, ShadowBlurEmu = 50000,
                ContentAlignment = "middle", Autofit = "TEXT_AUTOFIT", LinkUrl = "https://x"
            };
            var inner = J(StyleBuilders.BuildUpdateShapeProperties("s", spec)!)["updateShapeProperties"]!;
            string fields = (string)inner["fields"]!;
            foreach (var f in new[] { "shapeBackgroundFill.solidFill.color", "shapeBackgroundFill.solidFill.alpha", "outline.outlineFill.solidFill.color",
                                      "outline.weight", "outline.dashStyle", "shadow", "contentAlignment", "autofit.autofitType", "link" })
                Assert.Contains(f, fields);
            var props = inner["shapeProperties"]!;
            Assert.Equal(0.5, (double)props["shapeBackgroundFill"]!["solidFill"]!["alpha"]!);
            Assert.Equal("DASH", (string)props["outline"]!["dashStyle"]!);
            Assert.Equal("OUTER", (string)props["shadow"]!["type"]!);
            Assert.Equal("RENDERED", (string)props["shadow"]!["propertyState"]!);
            Assert.Equal(0.3, (double)props["shadow"]!["alpha"]!);
            Assert.Equal("MIDDLE", (string)props["contentAlignment"]!);
            Assert.Equal("TEXT_AUTOFIT", (string)props["autofit"]!["autofitType"]!);
        }

        [Fact]
        public void ShapeStyle_ShadowOff_UsesPropertyState_AndEmptyIsNull()
        {
            var inner = J(StyleBuilders.BuildUpdateShapeProperties("s", new StyleBuilders.ShapeStyleSpec { Shadow = false })!)["updateShapeProperties"]!;
            Assert.Equal("shadow.propertyState", (string)inner["fields"]!);
            Assert.Equal("NOT_RENDERED", (string)inner["shapeProperties"]!["shadow"]!["propertyState"]!);
            Assert.Null(StyleBuilders.BuildUpdateShapeProperties("s", new StyleBuilders.ShapeStyleSpec()));
        }

        [Fact]
        public void LegacyShapeProperties_StillDelegates()
        {
            var o = J(RequestBuilders.BuildUpdateShapeProperties("s", RequestBuilders.ColorJson(0, 255, 0), 9525)!);
            Assert.Equal("shapeBackgroundFill.solidFill.color,outline.weight", (string)o["updateShapeProperties"]!["fields"]!);
        }

        [Fact]
        public void TableCell_RangeOmittedForWholeTable_PresentForSubset()
        {
            var whole = J(StyleBuilders.BuildUpdateTableCellProperties("t", -1, -1, 1, 1, RequestBuilders.ColorJson(200, 200, 200), null, "MIDDLE")!)["updateTableCellProperties"]!;
            Assert.Null(whole["tableRange"]);
            Assert.Equal("tableCellBackgroundFill.solidFill.color,contentAlignment", (string)whole["fields"]!);

            var sub = J(StyleBuilders.BuildUpdateTableCellProperties("t", 0, 0, 1, 3, RequestBuilders.ColorJson(1, 2, 3), null, "")!)["updateTableCellProperties"]!;
            Assert.Equal(0, (int)sub["tableRange"]!["location"]!["rowIndex"]!);
            Assert.Equal(3, (int)sub["tableRange"]!["columnSpan"]!);
            Assert.Null(StyleBuilders.BuildUpdateTableCellProperties("t", 0, 0, 1, 1, "", null, ""));
        }

        [Fact]
        public void TableBorders_MergeUnmerge_ColumnRow()
        {
            var b = J(StyleBuilders.BuildUpdateTableBorderProperties("t", -1, -1, 1, 1, "outer", RequestBuilders.ColorJson(0, 0, 0), 12700, "solid")!)["updateTableBorderProperties"]!;
            Assert.Equal("OUTER", (string)b["borderPosition"]!);
            Assert.Equal("tableBorderFill.solidFill.color,weight,dashStyle", (string)b["fields"]!);
            Assert.Equal(12700.0, (double)b["tableBorderProperties"]!["weight"]!["magnitude"]!);

            var m = J(StyleBuilders.BuildMergeTableCells("t", 0, 0, 1, 4))["mergeTableCells"]!;
            Assert.Equal(4, (int)m["tableRange"]!["columnSpan"]!);
            Assert.NotNull(J(StyleBuilders.BuildUnmergeTableCells("t", 0, 0, 1, 4))["unmergeTableCells"]);

            var c = J(StyleBuilders.BuildUpdateTableColumnProperties("t", new[] { 0, 2 }, 914400))["updateTableColumnProperties"]!;
            Assert.Equal(new[] { 0, 2 }, c["columnIndices"]!.ToObject<int[]>());
            Assert.Equal("columnWidth", (string)c["fields"]!);
            var r = J(StyleBuilders.BuildUpdateTableRowProperties("t", new[] { 1 }, 457200))["updateTableRowProperties"]!;
            Assert.Equal("minRowHeight", (string)r["fields"]!);
        }

        [Fact]
        public void StyledTextRuns_ComputesRangesFromFragments_NeverFromUser()
        {
            // "Hi" + "there" joined with " " -> "Hi there": ranges [0,2) and [3,8)
            var runs = new List<StyleBuilders.TextStyleSpec>
            {
                new StyleBuilders.TextStyleSpec { FontSizePt = 12 },
                new StyleBuilders.TextStyleSpec { FontSizePt = 30, TextColorJson = RequestBuilders.ColorJson(255, 0, 0) },
            };
            var reqs = StyleBuilders.AssembleStyledTextRuns("s1", new[] { "Hi", "there" }, " ", 100, 50, 0, 0, "box", runs, null, "", null);
            Assert.Equal(4, reqs.Count);                                   // createShape, insertText, 2 x updateTextStyle
            Assert.Equal("Hi there", (string)J(reqs[1])["insertText"]!["text"]!);
            var r0 = J(reqs[2])["updateTextStyle"]!;
            var r1 = J(reqs[3])["updateTextStyle"]!;
            Assert.Equal("FIXED_RANGE", (string)r0["textRange"]!["type"]!);
            Assert.Equal(0, (int)r0["textRange"]!["startIndex"]!); Assert.Equal(2, (int)r0["textRange"]!["endIndex"]!);
            Assert.Equal(3, (int)r1["textRange"]!["startIndex"]!); Assert.Equal(8, (int)r1["textRange"]!["endIndex"]!);
            Assert.Equal(30.0, (double)r1["style"]!["fontSize"]!["magnitude"]!);
        }

        [Fact]
        public void StyledTextRuns_LetterByLetter_OneRangePerLetter_EmptySeparator()
        {
            var letters = new[] { "w", "a", "v", "e" };
            var runs = new List<StyleBuilders.TextStyleSpec>();
            for (int i = 0; i < 4; i++) runs.Add(new StyleBuilders.TextStyleSpec { FontSizePt = 10 + i * 5 });
            var reqs = StyleBuilders.AssembleStyledTextRuns("s1", letters, "", 100, 50, 0, 0, "box", runs, null, "", null);
            Assert.Equal("wave", (string)J(reqs[1])["insertText"]!["text"]!);
            Assert.Equal(6, reqs.Count);
            for (int i = 0; i < 4; i++)
            {
                var r = J(reqs[2 + i])["updateTextStyle"]!;
                Assert.Equal(i, (int)r["textRange"]!["startIndex"]!); Assert.Equal(i + 1, (int)r["textRange"]!["endIndex"]!);
            }
            // a fragment with no style set emits no request, but still occupies its characters
            var mixed = StyleBuilders.AssembleStyledTextRuns("s1", new[] { "a", "b" }, "", 1, 1, 0, 0, "box",
                new List<StyleBuilders.TextStyleSpec> { null, new StyleBuilders.TextStyleSpec { Bold = true } }, null, "", null);
            Assert.Equal(3, mixed.Count);
            Assert.Equal(1, (int)J(mixed[2])["updateTextStyle"]!["textRange"]!["startIndex"]!);
        }

        [Fact]
        public void RotatedTransform_KeepsCentre_ZeroIsPlainTranslate()
        {
            var plain = RequestBuilders.RotatedTransform(100, 200, 400, 200, 0);
            Assert.Equal(1.0, (double)plain["scaleX"]!); Assert.Null(plain["shearX"]);
            Assert.Equal(100.0, (double)plain["translateX"]!);

            // 90 degrees clockwise about the centre (300, 300): the rotated top-left lands at centre - R*(w/2, h/2)
            var r = RequestBuilders.RotatedTransform(100, 200, 400, 200, 90);
            Assert.Equal(0.0, (double)r["scaleX"]!, 9);
            Assert.Equal(-1.0, (double)r["shearX"]!, 9);
            Assert.Equal(1.0, (double)r["shearY"]!, 9);
            // R*(200,100) with cos=0, sin=1 = (-100, 200) -> top-left = (300+100, 300-200)
            Assert.Equal(400.0, (double)r["translateX"]!, 6);
            Assert.Equal(100.0, (double)r["translateY"]!, 6);

            var shape = J(RequestBuilders.BuildCreateShape("p", "RECTANGLE", 400, 200, 100, 200, "id", 90));
            Assert.Equal(-1.0, (double)shape.SelectToken("createShape.elementProperties.transform.shearX")!, 9);
            Assert.Equal(400.0, (double)shape.SelectToken("createShape.elementProperties.size.width.magnitude")!);
        }

        [Fact]
        public void ElementTransform_FlipX_MirrorsAboutCentre()
        {
            // element at (100,200) size 400x200: centre (300,300). Mirrored left-right, the visual box must not move:
            // the transform maps local (0,0)->(500,200) and local (400,0)->(100,200)
            var m = RequestBuilders.ElementTransform(100, 200, 400, 200, new RequestBuilders.Placement { FlipX = true });
            Assert.Equal(-1.0, (double)m["scaleX"]!, 9); Assert.Equal(1.0, (double)m["scaleY"]!, 9);
            Assert.Equal(0.0, (double)m["shearX"]!, 9); Assert.Equal(0.0, (double)m["shearY"]!, 9);
            Assert.Equal(500.0, (double)m["translateX"]!, 6); Assert.Equal(200.0, (double)m["translateY"]!, 6);

            var my = RequestBuilders.ElementTransform(100, 200, 400, 200, new RequestBuilders.Placement { FlipY = true });
            Assert.Equal(-1.0, (double)my["scaleY"]!, 9); Assert.Equal(400.0, (double)my["translateY"]!, 6); Assert.Equal(100.0, (double)my["translateX"]!, 6);
        }

        [Fact]
        public void ElementTransform_Skew_LeansWithoutDrifting()
        {
            // 45 degree X skew: shearX = tan(45) = 1; centre must stay at (300,300)
            var m = RequestBuilders.ElementTransform(100, 200, 400, 200, new RequestBuilders.Placement { SkewXDegrees = 45 });
            Assert.Equal(1.0, (double)m["shearX"]!, 9);
            Assert.Equal(1.0, (double)m["scaleX"]!, 9);
            double cx = (double)m["translateX"]! + (double)m["scaleX"]! * 200 + (double)m["shearX"]! * 100;
            double cy = (double)m["translateY"]! + (double)m["shearY"]! * 200 + (double)m["scaleY"]! * 100;
            Assert.Equal(300.0, cx, 6); Assert.Equal(300.0, cy, 6);
            // skew clamps rather than exploding at 90
            var big = RequestBuilders.ElementTransform(0, 0, 10, 10, new RequestBuilders.Placement { SkewXDegrees = 90 });
            Assert.True((double)big["shearX"]! < 20);
        }

        [Fact]
        public void ElementTransform_ScaleAndRotate_ComposeAboutCentre_IdentityIsTranslate()
        {
            var id = RequestBuilders.ElementTransform(7, 9, 10, 10, new RequestBuilders.Placement());
            Assert.Null(id["shearX"]); Assert.Equal(7.0, (double)id["translateX"]!);

            var m = RequestBuilders.ElementTransform(0, 0, 100, 50, new RequestBuilders.Placement { ScaleX = 2, ScaleY = 2, RotationDegrees = 180 });
            Assert.Equal(-2.0, (double)m["scaleX"]!, 9); Assert.Equal(-2.0, (double)m["scaleY"]!, 9);
            // centre of the original box is (50,25); doubled + rotated 180, the top-left lands at centre + (w, h/… ) = (150, 75)
            Assert.Equal(150.0, (double)m["translateX"]!, 6); Assert.Equal(75.0, (double)m["translateY"]!, 6);
        }

        [Fact]
        public void UpdateTransform_PivotsAboutCentreOnlyWhenSizeKnown()
        {
            var p = new RequestBuilders.Placement { FlipX = true };
            var known = J(RequestBuilders.BuildUpdateTransform("s", 100, 200, p, 400, 200))["updatePageElementTransform"]!;
            Assert.Equal("ABSOLUTE", (string)known["applyMode"]!);
            Assert.Equal(500.0, (double)known["transform"]!["translateX"]!, 6);
            var unknown = J(RequestBuilders.BuildUpdateTransform("s", 100, 200, p, 0, 0))["updatePageElementTransform"]!;
            Assert.Equal(-1.0, (double)unknown["transform"]!["scaleX"]!, 9);
            Assert.Equal(100.0, (double)unknown["transform"]!["translateX"]!, 6);   // API's own top-left pivot
        }

        [Fact]
        public void StyledTextBox_RequestOrder_CreateInsertStyleParagraphBulletsShape()
        {
            var reqs = StyleBuilders.AssembleStyledTextBox("slide_1", "a\nb", 100, 50, 10, 10, "box_1",
                new StyleBuilders.TextStyleSpec { Bold = true },
                new StyleBuilders.ParagraphStyleSpec { Alignment = "CENTER" },
                "BULLET_DISC_CIRCLE_SQUARE",
                new StyleBuilders.ShapeStyleSpec { ContentAlignment = "MIDDLE" });
            Assert.Equal(6, reqs.Count);
            Assert.Contains("createShape", reqs[0]); Assert.Contains("insertText", reqs[1]); Assert.Contains("updateTextStyle", reqs[2]);
            Assert.Contains("updateParagraphStyle", reqs[3]); Assert.Contains("createParagraphBullets", reqs[4]); Assert.Contains("updateShapeProperties", reqs[5]);
            Assert.Equal("box_1", (string)J(reqs[1])["insertText"]!["objectId"]!);

            // no text = no text/paragraph/bullet requests, but the box and its shape style still go out
            var bare = StyleBuilders.AssembleStyledTextBox("slide_1", "", 100, 50, 10, 10, "box_2",
                new StyleBuilders.TextStyleSpec { Bold = true }, null, "BULLET_DISC_CIRCLE_SQUARE", new StyleBuilders.ShapeStyleSpec { FillColorJson = RequestBuilders.ColorJson(1, 1, 1) });
            Assert.Equal(2, bare.Count);
        }
    }
}
