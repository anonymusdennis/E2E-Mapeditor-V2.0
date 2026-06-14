using System;
using System.Collections;
using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class BaseJob : T17MonoBehaviour
{
	public JobType m_Type;

	public JobInfo m_Info = new JobInfo();

	public int m_QuotaTarget = 1;

	public int m_FailsAllowed = 2;

	public int m_FailsAllowedForNPC = 1;

	public bool m_bPlayQuotaIncrementSound = true;

	public string m_IdlePerformingWorkText;

	public List<JobTutorialStep> m_TutorialSteps = new List<JobTutorialStep>();

	private Character m_Employee;

	private RoomBlob m_Room;

	private RoomBlob_JobRoom m_RoomData;

	private Item m_RoomDoorKey;

	private bool m_JobTimeActive;

	private int m_QuotaAchieved;

	private int m_FailureCounter;

	private int m_NumDaysVacant;

	private int m_EmployeeStartDay;

	private int m_EmployeeStartHour;

	private int m_EmployeeStartMinute;

	private const int NUM_BITS_FOR_DAYS_VACANT = 11;

	private const int MAX_VACANT_DAYS = 2047;

	private ItemData[] m_CachedJobRelatedItems;

	protected System.Random m_NetworkSycnedRandom;

	[HideInInspector]
	public int m_DeserialiseDataVersion;

	private Coroutine m_RecalculateInformationRoutine;

	protected List<int> m_ItemMgrResponseIDs = new List<int>();

	protected int m_ImmediateItemMgrResponseID = -1;

	protected const int NUM_LONGS_USED_BY_BASE = 1;

	public RoomBlob Room => m_Room;

	public RoomBlob_JobRoom RoomData => m_RoomData;

	public KeyFunctionality.KeyColour RoomDoorKeyColour
	{
		get
		{
			if (m_RoomData != null && m_RoomData.m_Door != null)
			{
				return m_RoomData.m_Door.m_DoorKeyColour;
			}
			return KeyFunctionality.KeyColour.None;
		}
	}

	public int RoomDoorKeySubCode
	{
		get
		{
			if (m_RoomData != null && m_RoomData.m_Door != null)
			{
				return m_RoomData.m_Door.m_DoorKeySubCode;
			}
			return 0;
		}
	}

	public Item RoomDoorKey
	{
		set
		{
			if (!(value != null))
			{
				return;
			}
			KeyFunctionality keyFunctionality = (KeyFunctionality)value.HasFunctionality(BaseItemFunctionality.Functionality.Key);
			if (keyFunctionality != null)
			{
				keyFunctionality.SetKeyHidden(isHidden: true);
				keyFunctionality.SetKeySubCode(RoomDoorKeySubCode);
				m_RoomDoorKey = value;
				if (m_Employee != null && m_Employee.m_NetView.isMine)
				{
					m_Employee.m_ItemContainer.AddItemRPC(m_RoomDoorKey);
				}
				m_RoomDoorKey.MeshRendererProp.enabled = false;
				RequiresSerialization = true;
			}
		}
	}

	public bool IsVacant => m_Employee == null;

	public int DaysVacant
	{
		get
		{
			if (IsVacant)
			{
				return m_NumDaysVacant;
			}
			return 0;
		}
	}

	public Character Employee
	{
		get
		{
			return m_Employee;
		}
		set
		{
			if (!(m_Employee != value))
			{
				return;
			}
			if (m_Employee != null && m_Employee.m_NetView.isMine && m_RoomDoorKey != null)
			{
				m_Employee.m_ItemContainer.RemoveItemRPC(m_RoomDoorKey);
			}
			if (value == null && m_Employee != null)
			{
				ItemContainer itemContainer = m_Employee.m_ItemContainer;
				itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(Employee_ItemContainerChanged));
				m_Employee.EquippedItemChangedEvent -= Employee_EquippedItemChangedEvent;
			}
			m_Employee = value;
			if (Employee != null)
			{
				SubscribeToEmployeeEvents();
				SetupEmployeeForJob(Employee);
			}
			m_FailureCounter = 0;
			if (m_Employee != null)
			{
				if (m_Employee.m_NetView.isMine && m_RoomDoorKey != null)
				{
					m_Employee.m_ItemContainer.AddItemRPC(m_RoomDoorKey);
				}
				RoutineManager instance = RoutineManager.GetInstance();
				if (instance != null)
				{
					RoutinesData.Routine currentRoutine = instance.GetCurrentRoutine();
					if (currentRoutine != null && currentRoutine.m_BaseRoutineType == Routines.JobTime)
					{
						int num = instance.GetDaysElapsed();
						if (TimeHelper.DoesTimeRangeGoAcrossMidnight(currentRoutine.m_StartHour, currentRoutine.m_EndHour))
						{
							num++;
						}
						m_EmployeeStartDay = num;
						m_EmployeeStartHour = currentRoutine.m_EndHour;
						m_EmployeeStartMinute = currentRoutine.m_EndMinutes;
					}
					else
					{
						m_EmployeeStartDay = instance.GetDaysElapsed();
						m_EmployeeStartHour = instance.TimeHourPart;
						m_EmployeeStartMinute = instance.TimeMinutePart;
					}
				}
			}
			else
			{
				m_NumDaysVacant = 0;
			}
			RequiresSerialization = true;
		}
	}

	public int QuotaTarget => m_QuotaTarget;

	public int QuotaAchieved
	{
		get
		{
			return m_QuotaAchieved;
		}
		set
		{
			if (m_QuotaAchieved != value)
			{
				m_QuotaAchieved = value;
				RequiresSerialization = true;
			}
		}
	}

	public float NormalizedProgress
	{
		get
		{
			float value = (float)m_QuotaAchieved / (float)m_QuotaTarget;
			return Mathf.Clamp01(value);
		}
	}

	public int FailsAllowed => m_FailsAllowed;

	public int FailsAllowedForNPC => m_FailsAllowedForNPC;

	public int FailureCounter
	{
		get
		{
			return m_FailureCounter;
		}
		set
		{
			if (m_FailureCounter != value)
			{
				m_FailureCounter = value;
				RequiresSerialization = true;
			}
		}
	}

	public bool RequiresSerialization { get; protected set; }

	private void SubscribeToEmployeeEvents()
	{
		ItemContainer itemContainer = m_Employee.m_ItemContainer;
		itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(Employee_ItemContainerChanged));
		m_Employee.EquippedItemChangedEvent += Employee_EquippedItemChangedEvent;
	}

	public bool IsJobActive()
	{
		if (!m_JobTimeActive || m_Employee == null)
		{
			return false;
		}
		return true;
	}

	public bool IsCurrentTimePastStartTime()
	{
		TimeSpan timeSpan = new TimeSpan(m_EmployeeStartDay, m_EmployeeStartHour, m_EmployeeStartMinute, 0);
		TimeSpan cachedTimespan = RoutineManager.GetInstance().GetCachedTimespan();
		return cachedTimespan > timeSpan;
	}

	public virtual void Init(RoomBlob jobRoom)
	{
		m_Room = jobRoom;
		m_RoomData = m_Room.GetRoomBlobData<RoomBlob_JobRoom>();
		if (m_RoomData.m_TutorialBoard != null)
		{
			m_RoomData.m_TutorialBoard.m_Job = this;
		}
		SetRefreshOffInPossibleItemContainerList(m_RoomData.m_Dispensers);
		SetRefreshOffInPossibleItemContainerList(m_RoomData.m_Processors);
		SetRefreshOffInPossibleItemContainerList(m_RoomData.m_Collectors);
		m_NetworkSycnedRandom = new System.Random();
		RequiresSerialization = true;
	}

	protected virtual void OnDestroy()
	{
		if (m_Employee != null)
		{
			ItemContainer itemContainer = m_Employee.m_ItemContainer;
			itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(Employee_ItemContainerChanged));
			m_Employee.EquippedItemChangedEvent -= Employee_EquippedItemChangedEvent;
			m_Employee = null;
		}
		m_Room = null;
		if ((bool)m_RoomData)
		{
			if (m_RoomData.m_TutorialBoard != null)
			{
				m_RoomData.m_TutorialBoard.m_Job = null;
			}
			m_RoomData = null;
		}
		m_RoomDoorKey = null;
	}

	public void CacheJobRelatedItems()
	{
		List<ItemData> list = OneTimeCalculateJobRelatedItems();
		if (list != null)
		{
			m_CachedJobRelatedItems = list.ToArray();
		}
	}

	private void SetRefreshOffInPossibleItemContainerList(List<InteractiveObject> objects)
	{
		if (objects == null)
		{
			return;
		}
		for (int num = objects.Count - 1; num >= 0; num--)
		{
			if (objects[num] != null)
			{
				ItemContainer component = objects[num].GetComponent<ItemContainer>();
				if (component != null)
				{
					component.m_ContainerType = ItemContainer.ItemContainerType.Job;
					component.m_bShouldConsiderItemRefresh = false;
				}
			}
		}
	}

	public virtual void OnJobTimeStarted(bool isSaveRestore)
	{
		m_JobTimeActive = true;
		if (!isSaveRestore)
		{
			m_QuotaAchieved = 0;
			if (m_Employee != null)
			{
				m_Employee.SetJobComplete(jobComplete: false);
				m_Employee.SetHaveAnyQuotaDone(haveAnyQuotaDone: false);
			}
		}
		RequiresSerialization = true;
	}

	public virtual void OnJobTimeEnded()
	{
		m_JobTimeActive = false;
		RequiresSerialization = true;
	}

	public virtual void SetRoutineInformationForCharacter(Character character)
	{
		if (character != null && character.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)character;
			if (player != null)
			{
				player.SetRoutineArrowTarget(Room);
			}
		}
	}

	private void Employee_ItemContainerChanged()
	{
		if (m_JobTimeActive)
		{
			StartDelayedRecalculteRoutineInformation();
		}
	}

	private void Employee_EquippedItemChangedEvent(Character character, Item equippedItem)
	{
		if (m_JobTimeActive)
		{
			StartDelayedRecalculteRoutineInformation();
		}
	}

	protected void StartDelayedRecalculteRoutineInformation()
	{
		if (m_RecalculateInformationRoutine != null)
		{
			StopCoroutine(m_RecalculateInformationRoutine);
		}
		m_RecalculateInformationRoutine = StartCoroutine(DelayedRecalculateRoutineInformation(Employee));
	}

	private IEnumerator DelayedRecalculateRoutineInformation(Character employeeAtTime)
	{
		yield return new WaitForSecondsRealtime(0.25f);
		if (m_JobTimeActive && Employee == employeeAtTime)
		{
			SetRoutineInformationForCharacter(Employee);
		}
		m_RecalculateInformationRoutine = null;
	}

	protected virtual List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		return null;
	}

	public ItemData[] GetJobRelatedItems()
	{
		if (m_CachedJobRelatedItems == null)
		{
		}
		return m_CachedJobRelatedItems;
	}

	public int GetStartDay()
	{
		return m_EmployeeStartDay;
	}

	public int GetStartHour()
	{
		return m_EmployeeStartHour;
	}

	public int GetStartMinute()
	{
		return m_EmployeeStartMinute;
	}

	public virtual int GetSaveDataVersion()
	{
		return 0;
	}

	public virtual List<ulong> Serialize()
	{
		List<ulong> list = new List<ulong>();
		RequiresSerialization = false;
		int uValue = 0;
		if (m_Employee != null)
		{
			uValue = m_Employee.m_NetView.viewID;
		}
		int uValue2 = 0;
		if (m_RoomDoorKey != null)
		{
			uValue2 = m_RoomDoorKey.m_NetView.viewID;
		}
		BitField bitField = new BitField();
		bitField.Set(12, (uint)uValue);
		bitField.Set(12, (uint)uValue2);
		bitField.Set(m_JobTimeActive);
		bitField.Set(6, (uint)m_QuotaAchieved);
		bitField.Set(4, (uint)m_FailureCounter);
		bitField.Set(11, (uint)m_NumDaysVacant);
		bitField.Set(5, (uint)m_EmployeeStartHour);
		bitField.Set(6, (uint)m_EmployeeStartMinute);
		bitField.Set(7, (uint)m_EmployeeStartDay);
		list.Add((ulong)bitField);
		return list;
	}

	public virtual void Deserialize(ulong[] jobData)
	{
		if (jobData == null || jobData.Length <= 0)
		{
			return;
		}
		BitField bitField = new BitField(jobData[0]);
		int uInt = (int)bitField.GetUInt(12);
		int uInt2 = (int)bitField.GetUInt(12);
		m_Employee = ((uInt == 0) ? null : T17NetView.Find<Character>(uInt));
		if (m_Employee != null)
		{
			SubscribeToEmployeeEvents();
		}
		m_RoomDoorKey = ((uInt2 == 0) ? null : T17NetView.Find<Item>(uInt2));
		m_JobTimeActive = bitField.GetBool();
		m_QuotaAchieved = (int)bitField.GetUInt(6);
		m_FailureCounter = (int)bitField.GetUInt(4);
		m_NumDaysVacant = (int)bitField.GetUInt(11);
		m_EmployeeStartHour = (int)bitField.GetUInt(5);
		m_EmployeeStartMinute = (int)bitField.GetUInt(6);
		m_EmployeeStartDay = (int)bitField.GetUInt(7);
		string text = ((!(m_Employee != null)) ? "NONE" : m_Employee.m_CharacterCustomisation.m_DisplayName);
		string text2 = "UNKNOWN";
		if (m_RoomDoorKey != null)
		{
			KeyFunctionality keyFunctionality = (KeyFunctionality)m_RoomDoorKey.HasFunctionality(BaseItemFunctionality.Functionality.Key);
			if (keyFunctionality != null)
			{
				text2 = "Colour: " + keyFunctionality.m_KeyColour.ToString() + " Code: " + keyFunctionality.SubCode;
			}
		}
		if (m_Employee != null)
		{
			m_Employee.SetJobRoom(m_Room);
		}
	}

	public void OnDayChanged()
	{
		int numDaysVacant = m_NumDaysVacant;
		if (IsVacant)
		{
			if (m_NumDaysVacant < 2047)
			{
				m_NumDaysVacant++;
			}
		}
		else
		{
			m_NumDaysVacant = 0;
		}
		if (m_NumDaysVacant != numDaysVacant)
		{
			RequiresSerialization = true;
		}
	}

	protected void IncrementQuotaAchieved()
	{
		if (m_JobTimeActive)
		{
			if (m_Employee != null)
			{
				m_Employee.SetHaveAnyQuotaDone(haveAnyQuotaDone: true);
			}
			JobsManager.GetInstance().IncrementQuotaAchieved(m_Type);
		}
	}

	protected void LogDesignerProblemToGoogle(string text)
	{
		T17NetManager.LogGoogleException("DESIGNERS PLZ: " + text + "\n" + LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " job is " + base.transform.name);
	}

	public virtual bool DoesEmployeeHaveToReportToJobRoom()
	{
		return true;
	}

	public void ReseedRandom(int seed)
	{
		m_NetworkSycnedRandom = new System.Random(seed);
	}

	protected virtual void SetupEmployeeForJob(Character localCharacter)
	{
		AIPlayer aIPlayer = localCharacter as AIPlayer;
		if (aIPlayer != null)
		{
			aIPlayer.m_Blackboard.SetValue("m_IdleJobText", m_IdlePerformingWorkText);
		}
	}

	protected int RequestItemCreation(int ownerID, int itemDataID)
	{
		int result = ItemManager.GetInstance().AssignItemRPC(ownerID, itemDataID, OnItemMgrResponseAddOutputItem, ref m_ImmediateItemMgrResponseID);
		m_ItemMgrResponseIDs.Add(m_ImmediateItemMgrResponseID);
		return result;
	}

	private void OnItemMgrResponseAddOutputItem(Item item, int eventID)
	{
		if ((item != null && eventID == m_ImmediateItemMgrResponseID) || m_ItemMgrResponseIDs.Contains(eventID))
		{
			OnItemManagerCreatedItemForUs(item, eventID);
			m_ImmediateItemMgrResponseID = -1;
			m_ItemMgrResponseIDs.Remove(eventID);
		}
	}

	protected virtual void OnItemManagerCreatedItemForUs(Item item, int eventId)
	{
	}
}
