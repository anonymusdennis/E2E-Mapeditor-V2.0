using System.Runtime.CompilerServices;

namespace E2EApi.Features
{
    /// <summary>
    /// Tracks the virtual layer index for a <c>Character</c> instance independently
    /// of the physical <c>FloorManager.Floor</c>. This allows disambiguation when
    /// two virtual layers share the same backing native floor.
    ///
    /// Phase 7 populates this via <c>Character.Teleport</c> patches and
    /// <c>Character.OnFloorChanged</c> hooks. For Sprint 1 it is created here as
    /// the backing store; patches that write it are added in subsequent sprints.
    /// </summary>
    public sealed class VirtualFloorState
    {
        /// <summary>The virtual layer index (position in MapGeometry.Current.Layers).</summary>
        public int VirtualIndex;

        /// <summary>The physical floor that was active when VirtualIndex was last set.</summary>
        public FloorManager.Floor PhysicalFloor;

        private static readonly ConditionalWeakTable<Character, VirtualFloorState> _table =
            new ConditionalWeakTable<Character, VirtualFloorState>();

        /// <summary>Gets or creates the <see cref="VirtualFloorState"/> for <paramref name="character"/>.</summary>
        public static VirtualFloorState GetOrCreate(Character character)
        {
            return _table.GetOrCreateValue(character);
        }

        /// <summary>
        /// Tries to get an existing <see cref="VirtualFloorState"/> for
        /// <paramref name="character"/>. Returns <c>false</c> if none has been set.
        /// </summary>
        public static bool TryGet(Character character, out VirtualFloorState state)
        {
            return _table.TryGetValue(character, out state);
        }

        /// <summary>
        /// Updates the virtual floor state for <paramref name="character"/>.
        /// </summary>
        public static void Set(Character character, int virtualIndex, FloorManager.Floor physicalFloor)
        {
            var state = _table.GetOrCreateValue(character);
            state.VirtualIndex = virtualIndex;
            state.PhysicalFloor = physicalFloor;
        }
    }
}
