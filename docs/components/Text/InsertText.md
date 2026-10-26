# Insert Text

Builds `insertText` requests for one or more text insertions.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape or placeholder to insert text into. |
| Text | string (list) | Yes | "" | Text to insert. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `insertText` request(s), one per record. |

## Notes

Object ID is commonly wired from [Create Shape](../Shapes/CreateShape.md)'s Object ID output, or
[Create Slide](../Slides/CreateSlide.md)'s Title/Body Object ID outputs when targeting a layout
placeholder that's already on the slide. Insert Text has no row/column input, so it doesn't target
table cells — fill a table from data with [Data Tree To Table](../Tables/DataTreeToTable.md).
