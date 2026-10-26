using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json.Linq;
using SlidesConnect;

namespace SlidesConnect.Tests
{
    // Proves the tree -> table assembly (GeometryBuilders) and the EMU conversion math are correct.
    public class BulkAssemblyTests
    {
        private static IList<IList<string>> Rows(params string[][] rows)
        {
            var list = new List<IList<string>>();
            foreach (var r in rows) list.Add(new List<string>(r));
            return list;
        }

        [Fact]
        public void BuildTableRequests_FullTable_CreateTablePlusOneInsertPerCell()
        {
            var rows = Rows(
                new[] { "A", "B", "C" },
                new[] { "1", "2", "3" });

            var reqs = GeometryBuilders.BuildTableRequests("p", rows);

            // 1 createTable + 6 cell inserts
            Assert.Equal(7, reqs.Count);
            Assert.Contains("createTable", reqs[0]);
            var create = JObject.Parse(reqs[0]);
            Assert.Equal(2, (int)create.SelectToken("createTable.rows")!);
            Assert.Equal(3, (int)create.SelectToken("createTable.columns")!);
        }

        [Fact]
        public void BuildTableRequests_RaggedRows_UsesMaxColumns_DoesNotThrow()
        {
            var rows = Rows(
                new[] { "A", "B", "C" },
                new[] { "1" });                 // short row

            var reqs = GeometryBuilders.BuildTableRequests("p", rows);

            var create = JObject.Parse(reqs[0]);
            Assert.Equal(3, (int)create.SelectToken("createTable.columns")!); // widest row wins
            // inserts = 3 (row0) + 1 (row1) = 4, plus createTable = 5
            Assert.Equal(5, reqs.Count);
        }

        [Fact]
        public void BuildTableRequests_EmptyCells_Skipped()
        {
            var rows = Rows(new[] { "A", "", "C" });

            var reqs = GeometryBuilders.BuildTableRequests("p", rows);

            // createTable + 2 non-empty cells (A and C), empty middle skipped
            Assert.Equal(3, reqs.Count);
        }

        [Fact]
        public void BuildTableRequests_EmptyInput_ReturnsEmpty()
        {
            Assert.Empty(GeometryBuilders.BuildTableRequests("p", Rows()));
        }

        [Theory]
        [InlineData(1, "in", 914400)]
        [InlineData(25.4, "mm", 914400)]
        [InlineData(2.54, "cm", 914400)]
        [InlineData(1, "ft", 10972800)]
        public void ToEmu_ConvertsUnits(double value, string unit, double expected)
        {
            Assert.Equal(expected, GeometryBuilders.ToEmu(value, unit), 1);
        }

        [Fact]
        public void BuildTableRequests_TwoCallsSamePage_DistinctTableIds()
        {
            var rows = Rows(new[] { "A" });
            string a = (string)JObject.Parse(GeometryBuilders.BuildTableRequests("p", rows)[0]).SelectToken("createTable.objectId")!;
            string b = (string)JObject.Parse(GeometryBuilders.BuildTableRequests("p", rows)[0]).SelectToken("createTable.objectId")!;
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void ToEmu_EmuPassesThrough_AndFromEmuInverts()
        {
            Assert.Equal(12345, GeometryBuilders.ToEmu(12345, "emu"), 6);
            Assert.Equal(2.5, GeometryBuilders.FromEmu(GeometryBuilders.ToEmu(2.5, "in"), "in"), 6);
            Assert.Equal(72, GeometryBuilders.FromEmu(GeometryBuilders.ToEmu(1, "in"), "pt"), 6);
        }

        [Fact]
        public void BuildFrame_FlipsY_RhinoOriginIsPageBottomLeft()
        {
            // a page-sized rectangle whose top-left is at Rhino (0, 5.625) lands at Slides (0, 0)
            var f = GeometryBuilders.BuildFrame(0, 5.625, 10, 5.625, "in", false);
            Assert.Equal(0, f[0], 1);
            Assert.Equal(0, f[1], 1);
            Assert.Equal(GeometryBuilders.PageWidthEmu, f[2], 1);
            Assert.Equal(GeometryBuilders.PageHeightEmu, f[3], 1);

            // 1 in down from the page top in Rhino = 1 in down in Slides
            var g = GeometryBuilders.BuildFrame(1, 4.625, 2, 1, "in", false);
            Assert.Equal(914400, g[0], 1);
            Assert.Equal(914400, g[1], 1);
        }

        [Fact]
        public void CsvParser_HandlesQuotedNewlinesAndEscapedQuotes()
        {
            string text = "Floor,Body\r\n1,\"Line one\r\nLine two\"\r\n2,\"He said \"\"hi\"\"\"\r\n\r\n3,plain";
            var rows = CsvParser.Parse(text, ',');
            Assert.Equal(5, rows.Count);
            Assert.Equal(new[] { "Floor", "Body" }, rows[0]);
            Assert.Equal("Line one\r\nLine two", rows[1][1]);
            Assert.Equal("He said \"hi\"", rows[2][1]);
            Assert.True(CsvParser.IsBlank(rows[3]));
            Assert.Equal(new[] { "3", "plain" }, rows[4]);
        }

        [Fact]
        public void BuildFrame_FitToPage_FitsWithinPageBounds()
        {
            // a 20x10 in rectangle is larger than the page; fit must scale it inside and center it
            var f = GeometryBuilders.BuildFrame(0, 0, 20, 10, "in", true);
            double x = f[0], y = f[1], w = f[2], h = f[3];
            Assert.True(w <= GeometryBuilders.PageWidthEmu + 1);
            Assert.True(h <= GeometryBuilders.PageHeightEmu + 1);
            Assert.True(x >= -1 && y >= -1);
        }
    }
}
