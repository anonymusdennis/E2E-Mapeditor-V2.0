using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class NetCreateRoomHelper
{
	public delegate void RoomJoinedHandler(bool setupOk);

	public RoomJoinedHandler OnRoomJoined;

	private T17DialogBox m_GameSetupSpinner;

	private T17NetRoomGameView.GameRoomType m_RoomTypeToCreate;

	private PrisonConfig.ConfigType m_PrisonConfigType;

	private string m_RoomPassword = string.Empty;

	private float m_ShownTimestamp;

	private bool m_bRaisedEvent;

	public float m_MaxDisplayTime = 10f;

	private static int m_ResolvingCount;

	public NetCreateRoomHelper(T17NetRoomGameView.GameRoomType roomType, PrisonConfig.ConfigType config, RoomJoinedHandler callback, T17DialogBox gameSetupSpinner, string password = "")
	{
		m_RoomTypeToCreate = roomType;
		m_PrisonConfigType = config;
		OnRoomJoined = callback;
		m_RoomPassword = password;
		m_GameSetupSpinner = gameSetupSpinner;
	}

	~NetCreateRoomHelper()
	{
		if (m_GameSetupSpinner != null)
		{
			m_GameSetupSpinner.Hide();
		}
	}

	public static void RequestCreateRoom(T17NetRoomGameView.GameRoomType roomType, PrisonConfig.ConfigType config, RoomJoinedHandler callback, bool showDialogs = true, string password = "")
	{
		T17DialogBox gameSetupSpinner = ((!showDialogs) ? null : T17DialogBoxManager.GetDialog(forSingleUser: false));
		NetCreateRoomHelper netCreateRoomHelper = new NetCreateRoomHelper(roomType, config, callback, gameSetupSpinner, password);
		if (roomType == T17NetRoomGameView.GameRoomType.Offline || T17NetManager.NetOnlineMode)
		{
			netCreateRoomHelper.CreateGame();
		}
		else
		{
			callback(setupOk: false);
		}
	}

	private static void AddInitialRoomProperty(ref Hashtable hashTable, T17NetRoomGameView.CustomProperty key, object value)
	{
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(key);
		if (string.IsNullOrEmpty(matchmakingParameterFromCustomProperty))
		{
			matchmakingParameterFromCustomProperty = ((byte)key).ToString();
			if (!NetMatchmakingConfig.IsEncrypted(matchmakingParameterFromCustomProperty))
			{
				hashTable[matchmakingParameterFromCustomProperty] = value;
			}
			else
			{
				Debug.LogError("Unable to set encryped room property " + key.ToString() + " when creating a room.  Wait for the key to be retreived.");
			}
		}
		else
		{
			hashTable.Add(matchmakingParameterFromCustomProperty, value);
		}
	}

	public static void CreateRoomMatchmakingProperties(PrisonConfig.ConfigType configType, T17NetRoomGameView.GameRoomType roomType, out string[] customPropertyDefinitionsForLobby, out Hashtable initialRoomPropertyValues, string password = "")
	{
		GlobalStart instance = GlobalStart.GetInstance();
		if (null != instance)
		{
			initialRoomPropertyValues = new Hashtable();
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.GamerCount, Gamer.m_GamerCount);
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.RoomPlatformType, T17NetConfig.GetPlatformType());
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.GameState, (int)GlobalStart.GetInstance().GetMode());
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.PrisonEnum, (int)GlobalStart.GetInstance().GetCurrentSelectedPrisonEnum());
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.PrisonDay, 0);
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.PrisonHour, 0);
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.AppVersion, T17NetConfig.MatchingVersionString);
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.ConfigType, (int)configType);
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.RoomType, (int)roomType);
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.HostName, Platform.GetInstance().GetPrimaryUserName().ToLowerInvariant());
			AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.Password, Encryption.Encrypt(password, "of all the flavours you choose to be salty", "SHA1", 2, 256, "default"));
			if (null != VersusFrontendMenu.GetSelectedPlaylist())
			{
				AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.PlaylistId, VersusFrontendMenu.GetSelectedPlaylist().m_NameLocalisationKey);
			}
			if (null != instance.GetCurrentSelectedPrisonData())
			{
				AddInitialRoomProperty(ref initialRoomPropertyValues, T17NetRoomGameView.CustomProperty.DisplayName, GlobalStart.GetInstance().GetCurrentSelectedPrisonData().m_NameLocalizationKey);
			}
			int num = 0;
			customPropertyDefinitionsForLobby = new string[initialRoomPropertyValues.Count];
			{
				foreach (object key in initialRoomPropertyValues.Keys)
				{
					customPropertyDefinitionsForLobby[num] = key.ToString();
					num++;
				}
				return;
			}
		}
		customPropertyDefinitionsForLobby = null;
		initialRoomPropertyValues = null;
	}

	private void CreateGame()
	{
		m_ResolvingCount++;
		m_ShownTimestamp = T17NetManager.RealTime;
		m_bRaisedEvent = false;
		if (m_GameSetupSpinner != null)
		{
			m_GameSetupSpinner.InitializeSpinner(hasCancelButton: false, "Text.Dialog.Net.CreateRoom.CreateTitle", "Text.Dialog.Net.CreateRoom.CreateBody", string.Empty);
			T17DialogBox gameSetupSpinner = m_GameSetupSpinner;
			gameSetupSpinner.OnUpdate = (T17DialogBox.DialogEvent)Delegate.Combine(gameSetupSpinner.OnUpdate, new T17DialogBox.DialogEvent(Update));
			m_GameSetupSpinner.Show();
		}
		T17NetManager.OnJoinedRoomEvent += T17NetManager_OnJoinedRoomEvent;
		T17NetManager.OnCreatedRoomEvent += T17NetManager_OnCreatedRoomEvent;
		switch (m_RoomTypeToCreate)
		{
		case T17NetRoomGameView.GameRoomType.Offline:
			if (T17NetManager.NetOfflineMode && T17NetRoomManager.IsInRoom())
			{
				T17NetManager_OnJoinedRoomEvent(0);
			}
			else
			{
				NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
			}
			break;
		case T17NetRoomGameView.GameRoomType.Public:
		case T17NetRoomGameView.GameRoomType.Private:
		{
			string lobbyName = NetConnectAndJoinRoom.GetLobbyName(m_PrisonConfigType);
			CreateRoomMatchmakingProperties(m_PrisonConfigType, m_RoomTypeToCreate, out var customPropertyDefinitionsForLobby, out var initialRoomPropertyValues, m_RoomPassword);
			NetConnectAndJoinRoom.Init_OnlineMode_CreateRoom(m_PrisonConfigType, lobbyName, customPropertyDefinitionsForLobby, initialRoomPropertyValues);
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_CreateRoom);
			break;
		}
		}
	}

	private void T17NetManager_OnCreatedRoomEvent(bool result)
	{
		if (!result)
		{
			OnCreateCompleted(result);
		}
	}

	private void T17NetManager_OnJoinedRoomEvent(short result)
	{
		OnCreateCompleted(result == 0);
	}

	private void OnCreateCompleted(bool result)
	{
		T17NetManager.OnJoinedRoomEvent -= T17NetManager_OnJoinedRoomEvent;
		T17NetManager.OnCreatedRoomEvent -= T17NetManager_OnCreatedRoomEvent;
		if (m_GameSetupSpinner != null)
		{
			m_GameSetupSpinner.Hide();
			m_GameSetupSpinner = null;
		}
		if (result)
		{
			T17NetRoomManager.Instance.SetPropertiesForGameroomType(m_RoomTypeToCreate);
			T17NetRoomManager.Instance.SetPropertiesForGameroomPassword(m_RoomPassword);
		}
		else
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Idle);
		}
		m_bRaisedEvent = true;
		m_ResolvingCount--;
		if (OnRoomJoined != null)
		{
			OnRoomJoined(result);
		}
	}

	private void Update(T17DialogBox sender)
	{
		if (T17NetManager.RealTime - m_ShownTimestamp >= m_MaxDisplayTime)
		{
			OnCreateCompleted(result: false);
		}
	}

	public static bool IsResolving()
	{
		return m_ResolvingCount != 0;
	}
}
