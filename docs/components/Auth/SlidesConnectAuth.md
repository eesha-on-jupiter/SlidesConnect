# Google Slides Auth

Validates a Google OAuth 2.0 token by checking it against Google's token validation endpoint and
returns its status and granted scopes.

## Inputs

| Name | Type | Required | Default | Description |
|---|---|---|---|---|
| Token | string (item) | Yes | "" | The OAuth 2.0 bearer token to validate. |

## Outputs

| Name | Type | Description |
|---|---|---|
| Status | string (item) | Result of the token check. |
| Response | string (item) | Raw response body from the token check, including granted scopes. |

## Notes

Auth is the first component to place on any canvas: every other component in this plugin needs a
valid token, so downstream components depend on a successful result from this one. Run it first on
any new canvas; if it fails, nothing downstream will work either.
