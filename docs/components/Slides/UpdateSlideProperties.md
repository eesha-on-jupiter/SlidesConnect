# Update Slide Background

Builds requests to set one or more slides' background fill.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Slide ID | string (list) | Yes | "" | Slide(s) to update. |
| Color | colour (list) | Yes | | A Grasshopper Colour Swatch, or a panel containing `255,0,0`-style RGB. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `updatePageProperties` request(s), one per record. |

## Notes

Feed a matched-length Slide ID / Color pair to set a different background per slide, or repeat a
single Color entry to apply the same background across every Slide ID.
