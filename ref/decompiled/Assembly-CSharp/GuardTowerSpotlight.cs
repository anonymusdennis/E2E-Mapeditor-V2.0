using System.Collections.Generic;
using System.Diagnostics;
using BitStream;
using UnityEngine;

public class GuardTowerSpotlight : T17MonoBehaviour, IControlledUpdate
{
	public class ReccuringDisuisedHeatPODO
	{
		public Character m_CharacterResponsible;

		public float m_ReoccuringHeat;

		public float m_ReoccuringTime;

		public float m_RemainingTime;
	}

	private const float COLLIDER_FLOOR_OVERLAP = 2f;

	private GuardTowerManager m_GuardTowerManager;

	private CustomLight m_Light;

	private Transform m_Transform;

	private GuardTower m_MyGuardTower;

	private int m_MyIndexInGuardTower;

	public float m_Size = 50f;

	public float m_Intensity = 5f;

	public Color m_Colour = Color.white;

	public float m_PatrolSpeed = 1f;

	public float m_FollowSpeed = 5f;

	private Vector2 m_FollowDirection;

	public float m_DirectionUpdateTime = 0.5f;

	private float m_directionRefresh;

	private float m_currentFollowSpeed;

	private float m_currentDirectionUpdateTime;

	public float m_trackingFalloffRate = 10f;

	public bool m_SpotlightActive;

	private PatrolPath m_PatrolPath;

	private Vector3 m_vFromPosition = Vector2.zero;

	private bool m_bWaiting;

	private int m_iCurrentWaypoint;

	private float m_fTimeToPoint;

	private float m_fCurrentTimePassed;

	private float m_fSqrCollisionSize;

	private PatrolPath.PathNode m_CurrentWaypoint;

	private Character m_FollowingCharacter;

	private List<Character> m_InView = new List<Character>();

	private bool m_bAllowUpdatesDuringRestore = true;

	private bool m_bShouldReevaluateOnTriggerStay;

	private const int TRIGGER_STAY_REEVALUATE_INTERVAL = 30;

	private int m_FramesUntilTriggerStayEvaluation;

	private List<ReccuringDisuisedHeatPODO> m_DisguisedCharactersInLight = new List<ReccuringDisuisedHeatPODO>();

	public Character FollowingCharacter => m_FollowingCharacter;

	protected override void Awake()
	{
		base.Awake();
		m_Transform = base.transform;
		m_Light = GetComponent<CustomLight>();
		m_PatrolPath = GetComponent<PatrolPath>();
		if (m_Light != null)
		{
			m_Light.SetColour(m_Colour);
			m_Light.SetIntensity(m_Intensity);
			m_Light.m_Size = m_Size;
			if (LevelScript.GetInstance() != null && LevelScript.GetInstance().m_LevelSetup != null && LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
			{
				m_Light.m_lightArea = CustomLight.LightArea.Everywhere;
			}
			else
			{
				m_Light.m_lightArea = CustomLight.LightArea.OutdoorsOnly;
			}
			InitColliderWithLight(m_Light);
		}
	}

	private void InitColliderWithLight(CustomLight light)
	{
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		float z = light.m_Range / 2f;
		component.center = new Vector3(component.center.x, component.center.y, z);
		float height = light.m_Range + 2f;
		component.height = height;
		m_fSqrCollisionSize = component.radius * component.radius + 2f;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_GuardTowerManager = GuardTowerManager.GetInstance();
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.RapidPeriodic);
		}
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		m_GuardTowerManager = null;
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.RapidPeriodic);
		}
		m_Light = null;
		m_Transform = null;
		m_MyGuardTower = null;
		m_PatrolPath = null;
		m_FollowingCharacter = null;
		m_InView.Clear();
		m_DisguisedCharactersInLight.Clear();
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
		m_FramesUntilTriggerStayEvaluation--;
		if (m_FramesUntilTriggerStayEvaluation <= 0)
		{
			m_bShouldReevaluateOnTriggerStay = true;
			m_FramesUntilTriggerStayEvaluation = 30;
		}
		else
		{
			m_bShouldReevaluateOnTriggerStay = false;
		}
	}

	public void ControlledUpdate()
	{
		if (!IsInited() || !(m_MyGuardTower != null) || !m_MyGuardTower.IsInited() || !m_bAllowUpdatesDuringRestore || !m_SpotlightActive)
		{
			return;
		}
		if (m_FollowingCharacter != null)
		{
			m_directionRefresh -= UpdateManager.deltaTime;
			if (m_directionRefresh <= 0f)
			{
				Vector3 position = m_FollowingCharacter.transform.position;
				Vector3 position2 = m_Transform.position;
				position2.z = position.z;
				if ((position - position2).sqrMagnitude < m_Size * m_Size)
				{
					Vector3 vector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
					position += vector;
					m_currentFollowSpeed = Mathf.Lerp(m_currentFollowSpeed, 0.3f, m_trackingFalloffRate * UpdateManager.deltaTime);
					m_currentDirectionUpdateTime = Mathf.Lerp(m_currentDirectionUpdateTime, 0.1f, m_DirectionUpdateTime * UpdateManager.deltaTime);
				}
				else
				{
					m_currentFollowSpeed = m_FollowSpeed;
					m_currentDirectionUpdateTime = m_DirectionUpdateTime;
				}
				m_FollowDirection = position - m_Transform.position;
				m_directionRefresh = m_currentDirectionUpdateTime;
			}
			MoveInDirection(m_currentFollowSpeed, 0f);
		}
		else if (T17NetManager.IsMasterClient || m_fTimeToPoint > 0f)
		{
			m_fCurrentTimePassed += UpdateManager.deltaTime;
			while (m_fCurrentTimePassed >= m_fTimeToPoint)
			{
				if (T17NetManager.IsMasterClient)
				{
					m_fCurrentTimePassed -= m_fTimeToPoint;
					RequestNextWaypoint();
					continue;
				}
				m_fCurrentTimePassed = m_fTimeToPoint;
				break;
			}
			Vector3 position3 = Vector3.Lerp(m_vFromPosition, m_CurrentWaypoint.m_vNodePos, m_fCurrentTimePassed / m_fTimeToPoint);
			position3.z = m_Transform.position.z;
			m_Transform.position = position3;
		}
		Vector2 vector2 = m_Transform.position;
		for (int num = m_InView.Count - 1; num >= 0; num--)
		{
			if (m_InView[num] != null)
			{
				Character character = m_InView[num];
				Vector2 vector3 = character.m_Transform.position;
				if ((vector3 - vector2).sqrMagnitude > m_fSqrCollisionSize || !GuardTowerManager.IsCharacterTrackable(character))
				{
					m_InView[num] = null;
					for (int num2 = m_DisguisedCharactersInLight.Count - 1; num2 >= 0; num2--)
					{
						if (m_DisguisedCharactersInLight[num2] != null && m_DisguisedCharactersInLight[num2].m_CharacterResponsible == character)
						{
							m_DisguisedCharactersInLight[num2] = null;
						}
					}
				}
			}
		}
		int num3 = 0;
		while (num3 < m_DisguisedCharactersInLight.Count)
		{
			ReccuringDisuisedHeatPODO reccuringDisuisedHeatPODO = m_DisguisedCharactersInLight[num3];
			if (reccuringDisuisedHeatPODO == null || reccuringDisuisedHeatPODO.m_CharacterResponsible == null || m_FollowingCharacter == reccuringDisuisedHeatPODO.m_CharacterResponsible || !GuardTowerManager.IsCharacterTrackable(reccuringDisuisedHeatPODO.m_CharacterResponsible))
			{
				m_DisguisedCharactersInLight.RemoveAt(num3);
				continue;
			}
			reccuringDisuisedHeatPODO.m_RemainingTime -= UpdateManager.deltaTime;
			if (reccuringDisuisedHeatPODO.m_RemainingTime <= 0f)
			{
				if (reccuringDisuisedHeatPODO.m_CharacterResponsible.m_NetView.isMine)
				{
					reccuringDisuisedHeatPODO.m_CharacterResponsible.m_CharacterStats.IncreaseHeat(reccuringDisuisedHeatPODO.m_ReoccuringHeat);
				}
				reccuringDisuisedHeatPODO.m_RemainingTime += reccuringDisuisedHeatPODO.m_ReoccuringTime;
			}
			if (!m_DisguisedCharactersInLight[num3].m_CharacterResponsible.m_bIsDisguised)
			{
				HandleCharacterInSpotlight(m_DisguisedCharactersInLight[num3].m_CharacterResponsible);
				m_DisguisedCharactersInLight.RemoveAt(num3);
			}
			else
			{
				num3++;
			}
		}
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return true;
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

	[Conditional("UNITY_EDITOR")]
	public static void PlayerLog(Character character, string log)
	{
		if (character != null && !character.IsPlayer())
		{
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (IsInited())
		{
			Character componentInParent = other.GetComponentInParent<Character>();
			ProcessCharacterEnteredSpotlight(componentInParent);
		}
	}

	private void ProcessCharacterEnteredSpotlight(Character character)
	{
		if (!(character != null) || !base.enabled || !base.gameObject.activeInHierarchy || character.m_CharacterRole != 0 || !GuardTowerManager.IsCharacterTrackable(character))
		{
			return;
		}
		AddCharacterToInViewList(character);
		if (m_FollowingCharacter == null)
		{
			HandleCharacterInSpotlight(character);
		}
		if (!character.m_bIsDisguised || !(m_FollowingCharacter != character))
		{
			return;
		}
		bool flag = false;
		for (int num = m_DisguisedCharactersInLight.Count - 1; num >= 0; num--)
		{
			if (m_DisguisedCharactersInLight[num].m_CharacterResponsible == character)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AIEvent eventByType = character.m_CharacterEventManager.GetEventByType(AIEvent.EventType.Character_Disguised);
			if (eventByType != null)
			{
				ReccuringDisuisedHeatPODO reccuringDisuisedHeatPODO = new ReccuringDisuisedHeatPODO();
				reccuringDisuisedHeatPODO.m_CharacterResponsible = character;
				reccuringDisuisedHeatPODO.m_ReoccuringHeat = eventByType.m_EventData.m_GuardHeatIncrease;
				reccuringDisuisedHeatPODO.m_RemainingTime = eventByType.m_EventData.m_ReoccuringHeatTime;
				reccuringDisuisedHeatPODO.m_ReoccuringTime = eventByType.m_EventData.m_ReoccuringHeatTime;
				m_DisguisedCharactersInLight.Add(reccuringDisuisedHeatPODO);
			}
		}
	}

	private void AddCharacterToInViewList(Character character)
	{
		if (m_InView.Contains(character))
		{
			return;
		}
		int num;
		for (num = m_InView.Count - 1; num >= 0; num--)
		{
			if (m_InView[num] == null)
			{
				m_InView[num] = character;
				break;
			}
		}
		if (num < 0)
		{
			m_InView.Add(character);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (m_bShouldReevaluateOnTriggerStay && IsInited())
		{
			Character componentInParent = other.GetComponentInParent<Character>();
			ProcessCharacterEnteredSpotlight(componentInParent);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Character componentInParent = other.GetComponentInParent<Character>();
		if (!(componentInParent != null) || !base.enabled || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		for (int num = m_DisguisedCharactersInLight.Count - 1; num >= 0; num--)
		{
			if (m_DisguisedCharactersInLight[num].m_CharacterResponsible == componentInParent)
			{
				m_DisguisedCharactersInLight.RemoveAt(num);
				break;
			}
		}
		for (int num2 = m_InView.Count - 1; num2 >= 0; num2--)
		{
			if (m_InView[num2] == componentInParent)
			{
				m_InView[num2] = null;
				break;
			}
		}
	}

	private void HandleCharacterInSpotlight(Character character)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_GuardTowerManager.EnterSpotlight(character, this);
		}
	}

	public void SetFollowing(Character character)
	{
		m_FollowingCharacter = character;
		m_FollowDirection = character.transform.position - m_Transform.position;
	}

	public void RemoveFromInView(Character character)
	{
		for (int num = m_InView.Count - 1; num >= 0; num--)
		{
			if (m_InView[num] == character)
			{
				m_InView[num] = null;
				break;
			}
		}
	}

	public void ClearInView()
	{
		m_InView.Clear();
	}

	public bool IsCharacterInView(Character character)
	{
		for (int num = m_InView.Count - 1; num >= 0; num--)
		{
			if (m_InView[num] != null && m_InView[num] == character)
			{
				return true;
			}
		}
		return false;
	}

	private void RequestNextWaypoint()
	{
		if (!m_bWaiting)
		{
			float num = m_CurrentWaypoint.m_fWaitTimer + Random.Range(0f, m_CurrentWaypoint.m_fWaitVariance);
			if (num > 0f)
			{
				m_bWaiting = true;
				m_fTimeToPoint = num;
				m_vFromPosition = m_CurrentWaypoint.m_vNodePos;
				return;
			}
		}
		int num2 = m_iCurrentWaypoint + 1;
		if (num2 >= m_PatrolPath.m_vPathNodes.Length)
		{
			num2 = 0;
		}
		m_vFromPosition = m_CurrentWaypoint.m_vNodePos;
		m_bWaiting = false;
		m_iCurrentWaypoint = num2;
		m_CurrentWaypoint = m_PatrolPath.m_vPathNodes[m_iCurrentWaypoint];
		float magnitude = (m_vFromPosition - m_CurrentWaypoint.m_vNodePos).magnitude;
		if (m_PatrolPath.m_vPathNodes.Length == 1 && magnitude < 0.01f)
		{
			m_fTimeToPoint = 100000f;
		}
		else
		{
			m_fTimeToPoint = Mathf.Max(magnitude / m_PatrolSpeed, 0.005f);
		}
		m_MyGuardTower.SetWayPointRPC(num2, m_MyIndexInGuardTower, m_fTimeToPoint - m_fCurrentTimePassed);
	}

	public void SetNextWaypoint(int iWaypoint, float fTimeToPoint, Vector3 spotlightStartPosition)
	{
		m_fCurrentTimePassed = 0f;
		float z = m_vFromPosition.z;
		m_Transform.position = spotlightStartPosition;
		m_vFromPosition = spotlightStartPosition;
		m_vFromPosition.z = z;
		m_bWaiting = false;
		m_iCurrentWaypoint = iWaypoint;
		m_CurrentWaypoint = m_PatrolPath.m_vPathNodes[m_iCurrentWaypoint];
		m_fTimeToPoint = fTimeToPoint;
	}

	private void SetToNearestWaypoint(float fX, float fY, float fZ)
	{
		m_bWaiting = false;
		int num = -1;
		float num2 = 0f;
		float z = m_vFromPosition.z;
		m_vFromPosition = m_Transform.position;
		m_vFromPosition.z = z;
		Vector3 vector = new Vector3(fX, fY, fZ);
		for (int num3 = m_PatrolPath.m_vPathNodes.Length - 1; num3 >= 0; num3--)
		{
			if (m_PatrolPath.m_vPathNodes[num3] != null)
			{
				float sqrMagnitude = (vector - m_PatrolPath.m_vPathNodes[num3].m_vNodePos).sqrMagnitude;
				if (num == -1 || sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					num = num3;
				}
			}
		}
		if (num == -1)
		{
			num = 0;
		}
		m_iCurrentWaypoint = num;
		m_CurrentWaypoint = m_PatrolPath.m_vPathNodes[m_iCurrentWaypoint];
		m_fTimeToPoint = 4f;
		m_fCurrentTimePassed = 0f;
	}

	private void MoveInDirection(float moveSpeed, float closeEnough)
	{
		float z = m_Transform.position.z;
		Vector2 vector = m_Transform.position;
		if (m_FollowDirection.sqrMagnitude <= closeEnough * closeEnough)
		{
			m_FollowDirection = Vector2.zero;
		}
		else
		{
			vector += m_FollowDirection.normalized * (moveSpeed * UpdateManager.deltaTime);
		}
		m_Transform.position = new Vector3(vector.x, vector.y, z);
	}

	protected void OnEnable()
	{
		bool flag = m_PatrolPath.m_vPathNodes.Length > 0;
		m_Light.enabled = flag;
		SetInitialPatrolPath();
		m_currentFollowSpeed = m_FollowSpeed;
		m_currentDirectionUpdateTime = m_DirectionUpdateTime;
	}

	protected void OnDisable()
	{
		m_SpotlightActive = false;
	}

	public void SetInitialPatrolPath()
	{
		if (IsInited() && m_PatrolPath != null && T17NetManager.IsMasterClient && m_MyGuardTower != null)
		{
			int iWaypoint = 0;
			if (!m_PatrolPath.m_bStartAtFirstWaypoint)
			{
				iWaypoint = Random.Range(0, m_PatrolPath.m_vPathNodes.Length);
			}
			m_MyGuardTower.SetInitialPatrolPathRPC(iWaypoint, m_MyIndexInGuardTower);
		}
	}

	public void SetStartingWaypoint(int iWaypoint, Vector3 spotlightStartPosition)
	{
		m_Transform.position = spotlightStartPosition;
		m_iCurrentWaypoint = iWaypoint;
		m_CurrentWaypoint = m_PatrolPath.m_vPathNodes[m_iCurrentWaypoint];
		m_vFromPosition = spotlightStartPosition;
		m_bWaiting = false;
		m_fTimeToPoint = 0f;
		m_SpotlightActive = true;
	}

	public void RequestResetSpotlight(float fX, float fY, float fZ)
	{
		m_FollowingCharacter = null;
		SetToNearestWaypoint(fX, fY, fZ);
		m_directionRefresh = 0f;
		m_currentFollowSpeed = m_FollowSpeed;
		m_currentDirectionUpdateTime = m_DirectionUpdateTime;
	}

	public void SetGuardTower(GuardTower tower, int iSpotlightIndex)
	{
		m_MyGuardTower = tower;
		m_MyIndexInGuardTower = iSpotlightIndex;
	}

	public GuardTower GetGuardTower()
	{
		return m_MyGuardTower;
	}

	public int GetGuardTowerSpotlightIndex()
	{
		return m_MyIndexInGuardTower;
	}

	public void CollectSnapshot(ref BitStreamWriter dataStream)
	{
		dataStream.Write(m_Transform.localPosition.x);
		dataStream.Write(m_Transform.localPosition.y);
		dataStream.Write((uint)m_iCurrentWaypoint, 6);
		dataStream.Write(m_directionRefresh);
		dataStream.Write(m_currentFollowSpeed);
		dataStream.Write(m_currentDirectionUpdateTime);
		dataStream.Write(m_vFromPosition.x);
		dataStream.Write(m_vFromPosition.y);
		dataStream.Write(m_vFromPosition.z);
		dataStream.Write(m_bWaiting);
		dataStream.Write(m_SpotlightActive);
		dataStream.Write(m_fTimeToPoint);
		dataStream.Write(m_fCurrentTimePassed);
		bool flag = m_FollowingCharacter != null;
		dataStream.Write(flag);
		if (flag)
		{
			dataStream.Write((uint)m_FollowingCharacter.m_NetView.viewID, 12);
		}
		int usedBitCount = dataStream.GetUsedBitCount();
		dataStream.Write(byte.MaxValue, 8);
		int count = m_DisguisedCharactersInLight.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (m_DisguisedCharactersInLight[i] != null && m_DisguisedCharactersInLight[i].m_CharacterResponsible != null)
			{
				T17NetView netView = m_DisguisedCharactersInLight[i].m_CharacterResponsible.m_NetView;
				if (netView != null)
				{
					int bits = netView.viewID & 0xFFF;
					dataStream.Write((uint)bits, 12);
					dataStream.Write(m_DisguisedCharactersInLight[i].m_ReoccuringHeat);
					dataStream.Write(m_DisguisedCharactersInLight[i].m_ReoccuringTime);
					dataStream.Write(m_DisguisedCharactersInLight[i].m_RemainingTime);
					num++;
				}
			}
		}
		dataStream.Overwrite((byte)num, 8, usedBitCount);
		usedBitCount = dataStream.GetUsedBitCount();
		dataStream.Write(byte.MaxValue, 8);
		count = m_InView.Count;
		num = 0;
		for (int j = 0; j < count; j++)
		{
			if (m_InView[j] != null)
			{
				T17NetView netView2 = m_InView[j].m_NetView;
				if (netView2 != null)
				{
					int bits2 = netView2.viewID & 0xFFF;
					dataStream.Write((uint)bits2, 12);
					num++;
				}
			}
		}
		dataStream.Overwrite((byte)num, 8, usedBitCount);
		dataStream.Write(base.enabled);
	}

	public void RestoreSnapshot(ref BitStreamReader dataStream)
	{
		Vector3 localPosition = m_Transform.localPosition;
		localPosition.x = dataStream.ReadFloat32();
		localPosition.y = dataStream.ReadFloat32();
		m_Transform.localPosition = localPosition;
		m_iCurrentWaypoint = (int)dataStream.ReadUInt32(6);
		if (m_PatrolPath == null)
		{
			m_PatrolPath = GetComponent<PatrolPath>();
		}
		m_CurrentWaypoint = null;
		if (m_iCurrentWaypoint < m_PatrolPath.m_vPathNodes.Length)
		{
			m_CurrentWaypoint = m_PatrolPath.m_vPathNodes[m_iCurrentWaypoint];
		}
		m_directionRefresh = dataStream.ReadFloat32();
		m_currentFollowSpeed = dataStream.ReadFloat32();
		m_currentDirectionUpdateTime = dataStream.ReadFloat32();
		m_vFromPosition.x = dataStream.ReadFloat32();
		m_vFromPosition.y = dataStream.ReadFloat32();
		m_vFromPosition.z = dataStream.ReadFloat32();
		m_bWaiting = dataStream.ReadBit();
		m_SpotlightActive = dataStream.ReadBit();
		m_fTimeToPoint = dataStream.ReadFloat32();
		m_fCurrentTimePassed = dataStream.ReadFloat32();
		if (dataStream.ReadBit())
		{
			int viewID = (int)dataStream.ReadUInt32(12);
			m_FollowingCharacter = T17NetView.Find<Character>(viewID);
		}
		m_DisguisedCharactersInLight.Clear();
		int num = (int)dataStream.ReadUInt32(8);
		for (int i = 0; i < num; i++)
		{
			int viewID2 = (int)dataStream.ReadUInt32(12);
			float reoccuringHeat = dataStream.ReadFloat32();
			float reoccuringTime = dataStream.ReadFloat32();
			float remainingTime = dataStream.ReadFloat32();
			Character character = T17NetView.Find<Character>(viewID2);
			if (character != null)
			{
				ReccuringDisuisedHeatPODO reccuringDisuisedHeatPODO = new ReccuringDisuisedHeatPODO();
				reccuringDisuisedHeatPODO.m_CharacterResponsible = character;
				reccuringDisuisedHeatPODO.m_ReoccuringHeat = reoccuringHeat;
				reccuringDisuisedHeatPODO.m_RemainingTime = remainingTime;
				reccuringDisuisedHeatPODO.m_ReoccuringTime = reoccuringTime;
				m_DisguisedCharactersInLight.Add(reccuringDisuisedHeatPODO);
			}
		}
		m_InView.Clear();
		num = (int)dataStream.ReadUInt32(8);
		for (int j = 0; j < num; j++)
		{
			int viewID3 = (int)dataStream.ReadUInt32(12);
			Character character2 = T17NetView.Find<Character>(viewID3);
			if (character2 != null)
			{
				m_InView.Add(character2);
			}
		}
		base.enabled = dataStream.ReadBit();
	}

	public void AllowUpdates(bool bAllow)
	{
		m_bAllowUpdatesDuringRestore = bAllow;
	}
}
