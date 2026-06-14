using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AUTOGEN_T17Wwise_Enums;
using NetworkLoadable;
using Rewired;
using UnityEngine;

[Serializable]
public class SolitaryManager : T17MonoBehaviour, IDeserializable, Saveable, INetworkLoadable
{
	public delegate void CharacterSolitaryHandler(Character character, bool wanted);

	[Serializable]
	public class SolitarySetupInfo
	{
		public int TimesSentToSolitary;

		public float Duration;
	}

	[Serializable]
	private class SolitaryData
	{
		public Character Character;

		public RoomBlob Room;

		public float RemainingDuration;
	}

	public delegate void OnLockdownChanged(bool isLockdown);

	[Serializable]
	public class LockdownNetSaveData
	{
		public float TimestampLockdownEnd = -1f;
	}

	[Serializable]
	public class CharacterNetSaveData
	{
		public int Character = -1;

		public int RoomID = -1;

		public float RemainingTime;
	}

	[Serializable]
	public class SolitaryNetSaveData
	{
		public LockdownNetSaveData LockdownData = new LockdownNetSaveData();

		public List<CharacterNetSaveData> SolitaryData = new List<CharacterNetSaveData>();

		public List<int> MissingKeys = new List<int>();
	}

	public Platform.RumbleController m_LockdownRumble = new Platform.RumbleController();

	public Platform.LightBarEffect m_LockdownLight = new Platform.LightBarEffect();

	[Header("Lockdown Settings")]
	public int m_LockdownDuration = 180;

	public int m_MiniLockdownDuration = 45;

	[ReadOnly]
	public float m_fLockdownDurationRealtime;

	private int m_ActiveLockdownMaxDuration;

	[Header("Solitary Settings")]
	public ItemContainer m_FreeKeyContainer;

	public List<SolitarySetupInfo> m_SolitarySetupInfo = new List<SolitarySetupInfo>();

	public float m_TaskCompleteReduction;

	[ReadOnly]
	[SerializeField]
	[Header("Debug Output (readonly)")]
	private List<SolitaryData> m_SolitaryData = new List<SolitaryData>();

	private RoutineManager.CallbackInGameTimer m_LockdownTimer;

	private List<RoomBlob> m_SolitaryRooms = new List<RoomBlob>();

	private T17NetView m_NetView;

	private List<Item> m_TempItemList = new List<Item>();

	private bool m_bIsLockdownActive;

	private bool m_bShouldTriggerLockdown;

	private List<int> m_MissingKeys = new List<int>();

	private SolitaryNetSaveData m_NetSaveData = new SolitaryNetSaveData();

	private SaveDataRegister m_SaveData;

	private static SolitaryManager m_Instance;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public event CharacterSolitaryHandler CharacterWantedForSolitaryEvent;

	public event OnLockdownChanged onLockdownChanged;

	public static SolitaryManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		m_ActiveLockdownMaxDuration = m_LockdownDuration;
	}

	private void Start()
	{
		m_NetView = GetComponent<T17NetView>();
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 15);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_NetView = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_SolitaryRooms = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.Solitary);
		return base.StartInit();
	}

	private void Update()
	{
		if (!IsInited())
		{
			return;
		}
		if (m_bShouldTriggerLockdown)
		{
			m_bShouldTriggerLockdown = false;
			TriggerLockdown();
		}
		for (int i = 0; i < m_SolitaryData.Count; i++)
		{
			SolitaryData solitaryData = m_SolitaryData[i];
			if (!(solitaryData.RemainingDuration > 0f))
			{
				continue;
			}
			solitaryData.RemainingDuration -= UpdateManager.deltaTime;
			if (solitaryData.RemainingDuration < 0f)
			{
				solitaryData.RemainingDuration = 0f;
				if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
				{
					SetWantedForSolitary(solitaryData.Character, sendToSolitary: false);
				}
			}
		}
	}

	public void SetWantedForSolitary(Character character, bool sendToSolitary = true, bool bSurrendered = false)
	{
		if (sendToSolitary == IsWantedForSolitary(character) || !T17NetManager.IsMasterClient)
		{
			return;
		}
		if (!sendToSolitary)
		{
			UnassignCellFromCharacterRPC(character);
		}
		m_NetView.PostLevelLoadRPC("RPC_SetCharacterWantedForSolitary", NetTargets.All, character.m_NetView.viewID, sendToSolitary);
		if (!sendToSolitary)
		{
			return;
		}
		if (!bSurrendered)
		{
			character.m_CharacterStats.IncreaseHeat(100f);
		}
		NPCManager.GetInstance().RespondToKnownEscapeAttempt(character);
		character.m_CharacterStats.IncreaseTimesSentToSolitary();
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			RoomBlob freeCell = GetFreeCell();
			if (freeCell != null)
			{
				AssignCellToCharacterRPC(freeCell, character);
			}
		}
	}

	[PunRPC]
	private void RPC_SetCharacterWantedForSolitary(int characterID, bool wanted, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterID);
		if (character == null)
		{
			return;
		}
		if (wanted)
		{
			m_SolitaryData.Add(new SolitaryData
			{
				Character = character
			});
		}
		else
		{
			for (int num = m_SolitaryData.Count - 1; num >= 0; num--)
			{
				if (m_SolitaryData[num].Character == character)
				{
					m_SolitaryData.RemoveAt(num);
				}
			}
			character.ResetGetToRoutineTimer();
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				Player player = (Player)character;
				if (player.m_Gamer != null && player.m_Gamer.IsLocal())
				{
					player.CLIENT_StartedSolitaryConfinement(enteringSolitary: false);
				}
			}
		}
		character.SetIsWantedForSolitary(wanted);
		if (this.CharacterWantedForSolitaryEvent != null)
		{
			this.CharacterWantedForSolitaryEvent(character, wanted);
		}
	}

	private void StartSolitaryConfinement(Character character)
	{
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter == null)
		{
			return;
		}
		if (dataForCharacter.Room == null || m_SolitarySetupInfo.Count <= 0)
		{
			SetWantedForSolitary(character, sendToSolitary: false);
		}
		else
		{
			RoomBlob room = dataForCharacter.Room;
			if (room != null)
			{
				RoomBlob_Solitary roomBlobData = room.GetRoomBlobData<RoomBlob_Solitary>();
				if (roomBlobData != null)
				{
					roomBlobData.LockToCharacter(character);
				}
			}
			int num = 0;
			for (int i = 1; i < m_SolitarySetupInfo.Count && character.m_CharacterStats.TimesSentToSolitary >= m_SolitarySetupInfo[i].TimesSentToSolitary; i++)
			{
				num++;
			}
			SolitarySetupInfo solitarySetupInfo = null;
			num = Mathf.Clamp(num, 0, m_SolitarySetupInfo.Count - 1);
			if (num >= 0 && num < m_SolitarySetupInfo.Count)
			{
				solitarySetupInfo = m_SolitarySetupInfo[num];
			}
			if (solitarySetupInfo != null)
			{
				m_NetView.RPC("RPC_StartSolitaryConfinement", NetTargets.All, character.m_NetView.viewID, solitarySetupInfo.Duration);
			}
		}
		LockdownEndCheck();
	}

	[PunRPC]
	private void RPC_StartSolitaryConfinement(int characterID, float duration, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterID);
		if (!(character != null))
		{
			return;
		}
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)character;
			if (null != player && player.m_Gamer != null && player.m_Gamer.IsLocal())
			{
				player.CLIENT_StartedSolitaryConfinement(enteringSolitary: true);
			}
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Solitary_Sting, character.gameObject);
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter != null)
		{
			dataForCharacter.RemainingDuration = duration;
		}
	}

	private void LockdownEndCheck()
	{
		if (!IsLockdownActive())
		{
			return;
		}
		bool flag = true;
		bool flag2 = m_MissingKeys.Count == 0;
		if (!flag2)
		{
		}
		for (int i = 0; i < m_SolitaryData.Count; i++)
		{
			flag = !m_SolitaryData[i].Character.m_CharacterStats.m_bIsPlayer || m_SolitaryData[i].RemainingDuration > 0f;
			if (!flag)
			{
				break;
			}
		}
		if (flag && flag2)
		{
			SetLockdownActive(active: false);
		}
	}

	public void OnTaskCompleted(Character character)
	{
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter != null)
		{
			ReduceSolitaryTimeRPC(character, m_TaskCompleteReduction);
		}
	}

	public void ReduceSolitaryTimeRPC(Character character, float reduction)
	{
		if (character != null && reduction > 0f)
		{
			m_NetView.RPC("RPC_ReduceSolitaryTime", NetTargets.All, character.m_NetView.viewID, m_TaskCompleteReduction);
		}
	}

	[PunRPC]
	private void RPC_ReduceSolitaryTime(int characterID, float reduction)
	{
		Character character = T17NetView.Find<Character>(characterID);
		if (character == null)
		{
			return;
		}
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter == null || !(dataForCharacter.RemainingDuration > 0f))
		{
			return;
		}
		dataForCharacter.RemainingDuration -= reduction;
		if (dataForCharacter.RemainingDuration <= 0f)
		{
			dataForCharacter.RemainingDuration = 0f;
			if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
			{
				SetWantedForSolitary(dataForCharacter.Character, sendToSolitary: false);
			}
		}
	}

	public void LockToCharacterRPC(Character character, RoomBlob_Solitary room)
	{
		if (!(character == null) && !(room == null))
		{
			m_NetView.RPC("RPC_LockToCharacter", NetTargets.MasterClient, character.m_NetView.viewID);
		}
	}

	[PunRPC]
	private void RPC_LockToCharacter(int characterID, PhotonMessageInfo info)
	{
		if (m_FreeKeyContainer == null)
		{
			return;
		}
		Character character = T17NetView.Find<Character>(characterID);
		if (!(character != null))
		{
			return;
		}
		character.m_ItemContainer.GetHiddenItems(ref m_TempItemList);
		for (int i = 0; i < m_TempItemList.Count; i++)
		{
			for (int j = 0; j < m_FreeKeyContainer.m_TrackedItems.Count; j++)
			{
				if (m_TempItemList[i].ItemDataID == m_FreeKeyContainer.m_TrackedItems[j].m_ItemDataID)
				{
					character.m_ItemContainer.MoveItemToAnotherContainerRPC(m_TempItemList[i].m_NetView.viewID, m_FreeKeyContainer.NetView.viewID);
					break;
				}
			}
		}
	}

	public void UnlockForCharacterRPC(Character character, RoomBlob_Solitary room)
	{
		if (!(character == null) && !(room == null))
		{
			m_NetView.RPC("RPC_UnlockForCharacter", NetTargets.MasterClient, character.m_NetView.viewID);
		}
	}

	[PunRPC]
	private void RPC_UnlockForCharacter(int characterID, PhotonMessageInfo info)
	{
		if (m_FreeKeyContainer == null)
		{
			return;
		}
		Character character = T17NetView.Find<Character>(characterID);
		if (character != null)
		{
			bool flag = false;
			Item firstItemWithItemFunctionality = m_FreeKeyContainer.GetFirstItemWithItemFunctionality(BaseItemFunctionality.Functionality.Key);
			if (firstItemWithItemFunctionality != null)
			{
				m_FreeKeyContainer.MoveItemToAnotherContainerRPC(firstItemWithItemFunctionality.m_NetView.viewID, character.m_ItemContainer.NetView.viewID, bInToHidden: true);
				flag = true;
			}
			if (flag)
			{
			}
		}
	}

	private RoomBlob GetFreeCell()
	{
		RoomBlob roomBlob = null;
		RoomBlob roomBlob2 = null;
		for (int i = 0; i < m_SolitaryRooms.Count; i++)
		{
			if (m_SolitaryRooms[i] == null)
			{
				continue;
			}
			RoomBlob roomBlob3 = m_SolitaryRooms[i];
			RoomBlob_Solitary roomBlobData = m_SolitaryRooms[i].GetRoomBlobData<RoomBlob_Solitary>();
			if (roomBlobData != null && !roomBlobData.IsCellAssigned())
			{
				if (roomBlob3.m_CharactersInRoom.Count <= 0)
				{
					roomBlob = roomBlob3;
					break;
				}
				if (roomBlob2 == null)
				{
					roomBlob2 = roomBlob3;
				}
			}
		}
		if (roomBlob == null)
		{
			roomBlob = roomBlob2;
		}
		return roomBlob;
	}

	private bool AssignCellToCharacterRPC(RoomBlob cell, Character character)
	{
		if (cell == null || character == null)
		{
			return false;
		}
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			int iDForCell = GetIDForCell(cell);
			m_NetView.RPC("RPC_AssignCellToCharacter", NetTargets.All, iDForCell, character.m_NetView.viewID);
		}
		return true;
	}

	[PunRPC]
	private void RPC_AssignCellToCharacter(int cellID, int characterID, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterID);
		RoomBlob cellByID = GetCellByID(cellID);
		if (character != null && cellByID != null)
		{
			AssignCellToCharacter_Internal(cellByID, character);
		}
	}

	private void AssignCellToCharacter_Internal(RoomBlob cell, Character character)
	{
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter != null)
		{
			RoomBlob_Solitary roomBlobData = cell.GetRoomBlobData<RoomBlob_Solitary>();
			if (roomBlobData != null)
			{
				dataForCharacter.Room = cell;
				roomBlobData.SetCharacterAssignment(character);
			}
		}
	}

	private void UnassignCellFromCharacterRPC(Character character)
	{
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter == null || dataForCharacter.Room == null)
		{
			return;
		}
		RoomBlob_Solitary roomBlobData = dataForCharacter.Room.GetRoomBlobData<RoomBlob_Solitary>();
		if (roomBlobData != null && roomBlobData.GetAssignedCharacter() == character)
		{
			if (dataForCharacter.Room.GetCharactersInRoom().Contains(character))
			{
				roomBlobData.ReleaseCharacter(character);
			}
			if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
			{
				m_NetView.RPC("RPC_UnassignCellFromCharacter", NetTargets.All, character.m_NetView.viewID);
			}
		}
	}

	[PunRPC]
	private void RPC_UnassignCellFromCharacter(int characterID, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterID);
		if (!(character != null))
		{
			return;
		}
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter != null && dataForCharacter.Room != null)
		{
			RoomBlob_Solitary roomBlobData = dataForCharacter.Room.GetRoomBlobData<RoomBlob_Solitary>();
			if (roomBlobData != null)
			{
				roomBlobData.SetCharacterAssignment(null);
			}
			dataForCharacter.Room = null;
		}
	}

	public bool IsWantedForSolitary(Character character)
	{
		for (int i = 0; i < m_SolitaryData.Count; i++)
		{
			if (m_SolitaryData[i].Character == character)
			{
				return true;
			}
		}
		return false;
	}

	private SolitaryData GetDataForCharacter(Character character)
	{
		int i = 0;
		for (int count = m_SolitaryData.Count; i < count; i++)
		{
			SolitaryData solitaryData = m_SolitaryData[i];
			if (solitaryData.Character == character)
			{
				return solitaryData;
			}
		}
		return null;
	}

	private int GetIDForCell(RoomBlob cell)
	{
		return m_SolitaryRooms.IndexOf(cell);
	}

	private RoomBlob GetCellByID(int cellID)
	{
		RoomBlob result = null;
		if (cellID >= 0 && cellID < m_SolitaryRooms.Count)
		{
			result = m_SolitaryRooms[cellID];
		}
		return result;
	}

	public RoomBlob_Solitary GetCellForCharacter(Character character, out bool success)
	{
		SolitaryData dataForCharacter = GetDataForCharacter(character);
		if (dataForCharacter != null && dataForCharacter.Room != null)
		{
			success = true;
			return dataForCharacter.Room.GetRoomBlobData<RoomBlob_Solitary>();
		}
		success = false;
		return null;
	}

	public float GetTimeRemainining(Character character)
	{
		return GetDataForCharacter(character)?.RemainingDuration ?? 0f;
	}

	public void TriggerLockdown(bool isMiniLockdown = false)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if ((!(instance != null) || instance.gameType != PrisonConfig.ConfigType.Versus) && (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient)))
		{
			if (RoutineManager.GetInstance() != null && !RoutineManager.GetInstance().isRoutineChangeResolving && UpdateManager.AquireHeavyCpuLock())
			{
				bool active = true;
				bool isMiniLockdown2 = isMiniLockdown;
				SetLockdownActive(active, bAllowTimeout: true, isMiniLockdown2);
			}
			else
			{
				m_bShouldTriggerLockdown = true;
			}
		}
	}

	private void SetLockdownActive(bool active, bool bAllowTimeout = true, bool isMiniLockdown = false)
	{
		if (!active || active != IsLockdownActive())
		{
			RoutineManager instance = RoutineManager.GetInstance();
			if (instance != null)
			{
				m_NetView.RPC("RPC_SetLockdownActive", NetTargets.All, active, bAllowTimeout, isMiniLockdown);
				instance.SetLockdownRoutine(active);
			}
		}
	}

	[PunRPC]
	protected void RPC_SetLockdownActive(bool active, bool bAllowTimeout, bool isMiniLockdown, PhotonMessageInfo info)
	{
		m_bIsLockdownActive = active;
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			if (m_LockdownTimer != null)
			{
				instance.RemoveCallbackTimer(m_LockdownTimer);
				m_LockdownTimer = null;
			}
			if (active)
			{
				if (bAllowTimeout)
				{
					if (!isMiniLockdown)
					{
						m_ActiveLockdownMaxDuration = m_LockdownDuration;
					}
					else
					{
						m_ActiveLockdownMaxDuration = m_MiniLockdownDuration;
					}
					m_LockdownTimer = instance.CreateCallbackTimer(0, 0, m_ActiveLockdownMaxDuration, OnAlarm_LockdownEnd, relativeToStart: false);
				}
				if (Platform.GetInstance() != null)
				{
					for (int i = 0; i < ReInput.players.playerCount; i++)
					{
						if (ReInput.players.GetPlayer(i) != null)
						{
							Platform.GetInstance().DoControllerRumble(m_LockdownRumble, i);
							Platform.GetInstance().StartLightBarEffect(m_LockdownLight, i);
						}
					}
				}
			}
			else if (Platform.GetInstance() != null)
			{
				for (int j = 0; j < ReInput.players.playerCount; j++)
				{
					if (ReInput.players.GetPlayer(j) != null)
					{
						Platform.GetInstance().StopLightBarEffect(j);
					}
				}
			}
		}
		if (this.onLockdownChanged != null)
		{
			this.onLockdownChanged(active);
		}
	}

	public bool IsLockdownActive()
	{
		return m_bIsLockdownActive || (m_LockdownTimer != null && !m_LockdownTimer.TimerDone);
	}

	public float GetLockdownProgress()
	{
		float result = 0f;
		if (m_LockdownTimer != null && !m_LockdownTimer.TimerDone)
		{
			result = 1f - m_LockdownTimer.TimeLeft / 60f / (float)m_ActiveLockdownMaxDuration;
		}
		return result;
	}

	private void OnAlarm_LockdownEnd()
	{
		m_LockdownTimer = null;
		if (!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient))
		{
			return;
		}
		for (int i = 0; i < m_SolitaryData.Count; i++)
		{
			SolitaryData solitaryData = m_SolitaryData[i];
			bool flag = false;
			if (!(solitaryData.Character != null) || !(solitaryData.Character.m_CharacterStats != null) || !solitaryData.Character.m_CharacterStats.m_bIsPlayer || solitaryData.Room == null || solitaryData.RemainingDuration <= 0f)
			{
				SetWantedForSolitary(solitaryData.Character, sendToSolitary: false);
			}
		}
		SetLockdownActive(active: false);
	}

	public void OnAI_CharacterPutInSolitary(Character character)
	{
		StartSolitaryConfinement(character);
	}

	public void SetKeyIsMissing(int itemViewID)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance == null || instance.gameType != PrisonConfig.ConfigType.Versus)
		{
			m_NetView.RPC("RPC_SetKeyMissing", NetTargets.All, itemViewID, true);
		}
	}

	public void SetKeyFound(int itemViewID)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance == null || instance.gameType != PrisonConfig.ConfigType.Versus)
		{
			m_NetView.RPC("RPC_SetKeyMissing", NetTargets.All, itemViewID, false);
		}
	}

	[PunRPC]
	private void RPC_SetKeyMissing(int viewID, bool missing, PhotonMessageInfo info)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus)
		{
			return;
		}
		if (missing)
		{
			if (!m_MissingKeys.Contains(viewID))
			{
				m_MissingKeys.Add(viewID);
				if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
				{
					PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(11, PrisonAlertnessManager.AlertnessReason.ItemMissing);
					TriggerLockdown();
				}
			}
		}
		else if (m_MissingKeys.Contains(viewID))
		{
			m_MissingKeys.Remove(viewID);
			if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
			{
				LockdownEndCheck();
			}
		}
	}

	public void SetWantedForSolitary_Quest(Character character, bool isWanted)
	{
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_SetWantedForSolitary_Quest", NetTargets.MasterClient, character.m_NetView.viewID, isWanted);
		}
	}

	[PunRPC]
	private void RPC_SetWantedForSolitary_Quest(int characterID, bool isWanted, PhotonMessageInfo info)
	{
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			Character character = T17NetView.Find<Character>(characterID);
			SetWantedForSolitary(character, isWanted);
		}
	}

	public void SetLockdownActive_Quest(bool active)
	{
		if (m_NetView != null)
		{
			m_NetView.RPC("RPC_SetLockdownActive_Quest", NetTargets.MasterClient, active);
		}
	}

	[PunRPC]
	private void RPC_SetLockdownActive_Quest(bool active, PhotonMessageInfo info)
	{
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			SetLockdownActive(active, bAllowTimeout: false);
		}
	}

	private string Serialize()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance == null)
		{
		}
		m_NetSaveData.LockdownData.TimestampLockdownEnd = -1f;
		if (m_LockdownTimer != null && !m_LockdownTimer.TimerDone)
		{
			m_NetSaveData.LockdownData.TimestampLockdownEnd = instance.GetElapsedSeconds() + m_LockdownTimer.TimeLeft;
		}
		m_NetSaveData.SolitaryData.Clear();
		for (int i = 0; i < m_SolitaryData.Count; i++)
		{
			if (m_SolitaryData[i] != null)
			{
				int viewID = m_SolitaryData[i].Character.m_NetView.viewID;
				int roomID = ((!(m_SolitaryData[i].Room != null)) ? (-1) : GetIDForCell(m_SolitaryData[i].Room));
				float remainingDuration = m_SolitaryData[i].RemainingDuration;
				m_NetSaveData.SolitaryData.Add(new CharacterNetSaveData
				{
					Character = viewID,
					RoomID = roomID,
					RemainingTime = remainingDuration
				});
			}
		}
		m_NetSaveData.MissingKeys.Clear();
		m_NetSaveData.MissingKeys.AddRange(m_MissingKeys);
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public string CreateSnapshot()
	{
		return Serialize();
	}

	public void StartedFromSnapshot()
	{
	}

	public string GetSerializationData()
	{
		string saveData = m_SaveData.GetSaveData();
		if (string.IsNullOrEmpty(saveData))
		{
			return NetPrisonViewDetails.Instance.SolitaryData;
		}
		return saveData;
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		SolitaryNetSaveData solitaryNetSaveData = null;
		try
		{
			solitaryNetSaveData = JsonUtility.FromJson<SolitaryNetSaveData>(data);
		}
		catch
		{
			error += "SolitartManager: JSON Data is corrupt.";
			return false;
		}
		return DeserializeBinary(solitaryNetSaveData, ref error);
	}

	public bool DeserializeBinary(SolitaryNetSaveData data, ref string error)
	{
		if (data == null)
		{
			error += "SolitartManager: JSON Data is corrupt.";
			return false;
		}
		bool result = true;
		if (data.LockdownData != null)
		{
			if (m_LockdownTimer != null)
			{
				RoutineManager.GetInstance().RemoveCallbackTimer(m_LockdownTimer);
				m_bIsLockdownActive = false;
			}
			if (data.LockdownData.TimestampLockdownEnd != -1f)
			{
				m_LockdownTimer = RoutineManager.GetInstance().CreateCallbackTimer(data.LockdownData.TimestampLockdownEnd, OnAlarm_LockdownEnd);
				m_bIsLockdownActive = true;
				if (this.onLockdownChanged != null)
				{
					this.onLockdownChanged(m_bIsLockdownActive);
				}
			}
		}
		if (data.SolitaryData != null)
		{
			m_SolitaryData.Clear();
			for (int i = 0; i < m_SolitaryData.Count; i++)
			{
				if (m_SolitaryData[i] != null && m_SolitaryData[i].Character != null)
				{
					m_SolitaryData[i].Character.SetIsWantedForSolitary(value: false);
				}
			}
			for (int j = 0; j < data.SolitaryData.Count; j++)
			{
				CharacterNetSaveData characterNetSaveData = data.SolitaryData[j];
				Character character = T17NetView.Find<Character>(characterNetSaveData.Character);
				RoomBlob cellByID = GetCellByID(characterNetSaveData.RoomID);
				float remainingTime = characterNetSaveData.RemainingTime;
				if (character != null)
				{
					SolitaryData solitaryData = new SolitaryData();
					solitaryData.Character = character;
					solitaryData.RemainingDuration = remainingTime;
					SolitaryData item = solitaryData;
					m_SolitaryData.Add(item);
					if (cellByID != null)
					{
						AssignCellToCharacter_Internal(cellByID, character);
					}
					character.SetIsWantedForSolitary(value: true);
				}
				else
				{
					string text = "SolitaryManager: " + $"Unable to deserialize solitary cell assignment at index '{j}'\n";
					error += text;
					result = false;
				}
			}
		}
		m_MissingKeys.Clear();
		if (data.MissingKeys != null)
		{
			m_MissingKeys.AddRange(data.MissingKeys);
		}
		return result;
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
		if (!T17NetManager.IsMasterClient || player.IsLocal)
		{
			return;
		}
		if (m_LoadState == LOADSTATE.Finished_OK)
		{
			Serialize();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, m_NetSaveData);
			m_NetView.RPC("RPC_RequestStateResponse_Yes_SolitaryManager", player, memoryStream.ToArray());
			return;
		}
		m_NetView.RPC("RPC_RequestStateResponse_No_SolitaryManager", player);
	}

	[PunRPC]
	private void RPC_RequestStateResponse_Yes_SolitaryManager(byte[] questData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(questData))
		{
			m_NetSaveData = (SolitaryNetSaveData)binaryFormatter.Deserialize(serializationStream);
		}
		if (DeserializeBinary(m_NetSaveData, ref error))
		{
			m_LoadState = LOADSTATE.Finished_OK;
		}
		else
		{
			m_LoadState = LOADSTATE.Finished_Error;
			m_LoadError += error;
		}
		m_LoadState = LOADSTATE.Finished_OK;
	}

	[PunRPC]
	private void RPC_RequestStateResponse_No_SolitaryManager(PhotonMessageInfo info)
	{
		m_LoadError = "SolitaryManager RPC_RequestStateResponse_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	public bool IsKeyByNetviewMissing(int netview)
	{
		return m_MissingKeys.Contains(netview);
	}
}
