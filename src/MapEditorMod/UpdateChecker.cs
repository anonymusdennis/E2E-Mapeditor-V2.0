using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace MapEditorMod
{
    /// <summary>
    /// Checks the GitHub Releases API for a newer version and shows an
    /// <see cref="UpdateDialog"/> when one is found.
    ///
    /// The primary transport is <see cref="WWW"/>, which uses Unity's native
    /// HTTP stack: the game's own Mono runtime (Unity 5.5 / .NET 3.5) has no
    /// TLS 1.2 support, so a plain HttpWebRequest can never connect to GitHub
    /// (which requires TLS 1.2+) — that is why older versions of the mod never
    /// managed to show the "new version available" prompt. A background
    /// HttpWebRequest remains as a fallback for runtimes where it does work.
    /// </summary>
    internal static class UpdateChecker
    {
        private const string RepoUrl = "https://github.com/anonymusdennis/E2E-Mapeditor-V2.0";
        private const string ApiUrl =
            "https://api.github.com/repos/anonymusdennis/E2E-Mapeditor-V2.0/releases/latest";
        private const float WwwTimeoutSeconds = 15f;

        /// <summary>Start the update check; returns immediately.</summary>
        internal static void CheckAsync()
        {
            try
            {
                Plugin.Instance.StartCoroutine(CheckRoutine());
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Update check via WWW could not start: " + e.Message);
                StartFallbackThread();
            }
        }

        // ── primary: Unity WWW on the main thread ──────────────────────────────

        private static IEnumerator CheckRoutine()
        {
            WWW www = CreateRequest();
            if (www == null)
            {
                StartFallbackThread();
                yield break;
            }
            float deadline = Time.realtimeSinceStartup + WwwTimeoutSeconds;
            while (!www.isDone && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            string json = null;
            string error;
            try
            {
                error = www.isDone ? www.error : "timed out";
                if (string.IsNullOrEmpty(error))
                {
                    json = www.text;
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            finally
            {
                www.Dispose();
            }
            if (json != null)
            {
                ProcessResponse(json);
            }
            else
            {
                Plugin.Log.LogInfo("Update check via WWW failed (" + error + "), trying fallback…");
                StartFallbackThread();
            }
        }

        private static WWW CreateRequest()
        {
            try
            {
                // GitHub's API rejects requests without a User-Agent header
                var headers = new Dictionary<string, string>
                {
                    { "User-Agent", "MapEditorMod/" + PluginInfo.Version },
                    { "Accept", "application/vnd.github.v3+json" },
                };
                return new WWW(ApiUrl, null, headers);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Update check via WWW failed to start: " + e.Message);
                return null;
            }
        }

        // ── fallback: HttpWebRequest on a background thread ────────────────────

        private static void StartFallbackThread()
        {
            var t = new Thread(CheckOnThread) { IsBackground = true, Name = "UpdateChecker" };
            t.Start();
        }

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
                try
                {
                    // request TLS 1.2 where the runtime knows it; old Mono
                    // throws here, in which case we try with its defaults
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                }
                catch (Exception)
                {
                }

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
                E2EApi.MainThread.Post(() => ProcessResponse(json));
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Update check failed: " + e.Message);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = prevCallback;
                try
                {
                    ServicePointManager.SecurityProtocol = prevProtocol;
                }
                catch (Exception)
                {
                }
            }
        }

        // ── shared response handling (main thread) ─────────────────────────────

        private static void ProcessResponse(string json)
        {
            string tagName = Extract(json, "tag_name");
            string htmlUrl = Extract(json, "html_url");
            if (tagName == null)
            {
                Plugin.Log.LogWarning("Update check: no tag_name in the release info.");
                return;
            }
            OnVersionReceived(tagName, htmlUrl ?? (RepoUrl + "/releases/latest"));
        }

        /// <summary>Extract the first string value of a JSON key (no full parser needed).</summary>
        private static string Extract(string json, string key)
        {
            var m = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\"([^\"]+)\"");
            return m.Success ? m.Groups[1].Value : null;
        }

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
