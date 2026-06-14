using System;
using System.Reflection;
using E2EApi.Events;
using E2EApi.Persistence;

namespace E2EApi.Features
{
    /// <summary>
    /// Multiplayer compatibility gate.
    ///
    /// When a custom map with <see cref="ModExtras.RequiresMod"/> set to
    /// <c>true</c> is loaded inside a Photon room (PUN 1.x), the host
    /// broadcasts a room custom property keyed <see cref="PropertyKey"/>
    /// whose value is the running mod/API version string. Modded clients that
    /// are already in the room (or that join later) can read this property to
    /// confirm the map needs the mod; unmodded clients have no gate code at
    /// all and are served the vanilla-fallback disclaimer map instead.
    ///
    /// Usage (MapEditorMod):
    /// <code>
    ///   MultiplayerGate.Initialise();
    ///   MultiplayerGate.RoomRequiresMod    += OnRoomRequiresMod;
    ///   MultiplayerGate.RoomNoLongerRequiresMod += OnRoomNormal;
    /// </code>
    ///
    /// Public read-only state:
    ///   <see cref="IsRequiredInCurrentRoom"/> — true when the active Photon
    ///     room carries the <see cref="PropertyKey"/> property.
    ///   <see cref="RoomModVersion"/> — the version string from that property
    ///     (null when absent).
    ///   <see cref="IsMultiplayer"/> — true when Photon reports being in a room.
    ///   <see cref="IsHost"/> — true when the local player is the Photon master
    ///     client (room owner).
    /// </summary>
    public static class MultiplayerGate
    {
        /// <summary>
        /// Room custom-property key used to broadcast the mod requirement.
        /// Value: the host's mod-version string (e.g. "2.4.0").
        /// </summary>
        public const string PropertyKey = "e2e_mapeditor";

        /// <summary>
        /// Fired on the Unity main thread when the current room transitions to
        /// a state where the map requires this mod (host set the property).
        /// </summary>
        public static event Action RoomRequiresMod;

        /// <summary>
        /// Fired on the Unity main thread when the requirement is lifted
        /// (map changed to one that does not require the mod, or left the room).
        /// </summary>
        public static event Action RoomNoLongerRequiresMod;

        /// <summary>True when Photon reports being in a room.</summary>
        public static bool IsMultiplayer => GetBoolProperty("inRoom");

        /// <summary>True when the local player owns the Photon room.</summary>
        public static bool IsHost => GetBoolProperty("isMasterClient");

        /// <summary>
        /// True when the current room's custom properties include
        /// <see cref="PropertyKey"/> (some client in this room is hosting a
        /// mod-required map).
        /// </summary>
        public static bool IsRequiredInCurrentRoom { get; private set; }

        /// <summary>
        /// The version string stored in the room's <see cref="PropertyKey"/>
        /// property, or <c>null</c> when the property is absent.
        /// </summary>
        public static string RoomModVersion { get; private set; }

        // ── internal ──────────────────────────────────────────────────────────

        private static bool _initialised;

        // Reflected Photon types / members (resolved once on first Initialise).
        private static Type _photonNetworkType;
        private static PropertyInfo _inRoomProp;
        private static PropertyInfo _isMasterClientProp;
        private static PropertyInfo _roomProp;
        private static Type _hashtableType;
        private static MethodInfo _hashtableAdd;
        private static PropertyInfo _roomCustomPropertiesProp;
        private static MethodInfo _roomSetCustomProperties;
        private static MethodInfo _roomRemoveCustomProperties;
        private static PropertyInfo _roomPropertiesIndexer;
        private static MethodInfo _roomContainsKey;

        /// <summary>
        /// Wire up save/load hooks and the level-loaded event. Safe to call
        /// multiple times; only the first call has any effect.
        /// </summary>
        public static void Initialise()
        {
            if (_initialised) return;
            _initialised = true;

            ResolvePhotonTypes();
            ModExtras.Loaded += OnExtrasLoaded;
            GameEvents.LevelLoaded += OnLevelLoaded;
            GameEvents.LevelUnloaded += OnLevelUnloaded;
        }

        // ── event handlers ────────────────────────────────────────────────────

        private static void OnExtrasLoaded(ModExtras extras)
        {
            // When a new sidecar loads (editor entry or play-mode load),
            // re-evaluate whether we should broadcast the requirement.
            if (IsMultiplayer && IsHost && extras.RequiresMod)
            {
                SetRoomProperty(E2EApiInfo.Version);
            }
            else if (IsMultiplayer && IsHost && !extras.RequiresMod)
            {
                ClearRoomProperty();
            }
        }

        private static void OnLevelLoaded()
        {
            // Re-evaluate room state on every level load (handles joining a
            // room where the host already set the property before we arrived).
            RefreshRoomState();
        }

        private static void OnLevelUnloaded()
        {
            // If we leave a level / room, clear our cached state.
            if (!IsMultiplayer)
            {
                UpdateState(false, null);
            }
        }

        // ── public helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Re-reads the current room properties and updates
        /// <see cref="IsRequiredInCurrentRoom"/> / <see cref="RoomModVersion"/>.
        /// Call this after joining a room or when a room-property-changed
        /// Photon callback fires. Safe to call at any time.
        /// </summary>
        public static void RefreshRoomState()
        {
            if (!IsMultiplayer)
            {
                UpdateState(false, null);
                return;
            }

            string version = ReadRoomProperty(PropertyKey);
            UpdateState(version != null, version);
        }

        /// <summary>
        /// Broadcast that the current map requires this mod by writing the
        /// room property. Only the host (master client) should call this.
        /// </summary>
        public static bool AnnounceRequiresMod()
        {
            if (!IsMultiplayer || !IsHost) return false;
            return SetRoomProperty(E2EApiInfo.Version);
        }

        /// <summary>
        /// Remove the mod-required room property. Only the host should call
        /// this (e.g. when switching to a vanilla map).
        /// </summary>
        public static bool AnnounceNoLongerRequiresMod()
        {
            if (!IsMultiplayer || !IsHost) return false;
            return ClearRoomProperty();
        }

        // ── private implementation ────────────────────────────────────────────

        private static void UpdateState(bool required, string version)
        {
            bool wasRequired = IsRequiredInCurrentRoom;
            IsRequiredInCurrentRoom = required;
            RoomModVersion = version;

            if (required && !wasRequired)
            {
                FireEvent(RoomRequiresMod, nameof(RoomRequiresMod));
            }
            else if (!required && wasRequired)
            {
                FireEvent(RoomNoLongerRequiresMod, nameof(RoomNoLongerRequiresMod));
            }
        }

        private static void FireEvent(Action handler, string name)
        {
            if (handler == null) return;
            try { handler(); }
            catch (Exception e)
            {
                Log.Error("MultiplayerGate." + name + " subscriber threw: " + e);
            }
        }

        // ── Photon bridge (reflection) ─────────────────────────────────────────
        // PUN 1.x ships in Assembly-CSharp-firstpass (older Unity) or in its
        // own PhotonUnityNetworking.dll. We resolve at runtime so the API DLL
        // compiles cleanly without a hard reference to those assemblies.

        private static readonly string[] _photonAssemblyCandidates = new[]
        {
            "Assembly-CSharp-firstpass",
            "PhotonUnityNetworking",
            "Photon3Unity3D",
        };

        private static readonly string[] _hashtableCandidates = new[]
        {
            "Assembly-CSharp-firstpass",
            "Photon3Unity3D",
            "ExitGames.Client.Photon",
        };

        private static void ResolvePhotonTypes()
        {
            foreach (var asm in _photonAssemblyCandidates)
            {
                var t = Type.GetType("PhotonNetwork, " + asm);
                if (t != null)
                {
                    _photonNetworkType = t;
                    break;
                }
            }

            if (_photonNetworkType == null)
            {
                // Try all loaded assemblies as a fallback
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = asm.GetType("PhotonNetwork");
                    if (t != null)
                    {
                        _photonNetworkType = t;
                        break;
                    }
                }
            }

            if (_photonNetworkType == null)
            {
                Log.Info("MultiplayerGate: Photon not found — multiplayer gate will be inactive.");
                return;
            }

            _inRoomProp = _photonNetworkType.GetProperty("inRoom",
                BindingFlags.Static | BindingFlags.Public);
            _isMasterClientProp = _photonNetworkType.GetProperty("isMasterClient",
                BindingFlags.Static | BindingFlags.Public);
            _roomProp = _photonNetworkType.GetProperty("room",
                BindingFlags.Static | BindingFlags.Public);

            // Resolve ExitGames.Client.Photon.Hashtable
            foreach (var asmName in _hashtableCandidates)
            {
                var t = Type.GetType("ExitGames.Client.Photon.Hashtable, " + asmName);
                if (t != null)
                {
                    _hashtableType = t;
                    break;
                }
            }
            if (_hashtableType == null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = asm.GetType("ExitGames.Client.Photon.Hashtable");
                    if (t != null) { _hashtableType = t; break; }
                }
            }

            if (_hashtableType != null)
            {
                _hashtableAdd = _hashtableType.GetMethod("Add",
                    new[] { typeof(object), typeof(object) });
            }

            // Room properties (PUN 1 uses lowercase; PUN 2 uses capitalised)
            if (_roomProp != null)
            {
                object testRoom = null; // can't call static get here safely
                // Cache method info from the return type
                var roomType = _roomProp.PropertyType;
                if (roomType != null)
                {
                    // SetCustomProperties(Hashtable, Hashtable, WebFlags)
                    // or SetCustomProperties(Hashtable)
                    foreach (var m in roomType.GetMethods())
                    {
                        if (m.Name == "SetCustomProperties")
                        {
                            var p = m.GetParameters();
                            if (p.Length >= 1 && _hashtableType != null &&
                                p[0].ParameterType.IsAssignableFrom(_hashtableType))
                            {
                                _roomSetCustomProperties = m;
                            }
                        }
                    }

                    // CustomProperties or customProperties
                    _roomCustomPropertiesProp = roomType.GetProperty("CustomProperties")
                                             ?? roomType.GetProperty("customProperties");

                    if (_roomCustomPropertiesProp != null)
                    {
                        var propsType = _roomCustomPropertiesProp.PropertyType;
                        // ContainsKey(object) — Hashtable has this as IDictionary
                        _roomContainsKey = propsType.GetMethod("ContainsKey",
                            new[] { typeof(object) });
                        _roomPropertiesIndexer = propsType.GetProperty("Item",
                            new[] { typeof(object) });
                    }
                }
            }

            Log.Info("MultiplayerGate: Photon resolved — " +
                     (_roomSetCustomProperties != null ? "full" : "read-only") +
                     " room-property support.");
        }

        private static bool GetBoolProperty(string name)
        {
            if (_photonNetworkType == null) return false;
            try
            {
                var prop = name == "inRoom" ? _inRoomProp
                         : name == "isMasterClient" ? _isMasterClientProp
                         : _photonNetworkType.GetProperty(name,
                               BindingFlags.Static | BindingFlags.Public);
                if (prop == null) return false;
                return (bool)prop.GetValue(null, null);
            }
            catch { return false; }
        }

        private static object GetRoom()
        {
            if (_roomProp == null) return null;
            try { return _roomProp.GetValue(null, null); }
            catch { return null; }
        }

        private static string ReadRoomProperty(string key)
        {
            try
            {
                var room = GetRoom();
                if (room == null) return null;
                if (_roomCustomPropertiesProp == null) return null;
                var props = _roomCustomPropertiesProp.GetValue(room, null);
                if (props == null) return null;
                if (_roomContainsKey != null)
                {
                    if (!(bool)_roomContainsKey.Invoke(props, new object[] { key }))
                        return null;
                }
                if (_roomPropertiesIndexer == null) return null;
                object val = _roomPropertiesIndexer.GetValue(props, new object[] { key });
                return val?.ToString();
            }
            catch (Exception e)
            {
                Log.Warn("MultiplayerGate: ReadRoomProperty failed: " + e.Message);
                return null;
            }
        }

        private static bool SetRoomProperty(string value)
        {
            try
            {
                var room = GetRoom();
                if (room == null) return false;
                if (_roomSetCustomProperties == null || _hashtableType == null) return false;

                object ht = Activator.CreateInstance(_hashtableType);
                if (_hashtableAdd != null)
                    _hashtableAdd.Invoke(ht, new object[] { (object)PropertyKey, (object)value });
                else
                    _hashtableType.GetMethod("set_Item")?.Invoke(ht,
                        new object[] { (object)PropertyKey, (object)value });

                // Invoke SetCustomProperties(Hashtable) or SetCustomProperties(Hashtable, null)
                var prms = _roomSetCustomProperties.GetParameters();
                if (prms.Length == 1)
                    _roomSetCustomProperties.Invoke(room, new object[] { ht });
                else if (prms.Length >= 2)
                    _roomSetCustomProperties.Invoke(room, new object[] { ht, null });

                Log.Info("MultiplayerGate: set room property '" + PropertyKey + "' = " + value);
                UpdateState(true, value);
                return true;
            }
            catch (Exception e)
            {
                Log.Warn("MultiplayerGate: SetRoomProperty failed: " + e.Message);
                return false;
            }
        }

        private static bool ClearRoomProperty()
        {
            try
            {
                var room = GetRoom();
                if (room == null) return false;
                if (_roomSetCustomProperties == null || _hashtableType == null) return false;

                // Set the key to null to remove it (Photon convention).
                object ht = Activator.CreateInstance(_hashtableType);
                if (_hashtableAdd != null)
                    _hashtableAdd.Invoke(ht, new object[] { (object)PropertyKey, null });

                var prms = _roomSetCustomProperties.GetParameters();
                if (prms.Length == 1)
                    _roomSetCustomProperties.Invoke(room, new object[] { ht });
                else if (prms.Length >= 2)
                    _roomSetCustomProperties.Invoke(room, new object[] { ht, null });

                Log.Info("MultiplayerGate: cleared room property '" + PropertyKey + "'");
                UpdateState(false, null);
                return true;
            }
            catch (Exception e)
            {
                Log.Warn("MultiplayerGate: ClearRoomProperty failed: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns a JSON summary suitable for the <c>/api/multiplayer</c>
        /// web-UI endpoint.
        /// </summary>
        public static string ToJson()
        {
            bool mp = IsMultiplayer;
            bool host = mp && IsHost;
            bool req = IsRequiredInCurrentRoom;
            string ver = RoomModVersion ?? "";
            return "{\"inRoom\":" + (mp ? "true" : "false") +
                   ",\"isHost\":" + (host ? "true" : "false") +
                   ",\"requiresMod\":" + (req ? "true" : "false") +
                   ",\"roomModVersion\":\"" + EscapeJson(ver) + "\"}";
        }

        private static string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
