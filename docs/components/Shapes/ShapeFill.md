# Shape Fill

Sets the fill colour and/or transparency of one or more existing shapes.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape to fill. |
| Fill Color | colour (list) | No |  | Fill colour (connect to apply). |
| Fill Transparency | double (list) | No |  | Transparency 0-1 (0 = opaque, 1 = invisible). Connect to apply. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateShapeProperties request JSON, one per shape. |

## Notes

A single-purpose slice of [Update Shape Style](UpdateShapeProperties.md): same `updateShapeProperties`
request, only the field(s) this component owns in the mask, so nothing else on the shape is touched.
Every input is a list: one value per shape, or a single value broadcast to all of them.
