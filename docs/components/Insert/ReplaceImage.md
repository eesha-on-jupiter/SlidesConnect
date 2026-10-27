# Replace Image

Builds `replaceImage` requests that swap the image content of one or more existing image/picture-
placeholder elements with a new image URL.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the existing image element(s) to replace. |
| Image URL | string (list) | Yes | "" | New public image URL. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `replaceImage` request(s), one per record. |

## Notes

Different from [Create Image](CreateImage.md), which places a brand-new image element; this targets
an already-inserted image (e.g. [Create Slide](../Slides/CreateSlide.md)'s Picture Object ID output
when a Layout has a picture placeholder) rather than a position, so it feeds
[Requests Aggregator](../Aggregate/RequestsAggregator.md) directly.
