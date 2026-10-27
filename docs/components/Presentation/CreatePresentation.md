# Create Presentation

Creates a new, empty Google Slides presentation.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token. |
| Title | string (item) | No | "Untitled presentation" | Title of the new presentation. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Presentation ID | string (item) | The new presentation's ID; everything downstream chains from this. |
| Status | string (item) | Result of the create call. |
| Response | string (item) | Raw response body. |

## Notes

Wire this component's `Presentation ID` output into every other component's `Presentation ID` (or
`Slide ID`, on Slides-tab components) input downstream to work against the deck you just created.
