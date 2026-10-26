# Styled Paragraph

Styles the paragraphs in one or more shapes or table cells with any combination of paragraph-level traits — the Slides counterpart of DocsConnect's Styled Paragraph.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape (or table) whose paragraphs to style. |
| Alignment | string (list) | No | "" | START, CENTER, END, JUSTIFIED (blank to skip) — see the Alignment preset. |
| Line Spacing | double (list) | No | 0 | Line spacing percent, e.g. 115 (>0 to apply; 100 = normal). |
| Indent | double (list) | No | 0 | Start indent in points (>0 to apply). |
| Space Above | double (list) | No | 0 | Space above each paragraph in points (>0 to apply). |
| Space Below | double (list) | No | 0 | Space below each paragraph in points (>0 to apply). |
| Indent End | double (list) | No | 0 | End indent in points (>0 to apply). |
| First Line Indent | double (list) | No | 0 | First-line indent in points (>0 to apply). |
| Direction | string (list) | No | "" | LEFT_TO_RIGHT or RIGHT_TO_LEFT (blank to skip). |
| Row | int (list) | No | -1 | Table only: row index of the cell (-1 = Object ID is a shape, not a table). |
| Column | int (list) | No | -1 | Table only: column index of the cell. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | batchUpdate request JSON, one per record. |

## Notes

Only supplied properties go into the field mask. Line spacing is a percentage of normal (100 = single);
the point-valued inputs become `PT` Dimensions. Records with nothing set are skipped with a warning.
