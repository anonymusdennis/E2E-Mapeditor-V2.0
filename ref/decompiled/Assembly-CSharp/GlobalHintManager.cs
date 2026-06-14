using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalHintManager : MonoBehaviour
{
	[Serializable]
	public struct PrisonHints
	{
		public LevelScript.PRISON_ENUM m_Prison;

		public HintConfig m_PrisonHintConfig;
	}

	private static GlobalHintManager m_Instance;

	public PrisonHints[] m_PrisonHints;

	private List<long[]> m_PlayerBitfields = new List<long[]>();

	public static GlobalHintManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public HintConfig.HintData GetHintData(LevelScript.PRISON_ENUM prison, int hintIndex)
	{
		for (int i = 0; i < m_PrisonHints.Length; i++)
		{
			if (m_PrisonHints[i].m_Prison == prison && hintIndex < m_PrisonHints[i].m_PrisonHintConfig.m_Hints.Length && hintIndex >= 0)
			{
				return m_PrisonHints[i].m_PrisonHintConfig.m_Hints[hintIndex];
			}
		}
		return null;
	}

	public HintConfig.CraftHintData GetCraftHintData(LevelScript.PRISON_ENUM prison, int craftHintIndex)
	{
		for (int i = 0; i < m_PrisonHints.Length; i++)
		{
			if (m_PrisonHints[i].m_Prison == prison)
			{
				craftHintIndex -= m_PrisonHints[i].m_PrisonHintConfig.m_Hints.Length;
				if (craftHintIndex < m_PrisonHints[i].m_PrisonHintConfig.m_CraftHints.Length && craftHintIndex >= 0)
				{
					return m_PrisonHints[i].m_PrisonHintConfig.m_CraftHints[craftHintIndex];
				}
			}
		}
		return null;
	}

	public int GetTotalHintCount(LevelScript.PRISON_ENUM prison)
	{
		for (int i = 0; i < m_PrisonHints.Length; i++)
		{
			if (m_PrisonHints[i].m_Prison == prison)
			{
				return m_PrisonHints[i].m_PrisonHintConfig.m_Hints.Length + m_PrisonHints[i].m_PrisonHintConfig.m_CraftHints.Length;
			}
		}
		return -1;
	}

	public bool CreateNewPlayerBitfield(Gamer gamer)
	{
		if (gamer != null && gamer.IsLocal() && gamer.m_RewiredPlayer != null)
		{
			int id = gamer.m_RewiredPlayer.id;
			if (gamer.m_bPrimaryLocal)
			{
				long[] value = new long[m_PrisonHints.Length];
				GlobalSave.GetInstance().Get("Hints:PlayerBitfields", out value, new long[m_PrisonHints.Length]);
				if (id >= 0 && id < m_PlayerBitfields.Count)
				{
					if (m_PlayerBitfields[id] != null)
					{
						return false;
					}
					m_PlayerBitfields[id] = value;
				}
				else
				{
					int count = m_PlayerBitfields.Count;
					for (int i = 0; i <= id - count; i++)
					{
						m_PlayerBitfields.Add(new long[m_PrisonHints.Length]);
					}
					if (m_PrisonHints.Length <= value.Length)
					{
						m_PlayerBitfields[id] = value;
					}
					else
					{
						for (int j = 0; j < value.Length; j++)
						{
							m_PlayerBitfields[id][j] = value[j];
						}
					}
				}
			}
			else
			{
				long[] array = new long[m_PrisonHints.Length];
				if (id >= 0 && id < m_PlayerBitfields.Count)
				{
					if (m_PlayerBitfields[id] != null)
					{
						return false;
					}
					m_PlayerBitfields[id] = array;
				}
				else
				{
					m_PlayerBitfields.Add(array);
				}
			}
			return true;
		}
		return false;
	}

	public void RemovePlayerBitfield(Gamer gamer)
	{
		if (gamer != null && gamer.IsLocal())
		{
			int id = gamer.m_RewiredPlayer.id;
			if (id >= 0 && id < m_PlayerBitfields.Count)
			{
				m_PlayerBitfields[id] = null;
			}
		}
	}

	public void SetHintBitfield(Gamer gamer, LevelScript.PRISON_ENUM prisonEnum, long hintBitfield)
	{
		int id = gamer.m_RewiredPlayer.id;
		if (id < 0 || id >= m_PlayerBitfields.Count)
		{
			return;
		}
		for (int i = 0; i < m_PrisonHints.Length; i++)
		{
			if (i < m_PlayerBitfields[id].Length && m_PrisonHints[i].m_Prison == prisonEnum)
			{
				m_PlayerBitfields[id][i] = hintBitfield;
				if (gamer.m_bPrimaryLocal)
				{
					GlobalSave.GetInstance().Set("Hints:PlayerBitfields", m_PlayerBitfields[id]);
					GlobalSave.GetInstance().RequestSave();
					break;
				}
			}
		}
	}

	public bool GetHintBitfield(Gamer gamer, LevelScript.PRISON_ENUM prisonEnum, out long bitfield)
	{
		bitfield = 0L;
		int id = gamer.m_RewiredPlayer.id;
		if (id >= 0 && id < m_PlayerBitfields.Count)
		{
			for (int i = 0; i < m_PrisonHints.Length; i++)
			{
				if (i < m_PlayerBitfields[id].Length && m_PrisonHints[i].m_Prison == prisonEnum)
				{
					bitfield = m_PlayerBitfields[id][i];
					return true;
				}
			}
		}
		return false;
	}
}
