# Data Tree To Table

**AEC feature.** Builds a `createTable` request plus one `insertText` request per populated cell from
a Grasshopper data tree: the whole table in one component call.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Slide ID | string (item) | Yes | "" | Slide to place the table on. |
| Data | string (tree) | Yes | None | One branch per row; items in a branch are that row's cell values. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The full request sequence: one createTable plus one insertText per populated cell. |

## Notes

Row and column counts are inferred from the tree's shape; the widest branch determines column
count, and branch count determines row count. Empty cells are simply skipped (no insertText emitted
for them), so a ragged tree doesn't error, it just leaves those cells blank.

The table's object ID is auto-generated on every solve and is not exposed as an output in 1.0.0. To
style the table with [Table Cell Style](TableCellStyle.md), [Table Borders](TableBorders.md),
[Merge Cells](MergeCells.md) or [Table Column Width](TableColumnWidth.md), read the `objectId` of the
`createTable` request (the first Request JSON item), or start from [Create Table](CreateTable.md),
which outputs its Object ID (but leaves the cells empty).
