using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlidesConnect
{
    // Single HTTP client for the Google Slides API — the only file that touches HttpClient.
    // Every method maps to one validated entry in spec/call-library-validated.json by endpoint_id.
    public sealed class SlidesConnectClient
    {
        #region Constructor and setup

        private const string TokenInfoUrl = "https://www.googleapis.com/oauth2/v3/tokeninfo";

        // one process-wide client: every component solve used to new up its own (never disposed),
        // which leaks a connection pool per solve and can exhaust sockets over a long Rhino session.
        // The bearer token is per-request (NewRequest), so sharing is safe across tokens.
        private static readonly HttpClient _http = new HttpClient { BaseAddress = new Uri("https://slides.googleapis.com") };
        private readonly string _token;

        public SlidesConnectClient(string token)
        {
            _token = (token ?? "").Trim();
        }

        private HttpRequestMessage NewRequest(HttpMethod method, string relativeUrl)
        {
            var req = new HttpRequestMessage(method, relativeUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return req;
        }

        #endregion

        #region Auth

        // auth_check — validates the OAuth access token via the tokeninfo endpoint (different host,
        // token passed as the access_token query param, not a bearer header).
        public async Task<Tuple<bool, string, string>> CheckTokenAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_token)) return Fail("Token is empty.");

                var url = TokenInfoUrl + "?access_token=" + Uri.EscapeDataString(_token);
                var req = new HttpRequestMessage(HttpMethod.Get, url);
                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + " — token invalid or expired.\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Presentation methods

        // create_presentation — POST /v1/presentations (only "title" is honored on create).
        public async Task<Tuple<bool, string, string>> CreatePresentationAsync(string title)
        {
            try
            {
                var payload = new JObject { ["title"] = title ?? "" };
                var req = NewRequest(HttpMethod.Post, "/v1/presentations");
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        // get_presentation — GET /v1/presentations/{presentationId}
        public async Task<Tuple<bool, string, string>> GetPresentationAsync(string presentationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(presentationId)) return Fail("Presentation ID is empty.");

                var req = NewRequest(HttpMethod.Get, "/v1/presentations/" + Uri.EscapeDataString(presentationId));
                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        // batch_update — POST /v1/presentations/{presentationId}:batchUpdate
        // requestJsonItems are individual request objects; this wraps them in { "requests": [ ... ] }.
        public async Task<Tuple<bool, string, string>> BatchUpdateAsync(string presentationId, IEnumerable<string> requestJsonItems)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(presentationId)) return Fail("Presentation ID is empty.");

                var requests = new JArray();
                if (requestJsonItems != null)
                {
                    foreach (var item in requestJsonItems)
                    {
                        if (string.IsNullOrWhiteSpace(item)) continue;
                        requests.Add(JToken.Parse(item));
                    }
                }
                if (requests.Count == 0) return Fail("No requests supplied — wire at least one Request JSON.");

                var payload = new JObject { ["requests"] = requests };
                var url = "/v1/presentations/" + Uri.EscapeDataString(presentationId) + ":batchUpdate";
                var req = NewRequest(HttpMethod.Post, url);
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Drive lookup and upload

        // find_presentation_by_name — GET https://www.googleapis.com/drive/v3/files (different host,
        // Drive API, not Slides). Resolves a presentation title to its ID via the most-recently-
        // modified match. Requires the token to also carry a Drive read scope (e.g. drive.readonly)
        // in addition to the Slides scope — Slides' own API has no way to search by title.
        public async Task<Tuple<bool, string, string>> FindPresentationIdByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) return Fail("Name is empty.");
                if (string.IsNullOrWhiteSpace(_token)) return Fail("Token is empty.");

                string escapedName = name.Trim().Replace("\\", "\\\\").Replace("'", "\\'");
                string q = "name = '" + escapedName + "' and mimeType = 'application/vnd.google-apps.presentation' and trashed = false";
                string url = "https://www.googleapis.com/drive/v3/files"
                            + "?q=" + Uri.EscapeDataString(q)
                            + "&orderBy=" + Uri.EscapeDataString("modifiedTime desc")
                            + "&pageSize=1"
                            + "&fields=" + Uri.EscapeDataString("files(id,name,modifiedTime)");

                var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + " — Drive lookup failed (check the token has drive.readonly scope).\n" + body);

                var files = JObject.Parse(body)["files"] as JArray;
                if (files == null || files.Count == 0) return Fail("No presentation found named \"" + name.Trim() + "\".");

                string id = (string)files[0]["id"] ?? "";
                if (string.IsNullOrWhiteSpace(id)) return Fail("Drive returned a match with no file ID.");

                return Ok(id);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        // upload_image — POST https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart,
        // then POST /permissions {role: reader, type: anyone}. Slides' createImage/replaceImage only
        // accept a URL Google can fetch unauthenticated, so a local Bitmap/File Path has no way onto
        // a slide without being hosted first — this hosts it on the user's own Drive and returns the
        // link-public download URL. Requires the drive.file scope (covers files the app itself
        // created, which is all this touches). Same pattern as DocsConnect/SheetsConnect's client.
        public async Task<Tuple<bool, string, string>> UploadImageAndGetUrlAsync(byte[] imageBytes, string fileName, string mimeType)
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0) return Fail("Image data is empty.");
                if (string.IsNullOrWhiteSpace(_token)) return Fail("Token is empty.");

                string uploadUrl = "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart&fields=" + Uri.EscapeDataString("id");
                var metadata = new JObject
                {
                    ["name"] = string.IsNullOrWhiteSpace(fileName) ? "image.png" : fileName,
                    ["mimeType"] = mimeType
                };

                using (var content = new MultipartContent("related"))
                {
                    var metadataPart = new StringContent(metadata.ToString(Formatting.None), Encoding.UTF8, "application/json");
                    var mediaPart = new ByteArrayContent(imageBytes);
                    mediaPart.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                    content.Add(metadataPart);
                    content.Add(mediaPart);

                    var uploadReq = NewRequest(HttpMethod.Post, uploadUrl);
                    uploadReq.Content = content;

                    var uploadRes = await _http.SendAsync(uploadReq).ConfigureAwait(false);
                    string uploadBody = await uploadRes.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!uploadRes.IsSuccessStatusCode)
                        return Fail("HTTP " + (int)uploadRes.StatusCode + " " + uploadRes.ReasonPhrase + " — Drive upload failed (check the token has drive.file scope).\n" + uploadBody);

                    string fileId = (string)JObject.Parse(uploadBody)["id"];
                    if (string.IsNullOrWhiteSpace(fileId)) return Fail("Upload succeeded but response had no file id:\n" + uploadBody);

                    var permReq = NewRequest(HttpMethod.Post, "https://www.googleapis.com/drive/v3/files/" + Uri.EscapeDataString(fileId) + "/permissions");
                    var permPayload = new JObject { ["role"] = "reader", ["type"] = "anyone" };
                    permReq.Content = new StringContent(permPayload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                    var permRes = await _http.SendAsync(permReq).ConfigureAwait(false);
                    string permBody = await permRes.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!permRes.IsSuccessStatusCode)
                        return Fail("Uploaded (id=" + fileId + ") but could not make it link-public: HTTP " + (int)permRes.StatusCode + " " + permRes.ReasonPhrase + "\n" + permBody);

                    return Ok("https://drive.google.com/uc?id=" + fileId);
                }
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Page methods

        // get_page — GET /v1/presentations/{presentationId}/pages/{pageObjectId}
        public async Task<Tuple<bool, string, string>> GetPageAsync(string presentationId, string pageObjectId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(presentationId)) return Fail("Presentation ID is empty.");
                if (string.IsNullOrWhiteSpace(pageObjectId)) return Fail("Page ID is empty.");

                var url = "/v1/presentations/" + Uri.EscapeDataString(presentationId)
                        + "/pages/" + Uri.EscapeDataString(pageObjectId);
                var req = NewRequest(HttpMethod.Get, url);
                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        // get_page_thumbnail — GET /v1/presentations/{presentationId}/pages/{pageObjectId}/thumbnail
        public async Task<Tuple<bool, string, string>> GetPageThumbnailAsync(string presentationId, string pageObjectId, string thumbnailSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(presentationId)) return Fail("Presentation ID is empty.");
                if (string.IsNullOrWhiteSpace(pageObjectId)) return Fail("Page ID is empty.");

                var url = "/v1/presentations/" + Uri.EscapeDataString(presentationId)
                        + "/pages/" + Uri.EscapeDataString(pageObjectId) + "/thumbnail";
                if (!string.IsNullOrWhiteSpace(thumbnailSize))
                    url += "?thumbnailProperties.thumbnailSize=" + Uri.EscapeDataString(thumbnailSize.Trim());

                var req = NewRequest(HttpMethod.Get, url);
                var res = await _http.SendAsync(req).ConfigureAwait(false);
                string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                    return Fail("HTTP " + (int)res.StatusCode + " " + res.ReasonPhrase + "\n" + body);

                return Ok(body);
            }
            catch (Exception ex) { return Fail(ex.ToString()); }
        }

        #endregion

        #region Private helpers

        private static Tuple<bool, string, string> Ok(string body)
            => new Tuple<bool, string, string>(true, body, "");

        private static Tuple<bool, string, string> Fail(string error)
            => new Tuple<bool, string, string>(false, "", error);

        #endregion
    }
}
