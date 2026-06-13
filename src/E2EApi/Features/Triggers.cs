using System.Collections.Generic;
using E2EApi.Editor;
using E2EApi.Persistence;

namespace E2EApi.Features
{
    /// <summary>
    /// Button/trigger links: a source tile ("button") linked to a target tile.
    /// Activating the button toggles the target (currently: electric fence
    /// on/off; the action set will grow). Persisted in the sidecar
    /// (section [triggers], lines "srcX,srcY,dstX,dstY,action").
    /// </summary>
    public static class Triggers
    {
        public enum TriggerAction
        {
            ToggleFence = 0,
        }

        public class Link
        {
            public int SourceX;
            public int SourceY;
            public int TargetX;
            public int TargetY;
            public TriggerAction Action;
        }

        private const string SectionName = "triggers";
        private static readonly List<Link> Links = new List<Link>();
        private static bool _initialised;

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

        public static void AddLink(int srcX, int srcY, int dstX, int dstY,
            TriggerAction action = TriggerAction.ToggleFence)
        {
            Initialise();
            Links.Add(new Link
            {
                SourceX = srcX, SourceY = srcY,
                TargetX = dstX, TargetY = dstY,
                Action = action,
            });
            Version++;
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

        public static void Clear()
        {
            Links.Clear();
            Version++;
        }

        /// <summary>
        /// Activate every link whose button is at the given tile (e.g. because a
        /// player standing there pressed the interact key). Returns how many fired.
        /// </summary>
        public static int ActivateAt(int x, int y)
        {
            int fired = 0;
            foreach (var link in Links)
            {
                if (link.SourceX != x || link.SourceY != y)
                {
                    continue;
                }
                Execute(link);
                fired++;
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
            int x, y;
            if (!Grid.WorldToTile(player.Pawn.transform.position, out x, out y))
            {
                return 0;
            }
            return ActivateAt(x, y);
        }

        private static void Execute(Link link)
        {
            switch (link.Action)
            {
                case TriggerAction.ToggleFence:
                    bool on = ElectricFences.Toggle(link.TargetX, link.TargetY);
                    Log.Info($"trigger ({link.SourceX},{link.SourceY}) → fence ({link.TargetX},{link.TargetY}) {(on ? "ON" : "off")}");
                    break;
            }
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
                section.Add($"{link.SourceX},{link.SourceY},{link.TargetX},{link.TargetY},{(int)link.Action}");
            }
            extras.RequiresMod = true;
        }

        private static void OnLoaded(ModExtras extras)
        {
            Links.Clear();
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
                    Links.Add(new Link
                    {
                        SourceX = sx, SourceY = sy,
                        TargetX = dx, TargetY = dy,
                        Action = (TriggerAction)action,
                    });
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
