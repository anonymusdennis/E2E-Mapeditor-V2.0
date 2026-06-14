using System.Collections.Generic;
using UnityEngine;

namespace E2EApi.Players
{
    /// <summary>
    /// Player wrapper around the game's <c>Gamer</c> / <c>Player</c> pair.
    /// Salvaged behaviours from the old MelonLoader experiments (heal,
    /// stealth/heat clearing) live here as proper API calls.
    /// </summary>
    public class Player
    {
        private readonly Gamer _gamer;

        private Player(Gamer gamer)
        {
            _gamer = gamer;
        }

        /// <summary>The primary local player, or null outside a session.</summary>
        public static Player GetLocal()
        {
            var gamer = Gamer.GetPrimaryGamer();
            return gamer != null ? new Player(gamer) : null;
        }

        /// <summary>All local players (couch co-op).</summary>
        public static List<Player> GetAllLocal() => Wrap(Gamer.GetLocalGamers());

        /// <summary>Every player in the session, local and remote.</summary>
        public static List<Player> GetAll() => Wrap(Gamer.GetAllGamers());

        private static List<Player> Wrap(Gamer[] gamers)
        {
            var result = new List<Player>();
            if (gamers == null)
            {
                return result;
            }
            foreach (var gamer in gamers)
            {
                if (gamer != null)
                {
                    result.Add(new Player(gamer));
                }
            }
            return result;
        }

        /// <summary>The underlying game objects, for API consumers that need to go deeper.</summary>
        public Gamer Gamer => _gamer;
        public global::Player Pawn => _gamer != null ? _gamer.m_PlayerObject : null;

        private CharacterStats Stats => Pawn != null ? Pawn.m_CharacterStats : null;

        /// <summary>True while this player has a live pawn in a level.</summary>
        public bool IsValid => Stats != null;

        public string Name => _gamer != null ? _gamer.m_GamerName : null;

        public float Health
        {
            get => IsValid ? Stats.Health : 0f;
            set { if (IsValid) Stats.SetHealth(value); }
        }

        public float Energy
        {
            get => IsValid ? Stats.Energy : 0f;
            set { if (IsValid) Stats.SetEnergy(value); }
        }

        public float Money
        {
            get => IsValid ? Stats.Money : 0f;
            set { if (IsValid) Stats.Money = value; }
        }

        /// <summary>Players never lose energy while this is on (game dev flag).</summary>
        public static bool InfiniteEnergy
        {
            get => CharacterStats.m_bInfinitePlayerEnergyOn;
            set => CharacterStats.m_bInfinitePlayerEnergyOn = value;
        }

        public float Heat
        {
            get => IsValid ? Stats.Heat : 0f;
            set { if (IsValid) Stats.SetHeat(value); }
        }

        /// <summary>Restore health to full (100).</summary>
        public void Heal()
        {
            if (IsValid)
            {
                Stats.SetHealth(100f);
            }
        }

        /// <summary>The player's tile position and floor, or false without a pawn.</summary>
        public bool GetTile(out int x, out int y, out int floor)
        {
            x = y = floor = -1;
            var pawn = Pawn;
            if (pawn == null)
            {
                return false;
            }
            // In play mode the Rotorz tile system has row 0 at the TOP of the map
            // (highest world y).  Use the game's own converter and then flip the row
            // to our y convention (y=0 at the southernmost tile).
            var floors = FloorManager.GetInstance();
            if (floors != null && pawn.CurrentFloor != null)
            {
                int row, column;
                if (floors.GetTileGridPoint(pawn.CurrentFloor,
                        FloorManager.TileSystem_Type.TileSystem_Ground,
                        pawn.transform.position, out row, out column))
                {
                    x = column;
                    y = (Editor.Grid.OriginY + Editor.Grid.Height - 1) - row;
                    floor = pawn.CurrentFloor.m_FloorIndex;
                    return true;
                }
            }
            floor = pawn.CurrentFloor != null ? pawn.CurrentFloor.m_FloorIndex : 0;
            return Editor.Grid.WorldToTile(pawn.transform.position, out x, out y);
        }

        /// <summary>
        /// The player's tile position, physical floor index, and virtual layer index.
        /// <paramref name="virtualLayer"/> is -1 when no virtual layer mapping exists
        /// (vanilla map or no registry entries).
        /// </summary>
        public bool GetTile(out int x, out int y, out int floor, out int virtualLayer)
        {
            virtualLayer = -1;
            if (!GetTile(out x, out y, out floor)) return false;
            var pawn = Pawn;
            if (pawn != null && Features.FloorTypeRegistry.HasEntries)
            {
                // Prefer the character's explicit VirtualFloorState (set on teleport /
                // floor-change) over the registry's first-match for this physical floor.
                Features.VirtualFloorState vfs;
                if (Features.VirtualFloorState.TryGet(pawn, out vfs) &&
                    vfs.PhysicalFloor != null &&
                    vfs.PhysicalFloor.m_FloorIndex == floor)
                {
                    virtualLayer = vfs.VirtualIndex;
                }
                else
                {
                    var physFloor = FloorManager.GetInstance()?.FindFloorbyIndex(floor);
                    if (physFloor != null)
                        virtualLayer = Features.FloorTypeRegistry.GetVirtualIndex(physFloor);
                }
            }
            return true;
        }

        /// <summary>
        /// Instantly move the pawn to a tile on a floor (default: stay on the
        /// current floor). Uses the game's own <c>Character.Teleport</c>, so
        /// networking, cached positions and floor state all stay consistent.
        /// </summary>
        public bool TeleportToTile(int x, int y, int floor = -1)
        {
            return TeleportToTile(x, y, floor, virtualLayer: -1);
        }

        /// <summary>
        /// Instantly move the pawn to a tile, targeting a specific virtual layer
        /// (which resolves to its backing physical floor).  When
        /// <paramref name="virtualLayer"/> is non-negative it takes precedence over
        /// <paramref name="floor"/>.  <see cref="Features.VirtualFloorState"/> is
        /// updated on success so Z-lookup disambiguation works immediately.
        /// </summary>
        public bool TeleportToTile(int x, int y, int floor, int virtualLayer)
        {
            var pawn = Pawn;
            if (pawn == null)
            {
                return false;
            }
            Vector3? world = Editor.Grid.TileToWorld(x, y);
            if (world == null)
            {
                return false;
            }
            FloorManager.Floor target = pawn.CurrentFloor;
            var floors = FloorManager.GetInstance();

            // Resolve floor from virtual layer index if provided.
            if (virtualLayer >= 0 && floors != null &&
                Features.FloorTypeRegistry.HasEntries)
            {
                int backing = Features.MapGeometry.GetBackingLayer(virtualLayer);
                target = floors.FindFloorbyIndex(backing) ?? target;
            }
            else if (floor >= 0 && floors != null)
            {
                target = floors.FindFloorbyIndex(floor) ?? target;
            }

            bool ok = pawn.Teleport(new Vector3(world.Value.x, world.Value.y, world.Value.z), target);

            // Write VirtualFloorState so Z-lookup disambiguation is immediately
            // accurate for the virtual layer we aimed at.
            if (ok && virtualLayer >= 0 && target != null)
                Features.VirtualFloorState.Set(pawn, virtualLayer, target);

            return ok;
        }

        /// <summary>
        /// Drop all suspicion: zero heat, clear wanted flags, mark as disguised.
        /// (the old "stealth mode" experiment)
        /// </summary>
        public void ClearSuspicion()
        {
            if (!IsValid)
            {
                return;
            }
            Stats.Heat = 0f;
            Pawn.IsPreparingToBeCarried = false;
            Pawn.SetIsWanted(false);
            Pawn.SetIsWantedForSolitary(false);
            Pawn.SetIsDisguised(true);
        }
    }
}
