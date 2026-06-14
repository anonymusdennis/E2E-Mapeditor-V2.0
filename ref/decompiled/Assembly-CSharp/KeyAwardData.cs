using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "KeyAwardData", menuName = "Team17/Create Key Award Data")]
public class KeyAwardData : ScriptableObject
{
	[Serializable]
	public struct AwardData
	{
		[HideInInspector]
		public string m_DispLayName;

		public LevelScript.PRISON_ENUM m_Prison;

		public EscapeMethod m_EscapeMethod;

		public int m_NumKeysAwarded;
	}

	[Header("Standard / Default")]
	public int m_DefaultNumKeysAwarded = 1;

	[Header("Prison Specific Awards")]
	public AwardData[] m_PrisonAwards;

	public int GetNumberOfKeysToAward(LevelScript.PRISON_ENUM thePrison, EscapeMethod theEscapeMethod)
	{
		LevelDataManager instance = LevelDataManager.GetInstance();
		if (instance != null && instance.IsDLCLevel(thePrison))
		{
			return 0;
		}
		int result = m_DefaultNumKeysAwarded;
		if (m_PrisonAwards != null)
		{
			for (int i = 0; i < m_PrisonAwards.Length; i++)
			{
				if (m_PrisonAwards[i].m_Prison == thePrison && m_PrisonAwards[i].m_EscapeMethod == theEscapeMethod)
				{
					result = m_PrisonAwards[i].m_NumKeysAwarded;
					break;
				}
			}
		}
		return result;
	}
}
