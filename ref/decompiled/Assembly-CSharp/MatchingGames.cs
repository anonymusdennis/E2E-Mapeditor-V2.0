using UnityEngine;

public class MatchingGames : MonoBehaviour
{
	public class Game
	{
		public string m_HostKey;

		public int m_SlotNetViewID;

		public float m_Health;

		public float m_Energy;

		public float m_Strength;

		public float m_Intellect;

		public float m_Cardio;

		public float m_Heat;

		public float m_Money;
	}

	private int m_LastAdded;

	private const int TOTAL_MATCHES_TRACKED = 50;

	private Game[] m_Games = new Game[50];

	private static MatchingGames m_Instance;

	private void Awake()
	{
		m_Instance = this;
	}

	private void OnDestroy()
	{
		m_Instance = null;
	}

	public static MatchingGames GetInstance()
	{
		return m_Instance;
	}

	public Game FindGame(string hostKey)
	{
		for (int i = 0; i < 50; i++)
		{
			if (m_Games[i] != null && m_Games[i].m_HostKey == hostKey)
			{
				return m_Games[i];
			}
		}
		return null;
	}

	public void SaveGame()
	{
		int num = -1;
		string hostKeyHash = PrisonSnapshotIO.GetHostKeyHash();
		for (int i = 0; i < 50; i++)
		{
			if (m_Games[i] != null && m_Games[i].m_HostKey == hostKeyHash)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = m_LastAdded + 1;
			if (num >= 50)
			{
				num = 0;
			}
			m_LastAdded = num;
		}
		if (num == -1)
		{
			return;
		}
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_PlayerObject != null)
		{
			if (m_Games[num] == null)
			{
				m_Games[num] = new Game();
			}
			m_Games[num].m_HostKey = hostKeyHash;
			m_Games[num].m_SlotNetViewID = primaryGamer.m_NetViewID;
			m_Games[num].m_Health = primaryGamer.m_PlayerObject.m_CharacterStats.Health;
			m_Games[num].m_Energy = primaryGamer.m_PlayerObject.m_CharacterStats.Energy;
			m_Games[num].m_Strength = primaryGamer.m_PlayerObject.m_CharacterStats.Strength;
			m_Games[num].m_Intellect = primaryGamer.m_PlayerObject.m_CharacterStats.Intellect;
			m_Games[num].m_Cardio = primaryGamer.m_PlayerObject.m_CharacterStats.Cardio;
			m_Games[num].m_Heat = primaryGamer.m_PlayerObject.m_CharacterStats.Heat;
			m_Games[num].m_Money = primaryGamer.m_PlayerObject.m_CharacterStats.Money;
			SaveOut();
		}
	}

	private void SaveOut()
	{
		GlobalSave instance = GlobalSave.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		instance.Set("MG-LA", m_LastAdded);
		for (int i = 0; i < 50; i++)
		{
			if (m_Games[i] != null)
			{
				instance.Set("MG-G" + i + "_Key", m_Games[i].m_HostKey);
				instance.Set("MG-G" + i + "_ID", m_Games[i].m_SlotNetViewID);
				instance.Set("MG-G" + i + "_H", m_Games[i].m_Health);
				instance.Set("MG-G" + i + "_E", m_Games[i].m_Energy);
				instance.Set("MG-G" + i + "_S", m_Games[i].m_Strength);
				instance.Set("MG-G" + i + "_I", m_Games[i].m_Intellect);
				instance.Set("MG-G" + i + "_C", m_Games[i].m_Cardio);
				instance.Set("MG-G" + i + "_T", m_Games[i].m_Heat);
				instance.Set("MG-G" + i + "_M", m_Games[i].m_Money);
			}
			else
			{
				instance.Set("MG-G" + i + "_Key", 0);
			}
		}
	}

	public void LoadIn()
	{
		GlobalSave instance = GlobalSave.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		instance.Get("MG-LA", out m_LastAdded, 0);
		for (int i = 0; i < 50; i++)
		{
			instance.Get("MG-G" + i + "_Key", out var value, string.Empty);
			if (value != string.Empty)
			{
				if (m_Games[i] == null)
				{
					m_Games[i] = new Game();
				}
				m_Games[i].m_HostKey = value;
				instance.Get("MG-G" + i + "_ID", out m_Games[i].m_SlotNetViewID, -1);
				instance.Get("MG-G" + i + "_H", out m_Games[i].m_Health, 0f);
				instance.Get("MG-G" + i + "_E", out m_Games[i].m_Energy, 0f);
				instance.Get("MG-G" + i + "_S", out m_Games[i].m_Strength, 0f);
				instance.Get("MG-G" + i + "_I", out m_Games[i].m_Intellect, 0f);
				instance.Get("MG-G" + i + "_C", out m_Games[i].m_Cardio, 0f);
				instance.Get("MG-G" + i + "_T", out m_Games[i].m_Heat, 0f);
				instance.Get("MG-G" + i + "_M", out m_Games[i].m_Money, 0f);
			}
		}
	}
}
