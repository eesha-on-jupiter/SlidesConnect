# Bullet List

Turns the paragraphs in one or more shapes or table cells into a bulleted or numbered list, the Slides counterpart of DocsConnect's Bullet List.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape (or table) whose paragraphs to bullet. |
| Bullet Preset | string (list) | No | "BULLET_DISC_CIRCLE_SQUARE" | Google Slides bullet preset; see the Bullet Preset dropdown. NONE removes bullets. |
| Row | int (list) | No | -1 | Table only: row index of the cell (-1 = Object ID is a shape, not a table). |
| Column | int (list) | No | -1 | Table only: column index of the cell. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | createParagraphBullets (or deleteParagraphBullets) request JSON, one per record. |

## Notes

Each paragraph (line) of the shape's text becomes one list item; put the items in first with
[Insert Text](../Text/InsertText.md) (one string with newlines) or [Styled Text Box](../Text/StyledTextBox.md).
Slides' preset names differ from Docs': numbered lists are `NUMBERED_DIGIT_…`, not `NUMBERED_DECIMAL_…`;
the [Bullet Preset](../Presets/BulletPreset.md) dropdown has the right strings. `NONE` emits
`deleteParagraphBullets` instead.
