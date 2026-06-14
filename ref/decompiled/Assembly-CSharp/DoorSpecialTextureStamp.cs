using System;
using UnityEngine;

public class DoorSpecialTextureStamp : SpecialTextureStamp
{
	[Serializable]
	public class TextureKeyPair
	{
		public KeyFunctionality.KeyColour colour = KeyFunctionality.KeyColour.None;

		public Texture2D m_Stamp;
	}

	public Door door;

	public TextureKeyPair[] m_KeyTexturePairs;

	public override Texture2D GetStampTexture()
	{
		for (int i = 0; i < m_KeyTexturePairs.Length; i++)
		{
			if (m_KeyTexturePairs[i].colour == door.m_DoorKeyColour)
			{
				m_Stamp = m_KeyTexturePairs[i].m_Stamp;
			}
		}
		return m_Stamp;
	}
}
