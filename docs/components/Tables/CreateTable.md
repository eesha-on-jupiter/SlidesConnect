# Create Table

Builds `createTable` requests for one or more empty tables.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Slide ID | string (list) | Yes | "" | Slide to place each table on. |
| Rows | int (list) | No | 2 | Row count. |
| Columns | int (list) | No | 2 | Column count. |
| Object ID | string (list) | No | "" | Table object ID. Auto-generated when blank, and resolved back out. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `createTable` request(s), one per record. |
| Object ID | string (list) | The resolved object ID of each table. |

## Notes

This creates an empty table only; populate cells afterward via
[Insert Text](../Text/InsertText.md) targeting each cell, or use
[Data Tree To Table](DataTreeToTable.md) to build and populate a table from a Grasshopper data tree
in one component.
