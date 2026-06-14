using System;
using System.Collections.Generic;
using Rotorz.Tile;

namespace Custom;

public static class TileCompositionUtility
{
	public static void EraseGroup(this TileSystem system, TileIndex index, TileComposition composition)
	{
		if (system == null)
		{
			throw new ArgumentNullException("system");
		}
		int num;
		int num2;
		if (composition != null)
		{
			num = composition.Rows;
			num2 = composition.Columns;
		}
		else
		{
			num = 1;
			num2 = 1;
		}
		CompositeTileMap component = system.GetComponent<CompositeTileMap>();
		try
		{
			system.BeginBulkEdit();
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					TileIndex index2 = new TileIndex(index.row + i, index.column + j);
					if (!system.InBounds(index2))
					{
						continue;
					}
					if (component != null)
					{
						List<int> groupedTileIndices = component.GetGroupedTileIndices(index2);
						if (groupedTileIndices != null)
						{
							foreach (int item in groupedTileIndices)
							{
								TileIndex index3 = component.TileIndexFromFlatIndex(item);
								if (system.InBounds(index3))
								{
									system.EraseTile(index3);
								}
							}
							component.UngroupTiles(index2);
						}
					}
					system.EraseTile(index2);
				}
			}
		}
		finally
		{
			system.EndBulkEdit();
		}
	}

	public static void EraseGroup(this TileSystem system, TileIndex index)
	{
		system.EraseGroup(index, null);
	}

	public static void PaintComposition(this TileSystem system, TileIndex index, TileComposition composition)
	{
		if (system == null)
		{
			throw new ArgumentNullException("system");
		}
		if (composition == null)
		{
			throw new ArgumentNullException("composition");
		}
		int num = composition.Rows * composition.Columns;
		if (num == 0)
		{
			return;
		}
		CompositeTileMap compositeTileMap = system.gameObject.GetComponent<CompositeTileMap>();
		if (compositeTileMap == null)
		{
			compositeTileMap = system.gameObject.AddComponent<CompositeTileMap>();
		}
		TileIndex index2 = system.FirstTileIndexInBounds(index, composition);
		try
		{
			system.BeginBulkEdit();
			system.EraseGroup(index, composition);
			for (int i = 0; i < composition.Rows; i++)
			{
				for (int j = 0; j < composition.Columns; j++)
				{
					TileIndex tileIndex = new TileIndex(index.row + i, index.column + j);
					if (system.InBounds(tileIndex))
					{
						if (num > 1)
						{
							compositeTileMap.GroupTiles(index2, tileIndex);
						}
						TileData tileData = composition[i, j];
						if (tileData != null)
						{
							system.SetTileFrom(index.row + i, index.column + j, tileData);
							system.RefreshTile(index.row + i, index.column + j);
						}
						else
						{
							system.EraseTile(index.row + i, index.column + j);
						}
					}
				}
			}
		}
		finally
		{
			system.EndBulkEdit();
		}
	}

	private static TileIndex FirstTileIndexInBounds(this TileSystem system, TileIndex index, TileComposition composition)
	{
		for (int i = 0; i < composition.Rows; i++)
		{
			for (int j = 0; j < composition.Columns; j++)
			{
				TileIndex tileIndex = new TileIndex(index.row + i, index.column + j);
				if (system.InBounds(tileIndex))
				{
					return tileIndex;
				}
			}
		}
		return TileIndex.invalid;
	}
}
