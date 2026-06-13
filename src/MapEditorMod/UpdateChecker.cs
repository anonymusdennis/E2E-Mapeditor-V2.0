using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace MapEditorMod
{
    /// <summary>
    /// Checks the GitHub Releases API on a background thread for a newer version
    /// and posts an <see cref="UpdateDialog"/> to the main thread when one is found.
    /// </summary>
    internal static class UpdateChecker
    {
        private const string RepoUrl = "https://github.com/anonymusdennis/E2E-Mapeditor-V2.0";
        private const string ApiUrl =
            "https://api.github.com/repos/anonymusdennis/E2E-Mapeditor-V2.0/releases/latest";

        /// <summary>Start the background check; returns immediately.</summary>
        internal static void CheckAsync()
        {
            var t = new Thread(CheckOnThread) { IsBackground = true, Name = "UpdateChecker" };
            t.Start();
        }

        // ── background thread ──────────────────────────────────────────────────

        private static void CheckOnThread()
        {
            // Save and restore the certificate callback so this update probe does
            // not permanently affect other HTTPS connections in the process.
            // Older Unity Mono builds may not trust the GitHub CA, so we accept
            // the certificate only for this request.
            var prevCallback = ServicePointManager.ServerCertificateValidationCallback;
            var prevProtocol = ServicePointManager.SecurityProtocol;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    (_, __, ___, ____) => true;
                ServicePointManager.SecurityProtocol =
                    (SecurityProtocolType)3072; // Tls12

                var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
                request.UserAgent = "MapEditorMod/" + PluginInfo.Version;
                request.Method = "GET";
                request.Timeout = 10000;
                request.Accept = "application/vnd.github.v3+json";

                string json;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }

                string tagName = Extract(json, "tag_name");
                string htmlUrl = Extract(json, "html_url");

                if (tagName == null)
                {
                    return;
                }

                string releaseUrl = htmlUrl ?? RepoUrl + "/releases/latest";
                E2EApi.MainThread.Post(() => OnVersionReceived(tagName, releaseUrl));
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Update check failed: " + e.Message);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = prevCallback;
                ServicePointManager.SecurityProtocol = prevProtocol;
            }
        }

        /// <summary>Extract the first string value of a JSON key (no full parser needed).</summary>
        private static string Extract(string json, string key)
        {
            var m = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\"([^\"]+)\"");
            return m.Success ? m.Groups[1].Value : null;
        }

        // ── main thread ────────────────────────────────────────────────────────

        private static void OnVersionReceived(string tagName, string releaseUrl)
        {
            // Strip leading 'v' (tags like "v2.1.0" → "2.1.0")
            string latest = tagName.TrimStart('v');

            // Skip if user opted out of this specific version
            if (Plugin.CfgSkipVersion.Value == latest)
            {
                Plugin.Log.LogInfo("Update check: version " + latest + " skipped by user preference.");
                return;
            }

            // Compare using System.Version for correct semantic ordering
            bool isNewer;
            try
            {
                isNewer = new Version(latest) > new Version(PluginInfo.Version);
            }
            catch
            {
                // Fallback: simple string equality check
                isNewer = latest != PluginInfo.Version;
            }

            if (!isNewer)
            {
                Plugin.Log.LogInfo("Update check: already on latest version (" + latest + ").");
                return;
            }

            Plugin.Log.LogInfo("Update check: new version available — " + latest);
            UpdateDialog.Show(latest, releaseUrl);
        }
    }
}
