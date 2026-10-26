# Highlight Text

Puts a highlight colour behind the text in one or more shapes. The Slides counterpart of DocsConnect's Highlight Text.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape whose text to style — from Create Shape / Styled Text Box's Object ID output, or Create Slide's Title/Body Object ID. |
| Highlight | colour (list) | No |  | Highlight (background) colour behind the text. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateTextStyle request JSON, one per record. |

## Notes

One trait, one request per record, styling the whole object's text. Docs' Highlight Text inserts new
text at an index; Slides has no document index, so this styles the text already in an object — wire the
Object ID from [Create Shape](../Shapes/CreateShape.md), [Styled Text Box](../Text/StyledTextBox.md) or
[Create Slide](../Slides/CreateSlide.md)'s placeholder outputs. The trait is a list: five Object IDs and
five values style five shapes independently; one value broadcasts. To combine several traits in one
request use [Styled Text](UpdateTextStyle.md); for per-letter or per-word styling inside one box use
[Styled Text Box](../Text/StyledTextBox.md)'s Same Box mode.
