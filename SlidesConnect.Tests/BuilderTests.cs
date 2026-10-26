using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json.Linq;
using SlidesConnect;

namespace SlidesConnect.Tests
{
    // Proves each RequestBuilders method emits JSON matching the validated batchUpdate request shapes.
    public class BuilderTests
    {
        public static IEnumerable<object[]> Cases()
        {
            yield return Case(RequestBuilders.BuildCreateSlide("TITLE", 2, "s1"),
                "createSlide", new[] { "createSlide.slideLayoutReference.predefinedLayout", "createSlide.objectId", "createSlide.insertionIndex" });

            yield return Case(RequestBuilders.BuildUpdatePageProperties("p", "{\"rgbColor\":{\"red\":0.1,\"green\":0.2,\"blue\":0.3}}"),
                "updatePageProperties", new[] { "updatePageProperties.objectId", "updatePageProperties.pageProperties.pageBackgroundFill.solidFill.color.rgbColor.red", "updatePageProperties.fields" });

            yield return Case(RequestBuilders.BuildDeleteObject("x1"),
                "deleteObject", new[] { "deleteObject.objectId" });

            yield return Case(RequestBuilders.BuildCreateShape("p", "RECTANGLE", 100, 200, 10, 20, "sh1"),
                "createShape", new[] { "createShape.shapeType", "createShape.elementProperties.pageObjectId", "createShape.elementProperties.size.width.magnitude", "createShape.elementProperties.transform.translateX" });

            yield return Case(RequestBuilders.BuildUpdateShapeProperties("sh1", "{\"rgbColor\":{\"red\":1,\"green\":0,\"blue\":0}}", 9525),
                "updateShapeProperties", new[] { "updateShapeProperties.objectId", "updateShapeProperties.shapeProperties.shapeBackgroundFill.solidFill.color.rgbColor.red", "updateShapeProperties.fields" });

            yield return Case(RequestBuilders.BuildUpdateTransform("sh1", 5, 6, 1, 1, 0),
                "updatePageElementTransform", new[] { "updatePageElementTransform.objectId", "updatePageElementTransform.applyMode", "updatePageElementTransform.transform.translateX" });

            yield return Case(RequestBuilders.BuildInsertText("sh1", "hello", 0),
                "insertText", new[] { "insertText.objectId", "insertText.text", "insertText.insertionIndex" });

            yield return Case(RequestBuilders.BuildReplaceAllText("{{x}}", "Y", true),
                "replaceAllText", new[] { "replaceAllText.containsText.text", "replaceAllText.containsText.matchCase", "replaceAllText.replaceText" });

            yield return Case(RequestBuilders.BuildUpdateTextStyle("sh1", true, false, 18, "Arial", "{\"rgbColor\":{\"red\":0,\"green\":0,\"blue\":1}}"),
                "updateTextStyle", new[] { "updateTextStyle.style.bold", "updateTextStyle.style.fontSize.magnitude", "updateTextStyle.style.foregroundColor.opaqueColor.rgbColor.blue", "updateTextStyle.fields" });

            yield return Case(RequestBuilders.BuildUpdateParagraphStyle("sh1", "CENTER"),
                "updateParagraphStyle", new[] { "updateParagraphStyle.style.alignment", "updateParagraphStyle.fields" });

            yield return Case(RequestBuilders.BuildCreateTable("p", 3, 4, "t1"),
                "createTable", new[] { "createTable.rows", "createTable.columns", "createTable.elementProperties.pageObjectId" });

            yield return Case(RequestBuilders.BuildCreateImage("p", "https://x/y.png", 100, 100, 0, 0, "im1"),
                "createImage", new[] { "createImage.url", "createImage.elementProperties.pageObjectId" });

            yield return Case(RequestBuilders.BuildReplaceAllShapesWithImage("{{img}}", "https://x/y.png", false),
                "replaceAllShapesWithImage", new[] { "replaceAllShapesWithImage.imageUrl", "replaceAllShapesWithImage.containsText.text" });

            yield return Case(RequestBuilders.BuildCreateSlide("BLANK", null, "s1", "layout-42"),
                "createSlide", new[] { "createSlide.slideLayoutReference.layoutId", "createSlide.objectId" });

            yield return Case(RequestBuilders.BuildReplaceAllText("{{x}}", "Y", false, new[] { "page1" }),
                "replaceAllText", new[] { "replaceAllText.pageObjectIds" });

            yield return Case(RequestBuilders.BuildReplaceAllShapesWithImage("{{img}}", "https://x/y.png", false, new[] { "page1" }),
                "replaceAllShapesWithImage", new[] { "replaceAllShapesWithImage.pageObjectIds" });
        }

        private static object[] Case(string json, string topKey, string[] paths)
            => new object[] { json, topKey, paths };

        [Theory]
        [MemberData(nameof(Cases))]
        public void Builder_EmitsExpectedShape(string json, string topKey, string[] requiredPaths)
        {
            var o = JObject.Parse(json);
            Assert.True(o.ContainsKey(topKey), $"missing top key: {topKey}");
            foreach (var path in requiredPaths)
                Assert.True(o.SelectToken(path) != null, $"missing path: {path}");
        }

        [Fact]
        public void Builders_NullAndEmptyInputs_DoNotThrow()
        {
            // builders null-coalesce strings — none of these should throw
            RequestBuilders.BuildCreateSlide(null, null, null);
            RequestBuilders.BuildDeleteObject(null);
            RequestBuilders.BuildInsertText(null, null, -5);
            RequestBuilders.BuildReplaceAllText(null, null, false);
            RequestBuilders.BuildUpdateShapeProperties(null, null, null);
            RequestBuilders.BuildUpdateTextStyle(null, false, false, 0, null, null);
            RequestBuilders.BuildCreateImage(null, null, 0, 0, 0, 0, null);
        }

        [Fact]
        public void UpdateTextStyle_NullBoldItalic_LeftOutOfFieldMask()
        {
            var o = JObject.Parse(RequestBuilders.BuildUpdateTextStyle("s", null, null, 24, "", ""));
            string fields = (string)o.SelectToken("updateTextStyle.fields")!;
            Assert.Equal("fontSize", fields);
            Assert.Null(o.SelectToken("updateTextStyle.style.bold"));
        }

        [Fact]
        public void UpdateTextStyle_NothingSet_ReturnsNull()
        {
            Assert.Null(RequestBuilders.BuildUpdateTextStyle("s", null, null, 0, "", ""));
        }

        [Fact]
        public void UpdateShapeProperties_NothingSet_ReturnsNull_NeverStarMask()
        {
            Assert.Null(RequestBuilders.BuildUpdateShapeProperties("s", "", null));
            var o = JObject.Parse(RequestBuilders.BuildUpdateShapeProperties("s", "", 9525));
            Assert.Equal("outline.weight", (string)o.SelectToken("updateShapeProperties.fields")!);
        }

        [Fact]
        public void UpdateTransform_Relative_IsPureTranslation()
        {
            var o = JObject.Parse(RequestBuilders.BuildUpdateTransform("s", 100, 200, 2, 3, 45, relative: true));
            Assert.Equal("RELATIVE", (string)o.SelectToken("updatePageElementTransform.applyMode")!);
            Assert.Equal(1.0, (double)o.SelectToken("updatePageElementTransform.transform.scaleX")!);
            Assert.Equal(1.0, (double)o.SelectToken("updatePageElementTransform.transform.scaleY")!);
            Assert.Null(o.SelectToken("updatePageElementTransform.transform.shearX"));
            Assert.Equal(100.0, (double)o.SelectToken("updatePageElementTransform.transform.translateX")!);
        }

        [Fact]
        public void CreatedIds_CollectsNewObjectsAndPlaceholderCopies()
        {
            var reqs = new List<string>
            {
                RequestBuilders.BuildCreateSlide("BLANK", null, "slide_1", null, new[] { "ph_layout" }, new[] { "title_1" }),
                RequestBuilders.BuildCreateShape("slide_1", "TEXT_BOX", 1, 1, 0, 0, "shape_1"),
                RequestBuilders.BuildInsertText("shape_1", "hi", 0),
            };
            var ids = RequestBuilders.CreatedIds(reqs);
            Assert.Equal(new HashSet<string> { "slide_1", "title_1", "shape_1" }, ids);
        }

        [Fact]
        public void RemapCreatedIds_SwapsAlreadySentIdsConsistentlyAcrossBatch()
        {
            var reqs = new List<string>
            {
                RequestBuilders.BuildCreateSlide("BLANK", null, "slide_abcdef0123"),
                RequestBuilders.BuildCreateShape("slide_abcdef0123", "TEXT_BOX", 1, 1, 0, 0, "shape_0123456789"),
                RequestBuilders.BuildInsertText("shape_0123456789", "hi", 0),
            };
            var sent = new HashSet<string> { "slide_abcdef0123", "shape_0123456789" };
            var outReqs = RequestBuilders.RemapCreatedIds(reqs, sent, out var map);

            Assert.Equal(2, map.Count);
            string newSlide = map["slide_abcdef0123"], newShape = map["shape_0123456789"];
            Assert.StartsWith("slide_", newSlide); Assert.NotEqual("slide_abcdef0123", newSlide);
            Assert.Equal(newSlide, (string)JObject.Parse(outReqs[0]).SelectToken("createSlide.objectId")!);
            Assert.Equal(newSlide, (string)JObject.Parse(outReqs[1]).SelectToken("createShape.elementProperties.pageObjectId")!);
            Assert.Equal(newShape, (string)JObject.Parse(outReqs[1]).SelectToken("createShape.objectId")!);
            Assert.Equal(newShape, (string)JObject.Parse(outReqs[2]).SelectToken("insertText.objectId")!);
            Assert.DoesNotContain("slide_abcdef0123", string.Join("", outReqs));
        }

        [Fact]
        public void RemapCreatedIds_NothingSent_LeavesBatchUntouched()
        {
            var reqs = new List<string> { RequestBuilders.BuildCreateSlide("BLANK", null, "slide_1") };
            var outReqs = RequestBuilders.RemapCreatedIds(reqs, new HashSet<string>(), out var map);
            Assert.Empty(map);
            Assert.Equal(reqs, outReqs);
        }

        [Fact]
        public void UpdateTextStyle_OmitsUnsetFontFields()
        {
            // fontSize 0 and empty fontFamily should be left out of the field mask
            var o = JObject.Parse(RequestBuilders.BuildUpdateTextStyle("s", false, false, 0, "", ""));
            string fields = (string)o.SelectToken("updateTextStyle.fields")!;
            Assert.DoesNotContain("fontSize", fields);
            Assert.DoesNotContain("fontFamily", fields);
            Assert.Contains("bold", fields);
        }

        [Fact]
        public void EnsureId_BlankGeneratesUniquePrefixedId()
        {
            string a = RequestBuilders.EnsureId("", "slide");
            string b = RequestBuilders.EnsureId("   ", "slide");
            Assert.StartsWith("slide_", a);
            Assert.StartsWith("slide_", b);
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void EnsureId_NonBlankPassesThroughTrimmed()
        {
            Assert.Equal("my-id", RequestBuilders.EnsureId("  my-id  ", "shape"));
        }

        [Fact]
        public void BuildCreateSlide_LayoutIdWinsOverPredefinedLayout()
        {
            var o = JObject.Parse(RequestBuilders.BuildCreateSlide("TITLE", null, "s1", "layout-42"));
            Assert.Equal("layout-42", (string)o.SelectToken("createSlide.slideLayoutReference.layoutId")!);
            Assert.Null(o.SelectToken("createSlide.slideLayoutReference.predefinedLayout"));
        }

        [Fact]
        public void BuildReplaceAllText_NoPageObjectIds_OmitsField()
        {
            var o = JObject.Parse(RequestBuilders.BuildReplaceAllText("{{x}}", "y", false));
            Assert.Null(o.SelectToken("replaceAllText.pageObjectIds"));
        }

        [Fact]
        public void BuildCreateSlide_PlaceholderIdMappings_ZipsPairsAndSkipsBlank()
        {
            var layoutIds = new List<string> { "layoutTitle1", "", "layoutPic1" };
            var newIds = new List<string> { "title_s1", "body_s1", "pic_s1" };

            var o = JObject.Parse(RequestBuilders.BuildCreateSlide("BLANK", null, "s1", "layout-42", layoutIds, newIds));
            var mappings = (JArray)o.SelectToken("createSlide.placeholderIdMappings")!;

            Assert.Equal(2, mappings.Count); // the blank layoutId pair is skipped
            Assert.Equal("layoutTitle1", (string)mappings[0]["layoutPlaceholderObjectId"]!);
            Assert.Equal("title_s1", (string)mappings[0]["objectId"]!);
            Assert.Equal("layoutPic1", (string)mappings[1]["layoutPlaceholderObjectId"]!);
            Assert.Equal("pic_s1", (string)mappings[1]["objectId"]!);
        }

        [Fact]
        public void BuildCreateSlide_NoPlaceholderMappings_OmitsField()
        {
            var o = JObject.Parse(RequestBuilders.BuildCreateSlide("BLANK", null, "s1"));
            Assert.Null(o.SelectToken("createSlide.placeholderIdMappings"));
        }

        [Fact]
        public void ColorJson_ConvertsByteRgbToUnitScale()
        {
            var o = JObject.Parse(RequestBuilders.ColorJson(255, 0, 128));
            Assert.Equal(1.0, (double)o.SelectToken("rgbColor.red")!);
            Assert.Equal(0.0, (double)o.SelectToken("rgbColor.green")!);
            Assert.Equal(0.502, (double)o.SelectToken("rgbColor.blue")!, 3);
        }

        [Fact]
        public void ColorJson_RoundTripsThroughShapeFill()
        {
            var o = JObject.Parse(RequestBuilders.BuildUpdateShapeProperties("s1", RequestBuilders.ColorJson(0, 255, 0), null));
            Assert.Equal(1.0, (double)o.SelectToken("updateShapeProperties.shapeProperties.shapeBackgroundFill.solidFill.color.rgbColor.green")!);
        }

        [Fact]
        public void BuildReplaceImage_EmitsExpectedShape()
        {
            var o = JObject.Parse(RequestBuilders.BuildReplaceImage("pic_s1", "https://x/y.png"));
            Assert.Equal("pic_s1", (string)o.SelectToken("replaceImage.imageObjectId")!);
            Assert.Equal("https://x/y.png", (string)o.SelectToken("replaceImage.url")!);
            Assert.Null(o.SelectToken("replaceImage.objectId"));
            Assert.Null(o.SelectToken("replaceImage.imageUri"));
            Assert.Equal("CENTER_INSIDE", (string)o.SelectToken("replaceImage.imageReplaceMethod")!);
        }
    }
}
