using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlidesConnect
{
    public static class PluginUtilities
    {
        // Shared enum helper — used by any preset component backed by an enum
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        // Bulk list-access helpers, shared by every list-access content-building component.
        // Broadcast rule: a length-1 list repeats for every record. Otherwise record count is
        // the shortest length-1-excluded input; longer lists are truncated to that count with a
        // runtime warning. Never throws, never drops the whole batch.
        internal static int RecordCount(params System.Collections.ICollection[] lists)
        {
            int min = -1;
            foreach (var l in lists)
            {
                if (l == null || l.Count <= 1) continue;
                if (min == -1 || l.Count < min) min = l.Count;
            }
            return min == -1 ? 1 : min;
        }

        internal static List<T> MatchLength<T>(IList<T> values, int count, T fallback)
        {
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                if (values != null && values.Count == 1) result.Add(values[0]);
                else if (values != null && i < values.Count) result.Add(values[i]);
                else result.Add(fallback);
            }
            return result;
        }

        // Native Grasshopper Colour list -> the Color JSON strings the pure builders take (one per
        // record). An unconnected optional Colour input yields an empty list here, which the
        // MatchLength("") padding downstream turns into "no color" for every record.
        internal static List<string> ToColorJsons(IList<System.Drawing.Color> colors)
        {
            var result = new List<string>(colors.Count);
            foreach (var c in colors) result.Add(RequestBuilders.ColorJson(c.R, c.G, c.B));
            return result;
        }

        // "connect to apply": an unconnected optional list input reads back empty, which means
        // "leave that trait alone" for every record — so pad with null rather than a fake default
        internal static List<T?> MatchLengthOrNull<T>(IList<T> values, int count) where T : struct
        {
            var result = new List<T?>(count);
            if (values == null || values.Count == 0) { for (int i = 0; i < count; i++) result.Add(null); return result; }
            var matched = MatchLength(values, count, values[values.Count - 1]);
            foreach (var v in matched) result.Add(v);
            return result;
        }

        // numeric "> 0 to apply" convention (DocsConnect's): 0 or negative = leave alone
        internal static double? Positive(double v) => v > 0 ? v : (double?)null;

        // true if the list carries at least one non-blank value — an unconnected text input with a
        // "" default reads back as [""], so Count > 0 alone can't tell "wired" from "unwired"
        internal static bool HasAny(IList<string> values)
        {
            if (values == null) return false;
            foreach (var v in values) if (!string.IsNullOrWhiteSpace(v)) return true;
            return false;
        }

        internal static void WarnIfRagged(GH_Component component, int recordCount, params (string name, int count)[] inputs)
        {
            foreach (var (name, count) in inputs)
                if (count > 1 && count != recordCount)
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        name + " has " + count + " item(s) but " + recordCount + " were expected — using the first " + recordCount + ".");
        }

        internal static readonly string TabName = "SlidesConnect";

        // Subcategory constants — one per subcategory, in display order
        internal static readonly string CategoryAAAuth         = "01. Auth";
        internal static readonly string CategoryABPresentation = "02. Presentation";
        internal static readonly string CategoryACSlides       = "03. Slides";
        internal static readonly string CategoryADLayouts      = "04. Layouts";
        internal static readonly string CategoryAEShapes       = "05. Shapes";
        internal static readonly string CategoryAFText         = "06. Text";
        internal static readonly string CategoryAGTextStyle    = "07. Text Style";
        internal static readonly string CategoryAHTables       = "08. Tables";
        internal static readonly string CategoryAIInsert       = "09. Insert";
        internal static readonly string CategoryAJAggregate    = "10. Aggregate";
        internal static readonly string CategoryZYPresets      = "11. Presets";
    }
}
