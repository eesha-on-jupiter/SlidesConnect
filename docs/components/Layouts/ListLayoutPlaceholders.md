# List Layout Placeholders

Lists the placeholder page elements on a Layout page — each entry's own object ID and placeholder
type (TITLE, BODY, PICTURE, etc).

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token. |
| Presentation ID | string (item) | Yes | "" | ID of the presentation. |
| Layout ID | string (item) | Yes | "" | Object ID of the Layout to inspect (from [List Layouts](ListLayouts.md)). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Placeholder ID | string (list) | Object ID of each placeholder on the layout. |
| Placeholder Type | string (list) | Placeholder type of each entry (TITLE, CENTERED_TITLE, BODY, SUBTITLE, PICTURE, etc), index-matched to Placeholder ID. |

## Notes

Feed a Placeholder ID into [Create Slide](../Slides/CreateSlide.md)'s Title/Body/Picture Placeholder
ID inputs to populate that placeholder deterministically via `placeholderIdMappings`, instead of
relying on text-token matching. For the common case of filling a Layout's Title/Body/Picture
placeholders from flat data lists, [Build Slides From Layout](BuildSlidesFromLayout.md) wraps this
whole discovery step for you.
