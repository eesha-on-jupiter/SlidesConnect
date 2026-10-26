# Authentication

SlidesConnect uses OAuth 2.0 Bearer tokens. The plugin does not run its own OAuth flow — you
supply a valid access token, and every HTTP component sends it as `Authorization: Bearer {token}`.

## Scopes you need

| Scope | Needed for |
|---|---|
| `https://www.googleapis.com/auth/presentations` | All Slides API calls: get/create/batchUpdate, list layouts and placeholders, Build Slides From Layout |
| `https://www.googleapis.com/auth/drive.readonly` (or `drive.metadata.readonly`) | [Get Presentation](components/Presentation/GetPresentation.md)'s optional `Name` lookup, which resolves a presentation by title via Drive's `files.list` |
| `https://www.googleapis.com/auth/drive.file` | [Get Image URL](components/Insert/GetImageUrl.md) when hosting a Bitmap or File Path: it uploads a new Drive file and sets it link-public, both of which `drive.file` covers for files the app itself created |

If you only ever pass a `Presentation ID` directly (never the `Name` input on Get Presentation) and
supply images by public URL rather than local file, the `presentations` scope alone is enough.

## Getting a token

The quickest way to get a short-lived Bearer token for testing is
[Google OAuth 2.0 Playground](https://developers.google.com/oauthplayground/):

1. Open the Playground and select the scope above under "Google Slides API v1" (and, under "Drive
   API v3", `drive.file` if you upload local images and/or `drive.readonly` if you need name lookup).
2. Authorize, then exchange the authorization code for tokens.
3. Copy the **Access token** — this is what goes into every component's `Token` input.

Access tokens from the Playground expire in about an hour. For a longer-lived setup, register your
own OAuth client and run a standard installed-app or service-account flow, then feed the resulting
access token into the plugin the same way.

## Wiring the token in

Every HTTP component ([Auth](components/Auth/index.md), [Presentation](components/Presentation/index.md),
and the discovery components in [Slides](components/Slides/index.md)/[Layouts](components/Layouts/index.md))
takes a `Token` input. Wire one Panel with your token into all of them, or read it from an environment
variable via a Grasshopper script component if you'd rather not paste it onto the canvas directly.

## Verifying it works

[Google Slides Auth](components/Auth/SlidesConnectAuth.md) calls Google's token validation
endpoint and returns the granted scopes on its `Status` / `Response` outputs. Run this first on any
new canvas — if it fails, nothing downstream will work either.

## What "Status" means

Every HTTP component reports a `Status` output. A successful call reports the HTTP status and
outcome; a failed call reports the HTTP error code and Google's error message body (e.g. `401` for
an expired/invalid token, `403` for a missing scope). Read `Status` before trusting `Response` on any
HTTP component.
