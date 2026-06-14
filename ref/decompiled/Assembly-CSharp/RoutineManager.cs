using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class RoutineManager : T17MonoBehaviour, Saveable, IPunObservable, IControlledUpdate
{
	public delegate void RoutineEnd(RoutinesData.Routine routine, bool forceEnd);

	public delegate void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd);

	public delegate void PurpleDoorsChange(bool areOpen);

	public enum DayType
	{
		NormalDay,
		GarbageDay,
		HelicopterDay,
		BoatDay
	}

	public delegate void DayChange();

	[Serializable]
	public struct EventData
	{
		public DayType m_EventType;

		public string m_EventDescription;
	}

	public class CallbackInGameTimer
	{
		public delegate void CallBackTimerEvent();

		private float m_StartElapsedInGameSeconds;

		private float m_TimerNeededInGameSeconds;

		private float m_TimeLeft;

		private CallBackTimerEvent m_Callback;

		private bool m_bTimerDone;

		private bool m_bActive;

		public bool TimerDone => m_bTimerDone;

		public float TimeLeft => m_TimeLeft;

		public bool Active => m_bActive;

		public CallbackInGameTimer(int days, int hours, int minutes, CallBackTimerEvent callback, bool relativeToStart = true)
			: this(minutes * 60 + hours * 3600 + days * 86400, callback, relativeToStart)
		{
		}

		public CallbackInGameTimer(float ingameSecondsNeeded, CallBackTimerEvent callback, bool relativeToStart = true)
		{
			m_TimerNeededInGameSeconds = ingameSecondsNeeded;
			m_TimeLeft = m_TimerNeededInGameSeconds;
			if (relativeToStart)
			{
				m_StartElapsedInGameSeconds = 0f;
			}
			else
			{
				m_StartElapsedInGameSeconds = GetInstance().m_ElapsedInGameSeconds;
			}
			m_Callback = callback;
			m_bActive = true;
		}

		public void Update(float currentElapsedIngameSecond)
		{
			if (!m_bActive)
			{
				return;
			}
			m_TimeLeft = m_TimerNeededInGameSeconds - (currentElapsedIngameSecond - m_StartElapsedInGameSeconds);
			if (m_TimeLeft < 0f)
			{
				m_bTimerDone = true;
				if (m_Callback != null)
				{
					m_Callback();
				}
			}
		}

		public void Pause()
		{
			m_bActive = false;
		}

		public void Resume()
		{
			m_bActive = true;
		}
	}

	[Serializable]
	protected class SaveData_RoutineManager_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public float S;

		public bool L;

		public int R;

		public SaveData_RoutineManager_V1()
		{
			m_Version = 1;
		}
	}

	private static RoutineManager m_Instance;

	public float m_RealLifeSecondPerGameMinute = 5f;

	public RoutinesData m_RoutinesData;

	public Platform.RumbleController m_RumbleSettingsForRoutineChange = new Platform.RumbleController();

	public Platform.LightBarEffect m_LightSettingsForRoutineChange = new Platform.LightBarEffect();

	private bool m_bFreezeTime;

	[Tooltip("When all players are in bed, this is how much time should fast forward")]
	public int m_AllSleepingFastForwardFactor = 100;

	private bool m_bAllPlayersSleeping;

	private bool m_bSpecialStartOfPrisonNoSleepPeriod = true;

	public bool m_bFastForward;

	public int m_FastForwardFactor = 100;

	private float m_GameSecondsPerRealSecond;

	private float m_ElapsedInGameSeconds;

	private bool m_bSleepSpeedUp;

	private int m_ElapsedInGameMinutes;

	private int m_ElapsedInGameHours;

	private int m_ElapsedInGameHoursPrev;

	private int m_ElapsedInGameDays;

	private int m_PreviousDaysFromStartTime;

	private int m_RefreshedItemContainerDay = -1;

	private int m_PreviousElapsedInGameDays;

	private int m_StatRecordedHour = -1;

	private int m_StatRecordedDay;

	private bool m_bRoutineManagerReady;

	private TimeSpan m_TimeSpan;

	private T17NetView m_NetView;

	private SaveDataRegister m_SaveData;

	private bool m_bSaveGamePending;

	private bool m_bRoutinePreviouslyNULL;

	private RoutinesData.Routine m_CurrentRoutine;

	private Events m_CurrentRoutineMusic = Events.Idle_Bongos_Down;

	private List<CallbackInGameTimer> m_CallbackTimers = new List<CallbackInGameTimer>();

	private bool m_bStartedRoutineMusic;

	private bool m_bPurpleDoorsOpen;

	private bool m_bLockdownRoutineActive;

	private bool m_bRoutineChangeResolving;

	private bool m_bSyncTime;

	private bool m_bTimedPrisonTimesUp;

	public DayType[] m_CalendarEvents = new DayType[30];

	public EventData[] m_EventData = new EventData[4];

	public int TimeMinutePart => m_ElapsedInGameMinutes;

	public int TimeHourPart => m_ElapsedInGameHours;

	public bool PurpleDoorsOpen => m_bPurpleDoorsOpen;

	public bool isRoutineChangeResolving => m_bRoutineChangeResolving;

	public event RoutineChanged OnRoutineChanged;

	public event RoutineChanged OnRoutineChangedManagerStart;

	public event RoutineEnd OnRoutineEnded;

	public event PurpleDoorsChange OnPurpleDoorLockStatusChanged;

	public event DayChange OnDayChange;

	public event DayChange OnDaysFromStartTimeChange;

	public static RoutineManager GetInstance()
	{
		return m_Instance;
	}

	public bool IsTimeFrozen()
	{
		return m_bFreezeTime;
	}

	public void SetTimeFrozenRPC(bool bFrozen)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_bFreezeTime = bFrozen;
			m_NetView.RPC("RPC_SyncTime", NetTargets.Others, m_ElapsedInGameSeconds, T17NetManager.ServerTimestamp, m_bFreezeTime, IsLockdownActive(), m_bSpecialStartOfPrisonNoSleepPeriod);
		}
	}

	public bool IsLockdownActive()
	{
		return m_bLockdownRoutineActive;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance == null)
		{
			m_Instance = this;
			m_GameSecondsPerRealSecond = 1f / (m_RealLifeSecondPerGameMinute / 60f);
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
		m_NetView = GetComponent<T17NetView>();
		m_NetView.NetViewSynchronization = T17NetViewSynchronization.UnreliableOnChange;
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Register(this, UpdateCategory.RapidPeriodic);
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_PreviousElapsedInGameDays = -1;
		m_PreviousDaysFromStartTime = -1;
		if (ConfigManager.GetInstance().routineConfig != null)
		{
			ApplyConfigData(ConfigManager.GetInstance().routineConfig);
		}
		if (m_NetView != null)
		{
			m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 1);
			m_NetView.AddToObservedComponents(this);
		}
		SetInitialTime();
		if (ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus)
		{
			SetupVersusTimeoutCallback();
		}
		else if (m_RoutinesData != null && m_RoutinesData.m_bIsTimedPrison)
		{
			SetupTimedPrisonCallback();
		}
		Gamer.OnCreate += Gamer_OnCreate;
		Gamer.OnDeleted += Gamer_OnDeleted;
		m_bRoutineManagerReady = true;
		if (DoorManager.GetInstance() != null)
		{
			DoorManager.GetInstance().RegisterToRoutineManager();
		}
		NetLoadSync.RegisterOnReadyToPlayInterest(GlobalStart_EnteredLevelEvent);
		return base.StartInit();
	}

	private void SetupVersusTimeoutCallback()
	{
		int minutes = -1;
		ConfigManager.GetInstance().GetVersusDuration(out var days, out var hours, out minutes);
		int num = 7;
		int num2 = 30;
		if (m_RoutinesData != null)
		{
			num = m_RoutinesData.m_StartOfTheDayHour;
			num2 = m_RoutinesData.m_StartOfTheDayMinutes;
		}
		float num3 = num * 60 * 60 + num2 * 60;
		float num4 = days * 24 * 60 * 60 + hours * 60 * 60 + minutes * 60;
		CreateCallbackTimer(num3 + num4, VersusGameOverCallback);
	}

	private void SetupTimedPrisonCallback()
	{
		int num = 7;
		int num2 = 30;
		if (m_RoutinesData != null)
		{
			num = m_RoutinesData.m_StartOfTheDayHour;
			num2 = m_RoutinesData.m_StartOfTheDayMinutes;
		}
		float num3 = num * 60 * 60 + num2 * 60;
		float num4 = m_RoutinesData.m_TimedHoursDuration * 60 * 60 + m_RoutinesData.m_TimedMinutesDuration * 60;
		CreateCallbackTimer(num3 + num4, TimedPrisonCallback);
	}

	protected void OnDestroy()
	{
		AudioController.StopPrisonAmbience(m_RoutinesData.m_Ambience);
		CutsceneManagerBase.CutsceneFinishedEvent -= CutsceneManagerBase_CutsceneFinishedEvent;
		if (m_CurrentRoutine != null)
		{
			AudioController.PauseRoutineMusic(m_CurrentRoutineMusic);
		}
		NetLoadSync.RegisterOnReadyToPlayInterest(GlobalStart_EnteredLevelEvent, bAdd: false);
		Gamer.OnCreate -= Gamer_OnCreate;
		Gamer.OnDeleted -= Gamer_OnDeleted;
		this.OnRoutineChanged = null;
		this.OnRoutineEnded = null;
		this.OnRoutineChangedManagerStart = null;
		if (m_SaveData != null)
		{
			if (m_NetView != null)
			{
				m_NetView.RemoveObservedComponent(this);
			}
			m_SaveData.Dispose();
		}
		m_NetView = null;
		AudioController.ClearRoutinesMusic();
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.RapidPeriodic);
		}
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void SetInitialTime()
	{
		if (T17NetManager.IsMasterClient)
		{
			SaveData_RoutineManager_V1 saveData_RoutineManager_V = RetrieveSaveFromRegister();
			if (saveData_RoutineManager_V != null)
			{
				SetRefreshedItemContainerDay(saveData_RoutineManager_V.R);
				TimeSpan timeSpan = TimeSpan.FromSeconds(saveData_RoutineManager_V.S);
				m_ElapsedInGameDays = timeSpan.Days;
				SetTime(timeSpan.Hours, timeSpan.Minutes, proceedToNextDay: false);
			}
			else if (m_RoutinesData != null)
			{
				SetTime(m_RoutinesData.m_StartOfTheDayHour, m_RoutinesData.m_StartOfTheDayMinutes, proceedToNextDay: false);
			}
			else
			{
				SetTime(7, 30, proceedToNextDay: false);
			}
		}
		if (m_RoutinesData != null)
		{
			AudioController.PlayPrisonAmbience(m_RoutinesData.m_Ambience);
		}
	}

	private void GlobalStart_EnteredLevelEvent(int iPhotonID)
	{
		if (T17NetManager.PhotonPlayerID != iPhotonID)
		{
			return;
		}
		if (T17NetManager.IsMasterClient)
		{
			SaveData_RoutineManager_V1 saveData_RoutineManager_V = RetrieveSaveFromRegister();
			if (saveData_RoutineManager_V != null && saveData_RoutineManager_V.L)
			{
				SetLockdownRoutine(active: true);
			}
			m_NetView.RPC("RPC_SyncTime", NetTargets.Others, m_ElapsedInGameSeconds, T17NetManager.ServerTimestamp, m_bFreezeTime, IsLockdownActive(), m_bSpecialStartOfPrisonNoSleepPeriod);
		}
		else
		{
			m_NetView.RPC("RPC_RequestTimeFromMaster", NetTargets.MasterClient);
		}
		NetLoadSync.RegisterOnReadyToPlayInterest(GlobalStart_EnteredLevelEvent, bAdd: false);
	}

	private void ApplyConfigData(RoutineConfig config)
	{
		m_RoutinesData = config.m_RoutineData;
		m_CalendarEvents = config.m_CalendarEvents;
		m_EventData = config.m_EventData;
	}

	public RoutinesData.Routine GetCurrentRoutine()
	{
		return m_CurrentRoutine;
	}

	public Routines GetCurrentRoutineBaseType()
	{
		if (m_CurrentRoutine == null)
		{
			return Routines.UNASSIGNED;
		}
		return m_CurrentRoutine.m_BaseRoutineType;
	}

	public RoutineSubTypes GetCurrentRoutineSubType()
	{
		if (m_CurrentRoutine == null)
		{
			return RoutineSubTypes.NoRoutine;
		}
		return m_CurrentRoutine.m_SubRoutineType;
	}

	public int GetCurrentMins()
	{
		return m_ElapsedInGameMinutes + m_ElapsedInGameHours * 60;
	}

	public float GetElapsedSeconds()
	{
		return m_ElapsedInGameSeconds;
	}

	public float GetSecondsSinceInitialTime()
	{
		int num = 7;
		int num2 = 30;
		if (m_RoutinesData != null)
		{
			num = m_RoutinesData.m_StartOfTheDayHour;
			num2 = m_RoutinesData.m_StartOfTheDayMinutes;
		}
		return m_ElapsedInGameSeconds - (float)(num * 60 * 60 + num2 * 60);
	}

	public int GetDaysSinceInitialTime()
	{
		return (int)GetSecondsSinceInitialTime() / 60 / 60 / 24;
	}

	public void SetLockdownRoutine(bool active)
	{
		if (active == m_bLockdownRoutineActive)
		{
			return;
		}
		m_bLockdownRoutineActive = active;
		if (active)
		{
			RoutinesData.Routine lockdownRoutine = m_RoutinesData.GetLockdownRoutine();
			if (lockdownRoutine != m_CurrentRoutine)
			{
				if (m_CurrentRoutine != null && m_CurrentRoutine.m_Index != -1)
				{
					m_NetView.RPC("RPC_RoutineEnded", NetTargets.All, m_CurrentRoutine.m_Index, true);
				}
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Lockdowns Triggered", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Lockdown Tiggered", Gamer.GetGamerCount() + " Player", 0L);
				m_NetView.RPC("RPC_StartLockdownRoutine", NetTargets.All);
			}
		}
		else
		{
			MasterCheckAndSendNewRoutineEvents(forced: true);
		}
	}

	public RoutinesData.Routine GetRoutineForTime(int hour, int minutes)
	{
		hour = Mathf.Clamp(hour, 0, 23);
		minutes = Mathf.Clamp(minutes, 0, 59);
		return m_RoutinesData.GetRoutine(hour, minutes);
	}

	public void GetRoutineThatMatchesBaseType(Routines baseType, out RoutineSubTypes subType)
	{
		int num = m_RoutinesData.m_Routines.IndexOf(m_CurrentRoutine);
		int num2 = m_RoutinesData.m_Routines.Count;
		num++;
		while (num2 > 0)
		{
			if (m_RoutinesData.m_Routines[num].m_BaseRoutineType == baseType)
			{
				subType = m_RoutinesData.m_Routines[num].m_SubRoutineType;
				num2 = -1;
				return;
			}
			num++;
			if (num >= num2)
			{
				num = 0;
			}
			num2--;
		}
		subType = RoutineSubTypes.NoRoutine;
	}

	public void ControlledUpdate()
	{
		GlobalStart instance = GlobalStart.GetInstance();
		if (!(instance != null) || instance.GetMode() != GlobalStart.GLOBALSTART_MODE.IN_LEVEL || !T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Success) || !m_bRoutineManagerReady)
		{
			return;
		}
		int elapsedInGameMinutes = m_ElapsedInGameMinutes;
		if (!m_bFreezeTime)
		{
			ProcessTime();
		}
		if (m_ElapsedInGameMinutes != elapsedInGameMinutes && m_ElapsedInGameMinutes == 59)
		{
			m_bSaveGamePending = true;
		}
		if (m_bSaveGamePending)
		{
			SaveManager instance2 = SaveManager.GetInstance();
			if (instance2 != null)
			{
				if (instance2.CurrentSaveMode == SaveManager.SaveMode.Automatic)
				{
					if (UpdateManager.AquireHeavyCpuLock())
					{
						instance2.SaveGame(null);
						m_bSaveGamePending = false;
					}
				}
				else
				{
					m_bSaveGamePending = false;
				}
			}
		}
		if (m_NetView.isMine)
		{
			ProcessRoutines();
		}
		ProcessCallbackTimers();
	}

	public float GetFastForwardFactor()
	{
		float result = 1f;
		if (ShouldSpeedUpForSleeping())
		{
			result = m_AllSleepingFastForwardFactor;
		}
		if (m_bFastForward)
		{
			result = m_FastForwardFactor;
		}
		return result;
	}

	public float GetCurrentGameSecondsPerRealSecond()
	{
		return m_GameSecondsPerRealSecond * GetFastForwardFactor();
	}

	private void ProcessTime()
	{
		float num = UpdateManager.deltaTime * GetCurrentGameSecondsPerRealSecond();
		bool flag = false;
		if (ShouldSpeedUpForSleeping())
		{
			m_bSleepSpeedUp = true;
			TimeSpan timeSpan = TimeSpan.FromSeconds(m_ElapsedInGameSeconds + num);
			if (!m_CurrentRoutine.IsTimeWithinRoutine(timeSpan.Hours, timeSpan.Minutes))
			{
				m_ElapsedInGameSeconds = (float)(new TimeSpan(m_TimeSpan.Days, m_CurrentRoutine.m_EndHour, m_CurrentRoutine.m_EndMinutes, 0) + new TimeSpan(0, 1, 0)).TotalSeconds;
			}
			else
			{
				m_ElapsedInGameSeconds += num;
			}
		}
		else
		{
			m_ElapsedInGameSeconds += num;
			if (m_bSleepSpeedUp)
			{
				m_bSleepSpeedUp = false;
				flag = true;
			}
		}
		m_TimeSpan = TimeSpan.FromSeconds(m_ElapsedInGameSeconds);
		int minutes = m_TimeSpan.Minutes;
		int hours = m_TimeSpan.Hours;
		int days = m_TimeSpan.Days;
		if (minutes != m_ElapsedInGameMinutes || hours != m_ElapsedInGameHours || days != m_ElapsedInGameDays)
		{
			float value = (float)hours + (float)minutes / 60f;
			AudioController.SetParameter(Game_Parameter.Time_Of_Day, value);
			ProcessPurpleLocks();
		}
		m_ElapsedInGameMinutes = m_TimeSpan.Minutes;
		m_ElapsedInGameHours = m_TimeSpan.Hours;
		m_ElapsedInGameDays = m_TimeSpan.Days;
		if (m_NetView.isMine && m_RefreshedItemContainerDay != m_ElapsedInGameDays && m_ElapsedInGameHours >= m_RoutinesData.m_ItemContainerRefreshHour && m_ElapsedInGameMinutes >= m_RoutinesData.m_ItemContainerRefreshMinute)
		{
			AllItemContainerRefresh(staggerItemRefresh: true);
		}
		if (m_ElapsedInGameHoursPrev != m_ElapsedInGameHours)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PrisonHour, m_ElapsedInGameHours);
			m_ElapsedInGameHoursPrev = m_ElapsedInGameHours;
		}
		if (m_ElapsedInGameHours != m_StatRecordedHour)
		{
			int num2 = m_ElapsedInGameHours - m_StatRecordedHour;
			int num3 = m_ElapsedInGameDays - m_StatRecordedDay;
			if (num2 == 1 || (num2 == -23 && num3 == 1))
			{
				Gamer primaryGamer = Gamer.GetPrimaryGamer();
				if (primaryGamer != null)
				{
					StatSystem.GetInstance().IncStat(31, 1f, primaryGamer, string.Empty);
				}
			}
			m_StatRecordedHour = m_ElapsedInGameHours;
			m_StatRecordedDay = m_ElapsedInGameDays;
		}
		if (flag)
		{
			flag = false;
			if (T17NetManager.IsMasterClient)
			{
				m_NetView.RPC("RPC_SyncTime", NetTargets.Others, m_ElapsedInGameSeconds, T17NetManager.ServerTimestamp, m_bFreezeTime, IsLockdownActive(), m_bSpecialStartOfPrisonNoSleepPeriod);
			}
		}
		int previousDaysFromStartTime = m_PreviousDaysFromStartTime;
		int num4 = GetDaysSinceInitialTime() + 1;
		if (m_PreviousDaysFromStartTime != num4)
		{
			m_PreviousDaysFromStartTime = num4;
			if (this.OnDaysFromStartTimeChange != null && previousDaysFromStartTime != -1)
			{
				this.OnDaysFromStartTimeChange();
			}
		}
		int previousElapsedInGameDays = m_PreviousElapsedInGameDays;
		if (m_PreviousElapsedInGameDays != m_ElapsedInGameDays)
		{
			m_PreviousElapsedInGameDays = m_ElapsedInGameDays;
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.PrisonDay, m_ElapsedInGameDays);
			if (this.OnDayChange != null && previousElapsedInGameDays != -1)
			{
				this.OnDayChange();
			}
		}
	}

	private void ProcessRoutines(bool forced = false)
	{
		if (!m_bRoutineManagerReady || m_bLockdownRoutineActive)
		{
			return;
		}
		int num = (int)(m_ElapsedInGameSeconds / 60f);
		if (m_CurrentRoutine != null)
		{
			int startInMinutes = m_CurrentRoutine.StartInMinutes;
			int endInMinutes = m_CurrentRoutine.EndInMinutes;
			if ((num < startInMinutes || num >= endInMinutes) && num >= endInMinutes)
			{
				MasterCheckAndSendNewRoutineEvents(forced);
			}
		}
		else
		{
			MasterCheckAndSendNewRoutineEvents(forced);
		}
	}

	private void ProcessPurpleLocks()
	{
		if (m_bRoutineManagerReady && m_RoutinesData != null)
		{
			bool flag = m_RoutinesData.PurpleDoorsOpen(m_ElapsedInGameHours, m_ElapsedInGameMinutes);
			flag &= !m_bLockdownRoutineActive;
			if (flag != m_bPurpleDoorsOpen && this.OnPurpleDoorLockStatusChanged != null)
			{
				m_NetView.RPC("RPC_PurpleDoorsChanged", NetTargets.All, flag);
			}
		}
	}

	public bool IsDuePurpleLockChange()
	{
		bool flag = m_RoutinesData.PurpleDoorsOpen(m_ElapsedInGameHours, m_ElapsedInGameMinutes);
		flag &= !m_bLockdownRoutineActive;
		return flag != m_bPurpleDoorsOpen;
	}

	private void MasterCheckAndSendNewRoutineEvents(bool forced)
	{
		if (!T17NetManager.IsMasterClient || !(m_RoutinesData != null))
		{
			return;
		}
		RoutinesData.Routine routine = m_RoutinesData.GetRoutine(m_ElapsedInGameHours, m_ElapsedInGameMinutes);
		if (routine != m_CurrentRoutine)
		{
			m_NetView.RPC("RPC_SyncTime", NetTargets.Others, m_ElapsedInGameSeconds, T17NetManager.ServerTimestamp, m_bFreezeTime, IsLockdownActive(), m_bSpecialStartOfPrisonNoSleepPeriod);
			if (m_CurrentRoutine != null && m_CurrentRoutine.m_Index != -1)
			{
				m_NetView.RPC("RPC_RoutineEnded", NetTargets.All, m_CurrentRoutine.m_Index, forced);
			}
			if (routine.m_Index != -1)
			{
				m_NetView.RPC("RPC_RoutineChanged", NetTargets.All, routine.m_Index, forced);
			}
		}
	}

	public void AllItemContainerRefresh(bool staggerItemRefresh, bool bUpdateNetworkService = false)
	{
		SetRefreshedItemContainerDay(m_ElapsedInGameDays);
		ItemContainerManager instance = ItemContainerManager.GetInstance();
		if (instance != null)
		{
			instance.RefreshAllItemContainers(staggerItemRefresh, bUpdateNetworkService);
		}
	}

	public void ResyncTime()
	{
		if (T17NetManager.IsConnectedOnline())
		{
			m_NetView.RPC("RPC_RequestTimeFromAllClients", NetTargets.Others);
		}
	}

	[PunRPC]
	private void RPC_RequestTimeFromAllClients(PhotonMessageInfo info)
	{
		m_NetView.RPC("RPC_TrySyncTime", info.sender, m_ElapsedInGameSeconds, T17NetManager.ServerTimestamp, m_bFreezeTime, IsLockdownActive(), m_bSpecialStartOfPrisonNoSleepPeriod);
	}

	[PunRPC]
	private void RPC_TrySyncTime(float elapsedIngameSeconds, int serverTimeStamp, bool bFreezeTime, bool isLockdownActive, bool bSpecialStartOfPrisonNoSleepPeriod, PhotonMessageInfo info)
	{
		if (elapsedIngameSeconds > m_ElapsedInGameSeconds)
		{
			RPC_SyncTime(elapsedIngameSeconds, serverTimeStamp, bFreezeTime, isLockdownActive, bSpecialStartOfPrisonNoSleepPeriod, info);
		}
	}

	[PunRPC]
	private void RPC_RequestTimeFromMaster(PhotonMessageInfo info)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_NetView.RPC("RPC_SyncTime", info.sender, m_ElapsedInGameSeconds, T17NetManager.ServerTimestamp, m_bFreezeTime, IsLockdownActive(), m_bSpecialStartOfPrisonNoSleepPeriod);
		}
	}

	[PunRPC]
	private void RPC_SyncTime(float elapsedIngameSeconds, int serverTimeStamp, bool bFreezeTime, bool isLockdownActive, bool bSpecialStartOfPrisonNoSleepPeriod, PhotonMessageInfo info)
	{
		int num = T17NetManager.ServerTimestamp - serverTimeStamp;
		TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedIngameSeconds) + TimeSpan.FromMilliseconds(num);
		m_ElapsedInGameDays = timeSpan.Days;
		SetTime(timeSpan.Hours, timeSpan.Minutes, proceedToNextDay: false);
		if (isLockdownActive)
		{
			Local_SetRoutineToLockdown();
		}
		else
		{
			RoutinesData.Routine newRoutine = m_RoutinesData.GetRoutine(m_ElapsedInGameHours, m_ElapsedInGameMinutes);
			if (newRoutine != m_CurrentRoutine)
			{
				if (m_CurrentRoutine != null && m_CurrentRoutine.m_Index != -1)
				{
					RoutinesData.Routine routine = m_RoutinesData.m_Routines[m_CurrentRoutine.m_Index];
					HandleRoutineEnded(routine, bForced: true);
				}
				if (newRoutine.m_Index != -1)
				{
					HandleRoutineChanged(ref newRoutine, bForced: true);
				}
			}
		}
		m_bFreezeTime = bFreezeTime;
		m_bSpecialStartOfPrisonNoSleepPeriod = bSpecialStartOfPrisonNoSleepPeriod;
	}

	[PunRPC]
	private void RPC_RoutineEnded(int routineIndex, bool bForced, PhotonMessageInfo info)
	{
		RoutinesData.Routine routine = m_RoutinesData.m_Routines[routineIndex];
		HandleRoutineEnded(routine, bForced);
	}

	private void HandleRoutineEnded(RoutinesData.Routine routine, bool bForced)
	{
		if (this.OnRoutineEnded != null)
		{
			this.OnRoutineEnded(routine, bForced);
		}
	}

	[PunRPC]
	private void RPC_RoutineChanged(int routineIndex, bool bForced, PhotonMessageInfo info)
	{
		RoutinesData.Routine newRoutine = m_RoutinesData.m_Routines[routineIndex];
		if (newRoutine != m_CurrentRoutine || newRoutine == null)
		{
			HandleRoutineChanged(ref newRoutine, bForced);
		}
	}

	private void HandleRoutineChanged(ref RoutinesData.Routine newRoutine, bool bForced)
	{
		m_bLockdownRoutineActive = false;
		UpdateRoutine(ref newRoutine, bForced);
	}

	[PunRPC]
	private void RPC_StartLockdownRoutine(PhotonMessageInfo info)
	{
		Local_SetRoutineToLockdown();
	}

	private void Local_SetRoutineToLockdown()
	{
		RoutinesData.Routine routine = m_RoutinesData.GetLockdownRoutine();
		m_bLockdownRoutineActive = true;
		ProcessPurpleLocks();
		UpdateRoutine(ref routine, bForced: true);
	}

	private void UpdateRoutine(ref RoutinesData.Routine routine, bool bForced)
	{
		if (!m_bStartedRoutineMusic || m_CurrentRoutine == null || routine.m_RoutineMusic != m_CurrentRoutineMusic)
		{
			if (m_CurrentRoutine != null && AudioController.PauseRoutineMusic(m_CurrentRoutineMusic))
			{
				m_bStartedRoutineMusic = false;
			}
			if (!m_bStartedRoutineMusic)
			{
				if (m_CurrentRoutine == null && routine.m_bHasSaveLoadRoutineMusic)
				{
					m_CurrentRoutineMusic = routine.m_SaveLoadRoutineMusic;
				}
				else
				{
					m_CurrentRoutineMusic = routine.m_RoutineMusic;
				}
				m_bStartedRoutineMusic = AudioController.PlayRoutineMusic(m_CurrentRoutineMusic);
			}
		}
		if (Platform.GetInstance() != null)
		{
			for (int i = 0; i < ReInput.players.playerCount; i++)
			{
				if (ReInput.players.GetPlayer(i) != null)
				{
					Platform.GetInstance().DoControllerRumble(m_RumbleSettingsForRoutineChange, i);
					Platform.GetInstance().StartLightBarEffect(m_LightSettingsForRoutineChange, i);
				}
			}
		}
		RoutinesData.Routine currentRoutine = m_CurrentRoutine;
		m_bRoutinePreviouslyNULL = m_CurrentRoutine == null;
		m_CurrentRoutine = routine;
		if (currentRoutine == null || routine != null)
		{
		}
		if (T17NetManager.IsMasterClient)
		{
			if (currentRoutine == null && routine.m_BaseRoutineType == Routines.LightsOut)
			{
				RoutinesData.Routine routine2 = m_RoutinesData.GetRoutine(m_RoutinesData.m_StartOfTheDayHour, m_RoutinesData.m_StartOfTheDayMinutes);
				if ((GetDaysElapsed() == 0 && routine2.m_EndHour >= m_ElapsedInGameHours) || (routine2.m_EndHour == m_ElapsedInGameHours && routine2.m_EndHour >= m_ElapsedInGameMinutes))
				{
					m_bSpecialStartOfPrisonNoSleepPeriod = true;
				}
				else
				{
					m_bSpecialStartOfPrisonNoSleepPeriod = false;
				}
			}
			else
			{
				m_bSpecialStartOfPrisonNoSleepPeriod = false;
			}
		}
		if (this.OnRoutineChanged != null)
		{
			m_bRoutineChangeResolving = true;
			if (this.OnRoutineChangedManagerStart != null)
			{
				this.OnRoutineChangedManagerStart(currentRoutine, routine, bForced);
			}
			this.OnRoutineChanged(currentRoutine, routine, bForced);
			m_bRoutineChangeResolving = false;
			Debug.Log("OnRoutineChangeEnd Routine: " + routine.m_BaseRoutineType);
		}
		Debug.Log(" ++++++  m_CurrentRoutine  set to rountine " + m_CurrentRoutine.m_LocalizationTag);
	}

	public bool IsFirstRoutineAfterStart()
	{
		return m_bRoutinePreviouslyNULL;
	}

	[PunRPC]
	private void RPC_PurpleDoorsChanged(bool purpleDoorsOpen, PhotonMessageInfo info)
	{
		m_bPurpleDoorsOpen = purpleDoorsOpen;
		if (this.OnPurpleDoorLockStatusChanged != null)
		{
			this.OnPurpleDoorLockStatusChanged(purpleDoorsOpen);
		}
	}

	public void SetTime(int hour, int minutes, bool proceedToNextDay = true)
	{
		hour = Mathf.Clamp(hour, 0, 23);
		minutes = Mathf.Clamp(minutes, 0, 59);
		if (proceedToNextDay)
		{
			m_ElapsedInGameDays++;
		}
		float elapsedInGameSeconds = m_ElapsedInGameDays * 24 * 60 * 60 + hour * 60 * 60 + minutes * 60;
		m_ElapsedInGameSeconds = elapsedInGameSeconds;
		m_TimeSpan = TimeSpan.FromSeconds(m_ElapsedInGameSeconds);
		m_ElapsedInGameMinutes = m_TimeSpan.Minutes;
		m_ElapsedInGameHours = m_TimeSpan.Hours;
		m_ElapsedInGameDays = m_TimeSpan.Days;
		m_StatRecordedHour = m_ElapsedInGameHours;
		m_StatRecordedDay = m_ElapsedInGameDays;
		ProcessRoutines(forced: true);
	}

	public string GetTime(out string mins)
	{
		mins = NumberToStringCache.GetIntAsString(m_ElapsedInGameMinutes, bSingleAs2: true);
		return NumberToStringCache.GetIntAsString(m_ElapsedInGameHours, bSingleAs2: true);
	}

	public bool FindTimeOfNextRoutineType(Routines baseType, RoutineSubTypes subType, out int hours, out int mins, out bool nextDay)
	{
		hours = -1;
		mins = -1;
		nextDay = false;
		if (m_RoutinesData != null && m_RoutinesData.m_Routines != null && m_RoutinesData.m_Routines.Count > 0)
		{
			int num = 0;
			if (m_CurrentRoutine != null)
			{
				num = m_RoutinesData.m_Routines.IndexOf(m_CurrentRoutine);
			}
			RoutinesData.Routine routine = null;
			bool flag = false;
			int num2 = num;
			do
			{
				if (++num2 >= m_RoutinesData.m_Routines.Count)
				{
					num2 = 0;
					flag = true;
				}
				RoutinesData.Routine routine2 = m_RoutinesData.m_Routines[num2];
				if (routine2.m_BaseRoutineType == baseType && routine2.m_SubRoutineType == subType)
				{
					routine = routine2;
					break;
				}
			}
			while (num2 != num);
			if (routine != null)
			{
				nextDay = flag;
				hours = routine.m_StartHour;
				mins = routine.m_StartMinutes + 1;
				return true;
			}
		}
		return false;
	}

	public CallbackInGameTimer CreateCallbackTimer(int days, int hours, int minutes, CallbackInGameTimer.CallBackTimerEvent callback, bool relativeToStart = true)
	{
		return CreateCallbackTimer(minutes * 60 + hours * 3600 + days * 86400, callback, relativeToStart);
	}

	public CallbackInGameTimer CreateCallbackTimer(float ingameSecondsNeeded, CallbackInGameTimer.CallBackTimerEvent callback, bool relativeToStart = true)
	{
		CallbackInGameTimer callbackInGameTimer = new CallbackInGameTimer(ingameSecondsNeeded, callback, relativeToStart);
		m_CallbackTimers.Add(callbackInGameTimer);
		return callbackInGameTimer;
	}

	public void RemoveCallbackTimer(CallbackInGameTimer timer)
	{
		m_CallbackTimers.Remove(timer);
	}

	private void ProcessCallbackTimers()
	{
		for (int num = m_CallbackTimers.Count - 1; num >= 0; num--)
		{
			m_CallbackTimers[num].Update(m_ElapsedInGameSeconds);
			if (m_CallbackTimers[num].TimerDone)
			{
				m_CallbackTimers.RemoveAt(num);
			}
		}
	}

	public int GetMonthNo()
	{
		return m_ElapsedInGameDays / 30;
	}

	public int GetNoOfDaysIntoMonth()
	{
		return m_ElapsedInGameDays % 30;
	}

	public DayType GetCurrentDayType()
	{
		return m_CalendarEvents[m_ElapsedInGameDays % 30];
	}

	public string GetTextForDayType(DayType dayType)
	{
		for (int i = 0; i < m_EventData.Length; i++)
		{
			if (m_EventData[i].m_EventType == dayType)
			{
				return m_EventData[i].m_EventDescription;
			}
		}
		return null;
	}

	public void OnPlayerSleepingInOwnBed(Player player, bool isSleeping)
	{
		CheckForAllPlayersSleepingInOwnBed();
		if (!isSleeping && m_bSpecialStartOfPrisonNoSleepPeriod)
		{
			m_bSpecialStartOfPrisonNoSleepPeriod = false;
		}
	}

	private void CheckForAllPlayersSleepingInOwnBed()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		bool bAllPlayersSleeping = true;
		for (int num = allPlayers.Count - 1; num >= 0; num--)
		{
			if (allPlayers[num].m_Gamer != null && !allPlayers[num].GetIsSleepingInOwnBed())
			{
				bAllPlayersSleeping = false;
				break;
			}
		}
		m_bAllPlayersSleeping = bAllPlayersSleeping;
	}

	private void Gamer_OnDeleted()
	{
		CheckForAllPlayersSleepingInOwnBed();
	}

	private void Gamer_OnCreate(Gamer gamer)
	{
		CheckForAllPlayersSleepingInOwnBed();
	}

	public bool IsTimeSpedUp()
	{
		return m_bFastForward || ShouldSpeedUpForSleeping();
	}

	private bool ShouldSpeedUpForSleeping()
	{
		return !m_bSpecialStartOfPrisonNoSleepPeriod && m_bAllPlayersSleeping && m_CurrentRoutine != null && m_CurrentRoutine.m_BaseRoutineType == Routines.LightsOut;
	}

	public bool RoutineManagerReady()
	{
		return m_bRoutineManagerReady;
	}

	public TimeSpan GetTimeLeftInRoutine()
	{
		if (m_CurrentRoutine == null)
		{
			return TimeSpan.FromTicks(0L);
		}
		int num = m_ElapsedInGameDays;
		if (m_CurrentRoutine.m_EndHour < m_ElapsedInGameHours)
		{
			num++;
		}
		TimeSpan timeSpan = new TimeSpan(num, m_CurrentRoutine.m_EndHour, m_CurrentRoutine.m_EndMinutes, 0);
		return timeSpan - m_TimeSpan;
	}

	public TimeSpan GetCachedTimespan()
	{
		return m_TimeSpan;
	}

	private void SetRefreshedItemContainerDay(int value)
	{
		m_NetView.RPC("RPC_SetRefreshedItemContainerDay", NetTargets.All, value);
	}

	[PunRPC]
	public void RPC_SetRefreshedItemContainerDay(int RefreshedItemContainerDay, PhotonMessageInfo info)
	{
		m_RefreshedItemContainerDay = RefreshedItemContainerDay;
	}

	[PunRPC]
	public void RPC_ClientRequestingSync(PhotonMessageInfo info)
	{
		m_NetView.RPC("RPC_MasterSendingSync", info.sender, m_RefreshedItemContainerDay);
	}

	[PunRPC]
	public void RPC_MasterSendingSync(int RefreshedItemContainerDay, PhotonMessageInfo info)
	{
		m_RefreshedItemContainerDay = RefreshedItemContainerDay;
	}

	public void InitalSyncClient()
	{
		m_NetView.RPC("RPC_ClientRequestingSync", NetTargets.MasterClient);
	}

	private void VersusGameOverCallback()
	{
		m_bTimedPrisonTimesUp = true;
		GlobalStart.GetInstance().EndLevel(bShowResults: true);
	}

	private void TimedPrisonCallback()
	{
		m_bTimedPrisonTimesUp = true;
		CutsceneManagerBase instance = CutsceneManagerBase.GetInstance();
		if (instance.m_TimesUpCutscene != null)
		{
			CutsceneManagerBase.CutsceneFinishedEvent += CutsceneManagerBase_CutsceneFinishedEvent;
			instance.PlayCutsceneSetupRPC(instance.m_TimesUpCutscene, UIAnimatedEffectController.Effects.FadeToOpaque, UIAnimatedEffectController.Effects.FadeToOpaque_Hold);
		}
		else
		{
			CutsceneManagerBase_CutsceneFinishedEvent(0f);
		}
	}

	private void CutsceneManagerBase_CutsceneFinishedEvent(float timeUntilCurtainRaised)
	{
		CutsceneManagerBase.CutsceneFinishedEvent -= CutsceneManagerBase_CutsceneFinishedEvent;
		GlobalStart.GetInstance().EndLevel(bShowResults: true);
	}

	public bool IsTimedPrisonTimeUp()
	{
		return m_bTimedPrisonTimesUp;
	}

	public int GetRoutineCount()
	{
		return (m_RoutinesData != null) ? m_RoutinesData.m_Routines.Count : 0;
	}

	public string CreateSnapshot()
	{
		SaveData_RoutineManager_V1 saveData_RoutineManager_V = new SaveData_RoutineManager_V1();
		saveData_RoutineManager_V.S = m_ElapsedInGameSeconds;
		saveData_RoutineManager_V.L = m_bLockdownRoutineActive;
		saveData_RoutineManager_V.R = m_RefreshedItemContainerDay;
		return JsonUtility.ToJson(saveData_RoutineManager_V);
	}

	public virtual void StartedFromSnapshot()
	{
	}

	private SaveData_RoutineManager_V1 RetrieveSaveFromRegister()
	{
		if (m_SaveData == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return null;
		}
		PrisonSnapshotIO.SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_Base>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (snapshotData_Base != null)
		{
			SaveData_RoutineManager_V1 saveData_RoutineManager_V = null;
			try
			{
				saveData_RoutineManager_V = JsonUtility.FromJson<SaveData_RoutineManager_V1>(m_SaveData.GetSaveData());
			}
			catch
			{
			}
			if (saveData_RoutineManager_V != null)
			{
				return saveData_RoutineManager_V;
			}
		}
		return null;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		float value = 0f;
		if (stream.isWriting)
		{
			if (m_bSyncTime)
			{
				value = m_ElapsedInGameSeconds;
			}
			stream.SendNext(m_bFastForward);
			stream.SendNext(value);
		}
		else
		{
			bool flag = T17NetView.ReceiveNext(ref stream, ref m_bFastForward);
			if ((flag & T17NetView.ReceiveNext(ref stream, ref value)) && value > float.Epsilon)
			{
				m_ElapsedInGameSeconds = value;
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public Events GetCurrentRoutineMusic()
	{
		return m_CurrentRoutineMusic;
	}

	public RoutinesData.Routine GetFirstRoutineOfType(Routines routineType)
	{
		int i = 0;
		for (int count = m_RoutinesData.m_Routines.Count; i < count; i++)
		{
			RoutinesData.Routine routine = m_RoutinesData.m_Routines[i];
			if (routine.m_StartHour >= 0 && routine.m_BaseRoutineType == routineType)
			{
				return routine;
			}
		}
		return null;
	}

	public int GetDaysElapsed()
	{
		return m_ElapsedInGameDays;
	}
}
