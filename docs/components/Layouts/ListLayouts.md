# List Layouts

Lists the Layout pages available in a presentation, so a hand-designed custom layout can be found by
name and wired straight into [Create Slide](../Slides/CreateSlide.md)'s Layout ID input instead of
hunting for its object ID in raw JSON.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token. |
| Presentation ID | string (item) | Yes | "" | ID of the presentation. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Layout ID | string (list) | Object ID of each Layout page in the presentation. |
| Layout Name | string (list) | Display name of each Layout, index-matched to Layout ID. |

## Notes

Parses the `layouts[]` array from [Get Presentation](../Presentation/GetPresentation.md)'s response
internally, so you don't need to call Get Presentation separately just to find a Layout ID.
