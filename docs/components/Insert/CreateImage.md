# Create Image

Builds `createImage` requests for one or more images, placed from a public URL.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Slide ID | string (list) | Yes | "" | Slide to place each image on. |
| Image URL | string (list) | Yes | "" | Public URL of the image. |
| Width | double (list) | No | 3.0 | Image width, in Unit. |
| Height | double (list) | No | 3.0 | Image height, in Unit. |
| X | double (list) | No | 0 | Left position, in Unit. |
| Y | double (list) | No | 0 | Top position, in Unit. |
| Unit | string (list) | No | "in" | Unit for Width/Height/X/Y — in, pt, mm, cm, m, ft, or `emu` for a raw API value. The page is 10 × 5.625 in. |
| Object ID | string (list) | No | "" | Image object ID. Auto-generated when blank, and resolved back out. |
| Rotation | double (list) | No | 0 | Rotation in degrees, clockwise, about the image\'s centre. |
| Flip X | bool (list) | No | false | Mirror left-right, about the centre. |
| Flip Y | bool (list) | No | false | Mirror top-bottom, about the centre. |
| Skew X | double (list) | No | 0 | Lean the top edge sideways, in degrees (italic-style shear; positive = right). |
| Skew Y | double (list) | No | 0 | Lean the left edge, in degrees (positive = down). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `createImage` request(s), one per record. |
| Object ID | string (list) | The resolved object ID of each image. |

## Notes

**Placement is the full affine transform.** Rotation, Flip X/Y and Skew X/Y compose into one Slides
`transform` (`RequestBuilders.ElementTransform`), always about the element's *centre* — so a mirrored
image stays where it was, a skewed one leans without drifting, and a rotated one spins in place. All
four are lists, so a `Range` into Rotation makes a fan, a `Series` into Skew X makes a lean that
increases across a row, and alternating Flip X mirrors every other element. Slides' own limits still
apply: skew is clamped to ±85°, and the API has no per-image corner radius or path editing.

The Image URL must be publicly reachable by Google's servers — a locally-saved file from
[Capture Camera View](CaptureCameraView.md) needs to be
hosted first — [Get Image URL](GetImageUrl.md) does that via Google Drive. This places a brand-new image element; to swap an existing one's content
instead, use [Replace Image](ReplaceImage.md).
