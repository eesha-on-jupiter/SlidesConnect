# Get Presentation

Fetches a presentation by ID, or by title when Presentation ID is left blank — the title is resolved
via a Drive `files.list` lookup, and the most-recently-modified match wins on ambiguity.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token. |
| Presentation ID | string (item) | No | "" | ID of the presentation to fetch. Leave blank to look up by Name instead. |
| Name | string (item) | No | "" | Presentation title to look up when Presentation ID is blank. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Response | string (item) | Raw response body — the full presentation resource, including its `layouts[]` array. |
| Presentation ID | string (item) | The resolved presentation ID (useful when you looked up by Name). |
| Status | string (item) | Result of the get call. |

## Notes

Name-based lookup requires the token to carry a Drive read scope
(`drive.readonly` or `drive.metadata.readonly`) in addition to the Slides `presentations` scope — see
[authentication.md](../../authentication.md) for the full scope list. If you only ever pass a
Presentation ID directly, the `presentations` scope alone is enough.
