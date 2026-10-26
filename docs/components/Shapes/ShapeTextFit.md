# Shape Text Fit

Sets how text sits in one or more existing shapes: vertical alignment and autofit.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape. |
| Content Alignment | string (list) | No | "" | Vertical alignment of the text in the shape: TOP, MIDDLE, BOTTOM (blank to skip) — see the Content Alignment preset. |
| Autofit | string (list) | No | "" | NONE, TEXT_AUTOFIT (shrink text to fit the shape) or SHAPE_AUTOFIT (grow the shape to fit the text). Blank to skip. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateShapeProperties request JSON, one per shape. |

## Notes

A single-purpose slice of [Update Shape Style](UpdateShapeProperties.md): same `updateShapeProperties`
request, only the field(s) this component owns in the mask, so nothing else on the shape is touched.
Every input is a list — one value per shape, or a single value broadcast to all of them.
