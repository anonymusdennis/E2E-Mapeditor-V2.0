using System;
using System.Collections.Generic;
using T17.UI.Carousel;
using UnityEngine;
using UnityEngine.Events;

public class BrowseGamesFrontendMenu : FrontendMenuBehaviour
{
	public enum RoomOrderings
	{
		Unassigned,
		GameName,
		Prison,
		Days,
		CurrentPlayers
	}

	[Header("UI references")]
	public T17Text m_InformationalText;

	public T17ScrollView m_ScrollView;

	public GameObject m_RoomInfoObjectPrefab;

	public UnityEvent m_OnRoomJoinActions;

	public TextCarousel m_PrisonFilter;

	public TextCarousel m_DayFilter;

	public T17InputField m_PasswordField;

	public GameObject m_LoadingIcon;

	public bool m_bEditorMenu;

	public PasswordDialogMenu m_PasswordDialog;

	[Tooltip("The day ranges we can search for. I will automatically include an 'Any' and a final range for eg 20days+")]
	[Header("Behaviour and Filters")]
	public List<Vector2> m_CustomDayFilters = new List<Vector2>
	{
		new Vector2(0f, 10f),
		new Vector2(11f, 20f)
	};

	private List<Vector2> m_DayFilters;

	private LevelScript.PRISON_ENUM[] m_LevelFilters;

	public RoomOrderings m_Ordering = RoomOrderings.GameName;

	public int m_MaxResults = 100;

	private Vector2 m_SelectedDayRange;

	private LevelScript.PRISON_ENUM m_SelectedPrisonFilter;

	private bool m_reverseOrdering;

	private List<LobbyRoomInfoObject> m_CurrentAttachedInfoObjects = new List<LobbyRoomInfoObject>();

	private float m_RefetchTimer;

	private const float REFETCH_TIME = 5f;

	private string m_InfoString = string.Empty;

	public Transform m_NoGamesMessage;

	public T17InputField m_HostNameInputField;

	private string m_HostName = string.Empty;

	protected override void Awake()
	{
		base.Awake();
		m_DayFilters = new List<Vector2>
		{
			new Vector2(-1f, -1f)
		};
		m_DayFilters.AddRange(m_CustomDayFilters);
		if (m_bEditorMenu)
		{
			m_SelectedPrisonFilter = LevelScript.PRISON_ENUM.CustomPrison;
			m_SelectedDayRange = new Vector2(-1f, -1f);
			m_LevelFilters = new LevelScript.PRISON_ENUM[1];
			m_LevelFilters[0] = LevelScript.PRISON_ENUM.CustomPrison;
		}
		else if (T17NetConfig.FeatureCampaignMatchmaking)
		{
			if (m_CustomDayFilters.Count > 0)
			{
				m_DayFilters.Add(new Vector2(m_CustomDayFilters[m_CustomDayFilters.Count - 1].y, -1f));
			}
			LevelScript.PRISON_ENUM[] playableLevels = LevelScript.GetPlayableLevels();
			m_LevelFilters = new LevelScript.PRISON_ENUM[playableLevels.Length + 1];
			m_SelectedPrisonFilter = LevelScript.PRISON_ENUM.Unassigned;
			m_SelectedDayRange = new Vector2(-1f, -1f);
			for (int i = 0; i < playableLevels.Length; i++)
			{
				m_LevelFilters[i] = playableLevels[i];
			}
		}
		else
		{
			if (m_CustomDayFilters.Count > 0)
			{
				m_DayFilters.Add(new Vector2(m_CustomDayFilters[m_CustomDayFilters.Count - 1].y, -1f));
			}
			LevelScript.PRISON_ENUM[] playableLevels2 = LevelScript.GetPlayableLevels();
			List<LevelScript.PRISON_ENUM> list = new List<LevelScript.PRISON_ENUM>();
			list.Add(LevelScript.PRISON_ENUM.Unassigned);
			for (int j = 0; j < 17; j++)
			{
				LevelScript.PRISON_ENUM pRISON_ENUM = LevelScript.MapLeaderboardToPrisonEnum((LevelScript.LEADERBOARD_PRISON_ENUM)j);
				LevelDataManager instance = LevelDataManager.GetInstance();
				for (int k = 0; k < playableLevels2.Length; k++)
				{
					if (pRISON_ENUM == playableLevels2[k])
					{
						if (instance != null && instance.IsLevelAvailable(pRISON_ENUM))
						{
							list.Add(pRISON_ENUM);
						}
						break;
					}
				}
			}
			m_LevelFilters = list.ToArray();
			m_SelectedPrisonFilter = LevelScript.PRISON_ENUM.Unassigned;
			m_SelectedDayRange = new Vector2(-1f, -1f);
		}
		PopulateFilters();
		if (m_PrisonFilter != null)
		{
			m_PrisonFilter.IndexSelectedEvent += PrisonFilter_IndexSelectedEvent;
		}
		if (m_DayFilter != null)
		{
			m_DayFilter.IndexSelectedEvent += DayFilter_IndexSelectedEvent;
		}
		if (m_HostNameInputField != null)
		{
			m_HostNameInputField.onEndEdit.AddListener(SetHostName);
		}
		T17NetManager.OnRoomListUpdatedEvent += OnRoomListUpdated;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_PrisonFilter != null)
		{
			m_PrisonFilter.IndexSelectedEvent -= PrisonFilter_IndexSelectedEvent;
		}
		if (m_DayFilter != null)
		{
			m_DayFilter.IndexSelectedEvent -= DayFilter_IndexSelectedEvent;
		}
		if (m_HostNameInputField != null)
		{
			m_HostNameInputField.onEndEdit.RemoveListener(SetHostName);
		}
		T17NetManager.OnRoomListUpdatedEvent -= OnRoomListUpdated;
	}

	private void OnEnable()
	{
		m_HostName = string.Empty;
	}

	private void OnDisable()
	{
		m_HostName = string.Empty;
	}

	private void SetHostName(string hostName)
	{
		m_HostName = hostName;
		Search();
	}

	private void Search()
	{
		if (m_PasswordDialog.gameObject.activeInHierarchy)
		{
			return;
		}
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
		if (m_SelectedPrisonFilter != 0)
		{
			string matchmakingParameterFromCustomProperty3 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.PrisonEnum);
			if (!string.IsNullOrEmpty(matchmakingParameterFromCustomProperty3))
			{
				string text2 = text;
				text = text2 + " AND " + matchmakingParameterFromCustomProperty3 + " = " + (int)m_SelectedPrisonFilter;
			}
		}
		else if (!m_bEditorMenu)
		{
			string matchmakingParameterFromCustomProperty4 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.PrisonEnum);
			if (!string.IsNullOrEmpty(matchmakingParameterFromCustomProperty4))
			{
				string text2 = text;
				text = text2 + " AND " + matchmakingParameterFromCustomProperty4 + " != " + -1;
			}
		}
		string matchmakingParameterFromCustomProperty5 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.PrisonDay);
		if (!string.IsNullOrEmpty(matchmakingParameterFromCustomProperty5))
		{
			if (m_SelectedDayRange[0] != -1f && m_SelectedDayRange[0] == m_SelectedDayRange[1])
			{
				string text2 = text;
				text = text2 + " AND " + matchmakingParameterFromCustomProperty5 + " = " + (int)m_SelectedDayRange[0];
			}
			else
			{
				if (m_SelectedDayRange[0] != -1f)
				{
					string text2 = text;
					text = text2 + " AND " + matchmakingParameterFromCustomProperty5 + " >= " + (int)m_SelectedDayRange[0];
				}
				if (m_SelectedDayRange[1] != -1f)
				{
					string text2 = text;
					text = text2 + " AND " + matchmakingParameterFromCustomProperty5 + " <= " + (int)m_SelectedDayRange[1];
				}
			}
		}
		string matchmakingParameterFromCustomProperty6 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.RoomType);
		if (matchmakingParameterFromCustomProperty6 != null)
		{
			string text2 = text;
			text = text2 + " AND " + matchmakingParameterFromCustomProperty6 + " = " + 1;
		}
		if (m_HostName != string.Empty && m_HostName.Trim() != string.Empty)
		{
			string matchmakingParameterFromCustomProperty7 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.HostName);
			if (matchmakingParameterFromCustomProperty7 != null)
			{
				string text2 = text;
				text = text2 + " AND " + matchmakingParameterFromCustomProperty7 + " LIKE \"%" + m_HostName + "%\"";
			}
		}
		string empty = string.Empty;
		empty = ((!T17NetConfig.FeatureCampaignMatchmaking) ? NetConnectAndJoinRoom.GenerateCampaignBrowseGamesLobbyName(PrisonConfig.ConfigType.Cooperative) : NetConnectAndJoinRoom.GenerateCampaignMatchmakingLobbyName(m_SelectedPrisonFilter));
		NetMatchmakingConfig instance = NetMatchmakingConfig.GetInstance(PrisonConfig.ConfigType.Cooperative);
		if (null != instance)
		{
			instance.m_strSearchPrefix = text;
			NetConnectAndJoinRoom.Init_OnlineMode_RoomSearchList(empty, text);
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_RoomSearchList);
		}
	}

	private void OnRoomListUpdated()
	{
		GetRooms();
	}

	private void DayFilter_IndexSelectedEvent(int index, SelectionDirections directionTravelledIn)
	{
		m_SelectedDayRange = m_DayFilters[index];
		m_RefetchTimer = 5f;
		if (m_LoadingIcon != null)
		{
			m_LoadingIcon.SetActive(value: true);
		}
		Search();
	}

	private void PrisonFilter_IndexSelectedEvent(int index, SelectionDirections directionTravelledIn)
	{
		m_SelectedPrisonFilter = m_LevelFilters[index];
		m_RefetchTimer = 5f;
		if (m_LoadingIcon != null)
		{
			m_LoadingIcon.SetActive(value: true);
		}
		Search();
	}

	private void PopulateFilters()
	{
		foreach (Vector2 dayFilter in m_DayFilters)
		{
			string localized;
			if (dayFilter.x != -1f || dayFilter.y != -1f)
			{
				localized = ((dayFilter.x == -1f || dayFilter.y != -1f) ? (dayFilter.x + " - " + dayFilter.y) : (dayFilter.x + "+"));
			}
			else
			{
				Localization.Get("Text.Prison.Any", out localized);
			}
			if (m_DayFilter != null && m_DayFilter.m_Options != null)
			{
				m_DayFilter.m_Options.Add(localized);
			}
		}
		LevelScript.PRISON_ENUM[] levelFilters = m_LevelFilters;
		for (int i = 0; i < levelFilters.Length; i++)
		{
			int num = (int)levelFilters[i];
			bool flag = false;
			string text;
			switch (num)
			{
			case 0:
				text = "Text.Prison.Any";
				flag = true;
				break;
			case -1:
				text = "Text.Prison.Custom";
				flag = true;
				break;
			default:
			{
				text = "Prison Error";
				PrisonData prisonDataForPrison = LevelDataManager.GetInstance().GetPrisonDataForPrison((LevelScript.PRISON_ENUM)num);
				if (prisonDataForPrison != null && (prisonDataForPrison.m_LevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Normal || prisonDataForPrison.m_LevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Transport))
				{
					text = prisonDataForPrison.m_NameLocalizationKey;
					flag = true;
				}
				break;
			}
			}
			if (flag && m_PrisonFilter != null && m_PrisonFilter.m_Options != null)
			{
				Localization.Get(text, out var localized2);
				m_PrisonFilter.m_Options.Add(localized2);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		m_RefetchTimer += Time.deltaTime;
		if (m_RoomInfoObjectPrefab != null && m_ScrollView != null && m_RefetchTimer >= 5f && NetConnectAndJoinRoom.GetRequestedConnectionState() == NetConnectionState.OnlineMode_RoomSearchList)
		{
			m_RefetchTimer = 0f;
			if (m_LoadingIcon != null)
			{
				m_LoadingIcon.SetActive(value: false);
			}
			Search();
		}
		if (m_InformationalText != null)
		{
			Localization.Get("Text.UI.SearchingAgainIn", out var localized);
			m_InformationalText.text = m_InfoString + "\n" + localized + ": " + (5f - m_RefetchTimer);
		}
	}

	private void GetRooms()
	{
		List<T17NetRoomListManager.NetPhotonRoom> lobbyRoomList = T17NetRoomListManager.Instance.GetLobbyRoomList();
		int comparisonMultiplier = ((!m_reverseOrdering) ? 1 : (-1));
		if (m_SelectedPrisonFilter == LevelScript.PRISON_ENUM.Unassigned)
		{
			LevelDataManager instance = LevelDataManager.GetInstance();
			for (int num = lobbyRoomList.Count - 1; num >= 0; num--)
			{
				if (instance != null && !instance.IsLevelAvailable(lobbyRoomList[num].PrisonEnum))
				{
					lobbyRoomList.RemoveAt(num);
				}
			}
		}
		lobbyRoomList.Sort((T17NetRoomListManager.NetPhotonRoom x, T17NetRoomListManager.NetPhotonRoom y) => m_Ordering switch
		{
			RoomOrderings.CurrentPlayers => x.NumPlayers.CompareTo(y.NumPlayers), 
			RoomOrderings.Days => x.RoomDays.CompareTo(y.RoomDays), 
			RoomOrderings.Prison => (x.PrisonEnum != LevelScript.PRISON_ENUM.CustomPrison) ? ((y.PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison) ? 1 : x.PrisonEnum.CompareTo(y.PrisonEnum)) : (-1), 
			_ => x.Name.CompareTo(y.Name), 
		} * comparisonMultiplier);
		if (lobbyRoomList.Count > m_MaxResults)
		{
			lobbyRoomList.RemoveRange(m_MaxResults, lobbyRoomList.Count - m_MaxResults);
		}
		if (lobbyRoomList != null)
		{
			m_InfoString = $"Rooms: {lobbyRoomList.Count} MS-P:{T17NetManager.CountOfPlayersOnMaster} GS-P={T17NetManager.CountOfPlayersOnServer}";
			UpdateUIWithRoomList(lobbyRoomList);
			if (m_NoGamesMessage != null)
			{
				m_NoGamesMessage.gameObject.SetActive(lobbyRoomList.Count == 0);
			}
		}
		else
		{
			m_InfoString = "No Rooms Found";
			if (m_NoGamesMessage != null)
			{
				m_NoGamesMessage.gameObject.SetActive(value: true);
			}
		}
	}

	private void UpdateUIWithRoomList(List<T17NetRoomListManager.NetPhotonRoom> roomList)
	{
		int i = 0;
		for (int count = m_CurrentAttachedInfoObjects.Count; i < count; i++)
		{
			LobbyRoomInfoObject lobbyRoomInfoObject = m_CurrentAttachedInfoObjects[i];
			lobbyRoomInfoObject.OnSelectedRoomEvent = (LobbyRoomInfoObject.OnSelectedRoom)Delegate.Remove(lobbyRoomInfoObject.OnSelectedRoomEvent, new LobbyRoomInfoObject.OnSelectedRoom(SetSelectedLobbyInfo));
		}
		int j;
		for (j = 0; j < roomList.Count; j++)
		{
			if (j < m_CurrentAttachedInfoObjects.Count)
			{
				m_CurrentAttachedInfoObjects[j].gameObject.SetActive(value: true);
				m_CurrentAttachedInfoObjects[j].SetRoomInfo(roomList[j], j);
				LobbyRoomInfoObject lobbyRoomInfoObject2 = m_CurrentAttachedInfoObjects[j];
				lobbyRoomInfoObject2.OnSelectedRoomEvent = (LobbyRoomInfoObject.OnSelectedRoom)Delegate.Combine(lobbyRoomInfoObject2.OnSelectedRoomEvent, new LobbyRoomInfoObject.OnSelectedRoom(SetSelectedLobbyInfo));
				continue;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(m_RoomInfoObjectPrefab);
			m_ScrollView.AddNewObject(gameObject);
			LobbyRoomInfoObject component = gameObject.GetComponent<LobbyRoomInfoObject>();
			if (component != null)
			{
				component.SetRoomInfo(roomList[j], j);
				component.m_ActionOnSelect = m_OnRoomJoinActions;
				component.OnSelectedRoomEvent = (LobbyRoomInfoObject.OnSelectedRoom)Delegate.Combine(component.OnSelectedRoomEvent, new LobbyRoomInfoObject.OnSelectedRoom(SetSelectedLobbyInfo));
				m_CurrentAttachedInfoObjects.Add(component);
			}
			T17Button component2 = gameObject.GetComponent<T17Button>();
			if (component2 != null)
			{
				component2.SetGamerForEventSystem(base.CurrentGamer, base.CachedEventSystem);
			}
		}
		for (; j < m_CurrentAttachedInfoObjects.Count; j++)
		{
			m_CurrentAttachedInfoObjects[j].gameObject.SetActive(value: false);
		}
	}

	private void SetSelectedLobbyInfo(LobbyRoomInfoObject lobby)
	{
		m_PasswordDialog.SetCurrentLobbyRoomInfo(lobby);
		m_PasswordDialog.Show(Gamer.GetPrimaryGamer(), this, null, hideInvoker: false);
	}

	public override void Close()
	{
		Hide();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		FrontendUserPath.RecordFrontendPath(m_bEditorMenu ? FrontEndFlow.MenuType.LevelEditor : FrontEndFlow.MenuType.GameFrontend, (!m_bEditorMenu) ? 3 : 2, 0);
		if (m_ScrollView != null)
		{
			m_ScrollView.Show(currentGamer, this, null, hideInvoker: false);
		}
		if (m_LoadingIcon != null)
		{
			m_LoadingIcon.SetActive(value: true);
		}
		Search();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		int i = 0;
		for (int count = m_CurrentAttachedInfoObjects.Count; i < count; i++)
		{
			LobbyRoomInfoObject lobbyRoomInfoObject = m_CurrentAttachedInfoObjects[i];
			lobbyRoomInfoObject.OnSelectedRoomEvent = (LobbyRoomInfoObject.OnSelectedRoom)Delegate.Remove(lobbyRoomInfoObject.OnSelectedRoomEvent, new LobbyRoomInfoObject.OnSelectedRoom(SetSelectedLobbyInfo));
		}
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	public void OnKeyRetrievelError()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
	}
}
