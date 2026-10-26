# Predefined Layout

A value-list preset of Google Slides predefined layout values, for use with the Layout input on
[Create Slide](../Slides/CreateSlide.md).

## Values

| Label | Value |
|---|---|
| Blank | `BLANK` |
| Title | `TITLE` |
| Title and Body | `TITLE_AND_BODY` |
| Title Only | `TITLE_ONLY` |
| Section Header | `SECTION_HEADER` |
| Section Title and Desc | `SECTION_TITLE_AND_DESCRIPTION` |
| One Column Text | `ONE_COLUMN_TEXT` |
| Main Point | `MAIN_POINT` |
| Caption Only | `CAPTION_ONLY` |
| Big Number | `BIG_NUMBER` |

## Feeds into

- [Create Slide](../Slides/CreateSlide.md)'s Layout input

## Notes

These are literal Google Slides `predefinedLayout` enum values, applied to the presentation's default
theme. They're ignored on Create Slide when a Layout ID (an existing custom Layout's object ID) is
also supplied — Layout ID always wins.
