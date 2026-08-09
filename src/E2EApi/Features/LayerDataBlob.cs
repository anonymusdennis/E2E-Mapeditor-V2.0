using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace E2EApi.Features
{
    /// <summary>
    /// Compact Base64 encoding of a <see cref="BaseLevelManager.LayerDataCollection"/>
    /// tile/wall/object ID arrays plus tile/room property masks. GameObject arrays
    /// are omitted — they are null after decode and re-spawned by the game/editor.
    /// </summary>
    public static class LayerDataBlob
    {
        private const int CellCount = BaseLevelManager.c_LayerTileCount; // 14400
        private static readonly byte[] Magic = Encoding.ASCII.GetBytes("LDB1");

        private static readonly int EmptyTileId =
            (int)(BaseLevelManager.TileIDData.IDMask | BaseLevelManager.TileIDData.VariantMask);
        private static readonly int EmptyWallId =
            (int)(BaseLevelManager.TileIDData.IDMask | BaseLevelManager.TileIDData.VariantMask);
        private static readonly int EmptyObjectId =
            (int)BaseLevelManager.TileIDData.IDMask;

        /// <summary>
        /// Encode non-empty cells of <paramref name="layer"/> to a sparse LDB1 Base64 blob.
        /// Returns null when the layer has no content.
        /// </summary>
        public static string Encode(BaseLevelManager.LayerDataCollection layer)
        {
            if (layer == null || !HasArrays(layer))
            {
                return null;
            }

            var cells = new List<int>();
            int n = Math.Min(CellCount, layer.m_TileProperties.Length);
            for (int i = 0; i < n; i++)
            {
                if (!IsEmptyCell(layer, i))
                {
                    cells.Add(i);
                }
            }

            if (cells.Count == 0)
            {
                return null;
            }

            using (var ms = new MemoryStream(8 + cells.Count * 40))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Magic);
                w.Write(cells.Count);
                for (int c = 0; c < cells.Count; c++)
                {
                    int i = cells[c];
                    w.Write(i);
                    w.Write((int)layer.m_TileProperties[i]);
                    w.Write((int)layer.m_TileTileIDs[i]);
                    w.Write((int)layer.m_WallTileIDs[i]);
                    w.Write((int)layer.m_ObjectTileIDs[i]);
                    w.Write((int)layer.m_RoomPropertiesMasks[i]);
                    w.Write(layer.m_RoomIDs[i]);
                    w.Write(layer.m_RoomIDs[i + CellCount]);
                    w.Write(layer.m_RoomIDs[i + CellCount * 2]);
                    w.Write(layer.m_RoomIDs[i + CellCount * 3]);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Decode a sparse LDB1 Base64 blob into <paramref name="layer"/>, resetting
        /// data arrays to empty defaults first. GameObject arrays are left alone
        /// (or allocated null by <see cref="BaseLevelManager.LayerDataCollection.Init"/>).
        /// </summary>
        public static void Decode(BaseLevelManager.LayerDataCollection layer, string base64)
        {
            if (layer == null || string.IsNullOrEmpty(base64))
            {
                return;
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch (Exception ex)
            {
                Log.Warn("LayerDataBlob.Decode: bad base64: " + ex.Message);
                return;
            }

            if (bytes.Length < 8)
            {
                Log.Warn("LayerDataBlob.Decode: truncated blob");
                return;
            }

            using (var ms = new MemoryStream(bytes))
            using (var r = new BinaryReader(ms))
            {
                var magic = r.ReadBytes(4);
                if (magic.Length != 4 ||
                    magic[0] != Magic[0] || magic[1] != Magic[1] ||
                    magic[2] != Magic[2] || magic[3] != Magic[3])
                {
                    Log.Warn("LayerDataBlob.Decode: bad magic");
                    return;
                }

                int sparseCount = r.ReadInt32();
                if (sparseCount < 0 || sparseCount > CellCount)
                {
                    Log.Warn("LayerDataBlob.Decode: bad sparseCount " + sparseCount);
                    return;
                }

                // Ensure capacity for the full grid; wipe to empty defaults.
                if (!HasArrays(layer) ||
                    layer.m_TileProperties.Length != CellCount ||
                    layer.m_RoomIDs.Length != CellCount * 4)
                {
                    layer.Init();
                }
                else
                {
                    ClearToEmpty(layer);
                }

                long needed = (long)sparseCount * 40;
                if (ms.Length - ms.Position < needed)
                {
                    Log.Warn("LayerDataBlob.Decode: truncated cell data");
                    return;
                }

                for (int c = 0; c < sparseCount; c++)
                {
                    int i = r.ReadInt32();
                    int tileProp = r.ReadInt32();
                    int tileId = r.ReadInt32();
                    int wallId = r.ReadInt32();
                    int objId = r.ReadInt32();
                    int roomMask = r.ReadInt32();
                    int r0 = r.ReadInt32();
                    int r1 = r.ReadInt32();
                    int r2 = r.ReadInt32();
                    int r3 = r.ReadInt32();

                    if (i < 0 || i >= CellCount)
                    {
                        continue;
                    }

                    layer.m_TileProperties[i] = (BaseLevelManager.TileProperty)tileProp;
                    layer.m_TileTileIDs[i] = (BaseLevelManager.TileIDData)tileId;
                    layer.m_WallTileIDs[i] = (BaseLevelManager.TileIDData)wallId;
                    layer.m_ObjectTileIDs[i] = (BaseLevelManager.TileIDData)objId;
                    layer.m_RoomPropertiesMasks[i] = (BaseLevelManager.RoomProperty)roomMask;
                    layer.m_RoomIDs[i] = r0;
                    layer.m_RoomIDs[i + CellCount] = r1;
                    layer.m_RoomIDs[i + CellCount * 2] = r2;
                    layer.m_RoomIDs[i + CellCount * 3] = r3;
                }

                layer.m_Changed = true;
            }
        }

        /// <summary>True when any cell differs from the Init() empty defaults.</summary>
        public static bool HasContent(BaseLevelManager.LayerDataCollection layer)
        {
            if (layer == null || !HasArrays(layer))
            {
                return false;
            }
            int n = Math.Min(CellCount, layer.m_TileProperties.Length);
            for (int i = 0; i < n; i++)
            {
                if (!IsEmptyCell(layer, i))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasArrays(BaseLevelManager.LayerDataCollection layer)
        {
            return layer.m_TileProperties != null &&
                   layer.m_TileTileIDs != null &&
                   layer.m_WallTileIDs != null &&
                   layer.m_ObjectTileIDs != null &&
                   layer.m_RoomPropertiesMasks != null &&
                   layer.m_RoomIDs != null &&
                   layer.m_TileProperties.Length > 0 &&
                   layer.m_RoomIDs.Length >= layer.m_TileProperties.Length * 4;
        }

        private static bool IsEmptyCell(BaseLevelManager.LayerDataCollection layer, int i)
        {
            if (layer.m_TileProperties[i] != BaseLevelManager.TileProperty.EMPTY)
            {
                return false;
            }
            if (layer.m_RoomPropertiesMasks[i] != BaseLevelManager.RoomProperty.EMPTY)
            {
                return false;
            }
            if ((int)layer.m_TileTileIDs[i] != EmptyTileId)
            {
                return false;
            }
            if ((int)layer.m_WallTileIDs[i] != EmptyWallId)
            {
                return false;
            }
            if ((int)layer.m_ObjectTileIDs[i] != EmptyObjectId)
            {
                return false;
            }
            if (layer.m_RoomIDs[i] != 0 ||
                layer.m_RoomIDs[i + CellCount] != 0 ||
                layer.m_RoomIDs[i + CellCount * 2] != 0 ||
                layer.m_RoomIDs[i + CellCount * 3] != 0)
            {
                return false;
            }
            return true;
        }

        private static void ClearToEmpty(BaseLevelManager.LayerDataCollection layer)
        {
            for (int num = CellCount - 1; num >= 0; num--)
            {
                layer.m_TileProperties[num] = BaseLevelManager.TileProperty.EMPTY;
                layer.m_RoomPropertiesMasks[num] = BaseLevelManager.RoomProperty.EMPTY;
                layer.m_RoomIDs[num] = 0;
                layer.m_RoomIDs[num + CellCount] = 0;
                layer.m_RoomIDs[num + CellCount * 2] = 0;
                layer.m_RoomIDs[num + CellCount * 3] = 0;
                layer.m_TileTileIDs[num] = (BaseLevelManager.TileIDData)EmptyTileId;
                layer.m_WallTileIDs[num] = (BaseLevelManager.TileIDData)EmptyWallId;
                layer.m_ObjectTileIDs[num] = (BaseLevelManager.TileIDData)EmptyObjectId;
            }
            layer.m_Changed = false;
        }
    }
}
