using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using BitStream;
using UnityEngine;

public class GuardTower : T17MonoBehaviour, IControlledUpdate
{
	private class TowerTarget
	{
		private Transform m_TowerGun;

		private Character m_Character;

		public float m_TimeUntilShot;

		public MeshRenderer m_Target;

		public TowerTarget(Character character, Transform towerGun)
		{
			m_Character = character;
			m_TowerGun = towerGun;
		}

		public Character GetTarget()
		{
			return m_Character;
		}

		public Transform GetGun()
		{
			return m_TowerGun;
		}

		public void Update(float deltaTime)
		{
			if (m_TimeUntilShot > 0f)
			{
				m_TimeUntilShot -= UpdateManager.deltaTime;
			}
			UpdateTargetPosition();
		}

		public bool IsReadyToShoot()
		{
			return m_TimeUntilShot <= 0f;
		}

		public void ResetShotCooldown(float timeUntilShot)
		{
			m_TimeUntilShot = timeUntilShot;
		}

		public void UpdateTargetPosition()
		{
			if (m_Target != null)
			{
				Vector3 position = m_Character.transform.position;
				position.y += 0.2f;
				position.z = m_Character.m_CharacterAnimator.m_CharacterAnimator.transform.position.z - 0.1f;
				m_Target.transform.position = position;
			}
		}
	}

	public string DEBUG_TowerLog;

	private T17NetView m_NetView;

	private GuardTowerManager m_GuardTowerManager;

	public Transform[] m_GunPositions;

	private List<TowerTarget> m_TrackedTargets = new List<TowerTarget>();

	private GuardTowerSpotlight[] m_Spotlights;

	private bool m_bCanRunTargetUpdates = true;

	private bool m_bAllowUpdatesDuringRestore = true;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		Debug.Log("    Guard Tower StartInit");
		if (m_Spotlights == null || m_Spotlights.Length == 0)
		{
			m_Spotlights = GetComponentsInChildren<GuardTowerSpotlight>();
			Debug.Log("    Guard Tower StartInit aa " + m_Spotlights.Length);
			for (int num = m_Spotlights.Length - 1; num >= 0; num--)
			{
				m_Spotlights[num].SetGuardTower(this, num);
			}
		}
		for (int num2 = m_Spotlights.Length - 1; num2 >= 0; num2--)
		{
			if (m_Spotlights[num2] != null && !m_Spotlights[num2].IsInited())
			{
				return T17BehaviourManager.INITSTATE.IS_DEPS;
			}
		}
		m_GuardTowerManager = GuardTowerManager.GetInstance();
		base.StartInit();
		for (int num3 = m_Spotlights.Length - 1; num3 >= 0; num3--)
		{
			if (m_Spotlights[num3] != null)
			{
				m_Spotlights[num3].SetInitialPatrolPath();
			}
		}
		CutsceneManagerBase.PrepareForCutsceneEvent += CutsceneManagerBase_PrepareForCutsceneEvent;
		CutsceneManagerBase.CutsceneFinishedEvent += CutsceneManagerBase_CutsceneFinishedEvent;
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.RapidPeriodic);
		}
		GlobalStart.EndLevelEvent += GlobalStart_EndLevelEvent;
		return T17BehaviourManager.INITSTATE.IS_FINISHED;
	}

	private void GlobalStart_EndLevelEvent()
	{
		m_bCanRunTargetUpdates = false;
	}

	protected virtual void OnDestroy()
	{
		CutsceneManagerBase.PrepareForCutsceneEvent -= CutsceneManagerBase_PrepareForCutsceneEvent;
		CutsceneManagerBase.CutsceneFinishedEvent -= CutsceneManagerBase_CutsceneFinishedEvent;
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.RapidPeriodic);
		}
		m_GuardTowerManager = null;
		GlobalStart.EndLevelEvent -= GlobalStart_EndLevelEvent;
	}

	private void CutsceneManagerBase_PrepareForCutsceneEvent(float timeUntilStart)
	{
		SetAllTargetRendererEnabled(state: false);
		m_bCanRunTargetUpdates = false;
	}

	private void CutsceneManagerBase_CutsceneFinishedEvent(float timeUntilCurtainRaised)
	{
		SetAllTargetRendererEnabled(state: true);
		m_bCanRunTargetUpdates = true;
	}

	private void SetAllTargetRendererEnabled(bool state)
	{
		for (int num = m_TrackedTargets.Count - 1; num >= 0; num--)
		{
			TowerTarget towerTarget = m_TrackedTargets[num];
			if (towerTarget != null && towerTarget.m_Target != null)
			{
				towerTarget.m_Target.enabled = state;
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledUpdate()
	{
		if (!(m_GuardTowerManager != null) || !m_GuardTowerManager.IsInited() || !m_bCanRunTargetUpdates || !m_bAllowUpdatesDuringRestore)
		{
			return;
		}
		int num = 0;
		while (num < m_TrackedTargets.Count)
		{
			TowerTarget towerTarget = m_TrackedTargets[num];
			Character target = towerTarget.GetTarget();
			bool flag = GuardTowerManager.IsCharacterTrackable(target);
			if (flag)
			{
				flag = CheckGunLineOfSight(towerTarget.GetGun(), target, m_GuardTowerManager.m_GuardTowerVisionMask);
			}
			if (!flag && target.m_NetView.isMine)
			{
				StopTrackingRPC(target);
				continue;
			}
			towerTarget.Update(UpdateManager.deltaTime);
			if (towerTarget.IsReadyToShoot())
			{
				ShootCharacter(target);
				towerTarget.ResetShotCooldown(m_GuardTowerManager.m_TimeBetweenShots);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Sniper_Equip, target.gameObject);
			}
			num++;
		}
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

	public T17NetView GetNetView()
	{
		return m_NetView;
	}

	public int GetGunIndexForTrackedCharacter(Character character)
	{
		for (int num = m_TrackedTargets.Count - 1; num >= 0; num--)
		{
			if (m_TrackedTargets[num].GetTarget() == character)
			{
				Transform gun = m_TrackedTargets[num].GetGun();
				for (int num2 = m_GunPositions.Length - 1; num2 >= 0; num2--)
				{
					if (m_GunPositions[num2] == gun)
					{
						return num2;
					}
				}
				return -1;
			}
		}
		return -1;
	}

	public int GetSpotlightIndex(GuardTowerSpotlight spotlight)
	{
		for (int num = m_Spotlights.Length - 1; num >= 0; num--)
		{
			if (m_Spotlights[num] == spotlight)
			{
				return num;
			}
		}
		return -1;
	}

	public GuardTowerSpotlight GetSpotlightFromIndex(int iIndex)
	{
		if (iIndex < m_Spotlights.Length && iIndex >= 0)
		{
			return m_Spotlights[iIndex];
		}
		return null;
	}

	public void StartTracking(Character character, int gunIndex, float fShotCountdown = -1f)
	{
		if (!IsCharacterTracked(character))
		{
			TowerTarget towerTarget = new TowerTarget(character, m_GunPositions[gunIndex]);
			towerTarget.m_Target = m_GuardTowerManager.GetTargetRenderer(base.gameObject);
			towerTarget.UpdateTargetPosition();
			if (fShotCountdown == -1f)
			{
				towerTarget.ResetShotCooldown(m_GuardTowerManager.m_TimeBetweenShots);
			}
			else
			{
				towerTarget.ResetShotCooldown(fShotCountdown);
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Sniper_Equip, character.gameObject);
			m_TrackedTargets.Add(towerTarget);
		}
		base.enabled = true;
	}

	public void StopTrackingRPC(Character character)
	{
		Vector3 position = character.m_Transform.position;
		m_NetView.PostLevelLoadRPC("RPC_StopTracking", NetTargets.All, character.m_NetView.viewID, position.x, position.y, position.z);
	}

	[PunRPC]
	private void RPC_StopTracking(int characterID, float fx, float fy, float fz)
	{
		if (!IsInited())
		{
			return;
		}
		Character character = null;
		PhotonView photonView = PhotonView.Find(characterID);
		if (photonView != null)
		{
			character = photonView.GetComponent<Character>();
		}
		int num = 0;
		while (num < m_TrackedTargets.Count)
		{
			if (m_TrackedTargets[num].GetTarget() == character)
			{
				m_GuardTowerManager.ReturnTargetRenderer(m_TrackedTargets[num].m_Target);
				m_TrackedTargets[num].m_Target = null;
				m_TrackedTargets.RemoveAt(num);
				m_GuardTowerManager.FreeSpotlights(character, fx, fy, fz);
			}
			else
			{
				num++;
			}
		}
		if (m_TrackedTargets.Count == 0)
		{
			base.enabled = false;
		}
	}

	public void SetInitialPatrolPathRPC(int iWaypoint, int iSpotlightIndex)
	{
		if (iSpotlightIndex >= 0 && iSpotlightIndex < m_Spotlights.Length && !(m_Spotlights[iSpotlightIndex] == null))
		{
			Vector3 position = m_Spotlights[iSpotlightIndex].transform.position;
			m_NetView.GameplayRPC("RPC_SetInitialPatrolPath", NetTargets.All, iWaypoint, iSpotlightIndex, position.x, position.y, position.z);
		}
	}

	[PunRPC]
	private void RPC_SetInitialPatrolPath(int iWaypoint, int iSpotlightIndex, float x, float y, float z)
	{
		if (IsInited())
		{
			Vector3 spotlightStartPosition = new Vector3(x, y, z);
			if (iSpotlightIndex >= 0 && iSpotlightIndex < m_Spotlights.Length && !(m_Spotlights[iSpotlightIndex] == null))
			{
				m_Spotlights[iSpotlightIndex].SetStartingWaypoint(iWaypoint, spotlightStartPosition);
			}
		}
	}

	public void SetWayPointRPC(int iWaypoint, int iSpotlightIndex, float fTime)
	{
		Vector3 position = m_Spotlights[iSpotlightIndex].transform.position;
		m_NetView.PostLevelLoadRPC("RPC_SetWayPoint", NetTargets.Others, iWaypoint, iSpotlightIndex, fTime, position.x, position.y, position.z);
	}

	[PunRPC]
	private void RPC_SetWayPoint(int iWaypoint, int iSpotlightIndex, float fTime, float spotlightStartX, float spotlightStartY, float spotlightStartZ)
	{
		if (IsInited() && iSpotlightIndex >= 0 && iSpotlightIndex < m_Spotlights.Length && !(m_Spotlights[iSpotlightIndex] == null))
		{
			Vector3 spotlightStartPosition = new Vector3(spotlightStartX, spotlightStartY, spotlightStartZ);
			m_Spotlights[iSpotlightIndex].SetNextWaypoint(iWaypoint, fTime, spotlightStartPosition);
		}
	}

	private void ShootCharacter(Character character)
	{
		if (character.m_NetView.isMine)
		{
			character.DamageSelf(character, m_GuardTowerManager.m_DamagePerShot);
			if (character.IsPlayer())
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Shots fired by sniper at player", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " shot fired at player", Gamer.GetGamerCount() + " Player", 0L);
			}
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Sniper_Fire, character.gameObject);
	}

	public int GetGunLineOfSight(Character character, LayerMask visionMask)
	{
		for (int i = 0; i < m_GunPositions.Length; i++)
		{
			if (CheckGunLineOfSight(m_GunPositions[i], character, visionMask))
			{
				return i;
			}
		}
		return -1;
	}

	private bool CheckGunLineOfSight(Transform gunPosition, Character character, LayerMask visionMask)
	{
		return true;
	}

	public bool IsCharacterTracked(Character character)
	{
		for (int i = 0; i < m_TrackedTargets.Count; i++)
		{
			if (m_TrackedTargets[i].GetTarget() == character)
			{
				return true;
			}
		}
		return false;
	}

	public void SetSpotlightsActive(bool active)
	{
		if (!IsInited())
		{
			return;
		}
		if (!active)
		{
			for (int i = 0; i < m_TrackedTargets.Count; i++)
			{
				if (m_TrackedTargets[i] != null)
				{
					Vector3 position = m_TrackedTargets[i].GetTarget().m_Transform.position;
					m_GuardTowerManager.FreeSpotlights(m_TrackedTargets[i].GetTarget(), position.x, position.y, position.z);
				}
			}
		}
		for (int j = 0; j < m_Spotlights.Length; j++)
		{
			if (m_Spotlights[j] != null)
			{
				m_Spotlights[j].gameObject.SetActive(active);
				m_Spotlights[j].ClearInView();
			}
		}
	}

	public bool IsCharacterInView(Character character)
	{
		for (int num = m_Spotlights.Length - 1; num >= 0; num--)
		{
			if (m_Spotlights[num] != null && m_Spotlights[num].IsCharacterInView(character))
			{
				return true;
			}
		}
		return false;
	}

	public void FreeSpotlights(Character character, float fX, float fY, float fZ)
	{
		for (int num = m_Spotlights.Length - 1; num >= 0; num--)
		{
			if (m_Spotlights[num] != null)
			{
				if (m_Spotlights[num].FollowingCharacter == character)
				{
					m_Spotlights[num].RequestResetSpotlight(fX, fY, fZ);
				}
				m_Spotlights[num].RemoveFromInView(character);
			}
		}
	}

	private void DEBUG_LogTowerHistory(string message)
	{
		string text = ((!T17NetManager.IsMasterClient) ? string.Empty : "(M) ");
		DEBUG_TowerLog = DEBUG_TowerLog + text + message + "\n";
		GuardTowerManager.DEBUG_AddTowerLog(this, message);
	}

	private static string LogTowerTarget(List<TowerTarget> towerTargets)
	{
		string text = "  Total Count " + towerTargets.Count + "\n";
		foreach (TowerTarget towerTarget in towerTargets)
		{
			if (towerTarget == null || towerTarget.GetTarget() == null)
			{
				text += "    Looped object is null / character is null \n";
				continue;
			}
			string text2 = text;
			text = text2 + "    Is targetting character " + towerTarget.GetTarget().m_NetView.viewID + " with reticule ";
			text = ((!(towerTarget.m_Target == null)) ? (text + towerTarget.m_Target.transform.name + "\n") : (text + "that is null \n"));
		}
		return text;
	}

	public void CollectSnapshot(ref BitStreamWriter dataStream)
	{
		int usedBitCount = dataStream.GetUsedBitCount();
		dataStream.Write(byte.MaxValue, 8);
		byte b = 0;
		int num = m_Spotlights.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_Spotlights[i] != null)
			{
				dataStream.Write((uint)i, 8);
				m_Spotlights[i].CollectSnapshot(ref dataStream);
				b++;
			}
		}
		dataStream.Overwrite(b, 8, usedBitCount);
		dataStream.Write(base.enabled);
	}

	public void RestoreSnapshot(ref BitStreamReader dataStream)
	{
		int num = (int)dataStream.ReadUInt32(8);
		Debug.Log("   ***  Guard Tower RestoreSnapshot iTotal " + num);
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)dataStream.ReadUInt32(8);
			Debug.Log("   ***  SpotLights RestoreSnapshot index " + num2 + "     have " + m_Spotlights.Length);
			if (m_Spotlights != null && num2 < m_Spotlights.Length)
			{
				m_Spotlights[num2].RestoreSnapshot(ref dataStream);
				continue;
			}
			Debug.Log("  *****   GuardTower  RestoreSnapshot   error  " + num2 + "    " + m_Spotlights.Length);
		}
		base.enabled = dataStream.ReadBit();
	}

	public float GetTimeBeforeShot(Character character)
	{
		for (int num = m_TrackedTargets.Count - 1; num >= 0; num--)
		{
			if (m_TrackedTargets[num] != null && m_TrackedTargets[num].GetTarget() == character)
			{
				return m_TrackedTargets[num].m_TimeUntilShot;
			}
		}
		return -1f;
	}

	public void AllowUpdates(bool bAllow)
	{
		m_bAllowUpdatesDuringRestore = bAllow;
		for (int num = m_Spotlights.Length - 1; num >= 0; num--)
		{
			if (m_Spotlights != null)
			{
				m_Spotlights[num].AllowUpdates(bAllow);
			}
		}
	}
}
