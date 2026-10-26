# Update Shape Style

Restyles one or more existing shapes with any combination of fill, outline, shadow, vertical text alignment, autofit and link.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape to style. |
| Fill Color | colour (list) | No |  | Fill colour (connect to apply). |
| Fill Transparency | double (list) | No |  | 0-1 (0 = opaque, 1 = invisible). Connect to apply. |
| Outline Color | colour (list) | No |  | Outline colour (connect to apply). |
| Outline Weight | double (list) | No | 0 | Outline weight, in Unit (>0 to apply). |
| Outline Dash | string (list) | No | "" | SOLID, DOT, DASH, DASH_DOT, LONG_DASH, LONG_DASH_DOT (blank to skip) — see the Dash Style preset. |
| Shadow | bool (list) | No |  | True = add a soft outer drop shadow, false = remove it. Connect to apply. |
| Content Alignment | string (list) | No | "" | Vertical alignment of the shape's text: TOP, MIDDLE, BOTTOM (blank to skip). |
| Autofit | string (list) | No | "" | NONE, TEXT_AUTOFIT or SHAPE_AUTOFIT (blank to skip). |
| Link URL | string (list) | No | "" | Hyperlink the whole shape to this URL (blank to skip). |
| Unit | string (list) | No | "pt" | Unit for Outline Weight (pt, in, mm, cm, m, ft). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateShapeProperties request JSON, one per shape. |

## Notes

The everything-in-one shape styler; [Shape Fill](ShapeFill.md), [Shape Outline](ShapeOutline.md),
[Shape Shadow](ShapeShadow.md), [Shape Link](ShapeLink.md) and [Shape Text Fit](ShapeTextFit.md) each
cover one slice of it with a smaller input list. Only connected inputs go into the field mask, so an
unconnected Fill Color leaves the current fill alone; a record with nothing set is skipped with a
warning rather than sent (an empty update would reset every property). Shadow here uses a fixed soft
look — for colour/blur/offset control use Shape Shadow. For styling at creation time,
[Create Shape](CreateShape.md) takes Color / Outline Weight directly.
