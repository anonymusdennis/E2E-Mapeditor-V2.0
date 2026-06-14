using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyAwardManager : MonoBehaviour
{
	[Serializable]
	private class PrisonEscapeSaveData
	{
		public LevelScript.PRISON_ENUM m_thePrison;

		public List<EscapeStat> m_Escapes;
	}

	[Serializable]
	private class PrisonEscapeCollectionSaveData
	{
		public PrisonEscapeSaveData[] m_PrisonEscapeData;
	}

	[Serializable]
	public class EscapeStat
	{
		public EscapeMethod m_EscapeMethod;

		public int m_Count;
	}

	private static KeyAwardManager s_TheInstance;

	private const string SAVE_ID = "KeyAwardData";

	private Dictionary<LevelScript.PRISON_ENUM, List<EscapeStat>> m_PrisonEscapeStats = new Dictionary<LevelScript.PRISON_ENUM, List<EscapeStat>>();

	private KeyAwardData m_AwardData;

	[Header("Testing")]
	public LevelScript.PRISON_ENUM m_TestPrison;

	public EscapeMethod m_TestEscapeMethod = EscapeMethod.NothingSpecial;

	public ProgressMilestone m_TestMilestone;

	public static bool AreAllPrisonsUnlocked => false;

	public static KeyAwardManager GetInstance()
	{
		return s_TheInstance;
	}

	private void Awake()
	{
		if (s_TheInstance != null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(this);
		s_TheInstance = this;
		m_AwardData = Resources.Load<KeyAwardData>("KeyAwardData");
		if (m_AwardData == null)
		{
			Debug.LogError(" ****** KEY AWARD DATA IS NULL *******");
		}
	}

	private void Start()
	{
	}

	protected virtual void OnDestroy()
	{
		if (s_TheInstance == this)
		{
			s_TheInstance = null;
		}
	}

	public void LoadData()
	{
		GlobalSave instance = GlobalSave.GetInstance();
		if (instance == null || string.IsNullOrEmpty("KeyAwardData"))
		{
			Debug.LogError(" ***** KeyAwardData - failed to load data **** ");
			return;
		}
		foreach (KeyValuePair<LevelScript.PRISON_ENUM, List<EscapeStat>> prisonEscapeStat in m_PrisonEscapeStats)
		{
			prisonEscapeStat.Value.Clear();
		}
		m_PrisonEscapeStats.Clear();
		instance.Get("KeyAwardData", out var value, string.Empty);
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		PrisonEscapeCollectionSaveData prisonEscapeCollectionSaveData = JsonUtility.FromJson<PrisonEscapeCollectionSaveData>(value);
		if (prisonEscapeCollectionSaveData == null)
		{
			return;
		}
		for (int i = 0; i < prisonEscapeCollectionSaveData.m_PrisonEscapeData.Length; i++)
		{
			PrisonEscapeSaveData prisonEscapeSaveData = prisonEscapeCollectionSaveData.m_PrisonEscapeData[i];
			List<EscapeStat> value2 = null;
			if (m_PrisonEscapeStats.TryGetValue(prisonEscapeSaveData.m_thePrison, out value2))
			{
				value2.AddRange(prisonEscapeSaveData.m_Escapes);
			}
			else
			{
				m_PrisonEscapeStats.Add(prisonEscapeSaveData.m_thePrison, prisonEscapeSaveData.m_Escapes);
			}
		}
	}

	public void SaveData()
	{
		GlobalSave instance = GlobalSave.GetInstance();
		if (instance == null || string.IsNullOrEmpty("KeyAwardData"))
		{
			return;
		}
		if (m_PrisonEscapeStats == null || m_PrisonEscapeStats.Count == 0)
		{
			instance.Set("KeyAwardData", string.Empty);
		}
		else
		{
			PrisonEscapeCollectionSaveData prisonEscapeCollectionSaveData = new PrisonEscapeCollectionSaveData();
			prisonEscapeCollectionSaveData.m_PrisonEscapeData = new PrisonEscapeSaveData[m_PrisonEscapeStats.Count];
			int num = 0;
			foreach (KeyValuePair<LevelScript.PRISON_ENUM, List<EscapeStat>> prisonEscapeStat in m_PrisonEscapeStats)
			{
				PrisonEscapeSaveData prisonEscapeSaveData = new PrisonEscapeSaveData();
				prisonEscapeSaveData.m_thePrison = prisonEscapeStat.Key;
				prisonEscapeSaveData.m_Escapes = prisonEscapeStat.Value;
				prisonEscapeCollectionSaveData.m_PrisonEscapeData[num++] = prisonEscapeSaveData;
			}
			string value = JsonUtility.ToJson(prisonEscapeCollectionSaveData);
			instance.Set("KeyAwardData", value);
		}
		instance.RequestSave();
	}

	public bool IsKeyAwarded(LevelScript.PRISON_ENUM thePrison, EscapeMethod theEscapeMethod)
	{
		return GetNumberOfTimesEscaped(thePrison, theEscapeMethod) > 0;
	}

	public int GetNumberOfTimesEscaped(LevelScript.PRISON_ENUM thePrison, EscapeMethod theEscapeMethod)
	{
		if (m_PrisonEscapeStats.TryGetValue(thePrison, out var value) && value != null && value.Count > 0)
		{
			EscapeStat escapeStat = value.Find((EscapeStat S) => S.m_EscapeMethod == theEscapeMethod);
			if (escapeStat != null)
			{
				return escapeStat.m_Count;
			}
		}
		return 0;
	}

	public static void OnPrisonEscaped(Gamer theGamer, PrisonData.LevelInfo levelInfo, EscapeMethod theEscapeMethod)
	{
		if (s_TheInstance != null && levelInfo != null)
		{
			s_TheInstance._OnPrisonEscaped(theGamer, levelInfo.m_PrisonEnum, theEscapeMethod);
		}
	}

	private void _OnPrisonEscaped(Gamer theGamer, LevelScript.PRISON_ENUM thePrison, EscapeMethod theEscapeMethod)
	{
		if (theGamer == null || thePrison == LevelScript.PRISON_ENUM.Unassigned || thePrison == LevelScript.PRISON_ENUM.CustomPrison)
		{
			return;
		}
		List<EscapeStat> value = null;
		if (m_PrisonEscapeStats.TryGetValue(thePrison, out value))
		{
			EscapeStat escapeStat = value.Find((EscapeStat S) => S.m_EscapeMethod == theEscapeMethod);
			if (escapeStat == null)
			{
				AddNewEscapeStat(value, theEscapeMethod);
				GiveAward(thePrison, theEscapeMethod, theGamer);
			}
			else
			{
				escapeStat.m_Count++;
			}
		}
		else
		{
			value = new List<EscapeStat>();
			AddNewEscapeStat(value, theEscapeMethod);
			m_PrisonEscapeStats.Add(thePrison, value);
			GiveAward(thePrison, theEscapeMethod, theGamer);
		}
		SaveData();
	}

	private void AddNewEscapeStat(List<EscapeStat> thePrisonStatsList, EscapeMethod theEscapeMethod)
	{
		EscapeStat escapeStat = new EscapeStat();
		escapeStat.m_EscapeMethod = theEscapeMethod;
		escapeStat.m_Count = 1;
		thePrisonStatsList.Add(escapeStat);
	}

	private void GiveAward(LevelScript.PRISON_ENUM thePrison, EscapeMethod theEscapeMethod, Gamer theGamer)
	{
		int num = ((m_AwardData == null) ? 1 : m_AwardData.GetNumberOfKeysToAward(thePrison, theEscapeMethod));
		StatSystem instance = StatSystem.GetInstance();
		if (instance != null)
		{
			instance.IncStat(44, num, theGamer, string.Empty);
		}
	}
}
