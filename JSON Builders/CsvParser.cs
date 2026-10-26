using System.Collections.Generic;
using System.Text;

namespace SlidesConnect
{
    // RFC 4180-style CSV record parser. Pure BCL so the test project covers it. Handles quoted
    // fields containing the delimiter, doubled quotes ("") as an escaped quote, and — the case a
    // line-by-line reader gets wrong — newlines inside a quoted field (Excel emits these for any
    // multi-line cell, e.g. a slide Body column). Accepts \r\n, \n or \r as the record terminator.
    public static class CsvParser
    {
        public static List<string[]> Parse(string text, char separator)
        {
            var records = new List<string[]>();
            if (string.IsNullOrEmpty(text)) return records;

            var cells = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { current.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else current.Append(c);
                    continue;
                }

                if (c == '"') inQuotes = true;
                else if (c == separator) { cells.Add(current.ToString()); current.Clear(); }
                else if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    cells.Add(current.ToString()); current.Clear();
                    records.Add(cells.ToArray()); cells.Clear();
                }
                else current.Append(c);
            }

            // last record without a trailing newline
            if (current.Length > 0 || cells.Count > 0)
            {
                cells.Add(current.ToString());
                records.Add(cells.ToArray());
            }

            return records;
        }

        // a record that is a single empty cell is a blank line, not data
        public static bool IsBlank(string[] record)
            => record == null || (record.Length == 1 && string.IsNullOrWhiteSpace(record[0]));
    }
}
