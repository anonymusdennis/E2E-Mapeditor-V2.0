using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AUTOGEN_T17Wwise_Enums;
using NetworkLoadable;
using UnityEngine;

public class JobsManager : T17MonoBehaviour, IDeserializable, INetworkLoadable, Saveable
{
	[Serializable]
	private class SaveData
	{
		public ulong[][] m_JobData;

		public int[] m_JobDataVersions;

		public float m_LastLockdownEndedElapsedIngameSeconds;
	}

	[Serializable]
	public class PrisonJobInfo
	{
		public BaseJob m_JobPrefabRef;

		public bool m_EmployNPCOnStart;

		public int m_DaysKeptVacant = 2;
	}

	public delegate void JobEvent(Character employee, JobType jobType);

	public delegate void JobTimeStartedHandler(bool isSaveRestore);

	public delegate void JobTimeEndedHandler();

	private static JobsManager m_Instance = null;

	private T17NetView m_NetView;

	public PrisonJobInfo[] m_PrisonJobsInfo;

	public JobEvent OnJobAssigned;

	public JobEvent OnJobLost;

	private List<BaseJob> m_Jobs = new List<BaseJob>();

	private List<Character> m_InmateNPCs = new List<Character>();

	private Dictionary<int, BaseJob> m_JobKeyRequestIDs = new Dictionary<int, BaseJob>();

	private SaveData m_SaveData;

	private SaveDataRegister m_SaveDataRegister;

	private int m_NumJobReservedIdsGiven;

	private float m_LastLockdownEndedElapsedIngameSeconds;

	private static JobCategory[] ms_JobTypeToCategoryLookUp = new JobCategory[29]
	{
		JobCategory.Invalid,
		JobCategory.ProcessItem,
		JobCategory.ProcessItem,
		JobCategory.ProcessItem,
		JobCategory.CarriedInputJob,
		JobCategory.Repair,
		JobCategory.Repair,
		JobCategory.ProcessItem,
		JobCategory.ProcessItem,
		JobCategory.CarriedInputJob,
		JobCategory.CarriedInputJob,
		JobCategory.Feed,
		JobCategory.Repair,
		JobCategory.ProcessItem,
		JobCategory.ProcessItem,
		JobCategory.ServiceCustomer,
		JobCategory.Feed,
		JobCategory.ServiceCustomer,
		JobCategory.ServiceCustomer,
		JobCategory.ServiceCustomer,
		JobCategory.Repair,
		JobCategory.ProcessItem,
		JobCategory.ProcessItem,
		JobCategory.Repair,
		JobCategory.CarriedInputJob,
		JobCategory.StayInRoomJob,
		JobCategory.CarriedInputJob,
		JobCategory.ServiceCustomer,
		JobCategory.Feed
	};

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public event JobTimeStartedHandler JobTimeStartedEvent;

	public event JobTimeEndedHandler JobTimeEndedEvent;

	public static JobsManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
		}
	}

	private void Start()
	{
		if (m_NetView != null)
		{
			PhotonView component = GetComponent<PhotonView>();
			component.viewID = 0;
			m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.JobsManager);
		}
		RoutineManager instance = RoutineManager.GetInstance();
		instance.OnRoutineChanged -= OnRoutineChanged;
		instance.OnRoutineChanged += OnRoutineChanged;
		instance.OnDayChange -= OnDayChanged;
		instance.OnDayChange += OnDayChanged;
		m_SaveDataRegister = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 6);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		if (m_SaveDataRegister != null)
		{
			m_SaveDataRegister.Dispose();
		}
		m_NetView = null;
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void Init()
	{
		AICharacter_Inmate[] array = UnityEngine.Object.FindObjectsOfType<AICharacter_Inmate>();
		GlobalStart.TimedNetworkService();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Character component = array[i].GetComponent<Character>();
				if (component != null)
				{
					m_InmateNPCs.Add(component);
				}
				GlobalStart.TimedNetworkService();
			}
		}
		if (m_PrisonJobsInfo != null)
		{
			List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.JobRoom);
			GlobalStart.TimedNetworkService();
			for (int j = 0; j < m_PrisonJobsInfo.Length; j++)
			{
				GlobalStart.TimedNetworkService();
				RoomBlob roomBlob = null;
				if (m_PrisonJobsInfo[j] == null || !(m_PrisonJobsInfo[j].m_JobPrefabRef != null))
				{
					continue;
				}
				for (int k = 0; k < allRoomsByLocation.Count; k++)
				{
					GlobalStart.TimedNetworkService();
					RoomBlob_JobRoom roomBlobData = allRoomsByLocation[k].GetRoomBlobData<RoomBlob_JobRoom>();
					if (roomBlobData != null && roomBlobData.m_JobType == m_PrisonJobsInfo[j].m_JobPrefabRef.m_Type)
					{
						roomBlob = allRoomsByLocation[k];
						break;
					}
				}
				if (!(roomBlob != null))
				{
					continue;
				}
				BaseJob baseJob = UnityEngine.Object.Instantiate(m_PrisonJobsInfo[j].m_JobPrefabRef, Vector3.zero, Quaternion.identity);
				GlobalStart.TimedNetworkService();
				if (baseJob != null)
				{
					m_Jobs.Add(baseJob);
					baseJob.Init(roomBlob);
					GlobalStart.TimedNetworkService();
					baseJob.CacheJobRelatedItems();
					GlobalStart.TimedNetworkService();
					baseJob.transform.parent = base.transform;
					if (!PrisonSnapshotIO.IsThereSaveData())
					{
						if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
						{
							KeyFunctionality.KeyColour roomDoorKeyColour = baseJob.RoomDoorKeyColour;
							int roomDoorKeySubCode = baseJob.RoomDoorKeySubCode;
							if (roomDoorKeyColour != KeyFunctionality.KeyColour.None)
							{
								m_JobKeyRequestIDs.Add(ItemManager.GetInstance().GetNextRequestID(), baseJob);
								int requestID = -1;
								if (ItemManager.GetInstance().AssignKeyRPC(0, roomDoorKeyColour, OnItemMgrKeyResponse, ref requestID) == -1)
								{
									m_JobKeyRequestIDs.Remove(ItemManager.GetInstance().GetNextRequestID());
								}
							}
						}
						if (m_PrisonJobsInfo[j].m_EmployNPCOnStart && (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient)))
						{
							AssignRandomNPCToJob(baseJob.m_Type);
						}
					}
				}
				GlobalStart.TimedNetworkService();
			}
		}
		ResetSaveData();
		GlobalStart.TimedNetworkService();
	}

	public int GetNumJobs()
	{
		return m_Jobs.Count;
	}

	public JobType GetJobType(int index)
	{
		if (index < m_Jobs.Count)
		{
			return m_Jobs[index].m_Type;
		}
		return JobType.Invalid;
	}

	public List<BaseJob> GetCurrentJobList()
	{
		return m_Jobs;
	}

	public JobType GetCharactersJobType(Character character)
	{
		BaseJob charactersJob = GetCharactersJob(character);
		if (charactersJob != null)
		{
			return charactersJob.m_Type;
		}
		return JobType.Invalid;
	}

	public bool HasCharacterMetQuota(Character character)
	{
		BaseJob charactersJob = GetCharactersJob(character);
		if (charactersJob != null)
		{
			return charactersJob.QuotaTarget <= charactersJob.QuotaAchieved;
		}
		return false;
	}

	public Character GetJobEmployee(JobType jobType)
	{
		BaseJob jobFromType = GetJobFromType(jobType);
		if (jobFromType != null)
		{
			return jobFromType.Employee;
		}
		return null;
	}

	public JobInfo GetJobInfo(JobType jobType)
	{
		BaseJob jobFromType = GetJobFromType(jobType);
		if (jobFromType != null)
		{
			return jobFromType.m_Info;
		}
		return null;
	}

	public BaseJob GetJob(JobType jobType)
	{
		return GetJobFromType(jobType);
	}

	public bool GetIsJobVacant(JobType jobType)
	{
		BaseJob jobFromType = GetJobFromType(jobType);
		if (jobFromType != null)
		{
			return jobFromType.IsVacant;
		}
		return false;
	}

	public bool DoesCharacterMeetJobRequirements(Character character, JobType jobType)
	{
		if (character == null || character.m_CharacterStats == null)
		{
			return false;
		}
		if (!character.m_CharacterStats.m_bIsPlayer)
		{
			return true;
		}
		JobInfo jobInfo = GetJobInfo(jobType);
		if (jobInfo != null)
		{
			return character.m_CharacterStats.Intellect >= jobInfo.m_IntellectRequired && character.m_CharacterStats.Strength >= jobInfo.m_StrengthRequired;
		}
		return false;
	}

	public void AssignCharacterToJob(Character character, JobType jobType)
	{
		if (!(character == null))
		{
			BaseJob charactersJob = GetCharactersJob(character);
			if (charactersJob != null && charactersJob.m_Type != 0)
			{
				RemoveCharacterFromJob(charactersJob.m_Type);
			}
			if (DoesCharacterMeetJobRequirements(character, jobType))
			{
				m_NetView.RPC("RPC_RequestAssignCharacterToJob", NetTargets.MasterClient, character.m_NetView.viewID, (int)jobType);
			}
		}
	}

	[PunRPC]
	public void RPC_RequestAssignCharacterToJob(int characterViewID, int jobType, PhotonMessageInfo info)
	{
		BaseJob jobFromType = GetJobFromType((JobType)jobType);
		if (jobFromType != null && jobFromType.IsVacant)
		{
			m_NetView.RPC("RPC_AssignCharacterToJob", NetTargets.All, characterViewID, jobType);
		}
	}

	[PunRPC]
	public void RPC_AssignCharacterToJob(int characterViewID, int jobType, PhotonMessageInfo info)
	{
		Character character = T17NetView.Find<Character>(characterViewID);
		if (character == null)
		{
			return;
		}
		BaseJob jobFromType = GetJobFromType((JobType)jobType);
		if (!(jobFromType == null))
		{
			jobFromType.Employee = character;
			character.SetJobRoom(jobFromType.Room);
			if (OnJobAssigned != null)
			{
				OnJobAssigned(character, jobFromType.m_Type);
			}
		}
	}

	public void RemoveCharacterFromJob(Character character)
	{
		BaseJob charactersJob = GetCharactersJob(character);
		if (charactersJob != null)
		{
			RemoveCharacterFromJob(charactersJob.m_Type);
		}
	}

	public void RemoveCharacterFromJob(JobType jobType)
	{
		m_NetView.RPC("RPC_RemoveCharacterFromJob", NetTargets.All, (int)jobType);
	}

	[PunRPC]
	public void RPC_RemoveCharacterFromJob(int jobType, PhotonMessageInfo info)
	{
		BaseJob jobFromType = GetJobFromType((JobType)jobType);
		if (!(jobFromType == null))
		{
			if (OnJobLost != null)
			{
				OnJobLost(jobFromType.Employee, jobFromType.m_Type);
			}
			if (jobFromType.Employee != null)
			{
				jobFromType.Employee.SetJobRoom(null);
			}
			jobFromType.Employee = null;
		}
	}

	public void IncrementQuotaAchieved(JobType jobType)
	{
		m_NetView.RPC("RPC_IncrementQuotaAchieved", NetTargets.MasterClient, (int)jobType);
	}

	[PunRPC]
	public void RPC_IncrementQuotaAchieved(int jobType, PhotonMessageInfo info)
	{
		BaseJob jobFromType = GetJobFromType((JobType)jobType);
		if (!(jobFromType != null))
		{
			return;
		}
		jobFromType.QuotaAchieved++;
		if (jobFromType.Employee != null)
		{
			if (jobFromType.Employee.m_CharacterStats.m_bIsPlayer && jobFromType.Employee.m_NetView.isMine)
			{
				if (jobFromType.QuotaAchieved == jobFromType.QuotaTarget)
				{
					GoogleAnalyticsV3.LogCommericalAnalyticEvent("Jobs", string.Concat(LevelScript.GetCurrentLevelInfo().m_PrisonEnum, " Job Quotas Completed"), jobFromType.m_Type.ToString() + " Quota hit", 0L);
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep_Complete, AudioController.UI_Audio_GO);
					StatSystem.GetInstance().AddIDStat(18, jobType, ((Player)jobFromType.Employee).m_Gamer);
					if (jobType == 7 && jobFromType.Employee.m_bIsNaked)
					{
						StatSystem.GetInstance().IncStat(30, 1f, ((Player)jobFromType.Employee).m_Gamer, string.Empty);
					}
				}
				else if (jobFromType.m_bPlayQuotaIncrementSound)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep, AudioController.UI_Audio_GO);
				}
			}
			if (jobFromType.QuotaAchieved == jobFromType.QuotaTarget)
			{
				jobFromType.Employee.HandleRoutineReachedEvent(RoutineManager.GetInstance().GetCurrentRoutine());
			}
			jobFromType.Employee.SetHaveAnyQuotaDone(haveAnyQuotaDone: true);
		}
		m_NetView.PostLevelLoadRPC("RPC_SetQuotaAchieved", NetTargets.Others, jobType, jobFromType.QuotaAchieved);
	}

	[PunRPC]
	public void RPC_SetQuotaAchieved(int jobType, int quotaAchieved, PhotonMessageInfo info)
	{
		BaseJob jobFromType = GetJobFromType((JobType)jobType);
		if (jobFromType == null)
		{
			return;
		}
		jobFromType.QuotaAchieved = quotaAchieved;
		if (jobFromType.Employee != null && jobFromType.Employee.m_CharacterStats.m_bIsPlayer && jobFromType.Employee.m_NetView.isMine)
		{
			if (jobFromType.QuotaAchieved == jobFromType.QuotaTarget)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep_Complete, AudioController.UI_Audio_GO);
				StatSystem.GetInstance().AddIDStat(18, jobType, ((Player)jobFromType.Employee).m_Gamer);
			}
			else
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep, AudioController.UI_Audio_GO);
			}
		}
		if (jobFromType.QuotaAchieved == jobFromType.QuotaTarget && jobFromType.Employee != null)
		{
			jobFromType.Employee.HandleRoutineReachedEvent(RoutineManager.GetInstance().GetCurrentRoutine());
		}
	}

	private void OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		bool isSaveRestore = oldRoutine == null;
		if (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.Lockdown)
		{
			m_LastLockdownEndedElapsedIngameSeconds = RoutineManager.GetInstance().GetElapsedSeconds();
		}
		if (newRoutine != null && newRoutine.m_BaseRoutineType == Routines.JobTime)
		{
			HandleJobTimeStarted(isSaveRestore);
		}
		else if (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.JobTime)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(m_LastLockdownEndedElapsedIngameSeconds);
			int daysElapsed = RoutineManager.GetInstance().GetDaysElapsed();
			bool flag = newRoutine.m_BaseRoutineType == Routines.Lockdown;
			if (!flag && (timeSpan.Days == daysElapsed || (TimeHelper.DoesTimeRangeGoAcrossMidnight(oldRoutine.m_StartHour, oldRoutine.m_EndHour) && timeSpan.Days == daysElapsed - 1)))
			{
				flag = TimeHelper.IsTimeWithinRange(timeSpan.Hours, timeSpan.Minutes, oldRoutine.m_StartHour, oldRoutine.m_StartMinutes, oldRoutine.m_EndHour, oldRoutine.m_EndMinutes);
			}
			HandleJobTimeEnded(oldRoutine, flag);
		}
	}

	private void HandleJobTimeStarted(bool isSaveRestore)
	{
		int i = 0;
		for (int count = m_Jobs.Count; i < count; i++)
		{
			BaseJob baseJob = m_Jobs[i];
			if (baseJob != null)
			{
				baseJob.OnJobTimeStarted(isSaveRestore);
			}
		}
		if (this.JobTimeStartedEvent != null)
		{
			this.JobTimeStartedEvent(isSaveRestore);
		}
	}

	private void HandleJobTimeEnded(RoutinesData.Routine oldRoutine, bool shouldQuotaBeForgiven)
	{
		for (int i = 0; i < m_Jobs.Count; i++)
		{
			if (m_Jobs[i] != null)
			{
				m_Jobs[i].OnJobTimeEnded();
			}
		}
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			for (int j = 0; j < m_Jobs.Count; j++)
			{
				if (!(m_Jobs[j] != null) || !(m_Jobs[j].Employee != null))
				{
					continue;
				}
				if (m_Jobs[j].QuotaAchieved >= m_Jobs[j].QuotaTarget)
				{
					m_NetView.RPC("RPC_RewardEmployee", m_Jobs[j].Employee.m_NetView, (int)m_Jobs[j].m_Type);
				}
				else
				{
					if (WasJobInFirstTimeGracePeriod(oldRoutine, m_Jobs[j]) || shouldQuotaBeForgiven)
					{
						continue;
					}
					int num = m_Jobs[j].FailureCounter + 1;
					Character employee = m_Jobs[j].Employee;
					employee.HandleRoutineMissedEvent(oldRoutine);
					if (employee.m_CharacterStats.m_bIsPlayer)
					{
						if (num < m_Jobs[j].FailsAllowed)
						{
							string textID = ((num != 1) ? "Text.JobQuota.B" : "Text.JobQuota.A");
							SpeechManager.GetInstance().SaySomething(employee, textID, new List<SpeechManager.Token>
							{
								new SpeechManager.Token("$ATTEMPTS_LEFT", (m_Jobs[j].FailsAllowed - num).ToString(), bIsCharacterNetviewID: false, bIsRequireTranslating: false)
							}, SpeechTone.Negative, 2f);
						}
						else
						{
							SpeechManager.GetInstance().SaySomething(employee, "Text.JobQuota.C", SpeechTone.Negative, 2f);
						}
					}
					if ((!employee.m_CharacterStats.m_bIsPlayer) ? (num >= m_Jobs[j].FailsAllowedForNPC) : (num >= m_Jobs[j].FailsAllowed))
					{
						RemoveCharacterFromJob(m_Jobs[j].m_Type);
						continue;
					}
					m_NetView.RPC("RPC_SetFailureCounter", NetTargets.All, (int)m_Jobs[j].m_Type, num);
				}
			}
		}
		if (this.JobTimeEndedEvent != null)
		{
			this.JobTimeEndedEvent();
		}
	}

	public bool WasJobInFirstTimeGracePeriod(RoutinesData.Routine theRoutine, BaseJob job)
	{
		int startDay = job.GetStartDay();
		int startHour = job.GetStartHour();
		int startMinute = job.GetStartMinute();
		int daysElapsed = RoutineManager.GetInstance().GetDaysElapsed();
		return startDay == daysElapsed && startHour == theRoutine.m_EndHour && startMinute == theRoutine.m_EndMinutes;
	}

	[PunRPC]
	public void RPC_RewardEmployee(int jobType, PhotonMessageInfo info)
	{
		BaseJob jobFromType = GetJobFromType((JobType)jobType);
		if (!(jobFromType == null) && !(jobFromType.Employee == null) && jobFromType.Employee.m_NetView.isMine)
		{
			jobFromType.Employee.m_CharacterStats.IncreaseMoney(jobFromType.m_Info.m_MoneyEarned);
			if (jobFromType.Employee.m_CharacterStats.m_bIsPlayer)
			{
				EffectManager.GetInstance().NewEffectInstanceRPC(EffectManager.effectType.MoneyIncreased, jobFromType.Employee.GetStatChangeEffectPosition());
			}
		}
	}

	[PunRPC]
	public void RPC_SetFailureCounter(int jobType, int timesFailed, PhotonMessageInfo info)
	{
		BaseJob jobFromType = GetJobFromType((JobType)jobType);
		if (!(jobFromType == null))
		{
			jobFromType.FailureCounter = timesFailed;
		}
	}

	private void OnDayChanged()
	{
		for (int i = 0; i < m_Jobs.Count; i++)
		{
			if (m_Jobs[i] != null)
			{
				m_Jobs[i].OnDayChanged();
			}
		}
		if (!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < m_Jobs.Count; j++)
		{
			if (!(m_Jobs[j] != null))
			{
				continue;
			}
			if (m_Jobs[j].Employee != null)
			{
				if (m_Jobs[j].Employee.m_CharacterStats.m_bIsPlayer)
				{
					num2++;
				}
			}
			else
			{
				num++;
			}
		}
		int num3 = ((num2 != 0) ? num : (num - 1));
		if (num3 <= 0)
		{
			return;
		}
		for (int k = 0; k < m_Jobs.Count; k++)
		{
			if (num3 <= 0)
			{
				break;
			}
			if (!(m_Jobs[k] != null) || !(m_Jobs[k].Employee == null))
			{
				continue;
			}
			int daysAllowedToBeVacant = GetDaysAllowedToBeVacant(m_Jobs[k].m_Type);
			if (daysAllowedToBeVacant != -1)
			{
				int daysVacant = m_Jobs[k].DaysVacant;
				if (daysVacant >= daysAllowedToBeVacant && AssignRandomNPCToJob(m_Jobs[k].m_Type))
				{
					num3--;
				}
			}
		}
	}

	private BaseJob GetJobFromType(JobType jobType)
	{
		if (jobType != 0)
		{
			for (int i = 0; i < m_Jobs.Count; i++)
			{
				if (m_Jobs[i].m_Type == jobType)
				{
					return m_Jobs[i];
				}
			}
		}
		return null;
	}

	public BaseJob GetCharactersJob(Character character)
	{
		if (character != null)
		{
			for (int i = 0; i < m_Jobs.Count; i++)
			{
				if (m_Jobs[i].Employee == character)
				{
					return m_Jobs[i];
				}
			}
		}
		return null;
	}

	private int GetDaysAllowedToBeVacant(JobType jobType)
	{
		if (m_PrisonJobsInfo != null)
		{
			for (int i = 0; i < m_PrisonJobsInfo.Length; i++)
			{
				if (m_PrisonJobsInfo[i] != null && m_PrisonJobsInfo[i].m_JobPrefabRef != null && m_PrisonJobsInfo[i].m_JobPrefabRef.m_Type == jobType)
				{
					return m_PrisonJobsInfo[i].m_DaysKeptVacant;
				}
			}
		}
		return -1;
	}

	private void OnItemMgrKeyResponse(Item keyItem, int eventID)
	{
		if (keyItem != null && m_JobKeyRequestIDs.ContainsKey(eventID))
		{
			BaseJob baseJob = m_JobKeyRequestIDs[eventID];
			if (baseJob != null)
			{
				m_NetView.RPC("RPC_SetJobRoomKey", NetTargets.All, keyItem.m_NetView.viewID, (int)baseJob.m_Type);
			}
		}
	}

	[PunRPC]
	public void RPC_SetJobRoomKey(int keyViewID, int jobType, PhotonMessageInfo info)
	{
		Item item = T17NetView.Find<Item>(keyViewID);
		if (!(item == null))
		{
			BaseJob jobFromType = GetJobFromType((JobType)jobType);
			if (!(jobFromType == null))
			{
				jobFromType.RoomDoorKey = item;
			}
		}
	}

	private bool AssignRandomNPCToJob(JobType jobType)
	{
		BaseJob jobFromType = GetJobFromType(jobType);
		if (jobFromType == null)
		{
			return false;
		}
		if (!jobFromType.IsVacant)
		{
			return false;
		}
		int i = 0;
		int num;
		for (num = 50; i < num; i++)
		{
			int count = m_InmateNPCs.Count;
			if (count == 0)
			{
				break;
			}
			int index = UnityEngine.Random.Range(0, count);
			Character character = m_InmateNPCs[index];
			if (character != null && GetCharactersJob(character) == null)
			{
				AssignCharacterToJob(character, jobType);
				break;
			}
		}
		if (i == num)
		{
			return false;
		}
		return true;
	}

	public RoomBlob SetRoutineInformationForCharacter(Character character)
	{
		BaseJob charactersJob = GetCharactersJob(character);
		if (charactersJob != null)
		{
			charactersJob.SetRoutineInformationForCharacter(character);
			return charactersJob.Room;
		}
		return null;
	}

	public bool IsCharactersContrabrandAllForActiveJob(Character character)
	{
		BaseJob charactersJob = GetCharactersJob(character);
		if (charactersJob == null || !charactersJob.IsJobActive())
		{
			return false;
		}
		ItemData[] jobRelatedItems = charactersJob.GetJobRelatedItems();
		if (jobRelatedItems == null)
		{
			return false;
		}
		List<Item> contrabandItems = new List<Item>();
		character.m_ItemContainer.HasContrabandItems(ref contrabandItems);
		Item equippedItem = character.GetEquippedItem();
		Item outFit = character.GetOutFit();
		if (equippedItem != null && equippedItem.m_ItemData != null && equippedItem.m_ItemData.IsContraband())
		{
			contrabandItems.Add(equippedItem);
		}
		if (outFit != null && outFit.m_ItemData != null && outFit.m_ItemData.IsContraband())
		{
			contrabandItems.Add(outFit);
		}
		for (int i = 0; i < jobRelatedItems.Length; i++)
		{
			if (jobRelatedItems[i] != null)
			{
				contrabandItems.RemoveAll((Item x) => x.ItemDataID == jobRelatedItems[i].m_ItemDataID);
			}
		}
		if (contrabandItems.Count == 0)
		{
			return true;
		}
		return false;
	}

	public static JobCategory GetJobCategory(JobType jobType)
	{
		if (ms_JobTypeToCategoryLookUp != null && ms_JobTypeToCategoryLookUp.Length > (int)jobType)
		{
			return ms_JobTypeToCategoryLookUp[(int)jobType];
		}
		return JobCategory.ProcessItem;
	}

	private void SerializeBinary()
	{
		for (int i = 0; i < m_Jobs.Count && i < m_SaveData.m_JobData.Length; i++)
		{
			if (m_Jobs[i].RequiresSerialization)
			{
				m_SaveData.m_JobData[i] = m_Jobs[i].Serialize().ToArray();
				m_SaveData.m_JobDataVersions[i] = m_Jobs[i].GetSaveDataVersion();
			}
		}
		m_SaveData.m_LastLockdownEndedElapsedIngameSeconds = m_LastLockdownEndedElapsedIngameSeconds;
	}

	private bool DeserializeBinary(SaveData jobsData, ref string error)
	{
		if (GetNumJobs() == 0)
		{
			return true;
		}
		m_SaveData = jobsData;
		if (m_SaveData == null || m_SaveData.m_JobData == null || m_SaveData.m_JobData.Length != m_Jobs.Count)
		{
			ResetSaveData();
			error = "JobsManager: SaveData is not valid.";
			return false;
		}
		if (m_SaveData.m_JobDataVersions == null)
		{
			m_SaveData.m_JobDataVersions = new int[m_Jobs.Count];
		}
		for (int i = 0; i < m_SaveData.m_JobData.Length; i++)
		{
			ulong[] array = m_SaveData.m_JobData[i];
			int deserialiseDataVersion = 0;
			if (i < m_SaveData.m_JobDataVersions.Length)
			{
				deserialiseDataVersion = m_SaveData.m_JobDataVersions[i];
			}
			if (array != null)
			{
				m_Jobs[i].m_DeserialiseDataVersion = deserialiseDataVersion;
				m_Jobs[i].Deserialize(array);
			}
		}
		m_LastLockdownEndedElapsedIngameSeconds = m_SaveData.m_LastLockdownEndedElapsedIngameSeconds;
		return true;
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			if (m_PrisonJobsInfo.Length == 0)
			{
				return true;
			}
			error += "There are jobs in the prison but no data to deserialize.";
			return false;
		}
		if (data == "Intentionally Left Blank" && m_PrisonJobsInfo.Length == 0)
		{
			return true;
		}
		SaveData jobsData = null;
		try
		{
			byte[] buffer = Convert.FromBase64String(data);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream serializationStream = new MemoryStream(buffer);
			jobsData = binaryFormatter.Deserialize(serializationStream) as SaveData;
		}
		catch
		{
			ResetSaveData();
			error = "JobsManager: JSON Data is currupt.";
			return false;
		}
		return DeserializeBinary(jobsData, ref error);
	}

	public string GetSerializationData()
	{
		return m_SaveDataRegister.GetSaveData();
	}

	private void ResetSaveData()
	{
		m_SaveData = new SaveData();
		m_SaveData.m_JobData = new ulong[m_Jobs.Count][];
		m_SaveData.m_JobDataVersions = new int[m_Jobs.Count];
	}

	public string CreateSnapshot()
	{
		SerializeBinary();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, m_SaveData);
		byte[] inArray = memoryStream.ToArray();
		return Convert.ToBase64String(inArray);
	}

	public void StartedFromSnapshot()
	{
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
			SerializeBinary();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, m_SaveData);
			m_NetView.RPC("RPC_RequestStateResponce_Yes_JobsManager", player, memoryStream.ToArray());
		}
		else
		{
			m_NetView.RPC("RPC_RequestStateResponce_No_JobsManager", player);
		}
		m_NetView.RPC("RPC_ALL_ReseedNetworkedRandom", NetTargets.All, (int)DateTime.Now.Ticks);
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_JobsManager(byte[] jobData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream serializationStream = new MemoryStream(jobData))
		{
			m_SaveData = binaryFormatter.Deserialize(serializationStream) as SaveData;
		}
		if (DeserializeBinary(m_SaveData, ref error))
		{
			m_LoadState = LOADSTATE.Finished_OK;
			return;
		}
		m_LoadState = LOADSTATE.Finished_Error;
		m_LoadError += error;
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_JobsManager(PhotonMessageInfo info)
	{
		m_LoadError = "ItemManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	[PunRPC]
	private void RPC_ALL_ReseedNetworkedRandom(int seed)
	{
		int i = 0;
		for (int count = m_Jobs.Count; i < count; i++)
		{
			if (m_Jobs[i] != null)
			{
				m_Jobs[i].ReseedRandom(seed);
			}
		}
	}

	public int TakeAReservedIdForJobs()
	{
		return T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.JobInstanceStart) + m_NumJobReservedIdsGiven++;
	}
}
