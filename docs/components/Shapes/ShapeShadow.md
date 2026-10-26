# Shape Shadow

Adds a soft outer drop shadow to one or more existing shapes, or removes it.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape. |
| Shadow | bool (list) | No | true | True = add the shadow, false = remove it. |
| Shadow Color | colour (list) | No |  | Shadow colour (default black). |
| Transparency | double (list) | No | 0.6 | Shadow transparency 0-1 (0 = solid, 1 = invisible). |
| Blur | double (list) | No | 3 | Blur radius, in Unit. |
| Offset X | double (list) | No | 3 | Horizontal offset, in Unit (positive = right). |
| Offset Y | double (list) | No | 3 | Vertical offset, in Unit (positive = down). |
| Unit | string (list) | No | pt | Unit for Blur / Offset (pt, in, mm, cm, m, ft). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateShapeProperties request JSON, one per shape. |

## Notes

A single-purpose slice of [Update Shape Style](UpdateShapeProperties.md): same `updateShapeProperties`
request, only the field(s) this component owns in the mask, so nothing else on the shape is touched.
Every input is a list — one value per shape, or a single value broadcast to all of them.
