# Get Image URL

Resolves images to the public URLs that [Create Image](CreateImage.md),
[Replace Image](ReplaceImage.md) and
[Build Slides From Layout](../Layouts/BuildSlidesFromLayout.md) need. Slides fetches image URLs
unauthenticated, so a local Bitmap or File Path has no way onto a slide without being hosted first;
this component uploads each one to your Google Drive, makes it link-public, and returns the resulting
URL. Already-public URLs pass straight through with no upload.

Button component; nothing is uploaded until you press **Get URLs**.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | OAuth 2.0 bearer token with the `drive.file` scope. Only used when a Bitmap or File Path is uploaded. |
| Bitmap | generic (list) | No | | In-memory image(s) to host (e.g. from another plugin). |
| File Path | string (list) | No | None | Local image path(s) to host, e.g. the File Path list from [Capture Camera View](CaptureCameraView.md). |
| URL | string (list) | No | None | Already-public image URL(s), passed through unchanged. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Image URL | string (list) | One publicly fetchable URL per record, in input order. Wire into Create Image's Image URL, Replace Image's Image URL, or Build Slides From Layout's Photo URL. |

## Notes

The three inputs are zipped by index: for each record, a URL wins over a Bitmap, which wins over a
File Path. A record with none of the three, or whose upload fails, yields an empty string (with a
runtime message) so the output list stays aligned with the input; downstream builders skip blanks.

Uploads land in the token owner's Drive root as `capture_<timestamp>_<i>.png` (Bitmap) or the
original file name (File Path), shared as "anyone with the link can view", and the URL returned is
`https://drive.google.com/uc?id=<fileId>`. Requires the
`https://www.googleapis.com/auth/drive.file` scope alongside `presentations`; see
[Authentication](../../authentication.md). Google's Slides-side limits still apply: PNG/JPG/GIF,
≤ 50 MB, ≤ 25 megapixels.
