# Replace All Text

Builds one `replaceAllText` request that finds and replaces text across the whole presentation.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Find Text | string (list) | Yes | "" | Text (or token, e.g. `{{name}}`) to search for. |
| Replace Text | string (list) | Yes | "" | Replacement text. |
| Match Case | bool (list) | No | false | Whether the search is case-sensitive. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `replaceAllText` request. |

## Notes

This is the one component whose output is item-access rather than list — it targets the whole
presentation in a single request. It still fans out cleanly into
[Requests Aggregator](../Aggregate/RequestsAggregator.md) alongside every other component's list
output.
