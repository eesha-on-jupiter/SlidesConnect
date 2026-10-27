# Update Shape Transform

Builds requests to reposition, scale, or rotate one or more existing page elements.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the element(s) to transform. |
| X | double (list) | No | 0 | Left position, in Unit. |
| Y | double (list) | No | 0 | Top position, in Unit. |
| Scale X | double (list) | No | 1 | Horizontal scale factor. |
| Scale Y | double (list) | No | 1 | Vertical scale factor. |
| Rotation | double (list) | No | 0 | Rotation in degrees (clockwise). |
| Unit | string (list) | No | "in" | Unit for X/Y: in, pt, mm, cm, m, ft, or `emu` for a raw API value. The page is 10 × 5.625 in. |
| Flip X | bool (list) | No | false | Absolute mode: mirror left-right. |
| Flip Y | bool (list) | No | false | Absolute mode: mirror top-bottom. |
| Skew X | double (list) | No | 0 | Absolute mode: lean the top edge sideways, in degrees. |
| Skew Y | double (list) | No | 0 | Absolute mode: lean the left edge, in degrees. |
| Width | double (list) | No | 0 | Optional: the element's base width, in Unit. With Width and Height given, rotation/flip/skew pivot about the centre (as Create Shape does); with 0, about the top-left corner (the API default). |
| Height | double (list) | No | 0 | Optional: the element's base height, in Unit (see Width). |
| Relative | bool (list) | No | false | False = absolute: X/Y/Scale/Rotation replace the element's whole transform. True = move by X/Y as a delta, leaving size and rotation exactly as they are (Scale/Rotation ignored). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `updatePageElementTransform` request(s), one per record, applied as an absolute transform. |

## Notes

[Rhino Shape Export](RhinoShapeExport.md)'s X/Y/Rotation outputs feed this component's X/Y/Rotation
inputs directly, for repositioning an existing element from Rhino model geometry. (For a *new* shape,
[Create Shape](CreateShape.md) now takes Rotation directly, about the centre; this component is for
moving things that already exist.)

**Pivot.** Give Width and Height (the size the element was created with: Rhino Shape Export's W/H, or
`size.width/height` in Get Slide's JSON) and rotation, flip and skew pivot about the element's centre,
matching Create Shape. Leave them 0 and the API's own top-left pivot applies, which is fine for a
pure move or scale-1/no-rotation cases but makes a rotated element swing around its corner.

**Absolute mode replaces the element's whole matrix.** Slides stores an element as a base size × a
scale, and most layout placeholders (and anything resized by hand in the editor) carry a scale that
isn't 1, so absolute with the default Scale 1 snaps the element back to its base size. Either read
the element's current `transform.scaleX/scaleY` from [Get Slide](../Slides/GetSlide.md)'s JSON and
pass those, or use **Relative** to nudge it without touching its size.
