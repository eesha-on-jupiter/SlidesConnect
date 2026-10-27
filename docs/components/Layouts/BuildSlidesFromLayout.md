# Build Slides From Layout

The one-component path from "a Layout + flat lists" to a finished deck's requests. Given a Layout ID
and up to four flat lists (Title/Subtitle/Body/Photo URL), it looks up the layout's placeholders
itself, then for each record builds a `createSlide` (with `placeholderIdMappings`) plus the matching
`insertText`/`replaceImage` requests.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token. |
| Presentation ID | string (item) | Yes | "" | ID of the presentation. |
| Layout ID | string (item) | Yes | "" | The Layout's object ID (from [List Layouts](ListLayouts.md)). |
| Title | string (list) | No | "" | Title text, one per slide. |
| Subtitle | string (list) | No | "" | Subtitle text, one per slide. |
| Body | string (list) | No | "" | Body text, one per slide. |
| Photo URL | string (list) | No | "" | Public image URL, one per slide. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The full request sequence for the whole deck; wire straight into [Batch Update](../Presentation/BatchUpdate.md) (Requests Aggregator is only needed when combining with other content). |
| Slide ID | string (list) | Object ID of each new slide. |

## Notes

Makes one internal `getPage` call to find the layout's `TITLE`-or-`CENTERED_TITLE`/`SUBTITLE`/`BODY`/
`PICTURE` placeholders; both title placeholder type variants are treated as the Title role (a
title-slide-style layout reports `CENTERED_TITLE`, not `TITLE`, so matching only `TITLE` would
silently drop it).

No [List Layout Placeholders](ListLayoutPlaceholders.md), no ID-threading, no text-token matching
required from the user; this is the fastest path from a hand-designed branded Layout to a
data-driven deck. The lower-level [List Layouts](ListLayouts.md) + List Layout Placeholders +
[Create Slide](../Slides/CreateSlide.md) + [Insert Text](../Text/InsertText.md) +
[Replace Image](../Insert/ReplaceImage.md) wiring path still exists underneath for anyone who needs
finer per-element control. For the no-layout case, use [Create Slide](../Slides/CreateSlide.md)
(BLANK) + [Styled Text Box](../Text/StyledTextBox.md), which places each record's text where you say.

Feed lists longer than one item to build a full multi-slide deck from one Layout; this is the
concrete mechanism behind pushing an entire data-driven deck (e.g. from a CSV via
[Read CSV](ReadCsv.md) / [Get CSV Column](GetCsvColumn.md)) in a single component call.
