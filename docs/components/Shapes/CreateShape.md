# Create Shape

Builds `createShape` requests for one or more shapes.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Slide ID | string (list) | Yes | "" | Slide to place each shape on. Repeat one value to place many shapes on one slide; vary it to spread shapes across slides. |
| Shape Type | string (list) | No | "TEXT_BOX" | Shape type keyword; see [Shape Type](../Presets/ShapeTypePreset.md). |
| Width | double (list) | No | 3.0 | Shape width, in Unit. |
| Height | double (list) | No | 1.0 | Shape height, in Unit. |
| X | double (list) | No | 0 | Left position, in Unit. |
| Y | double (list) | No | 0 | Top position, in Unit. |
| Unit | string (list) | No | "in" | Unit for Width/Height/X/Y: in, pt, mm, cm, m, ft, or `emu` for a raw API value. The page is 10 × 5.625 in. |
| Object ID | string (list) | No | "" | Shape object ID. Auto-generated when blank, and resolved back out. |
| Color | colour (list) | No | None | Optional fill colour(s): a Grasshopper Colour Swatch, a `Gradient` output, or a panel containing `255,0,0`-style RGB. Unconnected = keep the shape's default fill. |
| Outline Weight | double (list) | No | 0 | Optional outline weight, in Unit (0 = no outline set). |
| Rotation | double (list) | No | 0 | Rotation in degrees, clockwise, about the shape's centre; [Rhino Shape Export](RhinoShapeExport.md)'s Rotation wires straight in. |
| Flip X | bool (list) | No | false | Mirror left-right, about the centre. |
| Flip Y | bool (list) | No | false | Mirror top-bottom, about the centre. |
| Skew X | double (list) | No | 0 | Lean the top edge sideways, in degrees (italic-style shear; positive = right). |
| Skew Y | double (list) | No | 0 | Lean the left edge, in degrees (positive = down). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `createShape` request(s), plus an `updateShapeProperties` request right after each shape's create request when Color or Outline Weight is supplied. |
| Object ID | string (list) | The resolved object ID of each shape; feed into downstream Object ID inputs. |

## Notes

**Placement is the full affine transform.** Rotation, Flip X/Y and Skew X/Y compose into one Slides
`transform` (`RequestBuilders.ElementTransform`), always about the element's *centre*, so a mirrored
shape stays where it was, a skewed one leans without drifting, and a rotated one spins in place. All
four are lists, so a `Range` into Rotation makes a fan, a `Series` into Skew X makes a lean that
increases across a row, and alternating Flip X mirrors every other element. Slides' own limits still
apply: skew is clamped to ±85°, and the API has no per-shape corner radius or path editing.

The Google Slides API has no color field on `createShape` itself, so supplying Color or Outline
Weight bundles a follow-up `updateShapeProperties` request automatically. Color is a native
Grasshopper Colour parameter; the plugin converts it to the API's 0-1 RGB JSON itself.

Slide ID repeated across records places many shapes on one slide (e.g. wire [Create Slide](../Slides/CreateSlide.md)'s
single Slide ID output, broadcast across a longer Width/Height/X/Y list); varied Slide ID (e.g. wired
from Create Slide's list output) spreads shapes across many slides in the same call.
