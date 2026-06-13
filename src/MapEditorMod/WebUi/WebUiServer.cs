using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using E2EApi;
using E2EApi.Editor;
using E2EApi.Features;
using E2EApi.Persistence;

namespace MapEditorMod.WebUi
{
    /// <summary>
    /// Minimal HTTP server (TcpListener, net35-safe) exposing the map editor
    /// UI as a real browser window — draggable to any desktop or monitor,
    /// unlike an in-game overlay. Game access is marshalled to the main
    /// thread via the E2EApi.
    /// </summary>
    internal class WebUiServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private Thread _thread;
        private volatile bool _running;

        // two-step trigger linking state (mirrors the F7 flow)
        private int _pendingX = -1;
        private int _pendingY = -1;

        public WebUiServer(int port)
        {
            _port = port;
        }

        public string Url => $"http://127.0.0.1:{_port}/";

        private static string PrefsPath =>
            Path.Combine(BepInEx.Paths.ConfigPath, "e2e_webui_prefs.json");

        public void Start()
        {
            if (_running)
            {
                return;
            }
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
            _running = true;
            _thread = new Thread(AcceptLoop) { IsBackground = true, Name = "E2E-WebUI" };
            _thread.Start();
            Plugin.Log.LogInfo($"web UI listening on {Url}");
        }

        public void Stop()
        {
            _running = false;
            try { _listener.Stop(); } catch { }
        }

        private void AcceptLoop()
        {
            while (_running)
            {
                TcpClient client;
                try
                {
                    client = _listener.AcceptTcpClient();
                }
                catch
                {
                    if (!_running)
                    {
                        return;
                    }
                    continue;
                }
                ThreadPool.QueueUserWorkItem(_ => Handle(client));
            }
        }

        private void Handle(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    stream.ReadTimeout = 5000;
                    string requestLine = ReadLine(stream);
                    if (string.IsNullOrEmpty(requestLine))
                    {
                        return;
                    }
                    // drain headers, remembering the body length
                    int contentLength = 0;
                    string header;
                    while (!string.IsNullOrEmpty(header = ReadLine(stream)))
                    {
                        if (header.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(header.Substring("Content-Length:".Length).Trim(),
                                out contentLength);
                        }
                    }
                    string requestBody = "";
                    if (contentLength > 0 && contentLength <= 1 << 20)
                    {
                        var buf = new byte[contentLength];
                        int read = 0;
                        while (read < contentLength)
                        {
                            int n = stream.Read(buf, read, contentLength - read);
                            if (n <= 0)
                            {
                                break;
                            }
                            read += n;
                        }
                        requestBody = Encoding.UTF8.GetString(buf, 0, read);
                    }

                    string[] parts = requestLine.Split(' ');
                    if (parts.Length < 2)
                    {
                        return;
                    }
                    string method = parts[0];
                    string rawPath = parts[1];
                    string path = rawPath;
                    string query = "";
                    int qIndex = rawPath.IndexOf('?');
                    if (qIndex >= 0)
                    {
                        path = rawPath.Substring(0, qIndex);
                        query = rawPath.Substring(qIndex + 1);
                    }

                    byte[] body;
                    string contentType;
                    int status = Route(method, path, ParseQuery(query), requestBody,
                        out body, out contentType);
                    WriteResponse(stream, status, contentType, body);
                }
            }
            catch (Exception e)
            {
                if (_running)
                {
                    Plugin.Log.LogWarning("web UI request failed: " + e.Message);
                }
            }
        }

        private int Route(string method, string path, Dictionary<string, string> query,
            string requestBody, out byte[] body, out string contentType)
        {
            contentType = "application/json; charset=utf-8";
            try
            {
                if (path == "/" || path == "/index.html")
                {
                    contentType = "text/html; charset=utf-8";
                    body = Encoding.UTF8.GetBytes(WebUiPage.Html);
                    return 200;
                }
                if (path == "/api/prefs")
                {
                    // UI preferences (custom filters, tile arrangement) — an
                    // opaque JSON blob persisted next to the BepInEx config so
                    // it survives restarts and works from any browser
                    if (method == "POST")
                    {
                        if (requestBody.Length > 512 * 1024)
                        {
                            body = Encoding.UTF8.GetBytes("{\"ok\":false,\"msg\":\"too large\"}");
                            return 400;
                        }
                        File.WriteAllText(PrefsPath, requestBody);
                        body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                        return 200;
                    }
                    body = Encoding.UTF8.GetBytes(
                        File.Exists(PrefsPath) ? File.ReadAllText(PrefsPath) : "{}");
                    return 200;
                }
                if (path == "/api/state")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(GetStateJson));
                    return 200;
                }
                if (path == "/api/blocks")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(GetBlocksJson, 15000));
                    return 200;
                }
                if (method == "POST" && path == "/api/dev/skip-title")
                {
                    string result = MainThread.Run(EditorSession.SkipTitle);
                    body = Encoding.UTF8.GetBytes("{\"result\":" + Quote(result) + "}");
                    return 200;
                }
                if (method == "POST" && path == "/api/dev/enter-editor")
                {
                    string file = Get(query, "file");
                    bool ok = MainThread.Run(() => EditorSession.Enter(file));
                    body = Encoding.UTF8.GetBytes("{\"ok\":" + (ok ? "true" : "false") + "}");
                    return 200;
                }
                if (method == "POST" && path == "/api/dev/save")
                {
                    string result = MainThread.Run(EditorSession.Save, 15000);
                    body = Encoding.UTF8.GetBytes("{\"msg\":" + Quote(result) + "}");
                    return 200;
                }
                if (method == "POST" && path == "/api/dev/playtest")
                {
                    bool ok = MainThread.Run(EditorSession.PlayTest);
                    body = Encoding.UTF8.GetBytes(ok
                        ? "{\"ok\":true,\"msg\":\"starting play test…\"}"
                        : "{\"ok\":false,\"msg\":\"not in the editor\"}");
                    return 200;
                }
                if (path == "/api/icon-report")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(BlockIcons.DiagnoseJson, 15000));
                    return 200;
                }
                if (path.StartsWith("/api/icon/"))
                {
                    int id;
                    if (!int.TryParse(path.Substring("/api/icon/".Length).Replace(".png", ""), out id))
                    {
                        body = Encoding.UTF8.GetBytes("{\"error\":\"bad id\"}");
                        return 400;
                    }
                    byte[] png = MainThread.Run(() => BlockIcons.GetPng(id), 10000);
                    if (png == null)
                    {
                        body = new byte[0];
                        return 404;
                    }
                    contentType = "image/png";
                    body = png;
                    return 200;
                }
                if (method == "POST" && path.StartsWith("/api/brush/"))
                {
                    int id;
                    if (!int.TryParse(path.Substring("/api/brush/".Length), out id))
                    {
                        body = Encoding.UTF8.GetBytes("{\"error\":\"bad id\"}");
                        return 400;
                    }
                    bool ok = MainThread.Run(() => Blocks.SelectBrush(id));
                    body = Encoding.UTF8.GetBytes("{\"ok\":" + (ok ? "true" : "false") + "}");
                    return 200;
                }
                if (method == "POST" && path == "/api/fence/cursor")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(ToggleFenceAtCursor));
                    return 200;
                }
                if (method == "POST" && path == "/api/fence/set")
                {
                    int fx = GetInt(query, "x", -1);
                    int fy = GetInt(query, "y", -1);
                    bool on = GetBool(query, "value");
                    if (fx < 0 || fy < 0)
                    {
                        body = Encoding.UTF8.GetBytes("{\"ok\":false,\"msg\":\"bad tile\"}");
                        return 400;
                    }
                    MainThread.Run(() => ElectricFences.SetElectrified(fx, fy, on));
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                if (path == "/api/tilesets")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(TileSets.StatusJson, 10000));
                    return 200;
                }
                if (method == "POST" && path == "/api/tilesets/harvest")
                {
                    string set = Get(query, "set");
                    if (set.Length == 0)
                    {
                        set = "all";
                    }
                    TileSets.DumpRawTextures = GetBool(query, "raw");
                    string result = MainThread.Run(() => TileSets.StartHarvest(set));
                    body = Encoding.UTF8.GetBytes("{\"msg\":" + Quote(result) + "}");
                    return 200;
                }
                if (path == "/api/tilesets/atlases")
                {
                    // pure file I/O against the cache — no main thread needed
                    body = Encoding.UTF8.GetBytes(TileSets.SetAtlasesJson(Get(query, "set")));
                    return 200;
                }
                if (path == "/api/tilesets/atlas.png")
                {
                    string file = TileSets.GetAtlasPngPath(Get(query, "name"));
                    if (file == null)
                    {
                        body = Encoding.UTF8.GetBytes("{\"error\":\"atlas not cached\"}");
                        return 404;
                    }
                    contentType = "image/png";
                    body = File.ReadAllBytes(file);
                    return 200;
                }
                if (method == "POST" && path == "/api/tilesets/stamp")
                {
                    string atlas = Get(query, "atlas");
                    int sx = GetInt(query, "x", -1);
                    int sy = GetInt(query, "y", -1);
                    int sw = GetInt(query, "w", 0);
                    int sh = GetInt(query, "h", 0);
                    bool decor = GetBool(query, "decor");
                    if (atlas.Length == 0 || sx < 0 || sy < 0 || sw <= 0 || sh <= 0)
                    {
                        body = Encoding.UTF8.GetBytes("{\"ok\":false,\"msg\":\"bad stamp\"}");
                        return 400;
                    }
                    body = Encoding.UTF8.GetBytes(MainThread.Run(() =>
                        ArmStamp(atlas, sx, sy, sw, sh, decor)));
                    return 200;
                }
                if (method == "POST" && path == "/api/composite/arm")
                {
                    body = Encoding.UTF8.GetBytes(
                        MainThread.Run(() => ArmCompositeStamp(query, requestBody)));
                    return 200;
                }
                if (method == "POST" && path == "/api/connected/arm")
                {
                    body = Encoding.UTF8.GetBytes(
                        MainThread.Run(() => ArmConnectedStamp(query, requestBody)));
                    return 200;
                }
                if (path == "/api/composite/import")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(() =>
                        ImportCompositeRegion(query)));
                    return 200;
                }
                if (method == "POST" && path == "/api/tiles/place")
                {
                    string atlas = Get(query, "atlas");
                    int px = GetInt(query, "x", -1);
                    int py = GetInt(query, "y", -1);
                    int layer = GetInt(query, "layer", 1);
                    int rx = GetInt(query, "rx", -1);
                    int ry = GetInt(query, "ry", -1);
                    int rw = GetInt(query, "rw", 0);
                    int rh = GetInt(query, "rh", 0);
                    bool decor2 = GetBool(query, "decor");
                    if (atlas.Length == 0 || px < 0 || py < 0 || rx < 0 || ry < 0 ||
                        rw <= 0 || rh <= 0)
                    {
                        body = Encoding.UTF8.GetBytes("{\"ok\":false,\"msg\":\"bad placement\"}");
                        return 400;
                    }
                    MainThread.Run(() =>
                        ModTiles.Place(px, py, layer, decor2, atlas, rx, ry, rw, rh));
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                if (method == "POST" && path == "/api/tiles/erase")
                {
                    int ex = GetInt(query, "x", -1);
                    int ey = GetInt(query, "y", -1);
                    int elayer = GetInt(query, "layer", 1);
                    int removed = MainThread.Run(() => ModTiles.EraseAt(ex, ey, elayer));
                    body = Encoding.UTF8.GetBytes("{\"ok\":true,\"removed\":" + removed + "}");
                    return 200;
                }
                if (method == "POST" && path == "/api/tiles/clear")
                {
                    MainThread.Run(() =>
                    {
                        ModTiles.Clear();
                        AnimatedModTiles.Clear();
                    });
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                // ---- animated tile routes ----
                if (path == "/api/animated-tiles")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(GetAnimatedTilesJson));
                    return 200;
                }
                if (method == "POST" && path == "/api/animated-tiles/place")
                {
                    body = Encoding.UTF8.GetBytes(
                        MainThread.Run(() => PlaceAnimatedTile(query, requestBody)));
                    return 200;
                }
                if (method == "POST" && path == "/api/animated-tiles/erase")
                {
                    int ax = GetInt(query, "x", -1);
                    int ay = GetInt(query, "y", -1);
                    int alayer = GetInt(query, "layer", 1);
                    int aRemoved = MainThread.Run(() => AnimatedModTiles.EraseAt(ax, ay, alayer));
                    body = Encoding.UTF8.GetBytes(
                        "{\"ok\":true,\"removed\":" + aRemoved + "}");
                    return 200;
                }
                if (method == "POST" && path == "/api/animated-tiles/clear")
                {
                    MainThread.Run(() => AnimatedModTiles.Clear());
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                if (method == "POST" && path == "/api/tilesets/arm-animated")
                {
                    body = Encoding.UTF8.GetBytes(
                        MainThread.Run(() => ArmAnimatedStamp(query, requestBody)));
                    return 200;
                }
                if (path == "/api/tilesets/atlas-frame-check")
                {
                    // Pure file I/O: checks whether an atlas region has any
                    // non-transparent pixels (used by auto-detect as server fallback).
                    string afcAtlas = Get(query, "name");
                    int afcX = GetInt(query, "x", 0);
                    int afcY = GetInt(query, "y", 0);
                    int afcW = GetInt(query, "w", 32);
                    int afcH = GetInt(query, "h", 32);
                    bool hasContent = MainThread.Run(
                        () => TileSets.RegionHasContent(afcAtlas, afcX, afcY, afcW, afcH));
                    body = Encoding.UTF8.GetBytes(
                        "{\"hasContent\":" + (hasContent ? "true" : "false") + "}");
                    return 200;
                }
                if (path == "/api/dlc")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(DlcContent.StatusJson));
                    return 200;
                }
                if (path == "/api/items")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(DlcContent.ItemsJson, 10000));
                    return 200;
                }
                if (path == "/api/debug")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(GetDebugJson));
                    return 200;
                }
                if (path == "/api/debug/load")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(E2EApi.Diagnostics.LoadStateJson));
                    return 200;
                }
                if (method == "POST" && path == "/api/trigger/cursor")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(TriggerLinkStep));
                    return 200;
                }
                if (method == "POST" && path == "/api/tool")
                {
                    string name = Get(query, "name");
                    body = Encoding.UTF8.GetBytes(MainThread.Run(() => SetTool(name)));
                    return 200;
                }
                if (method == "POST" && path == "/api/requiresmod")
                {
                    bool value = GetBool(query, "value");
                    MainThread.Run(() => { ModExtras.Current.RequiresMod = value; });
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                if (method == "POST" && path == "/api/setting")
                {
                    string name = Get(query, "name");
                    bool value = GetBool(query, "value");
                    MainThread.Run(() => SetSetting(name, value));
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                if (method == "POST" && path == "/api/clear-extras")
                {
                    MainThread.Run(() =>
                    {
                        ElectricFences.Clear();
                        Triggers.Clear();
                        ModTiles.Clear();
                        AnimatedModTiles.Clear();
                    });
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                if (method == "POST" && path == "/api/numsetting")
                {
                    string name = Get(query, "name");
                    float value;
                    if (!float.TryParse(Get(query, "value"),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out value))
                    {
                        body = Encoding.UTF8.GetBytes("{\"ok\":false,\"msg\":\"bad value\"}");
                        return 400;
                    }
                    MainThread.Run(() => SetNumSetting(name, value));
                    body = Encoding.UTF8.GetBytes("{\"ok\":true}");
                    return 200;
                }
                if (path == "/api/floors")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(GetFloorsJson));
                    return 200;
                }
                if (path.StartsWith("/api/map/"))
                {
                    int floor;
                    if (!int.TryParse(path.Substring("/api/map/".Length).Replace(".png", ""), out floor))
                    {
                        body = Encoding.UTF8.GetBytes("{\"error\":\"bad floor\"}");
                        return 400;
                    }
                    byte[] png = MainThread.Run(() => GameMap.GetFloorPng(floor), 10000);
                    if (png == null)
                    {
                        body = Encoding.UTF8.GetBytes("{\"error\":\"no map texture\"}");
                        return 404;
                    }
                    contentType = "image/png";
                    body = png;
                    return 200;
                }
                if (method == "POST" && path == "/api/teleport")
                {
                    int x = GetInt(query, "x", -1);
                    int y = GetInt(query, "y", -1);
                    int floor = GetInt(query, "floor", -1);
                    body = Encoding.UTF8.GetBytes(MainThread.Run(() => DoTeleport(x, y, floor)));
                    return 200;
                }
                if (path == "/api/player")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(GetPlayerJson));
                    return 200;
                }
                if (method == "POST" && path == "/api/cheat")
                {
                    string name = Get(query, "name");
                    string value = Get(query, "value");
                    body = Encoding.UTF8.GetBytes(MainThread.Run(() => RunCheat(name, value)));
                    return 200;
                }
                if (path == "/api/geometry")
                {
                    body = Encoding.UTF8.GetBytes(MainThread.Run(() => MapGeometry.ToJson()));
                    return 200;
                }
                if (method == "POST" && path.StartsWith("/api/geometry/"))
                {
                    string action = path.Substring("/api/geometry/".Length);
                    body = Encoding.UTF8.GetBytes(MainThread.Run(() => HandleGeometry(action, query)));
                    return 200;
                }
                body = Encoding.UTF8.GetBytes("{\"error\":\"not found\"}");
                return 404;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("web UI route failed: " + e);
                body = Encoding.UTF8.GetBytes("{\"error\":" + Quote(e.Message) + "}");
                return 500;
            }
        }

        // ---- handlers (run on main thread) ----

        private static string GetStateJson()
        {
            int cx, cy;
            bool hasCursor = Placement.GetCursorTile(out cx, out cy);
            var sb = new StringBuilder();
            sb.Append("{\"inEditor\":").Append(E2EApi.Events.GameEvents.IsInEditor ? "true" : "false");
            sb.Append(",\"playtesting\":").Append(E2EApi.Events.GameEvents.IsPlaytesting ? "true" : "false");
            sb.Append(",\"loadingOverlay\":").Append(LoadingOverlay.Visible ? "true" : "false");
            sb.Append(",\"cursor\":");
            if (hasCursor)
            {
                sb.Append("{\"x\":").Append(cx).Append(",\"y\":").Append(cy).Append("}");
            }
            else
            {
                sb.Append("null");
            }
            sb.Append(",\"fences\":").Append(ElectricFences.Count);
            sb.Append(",\"triggers\":").Append(Triggers.All.Count);
            sb.Append(",\"modTiles\":").Append(ModTiles.Count);
            sb.Append(",\"animatedTiles\":").Append(AnimatedModTiles.Count);
            sb.Append(",\"editorLayer\":").Append(Grid.CurrentEditorLayer);
            sb.Append(",\"nativeEditorLayer\":").Append(Grid.CurrentNativeEditorLayer);
            sb.Append(",\"geometryFeatureVersion\":").Append(ModExtras.Current.GeometryFeatureVersion);
            sb.Append(",\"geometryHash\":").Append(Quote(ModExtras.Current.GeometryHash ?? ""));
            sb.Append(",\"mapGeometry\":").Append(MapGeometry.ToJson());
            sb.Append(",\"brush\":").Append(Blocks.CurrentBrush);
            sb.Append(",\"stamp\":");
            var stamp = EditorTools.Stamp;
            if (stamp != null)
            {
                sb.Append("{\"atlas\":").Append(Quote(stamp.Atlas))
                  .Append(",\"x\":").Append(stamp.X).Append(",\"y\":").Append(stamp.Y)
                  .Append(",\"w\":").Append(stamp.W).Append(",\"h\":").Append(stamp.H)
                  .Append(",\"decor\":").Append(stamp.Decor ? "true" : "false").Append("}");
            }
            else
            {
                sb.Append("null");
            }
            var missingAtlases = ModTiles.MissingAtlases();
            sb.Append(",\"missingAtlases\":[");
            for (int i = 0; i < missingAtlases.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(Quote(missingAtlases[i]));
            }
            sb.Append("]");
            sb.Append(",\"tool\":").Append(Quote(ToolName(EditorTools.Mode)));
            sb.Append(",\"toolHint\":").Append(Quote(EditorTools.HintText()));
            sb.Append(",\"pendingSwitch\":");
            if (EditorTools.SwitchX >= 0)
            {
                sb.Append("{\"x\":").Append(EditorTools.SwitchX)
                  .Append(",\"y\":").Append(EditorTools.SwitchY).Append("}");
            }
            else
            {
                sb.Append("null");
            }
            sb.Append(",\"requiresMod\":").Append(ModExtras.Current.RequiresMod ? "true" : "false");
            sb.Append(",\"settings\":{");
            sb.Append("\"devBlocks\":").Append(Plugin.CfgUnlockDevBlocks.Value ? "true" : "false");
            sb.Append(",\"dlcBlocks\":").Append(Plugin.CfgUnlockDlcBlocks.Value ? "true" : "false");
            sb.Append(",\"layers\":").Append(Plugin.CfgIgnoreLayerRestrictions.Value ? "true" : "false");
            sb.Append(",\"completion\":").Append(Plugin.CfgIgnoreCompletionState.Value ? "true" : "false");
            sb.Append(",\"restrictions\":").Append(Restrictions.IgnoreAll ? "true" : "false");
            sb.Append(",\"xray\":").Append(XRay.Enabled ? "true" : "false");
            sb.Append(",\"fenceOverlay\":").Append(FenceOverlay.Enabled ? "true" : "false");
            sb.Append(",\"arrows\":").Append(TriggerArrows.Enabled ? "true" : "false");
            sb.Append(",\"cameraLock\":").Append(EditorCamera.LockPan ? "true" : "false");
            sb.Append(",\"forceWindowed\":").Append(Plugin.CfgForceWindowed.Value ? "true" : "false");
            sb.Append(",\"autoOpen\":").Append(Plugin.CfgWebUiAutoOpen.Value ? "true" : "false");
            sb.Append(",\"vanillaFallback\":").Append(Plugin.CfgVanillaFallback.Value ? "true" : "false");
            sb.Append(",\"infiniteEnergy\":").Append(E2EApi.Players.Player.InfiniteEnergy ? "true" : "false");
            sb.Append("},\"numbers\":{");
            sb.Append("\"guardCap\":").Append(Plugin.CfgGuardInmateCap.Value);
            sb.Append(",\"zoomSteps\":").Append(Plugin.CfgExtraZoomSteps.Value);
            sb.Append(",\"fenceDamage\":").Append(ElectricFences.DamagePerTick.ToString("0.#",
                System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(",\"windowWidth\":").Append(Plugin.CfgWindowedWidth.Value);
            sb.Append(",\"windowHeight\":").Append(Plugin.CfgWindowedHeight.Value);
            sb.Append("}}");
            return sb.ToString();
        }

        private static string GetBlocksJson()
        {
            var blocks = Blocks.GetSpawnList();
            var sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var block in blocks)
            {
                if (!first)
                {
                    sb.Append(",");
                }
                first = false;
                sb.Append("{\"id\":").Append(block.Id);
                sb.Append(",\"name\":").Append(Quote(block.DisplayName ?? ""));
                sb.Append(",\"kind\":").Append(Quote(block.Kind.ToString()));
                sb.Append(",\"editorOnly\":").Append(block.EditorOnly ? "true" : "false");
                sb.Append(",\"hasIcon\":").Append(block.Icon != null ? "true" : "false");
                sb.Append(",\"internalName\":").Append(Quote(block.InternalName ?? ""));
                sb.Append(",\"className\":").Append(Quote(block.ClassName ?? ""));
                sb.Append(",\"prefab\":").Append(Quote(block.PrefabName ?? ""));
                sb.Append(",\"layers\":").Append(Quote(block.ValidLayers ?? ""));
                sb.Append(",\"themes\":").Append(Quote(block.Themes ?? ""));
                sb.Append(",\"purpose\":").Append(Quote(block.Purpose ?? ""));
                sb.Append(",\"zone\":").Append(block.IsZone ? "true" : "false");
                sb.Append(",\"limitGroup\":").Append(Quote(block.LimitGroup ?? ""));
                string desc = block.Description != null && !block.Description.StartsWith("Text.")
                    ? block.Description : "";
                sb.Append(",\"desc\":").Append(Quote(desc));
                sb.Append(",\"notes\":").Append(Quote(block.Notes ?? ""));
                sb.Append("}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string ToggleFenceAtCursor()
        {
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return "{\"ok\":false,\"msg\":\"cursor not on the map\"}";
            }
            bool on = ElectricFences.Toggle(x, y);
            return "{\"ok\":true,\"msg\":\"fence at (" + x + "," + y + ") " + (on ? "ON" : "off") + "\"}";
        }

        private string TriggerLinkStep()
        {
            int x, y;
            if (!Placement.GetCursorTile(out x, out y))
            {
                return "{\"ok\":false,\"msg\":\"cursor not on the map\"}";
            }
            if (_pendingX < 0)
            {
                _pendingX = x;
                _pendingY = y;
                return "{\"ok\":true,\"msg\":\"button set at (" + x + "," + y + ") — aim at the target and click again\"}";
            }
            Triggers.AddLink(_pendingX, _pendingY, x, y);
            string msg = "linked (" + _pendingX + "," + _pendingY + ") → (" + x + "," + y + ")";
            _pendingX = _pendingY = -1;
            return "{\"ok\":true,\"msg\":\"" + msg + "\"}";
        }

        private static string ToolName(EditorToolMode mode)
        {
            switch (mode)
            {
                case EditorToolMode.PaintElectric: return "paint";
                case EditorToolMode.EraseElectric: return "erase";
                case EditorToolMode.LinkSwitch: return "link";
                case EditorToolMode.PaintTile: return "tilepaint";
                case EditorToolMode.EraseTile: return "tileerase";
                case EditorToolMode.PaintAnimatedTile: return "animtilepaint";
                case EditorToolMode.EraseAnimatedTile: return "animtileerase";
                case EditorToolMode.PaintConnectedTile: return "connectedpaint";
                default: return "none";
            }
        }

        private static string ArmStamp(string atlas, int x, int y, int w, int h, bool decor)
        {
            if (!TileSets.HasAtlas(atlas))
            {
                return "{\"ok\":false,\"msg\":\"atlas not cached: " + atlas + "\"}";
            }
            EditorTools.Stamp = new TileStamp
            {
                Atlas = atlas,
                X = x,
                Y = y,
                W = w,
                H = h,
                Decor = decor,
            };
            EditorTools.SetMode(EditorToolMode.PaintTile);
            return "{\"ok\":true,\"msg\":" + Quote("stamp armed — " + EditorTools.HintText()) + "}";
        }

        private static string ArmCompositeStamp(
            Dictionary<string, string> query, string body)
        {
            string name = Get(query, "name");
            int wTiles = GetInt(query, "w", 1);
            int hTiles = GetInt(query, "h", 1);
            var parts = ParseCompositeParts(body);
            if (parts.Count == 0)
            {
                return "{\"ok\":false,\"msg\":\"composite has no parts\"}";
            }
            foreach (var part in parts)
            {
                if (part.Vanilla)
                {
                    if (!Blocks.Exists(part.BlockId))
                    {
                        return "{\"ok\":false,\"msg\":\"block not found: " + part.BlockId + "\"}";
                    }
                }
                else if (part.Animated)
                {
                    if (part.Frames == null || part.Frames.Count == 0)
                    {
                        return "{\"ok\":false,\"msg\":\"animated part has no frames\"}";
                    }
                    if (!TileSets.HasAtlas(part.Frames[0].Atlas))
                    {
                        return "{\"ok\":false,\"msg\":\"atlas not cached: " +
                            part.Frames[0].Atlas + "\"}";
                    }
                }
                else if (!TileSets.HasAtlas(part.Atlas))
                {
                    return "{\"ok\":false,\"msg\":\"atlas not cached: " + part.Atlas + "\"}";
                }
            }
            EditorTools.Stamp = null;
            EditorTools.AnimatedStamp = null;
            EditorTools.Composite = new CompositeStamp
            {
                Name = string.IsNullOrEmpty(name) ? "combined block" : name,
                WTiles = System.Math.Max(1, wTiles),
                HTiles = System.Math.Max(1, hTiles),
                Parts = parts,
            };
            EditorTools.SetMode(EditorToolMode.PaintTile);
            return "{\"ok\":true,\"msg\":" +
                Quote("combined block armed — " + EditorTools.HintText()) + "}";
        }

        private static string ArmConnectedStamp(
            Dictionary<string, string> query, string body)
        {
            var stamp = new ConnectedTileStamp
            {
                Id = Get(query, "id"),
                Name = Get(query, "name"),
                Mode = Get(query, "mode"),
                Decor = GetBool(query, "decor"),
                Color = GetInt(query, "color", 1),
                Auto = !GetBool(query, "manual"),
                LockPlaced = GetBool(query, "lock"),
                CollideWithPlayers = GetBool(query, "collide"),
                Destructible = GetBool(query, "destructible"),
                Damaging = GetBool(query, "damaging"),
            };
            if (string.IsNullOrEmpty(stamp.Name)) stamp.Name = "connected tile";
            if (string.IsNullOrEmpty(stamp.Mode)) stamp.Mode = "floor";
            foreach (var line in body.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;
                string[] parts = trimmed.Split('|');
                if (parts.Length < 2) continue;
                int mask;
                if (!int.TryParse(parts[0], out mask)) continue;
                var variants = new List<ConnectedVariant>();
                foreach (var raw in parts[1].Split(';'))
                {
                    string[] v = raw.Split(':');
                    if (v.Length < 6) continue;
                    int rx, ry, rw, rh, weight;
                    if (!int.TryParse(v[v.Length - 5], out rx) ||
                        !int.TryParse(v[v.Length - 4], out ry) ||
                        !int.TryParse(v[v.Length - 3], out rw) ||
                        !int.TryParse(v[v.Length - 2], out rh) ||
                        !int.TryParse(v[v.Length - 1], out weight))
                    {
                        continue;
                    }
                    string atlas = string.Join(":", v, 0, v.Length - 5);
                    if (!TileSets.HasAtlas(atlas))
                    {
                        return "{\"ok\":false,\"msg\":\"atlas not cached: " + atlas + "\"}";
                    }
                    variants.Add(new ConnectedVariant
                    {
                        Atlas = atlas, X = rx, Y = ry, W = rw, H = rh,
                        Weight = weight,
                    });
                }
                if (variants.Count > 0) stamp.Variants[mask] = variants;
            }
            if (stamp.Variants.Count == 0)
            {
                return "{\"ok\":false,\"msg\":\"connected texture has no configured variants\"}";
            }
            EditorTools.ArmConnectedStamp(stamp);
            return "{\"ok\":true,\"msg\":" +
                Quote("connected brush armed — " + EditorTools.HintText()) + "}";
        }

        private static List<CompositeStampPart> ParseCompositeParts(string body)
        {
            var result = new List<CompositeStampPart>();
            if (string.IsNullOrEmpty(body))
            {
                return result;
            }
            foreach (var line in body.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }
                string[] fields = trimmed.Split('|');
                if (fields.Length < 2)
                {
                    continue;
                }
                string[] h = fields[0].Split(',');
                if (h.Length < 6)
                {
                    continue;
                }
                int dx, dy, layer;
                if (!int.TryParse(h[0], out dx) ||
                    !int.TryParse(h[1], out dy) ||
                    !int.TryParse(h[2], out layer))
                {
                    continue;
                }
                bool decor = h[3] == "d";
                bool animated = h[4] == "a";
                bool vanilla = h[4] == "v";
                var part = new CompositeStampPart
                {
                    Dx = dx,
                    Dy = dy,
                    Layer = layer,
                    Decor = decor,
                    Animated = animated,
                    Vanilla = vanilla,
                };
                if (vanilla)
                {
                    int blockId;
                    if (!int.TryParse(fields[1], out blockId))
                    {
                        continue;
                    }
                    part.BlockId = blockId;
                }
                else if (animated)
                {
                    float fps;
                    part.Fps = float.TryParse(h.Length > 5 ? h[5] : "4",
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out fps)
                        ? fps : 4f;
                    part.Loop = h.Length <= 6 || h[6] == "1";
                    part.PingPong = h.Length > 7 && h[7] == "p";
                    part.Frames = ParseFramesParam(fields[1]);
                }
                else
                {
                    string[] p = fields[1].Split(':');
                    if (p.Length < 5)
                    {
                        continue;
                    }
                    int rx, ry, rw, rh;
                    if (!int.TryParse(p[p.Length - 4], out rx) ||
                        !int.TryParse(p[p.Length - 3], out ry) ||
                        !int.TryParse(p[p.Length - 2], out rw) ||
                        !int.TryParse(p[p.Length - 1], out rh))
                    {
                        continue;
                    }
                    part.Atlas = string.Join(":", p, 0, p.Length - 4);
                    part.X = rx;
                    part.Y = ry;
                    part.W = rw;
                    part.H = rh;
                }
                result.Add(part);
            }
            return result;
        }

        private static string ImportCompositeRegion(Dictionary<string, string> query)
        {
            int x = GetInt(query, "x", -1);
            int y = GetInt(query, "y", -1);
            int w = GetInt(query, "w", 0);
            int h = GetInt(query, "h", 0);
            if (x < 0 || y < 0 || w <= 0 || h <= 0)
            {
                return "{\"ok\":false,\"msg\":\"bad region\"}";
            }
            var seenStatic = new HashSet<string>();
            var seenAnim = new HashSet<string>();
            var sb = new StringBuilder();
            sb.Append("{\"ok\":true,\"parts\":[");
            bool first = true;
            for (int layer = 0; layer < Grid.LayerCount; layer++)
            {
                foreach (var p in ModTiles.All())
                {
                    if (p.Layer != layer || p.X < x || p.Y < y ||
                        p.X >= x + w || p.Y >= y + h)
                    {
                        continue;
                    }
                    string key = p.X + "," + p.Y + "," + p.Layer + "," +
                        p.Decor + "," + p.Atlas;
                    if (!seenStatic.Add(key))
                    {
                        continue;
                    }
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append("{\"kind\":\"static\",\"dx\":").Append(p.X - x)
                      .Append(",\"dy\":").Append(p.Y - y)
                      .Append(",\"layer\":").Append(p.Layer)
                      .Append(",\"decor\":").Append(p.Decor ? "true" : "false")
                      .Append(",\"atlas\":").Append(Quote(p.Atlas))
                      .Append(",\"x\":").Append(p.Rx)
                      .Append(",\"y\":").Append(p.Ry)
                      .Append(",\"w\":").Append(p.Rw)
                      .Append(",\"h\":").Append(p.Rh)
                      .Append("}");
                }
                foreach (var p in AnimatedModTiles.All())
                {
                    if (p.Layer != layer || p.X < x || p.Y < y ||
                        p.X >= x + w || p.Y >= y + h)
                    {
                        continue;
                    }
                    string key = p.X + "," + p.Y + "," + p.Layer + "," +
                        p.Decor + "," + p.Frames.Count;
                    if (!seenAnim.Add(key))
                    {
                        continue;
                    }
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append("{\"kind\":\"animated\",\"dx\":").Append(p.X - x)
                      .Append(",\"dy\":").Append(p.Y - y)
                      .Append(",\"layer\":").Append(p.Layer)
                      .Append(",\"decor\":").Append(p.Decor ? "true" : "false")
                      .Append(",\"fps\":").Append(p.Fps.ToString("0.##",
                          System.Globalization.CultureInfo.InvariantCulture))
                      .Append(",\"loop\":").Append(p.Loop ? "true" : "false")
                      .Append(",\"pingpong\":").Append(p.PingPong ? "true" : "false")
                      .Append(",\"frames\":[");
                    for (int i = 0; i < p.Frames.Count; i++)
                    {
                        if (i > 0) sb.Append(",");
                        var f = p.Frames[i];
                        sb.Append("{\"atlas\":").Append(Quote(f.Atlas))
                          .Append(",\"x\":").Append(f.Rx)
                          .Append(",\"y\":").Append(f.Ry)
                          .Append(",\"w\":").Append(f.Rw)
                          .Append(",\"h\":").Append(f.Rh)
                          .Append("}");
                    }
                    sb.Append("]}");
                }
            }
            sb.Append("],\"w\":").Append(w).Append(",\"h\":").Append(h).Append("}");
            return sb.ToString();
        }

        // ---- animated tile handlers ----

        private static string GetAnimatedTilesJson()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var p in AnimatedModTiles.All())
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("{\"x\":").Append(p.X)
                  .Append(",\"y\":").Append(p.Y)
                  .Append(",\"layer\":").Append(p.Layer)
                  .Append(",\"decor\":").Append(p.Decor ? "true" : "false")
                  .Append(",\"fps\":").Append(p.Fps.ToString("0.##",
                      System.Globalization.CultureInfo.InvariantCulture))
                  .Append(",\"loop\":").Append(p.Loop ? "true" : "false")
                  .Append(",\"pingPong\":").Append(p.PingPong ? "true" : "false")
                  .Append(",\"frameCount\":").Append(p.Frames.Count)
                  .Append("}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private static string PlaceAnimatedTile(
            Dictionary<string, string> query, string body)
        {
            int px = GetInt(query, "x", -1);
            int py = GetInt(query, "y", -1);
            int layer = GetInt(query, "layer", 1);
            bool decor = GetBool(query, "decor");
            float fps;
            if (!float.TryParse(Get(query, "fps"),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out fps))
            {
                fps = 4f;
            }
            bool loop = GetBool(query, "loop");
            bool pingPong = GetBool(query, "pingpong");
            string framesParam = Get(query, "frames");

            if (px < 0 || py < 0 || string.IsNullOrEmpty(framesParam))
            {
                return "{\"ok\":false,\"msg\":\"missing x, y, or frames\"}";
            }

            var frames = ParseFramesParam(framesParam);
            if (frames.Count == 0)
            {
                return "{\"ok\":false,\"msg\":\"no valid frames\"}";
            }

            AnimatedModTiles.Place(px, py, layer, decor, fps, loop, pingPong, frames);
            return "{\"ok\":true}";
        }

        private static string ArmAnimatedStamp(
            Dictionary<string, string> query, string body)
        {
            float fps;
            if (!float.TryParse(Get(query, "fps"),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out fps))
            {
                fps = 4f;
            }
            bool loop = GetBool(query, "loop");
            bool pingPong = GetBool(query, "pingpong");
            bool decor = GetBool(query, "decor");
            string name = Get(query, "name");
            string framesParam = Get(query, "frames");

            if (string.IsNullOrEmpty(framesParam))
            {
                return "{\"ok\":false,\"msg\":\"no frames\"}";
            }

            var frames = ParseFramesParam(framesParam);
            if (frames.Count == 0)
            {
                return "{\"ok\":false,\"msg\":\"no valid frames\"}";
            }

            // Validate that at least the first atlas exists
            if (!TileSets.HasAtlas(frames[0].Atlas))
            {
                return "{\"ok\":false,\"msg\":\"atlas not cached: " + frames[0].Atlas + "\"}";
            }

            EditorTools.AnimatedStamp = new AnimatedTileStamp
            {
                Frames = frames,
                Fps = fps,
                Loop = loop,
                PingPong = pingPong,
                Decor = decor,
                Name = name,
            };
            EditorTools.SetMode(EditorToolMode.PaintAnimatedTile);
            return "{\"ok\":true,\"msg\":" +
                Quote("animated stamp armed — " + EditorTools.HintText()) + "}";
        }

        /// <summary>
        /// Parse the "frames" query param (format: atlas:rx:ry:rw:rh;atlas:rx:ry:rw:rh).
        /// </summary>
        private static List<AnimatedModTiles.AnimFrame> ParseFramesParam(string param)
        {
            var result = new List<AnimatedModTiles.AnimFrame>();
            foreach (var part in param.Split(';'))
            {
                if (string.IsNullOrEmpty(part)) continue;
                string[] pieces = part.Split(':');
                if (pieces.Length < 5) continue;
                int rx, ry, rw, rh;
                if (!int.TryParse(pieces[pieces.Length - 4], out rx) ||
                    !int.TryParse(pieces[pieces.Length - 3], out ry) ||
                    !int.TryParse(pieces[pieces.Length - 2], out rw) ||
                    !int.TryParse(pieces[pieces.Length - 1], out rh))
                {
                    continue;
                }
                string atlas = string.Join(":", pieces, 0, pieces.Length - 4);
                result.Add(new AnimatedModTiles.AnimFrame
                {
                    Atlas = atlas, Rx = rx, Ry = ry, Rw = rw, Rh = rh
                });
            }
            return result;
        }

        private static string SetTool(string name)
        {
            EditorToolMode mode;
            switch (name)
            {
                case "paint": mode = EditorToolMode.PaintElectric; break;
                case "erase": mode = EditorToolMode.EraseElectric; break;
                case "link": mode = EditorToolMode.LinkSwitch; break;
                case "tilepaint": mode = EditorToolMode.PaintTile; break;
                case "tileerase": mode = EditorToolMode.EraseTile; break;
                case "animtilepaint": mode = EditorToolMode.PaintAnimatedTile; break;
                case "animtileerase": mode = EditorToolMode.EraseAnimatedTile; break;
                case "connectedpaint": mode = EditorToolMode.PaintConnectedTile; break;
                case "none":
                case "": mode = EditorToolMode.None; break;
                default:
                    return "{\"ok\":false,\"msg\":\"unknown tool: " + name + "\"}";
            }
            EditorTools.SetMode(mode);
            string msg = mode == EditorToolMode.None
                ? "tool deactivated"
                : "tool active: " + name + " — " + EditorTools.HintText();
            return "{\"ok\":true,\"tool\":" + Quote(ToolName(mode)) + ",\"msg\":" + Quote(msg) + "}";
        }

        private static void SetSetting(string name, bool value)
        {
            switch (name)
            {
                case "devBlocks": Plugin.CfgUnlockDevBlocks.Value = value; break;
                case "dlcBlocks": Plugin.CfgUnlockDlcBlocks.Value = value; break;
                case "layers": Plugin.CfgIgnoreLayerRestrictions.Value = value; break;
                case "completion": Plugin.CfgIgnoreCompletionState.Value = value; break;
                case "restrictions":
                    Plugin.CfgIgnoreAllRestrictions.Value = value;
                    Restrictions.IgnoreAll = value;
                    break;
                case "xray":
                    Plugin.CfgXRay.Value = value;
                    XRay.Enabled = value;
                    break;
                case "fenceOverlay":
                    Plugin.CfgFenceOverlay.Value = value;
                    FenceOverlay.Enabled = value;
                    break;
                case "arrows":
                    Plugin.CfgTriggerArrows.Value = value;
                    TriggerArrows.Enabled = value;
                    break;
                case "cameraLock":
                    Plugin.CfgLockCameraPan.Value = value;
                    EditorCamera.LockPan = value;
                    break;
                case "forceWindowed": Plugin.CfgForceWindowed.Value = value; break;
                case "autoOpen": Plugin.CfgWebUiAutoOpen.Value = value; break;
                case "vanillaFallback":
                    Plugin.CfgVanillaFallback.Value = value;
                    E2EApi.Persistence.VanillaFallback.Enabled = value;
                    break;
                case "infiniteEnergy": E2EApi.Players.Player.InfiniteEnergy = value; break;
            }
        }

        private static void SetNumSetting(string name, float value)
        {
            switch (name)
            {
                case "guardCap": Plugin.CfgGuardInmateCap.Value = (int)value; break;
                case "zoomSteps": Plugin.CfgExtraZoomSteps.Value = (int)value; break;
                case "fenceDamage": ElectricFences.DamagePerTick = value; break;
                case "windowWidth": Plugin.CfgWindowedWidth.Value = (int)value; break;
                case "windowHeight": Plugin.CfgWindowedHeight.Value = (int)value; break;
            }
        }

        private static string GetFloorsJson()
        {
            var sb = new StringBuilder();
            sb.Append("{\"floors\":[");
            bool first = true;
            foreach (var floor in GameMap.GetFloors())
            {
                if (!first)
                {
                    sb.Append(",");
                }
                first = false;
                sb.Append("{\"index\":").Append(floor.Index);
                sb.Append(",\"name\":").Append(Quote(floor.Name));
                sb.Append(",\"start\":").Append(floor.IsStartFloor ? "true" : "false");
                sb.Append(",\"hasMap\":").Append(floor.HasTexture ? "true" : "false");
                sb.Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private static string GetPlayerJson()
        {
            var player = E2EApi.Players.Player.GetLocal();
            if (player == null || !player.IsValid)
            {
                return "{\"present\":false}";
            }
            int x, y, floor;
            bool hasTile = player.GetTile(out x, out y, out floor);
            var sb = new StringBuilder();
            sb.Append("{\"present\":true");
            sb.Append(",\"name\":").Append(Quote(player.Name ?? ""));
            sb.Append(",\"health\":").Append(player.Health.ToString("0.#",
                System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(",\"energy\":").Append(player.Energy.ToString("0.#",
                System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(",\"money\":").Append(player.Money.ToString("0.#",
                System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(",\"heat\":").Append(player.Heat.ToString("0.#",
                System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(",\"infiniteEnergy\":")
              .Append(E2EApi.Players.Player.InfiniteEnergy ? "true" : "false");
            sb.Append(",\"tile\":");
            if (hasTile)
            {
                sb.Append("{\"x\":").Append(x).Append(",\"y\":").Append(y)
                  .Append(",\"floor\":").Append(floor).Append("}");
            }
            else
            {
                sb.Append("null");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static string DoTeleport(int x, int y, int floor)
        {
            var player = E2EApi.Players.Player.GetLocal();
            if (player == null || !player.IsValid)
            {
                return "{\"ok\":false,\"msg\":\"no player pawn (start the map first)\"}";
            }
            bool ok = player.TeleportToTile(x, y, floor);
            return ok
                ? "{\"ok\":true,\"msg\":\"teleported to (" + x + "," + y + ") floor " + floor + "\"}"
                : "{\"ok\":false,\"msg\":\"teleport failed (bad tile/floor?)\"}";
        }

        private static string RunCheat(string name, string value)
        {
            var player = E2EApi.Players.Player.GetLocal();
            bool hasPawn = player != null && player.IsValid;
            switch (name)
            {
                case "heal":
                    if (!hasPawn) break;
                    player.Heal();
                    return Ok("healed to 100");
                case "energy":
                    if (!hasPawn) break;
                    player.Energy = 100f;
                    return Ok("energy restored");
                case "money":
                    if (!hasPawn) break;
                    player.Money = 999f;
                    return Ok("money set to 999");
                case "stealth":
                    if (!hasPawn) break;
                    player.ClearSuspicion();
                    return Ok("heat cleared, wanted flags off");
                case "ko-guards":
                    return Ok(Cheats.KnockOutGuards() + " guard(s) knocked out");
                case "ko-dogs":
                    return Ok(Cheats.KnockOutDogs() + " dog(s) knocked out");
                default:
                    return "{\"ok\":false,\"msg\":\"unknown cheat: " + name + "\"}";
            }
            return "{\"ok\":false,\"msg\":\"no player pawn (start the map first)\"}";
        }

        private static string HandleGeometry(string action, Dictionary<string, string> query)
        {
            if (!E2EApi.Events.GameEvents.IsEditorActive)
            {
                return "{\"ok\":false,\"msg\":\"open the level editor first\"}";
            }
            try
            {
                switch (action)
                {
                    case "select":
                        MapGeometry.SelectLayer(GetInt(query, "index", 0));
                        break;
                    case "add":
                        MapGeometry.AddLayer(ParseGeometryLayerType(Get(query, "type")));
                        break;
                    case "remove":
                        MapGeometry.RemoveLayer(GetInt(query, "index", MapGeometry.SelectedVirtualLayerIndex));
                        break;
                    case "move":
                        MapGeometry.MoveLayer(
                            GetInt(query, "index", MapGeometry.SelectedVirtualLayerIndex),
                            GetInt(query, "delta", 0));
                        break;
                    case "duplicate":
                        MapGeometry.DuplicateLayer(GetInt(query, "index", MapGeometry.SelectedVirtualLayerIndex));
                        break;
                    case "type":
                        MapGeometry.SetLayerType(
                            GetInt(query, "index", MapGeometry.SelectedVirtualLayerIndex),
                            ParseGeometryLayerType(Get(query, "type")));
                        break;
                    case "bounds-delta":
                        ApplyGeometryBoundsDelta(Get(query, "field"), GetInt(query, "delta", 0));
                        break;
                    case "reset":
                        MapGeometry.ResetDefault();
                        break;
                    default:
                        return "{\"ok\":false,\"msg\":\"unknown geometry action\"}";
                }
                return "{\"ok\":true,\"mapGeometry\":" + MapGeometry.ToJson() + "}";
            }
            catch (Exception e)
            {
                return "{\"ok\":false,\"msg\":" + Quote(e.Message) + "}";
            }
        }

        private static void ApplyGeometryBoundsDelta(string field, int delta)
        {
            int width = MapGeometry.Width;
            int height = MapGeometry.Height;
            int originX = MapGeometry.OriginX;
            int originY = MapGeometry.OriginY;
            switch (field)
            {
                case "width": width += delta; break;
                case "height": height += delta; break;
                case "originX": originX += delta; break;
                case "originY": originY += delta; break;
                default: return;
            }
            MapGeometry.SetBounds(width, height, originX, originY);
        }

        private static MapGeometry.VirtualLayerType ParseGeometryLayerType(string value)
        {
            try
            {
                return (MapGeometry.VirtualLayerType)Enum.Parse(
                    typeof(MapGeometry.VirtualLayerType), value, true);
            }
            catch
            {
                return MapGeometry.VirtualLayerType.Ground;
            }
        }

        private static string Ok(string msg) => "{\"ok\":true,\"msg\":" + Quote(msg) + "}";

        private static string GetDebugJson() => E2EApi.Diagnostics.OverlayDebugJson();

        // ---- plumbing ----

        private static string ReadLine(Stream stream)
        {
            var sb = new StringBuilder();
            while (true)
            {
                int b = stream.ReadByte();
                if (b == -1 || b == '\n')
                {
                    break;
                }
                if (b != '\r')
                {
                    sb.Append((char)b);
                }
            }
            return sb.ToString();
        }

        private static void WriteResponse(Stream stream, int status, string contentType, byte[] body)
        {
            string reason = status == 200 ? "OK" : status == 404 ? "Not Found" : status == 400 ? "Bad Request" : "Error";
            string header = "HTTP/1.1 " + status + " " + reason + "\r\n" +
                            "Content-Type: " + contentType + "\r\n" +
                            "Content-Length: " + body.Length + "\r\n" +
                            "Cache-Control: no-store\r\n" +
                            "Connection: close\r\n\r\n";
            byte[] headerBytes = Encoding.ASCII.GetBytes(header);
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Write(body, 0, body.Length);
            stream.Flush();
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>();
            foreach (string pair in query.Split('&'))
            {
                int eq = pair.IndexOf('=');
                if (eq > 0)
                {
                    result[Uri.UnescapeDataString(pair.Substring(0, eq))] =
                        Uri.UnescapeDataString(pair.Substring(eq + 1));
                }
            }
            return result;
        }

        private static string Get(Dictionary<string, string> query, string key)
        {
            string value;
            return query.TryGetValue(key, out value) ? value : "";
        }

        private static bool GetBool(Dictionary<string, string> query, string key)
        {
            string v = Get(query, key);
            if (string.IsNullOrEmpty(v))
            {
                return false;
            }
            v = v.ToLowerInvariant();
            return v == "true" || v == "1" || v == "yes" || v == "on";
        }

        private static int GetInt(Dictionary<string, string> query, string key, int fallback)
        {
            int value;
            return int.TryParse(Get(query, key), out value) ? value : fallback;
        }

        private static string Quote(string value)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");
            return sb.ToString();
        }
    }
}
