using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using E2EApi.Editor;
using E2EApi.Events;
using E2EApi.UI;
using UnityEngine;

namespace MapEditorMod
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;
        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> CfgUnlockDevBlocks;
        internal static ConfigEntry<bool> CfgUnlockDlcBlocks;
        internal static ConfigEntry<bool> CfgIgnoreLayerRestrictions;
        internal static ConfigEntry<bool> CfgIgnoreCompletionState;
        internal static ConfigEntry<int> CfgGuardInmateCap;
        internal static ConfigEntry<bool> CfgForceWindowed;
        internal static ConfigEntry<int> CfgWindowedWidth;
        internal static ConfigEntry<int> CfgWindowedHeight;
        internal static ConfigEntry<KeyCode> CfgWindowKey;
        internal static ConfigEntry<KeyCode> CfgFenceKey;
        internal static ConfigEntry<KeyCode> CfgTriggerKey;
        internal static ConfigEntry<KeyCode> CfgActivateKey;
        internal static ConfigEntry<int> CfgWebUiPort;
        internal static ConfigEntry<bool> CfgWebUiAutoOpen;

        internal static ConfigEntry<bool> CfgIgnoreAllRestrictions;
        internal static ConfigEntry<int> CfgExtraZoomSteps;
        internal static ConfigEntry<bool> CfgLockCameraPan;
        internal static ConfigEntry<bool> CfgVanillaFallback;
        internal static ConfigEntry<string> CfgSkipVersion;

        private readonly EditorWindow _window = new EditorWindow();
        private readonly QuickPanel _quickPanel = new QuickPanel();
        private readonly HoverTooltip _tooltip = new HoverTooltip();
        private WebUi.WebUiServer _webUi;
        private bool _inEditor;
        private bool _browserOpened;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo($"{PluginInfo.Name} {PluginInfo.Version} loading (E2EApi {E2EApi.E2EApiInfo.Version})");

            BindConfig();
            ApplyConfig();

            // sidecar-backed features must hook save/load before any map loads
            E2EApi.Features.ElectricFences.Initialise();
            E2EApi.Features.Triggers.Initialise();
            E2EApi.Features.ModTiles.Initialise();
            E2EApi.Persistence.VanillaFallback.Initialise();

            GameEvents.LevelLoaded += () => Log.LogInfo("level loaded");
            GameEvents.EditorEntered += OnEditorEntered;
            GameEvents.EditorExited += OnEditorExited;
            GameEvents.PlaytestStarted += OnPlaytestStarted;
            GameEvents.PlaytestEnded += OnPlaytestEnded;

            _webUi = new WebUi.WebUiServer(CfgWebUiPort.Value);
            try
            {
                _webUi.Start();
            }
            catch (System.Exception e)
            {
                Log.LogError($"web UI failed to start on port {CfgWebUiPort.Value}: {e.Message}");
            }

            UpdateChecker.CheckAsync();
        }

        private void OnDestroy()
        {
            if (_webUi != null)
            {
                _webUi.Stop();
            }
        }

        private void OpenBrowser()
        {
            string url = _webUi.Url;
            try
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    System.Diagnostics.Process.Start(url);
                }
                else
                {
                    System.Diagnostics.Process.Start("xdg-open", url);
                }
                Log.LogInfo($"opened web UI: {url}");
            }
            catch (System.Exception e)
            {
                Log.LogWarning($"could not open browser ({e.Message}) — open {url} manually");
            }
        }

        private void Start()
        {
            if (CfgForceWindowed.Value)
            {
                WindowMode.ForceWindowed(CfgWindowedWidth.Value, CfgWindowedHeight.Value);
            }
        }

        private void Update()
        {
            TickLoadingOverlay();
            if (LoadingOverlay.Visible)
            {
                return; // mod asset work in progress — eat all mod input
            }
            if (Input.GetKeyDown(CfgWindowKey.Value))
            {
                _window.Toggle();
                if (_window.IsCreated && _inEditor)
                {
                    _window.Mapping.Refresh();
                }
            }
            bool editorActive = _inEditor && GameEvents.IsEditorActive;
            if (editorActive)
            {
                EditorTools.Tick();
            }
            if (editorActive && _window.IsCreated)
            {
                if (Input.GetKeyDown(CfgFenceKey.Value))
                {
                    _window.Extras.ToggleFenceAtCursor();
                }
                if (Input.GetKeyDown(CfgTriggerKey.Value))
                {
                    _window.Extras.LinkStep();
                }
            }
            else if (!editorActive && Input.GetKeyDown(CfgActivateKey.Value))
            {
                // in play mode (incl. play-test): activate the button tile under the player
                var player = E2EApi.Players.Player.GetLocal();
                if (player != null)
                {
                    E2EApi.Features.Triggers.ActivateUnder(player);
                }
            }
        }

        private void OnEditorEntered()
        {
            Log.LogInfo("level editor entered");
            _inEditor = true;
            _quickPanel.Show();
            E2EApi.Editor.EditorCamera.ExtendZoom(CfgExtraZoomSteps.Value);
            E2EApi.Editor.EditorCamera.ApplyLock();
            if (CfgWebUiAutoOpen.Value && !_browserOpened)
            {
                _browserOpened = true;
                OpenBrowser();
            }
        }

        private void OnEditorExited()
        {
            Log.LogInfo("level editor exited");
            _inEditor = false;
            EditorTools.SetMode(EditorToolMode.None);
            _quickPanel.Hide();
            _window.Hide();
        }

        /// <summary>
        /// Editor → play-test: tear down every mod editor artifact. The editor
        /// session object survives the preview, so EditorExited never fires —
        /// this hook covers the gap.
        /// </summary>
        private void OnPlaytestStarted()
        {
            Log.LogInfo("playtest started — hiding editor artifacts");
            EditorTools.SetMode(EditorToolMode.None); // also clears mouse suppression
            VanillaEditor.SetBrushVisible(false);     // …but keep the brush hidden in play
            _quickPanel.Hide();
            _window.Hide();
            VanillaEditor.SetEditorUiVisible(false);
        }

        private void OnPlaytestEnded()
        {
            Log.LogInfo("playtest ended — restoring editor artifacts");
            if (GameEvents.IsInEditor)
            {
                _quickPanel.Show();
                VanillaEditor.SetBrushVisible(true);
                VanillaEditor.SetEditorUiVisible(true);
            }
        }

        private void LateUpdate()
        {
            _tooltip.Tick(_inEditor && GameEvents.IsEditorActive);
        }

        /// <summary>
        /// Show/update the blocking overlay while the mod is doing its own
        /// asset work (tileset harvest, per-map atlas preload); hide it the
        /// moment the work finishes.
        /// </summary>
        private void TickLoadingOverlay()
        {
            if (TileSets.Busy)
            {
                int total = Mathf.Max(TileSets.ProgressTotal, 1);
                LoadingOverlay.Show(
                    "Loading custom tilesets… " + TileSets.Status,
                    TileSets.ProgressCurrent / (float)total);
                return;
            }
            if (E2EApi.Features.ModTiles.Preloading)
            {
                int total = Mathf.Max(E2EApi.Features.ModTiles.PreloadTotal, 1);
                LoadingOverlay.Show(
                    "Loading modded tile art… " +
                    E2EApi.Features.ModTiles.PreloadCurrent + "/" + total,
                    E2EApi.Features.ModTiles.PreloadCurrent / (float)total);
                return;
            }
            LoadingOverlay.Hide();
        }

        private void BindConfig()
        {
            CfgUnlockDevBlocks = Config.Bind(
                "Editor", "UnlockDevBlocks", true,
                "Show dev-only (m_EditorOnly) blocks in the level editor spawnlist. (V0 feature)");
            CfgUnlockDlcBlocks = Config.Bind(
                "Editor", "UnlockDlcBlocks", true,
                "Unlock all installed DLC blocks: make the map items and craft recipes of every " +
                "installed DLC (and of all base-game prisons, incl. transports) available in " +
                "custom maps. DLC you do not own stays locked (Steam entitlement check).");
            CfgIgnoreLayerRestrictions = Config.Bind(
                "Editor", "IgnoreLayerRestrictions", false,
                "Offer every block on every layer regardless of its m_ValidLayers mask. (V0 behaviour; can produce broken maps)");
            CfgIgnoreCompletionState = Config.Bind(
                "Editor", "IgnoreCompletionState", false,
                "Also list blocks the game marks as incomplete/invalid.");
            CfgGuardInmateCap = Config.Bind(
                "Editor", "GuardInmateCap", 24,
                "Force the available guard/inmate count in the editor to this value (0 = vanilla behaviour). (V0 feature)");
            CfgForceWindowed = Config.Bind(
                "Display", "ForceWindowed", true,
                "Switch the game to windowed mode on startup.");
            CfgWindowedWidth = Config.Bind(
                "Display", "WindowedWidth", 1600,
                "Window width when ForceWindowed is on (0 = keep current).");
            CfgWindowedHeight = Config.Bind(
                "Display", "WindowedHeight", 900,
                "Window height when ForceWindowed is on (0 = keep current).");
            CfgWindowKey = Config.Bind(
                "UI", "WindowToggleKey", KeyCode.F10,
                "Hotkey that shows/hides the mod window.");
            CfgFenceKey = Config.Bind(
                "Extras", "FenceToggleKey", KeyCode.F6,
                "Editor hotkey: toggle an electric fence on the tile under the cursor.");
            CfgTriggerKey = Config.Bind(
                "Extras", "TriggerLinkKey", KeyCode.F7,
                "Editor hotkey: press once on the button tile, once on the fence tile to link them.");
            CfgActivateKey = Config.Bind(
                "Extras", "TriggerActivateKey", KeyCode.E,
                "Play-mode hotkey: activate the trigger button you are standing on.");
            CfgWebUiPort = Config.Bind(
                "WebUI", "Port", 8723,
                "Port for the browser-based editor UI (http://127.0.0.1:<port>/).");
            CfgWebUiAutoOpen = Config.Bind(
                "WebUI", "AutoOpenBrowser", true,
                "Open the editor UI in your browser when you first enter the level editor.");
            CfgIgnoreAllRestrictions = Config.Bind(
                "Editor", "IgnoreAllRestrictions", false,
                "Master switch: place anything anywhere, save/upload/play unfinished maps.");
            CfgExtraZoomSteps = Config.Bind(
                "Editor", "ExtraZoomSteps", 2,
                "Additional zoom-in steps in the level editor (each halves the view size).");
            CfgLockCameraPan = Config.Bind(
                "Editor", "LockCameraPan", false,
                "Lock the editor camera in place: no automatic edge panning, only WASD/arrow keys move it.");
            CfgVanillaFallback = Config.Bind(
                "Tilesets", "VanillaFallbackMap", true,
                "When a map uses modded tiles, store a placeholder disclaimer map in the vanilla " +
                "Level.dat for unmodded players (the real map travels in the Level.e2e sidecar and " +
                "is restored automatically on modded clients). When off, the map saves normally and " +
                "vanilla players simply see it without the modded tiles.");

            CfgSkipVersion = Config.Bind(
                "Updates", "SkipVersion", "",
                "Version string to suppress update prompts for (set automatically by 'Do not ask for this release').");

            Config.SettingChanged += (_, _2) => ApplyConfig();
        }

        private void ApplyConfig()
        {
            Blocks.ShowEditorOnly = CfgUnlockDevBlocks.Value;
            DlcContent.UnlockInstalled = CfgUnlockDlcBlocks.Value;
            Blocks.IgnoreLayerRestrictions = CfgIgnoreLayerRestrictions.Value;
            Blocks.IgnoreCompletionState = CfgIgnoreCompletionState.Value;
            Limits.SetGuardInmateAvailability(CfgGuardInmateCap.Value);
            EditorCamera.LockPan = CfgLockCameraPan.Value;
            E2EApi.Persistence.VanillaFallback.Enabled = CfgVanillaFallback.Value;
            if (CfgIgnoreAllRestrictions.Value)
            {
                Restrictions.IgnoreAll = true;
            }
        }
    }

    public static class PluginInfo
    {
        public const string Guid = "org.anonymusdennis.e2e.mapeditor";
        public const string Name = "E2E Map Editor";
        public const string Version = "2.0.0";
    }
}
