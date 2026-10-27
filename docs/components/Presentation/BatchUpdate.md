# Batch Update

Pushes a list of Request JSON objects to a live presentation in one `batchUpdate` call, the single
write endpoint every content-building component's output ultimately flows into.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token. |
| Presentation ID | string (item) | Yes | "" | ID of the presentation to update. |
| Requests JSON | string (tree) | Yes | None | The request objects: a list or a tree (flattened branch by branch); typically the output of [Requests Aggregator](../Aggregate/RequestsAggregator.md). |

## Outputs

| Name | Type | Description |
|---|---|---|
| Replies | string (list) | One reply object per request submitted, index-matched to Requests JSON. |
| Status | string (item) | Result of the batch call. |
| Response | string (item) | Raw response body. |

## Notes

Google Slides has a single flat `requests[]` array; there's no per-request-type endpoint, so every
JSON-emitting component in this plugin (Slides/Shapes/Text/Tables/Insert tabs) feeds this same
component via [Requests Aggregator](../Aggregate/RequestsAggregator.md). Check `Status` before
trusting `Replies`; a partial failure surfaces there.

**Pressing Send twice.** Builder components generate their object IDs when they compute, and
pressing Send doesn't recompute them, so a second push would re-send the same IDs and Google would
reject the whole batch with "object ID already exists". Batch Update remembers which IDs it has
already created in each presentation; on a repeat push it swaps those for fresh ones (consistently
across the batch, so a new slide and the shapes targeting it move together) and adds a remark. The
result is a second copy of the content. The upstream Slide ID / Object ID outputs still name the
first copy; the new IDs are in `Replies`.
