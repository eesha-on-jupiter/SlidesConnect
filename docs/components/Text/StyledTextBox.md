# Styled Text Box

Creates a text box per record, inserts the text, and applies any combination of character, paragraph and box styles in one call, the Slides counterpart of DocsConnect's Styled Text.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Slide ID | string (list) | Yes | "" | Slide ID(s) to place the text box on. |
| Text | string (list) | Yes | "" | Text for each box. Newlines make separate paragraphs within the box. |
| X | double (list) | No | 0.5 | Left position, in Unit. |
| Y | double (list) | No | 0.5 | Top position, in Unit. |
| Width | double (list) | No | 4.0 | Box width, in Unit. |
| Height | double (list) | No | 1.0 | Box height, in Unit. |
| Unit | string (list) | No | "in" | Unit for X/Y/Width/Height: in, pt, mm, cm, m, ft or emu. The page is 10 × 5.625 in. |
| Object ID | string (list) | No | "" | Optional: your own ID(s) for the new box. Blank = auto-generated; the resolved ID comes out of the Object ID output either way. |
| Bold | bool (list) | No |  | Bold (connect to apply). |
| Italic | bool (list) | No |  | Italic (connect to apply). |
| Underline | bool (list) | No |  | Underline (connect to apply). |
| Strikethrough | bool (list) | No |  | Strikethrough (connect to apply). |
| Font Size | double (list) | No | 0 | Font size in points (>0 to apply). |
| Font Family | string (list) | No | "" | Font family name (blank to skip); see the Font Family preset. |
| Text Color | colour (list) | No |  | Text colour (connect to apply). |
| Highlight | colour (list) | No |  | Highlight (background) colour behind the text (connect to apply). |
| Link URL | string (list) | No | "" | Hyperlink URL (blank to skip). |
| Baseline | string (list) | No | "" | SUPERSCRIPT, SUBSCRIPT or NONE (blank to skip). |
| Alignment | string (list) | No | "" | Paragraph alignment: START, CENTER, END, JUSTIFIED (blank to skip). |
| Line Spacing | double (list) | No | 0 | Line spacing percent, e.g. 115 (>0 to apply). |
| Bullet Preset | string (list) | No | "" | Bullet preset to list the paragraphs, e.g. BULLET_DISC_CIRCLE_SQUARE (blank = no bullets). |
| Fill Color | colour (list) | No |  | Box background fill colour (connect to apply). |
| Content Alignment | string (list) | No | "" | Vertical alignment of the text in the box: TOP, MIDDLE, BOTTOM (blank to skip). |
| Autofit | string (list) | No | "" | NONE, TEXT_AUTOFIT (shrink text to fit) or SHAPE_AUTOFIT (grow box to fit). Blank to skip. |
| Rotation | double (list) | No | 0 | Rotation in degrees, clockwise, about the box's centre. |
| Flip X | bool (list) | No | false | Mirror left-right, about the centre. |
| Flip Y | bool (list) | No | false | Mirror top-bottom, about the centre. |
| Skew X | double (list) | No | 0 | Lean the top edge sideways, in degrees (italic-style shear; positive = right). |
| Skew Y | double (list) | No | 0 | Lean the left edge, in degrees (positive = down). |
| Same Box | bool (item) | No | false | False: one box per Text item. True: all Text items join into ONE box (separated by Separator), each keeping its own style. |
| Separator | string (item) | No | "" | Same Box only: inserted between items: `""` for letter-by-letter, `" "` for word-by-word. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | createShape + insertText + style requests for every box, in order. |
| Object ID | string (list) | Resolved object ID of each new text box; feed Styled Text / Styled Paragraph / Bullet List for further styling. |

## Notes

**Placement is the full affine transform.** Rotation, Flip X/Y and Skew X/Y compose into one Slides
`transform` (`RequestBuilders.ElementTransform`), always about the element's *centre*, so a mirrored
box stays where it was, a skewed one leans without drifting, and a rotated one spins in place. All
four are lists, so a `Range` into Rotation makes a fan, a `Series` into Skew X makes a lean that
increases across a row, and alternating Flip X mirrors every other element. Slides' own limits still
apply: skew is clamped to ±85°, and the API has no per-box corner radius or path editing.

Emits, per record and in the order the API needs them: `createShape` (TEXT_BOX), `insertText`, then an
`updateTextStyle`, `updateParagraphStyle`, `createParagraphBullets` and `updateShapeProperties` only for the
trait groups you actually connected. Only the inputs you connect are applied; an unconnected Bold or an
empty Font Family is simply left out of the field mask.

**Same Box: per-letter / per-word styling without indices.** Turn it on and every Text item becomes
one *run* inside a single box; the per-run inputs (Font Size, Text Color, Bold, …) are index-matched
to the Text list, and the component computes each run's character range from the fragment lengths;
you never touch an index. One letter per item plus a `Range` → `Sine` → `Remap` into Font Size gives
a wave across a word; one word per item plus a `Gradient` into Text Color gives a colour sweep across
a sentence. Box position/size, alignment, bullets, fill and rotation come from the first record. With
a tree of Text, each branch becomes its own box.

For traits not exposed here (space above/below, indents, small caps) wire the **Object ID** output
into [Styled Text](../TextStyle/UpdateTextStyle.md) / [Styled Paragraph](../TextStyle/UpdateParagraphStyle.md).
For text in an existing shape or a layout placeholder, use Styled Text directly.
