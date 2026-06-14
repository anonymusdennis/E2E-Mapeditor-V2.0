using ExitGames.Client.Photon;
using UnityEngine;

public class NetConnectAndJoinRoom : T17MonoBehaviour
{
	public static CloudRegionCode PhotonRegion = CloudRegionCode.none;

	public static string AppVersion()
	{
		if (null != T17NetConnectAndJoinRoom.Instance)
		{
			return T17NetConnectAndJoinRoom.Instance.AppVersion;
		}
		return null;
	}

	public static bool GetCampaignMatchmakingSearch(out string lobbyName, out NetMatchmakingConfig config)
	{
		config = null;
		lobbyName = string.Empty;
		string text = string.Empty;
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.RoomPlatformType);
		if (matchmakingParameterFromCustomProperty != null)
		{
			string text2 = text;
			text = text2 + matchmakingParameterFromCustomProperty + " = \"" + T17NetConfig.GetPlatformType() + "\"";
		}
		string matchmakingParameterFromCustomProperty2 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.AppVersion);
		if (matchmakingParameterFromCustomProperty2 != null)
		{
			string text2 = text;
			text = text2 + " AND " + matchmakingParameterFromCustomProperty2 + " = \"" + T17NetConfig.MatchingVersionString + "\"";
		}
		config = NetMatchmakingConfig.GetInstance(PrisonConfig.ConfigType.Cooperative);
		if (null != config)
		{
			config.m_strSearchPrefix = text;
			lobbyName = GetLobbyName(PrisonConfig.ConfigType.Cooperative);
			return true;
		}
		return false;
	}

	public static string GetLobbyName(PrisonConfig.ConfigType config)
	{
		string empty = string.Empty;
		if (T17NetConfig.FeatureCampaignMatchmaking)
		{
			empty = string.Empty;
			switch (config)
			{
			case PrisonConfig.ConfigType.Cooperative:
			case PrisonConfig.ConfigType.Singleplayer:
				empty = GenerateCampaignMatchmakingLobbyName(GlobalStart.GetInstance().GetCurrentSelectedPrisonEnum());
				break;
			case PrisonConfig.ConfigType.Versus:
				if (null != VersusFrontendMenu.GetSelectedPlaylist())
				{
					empty = T17NetConfig.GetPlatformType() + VersusFrontendMenu.GetSelectedPlaylist().m_NameLocalisationKey;
				}
				break;
			}
		}
		else
		{
			empty = GenerateCampaignBrowseGamesLobbyName(config);
		}
		return empty;
	}

	public static string GenerateCampaignMatchmakingLobbyName(LevelScript.PRISON_ENUM prison)
	{
		return T17NetConfig.GetPlatformType() + prison;
	}

	public static string GenerateCampaignBrowseGamesLobbyName(PrisonConfig.ConfigType config)
	{
		return T17NetConfig.GetPlatformType() + config;
	}

	public static void Init_OnlineMode_JoinSpecific(string strRoomName)
	{
		T17NetConnectAndJoinRoom.Instance.Init_OnlineMode_JoinSpecific(strRoomName);
	}

	public static void Init_OnlineMode_JoinFilter(string lobbyName, string strSQLGameSearch, byte maxPlayers)
	{
		T17NetConnectAndJoinRoom.Instance.Init_OnlineMode_JoinFilter(lobbyName, strSQLGameSearch, maxPlayers);
	}

	public static void Init_OnlineMode_CreateRoom(PrisonConfig.ConfigType type, string lobbyName, string[] customPropertiesForLobby, Hashtable roomPropertyValues)
	{
		string roomName = (int)type + "_" + Platform.GetInstance().GetPrimaryUserName() + "-R" + Random.Range(1, int.MaxValue);
		T17NetConnectAndJoinRoom.Instance.Init_OnlineMode_CreateRoom(roomName, lobbyName, customPropertiesForLobby, roomPropertyValues);
	}

	public static void Init_OnlineMode_RoomLobbyList(string lobbyName)
	{
		T17NetConnectAndJoinRoom.Instance.Init_OnlineMode_RoomLobbyList(lobbyName);
	}

	public static void Init_OnlineMode_Matchmaking(string lobbyName, NetMatchmakingConfig config, byte maxPlayers, T17NetMatchmakingAgent.OnComplete completeCallback)
	{
		T17NetConnectAndJoinRoom.Instance.Init_OnlineMode_Matchmaking(lobbyName, config, maxPlayers, completeCallback);
	}

	public static void Init_OnlineMode_RoomSearchList(string lobbyName, string strSQLGameSearch)
	{
		T17NetConnectAndJoinRoom.Instance.Init_OnlineMode_RoomSearchList(lobbyName, strSQLGameSearch);
	}

	public static bool RequestConnectionState(NetConnectionState state, bool silentErrorDialogMode = false)
	{
		T17NetManager.UpdateStatus();
		return T17NetConnectAndJoinRoom.Instance.RequestConnectionState(state, silentErrorDialogMode);
	}

	public static NetConnectionState GetRequestedConnectionState()
	{
		return T17NetConnectAndJoinRoom.Instance.GetRequestedConnectionState();
	}
}
