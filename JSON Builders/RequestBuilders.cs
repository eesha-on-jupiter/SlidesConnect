using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlidesConnect
{
    // Builds individual batchUpdate request objects as JSON. Pure Newtonsoft + BCL only — no Grasshopper.
    // Each method's shape matches the validated batchUpdate body in spec/call-library-validated.json.
    public static class RequestBuilders
    {
        #region Private helpers

        private static JObject Emu(double magnitude)
            => new JObject { ["magnitude"] = magnitude, ["unit"] = "EMU" };

        private static JObject Size(double width, double height)
            => new JObject { ["width"] = Emu(width), ["height"] = Emu(height) };

        private static JObject Translate(double x, double y, double scaleX = 1, double scaleY = 1)
            => new JObject
            {
                ["scaleX"] = scaleX,
                ["scaleY"] = scaleY,
                ["translateX"] = x,
                ["translateY"] = y,
                ["unit"] = "EMU"
            };

        private static JObject ElementProperties(string pageId, double width, double height, double x, double y, Placement placement = null)
            => new JObject
            {
                ["pageObjectId"] = pageId ?? "",
                ["size"] = Size(width, height),
                ["transform"] = ElementTransform(x, y, width, height, placement ?? new Placement())
            };

        // Everything a Slides AffineTransform can express beyond position, as a designer thinks of it:
        // rotate, mirror, skew, scale — all about the element's centre. Defaults = identity.
        public sealed class Placement
        {
            public double RotationDegrees;   // clockwise (screen-space, Y down)
            public bool FlipX, FlipY;        // mirror left-right / top-bottom
            public double SkewXDegrees;      // lean the top edge sideways (italic-style)
            public double SkewYDegrees;      // lean the left edge up/down
            public double ScaleX = 1, ScaleY = 1;

            public bool IsIdentity =>
                Math.Abs(RotationDegrees) < 1e-9 && !FlipX && !FlipY
                && Math.Abs(SkewXDegrees) < 1e-9 && Math.Abs(SkewYDegrees) < 1e-9
                && Math.Abs(ScaleX - 1) < 1e-12 && Math.Abs(ScaleY - 1) < 1e-12;
        }

        // A Slides transform is applied about the element's own origin (its top-left corner), which
        // is not what anyone drawing a rotated or mirrored rectangle means. This composes
        // A = Rotate * Skew * Flip * Scale and then places the element so its CENTRE stays where an
        // untransformed element at (x, y) with size (w, h) would have it — so a Rhino rectangle's
        // rotation from Rhino Shape Export lands on the slide the way it looks in Rhino, a flipped
        // shape stays in place, and a skewed one leans without drifting. Slides' matrix fields map
        // as scaleX = A00, shearX = A01, shearY = A10, scaleY = A11. Identity = a plain translate.
        public static JObject ElementTransform(double x, double y, double width, double height, Placement p)
        {
            if (p == null || p.IsIdentity) return Translate(x, y);
            double rad = p.RotationDegrees * Math.PI / 180.0;
            double c = Math.Cos(rad), s = Math.Sin(rad);
            double tsx = Math.Tan(ClampSkew(p.SkewXDegrees) * Math.PI / 180.0);
            double tsy = Math.Tan(ClampSkew(p.SkewYDegrees) * Math.PI / 180.0);
            double fx = (p.FlipX ? -1 : 1) * p.ScaleX;
            double fy = (p.FlipY ? -1 : 1) * p.ScaleY;

            // R * K * F, with K = [[1, tsx], [tsy, 1]] and F = diag(fx, fy)
            double a00 = fx * (c - s * tsy);
            double a01 = fy * (c * tsx - s);
            double a10 = fx * (s + c * tsy);
            double a11 = fy * (s * tsx + c);

            double cx = x + width / 2.0, cy = y + height / 2.0;
            double tx = cx - (a00 * width / 2.0 + a01 * height / 2.0);
            double ty = cy - (a10 * width / 2.0 + a11 * height / 2.0);
            return new JObject
            {
                ["scaleX"] = a00, ["scaleY"] = a11,
                ["shearX"] = a01, ["shearY"] = a10,
                ["translateX"] = tx, ["translateY"] = ty,
                ["unit"] = "EMU"
            };
        }

        // tan() blows up at 90 degrees; a skew past ~85 is unreadable anyway
        private static double ClampSkew(double deg) => deg > 85 ? 85 : (deg < -85 ? -85 : deg);

        // rotation-only convenience, kept for callers that have nothing else to say
        public static JObject RotatedTransform(double x, double y, double width, double height, double rotationDegrees)
            => ElementTransform(x, y, width, height, new Placement { RotationDegrees = rotationDegrees });

        private static void PutIfId(JObject o, string objectId)
        {
            if (!string.IsNullOrWhiteSpace(objectId)) o["objectId"] = objectId.Trim();
        }

        // returns the provided id trimmed, or a fresh short unique id when blank — the only
        // place a caller needs an id to reference an object created earlier in the same batch
        internal static string EnsureId(string provided, string prefix)
            => string.IsNullOrWhiteSpace(provided)
                ? prefix + "_" + Guid.NewGuid().ToString("N").Substring(0, 10)
                : provided.Trim();

        #endregion

        #region Slides

        // create slide — layoutId (an existing Layout's object ID) and predefinedLayout are
        // mutually exclusive per the API; layoutId wins when supplied. layoutPlaceholderIds +
        // placeholderObjectIds are parallel lists (zipped pairwise, min length): each pair maps a
        // known placeholder on the layout (by its own object ID) to a caller-chosen object ID for
        // the copy that lands on this new slide — the deterministic alternative to replaceAllText
        // token matching for filling a layout's placeholders.
        public static string BuildCreateSlide(string layout, int? insertionIndex, string objectId, string layoutId = null,
            IList<string> layoutPlaceholderIds = null, IList<string> placeholderObjectIds = null)
        {
            var inner = new JObject
            {
                ["slideLayoutReference"] = string.IsNullOrWhiteSpace(layoutId)
                    ? new JObject { ["predefinedLayout"] = string.IsNullOrWhiteSpace(layout) ? "BLANK" : layout.Trim() }
                    : new JObject { ["layoutId"] = layoutId.Trim() }
            };
            PutIfId(inner, objectId);
            if (insertionIndex.HasValue && insertionIndex.Value >= 0) inner["insertionIndex"] = insertionIndex.Value;

            if (layoutPlaceholderIds != null && placeholderObjectIds != null)
            {
                var mappings = new JArray();
                int pairs = Math.Min(layoutPlaceholderIds.Count, placeholderObjectIds.Count);
                for (int i = 0; i < pairs; i++)
                {
                    if (string.IsNullOrWhiteSpace(layoutPlaceholderIds[i]) || string.IsNullOrWhiteSpace(placeholderObjectIds[i])) continue;
                    mappings.Add(new JObject
                    {
                        ["objectId"] = placeholderObjectIds[i].Trim(),
                        ["layoutPlaceholderObjectId"] = layoutPlaceholderIds[i].Trim()
                    });
                }
                if (mappings.Count > 0) inner["placeholderIdMappings"] = mappings;
            }

            return new JObject { ["createSlide"] = inner }.ToString(Formatting.None);
        }

        // update page properties (background color)
        public static string BuildUpdatePageProperties(string pageId, string colorJson)
        {
            var fill = new JObject { ["solidFill"] = new JObject { ["color"] = Parse(colorJson) ?? new JObject() } };
            var inner = new JObject
            {
                ["objectId"] = pageId ?? "",
                ["pageProperties"] = new JObject { ["pageBackgroundFill"] = fill },
                ["fields"] = "pageBackgroundFill.solidFill.color"
            };
            return new JObject { ["updatePageProperties"] = inner }.ToString(Formatting.None);
        }

        // delete object
        public static string BuildDeleteObject(string objectId)
            => new JObject { ["deleteObject"] = new JObject { ["objectId"] = objectId ?? "" } }.ToString(Formatting.None);

        #endregion

        #region Shapes

        // create shape
        public static string BuildCreateShape(string pageId, string shapeType, double width, double height, double x, double y, string objectId, double rotationDegrees = 0)
            => BuildCreateShape(pageId, shapeType, width, height, x, y, objectId, new Placement { RotationDegrees = rotationDegrees });

        public static string BuildCreateShape(string pageId, string shapeType, double width, double height, double x, double y, string objectId, Placement placement)
        {
            var inner = new JObject
            {
                ["shapeType"] = string.IsNullOrWhiteSpace(shapeType) ? "TEXT_BOX" : shapeType.Trim(),
                ["elementProperties"] = ElementProperties(pageId, width, height, x, y, placement)
            };
            PutIfId(inner, objectId);
            return new JObject { ["createShape"] = inner }.ToString(Formatting.None);
        }

        // update shape properties (fill color + outline weight) — the original narrow signature,
        // kept for Create Shape's inline styling. Everything else goes through StyleBuilders.
        // Returns null when neither is supplied (never a "*" mask, which resets every property).
        public static string BuildUpdateShapeProperties(string objectId, string colorJson, double? outlineWeight)
            => StyleBuilders.BuildUpdateShapeProperties(objectId, new StyleBuilders.ShapeStyleSpec
            {
                FillColorJson = colorJson,
                OutlineWeightEmu = outlineWeight.HasValue && outlineWeight.Value > 0 ? outlineWeight : null
            });

        // update transform (position, scale, rotation in degrees). ABSOLUTE replaces the element's
        // whole matrix — so scale 1 on an element whose existing scale isn't 1 (most layout
        // placeholders) resizes it to its base size. relative = true instead emits a pure translation
        // in RELATIVE mode (the delta concatenates onto the existing matrix, so size and rotation are
        // untouched); scale/rotation are ignored there because a relative scale or rotation would pivot
        // about the page origin, not the element.
        public static string BuildUpdateTransform(string objectId, double x, double y, double scaleX, double scaleY, double rotationDegrees, bool relative = false)
            => BuildUpdateTransform(objectId, x, y, new Placement { ScaleX = scaleX, ScaleY = scaleY, RotationDegrees = rotationDegrees }, 0, 0, relative);

        // width/height are the element's base size (what it was created with, or size.width/height in
        // Get Slide's JSON). When known (> 0) the placement pivots about the centre like createShape
        // does; when 0 the API's own top-left pivot applies, which is only what you want for a pure
        // move or for scale 1 / no rotation.
        public static string BuildUpdateTransform(string objectId, double x, double y, Placement placement, double width, double height, bool relative = false)
        {
            JObject transform;
            if (relative)
            {
                transform = Translate(x, y);
            }
            else if (width > 0 && height > 0)
            {
                transform = ElementTransform(x, y, width, height, placement);
            }
            else
            {
                var m = ElementTransform(0, 0, 0, 0, placement);   // matrix about the origin, no centre correction
                m["translateX"] = x; m["translateY"] = y;
                transform = m;
            }
            var inner = new JObject
            {
                ["objectId"] = objectId ?? "",
                ["applyMode"] = relative ? "RELATIVE" : "ABSOLUTE",
                ["transform"] = transform
            };
            return new JObject { ["updatePageElementTransform"] = inner }.ToString(Formatting.None);
        }

        #endregion

        #region Text

        // insert text
        public static string BuildInsertText(string objectId, string text, int insertionIndex)
        {
            var inner = new JObject
            {
                ["objectId"] = objectId ?? "",
                ["text"] = text ?? "",
                ["insertionIndex"] = insertionIndex < 0 ? 0 : insertionIndex
            };
            return new JObject { ["insertText"] = inner }.ToString(Formatting.None);
        }

        // replace all shapes containing text with an image (template fill) — pageObjectIds
        // scopes the replacement to specific pages instead of the whole deck when supplied
        public static string BuildReplaceAllShapesWithImage(string findText, string imageUrl, bool matchCase, IEnumerable<string> pageObjectIds = null)
        {
            var inner = new JObject
            {
                ["imageUrl"] = imageUrl ?? "",
                ["imageReplaceMethod"] = "CENTER_INSIDE",
                ["containsText"] = new JObject { ["text"] = findText ?? "", ["matchCase"] = matchCase }
            };
            PutPageObjectIds(inner, pageObjectIds);
            return new JObject { ["replaceAllShapesWithImage"] = inner }.ToString(Formatting.None);
        }

        // replace all text — pageObjectIds scopes the replacement to specific pages instead of
        // the whole deck when supplied
        public static string BuildReplaceAllText(string findText, string replaceText, bool matchCase, IEnumerable<string> pageObjectIds = null)
        {
            var inner = new JObject
            {
                ["containsText"] = new JObject { ["text"] = findText ?? "", ["matchCase"] = matchCase },
                ["replaceText"] = replaceText ?? ""
            };
            PutPageObjectIds(inner, pageObjectIds);
            return new JObject { ["replaceAllText"] = inner }.ToString(Formatting.None);
        }

        private static void PutPageObjectIds(JObject o, IEnumerable<string> pageObjectIds)
        {
            if (pageObjectIds == null) return;
            var arr = new JArray();
            foreach (var id in pageObjectIds)
                if (!string.IsNullOrWhiteSpace(id)) arr.Add(id.Trim());
            if (arr.Count > 0) o["pageObjectIds"] = arr;
        }

        // update text style — narrow signature kept for the tests and any older caller; the full
        // trait set (underline, highlight, link, ranges, table cells...) lives in StyleBuilders.
        public static string BuildUpdateTextStyle(string objectId, bool? bold, bool? italic, double fontSize, string fontFamily, string colorJson)
            => StyleBuilders.BuildUpdateTextStyle(objectId, new StyleBuilders.TextStyleSpec
            {
                Bold = bold, Italic = italic,
                FontSizePt = fontSize > 0 ? fontSize : (double?)null,
                FontFamily = fontFamily,
                TextColorJson = colorJson
            });

        // update paragraph style (alignment only) — see StyleBuilders for the full paragraph spec
        public static string BuildUpdateParagraphStyle(string objectId, string alignment)
            => StyleBuilders.BuildUpdateParagraphStyle(objectId, new StyleBuilders.ParagraphStyleSpec
            {
                Alignment = string.IsNullOrWhiteSpace(alignment) ? "START" : alignment.Trim()
            });

        #endregion

        #region Tables

        // create table
        public static string BuildCreateTable(string pageId, int rows, int columns, string objectId)
        {
            var inner = new JObject
            {
                ["elementProperties"] = new JObject { ["pageObjectId"] = pageId ?? "" },
                ["rows"] = Math.Max(1, rows),
                ["columns"] = Math.Max(1, columns)
            };
            PutIfId(inner, objectId);
            return new JObject { ["createTable"] = inner }.ToString(Formatting.None);
        }

        // insert text into a table cell
        public static string BuildInsertTextCell(string tableObjectId, int rowIndex, int columnIndex, string text)
        {
            var inner = new JObject
            {
                ["objectId"] = tableObjectId ?? "",
                ["cellLocation"] = new JObject { ["rowIndex"] = rowIndex, ["columnIndex"] = columnIndex },
                ["text"] = text ?? "",
                ["insertionIndex"] = 0
            };
            return new JObject { ["insertText"] = inner }.ToString(Formatting.None);
        }

        #endregion

        #region Media

        // create image
        public static string BuildCreateImage(string pageId, string imageUrl, double width, double height, double x, double y, string objectId, double rotationDegrees = 0)
            => BuildCreateImage(pageId, imageUrl, width, height, x, y, objectId, new Placement { RotationDegrees = rotationDegrees });

        public static string BuildCreateImage(string pageId, string imageUrl, double width, double height, double x, double y, string objectId, Placement placement)
        {
            var inner = new JObject
            {
                ["url"] = imageUrl ?? "",
                ["elementProperties"] = ElementProperties(pageId, width, height, x, y, placement)
            };
            PutIfId(inner, objectId);
            return new JObject { ["createImage"] = inner }.ToString(Formatting.None);
        }

        // replace image — swaps the image content of an EXISTING image/picture-placeholder element
        // (targeted by its own objectId, e.g. from CreateSlideJson's Picture Object ID output) with
        // a new one. Different from BuildCreateImage, which places a brand new image element.
        // Field names are imageObjectId / url (NOT objectId / imageUri — the API rejects those with
        // "Unknown name", confirmed against a live batchUpdate 2026-09-15).
        public static string BuildReplaceImage(string objectId, string imageUri, string imageReplaceMethod = "CENTER_INSIDE")
        {
            var inner = new JObject
            {
                ["imageObjectId"] = objectId ?? "",
                ["url"] = imageUri ?? "",
                ["imageReplaceMethod"] = string.IsNullOrWhiteSpace(imageReplaceMethod) ? "CENTER_INSIDE" : imageReplaceMethod.Trim()
            };
            return new JObject { ["replaceImage"] = inner }.ToString(Formatting.None);
        }

        #endregion

        #region Batch ID bookkeeping

        private static readonly string[] CreateRequestKeys = { "createSlide", "createShape", "createTable", "createImage" };

        // the object IDs a batch *creates* (new slides/shapes/tables/images and placeholder copies) —
        // the ones the API rejects with "object ID already exists" if the same batch is pushed twice
        public static HashSet<string> CreatedIds(IEnumerable<string> requests)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (requests == null) return ids;
            foreach (var r in requests)
            {
                if (string.IsNullOrWhiteSpace(r)) continue;
                JObject o;
                try { o = JObject.Parse(r); } catch { continue; }
                foreach (var key in CreateRequestKeys)
                {
                    if (!(o[key] is JObject inner)) continue;
                    string id = (string)inner["objectId"];
                    if (!string.IsNullOrWhiteSpace(id)) ids.Add(id);
                    if (inner["placeholderIdMappings"] is JArray maps)
                        foreach (var m in maps)
                        {
                            string pid = (string)m["objectId"];
                            if (!string.IsNullOrWhiteSpace(pid)) ids.Add(pid);
                        }
                }
            }
            return ids;
        }

        // Re-sending a batch whose upstream builders haven't recomputed re-uses the same auto-generated
        // IDs. Any created ID already in alreadySent is swapped for a fresh one — consistently across
        // every request in the batch (a createSlide's objectId and the createShape.pageObjectId that
        // targets it move together), so the push creates a second copy instead of failing.
        public static List<string> RemapCreatedIds(IList<string> requests, ICollection<string> alreadySent, out Dictionary<string, string> remapped)
        {
            remapped = new Dictionary<string, string>(StringComparer.Ordinal);
            var result = new List<string>(requests ?? new List<string>());
            if (requests == null || alreadySent == null || alreadySent.Count == 0) return result;

            foreach (var id in CreatedIds(requests))
                if (alreadySent.Contains(id)) remapped[id] = FreshVariant(id);
            if (remapped.Count == 0) return result;

            for (int i = 0; i < result.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(result[i])) continue;
                JContainer tok;
                try { tok = JToken.Parse(result[i]) as JContainer; } catch { continue; }
                if (tok == null) continue;
                foreach (var v in tok.Descendants().OfType<JValue>().Where(v => v.Type == JTokenType.String).ToList())
                    if (remapped.TryGetValue((string)v.Value, out var fresh)) v.Value = fresh;
                result[i] = tok.ToString(Formatting.None);
            }
            return result;
        }

        // auto ids look like prefix_xxxxxxxxxx (EnsureId): swap the suffix; anything else gets one appended
        private static string FreshVariant(string id)
        {
            int us = id.LastIndexOf('_');
            string prefix = (us > 0 && id.Length - us - 1 == 10) ? id.Substring(0, us) : id;
            return EnsureId("", prefix);
        }

        #endregion

        #region Parse helpers

        public static JObject Parse(string json)
            => string.IsNullOrWhiteSpace(json) ? null : JObject.Parse(json);

        // 0-255 RGB -> the API's OpaqueColor JSON ({"rgbColor":{red,green,blue}} on a 0-1 scale).
        // Components take a native Grasshopper Colour and convert here, so the builders above keep
        // their string colorJson signature (and stay testable without System.Drawing).
        public static string ColorJson(int red, int green, int blue)
        {
            var rgb = new JObject
            {
                ["red"] = Math.Round(Clamp255(red) / 255.0, 4),
                ["green"] = Math.Round(Clamp255(green) / 255.0, 4),
                ["blue"] = Math.Round(Clamp255(blue) / 255.0, 4)
            };
            return new JObject { ["rgbColor"] = rgb }.ToString(Formatting.None);
        }

        private static int Clamp255(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);

        #endregion
    }
}
