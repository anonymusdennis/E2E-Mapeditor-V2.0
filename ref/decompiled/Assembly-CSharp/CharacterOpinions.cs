using System;
using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class CharacterOpinions : MonoBehaviour
{
	private Dictionary<Character, int> m_Opinions = new Dictionary<Character, int>(Character.CharacterTComparer);

	private T17NetView m_NetView;

	private Character m_AttachedToCharacter;

	private int m_DogLoveOpinion = -1;

	private bool m_bTriggeredDogLove;

	private List<Character> m_allLikedCharacters = new List<Character>();

	private List<Character> m_allHatedCharacters = new List<Character>();

	private bool m_bUpdateLikedHated = true;

	private bool m_bLikedShuffled;

	private bool m_bHatedShuffled;

	private void Awake()
	{
		m_NetView = GetComponent<T17NetView>();
		m_AttachedToCharacter = GetComponent<Character>();
	}

	protected virtual void OnDestroy()
	{
		m_AttachedToCharacter = null;
		m_allLikedCharacters.Clear();
		m_allHatedCharacters.Clear();
		m_NetView = null;
	}

	private void UpdateLikedAndHated()
	{
		if (m_bUpdateLikedHated)
		{
			m_allLikedCharacters = GetAllCharacters(IsLiked, shuffle: true);
			m_allHatedCharacters = GetAllCharacters(IsHated, shuffle: true);
			m_bUpdateLikedHated = false;
			m_bLikedShuffled = (m_bHatedShuffled = true);
		}
	}

	private void Update()
	{
		if (m_bUpdateLikedHated)
		{
			UpdateLikedAndHated();
		}
	}

	public int GetOpinionOf(Character character)
	{
		int result = 50;
		if (m_Opinions.ContainsKey(character))
		{
			result = m_Opinions[character];
		}
		return result;
	}

	public void SetOpinionOf(Character character, int amount)
	{
		SetOpinion_InternalRPC(character, amount);
	}

	public void IncreaseOpinionOf(Character character, int increase)
	{
		int num = 50;
		if (m_Opinions.ContainsKey(character))
		{
			num = m_Opinions[character];
		}
		SetOpinion_InternalRPC(character, num + increase);
	}

	public void DecreaseOpinionOf(Character character, int decrease)
	{
		int num = 50;
		if (m_Opinions.ContainsKey(character))
		{
			num = m_Opinions[character];
		}
		SetOpinion_InternalRPC(character, num - decrease);
	}

	private void SetOpinion_InternalRPC(Character character, int value)
	{
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			int viewID = character.m_NetView.viewID;
			m_NetView.PostLevelLoadRPC("RPCSetOpinion_Internal", NetTargets.All, viewID, value);
		}
	}

	[PunRPC]
	private void RPCSetOpinion_Internal(int characterID, int value, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterID);
		if (!(character == null))
		{
			SetOpinion_Internal(character, value);
		}
	}

	private void SetOpinion_Internal(Character character, int value)
	{
		int num = Mathf.Clamp(value, 0, 100);
		if (m_Opinions.ContainsKey(character))
		{
			m_Opinions[character] = num;
		}
		else
		{
			m_Opinions.Add(character, num);
		}
		m_bUpdateLikedHated = true;
		if (!m_bTriggeredDogLove && character.m_CharacterStats.m_bIsPlayer && character.m_NetView.isMine && m_AttachedToCharacter.m_CharacterRole == CharacterRole.Dog)
		{
			if (m_DogLoveOpinion == -1)
			{
				m_DogLoveOpinion = ConfigManager.GetInstance().aiConfig.GetDogLoveOpinion();
			}
			if (num >= m_DogLoveOpinion)
			{
				m_bTriggeredDogLove = true;
				StatSystem.GetInstance().IncStat(20, 1f, ((Player)character).m_Gamer, string.Empty);
			}
		}
		else
		{
			OpinionManager.GetInstance().OnOpinionUpdated();
		}
	}

	private List<Character> GetAllCharacters()
	{
		return new List<Character>(m_Opinions.Keys);
	}

	private List<Character> GetAllCharacters(Func<Character, int, bool> filter, bool shuffle)
	{
		List<Character> list = new List<Character>();
		foreach (KeyValuePair<Character, int> opinion in m_Opinions)
		{
			if (filter(opinion.Key, opinion.Value))
			{
				list.Add(opinion.Key);
			}
		}
		if (shuffle)
		{
			list.Shuffle();
		}
		return list;
	}

	public IList<Character> GetAllLikedCharacters()
	{
		UpdateLikedAndHated();
		if (!m_bLikedShuffled)
		{
			m_allLikedCharacters.Shuffle();
		}
		else
		{
			m_bLikedShuffled = false;
		}
		return m_allLikedCharacters.AsReadOnly();
	}

	public IList<Character> GetAllHatedCharacters()
	{
		UpdateLikedAndHated();
		if (!m_bHatedShuffled)
		{
			m_allHatedCharacters.Shuffle();
		}
		else
		{
			m_bHatedShuffled = false;
		}
		return m_allHatedCharacters.AsReadOnly();
	}

	private static bool IsLiked(Character character, int opinion)
	{
		return opinion > OpinionManager.GetInstance().GetHighOpinionThreshold();
	}

	private static bool IsHated(Character character, int opinion)
	{
		return opinion < OpinionManager.GetInstance().GetLowOpinionThreshold();
	}

	public List<ulong> Serialize()
	{
		List<ulong> list = new List<ulong>(m_Opinions.Count);
		List<Character> list2 = new List<Character>(m_Opinions.Keys);
		for (int i = 0; i < list2.Count; i++)
		{
			BitField bitField = new BitField();
			bitField.Set(12, (uint)list2[i].m_NetView.viewID);
			bitField.Set(8, (uint)m_Opinions[list2[i]]);
			list.Add((ulong)bitField);
		}
		return list;
	}

	public bool Deserialize(List<ulong> data, ref string error)
	{
		int num = 0;
		for (int i = 0; i < data.Count; i++)
		{
			BitField bitField = new BitField(data[i]);
			int uInt = (int)bitField.GetUInt(12);
			int uInt2 = (int)bitField.GetUInt(8);
			Character character = T17NetView.Find<Character>(uInt);
			if (character != null)
			{
				SetOpinion_Internal(character, uInt2);
				num++;
			}
		}
		if (num < data.Count)
		{
			string text = $"CharacterOpinions: Failed to properly deserialize opinions for character '{m_NetView.viewID}'";
			error = error + text + "\n";
			return false;
		}
		return true;
	}
}
