# Delete Slide

Builds requests to delete one or more slides or page elements.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Slide ID | string (list) | Yes | "" | Slide ID(s) to delete — e.g. Create Slide's Slide ID output. Any element's Object ID works too: `deleteObject` removes shapes, images and tables the same way. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Request JSON | string (list) | The `deleteObject` request(s), one per record. |

## Notes

Works on any object ID — a slide, a shape, an image, a table — since `deleteObject` is generic across
page element types in the Slides API.
