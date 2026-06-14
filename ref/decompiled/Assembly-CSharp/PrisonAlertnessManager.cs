using System;
using System.Collections.Generic;
using NetworkLoadable;
using UnityEngine;

public class PrisonAlertnessManager : MonoBehaviour, IDeserializable, Saveable, INetworkLoadable
{
	public delegate void AlertnessChanged(PrisonAlertness alertness);

	public enum AlertnessReason
	{
		UNASSIGNED,
		MissedRoutine,
		CharacterBound,
		NaughtyLocation,
		StandingOnDesk,
		Naked,
		HasContraband,
		Digging,
		Chipping,
		Cutting,
		Looting,
		CarryingObject,
		AttackingInmate,
		AttackingGuard,
		ContrabandOnFloor,
		ContrabandInContainer,
		DamagedTile,
		MissingTile,
		DugHole,
		Flooded,
		SearchingDesk,
		Escaping,
		Tardy,
		Disguised,
		ItemMissing,
		OutDuringLightsOut,
		FromMasterClient
	}

	[Serializable]
	public struct AlertReasonLoc
	{
		public AlertnessReason m_Reason;

		public string m_LocalisationTag;
	}

	[Serializable]
	public class NetSaveData
	{
		public short Alertness;
	}

	public static PrisonAlertnessManager m_Instance;

	public int m_MorningRollCallReduction = 1;

	public PrisonAlertness m_StartingAlertness;

	private T17NetView m_NetView;

	private PrisonAlertness m_eAlertness;

	private NetSaveData m_NetSaveData = new NetSaveData();

	private SaveDataRegister m_SaveData;

	public string m_AlertnessRaisedLocalisation;

	public string m_AlertnessDecreasedLocalisation;

	public string m_LockdownCausedLocalisation;

	public string m_CharacterResponsibleToken;

	public RoutinesData.Routine m_FirstRollcallRoutine;

	public List<AlertReasonLoc> m_ReasonLocalisation = new List<AlertReasonLoc>();

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public event AlertnessChanged OnPrisonAlertnessChanged;

	public static PrisonAlertnessManager GetInstance()
	{
		return m_Instance;
	}

	public PrisonAlertness GetCurrentAlertness()
	{
		return m_eAlertness;
	}

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
			if (T17NetManager.IsMasterClient)
			{
				m_eAlertness = m_StartingAlertness;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Start()
	{
		if (m_Instance == this)
		{
			RoutineManager.GetInstance().OnRoutineChanged += RoutineChanged;
			m_FirstRollcallRoutine = RoutineManager.GetInstance().GetFirstRoutineOfType(Routines.RollCall);
			AudioController.RegisterToPrisonAlertnessManager();
			AudioController.Instance.OnPrisonAlertnessChanged(m_StartingAlertness);
		}
		if (null == m_NetView)
		{
			m_NetView = GetComponent<T17NetView>();
		}
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 10);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			if (RoutineManager.GetInstance() != null)
			{
				RoutineManager.GetInstance().OnRoutineChanged -= RoutineChanged;
			}
			AudioController.UnregisterToPrisonAlertnessManager();
			m_Instance = null;
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		m_NetView = null;
	}

	public string CreateSnapshot()
	{
		Serialize();
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public void StartedFromSnapshot()
	{
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public bool Deserialize(string serializedData, ref string error)
	{
		if (string.IsNullOrEmpty(serializedData))
		{
			return true;
		}
		NetSaveData netSaveData = null;
		try
		{
			netSaveData = JsonUtility.FromJson<NetSaveData>(serializedData);
		}
		catch
		{
			error = "Room Manager could not parse JSON data, it is corrupt.";
			return false;
		}
		if (netSaveData == null)
		{
			error = "PrisonAlertnessManager: JSON data returned null.";
			return false;
		}
		return DeserializeBinary(netSaveData, ref error);
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (T17NetManager.IsMasterClient && !player.IsLocal)
		{
			if (m_LoadState == LOADSTATE.Finished_OK)
			{
				m_NetView.RPC("RPC_LoadPrisonAlertness", player, (byte)m_eAlertness);
			}
			else
			{
				m_NetView.RPC("RPC_RequestStateResponce_No_PrisonAlertnessManager", player);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_PrisonAlertnessManager(PhotonMessageInfo info)
	{
		m_LoadError = "RoomManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	private void SetPrisonAlertnessRPC(PrisonAlertness alertness)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_NetView.PostLevelLoadRPC("RPC_SetPrisonAlertness", NetTargets.All, (byte)alertness);
		}
	}

	[PunRPC]
	private void RPC_SetPrisonAlertness(byte alertness, PhotonMessageInfo info)
	{
		m_eAlertness = (PrisonAlertness)alertness;
		if (this.OnPrisonAlertnessChanged != null)
		{
			this.OnPrisonAlertnessChanged(m_eAlertness);
		}
	}

	[PunRPC]
	private void RPC_LoadPrisonAlertness(byte alertness, PhotonMessageInfo info)
	{
		RPC_SetPrisonAlertness(alertness, info);
		m_LoadState = LOADSTATE.Finished_OK;
	}

	public PrisonAlertness IncrementAlertnessBy(int incrementVal, Character characterResponsible, AIEvent.EventType eventType, bool punishCharacter = true)
	{
		return IncrementAlertnessBy(incrementVal, characterResponsible, AIEventTypeToAlertnessReason(eventType), punishCharacter);
	}

	public PrisonAlertness IncrementAlertnessBy(int incrementVal, Character characterResponsible, AlertnessReason reason, bool punishCharacter = true)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return m_eAlertness;
		}
		if (characterResponsible == null)
		{
			return m_eAlertness;
		}
		if (characterResponsible.m_CharacterRole != 0)
		{
			return m_eAlertness;
		}
		int num = (int)m_eAlertness + incrementVal;
		bool flag = punishCharacter && incrementVal > 0 && num >= 10;
		if (flag)
		{
			SolitaryManager.GetInstance().SetWantedForSolitary(characterResponsible);
		}
		if (characterResponsible.m_CharacterStats.m_bIsPlayer && !characterResponsible.GetIsDisabled())
		{
			if (flag)
			{
				SolitaryManager.GetInstance().TriggerLockdown();
			}
			if (incrementVal > 0 || flag)
			{
				ChatFeedManager instance = ChatFeedManager.GetInstance();
				Player player = (Player)characterResponsible;
				if (instance != null && player != null && player.m_Gamer != null && T17NetManager.IsMasterClient && num <= 11)
				{
					string localisationForAlertnessReason = GetLocalisationForAlertnessReason(reason);
					string localizationTagMain = ((!flag) ? m_AlertnessRaisedLocalisation : m_LockdownCausedLocalisation);
					instance.SendAlertnessMessage_RPC(localizationTagMain, localisationForAlertnessReason, m_CharacterResponsibleToken, player.m_NetView.viewID, ChatFeedManager.MessageTag.Prison);
				}
			}
			IncrementAlertness_Internal(incrementVal);
		}
		return m_eAlertness;
	}

	public PrisonAlertness IncrementAlertnessBy(int incrementVal, AlertnessReason reason)
	{
		IncrementAlertness_Internal(incrementVal);
		ChatFeedManager instance = ChatFeedManager.GetInstance();
		if (instance != null)
		{
			int num = (int)m_eAlertness + incrementVal;
			bool flag = incrementVal > 0 && num >= 10;
			string localisationForAlertnessReason = GetLocalisationForAlertnessReason(reason);
			string localizationTagMain = ((!flag) ? m_AlertnessRaisedLocalisation : m_LockdownCausedLocalisation);
			instance.SendAlertnessMessage_RPC(localizationTagMain, localisationForAlertnessReason, null, -1, ChatFeedManager.MessageTag.Prison);
		}
		return m_eAlertness;
	}

	private void IncrementAlertness_Internal(int incrementVal)
	{
		if (incrementVal > 0 && (int)m_eAlertness + incrementVal >= 6)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				if (allPlayers[i].m_Gamer != null)
				{
					TutorialManager instance = TutorialManager.GetInstance();
					if (instance != null && TutorialManager.GetInstance().CheckTutorialNeeded(allPlayers[i], TutorialSubject.Alertness))
					{
						TutorialManager.GetInstance().StartTutorialRPC(allPlayers[i], TutorialSubject.Alertness);
					}
				}
			}
		}
		int eAlertness = (int)m_eAlertness;
		eAlertness += incrementVal;
		eAlertness = Mathf.Clamp(eAlertness, 0, 10);
		if (incrementVal > 0 && eAlertness >= 10)
		{
			SolitaryManager.GetInstance().TriggerLockdown();
		}
		SetPrisonAlertnessRPC((PrisonAlertness)eAlertness);
	}

	public PrisonAlertness DecrementAlertnessBy(int decreaseAmount)
	{
		if (T17NetManager.IsMasterClient)
		{
			ChatFeedManager instance = ChatFeedManager.GetInstance();
			if (instance != null && m_eAlertness != 0)
			{
				instance.SendSystemMessageLocalized_RPC(m_AlertnessDecreasedLocalisation, ChatFeedManager.MessageTag.Prison);
			}
		}
		if (!T17NetManager.IsMasterClient)
		{
			return m_eAlertness;
		}
		if (SolitaryManager.GetInstance().IsLockdownActive())
		{
			return m_eAlertness;
		}
		int eAlertness = (int)m_eAlertness;
		eAlertness -= decreaseAmount;
		eAlertness = Mathf.Clamp(eAlertness, 0, 10);
		SetPrisonAlertnessRPC((PrisonAlertness)eAlertness);
		return m_eAlertness;
	}

	public bool AtFiveStars()
	{
		return (int)m_eAlertness >= 10;
	}

	public void SetAlertness_Quest(PrisonAlertness alertness)
	{
		if (m_NetView != null)
		{
			m_NetView.PostLevelLoadRPC("RPC_SetPrisonAlertness", NetTargets.MasterClient, (byte)alertness);
		}
	}

	private void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forced)
	{
		if (T17NetManager.IsMasterClient)
		{
			if (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.Lockdown)
			{
				int postLockdownAlertness = oldRoutine.m_PostLockdownAlertness;
				int decreaseAmount = (int)m_eAlertness - postLockdownAlertness;
				DecrementAlertnessBy(decreaseAmount);
			}
			else if (newRoutine == m_FirstRollcallRoutine && RoutineManager.GetInstance() != null && !RoutineManager.GetInstance().IsFirstRoutineAfterStart())
			{
				DecrementAlertnessBy(m_MorningRollCallReduction);
			}
		}
	}

	public void Serialize()
	{
		m_NetSaveData.Alertness = (short)m_eAlertness;
	}

	public bool DeserializeBinary(NetSaveData data, ref string error)
	{
		int alertness = data.Alertness;
		if (alertness > 11 || alertness < 0)
		{
			error = "Unexpected error when deserializing the Prison Alertness! Got value " + alertness;
			return false;
		}
		m_NetSaveData = data;
		SetPrisonAlertnessRPC((PrisonAlertness)alertness);
		return true;
	}

	public string GetLocalisationForAlertnessReason(AlertnessReason reason)
	{
		for (int i = 0; i < m_ReasonLocalisation.Count; i++)
		{
			if (reason == m_ReasonLocalisation[i].m_Reason)
			{
				return m_ReasonLocalisation[i].m_LocalisationTag;
			}
		}
		return string.Empty;
	}

	public static AlertnessReason AIEventTypeToAlertnessReason(AIEvent.EventType eventType)
	{
		return eventType switch
		{
			AIEvent.EventType.Character_Bound => AlertnessReason.CharacterBound, 
			AIEvent.EventType.Character_NaughtyLocation => AlertnessReason.NaughtyLocation, 
			AIEvent.EventType.Character_StandingOnDesk => AlertnessReason.StandingOnDesk, 
			AIEvent.EventType.Character_Naked => AlertnessReason.Naked, 
			AIEvent.EventType.Character_HasContraband => AlertnessReason.HasContraband, 
			AIEvent.EventType.Character_Digging => AlertnessReason.Digging, 
			AIEvent.EventType.Character_Chipping => AlertnessReason.Chipping, 
			AIEvent.EventType.Character_Cutting => AlertnessReason.Cutting, 
			AIEvent.EventType.Character_Looting => AlertnessReason.Looting, 
			AIEvent.EventType.Character_CarryingObject => AlertnessReason.CarryingObject, 
			AIEvent.EventType.Item_ContrabandOnFloor => AlertnessReason.ContrabandOnFloor, 
			AIEvent.EventType.Item_ContrabandInContainer => AlertnessReason.ContrabandInContainer, 
			AIEvent.EventType.Tile_DamagedTile => AlertnessReason.DamagedTile, 
			AIEvent.EventType.Tile_MissingTile => AlertnessReason.MissingTile, 
			AIEvent.EventType.Tile_DugHole => AlertnessReason.DugHole, 
			AIEvent.EventType.Tile_Flooded => AlertnessReason.Flooded, 
			AIEvent.EventType.Character_SearchingDesk => AlertnessReason.SearchingDesk, 
			AIEvent.EventType.Character_Escaping => AlertnessReason.Escaping, 
			AIEvent.EventType.Character_Tardy => AlertnessReason.Tardy, 
			AIEvent.EventType.Character_Disguised => AlertnessReason.Disguised, 
			AIEvent.EventType.ItemMissing => AlertnessReason.ItemMissing, 
			_ => AlertnessReason.UNASSIGNED, 
		};
	}

	public static int GetMaxAlertnessIncreaseForContrabandItems(List<Item> contrabandItems)
	{
		int num = 0;
		if (contrabandItems != null)
		{
			for (int num2 = contrabandItems.Count - 1; num2 >= 0; num2--)
			{
				if (contrabandItems[num2] != null && contrabandItems[num2].m_ItemData.IsContraband())
				{
					num = Mathf.Max(num, contrabandItems[num2].m_ItemData.m_AlertnessIncreaseWhenFound);
				}
			}
		}
		return num;
	}

	public static void IncreaseAlertnessForContrabandItems(List<Item> contrabandItems, AlertnessReason reason, Character characterResponsible, bool shouldPunishCharacter)
	{
		if (characterResponsible != null)
		{
			int maxAlertnessIncreaseForContrabandItems = GetMaxAlertnessIncreaseForContrabandItems(contrabandItems);
			if (maxAlertnessIncreaseForContrabandItems > 0)
			{
				GetInstance().IncrementAlertnessBy(maxAlertnessIncreaseForContrabandItems, characterResponsible, reason, shouldPunishCharacter);
			}
		}
		else
		{
			IncreaseAlertnessForContrabandItems(contrabandItems, reason);
		}
	}

	public static void IncreaseAlertnessForContrabandItems(List<Item> contrabandItems, AlertnessReason reason)
	{
		int maxAlertnessIncreaseForContrabandItems = GetMaxAlertnessIncreaseForContrabandItems(contrabandItems);
		if (maxAlertnessIncreaseForContrabandItems > 0)
		{
			GetInstance().IncrementAlertnessBy(maxAlertnessIncreaseForContrabandItems, reason);
		}
	}

	public static void IncreaseAlertnessForContrabandItem(Item item, AlertnessReason reason, Character characterResponsible, bool shouldPunishCharacter)
	{
		if (characterResponsible == null)
		{
			IncreaseAlertnessForContrabandItem(item, reason);
		}
		else if (item != null && item.m_ItemData.IsContraband() && item.m_ItemData.m_AlertnessIncreaseWhenFound > 0)
		{
			GetInstance().IncrementAlertnessBy(item.m_ItemData.m_AlertnessIncreaseWhenFound, characterResponsible, reason, shouldPunishCharacter);
		}
	}

	public static void IncreaseAlertnessForContrabandItem(Item item, AlertnessReason reason)
	{
		if (item != null && item.m_ItemData.IsContraband() && item.m_ItemData.m_AlertnessIncreaseWhenFound > 0)
		{
			GetInstance().IncrementAlertnessBy(item.m_ItemData.m_AlertnessIncreaseWhenFound, reason);
		}
	}
}
