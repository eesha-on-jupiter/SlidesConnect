# Table Cell Style

Sets the fill colour and/or vertical content alignment of a range of cells in an existing table.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the table (from Create Table's Object ID output). |
| Row | int (list) | No | -1 | First row of the range (0-based). -1 = the whole table. |
| Column | int (list) | No | -1 | First column of the range (0-based). -1 = the whole table. |
| Row Span | int (list) | No | 1 | Number of rows in the range. |
| Column Span | int (list) | No | 1 | Number of columns in the range. |
| Fill Color | colour (list) | No |  | Cell fill colour (connect to apply). |
| Content Alignment | string (list) | No | "" | Vertical alignment of the cell text: TOP, MIDDLE, BOTTOM (blank to skip). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateTableCellProperties request JSON. |

## Notes

Wire the table's Object ID from [Create Table](CreateTable.md). [Data Tree To Table](DataTreeToTable.md) has no Object ID output in 1.0.0 — its table ID is the `objectId` of the `createTable` request (the first item of its Request JSON).
Row/Column -1 applies to the whole table (the API's default when `tableRange` is omitted); otherwise the
range starts at Row/Column and covers Row Span × Column Span cells. For the *text* in a cell (bold,
size, colour, alignment) use [Styled Text](../TextStyle/UpdateTextStyle.md) / [Styled Paragraph](../TextStyle/UpdateParagraphStyle.md)
with their Row/Column inputs.
