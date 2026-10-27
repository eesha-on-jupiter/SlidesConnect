# Create Slide

Builds `createSlide` requests for one or more slides.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Layout | string (list) | No | "BLANK" | A predefined layout keyword (see [Predefined Layout](../Presets/PredefinedLayoutPreset.md)). Ignored when Layout ID is supplied. |
| Insertion Index | int (list) | No | -1 | Zero-based slide position. -1 appends to the end. |
| Object ID | string (list) | No | "" | Slide object ID. Auto-generated when blank, and resolved back out on Slide ID. |
| Layout ID | string (list) | No | "" | An existing Layout's object ID (from [List Layouts](../Layouts/ListLayouts.md)). Wins over Layout when both are supplied. |
| Title Placeholder ID | string (list) | No | "" | A layout placeholder's object ID (from [List Layout Placeholders](../Layouts/ListLayoutPlaceholders.md)) to map to this slide's title. |
| Body Placeholder ID | string (list) | No | "" | Same, for the body placeholder. |
| Picture Placeholder ID | string (list) | No | "" | Same, for the picture placeholder. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `createSlide` request(s), one per record. |
| Slide ID | string (list) | The resolved object ID of each new slide; feed into downstream Slide ID inputs. |
| Title Object ID | string (list) | The new object ID assigned to the title placeholder mapping, if Title Placeholder ID was supplied. |
| Body Object ID | string (list) | Same, for the body placeholder. |
| Picture Object ID | string (list) | Same, for the picture placeholder. |

## Notes

Object ID is auto-generated when left blank and always resolved back out on Slide ID; this is the
mechanism behind parametric multi-slide wiring: feed Slide ID into [Create Shape](../Shapes/CreateShape.md)'s
or [Create Image](../Insert/CreateImage.md)'s Slide ID to target the slide(s) just created in the same
batch.

Layout ID (an existing Layout's object ID) wins over the predefined Layout keyword when both are
supplied. When Title/Body/Picture Placeholder ID are also supplied, this component populates
`createSlide.placeholderIdMappings`, mapping each layout placeholder to a fresh auto-generated object
ID resolved back out per role; this is the deterministic way to fill a layout's placeholders,
without relying on any text-token matching. For the common case of "a Layout plus flat content
lists," [Build Slides From Layout](../Layouts/BuildSlidesFromLayout.md) wraps this whole flow (plus
the placeholder discovery step) into one component call.
