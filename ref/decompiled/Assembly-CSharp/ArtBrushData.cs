using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Art Brush", menuName = "Team17/Create Art Brush", order = 100)]
public class ArtBrushData : ScriptableObject
{
	public Material m_MainMaterial;

	public List<Material> m_DamageStageMaterials;

	public void CleanUp()
	{
		if (m_MainMaterial != null)
		{
			m_MainMaterial.mainTexture = null;
			m_MainMaterial = null;
		}
		for (int num = m_DamageStageMaterials.Count - 1; num >= 0; num--)
		{
			if (m_DamageStageMaterials[num] != null)
			{
				m_DamageStageMaterials[num].mainTexture = null;
				m_DamageStageMaterials[num] = null;
			}
		}
	}
}
