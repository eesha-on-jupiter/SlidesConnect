# Table Column Width

Sets the width of one or more columns of an existing table.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the table. |
| Column Index | int (list) | No | 0 | Column index per record (0-based). |
| Width | double (list) | No | 1.5 | Column width per record, in Unit. |
| Unit | string (list) | No | in | Unit for Width (in, pt, mm, cm, m, ft). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateTableColumnProperties request JSON. |

## Notes

`updateTableColumnProperties`, one per record. Five Column Indices + five Widths = five columns each
sized independently; one Width with five indices sizes them all the same (length-1 broadcast).
