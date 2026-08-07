using System.Collections.Generic;
using E2EApi.Persistence;

namespace E2EApi.Features
{
    /// <summary>
    /// Button/trigger links: a source tile ("button") linked to a target tile.
    /// Actions: toggle an electric fence, or teleport the activating player to
    /// the target tile on any virtual map layer (stairs/ladders between custom
    /// floors). Persisted in the sidecar (section [triggers], lines
    /// "srcX,srcY,dstX,dstY,action[,srcLayer,dstLayer]").
    /// </summary>
    public static class Triggers
    {
        public enum TriggerAction
        {
            ToggleFence = 0,
            Teleport = 1,
        }

        public class Link
        {
            public int SourceX;
            public int SourceY;
            public int TargetX;
            public int TargetY;
            public TriggerAction Action;
            /// <summary>Virtual layer of the source tile; -1 = any layer (legacy links).</summary>
            public int SourceLayer = -1;
            /// <summary>Virtual layer the target tile lives on; -1 = keep current floor.</summary>
            public int TargetLayer = -1;
        }

        private const string SectionName = "triggers";
        private static readonly List<Link> Links = new List<Link>();
        private static bool _initialised;

        // Auto-teleport re-trigger guard: pawn instance id -> tile the pawn must
        // leave before another teleport may fire.
        private static readonly Dictionary<int, long> _teleportHold = new Dictionary<int, long>();

        /// <summary>Bumped on every change; overlays use it to know when to rebuild.</summary>
        public static int Version { get; private set; }

        public static void Initialise()
        {
            if (_initialised)
            {
                return;
            }
            _initialised = true;
            ModExtras.EnsurePatched();
            ModExtras.Saving += OnSaving;
            ModExtras.Loaded += OnLoaded;
        }

        public static IList<Link> All => Links;

        public static Link AddLink(int srcX, int srcY, int dstX, int dstY,
            TriggerAction action = TriggerAction.ToggleFence,
            int srcLayer = -1, int dstLayer = -1)
        {
            Initialise();
            var link = new Link
            {
                SourceX = srcX, SourceY = srcY,
                TargetX = dstX, TargetY = dstY,
                Action = action,
                SourceLayer = srcLayer,
                TargetLayer = dstLayer,
            };
            Links.Add(link);
            Version++;
            return link;
        }

        /// <summary>Re-insert a previously removed link (undo support).</summary>
        public static void Restore(Link link)
        {
            if (link == null || Links.Contains(link))
            {
                return;
            }
            Links.Add(link);
            Version++;
        }

        /// <summary>Remove one specific link instance (undo support).</summary>
        public static bool Remove(Link link)
        {
            if (Links.Remove(link))
            {
                Version++;
                return true;
            }
            return false;
        }

        public static int RemoveLinksAt(int x, int y)
        {
            int removed = Links.RemoveAll(l => l.SourceX == x && l.SourceY == y);
            if (removed > 0)
            {
                Version++;
            }
            return removed;
        }

        /// <summary>Remove links sourced at a tile, returning them for undo.</summary>
        public static List<Link> TakeLinksAt(int x, int y)
        {
            var taken = new List<Link>();
            for (int i = Links.Count - 1; i >= 0; i--)
            {
                if (Links[i].SourceX == x && Links[i].SourceY == y)
                {
                    taken.Add(Links[i]);
                    Links.RemoveAt(i);
                }
            }
            if (taken.Count > 0)
            {
                Version++;
            }
            return taken;
        }

        public static void Clear()
        {
            Links.Clear();
            _teleportHold.Clear();
            Version++;
        }

        /// <summary>
        /// Activate every link whose button is at the given tile (e.g. because a
        /// player standing there pressed the interact key). Returns how many fired.
        /// <paramref name="player"/> receives teleport actions; when null those
        /// links are skipped. <paramref name="layer"/> filters layer-bound links
        /// (-1 matches any).
        /// </summary>
        public static int ActivateAt(int x, int y, Players.Player player = null, int layer = -1)
        {
            int fired = 0;
            // Iterate over a copy: a teleport can re-enter via movement callbacks.
            var snapshot = Links.ToArray();
            foreach (var link in snapshot)
            {
                if (link.SourceX != x || link.SourceY != y)
                {
                    continue;
                }
                if (link.SourceLayer >= 0 && layer >= 0 && link.SourceLayer != layer)
                {
                    continue;
                }
                if (Execute(link, player))
                {
                    fired++;
                }
            }
            return fired;
        }

        /// <summary>Activate links under a player's feet.</summary>
        public static int ActivateUnder(Players.Player player)
        {
            if (player == null || !player.IsValid)
            {
                return 0;
            }
            int x, y, floor, virtualLayer;
            if (!player.GetTile(out x, out y, out floor, out virtualLayer))
            {
                return 0;
            }
            return ActivateAt(x, y, player, virtualLayer);
        }

        private static bool Execute(Link link, Players.Player player)
        {
            switch (link.Action)
            {
                case TriggerAction.ToggleFence:
                    bool on = ElectricFences.Toggle(link.TargetX, link.TargetY);
                    Log.Info($"trigger ({link.SourceX},{link.SourceY}) → fence ({link.TargetX},{link.TargetY}) {(on ? "ON" : "off")}");
                    return true;
                case TriggerAction.Teleport:
                    if (player == null || !player.IsValid)
                    {
                        return false;
                    }
                    bool ok = player.TeleportToTile(link.TargetX, link.TargetY,
                        floor: -1, virtualLayer: link.TargetLayer);
                    if (ok)
                    {
                        HoldTeleport(player, link.TargetX, link.TargetY);
                        Log.Info($"trigger ({link.SourceX},{link.SourceY}) → teleport ({link.TargetX},{link.TargetY}) layer {link.TargetLayer}");
                    }
                    return ok;
            }
            return false;
        }

        /// <summary>
        /// Per-frame play-mode tick: players stepping onto a teleport source tile
        /// are moved automatically (stairs between custom floors). A player must
        /// leave the arrival/source tile before another teleport can fire, which
        /// prevents ping-pong loops between two linked staircases.
        /// </summary>
        public static void Tick()
        {
            if (Links.Count == 0 ||
                Events.GameEvents.IsEditorActive ||
                BaseLevelManager.GetInstance() == null)
            {
                return;
            }

            bool anyTeleport = false;
            foreach (var link in Links)
            {
                if (link.Action == TriggerAction.Teleport)
                {
                    anyTeleport = true;
                    break;
                }
            }
            if (!anyTeleport)
            {
                return;
            }

            foreach (var player in Players.Player.GetAllLocal())
            {
                TickPlayer(player);
            }
        }

        private static void TickPlayer(Players.Player player)
        {
            if (player == null || !player.IsValid || player.Pawn == null)
            {
                return;
            }
            int x, y, floor, virtualLayer;
            if (!player.GetTile(out x, out y, out floor, out virtualLayer))
            {
                return;
            }

            int pawnId = player.Pawn.GetInstanceID();
            long tileKey = PackTile(x, y);
            long held;
            if (_teleportHold.TryGetValue(pawnId, out held))
            {
                if (held == tileKey)
                {
                    return; // still standing where we arrived / last fired
                }
                _teleportHold.Remove(pawnId);
            }

            foreach (var link in Links)
            {
                if (link.Action != TriggerAction.Teleport ||
                    link.SourceX != x || link.SourceY != y)
                {
                    continue;
                }
                if (link.SourceLayer >= 0 && virtualLayer >= 0 &&
                    link.SourceLayer != virtualLayer)
                {
                    continue;
                }
                if (Execute(link, player))
                {
                    return; // one teleport per tick per player
                }
            }
        }

        private static void HoldTeleport(Players.Player player, int x, int y)
        {
            if (player == null || player.Pawn == null)
            {
                return;
            }
            _teleportHold[player.Pawn.GetInstanceID()] = PackTile(x, y);
        }

        private static long PackTile(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }

        private static void OnSaving(ModExtras extras)
        {
            extras.ClearSection(SectionName);
            if (Links.Count == 0)
            {
                return;
            }
            var section = extras.Section(SectionName);
            foreach (var link in Links)
            {
                section.Add($"{link.SourceX},{link.SourceY},{link.TargetX},{link.TargetY},{(int)link.Action},{link.SourceLayer},{link.TargetLayer}");
            }
            extras.RequiresMod = true;
        }

        private static void OnLoaded(ModExtras extras)
        {
            Links.Clear();
            _teleportHold.Clear();
            foreach (var line in extras.Section(SectionName))
            {
                string[] parts = line.Split(',');
                if (parts.Length < 5)
                {
                    continue;
                }
                int sx, sy, dx, dy, action;
                if (int.TryParse(parts[0], out sx) && int.TryParse(parts[1], out sy) &&
                    int.TryParse(parts[2], out dx) && int.TryParse(parts[3], out dy) &&
                    int.TryParse(parts[4], out action))
                {
                    var link = new Link
                    {
                        SourceX = sx, SourceY = sy,
                        TargetX = dx, TargetY = dy,
                        Action = (TriggerAction)action,
                    };
                    int srcLayer, dstLayer;
                    if (parts.Length >= 7 &&
                        int.TryParse(parts[5], out srcLayer) &&
                        int.TryParse(parts[6], out dstLayer))
                    {
                        link.SourceLayer = srcLayer;
                        link.TargetLayer = dstLayer;
                    }
                    Links.Add(link);
                }
            }
            Version++;
            if (Links.Count > 0)
            {
                Log.Info($"triggers: {Links.Count} link(s) active");
            }
        }
    }
}
