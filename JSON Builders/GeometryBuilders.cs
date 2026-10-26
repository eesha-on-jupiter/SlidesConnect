using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlidesConnect
{
    // AEC geometry helpers: Rhino-unit -> Slides EMU conversion and value -> color mapping.
    // Pure Newtonsoft + BCL only — no Grasshopper / RhinoCommon — so the test project compiles it on net8.0.
    public static class GeometryBuilders
    {
        // Standard widescreen page in EMU (10 x 5.625 in).
        public const double PageWidthEmu = 9144000.0;
        public const double PageHeightEmu = 5143500.0;

        private const double EmuPerInch = 914400.0;

        #region EMU conversion

        // converts a length in the given unit to EMU (914400 EMU per inch). "emu" passes through
        // unchanged so a raw API value can be fed to a Unit-taking input without a double conversion.
        public static double ToEmu(double value, string unit)
        {
            switch ((unit ?? "in").Trim().ToLowerInvariant())
            {
                case "emu": return value;
                case "mm": return value * EmuPerInch / 25.4;
                case "cm": return value * EmuPerInch / 2.54;
                case "m": return value * EmuPerInch / 0.0254;
                case "ft": case "feet": return value * EmuPerInch * 12.0;
                case "pt": case "point": return value * EmuPerInch / 72.0;
                case "in": case "inch": case "inches": default: return value * EmuPerInch;
            }
        }

        // converts EMU back to the given unit — the inverse of ToEmu
        public static double FromEmu(double emu, string unit)
            => emu / ToEmu(1.0, unit);

        // converts a rectangle's visual top-left corner (Rhino Y-up coordinates) + size, in 'unit',
        // to Slides page-space EMU [x, y, width, height]. Rhino's origin is taken as the page's
        // bottom-left corner: y is flipped against the standard 5.625 in page height, so what you
        // see in Rhino's Top view is what lands on the slide. fitToPage scales the rectangle
        // uniformly to fit the page and centers it (the position is then ignored).
        public static double[] BuildFrame(double topLeftX, double topLeftY, double width, double height, string unit, bool fitToPage)
        {
            double w = ToEmu(Math.Abs(width), unit);
            double h = ToEmu(Math.Abs(height), unit);
            double x = ToEmu(topLeftX, unit);
            double y = PageHeightEmu - ToEmu(topLeftY, unit);

            if (fitToPage && w > 0 && h > 0)
            {
                double scale = Math.Min(PageWidthEmu / w, PageHeightEmu / h);
                w *= scale;
                h *= scale;
                x = (PageWidthEmu - w) / 2.0;
                y = (PageHeightEmu - h) / 2.0;
            }
            return new[] { x, y, w, h };
        }

        #endregion

        #region Table assembly

        // builds the full createTable + per-cell insertText request sequence from a row-major table.
        // rows = list of rows; each row = list of cell strings. Returns one Request JSON per entry.
        // (Header styling is a v2 add — kept to validated createTable + insertText request types for v1.)
        public static List<string> BuildTableRequests(string pageId, IList<IList<string>> rows)
        {
            var requests = new List<string>();
            int rowCount = rows?.Count ?? 0;
            int colCount = 0;
            for (int r = 0; r < rowCount; r++)
                colCount = Math.Max(colCount, rows[r]?.Count ?? 0);
            if (rowCount == 0 || colCount == 0) return requests;

            // a fresh id per call — deriving it from the page id (as v1 did) meant a second table on
            // the same slide, or re-sending the same graph, collided with "object ID already exists"
            string tableId = RequestBuilders.EnsureId("", "tbl");

            requests.Add(RequestBuilders.BuildCreateTable(pageId, rowCount, colCount, tableId));

            for (int r = 0; r < rowCount; r++)
            {
                var row = rows[r];
                if (row == null) continue;
                for (int c = 0; c < row.Count; c++)
                {
                    if (string.IsNullOrEmpty(row[c])) continue;
                    requests.Add(RequestBuilders.BuildInsertTextCell(tableId, r, c, row[c]));
                }
            }
            return requests;
        }

        #endregion
    }
}
