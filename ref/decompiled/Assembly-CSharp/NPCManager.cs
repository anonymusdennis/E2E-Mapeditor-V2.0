using System;
using System.Collections.Generic;
using System.IO;
using BitStream;
using UnityEngine;

public class NPCManager : MonoBehaviour, IDeserializable, Saveable, IControlledUpdate
{
	private class RoutineChat
	{
		public Transform m_Characterlocation;

		public float m_fEpoch;
	}

	[Serializable]
	public class NetSaveData
	{
		public List<byte> aiCharacterID = new List<byte>();

		public List<AICharacter.CharacterSaveData> aiCharacterData = new List<AICharacter.CharacterSaveData>();

		public bool[] m_CrowdAllowedIndicies;

		public float[] m_CharactersToAllowToShowTime;
	}

	private static NPCManager s_Instance;

	private NetSaveData m_NetSaveData = new NetSaveData();

	private SlowBitStreamWriter m_NetSaveBitWriter;

	private BitStreamReader m_NetSaveBitReader;

	public List<AICharacter> m_AICharacters = new List<AICharacter>();

	public List<AICharacter> m_Medics = new List<AICharacter>();

	public List<AICharacter> m_MaintenanceMen = new List<AICharacter>();

	public List<AICharacter> m_Doggies = new List<AICharacter>();

	public List<AICharacter> m_Guards = new List<AICharacter>();

	private List<AICharacter_CrowdNPC> m_CrowdNPCs = new List<AICharacter_CrowdNPC>();

	private bool[] m_CrowdAllowedIndices;

	private const int NUMBER_LOCAL_PLAYERS_TO_DO_LIMIT = 2;

	private const int MAX_ALLOW_TO_SHOWTIME_IF_LIMITED = 13;

	private const int EMPTY_SEAT_COUNT = 30;

	private float[] m_CharactersToAllowToShowTime = new float[13];

	private float m_fKnownEventPenalty = 150f;

	private RoutineChat[] m_RoutineTalkingCharacters;

	private const int MAX_ROUTINE_TALKING = 4;

	private const float ROUTINE_TALKING_TIME = 4f;

	private const float ROUTINE_TALKING_MIN_DIST = 16f;

	private Vector3 m_vZero = Vector3.zero;

	private List<Character> m_MealTimeQueue;

	private List<RoomWaypoint> m_MealTimeWaypoints;

	private float m_fNormalisedRotation;

	[SerializeField]
	private float m_fCrowdRotationSpeed = 5f;

	[SerializeField]
	private int m_ConcurrentWaving = 20;

	private int m_CrowdIndex;

	private T17NetView m_NetView;

	private bool m_bAllowCrowdAnimatorFeature;

	private bool m_bCrowdAnimatorFeatureEnabled;

	private SaveDataRegister m_SaveData;

	public static NPCManager GetInstance()
	{
		return s_Instance;
	}

	private void Awake()
	{
		if (s_Instance != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			s_Instance = this;
		}
		T17NetManager.OnBecameMasterClient += OnBecameMasterClient;
		T17NetRoomGameView.OnRoomSignalEvent += OnEvent;
		m_NetView = GetComponent<T17NetView>();
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 13);
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.RegularPeriodic);
		}
	}

	public void Init()
	{
		AIEventManager.GetInstance().SubscribeToGlobalCallback(AIEvent.EventType.Character_KnockedOut, CallMedics);
		m_MealTimeQueue = new List<Character>();
		RoomBlob firstRoomByLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.MealHall);
		m_MealTimeWaypoints = null;
		if (firstRoomByLocation != null)
		{
			m_MealTimeWaypoints = firstRoomByLocation.GetWaypointList();
			for (int num = m_MealTimeWaypoints.Count - 1; num >= 0; num--)
			{
				if (m_MealTimeWaypoints[num] == null)
				{
					m_MealTimeWaypoints.RemoveAt(num);
				}
			}
		}
		RoutineManager.GetInstance().OnRoutineEnded += OnRoutineEnded;
		m_CrowdNPCs.Sort((AICharacter_CrowdNPC a, AICharacter_CrowdNPC b) => a.GetCrowdSeatingPosition().CompareTo(b.GetCrowdSeatingPosition()));
		if (m_ConcurrentWaving > m_CrowdNPCs.Count)
		{
			m_ConcurrentWaving = m_CrowdNPCs.Count;
		}
		SetUpCrowdAllowedData();
		m_bAllowCrowdAnimatorFeature = false;
		m_bCrowdAnimatorFeatureEnabled = false;
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public void ControlledUpdate()
	{
		m_fNormalisedRotation += UpdateManager.deltaTime * m_fCrowdRotationSpeed;
		while (m_fNormalisedRotation > 1f)
		{
			m_fNormalisedRotation -= 1f;
			int count = m_CrowdNPCs.Count;
			if (count != 0)
			{
				int num = m_CrowdIndex - m_ConcurrentWaving;
				if (num < 0)
				{
					num += count;
				}
				AICharacter_CrowdNPC aICharacter_CrowdNPC = m_CrowdNPCs[num];
				if (aICharacter_CrowdNPC != null)
				{
					aICharacter_CrowdNPC.DoWave(shouldBeActive: false);
				}
				AICharacter_CrowdNPC aICharacter_CrowdNPC2 = m_CrowdNPCs[m_CrowdIndex];
				if (aICharacter_CrowdNPC2 != null)
				{
					aICharacter_CrowdNPC2.DoWave(shouldBeActive: true);
				}
				m_CrowdIndex++;
				if (m_CrowdIndex >= count)
				{
					m_CrowdIndex = 0;
				}
			}
		}
		m_bAllowCrowdAnimatorFeature = false;
		if (RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.ShowTime && CameraManager.GetInstance().GetUsedCameraCount() > 2)
		{
			m_bAllowCrowdAnimatorFeature = true;
		}
		if (m_bAllowCrowdAnimatorFeature && !m_bCrowdAnimatorFeatureEnabled)
		{
			m_bCrowdAnimatorFeatureEnabled = true;
		}
		else if (!m_bAllowCrowdAnimatorFeature && m_bCrowdAnimatorFeatureEnabled)
		{
			int count2 = m_CrowdNPCs.Count;
			for (int i = 0; i < count2; i++)
			{
				if (m_CrowdNPCs[i].IsCrowdAnimatorFeature())
				{
					m_CrowdNPCs[i].SetCrowdAnimatorFeature(banimatorFeature: false);
					m_CrowdNPCs[i].ControllAnimatorFromNPCManager(bEnable: true);
				}
			}
			m_bCrowdAnimatorFeatureEnabled = false;
		}
		if (!m_bCrowdAnimatorFeatureEnabled)
		{
			return;
		}
		int count3 = m_CrowdNPCs.Count;
		for (int j = 0; j < count3; j++)
		{
			if (m_CrowdNPCs[j].IsSeated())
			{
				if (!m_CrowdNPCs[j].IsCrowdAnimatorFeature())
				{
					m_CrowdNPCs[j].SetCrowdAnimatorFeature(banimatorFeature: true);
					m_CrowdNPCs[j].ControllAnimatorFromNPCManager(bEnable: false);
				}
			}
			else if (m_CrowdNPCs[j].IsCrowdAnimatorFeature())
			{
				m_CrowdNPCs[j].SetCrowdAnimatorFeature(banimatorFeature: false);
				m_CrowdNPCs[j].ControllAnimatorFromNPCManager(bEnable: true);
			}
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < m_AICharacters.Count; i++)
		{
			AICharacter aICharacter = m_AICharacters[i];
			if (!(aICharacter == null) && aICharacter.m_AIMovement != null)
			{
				aICharacter.m_AIMovement.CancelCurrentPath();
				if (aICharacter.m_AIMovement.m_Seeker != null)
				{
					aICharacter.m_AIMovement.m_Seeker.ReleaseClaimedPath();
				}
			}
		}
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnRoutineEnded -= OnRoutineEnded;
		}
		m_AICharacters.Clear();
		m_Medics.Clear();
		m_MaintenanceMen.Clear();
		m_Doggies.Clear();
		m_Guards.Clear();
		m_CrowdNPCs.Clear();
	}

	public void AddAICharacter(AICharacter character)
	{
		if (character != null && character.gameObject != null && character.gameObject.activeInHierarchy)
		{
			m_AICharacters.Add(character);
		}
	}

	public void AddMedic(AICharacter medic)
	{
		if (medic != null && medic.gameObject != null && medic.gameObject.activeInHierarchy)
		{
			m_Medics.Add(medic);
		}
	}

	public void AddMaintenanceMan(AICharacter maintenanceMan)
	{
		if (maintenanceMan != null && maintenanceMan.gameObject != null && maintenanceMan.gameObject.activeInHierarchy)
		{
			m_MaintenanceMen.Add(maintenanceMan);
		}
	}

	public void AddDoggie(AICharacter doggie)
	{
		if (doggie != null && doggie.gameObject != null && doggie.gameObject.activeInHierarchy)
		{
			m_Doggies.Add(doggie);
		}
	}

	public void AddGuard(AICharacter guard)
	{
		if (guard != null && guard.gameObject != null && guard.gameObject.activeInHierarchy)
		{
			m_Guards.Add(guard);
		}
	}

	public void AddCrowdNPC(AICharacter_CrowdNPC crowdNPC)
	{
		if (crowdNPC != null && crowdNPC.gameObject != null && crowdNPC.gameObject.activeInHierarchy)
		{
			m_CrowdNPCs.Add(crowdNPC);
		}
	}

	private bool CallMedics(AIEvent aiEvent)
	{
		AICharacter bestNPCForEvent = GetBestNPCForEvent(ref m_Medics, ref aiEvent);
		if (bestNPCForEvent == null)
		{
			return false;
		}
		bestNPCForEvent.AddEvent(aiEvent);
		Character targetCharacter = aiEvent.m_TargetCharacter;
		if (SolitaryManager.GetInstance().IsWantedForSolitary(targetCharacter))
		{
			AIEvent escapingAIEvent = targetCharacter.m_CharacterEventManager.GetEscapingAIEvent();
			bestNPCForEvent.AddEvent(escapingAIEvent);
		}
		else
		{
			bestNPCForEvent.AddEvent(aiEvent);
		}
		if (aiEvent.m_Target != null)
		{
			Character targetCharacter2 = aiEvent.m_TargetCharacter;
			if (targetCharacter2 != null && targetCharacter2.m_CharacterStats.m_bIsPlayer)
			{
				PIPManager.GetInstance().NewPlayerPIPEvent(PIPManager.PIPEventType.MedicCalled, targetCharacter2.m_NetView.viewID, bestNPCForEvent.m_NetView.viewID, 0, 5f);
			}
		}
		return true;
	}

	public void CallGuards(AIEvent aiEvent, float maxDistanceScore = float.MaxValue)
	{
		AICharacter bestNPCForEvent = GetBestNPCForEvent(ref m_Guards, ref aiEvent, eventPenalty: false, maxDistanceScore);
		if (bestNPCForEvent != null)
		{
			bestNPCForEvent.AddEvent(aiEvent);
		}
	}

	public void CallDoggies(AIEvent aiEvent)
	{
		AICharacter bestNPCForEvent = GetBestNPCForEvent(ref m_Doggies, ref aiEvent);
		if (bestNPCForEvent != null)
		{
			bestNPCForEvent.AddEvent(aiEvent);
		}
	}

	public bool CallMaintenanceMen(AIEvent aiEvent)
	{
		AICharacter bestNPCForEvent = GetBestNPCForEvent(ref m_MaintenanceMen, ref aiEvent);
		if (bestNPCForEvent == null)
		{
			return false;
		}
		bestNPCForEvent.AddEvent(aiEvent);
		PIPManager.GetInstance().NewGlobalPIPEvent(PIPManager.PIPEventType.MaintenanceCalled, bestNPCForEvent.m_NetView.viewID, 0, 5f);
		return true;
	}

	public void SubmitReports(List<AICharacter_Guard.ReportData> reports)
	{
		if (m_MaintenanceMen.Count == 0)
		{
			return;
		}
		AICharacter aICharacter = null;
		for (int i = 0; i < reports.Count; i++)
		{
			AIEvent aiEvent = reports[i].m_Event;
			if (aiEvent != null && aiEvent.m_EventData != null && aiEvent.m_EventData.m_eEventType == AIEvent.EventType.Item_ContrabandInContainer)
			{
				continue;
			}
			AICharacter bestNPCForEvent = GetBestNPCForEvent(ref m_MaintenanceMen, ref aiEvent);
			if (bestNPCForEvent != null)
			{
				bestNPCForEvent.AddEvent(aiEvent);
				if (aICharacter == null)
				{
					aICharacter = bestNPCForEvent;
				}
			}
		}
		if (aICharacter != null)
		{
			PIPManager.GetInstance().NewGlobalPIPEvent(PIPManager.PIPEventType.MaintenanceCalled, aICharacter.m_NetView.viewID, 0, 5f);
		}
	}

	public void RespondToKnownEscapeAttempt(Character character)
	{
		if (!character.GetIsKnockedOut())
		{
			for (int i = 0; i < m_Doggies.Count; i++)
			{
				AIEvent escapingAIEvent = character.m_CharacterEventManager.GetEscapingAIEvent();
				m_Doggies[i].AddEvent(escapingAIEvent);
			}
		}
	}

	private AICharacter GetBestNPCForEvent(ref List<AICharacter> NPCs, ref AIEvent aiEvent, bool eventPenalty = true, float maxScore = float.MaxValue)
	{
		if (NPCs == null)
		{
			return null;
		}
		Vector3 position = aiEvent.GetPosition();
		RoomFloor floorFromZ = RoomManager.GetInstance().GetFloorFromZ(position.z);
		Vector3 vector = RoomUtility.WorldToRoomGrid(position, floorFromZ);
		RoomBlob roomB = floorFromZ.LookUpRoom((int)vector.x, (int)vector.y);
		float[] array = new float[NPCs.Count];
		for (int i = 0; i < NPCs.Count; i++)
		{
			AICharacter aICharacter = NPCs[i];
			RoomBlob roomA = aICharacter.m_Character.m_CurrentLocation;
			array[i] = RoomUtility.GetInstance().GetDistanceEstimate(ref roomA, ref roomB);
			List<AIEventMemory> knownEvents = aICharacter.GetKnownEvents(aiEvent.m_EventData.m_eEventType);
			array[i] += ((knownEvents != null && eventPenalty) ? ((float)knownEvents.Count * m_fKnownEventPenalty) : 0f);
		}
		AICharacter result = null;
		float num = maxScore;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] <= num)
			{
				result = NPCs[j];
				num = array[j];
			}
		}
		return result;
	}

	public bool CanDoRoutineSpeech(AICharacter character)
	{
		if (m_RoutineTalkingCharacters == null)
		{
			m_RoutineTalkingCharacters = new RoutineChat[4];
			for (int i = 0; i < 4; i++)
			{
				m_RoutineTalkingCharacters[i] = new RoutineChat();
			}
		}
		int num = -1;
		for (int j = 0; j < 4; j++)
		{
			if (!(m_RoutineTalkingCharacters[j].m_fEpoch < UpdateManager.time))
			{
				continue;
			}
			Transform characterlocation = m_RoutineTalkingCharacters[j].m_Characterlocation;
			if (characterlocation != null)
			{
				if (character.m_Transform == characterlocation)
				{
					return false;
				}
				if (Vector2.SqrMagnitude(characterlocation.position - character.m_Transform.position) < 16f)
				{
					return false;
				}
			}
			num = j;
		}
		if (num != -1)
		{
			m_RoutineTalkingCharacters[num].m_Characterlocation = character.m_Transform;
			m_RoutineTalkingCharacters[num].m_fEpoch = UpdateManager.time + 4f;
			return true;
		}
		return false;
	}

	public void OnRoutineEnded(RoutinesData.Routine routine, bool forceEnd)
	{
		if (routine.m_BaseRoutineType == Routines.MealTime)
		{
			m_MealTimeQueue.Clear();
		}
		if (routine.m_BaseRoutineType == Routines.ShowTime)
		{
			SetUpCrowdAllowedData();
		}
	}

	public void SetUpCrowdAllowedData()
	{
		if (m_CrowdAllowedIndices == null)
		{
			m_CrowdAllowedIndices = new bool[m_CrowdNPCs.Count];
		}
		if (m_CrowdNPCs.Count > 0)
		{
			for (int i = 0; i < m_CrowdNPCs.Count; i++)
			{
				m_CrowdAllowedIndices[i] = true;
			}
			for (int i = 0; i < 30; i++)
			{
				m_CrowdAllowedIndices[UnityEngine.Random.Range(0, m_CrowdNPCs.Count)] = false;
			}
		}
		for (int i = 0; i < 13; i++)
		{
			m_CharactersToAllowToShowTime[i] = -1f;
		}
	}

	public bool GetQueuePosition(Character character, out Vector3 queuePosition, out Directionx4 facingDirection)
	{
		facingDirection = Directionx4.Up;
		if (m_MealTimeWaypoints == null || m_MealTimeWaypoints.Count == 0 || character == null)
		{
			queuePosition = m_vZero;
			return true;
		}
		if (character.GetIsImmobilised())
		{
			queuePosition = character.m_CachedCurrentPosition;
			RemoveFromQueue(character);
			return false;
		}
		int num = 0;
		if (!m_MealTimeQueue.Contains(character))
		{
			int num2 = m_MealTimeQueue.Count - 1;
			Vector3 position = character.m_Transform.position;
			for (int i = 0; i < m_MealTimeQueue.Count; i++)
			{
				if (ShouldWeCutIn(character, i))
				{
					num2 = i;
					break;
				}
			}
			if (num2 >= m_MealTimeQueue.Count - 1)
			{
				m_MealTimeQueue.Add(character);
			}
			else
			{
				m_MealTimeQueue.Insert(num2, character);
			}
			num = num2;
		}
		else
		{
			num = m_MealTimeQueue.IndexOf(character);
			int num3 = num - 1;
			if (num3 >= 0 && ShouldWeCutIn(character, num3))
			{
				RemoveFromQueue(character);
				m_MealTimeQueue.Insert(num3, character);
				num = num3;
			}
		}
		queuePosition = GetPositionInQueue(num);
		facingDirection = GetFacingDirectionInQueue(num);
		if (num == 0)
		{
			Vector3 cachedCurrentPosition = character.m_CachedCurrentPosition;
			float sqrMagnitude = (queuePosition - cachedCurrentPosition).sqrMagnitude;
			if (sqrMagnitude < 0.5f)
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldWeCutIn(Character us, int contestedIndex)
	{
		Character character = m_MealTimeQueue[contestedIndex];
		Vector3 cachedCurrentPosition = us.m_CachedCurrentPosition;
		Vector3 cachedCurrentPosition2 = character.m_CachedCurrentPosition;
		Vector3 positionInQueue = GetPositionInQueue(contestedIndex);
		Vector3 vector = positionInQueue - cachedCurrentPosition;
		Vector3 vector2 = positionInQueue - cachedCurrentPosition2;
		vector.z *= 10f;
		vector2.z *= 10f;
		float sqrMagnitude = vector.sqrMagnitude;
		float sqrMagnitude2 = vector2.sqrMagnitude;
		if (sqrMagnitude2 > 1f && sqrMagnitude / sqrMagnitude2 < 0.8f)
		{
			return true;
		}
		return false;
	}

	public void RemoveFromQueue(Character character)
	{
		if (m_MealTimeQueue.Contains(character))
		{
			m_MealTimeQueue.Remove(character);
		}
	}

	private Vector3 GetPositionInQueue(int index)
	{
		int max = m_MealTimeWaypoints.Count - 1;
		int index2 = Mathf.Clamp(index, 0, max);
		return m_MealTimeWaypoints[index2].GetPosition();
	}

	private Directionx4 GetFacingDirectionInQueue(int index)
	{
		int max = m_MealTimeWaypoints.Count - 1;
		int index2 = Mathf.Clamp(index, 0, max);
		return m_MealTimeWaypoints[index2].m_FacingDirection;
	}

	public float GetCrowdRotation()
	{
		return m_fNormalisedRotation;
	}

	public bool AllowToTakeASeatAtShowTime(int crowdID)
	{
		return m_CrowdAllowedIndices[crowdID];
	}

	public bool AllowToTakePartInShowTime(float characterID)
	{
		bool flag = false;
		int num = -1;
		for (int i = 0; i < 13; i++)
		{
			if (m_CharactersToAllowToShowTime[i] == characterID)
			{
				flag = true;
				break;
			}
			if (num == -1 && m_CharactersToAllowToShowTime[i] == -1f)
			{
				num = i;
			}
		}
		if (!flag && num != -1)
		{
			m_CharactersToAllowToShowTime[num] = characterID;
			flag = true;
		}
		return flag;
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

	public void TriggerAICharacterSerialization()
	{
		Serialize();
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public void Serialize()
	{
		if (!T17NetManager.IsMasterClient || !(NetPrisonViewDetails.Instance != null))
		{
			return;
		}
		UpdateManager.AquireHeavyCpuLock();
		if (m_NetSaveData.aiCharacterID == null)
		{
			m_NetSaveData.aiCharacterID = new List<byte>();
		}
		m_NetSaveData.aiCharacterID.Clear();
		if (m_NetSaveBitWriter == null)
		{
			m_NetSaveBitWriter = new SlowBitStreamWriter(m_NetSaveData.aiCharacterID);
		}
		m_NetSaveBitWriter.Reset(m_NetSaveData.aiCharacterID);
		if (m_NetSaveData.aiCharacterData == null)
		{
			m_NetSaveData.aiCharacterData = new List<AICharacter.CharacterSaveData>();
		}
		m_NetSaveData.aiCharacterData.Clear();
		for (int i = 0; i < m_AICharacters.Count; i++)
		{
			AICharacter aICharacter = m_AICharacters[i];
			if (!(aICharacter == null))
			{
				AICharacter.CharacterSaveData item = aICharacter.SerialiseCharacter();
				int viewID = aICharacter.m_NetView.viewID;
				m_NetSaveBitWriter.Write((uint)viewID, 12);
				m_NetSaveData.aiCharacterData.Add(item);
			}
		}
		m_NetSaveData.m_CrowdAllowedIndicies = m_CrowdAllowedIndices;
		m_NetSaveData.m_CharactersToAllowToShowTime = m_CharactersToAllowToShowTime;
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		int count = m_NetSaveData.aiCharacterID.Count;
		binaryWriter.Write(count);
		for (int j = 0; j < count; j++)
		{
			binaryWriter.Write(m_NetSaveData.aiCharacterID[j]);
		}
		int count2 = m_NetSaveData.aiCharacterData.Count;
		binaryWriter.Write(count2);
		for (int k = 0; k < count2; k++)
		{
			if (m_NetSaveData.aiCharacterData[k].m_EventMemories == null)
			{
				binaryWriter.Write(0);
			}
			else
			{
				binaryWriter.Write(m_NetSaveData.aiCharacterData[k].m_EventMemories.Length);
				binaryWriter.Write(m_NetSaveData.aiCharacterData[k].m_EventMemories);
			}
			if (m_NetSaveData.aiCharacterData[k].m_EventsToReport == null)
			{
				binaryWriter.Write(0);
			}
			else
			{
				binaryWriter.Write(m_NetSaveData.aiCharacterData[k].m_EventsToReport.Length);
				binaryWriter.Write(m_NetSaveData.aiCharacterData[k].m_EventsToReport);
			}
			binaryWriter.Write(m_NetSaveData.aiCharacterData[k].m_Personality);
			binaryWriter.Write(m_NetSaveData.aiCharacterData[k].m_bReleasedKeySpawnedOnUs);
		}
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.Receivers = ReceiverGroup.Others;
		raiseEventOptions.Encrypt = true;
		PhotonNetwork.RaiseEvent(17, memoryStream.ToArray(), sendReliable: false, raiseEventOptions);
	}

	private void OnBecameMasterClient()
	{
		string error = string.Empty;
		if (DeserializeBinary(m_NetSaveData, ref error))
		{
		}
	}

	public bool Deserialize(string data, ref string error)
	{
		return true;
	}

	public bool Real_Deserialize(ref string error)
	{
		string serializationData = GetSerializationData();
		NetSaveData netSaveData = JsonUtility.FromJson<NetSaveData>(serializationData);
		return DeserializeBinary(netSaveData, ref error);
	}

	public void PostRealDeserialize(bool isFromSaveFile)
	{
		int i = 0;
		for (int count = m_AICharacters.Count; i < count; i++)
		{
			m_AICharacters[i].Post_RealDeserialize(isFromSaveFile);
		}
	}

	protected virtual void OnDestroy()
	{
		s_Instance = null;
		T17NetManager.OnBecameMasterClient -= OnBecameMasterClient;
		T17NetRoomGameView.OnRoomSignalEvent -= OnEvent;
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.RegularPeriodic);
		}
		m_NetView = null;
	}

	public bool DeserializeBinary(NetSaveData netSaveData, ref string error)
	{
		if (netSaveData == null)
		{
			m_NetSaveData.aiCharacterID.Clear();
			m_NetSaveData.aiCharacterData.Clear();
			return true;
		}
		m_NetSaveData = netSaveData;
		byte[] buffer = m_NetSaveData.aiCharacterID.ToArray();
		if (m_NetSaveBitReader == null)
		{
			m_NetSaveBitReader = new BitStreamReader(buffer);
		}
		else
		{
			m_NetSaveBitReader.Reset(buffer);
		}
		for (int i = 0; i < m_NetSaveData.aiCharacterData.Count; i++)
		{
			int viewID = m_NetSaveBitReader.ReadUInt16(12);
			AICharacter aICharacter = T17NetView.Find<AICharacter>(viewID);
			if (!(aICharacter == null))
			{
				AICharacter.CharacterSaveData data = m_NetSaveData.aiCharacterData[i];
				aICharacter.DeserialiseCharacter(data);
			}
		}
		if (m_NetSaveData.m_CrowdAllowedIndicies != null && m_NetSaveData.m_CrowdAllowedIndicies.Length > 0)
		{
			m_CrowdAllowedIndices = m_NetSaveData.m_CrowdAllowedIndicies;
		}
		if (m_NetSaveData.m_CharactersToAllowToShowTime != null && m_NetSaveData.m_CharactersToAllowToShowTime.Length >= 13)
		{
			m_CharactersToAllowToShowTime = m_NetSaveData.m_CharactersToAllowToShowTime;
		}
		return true;
	}

	public string CreateSnapshot()
	{
		Serialize();
		return JsonUtility.ToJson(m_NetSaveData);
	}

	public void StartedFromSnapshot()
	{
	}

	private void OnEvent(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs)
	{
		if (roomSignal != T17NetConfig.NetEventTypes.NPCManager)
		{
			return;
		}
		byte[] buffer = (byte[])payload;
		if (m_NetSaveData == null)
		{
			m_NetSaveData = new NetSaveData();
		}
		using MemoryStream input = new MemoryStream(buffer);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (m_NetSaveData.aiCharacterID == null)
		{
			m_NetSaveData.aiCharacterID = new List<byte>(num);
		}
		else
		{
			m_NetSaveData.aiCharacterID.Clear();
			m_NetSaveData.aiCharacterID.Capacity = num;
		}
		for (int i = 0; i < num; i++)
		{
			byte item = binaryReader.ReadByte();
			m_NetSaveData.aiCharacterID.Add(item);
		}
		int num2 = binaryReader.ReadInt32();
		if (m_NetSaveData.aiCharacterData == null)
		{
			m_NetSaveData.aiCharacterData = new List<AICharacter.CharacterSaveData>(num2);
		}
		else
		{
			m_NetSaveData.aiCharacterData.Clear();
			m_NetSaveData.aiCharacterData.Capacity = num2;
		}
		for (int j = 0; j < num2; j++)
		{
			AICharacter.CharacterSaveData characterSaveData = new AICharacter.CharacterSaveData();
			int num3 = binaryReader.ReadInt32();
			if (num3 > 0)
			{
				characterSaveData.m_EventMemories = binaryReader.ReadBytes(num3);
			}
			num3 = binaryReader.ReadInt32();
			if (num3 > 0)
			{
				characterSaveData.m_EventsToReport = binaryReader.ReadBytes(num3);
			}
			characterSaveData.m_Personality = binaryReader.ReadInt32();
			characterSaveData.m_bReleasedKeySpawnedOnUs = binaryReader.ReadBoolean();
			m_NetSaveData.aiCharacterData.Add(characterSaveData);
		}
	}
}
