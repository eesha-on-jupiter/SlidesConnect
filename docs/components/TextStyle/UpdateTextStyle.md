# Styled Text

Styles the text in one or more shapes or table cells with any combination of character traits, the Slides counterpart of DocsConnect's Styled Text.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape (or table) whose text to style. |
| Bold | bool (list) | No |  | Bold (connect to apply; unconnected = leave as is). |
| Italic | bool (list) | No |  | Italic (connect to apply). |
| Underline | bool (list) | No |  | Underline (connect to apply). |
| Strikethrough | bool (list) | No |  | Strikethrough (connect to apply). |
| Font Size | double (list) | No | 0 | Font size in points (>0 to apply). |
| Font Family | string (list) | No | "" | Font family name (blank to skip); see the Font Family preset. |
| Text Color | colour (list) | No |  | Text colour (connect to apply). |
| Highlight | colour (list) | No |  | Highlight (background) colour behind the text (connect to apply). |
| Link URL | string (list) | No | "" | Hyperlink URL (blank to skip). |
| Baseline | string (list) | No | "" | SUPERSCRIPT, SUBSCRIPT or NONE (blank to skip). |
| Small Caps | bool (list) | No |  | Small caps (connect to apply). |
| Row | int (list) | No | -1 | Table only: row index of the cell (-1 = Object ID is a shape, not a table). |
| Column | int (list) | No | -1 | Table only: column index of the cell. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | batchUpdate request JSON, one per record. |

## Notes

The field mask only carries the traits you connected, so styling a title's colour never touches its
weight. Records with nothing set are skipped with a warning. This styles the whole object's text; for
per-letter or per-word styling (a font-size wave, a colour gradient across a sentence) use
[Styled Text Box](../Text/StyledTextBox.md)'s Same Box mode, which computes the character ranges itself. Row/Column point the request at a table cell via
`cellLocation` instead of a shape.
