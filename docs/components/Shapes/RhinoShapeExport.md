# Rhino Shape Export

**AEC feature.** Converts one or more Rhino rectangles plus a unit into Slides coordinates in that
same unit, flipping Rhino's Y-up coordinate system to Slides' top-left origin and optionally fitting
to the standard page. Draw your layout in Rhino's Top view with the world origin at the page's
bottom-left corner (the page is 10 × 5.625 in) and it lands on the slide the way it looks in Rhino.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Rectangle | rectangle (list) | Yes | — | Rectangle(s) defining the element frame. |
| Unit | string (list) | No | "in" | Model unit of the rectangle — in, mm, cm, m, ft, or pt. |
| Fit To Page | bool (list) | No | false | Scale and center the frame to fit the standard 10×5.625 in page. |

## Outputs

| Name | Type | Description |
|---|---|---|
| X | double (list) | Left position on the slide, in Unit. |
| Y | double (list) | Top position on the slide, in Unit — already flipped from Rhino's Y-up. |
| Width | double (list) | Width, in Unit. |
| Height | double (list) | Height, in Unit. |
| Rotation | double (list) | Rotation in degrees, clockwise (Slides convention). |
| Unit | string (list) | The unit the outputs are in — wire into the downstream component's Unit. |

## Notes

This is the bridge that makes Rhino model geometry into placed slide elements real — outputs feed
[Create Shape](CreateShape.md)/[Create Image](../Insert/CreateImage.md)/
[Update Shape Transform](UpdateShapeTransform.md)'s X/Y/Width/Height/Rotation inputs directly —
wire the **Unit** output into their Unit input too, so both sides agree (the outputs are in your
model unit, not raw EMU). An
invalid rectangle at a given index is skipped (with a runtime warning) without aborting the rest of
the batch — feed a whole list of Rhino rectangles (e.g. one per floor plate) to place many elements
in one call.

Requires RhinoCommon/`Rhino.Geometry` — this component only works inside Rhino, unlike the pure JSON
builders elsewhere in the plugin.
