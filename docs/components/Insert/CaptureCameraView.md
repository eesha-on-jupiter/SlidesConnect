# Capture Camera View

**AEC feature.** Repositions the Rhino viewport camera from a Plane and captures a PNG at each one —
e.g. one per floor, "sliding" the camera up a building.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Camera Plane | plane (list) | Yes | — | Camera plane(s) — origin = camera location, Z axis = view direction, Y axis = up. |
| Projection | string (list) | No | "Perspective" | Perspective or Parallel. |
| View Name | string (item) | No | "" | Named Rhino viewport to move and capture. Blank uses the active viewport. |
| File Path | string (list) | No | "" | Where to save each PNG — a full file path, or a folder for an auto-named file. Blank uses a temp folder. |
| Width | int (list) | No | 0 | Capture width in pixels. 0 = current viewport size. |
| Height | int (list) | No | 0 | Capture height in pixels. 0 = current viewport size. |
| Restore View | bool (item) | No | true | Put the camera back where it was after the last capture. |

## Outputs

| Name | Type | Description |
|---|---|---|
| File Path | string (list) | Path of each saved PNG file. |

## Notes

This is a `ButtonComponent` — it only fires on a literal mouse-click, never on canvas recompute.

This component moves the camera itself before each shot — set a Plane's
origin, Z-axis, and Y-axis to define exactly where the camera sits, which way it looks, and which way
is up. Feed a list of Planes (e.g. one per building floor, stepping up in Z) to capture a full series
of consistent photos in one call.

Does not upload anywhere itself — wire the File Path list into [Get Image URL](GetImageUrl.md) to
host the files on Google Drive, then feed the resulting public URLs into
[Create Image](CreateImage.md) or [Build Slides From Layout](../Layouts/BuildSlidesFromLayout.md). A
failed or invalid plane at one index is skipped (with a runtime warning) without aborting the rest of
the batch.

By default the viewport's camera and projection are put back where they were once the series is
done (**Restore View** = true), so capturing doesn't disturb your working view; set it false to leave
the viewport parked on the last plane.

Requires RhinoCommon — this component only works inside Rhino.
