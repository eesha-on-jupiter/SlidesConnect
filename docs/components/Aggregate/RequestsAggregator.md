# Requests Aggregator

Collects Request JSON from across the plugin into a single Requests JSON list.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Request JSON | string (tree) | Yes | None | The request objects to gather. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Requests JSON | string (list) | The combined list of request objects. |

## Notes

The gathering point before a presentation gets edited. Wire in the Request JSON output from any
combination of Slides/Shapes/Text/Tables/Insert-tab components, and this flattens everything into
the single Requests JSON list [Batch Update](../Presentation/BatchUpdate.md) expects; order matters
when later requests need to reference something created earlier in the same batch (e.g. wire slide
creation before the content that targets those new slides).
