# Space Below Text

Adds space below the paragraphs in one or more shapes. The Slides counterpart of DocsConnect's Space Below Text.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Object ID | string (list) | Yes | "" | Object ID(s) of the shape whose text to style — from Create Shape / Styled Text Box's Object ID output, or Create Slide's Title/Body Object ID. |
| Space Below | double (list) | Yes | 6 | Space below each paragraph in points. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | updateParagraphStyle request JSON, one per record. |

## Notes

One trait, one request per record, styling the whole object's text. Docs' Space Below Text inserts new
text at an index; Slides has no document index, so this styles the text already in an object — wire the
Object ID from [Create Shape](../Shapes/CreateShape.md), [Styled Text Box](../Text/StyledTextBox.md) or
[Create Slide](../Slides/CreateSlide.md)'s placeholder outputs. The trait is a list: five Object IDs and
five values style five shapes independently; one value broadcasts. To combine several traits in one
request use [Styled Paragraph](UpdateParagraphStyle.md); for per-letter or per-word styling inside one box use
[Styled Text Box](../Text/StyledTextBox.md)'s Same Box mode.
