using System;
using System.Collections.Generic;
using System.Linq;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class CCTVCamera : T17MonoBehaviour
{
	public class ReoccuringHeatEvent
	{
		public Character m_CharacterResponsible;

		public float m_ReoccuringHeat;

		public float m_ReoccuringTime;

		public float m_RemainingTime;
	}

	public class CCTVDeserializer : IDeserializable
	{
		public bool Deserialize(string data, ref string error)
		{
			return GlobalDeserialize(data, ref error);
		}

		public string GetSerializationData()
		{
			return NetPrisonViewDetails.Instance.CctvCameraData;
		}
	}

	[Serializable]
	public class CCTVCamerasSaveData
	{
		public List<string> m_CctvCameraSerializedData = new List<string>();
	}

	[Serializable]
	private class SaveData_CCTVCamera_V1
	{
		public int NID;

		public float BLD;

		public float ASD;

		public int TID;

		public bool ATV;

		public bool BND;

		public float BT;

		public float RT;

		public float NST;

		public int DIR;

		public bool MS;
	}

	private static Dictionary<Vector3, CCTVCamera> m_CctvCameras = new Dictionary<Vector3, CCTVCamera>();

	private bool m_Enabled = true;

	private Transform m_VisionTrigger;

	private CharacterUtil m_CharacterUtil;

	private Animator m_Animator;

	private Light m_Light;

	private LocationEventManager m_LocationEventManager;

	private T17NetView m_NetView;

	private AIConfig m_AiConfig;

	private Transform m_Transform;

	public bool m_IsHorizontal = true;

	public float m_MaxRotationAngle;

	public float m_Fov;

	public float m_VisionDistance;

	public float m_BlindspotDistance = 1f;

	private Quaternion m_VisionDirection;

	public float m_Speed = 1f;

	public float m_RestTime = 3f;

	private float m_CurrentRestTime;

	private int m_DirectionSign = 1;

	private float m_SweepTimeNormalised = 1f;

	private bool m_MidpointStop;

	public bool m_PlaybackStartedAfterEnabling;

	public bool m_bReverseCameraDirection;

	private int m_FloorIndex;

	private Vector3 m_TilePosition;

	private Vector3 m_Position;

	public List<AIEvent.EventType> m_ReportableEvents = new List<AIEvent.EventType>();

	public float m_HeatIncrease;

	public int m_AlertnessIncrease;

	public LayerMask m_VisionMask;

	private List<Character> m_NearbyCharacters = new List<Character>();

	private List<Character> m_AlertedCharacters = new List<Character>();

	private Character m_TrackingCharacter;

	private const int kMaxCycleSweep = 2;

	private const int kMaxCycleChar = 4;

	private static int m_uniqueIDcounter = 0;

	private int m_updateCycleSweep;

	private int m_updateCycleChar;

	private bool m_bIsLightsOut;

	private List<ReoccuringHeatEvent> m_ReoccuringHeatEvents = new List<ReoccuringHeatEvent>();

	private bool m_CameraBound;

	private float m_BoundTime;

	public Vector3 m_EffectOffsetPosition;

	private static CCTVCamerasSaveData m_NetCctvCamerasSaveData = null;

	private static Dictionary<int, string> m_CctvCamerasNetData = new Dictionary<int, string>();

	public static CCTVCamera GetCameraAtTile(Vector3 tilePos)
	{
		CCTVCamera value = null;
		m_CctvCameras.TryGetValue(tilePos, out value);
		return value;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Transform = base.transform;
		m_CharacterUtil = GetComponent<CharacterUtil>();
		m_LocationEventManager = GetComponent<LocationEventManager>();
		m_NetView = GetComponent<T17NetView>();
		m_Light = GetComponentInChildren<Light>();
		m_Animator = GetComponentInChildren<Animator>();
		m_Light.spotAngle = m_Fov * 57.29578f * 2f;
	}

	private void Start()
	{
		m_VisionTrigger = m_Transform.FindChild("VisionTrigger");
		m_Position = m_Transform.position;
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			FloorManager.Floor floor = instance.FindFloorAtZ(m_Position.z);
			m_FloorIndex = floor.m_FloorIndex;
			int row = 0;
			int column = 0;
			if (instance.GetTileGridPoint(floor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Position, out row, out column))
			{
				m_TilePosition.x = row;
				m_TilePosition.y = column;
				m_TilePosition.z = m_FloorIndex;
				if (GetCameraAtTile(m_TilePosition) != null)
				{
					m_CctvCameras.Remove(m_TilePosition);
				}
				m_CctvCameras.Add(m_TilePosition, this);
			}
		}
		if (ReverseCameraDirection())
		{
			m_VisionDistance = 0f - m_VisionDistance;
		}
		m_updateCycleSweep = m_uniqueIDcounter % 2;
		m_updateCycleChar = m_uniqueIDcounter % 4;
		m_uniqueIDcounter++;
		RoutineManager instance2 = RoutineManager.GetInstance();
		if (instance2 != null)
		{
			instance2.OnRoutineChanged += RoutineManager_OnRoutineChanged;
		}
	}

	private bool ReverseCameraDirection()
	{
		return m_bReverseCameraDirection || m_Transform.localScale.x < 0f;
	}

	protected virtual void OnDestroy()
	{
		m_CctvCameras.Remove(m_TilePosition);
		m_NetView = null;
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= RoutineManager_OnRoutineChanged;
		}
	}

	public static void Cleanup()
	{
		m_CctvCameras.Clear();
		m_NetCctvCamerasSaveData = null;
		m_CctvCamerasNetData.Clear();
	}

	private void RoutineManager_OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		m_bIsLightsOut = newRoutine.m_BaseRoutineType == Routines.LightsOut;
	}

	private void Update()
	{
		if (m_CameraBound)
		{
			m_BoundTime -= UpdateManager.deltaTime;
			if (m_BoundTime <= 0f)
			{
				ToggleBindRPC(bind: false, 0f);
			}
			return;
		}
		if (m_Animator.enabled && !m_PlaybackStartedAfterEnabling)
		{
			m_Animator.StartPlayback();
			m_PlaybackStartedAfterEnabling = true;
		}
		else if (!m_Animator.enabled && m_PlaybackStartedAfterEnabling)
		{
			m_PlaybackStartedAfterEnabling = false;
		}
		bool flag = true;
		if (PrisonPowerManager.GetInstance() != null)
		{
			flag = PrisonPowerManager.GetInstance().PowerIsActive();
		}
		ToggleActivation(flag);
		if (!flag)
		{
			return;
		}
		if (UpdateManager.frameCount % 2 == m_updateCycleSweep)
		{
			if (m_TrackingCharacter != null)
			{
				Update_TrackCharacter();
			}
			else
			{
				Update_Sweep(UpdateManager.deltaTime * 2f);
			}
		}
		UpdateDirection();
		if (UpdateManager.frameCount % 4 != m_updateCycleChar)
		{
			return;
		}
		if (T17NetManager.IsMasterClient)
		{
			FindNearbyCharacters();
			CheckCharacters();
		}
		float num = UpdateManager.deltaTime * 4f;
		for (int i = 0; i < m_ReoccuringHeatEvents.Count; i++)
		{
			ReoccuringHeatEvent reoccuringHeatEvent = m_ReoccuringHeatEvents[i];
			reoccuringHeatEvent.m_RemainingTime -= num;
			if (reoccuringHeatEvent.m_RemainingTime <= 0f)
			{
				reoccuringHeatEvent.m_CharacterResponsible.m_CharacterStats.IncreaseHeat(reoccuringHeatEvent.m_ReoccuringHeat);
				reoccuringHeatEvent.m_RemainingTime = reoccuringHeatEvent.m_ReoccuringTime;
			}
		}
	}

	private void Update_Sweep(float deltaTime)
	{
		if (m_CurrentRestTime <= 0f)
		{
			m_SweepTimeNormalised += m_Speed * deltaTime * (float)m_DirectionSign;
			if (Mathf.Abs(m_SweepTimeNormalised) > 1f)
			{
				m_CurrentRestTime = m_RestTime;
				m_DirectionSign *= -1;
				m_MidpointStop = false;
				m_SweepTimeNormalised = Mathf.Clamp(m_SweepTimeNormalised, -1f, 1f);
				m_Animator.speed = 0f;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_CCTV_Movement, base.gameObject);
			}
			else if (((m_DirectionSign == 1 && m_SweepTimeNormalised > 0f) || (m_DirectionSign == -1 && m_SweepTimeNormalised < 0f)) && !m_MidpointStop)
			{
				m_MidpointStop = true;
				m_CurrentRestTime = m_RestTime;
				m_Animator.speed = 0f;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_CCTV_Movement, base.gameObject);
			}
			m_VisionDirection = Quaternion.Euler(0f, 0f, m_MaxRotationAngle * m_SweepTimeNormalised);
		}
		else
		{
			m_CurrentRestTime -= deltaTime;
			if (m_CurrentRestTime <= 0f)
			{
				m_Animator.speed = 1f;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_CCTV_Movement, base.gameObject);
			}
		}
	}

	private void Update_TrackCharacter()
	{
		Vector3 normalized = (m_Position - m_TrackingCharacter.transform.position).normalized;
		if (m_IsHorizontal)
		{
			m_VisionTrigger.up = normalized;
		}
		else
		{
			m_VisionTrigger.right = -normalized;
		}
		Vector3 eulerAngles = m_VisionTrigger.rotation.eulerAngles;
		if (eulerAngles.z > 180f)
		{
			eulerAngles.z = Mathf.Clamp(eulerAngles.z, 360f - m_MaxRotationAngle, 360f);
			m_SweepTimeNormalised = 0f - eulerAngles.z / (360f - m_MaxRotationAngle);
		}
		else
		{
			eulerAngles.z = Mathf.Clamp(eulerAngles.z, 0f, m_MaxRotationAngle);
			m_SweepTimeNormalised = eulerAngles.z / m_MaxRotationAngle;
		}
		m_VisionDirection = Quaternion.Euler(0f, 0f, eulerAngles.z);
		m_Animator.speed = 0f;
	}

	private void CheckCharacters()
	{
		for (int i = 0; i < m_NearbyCharacters.Count; i++)
		{
			Character character = m_NearbyCharacters[i];
			if (LineOfSight(character.transform.position))
			{
				if (!m_AlertedCharacters.Contains(character) && m_TrackingCharacter == null && IsCharacterMisbehaving(character))
				{
					SetAlertedRPC(character);
				}
			}
			else if (m_AlertedCharacters.Contains(character))
			{
				EndAlertedRPC(character);
			}
		}
	}

	private void UpdateDirection()
	{
		float num = (m_SweepTimeNormalised - -1f) / 2f;
		AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
		m_Animator.Play(currentAnimatorStateInfo.shortNameHash, 0, 1f - num);
		if (ReverseCameraDirection())
		{
			m_VisionTrigger.rotation = Quaternion.Euler(m_VisionDirection.eulerAngles.x, m_VisionDirection.eulerAngles.y, 0f - m_VisionDirection.eulerAngles.z);
		}
		else
		{
			m_VisionTrigger.rotation = m_VisionDirection;
		}
	}

	private void FindNearbyCharacters()
	{
		short row = 0;
		short column = 0;
		if (AIEventManager.GetInstance() == null)
		{
			return;
		}
		AIEventManager.GetInstance().GetBucketPosition(m_Position, out row, out column);
		m_NearbyCharacters.Clear();
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				List<EventManager> eventManagers = AIEventManager.GetInstance().GetEventManagers(row + i, column + j, m_FloorIndex);
				if (eventManagers == null)
				{
					continue;
				}
				for (int k = 0; k < eventManagers.Count; k++)
				{
					InmateEventManager inmateEventManager = eventManagers[k] as InmateEventManager;
					Character character = null;
					if (inmateEventManager != null)
					{
						character = inmateEventManager.m_Character;
					}
					if (character != null)
					{
						float sqrMagnitude = (m_Position - character.m_CachedCurrentPosition).sqrMagnitude;
						if (sqrMagnitude <= m_VisionDistance * m_VisionDistance)
						{
							m_NearbyCharacters.Add(character);
						}
					}
				}
			}
		}
		m_NearbyCharacters.AddRange(m_AlertedCharacters);
	}

	public void SetAlertedRPC(Character character)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_NetView.PostLevelLoadRPC("RPC_SetAlertedState", NetTargets.All, true, character.m_NetView.viewID);
		}
	}

	public void SetAlertedForCharacter(int characterID)
	{
		Character character = null;
		PhotonView photonView = PhotonView.Find(characterID);
		if (photonView != null)
		{
			character = photonView.GetComponent<Character>();
		}
		if (T17NetManager.IsMasterClient)
		{
			AIEvent investigateObjectEvent = m_LocationEventManager.GetInvestigateObjectEvent();
			NPCManager.GetInstance().CallGuards(investigateObjectEvent);
			if (character.IsPlayer())
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("CCTV Reported", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " CCTV Reported", investigateObjectEvent.m_EventData.m_eEventType.ToString(), 0L);
			}
		}
		m_AlertedCharacters.Add(character);
		m_TrackingCharacter = character;
		m_Animator.SetFloat("Blend", 1f);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_CCTV_Scan, base.gameObject);
	}

	public void EndAlertedRPC(Character character)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_NetView.PostLevelLoadRPC("RPC_SetAlertedState", NetTargets.All, false, character.m_NetView.viewID);
		}
	}

	[PunRPC]
	public void RPC_SetAlertedState(bool isAlerted, int concerningCharacterId)
	{
		if (isAlerted)
		{
			SetAlertedForCharacter(concerningCharacterId);
		}
		else
		{
			EndAlertedForCharacter(concerningCharacterId);
		}
	}

	public void EndAlertedForCharacter(int characterID)
	{
		Character character = null;
		PhotonView photonView = PhotonView.Find(characterID);
		if (photonView != null)
		{
			character = photonView.GetComponent<Character>();
		}
		m_AlertedCharacters.Remove(character);
		int num = 0;
		while (num < m_ReoccuringHeatEvents.Count)
		{
			ReoccuringHeatEvent reoccuringHeatEvent = m_ReoccuringHeatEvents[num];
			if (reoccuringHeatEvent.m_CharacterResponsible == character)
			{
				m_ReoccuringHeatEvents.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		if (m_AlertedCharacters.Count == 0)
		{
			if (m_TrackingCharacter != null)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_CCTV_Deactivate, base.gameObject);
			}
			m_Animator.SetFloat("Blend", 0f);
			m_TrackingCharacter = null;
			m_SweepTimeNormalised = 0f;
			m_DirectionSign = 1;
			m_CurrentRestTime = 0f;
		}
		else if (character == m_TrackingCharacter)
		{
			m_TrackingCharacter = m_AlertedCharacters[0];
		}
	}

	private void ToggleActivation(bool active)
	{
		if (m_Enabled != active)
		{
			if (active)
			{
				m_Animator.SetFloat("Blend", 0f);
			}
			else
			{
				m_Animator.SetFloat("Blend", 0.5f);
				m_TrackingCharacter = null;
				m_NearbyCharacters.Clear();
				m_AlertedCharacters.Clear();
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_CCTV_Deactivate, base.gameObject);
			}
			m_Light.enabled = active;
			m_Enabled = active;
		}
	}

	public void ToggleBindRPC(bool bind, float bindDuration)
	{
		m_NetView.RPC("RPC_ToggleBind", NetTargets.All, bind, bindDuration);
	}

	[PunRPC]
	private void RPC_ToggleBind(bool bind, float bindDuration)
	{
		m_CameraBound = bind;
		m_Animator.SetBool("IsBound", bind);
		m_BoundTime = bindDuration;
		m_Light.enabled = !bind;
		UpdateNetPrisonViewData(this);
	}

	private bool IsCharacterMisbehaving(Character character)
	{
		List<AIEvent> visibleEvents = character.m_CharacterEventManager.GetVisibleEvents();
		if (visibleEvents == null || character.GetIsKnockedOut())
		{
			return false;
		}
		for (int i = 0; i < visibleEvents.Count; i++)
		{
			Character characterResponsible = visibleEvents[i].m_CharacterResponsible;
			if (!(characterResponsible != null) || characterResponsible.m_CharacterRole != 0)
			{
				continue;
			}
			AIEventData eventData = visibleEvents[i].m_EventData;
			if (!m_ReportableEvents.Contains(eventData.m_eEventType))
			{
				continue;
			}
			if (characterResponsible.m_bIsDisguised)
			{
				if (m_AiConfig == null)
				{
					m_AiConfig = ConfigManager.GetInstance().aiConfig;
				}
				if (m_AiConfig.DisguiseableEvents.Contains(eventData.m_eEventType))
				{
					continue;
				}
			}
			if (eventData.m_ReoccuringHeatTime > 0f)
			{
				ReoccuringHeatEvent reoccuringHeatEvent = new ReoccuringHeatEvent();
				reoccuringHeatEvent.m_ReoccuringHeat = eventData.m_GuardHeatIncrease;
				reoccuringHeatEvent.m_ReoccuringTime = eventData.m_ReoccuringHeatTime;
				reoccuringHeatEvent.m_CharacterResponsible = characterResponsible;
				reoccuringHeatEvent.m_RemainingTime = eventData.m_ReoccuringHeatTime;
				m_ReoccuringHeatEvents.Add(reoccuringHeatEvent);
			}
			IncreaseCharactersHeat(characterResponsible);
			return true;
		}
		if (m_bIsLightsOut && !character.m_bIsDisguised && character.m_bIsMissing)
		{
			IncreaseCharactersHeat(character);
			return true;
		}
		return false;
	}

	private void IncreaseCharactersHeat(Character charResponsible)
	{
		charResponsible.m_CharacterStats.IncreaseHeat(m_HeatIncrease);
		if (charResponsible.m_CharacterStats.m_bIsPlayer)
		{
			EffectManager.PlayEffect(EffectManager.effectType.HeatIncreased, m_Position + m_EffectOffsetPosition);
		}
	}

	public bool LineOfSight(Vector3 toPosition, bool useFoV = true)
	{
		Vector3 vector = m_Position + m_CharacterUtil.m_vEyeHeight;
		Vector3 normalized = (toPosition - vector).normalized;
		float distance = Vector3.Distance(vector, toPosition);
		if (!InFieldofViewCheck(normalized, distance, useFoV))
		{
			return false;
		}
		int num = EscapistsRaycast.RaycastAll(vector, normalized, distance, m_VisionMask, QueryTriggerInteraction.Ignore);
		bool result = true;
		Collider collider = null;
		for (int i = 0; i < num; i++)
		{
			collider = EscapistsRaycast.RaycastHitList[i].collider;
			if (!collider.isTrigger)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool InFieldofViewCheck(Vector2 direction, float distance, bool useFoV = true)
	{
		if (distance > Mathf.Abs(m_VisionDistance) || distance < m_BlindspotDistance)
		{
			return false;
		}
		if (!useFoV)
		{
			return true;
		}
		Vector2 rhs = (m_IsHorizontal ? ((Vector2)(-m_VisionTrigger.up)) : ((!ReverseCameraDirection()) ? ((Vector2)m_VisionTrigger.right) : ((Vector2)(-m_VisionTrigger.right))));
		float f = Mathf.Clamp(Vector2.Dot(direction, rhs), -1f, 1f);
		float num = Mathf.Acos(f);
		return num < m_Fov * 2f;
	}

	public string Serialize()
	{
		SaveData_CCTVCamera_V1 saveData_CCTVCamera_V = new SaveData_CCTVCamera_V1();
		saveData_CCTVCamera_V.NID = m_NetView.viewID;
		saveData_CCTVCamera_V.BLD = m_Animator.GetFloat("Blend");
		saveData_CCTVCamera_V.ASD = m_Animator.speed;
		saveData_CCTVCamera_V.TID = ((!(m_TrackingCharacter != null)) ? (-1) : m_TrackingCharacter.m_NetView.viewID);
		saveData_CCTVCamera_V.ATV = m_Enabled;
		saveData_CCTVCamera_V.BND = m_CameraBound;
		saveData_CCTVCamera_V.BT = m_BoundTime;
		saveData_CCTVCamera_V.RT = m_CurrentRestTime;
		saveData_CCTVCamera_V.NST = m_SweepTimeNormalised;
		saveData_CCTVCamera_V.DIR = m_DirectionSign;
		saveData_CCTVCamera_V.MS = m_MidpointStop;
		return JsonUtility.ToJson(saveData_CCTVCamera_V);
	}

	private bool Restore(SaveData_CCTVCamera_V1 saveData)
	{
		if (saveData == null)
		{
			return false;
		}
		m_Animator.speed = saveData.ASD;
		m_TrackingCharacter = T17NetView.Find<Character>(saveData.TID);
		m_Enabled = saveData.ATV;
		m_CameraBound = saveData.BND;
		m_BoundTime = saveData.BT;
		m_CurrentRestTime = saveData.RT;
		m_SweepTimeNormalised = saveData.NST;
		m_DirectionSign = saveData.DIR;
		m_MidpointStop = saveData.MS;
		m_Animator.SetFloat("Blend", saveData.BLD);
		m_Animator.SetBool("IsBound", m_CameraBound);
		if (!m_Enabled || m_CameraBound)
		{
			m_Light.enabled = false;
		}
		return true;
	}

	public static bool GlobalDeserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		CCTVCamerasSaveData cCTVCamerasSaveData = null;
		try
		{
			cCTVCamerasSaveData = JsonUtility.FromJson<CCTVCamerasSaveData>(data);
		}
		catch
		{
			error = "GlobalDeserialize: JSON data is corrupt";
			return false;
		}
		for (int i = 0; i < cCTVCamerasSaveData.m_CctvCameraSerializedData.Count; i++)
		{
			string json = cCTVCamerasSaveData.m_CctvCameraSerializedData[i];
			SaveData_CCTVCamera_V1 saveData_CCTVCamera_V = JsonUtility.FromJson<SaveData_CCTVCamera_V1>(json);
			if (saveData_CCTVCamera_V != null)
			{
				CCTVCamera cCTVCamera = T17NetView.Find<CCTVCamera>(saveData_CCTVCamera_V.NID);
				if (cCTVCamera != null)
				{
					cCTVCamera.Restore(saveData_CCTVCamera_V);
				}
			}
		}
		return true;
	}

	private static void UpdateNetPrisonViewData(CCTVCamera cctvCamera)
	{
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			string value = cctvCamera.Serialize();
			if (m_NetCctvCamerasSaveData == null)
			{
				m_NetCctvCamerasSaveData = new CCTVCamerasSaveData();
			}
			int viewID = cctvCamera.m_NetView.viewID;
			if (m_CctvCamerasNetData.ContainsKey(viewID))
			{
				m_CctvCamerasNetData[viewID] = value;
			}
			else
			{
				m_CctvCamerasNetData.Add(viewID, value);
			}
			if (NetPrisonViewDetails.Instance != null)
			{
				m_NetCctvCamerasSaveData.m_CctvCameraSerializedData = m_CctvCamerasNetData.Values.ToList();
				NetPrisonViewDetails.Instance.CctvCameraData = JsonUtility.ToJson(m_NetCctvCamerasSaveData);
			}
		}
	}
}
