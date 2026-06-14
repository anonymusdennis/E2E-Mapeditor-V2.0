using Rotorz.Tile;
using UnityEngine;

public class LevelSetup_MapTextures : BaseComponentSetup
{
	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_10_Last;
	}

	public override SetupReturnState Setup()
	{
		if (BaseComponentSetup.m_FloorManager == null && !GetFloorManager())
		{
			return FinishedAndRemove();
		}
		FloorManager.Floor[] floors = BaseComponentSetup.m_FloorManager.GetFloors();
		int num = 8;
		for (int i = 0; i < floors.Length; i++)
		{
			TileSystem[] tileSystems = floors[i].m_TileSystems;
			int num2 = tileSystems[0].ColumnCount * num;
			int num3 = tileSystems[0].RowCount * num;
			Texture2D tex = new Texture2D(num2 + 2, num3 + 2, TextureFormat.ARGB32, mipmap: false);
			tex.anisoLevel = 0;
			tex.filterMode = FilterMode.Point;
			tex.wrapMode = TextureWrapMode.Clamp;
			for (int j = 0; j < num2 + 2; j++)
			{
				for (int k = 0; k < num3 + 2; k++)
				{
					tex.SetPixel(j, k, Color.clear);
				}
			}
			for (int l = 0; l < tileSystems.Length; l++)
			{
				if (tileSystems[l] != null && (tileSystems[l].gameObject.CompareTag("GroundTiles") || tileSystems[l].gameObject.CompareTag("WallTiles")))
				{
					DrawOnMap(tileSystems[l], ref tex, num);
				}
			}
			DrawSpecialBitsOnMap(floors[i].m_FloorRootObject.gameObject, ref tex, num, tileSystems[0].ColumnCount, tileSystems[0].RowCount);
			MapTextureInfo mapTextureInfo = floors[i].m_FloorRootObject.GetComponent<MapTextureInfo>();
			if (mapTextureInfo == null)
			{
				mapTextureInfo = floors[i].m_FloorRootObject.gameObject.AddComponent<MapTextureInfo>();
			}
			mapTextureInfo.m_MapTexturePath = "Generated Map for Custom Level";
			mapTextureInfo.m_MapTexture = tex;
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	private void DrawOnMap(TileSystem tileSystem, ref Texture2D tex, int tileSize)
	{
		for (int i = 0; i < tileSystem.ColumnCount; i++)
		{
			for (int j = 0; j < tileSystem.RowCount; j++)
			{
				TileIndex index = new TileIndex(j, i);
				TileData tileOrNull = tileSystem.GetTileOrNull(index);
				if (tileOrNull == null || !(tileOrNull.gameObject != null))
				{
					continue;
				}
				TextureStamp component = tileOrNull.gameObject.GetComponent<TextureStamp>();
				if (component != null)
				{
					Texture2D stampTexture = component.GetStampTexture();
					if (stampTexture != null)
					{
						int num = Mathf.RoundToInt(i * tileSize);
						int num2 = Mathf.RoundToInt((tileSystem.RowCount - 1 - j) * tileSize);
						tex.SetPixels(num + 1, num2 + 1, stampTexture.width, stampTexture.height, stampTexture.GetPixels());
					}
				}
			}
		}
		tex.Apply();
	}

	private void DrawSpecialBitsOnMap(GameObject floorParent, ref Texture2D tex, int tileSize, int systemWidth, int systemHeight)
	{
		SpecialTextureStamp[] componentsInChildren = floorParent.GetComponentsInChildren<SpecialTextureStamp>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Vector2 vector = componentsInChildren[i].gameObject.transform.position;
			vector.x += systemWidth / 2;
			vector.y += systemHeight / 2;
			Texture2D stampTexture = componentsInChildren[i].GetStampTexture();
			if (stampTexture != null)
			{
				int num = Mathf.RoundToInt(vector.x * (float)tileSize - (float)(stampTexture.width / 2));
				int num2 = Mathf.RoundToInt(vector.y * (float)tileSize - (float)(stampTexture.height / tileSize * (tileSize / 2)));
				tex.SetPixels(num + 1, num2 + 1, stampTexture.width, stampTexture.height, stampTexture.GetPixels());
			}
		}
		tex.Apply();
	}
}
