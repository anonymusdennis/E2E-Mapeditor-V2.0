using System;
using System.Collections.Generic;
using UnityEngine;

public class PIPManager : MonoBehaviour
{
	public enum PIPEventType
	{
		MedicCalled,
		MaintenanceCalled,
		VisitorArriving,
		GuardAlert,
		GuardAlertDamagedPrison,
		GuardAlertDesk,
		GuardGoingPlayerDesk
	}

	[Serializable]
	public class EventMapping
	{
		public PIPEventType m_EventType;

		public Texture m_Image;

		public string m_Sfx;

		public string m_LocalizationTag;
	}

	[Serializable]
	public class EventInfo
	{
		public PIPEventType m_EventType;

		public float m_EventTime;

		public Character m_TargetCharacter;

		public bool m_PlayedSfx;

		public int eventID;
	}

	private static PIPManager m_Instance;

	private int nextEventID;

	private List<EventInfo>[] m_GlobalPIPEventLists = new List<EventInfo>[3];

	private List<EventInfo>[] m_Player1PIPEventLists = new List<EventInfo>[3];

	private List<EventInfo>[] m_Player2PIPEventLists = new List<EventInfo>[3];

	private List<EventInfo>[] m_Player3PIPEventLists = new List<EventInfo>[3];

	private List<EventInfo>[] m_Player4PIPEventLists = new List<EventInfo>[3];

	private Texture[] m_EventImageLookup;

	private string[] m_EventSfxLookup;

	private string[] m_LocalizationLookup;

	public float m_DefaultEventTimeLimit = 10f;

	public HUDMenuFlow m_HUDFlow;

	public T17NetView m_NetView;

	public EventMapping[] m_EventMapping;

	public static PIPManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		InitialiseEvents(m_GlobalPIPEventLists);
		InitialiseEvents(m_Player1PIPEventLists);
		InitialiseEvents(m_Player2PIPEventLists);
		InitialiseEvents(m_Player3PIPEventLists);
		InitialiseEvents(m_Player4PIPEventLists);
		m_Instance = this;
	}

	private void InitialiseEvents(List<EventInfo>[] eventLists)
	{
		for (int i = 0; i < eventLists.Length; i++)
		{
			eventLists[i] = new List<EventInfo>();
		}
	}

	private void Start()
	{
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
			if (m_NetView == null)
			{
				Debug.LogErrorFormat("Failed to find NetView for PIPManager", "PIP");
			}
		}
		if (m_NetView != null && m_NetView.viewID != T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.HUDParentView))
		{
			m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.HUDParentView);
		}
		if (m_EventMapping != null && m_EventMapping.Length > 0)
		{
			int length = Enum.GetValues(typeof(PIPEventType)).Length;
			m_EventImageLookup = new Texture[length];
			m_EventSfxLookup = new string[length];
			m_LocalizationLookup = new string[length];
			for (int i = 0; i < m_EventMapping.Length; i++)
			{
				m_EventImageLookup[(int)m_EventMapping[i].m_EventType] = m_EventMapping[i].m_Image;
				m_EventSfxLookup[(int)m_EventMapping[i].m_EventType] = m_EventMapping[i].m_Sfx;
				m_LocalizationLookup[(int)m_EventMapping[i].m_EventType] = m_EventMapping[i].m_LocalizationTag;
			}
		}
	}

	private void Update()
	{
		if (!PIPUpdate(m_GlobalPIPEventLists, -1))
		{
			PIPUpdate(m_Player1PIPEventLists, 0);
			PIPUpdate(m_Player2PIPEventLists, 1);
			PIPUpdate(m_Player3PIPEventLists, 2);
			PIPUpdate(m_Player4PIPEventLists, 3);
		}
	}

	private bool PIPUpdate(List<EventInfo>[] eventInfoLists, int targetPIP)
	{
		EventInfo eventInfo = null;
		for (int i = 0; i < eventInfoLists.Length; i++)
		{
			List<EventInfo> list = eventInfoLists[i];
			if (list.Count > 0)
			{
				for (int num = list.Count - 1; num >= 0; num--)
				{
					list[num].m_EventTime -= UpdateManager.deltaTime;
					if (list[num].m_EventTime <= 0f)
					{
						list.RemoveAt(num);
					}
				}
				if (eventInfo == null && list.Count > 0 && list[0] != null)
				{
					eventInfo = list[0];
				}
			}
			CameraManager instance = CameraManager.GetInstance();
			if (!(instance != null) || instance.m_PIPBindings.Length <= 0)
			{
				continue;
			}
			targetPIP = Mathf.Clamp(targetPIP, -1, instance.m_PIPBindings.Length - 2);
			CameraManager.CameraBinding cameraBinding = instance.m_PIPBindings[targetPIP + 1];
			if (eventInfoLists[i].Count > 0)
			{
				cameraBinding.m_Character = eventInfo.m_TargetCharacter;
				Texture texture = null;
				if (m_EventImageLookup != null)
				{
					texture = m_EventImageLookup[(int)eventInfo.m_EventType];
				}
				string localizationTag = null;
				if (m_LocalizationLookup != null)
				{
					localizationTag = m_LocalizationLookup[(int)eventInfo.m_EventType];
				}
				string text = null;
				if (m_EventSfxLookup != null)
				{
					text = m_EventSfxLookup[(int)eventInfo.m_EventType];
				}
				if (targetPIP >= 0 && targetPIP <= 4)
				{
					if (m_HUDFlow.m_PlayersHUDData.Count <= targetPIP || !(m_HUDFlow.m_PlayersHUDData[targetPIP].m_PIPImage != null))
					{
						break;
					}
					HUDMenuFlow.PlayerHUDData playerHUDData = m_HUDFlow.m_PlayersHUDData[targetPIP];
					if (playerHUDData == null || !(texture != null))
					{
						break;
					}
					playerHUDData.m_PIPImage.texture = texture;
					playerHUDData.m_PIPImage.gameObject.SetActive(value: true);
					playerHUDData.m_MiniMapParent.BeginPIPDisplay(localizationTag);
					if (!eventInfo.m_PlayedSfx)
					{
						eventInfo.m_PlayedSfx = true;
						if (!string.IsNullOrEmpty(text))
						{
							AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, text, AudioController.UI_Audio_GO);
						}
					}
					return true;
				}
				for (int j = 0; j < m_HUDFlow.m_PlayersHUDData.Count; j++)
				{
					if (!(m_HUDFlow.m_PlayersHUDData[j].m_PIPImage != null) || !(texture != null))
					{
						continue;
					}
					m_HUDFlow.m_PlayersHUDData[j].m_PIPImage.texture = texture;
					m_HUDFlow.m_PlayersHUDData[j].m_PIPImage.gameObject.SetActive(value: true);
					m_HUDFlow.m_PlayersHUDData[j].m_MiniMapParent.BeginPIPDisplay(localizationTag);
					if (!eventInfo.m_PlayedSfx)
					{
						eventInfo.m_PlayedSfx = true;
						if (!string.IsNullOrEmpty(text))
						{
							AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, text, AudioController.UI_Audio_GO);
						}
					}
					return true;
				}
				break;
			}
			if (!(cameraBinding.m_Character != null))
			{
				continue;
			}
			cameraBinding.m_Character = null;
			cameraBinding.m_Camera.enabled = false;
			if (targetPIP < 0)
			{
				InGameMenuFlow instance2 = InGameMenuFlow.Instance;
				for (int k = 0; k < m_HUDFlow.m_PlayersHUDData.Count; k++)
				{
					m_HUDFlow.m_PlayersHUDData[k].m_PIPImage.gameObject.SetActive(value: false);
					m_HUDFlow.m_PlayersHUDData[k].m_MiniMapParent.EndPIPDisplay();
					if (instance2 != null)
					{
						MainMapMenu mapForPlayer = instance2.GetMapForPlayer(m_HUDFlow.m_PlayersHUDData[k].m_PlayerBindingID);
						if (mapForPlayer != null && mapForPlayer.m_RoutineTracker != null)
						{
							mapForPlayer.m_RoutineTracker.EndPIPDisplay();
						}
					}
				}
				continue;
			}
			m_HUDFlow.m_PlayersHUDData[targetPIP].m_PIPImage.gameObject.SetActive(value: false);
			m_HUDFlow.m_PlayersHUDData[targetPIP].m_MiniMapParent.EndPIPDisplay();
			InGameMenuFlow instance3 = InGameMenuFlow.Instance;
			if (instance3 != null)
			{
				MainMapMenu mapForPlayer2 = instance3.GetMapForPlayer(m_HUDFlow.m_PlayersHUDData[targetPIP].m_PlayerBindingID);
				if (mapForPlayer2 != null && mapForPlayer2.m_RoutineTracker != null)
				{
					mapForPlayer2.m_RoutineTracker.EndPIPDisplay();
				}
			}
		}
		return false;
	}

	private void NewEvent(PIPEventType eventType, Character targetCharacter, int priority, int eventID, float duration)
	{
		EventInfo eventInfo = new EventInfo();
		eventInfo.m_EventType = eventType;
		eventInfo.m_TargetCharacter = targetCharacter;
		if (duration == 0f)
		{
			eventInfo.m_EventTime = m_DefaultEventTimeLimit;
		}
		else
		{
			eventInfo.m_EventTime = duration;
		}
		eventInfo.m_PlayedSfx = false;
		eventInfo.eventID = eventID;
		nextEventID = ++eventID;
		int num = Mathf.Clamp(priority, 0, m_GlobalPIPEventLists.Length - 1);
		m_GlobalPIPEventLists[num].Add(eventInfo);
	}

	private int NewEvent(PIPEventType eventType, Character targetCharacter, CameraManager.PlayerBindingID playerBindingID, int priority, int eventID, float duration)
	{
		EventInfo eventInfo = new EventInfo();
		eventInfo.m_EventType = eventType;
		eventInfo.m_TargetCharacter = targetCharacter;
		if (duration == 0f)
		{
			eventInfo.m_EventTime = m_DefaultEventTimeLimit;
		}
		else
		{
			eventInfo.m_EventTime = duration;
		}
		eventInfo.m_PlayedSfx = false;
		eventInfo.eventID = eventID;
		switch (playerBindingID)
		{
		case CameraManager.PlayerBindingID.CM_PBID_UNSET:
		{
			int num = Mathf.Clamp(priority, 0, m_GlobalPIPEventLists.Length - 1);
			m_GlobalPIPEventLists[num].Add(eventInfo);
			break;
		}
		case CameraManager.PlayerBindingID.CM_PBID_PLAYER_ALPHA:
			if (CameraManager.GetInstance().GetUsedCameraCount() >= 1)
			{
				int num = Mathf.Clamp(priority, 0, m_Player1PIPEventLists.Length - 1);
				m_Player1PIPEventLists[num].Add(eventInfo);
			}
			break;
		case CameraManager.PlayerBindingID.CM_PBID_PLAYER_BETA:
			if (CameraManager.GetInstance().GetUsedCameraCount() >= 2)
			{
				int num = Mathf.Clamp(priority, 0, m_Player1PIPEventLists.Length - 1);
				m_Player2PIPEventLists[num].Add(eventInfo);
			}
			break;
		case CameraManager.PlayerBindingID.CM_PBID_PLAYER_GAMMA:
			if (CameraManager.GetInstance().GetUsedCameraCount() >= 3)
			{
				int num = Mathf.Clamp(priority, 0, m_Player1PIPEventLists.Length - 1);
				m_Player3PIPEventLists[num].Add(eventInfo);
			}
			break;
		case CameraManager.PlayerBindingID.CM_PBID_PLAYER_DELTA:
			if (CameraManager.GetInstance().GetUsedCameraCount() >= 4)
			{
				int num = Mathf.Clamp(priority, 0, m_Player1PIPEventLists.Length - 1);
				m_Player4PIPEventLists[num].Add(eventInfo);
			}
			break;
		}
		return eventInfo.eventID;
	}

	private void EndEvent(int targetEventID)
	{
		if (!EndEvent(m_GlobalPIPEventLists, targetEventID) && !EndEvent(m_Player1PIPEventLists, targetEventID) && !EndEvent(m_Player2PIPEventLists, targetEventID) && !EndEvent(m_Player3PIPEventLists, targetEventID) && !EndEvent(m_Player4PIPEventLists, targetEventID))
		{
		}
	}

	private bool EndEvent(List<EventInfo>[] eventList, int targetEventID)
	{
		for (int i = 0; i < eventList.Length; i++)
		{
			for (int j = 0; j < eventList[i].Count; j++)
			{
				if (eventList[i][j].eventID == targetEventID)
				{
					eventList[i].Remove(eventList[i][j]);
					return true;
				}
			}
		}
		return false;
	}

	public int NewGlobalPIPEvent(PIPEventType eventType, int targetCharacterNetID, int priority, float duration = 0f)
	{
		nextEventID++;
		int num = nextEventID;
		if (m_NetView != null)
		{
			m_NetView.PostLevelLoadRPC("RPC_GlobalPIP", NetTargets.All, (int)eventType, targetCharacterNetID, priority, num, duration);
		}
		return num;
	}

	[PunRPC]
	public void RPC_GlobalPIP(int eventType, int characterTargetID, int priority, int eventID, float duration, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterTargetID);
		if (!(character == null))
		{
			NewEvent((PIPEventType)eventType, character, priority, eventID, duration);
		}
	}

	public int NewPlayerPIPEvent(PIPEventType eventType, int targetPlayerNetID, int targetCharacterNetID, int priority, float duration = 0f)
	{
		nextEventID++;
		int num = nextEventID;
		T17NetView target = T17NetView.Find<T17NetView>(targetPlayerNetID);
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_PlayerPIP", target, (int)eventType, targetPlayerNetID, targetCharacterNetID, priority, num, duration);
		}
		return num;
	}

	[PunRPC]
	public void RPC_PlayerPIP(int eventType, int playerTargetID, int characterTargetID, int priority, int eventID, float duration, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterTargetID);
		if (character == null)
		{
			return;
		}
		Gamer gamerByViewID = Gamer.GetGamerByViewID(playerTargetID);
		if (gamerByViewID != null)
		{
			Player playerObject = gamerByViewID.m_PlayerObject;
			if (!(playerObject == null))
			{
				CameraManager.PlayerBindingID playerCameraManagerBindingID = playerObject.m_PlayerCameraManagerBindingID;
				NewEvent((PIPEventType)eventType, character, playerCameraManagerBindingID, priority, eventID, duration);
			}
		}
	}

	public void EndPIPEvent(Player targetPlayer, int eventID)
	{
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_EndEvent", targetPlayer.m_NetView, eventID);
		}
	}

	public void EndPIPEvent(int eventID)
	{
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_EndEvent", NetTargets.All, eventID);
		}
	}

	[PunRPC]
	public void RPC_EndEvent(int eventID, PhotonMessageInfo info)
	{
		EndEvent(eventID);
	}

	public static void RunPIPRandomCharacter()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		Character[] array = UnityEngine.Object.FindObjectsOfType<Character>();
		foreach (Player item in allPlayers)
		{
			Character character = array[UnityEngine.Random.Range(0, array.Length)];
			GetInstance().NewPlayerPIPEvent(PIPEventType.GuardAlert, item.m_NetView.viewID, character.m_NetView.viewID, 100000, 10f);
			SpeechManager.GetInstance().SaySomething(character, "Look ma I'm on TV", SpeechTone.Positive, 10f, 10000);
		}
	}

	protected virtual void OnDestroy()
	{
		m_Instance = null;
		m_NetView = null;
	}
}
