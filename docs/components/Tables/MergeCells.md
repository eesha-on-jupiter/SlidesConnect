# Merge Cells

Merges (or unmerges) a rectangular range of cells in an existing table.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the table. |
| Row | int (list) | No | 0 | First row of the range (0-based). |
| Column | int (list) | No | 0 | First column of the range (0-based). |
| Row Span | int (list) | No | 1 | Number of rows to merge. |
| Column Span | int (list) | No | 2 | Number of columns to merge. |
| Unmerge | bool (list) | No | false | True = split the range's merged cells back apart instead. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | mergeTableCells / unmergeTableCells request JSON. |

## Notes

`mergeTableCells` / `unmergeTableCells`. Merging keeps only the top-left cell's text, so merge first and
insert the merged cell's text afterwards (Insert Text with Row/Column, or Styled Text). The typical use
is a header row spanning every column: Row 0, Column 0, Row Span 1, Column Span = number of columns.
