using System;
using System.Collections.Generic;
using UnityEngine;

public class T17IconManager : T17MonoBehaviour
{
	[Serializable]
	public class IconKeyPair
	{
		public string m_Key;

		public Sprite m_Sprite;
	}

	private static T17IconManager s_Instance;

	public List<IconKeyPair> m_Icons;

	public static T17IconManager GetInstance()
	{
		return s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (s_Instance == null)
		{
			s_Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
	}

	protected virtual void OnDestroy()
	{
		m_Icons.Clear();
		if (s_Instance != null)
		{
			s_Instance = null;
		}
	}

	public static Sprite GetSpriteForKey(string key)
	{
		if (s_Instance != null)
		{
			return s_Instance.FindFirstMatchingSprite(key);
		}
		return null;
	}

	public Sprite FindFirstMatchingSprite(string key)
	{
		for (int num = m_Icons.Count - 1; num >= 0; num--)
		{
			if (m_Icons[num].m_Key == key)
			{
				return m_Icons[num].m_Sprite;
			}
		}
		return null;
	}
}
