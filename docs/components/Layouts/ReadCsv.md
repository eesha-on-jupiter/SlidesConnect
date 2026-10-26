# Read CSV

Reads a CSV file and outputs a column-per-branch Grasshopper tree — one branch per column, items are
that column's values down the rows — plus a parallel Headers list.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| File Path | string (item) | Yes | — | Full path to the CSV file. |
| Has Headers | bool (item) | No | true | True if the first row contains column headers. |
| Delimiter | string (item) | No | "," | Column delimiter character. |
| Encoding | string (item) | No | "UTF8" | File encoding: UTF8, ASCII, Unicode, Latin1. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Columns | string (tree) | Column values as a tree — one branch per column, one item per row. |
| Headers | string (list) | Column header names, parallel to Columns branches. |
| Rows | int (item) | Number of data rows read. |
| Cols | int (item) | Number of columns read. |
| Error | string (item) | Error message, if any. |

## Notes

Quoted cells may contain the delimiter, doubled quotes (`""`) and line breaks — a multi-line Body
cell exported from Excel comes through as one value, not two rows.

Pairs directly with [Get CSV Column](GetCsvColumn.md), which pulls a single flat list out of the
Columns tree by header name — already the shape
[Build Slides From Layout](BuildSlidesFromLayout.md) / [Styled Text Box](../Text/StyledTextBox.md)
want for Title/Subtitle/Body/Photo URL or Text. This is the CSV-driven half of the parametric multi-slide
story: one CSV row becomes one slide.
