using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Underground Material Map", menuName = "Team17/Create Underground Material Mapper", order = 101)]
public class UndergroundMaterialMapper : ScriptableObject
{
	[Serializable]
	public class Mapping
	{
		public int m_Mask;

		public ArtBrushData m_WallArtBrush;

		public Material m_GroundMaterial;

		public Mapping()
		{
			m_Mask = 0;
			m_WallArtBrush = null;
			m_GroundMaterial = null;
		}
	}

	public List<Mapping> m_Map = new List<Mapping>();

	public void CleanUp()
	{
		for (int num = m_Map.Count - 1; num >= 0; num--)
		{
			if (m_Map[num] != null)
			{
				if (m_Map[num].m_GroundMaterial != null)
				{
					m_Map[num].m_GroundMaterial.mainTexture = null;
					m_Map[num].m_GroundMaterial = null;
				}
				if (m_Map[num].m_WallArtBrush != null)
				{
					m_Map[num].m_WallArtBrush.CleanUp();
				}
			}
		}
		m_Map.Clear();
	}
}
