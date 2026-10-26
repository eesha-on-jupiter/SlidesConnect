# Get CSV Column

Pulls one column out of [Read CSV](ReadCsv.md)'s Columns tree by header name (case-insensitive)
instead of by branch index — robust to the CSV's column order changing.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Columns | string (tree) | Yes | — | The tree from [Read CSV](ReadCsv.md). |
| Headers | string (list) | Yes | — | The headers list from [Read CSV](ReadCsv.md). |
| Header Name | string (item) | No | "" | The column header to pull, matched case-insensitively. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Values | string (list) | The flat list of values for the matched column. |
| Found | bool (item) | Whether Header Name matched a column. |

## Notes

Output is already the flat list [Build Slides From Layout](BuildSlidesFromLayout.md) /
[Styled Text Box](../Text/StyledTextBox.md) want for Title/Subtitle/Body/Photo URL or Text — wire
this component's Values straight into one of those inputs, one Get CSV Column per column you need.
