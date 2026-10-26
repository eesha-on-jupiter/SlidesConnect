# Shape Outline

Sets the outline colour, weight and/or dash style of one or more existing shapes.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape to outline. |
| Outline Color | colour (list) | No |  | Outline colour (connect to apply). |
| Outline Weight | double (list) | No | 0 | Outline weight, in Unit (>0 to apply). |
| Outline Dash | string (list) | No | "" | SOLID, DOT, DASH, DASH_DOT, LONG_DASH or LONG_DASH_DOT (blank to skip) — see the Dash Style preset. |
| Unit | string (list) | No | pt | Unit for Outline Weight (pt, in, mm, cm, m, ft). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateShapeProperties request JSON, one per shape. |

## Notes

A single-purpose slice of [Update Shape Style](UpdateShapeProperties.md): same `updateShapeProperties`
request, only the field(s) this component owns in the mask, so nothing else on the shape is touched.
Every input is a list — one value per shape, or a single value broadcast to all of them.
