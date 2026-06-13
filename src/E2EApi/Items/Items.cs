using System.Collections.Generic;

namespace E2EApi.Items
{
    /// <summary>Item registry and inventory operations, wrapping <c>ItemManager</c>.</summary>
    public static class Items
    {
        private static ItemManager Manager => ItemManager.GetInstance();

        /// <summary>Look up an item definition by its data ID, or null.</summary>
        public static ItemData GetData(int itemDataId)
        {
            var mgr = Manager;
            return mgr != null ? mgr.GetItemDataWithID(itemDataId) : null;
        }

        /// <summary>Items allowed in the current level (empty outside a level).</summary>
        public static List<ItemData> Allowed
        {
            get
            {
                var mgr = Manager;
                return mgr != null ? mgr.GetAllowedList() : new List<ItemData>();
            }
        }

        /// <summary>Key items for the current level.</summary>
        public static List<ItemData> KeyItems
        {
            get
            {
                var mgr = Manager;
                return mgr != null ? mgr.GetKeyList() : new List<ItemData>();
            }
        }

        /// <summary>
        /// Give an item to a player's inventory (network-aware; routes through
        /// the game's item assignment RPC).
        /// </summary>
        public static bool Give(Players.Player player, int itemDataId)
        {
            var mgr = Manager;
            if (mgr == null || player == null || !player.IsValid)
            {
                return false;
            }
            int requestId = -1;
            mgr.AssignItemRPC(player.Pawn.m_NetView.ownerId, itemDataId, null, ref requestId);
            return requestId != -1;
        }
    }
}
