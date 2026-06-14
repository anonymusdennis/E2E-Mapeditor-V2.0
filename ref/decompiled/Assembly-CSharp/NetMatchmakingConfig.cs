using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

[Serializable]
public class NetMatchmakingConfig : MonoBehaviour
{
	private static Dictionary<PrisonConfig.ConfigType, NetMatchmakingConfig> m_Instances = new Dictionary<PrisonConfig.ConfigType, NetMatchmakingConfig>(2);

	public string m_strSearchPrefix = string.Empty;

	public PrisonConfig.ConfigType m_Type;

	[SerializeField]
	public List<string> m_strGameSearches = new List<string>();

	[SerializeField]
	public List<int> m_iGameSearchDurations = new List<int>();

	private static ExitGames.Client.Photon.Hashtable m_MatchmakingParametersMap = new ExitGames.Client.Photon.Hashtable
	{
		{
			T17NetRoomGameView.CustomProperty.GamerCount,
			"C0"
		},
		{
			T17NetRoomGameView.CustomProperty.RoomPlatformType,
			"C1"
		},
		{
			T17NetRoomGameView.CustomProperty.GameState,
			"C2"
		},
		{
			T17NetRoomGameView.CustomProperty.PrisonEnum,
			"C3"
		},
		{
			T17NetRoomGameView.CustomProperty.PrisonDay,
			"C4"
		},
		{
			T17NetRoomGameView.CustomProperty.HostName,
			"C5"
		},
		{
			T17NetRoomGameView.CustomProperty.PlaylistId,
			"C6"
		},
		{
			T17NetRoomGameView.CustomProperty.AppVersion,
			"C7"
		},
		{
			T17NetRoomGameView.CustomProperty.RoomType,
			"C8"
		},
		{
			T17NetRoomGameView.CustomProperty.Password,
			"C9"
		}
	};

	public static bool IsEncrypted(string propertyKey)
	{
		return false;
	}

	public static ExitGames.Client.Photon.Hashtable GetMatchmakingParameters()
	{
		return m_MatchmakingParametersMap;
	}

	public static string GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty property)
	{
		if (GetMatchmakingParameters().TryGetValue(property, out var value))
		{
			return value as string;
		}
		return null;
	}

	public static bool GetMatchmakingCustomPropertyFromString(string strValue, ref T17NetRoomGameView.CustomProperty property)
	{
		if (!string.IsNullOrEmpty(strValue))
		{
			foreach (DictionaryEntry item in m_MatchmakingParametersMap)
			{
				string value = item.Value.ToString();
				if (!string.IsNullOrEmpty(value) && strValue.Equals(value))
				{
					property = (T17NetRoomGameView.CustomProperty)item.Key;
					return true;
				}
			}
		}
		return false;
	}

	private void Awake()
	{
		m_Instances[m_Type] = this;
	}

	public static NetMatchmakingConfig GetInstance(PrisonConfig.ConfigType type)
	{
		return m_Instances[type];
	}
}
