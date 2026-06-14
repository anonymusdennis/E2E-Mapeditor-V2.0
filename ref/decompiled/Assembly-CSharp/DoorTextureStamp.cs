using System;
using UnityEngine;

public class DoorTextureStamp : TextureStamp
{
	[Serializable]
	public class TextureKeyPair
	{
		public KeyFunctionality.KeyColour colour = KeyFunctionality.KeyColour.None;

		public Texture2D m_Stamp;
	}

	private Door m_Door;

	public TextureKeyPair[] m_KeyTexturePairs;

	public override Texture2D GetStampTexture()
	{
		m_Door = GetComponent<Door>();
		if (m_Door == null)
		{
			Debug.LogError("Door map stamp does not have a door script on it");
			return m_Stamp;
		}
		if (m_Door.m_DoorOutfitType != 0)
		{
			return m_Stamp;
		}
		for (int i = 0; i < m_KeyTexturePairs.Length; i++)
		{
			if (m_KeyTexturePairs[i].colour == m_Door.m_DoorKeyColour)
			{
				m_Stamp = m_KeyTexturePairs[i].m_Stamp;
			}
		}
		return m_Stamp;
	}
}
