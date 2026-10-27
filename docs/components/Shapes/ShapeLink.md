# Shape Link

Hyperlinks one or more existing shapes to a URL.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape. |
| URL | string (list) | No | "" | Hyperlink URL the whole shape opens when clicked. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateShapeProperties request JSON, one per shape. |

## Notes

A single-purpose slice of [Update Shape Style](UpdateShapeProperties.md): same `updateShapeProperties`
request, only the field(s) this component owns in the mask, so nothing else on the shape is touched.
Every input is a list: one value per shape, or a single value broadcast to all of them.
