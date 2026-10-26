# Get Slide

Fetches a single page (a Slide, Layout, or Master) by its object ID.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token. |
| Presentation ID | string (item) | Yes | "" | ID of the presentation. |
| Slide ID | string (item) | Yes | "" | ID of the page to fetch — a slide, Layout, or Master ID all work, since they share the same underlying Page resource. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Response | string (item) | Raw response body — the full Page resource, including its `pageElements[]`. |
| Status | string (item) | Result of the get call. |

## Notes

Slides, Layouts, and Masters are all the same underlying `Page` resource in the Slides API (they
differ only in their `pageType`), so this one component works for any of them — it's what
[List Layout Placeholders](../Layouts/ListLayoutPlaceholders.md) uses internally to inspect a
Layout's placeholder elements.
