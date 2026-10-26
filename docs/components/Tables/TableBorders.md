# Table Borders

Sets border colour, weight and/or dash on one side (or all) of a range of cells in an existing table.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the table. |
| Row | int (list) | No | -1 | First row of the range (0-based). -1 = the whole table. |
| Column | int (list) | No | -1 | First column of the range (0-based). -1 = the whole table. |
| Row Span | int (list) | No | 1 | Number of rows in the range. |
| Column Span | int (list) | No | 1 | Number of columns in the range. |
| Border Position | string (list) | No | ALL | ALL, OUTER, INNER, INNER_HORIZONTAL, INNER_VERTICAL, TOP, BOTTOM, LEFT or RIGHT — see the Border Position preset. |
| Border Color | colour (list) | No |  | Border colour (connect to apply). |
| Border Weight | double (list) | No | 0 | Border weight, in Unit (>0 to apply). |
| Border Dash | string (list) | No | "" | SOLID, DOT, DASH, DASH_DOT, LONG_DASH, LONG_DASH_DOT (blank to skip). |
| Unit | string (list) | No | pt | Unit for Border Weight (pt, in, mm, cm, m, ft). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateTableBorderProperties request JSON. |

## Notes

`updateTableBorderProperties`. Border Position picks which edges of the range are styled — `ALL`,
`OUTER`, `INNER`, `INNER_HORIZONTAL`, `INNER_VERTICAL`, or a single side; see the
[Border Position](../Presets/BorderPositionPreset.md) preset. Row/Column -1 = the whole table.
