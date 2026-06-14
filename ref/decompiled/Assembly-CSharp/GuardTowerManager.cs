using System;
using System.Collections;
using System.Collections.Generic;
using BitStream;
using NetworkLoadable;
using UnityEngine;

public class GuardTowerManager : T17MonoBehaviour, IControlledUpdate, Saveable, INetworkLoadable
{
	private class TrackedCharacter
	{
		private Character m_Character;

		private GuardTower m_GuardTower;

		public TrackedCharacter(Character character)
		{
			m_Character = character;
		}

		public Character GetTrackedCharacter()
		{
			return m_Character;
		}

		public GuardTower GetTrackingTower()
		{
			return m_GuardTower;
		}

		public void SetTrackingTower(GuardTower tower, int gunIndex, float fShotCountdown = -1f)
		{
			if (m_GuardTower != null)
			{
				m_GuardTower.StopTrackingRPC(m_Character);
			}
			tower.StartTracking(m_Character, gunIndex, fShotCountdown);
			m_GuardTower = tower;
		}
	}

	private struct TrackingPoint
	{
		public int m_guardTowerIndex;

		public int m_gunIndex;
	}

	[Serializable]
	public class SaveData_GuardTowerManager_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		[Flags]
		public enum Bools : byte
		{
			NOTHING = 0,
			SearchedGuardTowers = 1,
			SpotlightsActiveTimed = 2,
			SpotlightsActiveGenerator = 4
		}

		public enum DataType : byte
		{
			TrackedCharacters = 0,
			VariousFlags = 1,
			Towers = 2,
			END = 63
		}

		public const int HEADERSIZE = 6;

		public const int LENGTHSIZE = 8;

		public const int NETVIEWSIZE = 12;

		public byte[] m_Data = new byte[0];

		public SaveData_GuardTowerManager_V1()
		{
			m_Version = 1;
		}
	}

	private const string TARGET_PREFAB_NAME = "Prefabs/SniperTarget";

	private string DEBUG_ManagerLog = string.Empty;

	private static string DEBUG_InterleavedLog = string.Empty;

	private static GuardTowerManager m_Instance;

	private T17NetView m_NetView;

	private AIConfig m_AiConfig;

	public Material m_LaserMaterial;

	public float m_LaserWidth = 0.1f;

	public float m_TimeBetweenShots = 3f;

	public float m_DamagePerShot = 40f;

	public float m_ShootingHeatTolerance = 70f;

	public LayerMask m_GuardTowerVisionMask;

	public List<AIEvent.EventType> m_ReportableEvents = new List<AIEvent.EventType>();

	private bool m_SearchedGuardTowers;

	private GuardTower[] m_GuardTowers;

	private List<TrackedCharacter> m_TrackedCharacters = new List<TrackedCharacter>();

	private List<MeshRenderer> m_AvailableTargetRenderers = new List<MeshRenderer>();

	private bool m_bAllowUpdatesDuringRestore = true;

	private bool m_bSpotlightsActiveTimed;

	private bool m_bSpotlightsActiveGenerator;

	private SaveDataRegister m_SaveData;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public static GuardTowerManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance == null)
		{
			m_Instance = this;
		}
		m_NetView = GetComponent<T17NetView>();
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		Debug.Log(" ********   Guard Tower Manager StartInit    ***");
		if (m_GuardTowers == null || m_GuardTowers.Length == 0)
		{
			m_GuardTowers = UnityEngine.Object.FindObjectsOfType<GuardTower>();
		}
		Debug.Log(" ********   Guard Tower Manager StartInit    ***  " + m_GuardTowers.Length);
		for (int num = m_GuardTowers.Length - 1; num >= 0; num--)
		{
			if (m_GuardTowers[num] != null && !m_GuardTowers[num].IsInited())
			{
				return T17BehaviourManager.INITSTATE.IS_DEPS;
			}
		}
		Debug.Log(" ********   Guard Tower Manager StartInit  actually   inited  ***  ");
		m_SearchedGuardTowers = false;
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.RegularPeriodic);
		}
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 16);
		InitFromSnapshot();
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.RegularPeriodic);
		}
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		m_NetView = null;
	}

	public void ControlledUpdate()
	{
		if (!m_bAllowUpdatesDuringRestore)
		{
			return;
		}
		int num = 0;
		while (num < m_TrackedCharacters.Count)
		{
			TrackedCharacter trackedCharacter = m_TrackedCharacters[num];
			Character trackedCharacter2 = trackedCharacter.GetTrackedCharacter();
			GuardTower trackingTower = trackedCharacter.GetTrackingTower();
			if (!trackingTower.IsCharacterTracked(trackedCharacter2))
			{
				m_TrackedCharacters.Remove(trackedCharacter);
				if (m_TrackedCharacters.Count <= 0)
				{
					base.enabled = true;
				}
			}
			else
			{
				num++;
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void AlertGuardTowerRPC(Character character, AIEvent.EventType eventType = AIEvent.EventType.Event_Count, bool immediateLockdown = false, GuardTowerSpotlight spotlight = null)
	{
		if (character != null)
		{
			if (character.m_NetView.isMine)
			{
				ValidateThenStartTrackingCharacter(character, eventType, immediateLockdown, spotlight);
			}
			else
			{
				StartCoroutine(WaitForRemoteCharacterUpdateThenStartTracking(UpdateManager.frameCount, character, eventType, immediateLockdown, spotlight));
			}
		}
	}

	private IEnumerator WaitForRemoteCharacterUpdateThenStartTracking(int requestFrameCount, Character character, AIEvent.EventType eventType = AIEvent.EventType.Event_Count, bool immediateLockdown = false, GuardTowerSpotlight spotlight = null)
	{
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		while (character != null && !character.m_NetView.isMine && character.GetNetworkLastReadFrameCount() <= requestFrameCount && character.GetLastLocationUpdateFrameCount() <= requestFrameCount)
		{
			yield return waitForEndOfFrame;
		}
		if (character != null)
		{
			ValidateThenStartTrackingCharacter(character, eventType, immediateLockdown, spotlight);
		}
	}

	private void ValidateThenStartTrackingCharacter(Character character, AIEvent.EventType eventType, bool immediateLockdown, GuardTowerSpotlight spotlight = null)
	{
		if (IsCharacterTrackable(character) && (!m_bSpotlightsActiveTimed || IsCharacterInView(character)) && ((eventType != AIEvent.EventType.Character_Wanted && eventType != AIEvent.EventType.Event_Count) || !(character.GetToRoutineTimer > 0f) || !(character.m_CharacterStats.Heat < m_ShootingHeatTolerance)))
		{
			StartTrackingRPC(character, eventType, immediateLockdown, spotlight);
		}
	}

	public void StartTrackingRPC(Character character, AIEvent.EventType eventType = AIEvent.EventType.Event_Count, bool immediateLockdown = false, GuardTowerSpotlight spotlight = null)
	{
		int num = 0;
		int num2 = -1;
		if (spotlight != null && spotlight.GetGuardTower() != null)
		{
			num2 = spotlight.GetGuardTowerSpotlightIndex();
			if (spotlight.GetGuardTower().GetNetView() != null)
			{
				num = spotlight.GetGuardTower().GetNetView().viewID;
			}
		}
		m_NetView.PostLevelLoadRPC("RPC_StartTracking", NetTargets.All, character.m_NetView.viewID, eventType, immediateLockdown, num, num2);
	}

	[PunRPC]
	public void RPC_StartTracking(int characterID, AIEvent.EventType eventType, bool immediateLockdown = false, int iTowerViewID = 0, int iSpotIndex = -1)
	{
		Character character = null;
		character = T17NetView.Find<Character>(characterID);
		if (!m_SearchedGuardTowers)
		{
			if (m_GuardTowers == null || m_GuardTowers.Length == 0)
			{
				m_GuardTowers = UnityEngine.Object.FindObjectsOfType<GuardTower>();
			}
			m_SearchedGuardTowers = true;
		}
		if (m_GuardTowers == null || m_GuardTowers.Length == 0 || (!T17NetManager.IsMasterClient && m_LoadState != LOADSTATE.Finished_Error && m_LoadState != LOADSTATE.Finished_OK))
		{
			return;
		}
		bool flag = false;
		GuardTower tower = null;
		if (iTowerViewID != 0)
		{
			tower = T17NetView.Find<GuardTower>(iTowerViewID);
		}
		if (FindNearestGuardTowerWithLoS(character, out var trackingPoint, characterID, tower))
		{
			if (character.m_NetView.isMine)
			{
				SpotMisbehaviourAndStartTracking(eventType, immediateLockdown, character, trackingPoint, iSpotIndex);
			}
			else
			{
				StartCoroutine(WaitForRemoteCharacterUpdateThenSpot(UpdateManager.frameCount, eventType, immediateLockdown, character, trackingPoint, iSpotIndex));
			}
		}
	}

	private IEnumerator WaitForRemoteCharacterUpdateThenSpot(int requestFrameCount, AIEvent.EventType eventType, bool immediateLockdown, Character character, TrackingPoint trackingPoint, int iSpotIndex = -1)
	{
		WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		while (character != null && !character.m_NetView.isMine && (character.GetNetworkLastReadFrameCount() <= requestFrameCount || character.GetLastLocationUpdateFrameCount() <= requestFrameCount))
		{
			yield return waitForEndOfFrame;
		}
		if (character != null)
		{
			if (iSpotIndex == -1)
			{
				bool flag = FindNearestGuardTowerWithLoS(character, out trackingPoint, character.m_NetView.viewID);
				DEBUG_LogManager("Re-found tower due to waiting for update, tower found: " + flag + " Tower found: " + m_GuardTowers[trackingPoint.m_guardTowerIndex].gameObject.name);
			}
			SpotMisbehaviourAndStartTracking(eventType, immediateLockdown, character, trackingPoint, iSpotIndex);
		}
	}

	private void SpotMisbehaviourAndStartTracking(AIEvent.EventType eventType, bool immediateLockdown, Character character, TrackingPoint trackingPoint, int iSpotIndex = -1)
	{
		SpotMisbehaviour(character, eventType, immediateLockdown);
		GuardTower guardTower = m_GuardTowers[trackingPoint.m_guardTowerIndex];
		if (!guardTower.IsInited())
		{
			return;
		}
		ConfigManager instance = ConfigManager.GetInstance();
		if ((instance != null && instance.gameType == PrisonConfig.ConfigType.Versus) || !(character.m_CharacterStats.Heat >= m_ShootingHeatTolerance))
		{
			return;
		}
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			Player component = character.GetComponent<Player>();
			TutorialManager instance2 = TutorialManager.GetInstance();
			if (component != null && instance2 != null)
			{
				instance2.StartTutorialRPC(component, TutorialSubject.GuardTowers);
			}
		}
		if (!IsCharacterTracked(character))
		{
			TrackedCharacter trackedCharacter = new TrackedCharacter(character);
			trackedCharacter.SetTrackingTower(guardTower, trackingPoint.m_gunIndex);
			m_TrackedCharacters.Add(trackedCharacter);
		}
		if (iSpotIndex != -1)
		{
			GuardTowerSpotlight spotlightFromIndex = guardTower.GetSpotlightFromIndex(iSpotIndex);
			if (spotlightFromIndex != null && spotlightFromIndex.FollowingCharacter == null)
			{
				spotlightFromIndex.SetFollowing(character);
			}
		}
		base.enabled = true;
	}

	public bool IsCharacterInView(Character character)
	{
		for (int num = m_GuardTowers.Length - 1; num >= 0; num--)
		{
			if (m_GuardTowers[num] != null && m_GuardTowers[num].IsCharacterInView(character))
			{
				return true;
			}
		}
		return false;
	}

	public void SetSpotlightsActive_Timed(bool active)
	{
		if (m_bSpotlightsActiveTimed != active)
		{
			m_bSpotlightsActiveTimed = active;
			UpdateSpotlightsActive();
		}
	}

	public void SetSpotlightsActive_Generator(bool active)
	{
		if (m_bSpotlightsActiveGenerator != active)
		{
			m_bSpotlightsActiveGenerator = active;
			UpdateSpotlightsActive();
		}
	}

	private void UpdateSpotlightsActive()
	{
		if (m_GuardTowers == null || m_GuardTowers.Length == 0)
		{
			return;
		}
		bool spotlightsActive = m_bSpotlightsActiveTimed && m_bSpotlightsActiveGenerator;
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus)
		{
			spotlightsActive = false;
		}
		for (int i = 0; i < m_GuardTowers.Length; i++)
		{
			if (m_GuardTowers[i] != null)
			{
				m_GuardTowers[i].SetSpotlightsActive(spotlightsActive);
			}
		}
	}

	private void SpotMisbehaviour(Character character, AIEvent.EventType eventType, bool immediateLockdown = false)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if ((instance != null && instance.gameType == PrisonConfig.ConfigType.Versus) || !(character != null) || character.m_CharacterRole != 0)
		{
			return;
		}
		List<AIEvent> visibleEvents = character.m_CharacterEventManager.GetVisibleEvents();
		RoutineManager instance2 = RoutineManager.GetInstance();
		if (!(instance2 != null))
		{
			return;
		}
		Routines currentRoutineBaseType = instance2.GetCurrentRoutineBaseType();
		if (!immediateLockdown)
		{
			if (visibleEvents == null)
			{
				return;
			}
			for (int i = 0; i < visibleEvents.Count; i++)
			{
				bool flag = false;
				AIEvent aIEvent = visibleEvents[i];
				if (!((eventType != AIEvent.EventType.Event_Count) ? (aIEvent.m_EventData.m_eEventType == eventType) : m_ReportableEvents.Contains(aIEvent.m_EventData.m_eEventType)))
				{
					continue;
				}
				if (character.m_bIsDisguised)
				{
					if (m_AiConfig == null)
					{
						m_AiConfig = ConfigManager.GetInstance().aiConfig;
					}
					if (m_AiConfig.DisguiseableEvents.Contains(aIEvent.m_EventData.m_eEventType))
					{
						continue;
					}
				}
				int prisonAlertnessIncrease = (int)aIEvent.m_EventData.m_PrisonAlertnessIncrease;
				float guardHeatIncrease = aIEvent.m_EventData.m_GuardHeatIncrease;
				PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(prisonAlertnessIncrease, character, aIEvent.m_EventData.m_eEventType);
				bool flag2 = currentRoutineBaseType == Routines.LightsOut || currentRoutineBaseType == Routines.Lockdown;
				if ((guardHeatIncrease > 0f || prisonAlertnessIncrease > 0) && flag2)
				{
					character.m_CharacterStats.IncreaseHeat(100f);
					PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(11, character, aIEvent.m_EventData.m_eEventType);
				}
				else
				{
					character.m_CharacterStats.IncreaseHeat(guardHeatIncrease);
				}
			}
		}
		else
		{
			character.m_CharacterStats.IncreaseHeat(100f);
			PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(11, character, eventType);
		}
	}

	private bool FindNearestGuardTowerWithLoS(Character character, out TrackingPoint trackingPoint, int characterID, GuardTower tower = null)
	{
		float num = 0f;
		trackingPoint.m_guardTowerIndex = -1;
		trackingPoint.m_gunIndex = 0;
		if (character == null)
		{
			if (GlobalStart.GetInstance() != null)
			{
				T17NetManager.LogGoogleException("Attempting to get nearest guard tower to a null character  " + characterID + "   " + GlobalStart.GetInstance().GetModeAsString());
			}
			else
			{
				T17NetManager.LogGoogleException("Attempting to get nearest guard tower to a null character  " + characterID);
			}
			return false;
		}
		Vector3 position = character.transform.position;
		for (int i = 0; i < m_GuardTowers.Length; i++)
		{
			if (m_GuardTowers[i] == null)
			{
				PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
				string text = "GuardTower is null at index " + i;
				if (currentLevelInfo != null)
				{
					text = text + " in prison " + currentLevelInfo.m_PrisonEnum;
				}
				T17NetManager.LogGoogleException(text);
				continue;
			}
			if (tower != null)
			{
				if (m_GuardTowers[i] == tower)
				{
					int gunLineOfSight = m_GuardTowers[i].GetGunLineOfSight(character, m_GuardTowerVisionMask);
					if (gunLineOfSight >= 0)
					{
						trackingPoint.m_guardTowerIndex = i;
						trackingPoint.m_gunIndex = gunLineOfSight;
						return true;
					}
				}
				continue;
			}
			Vector3 position2 = m_GuardTowers[i].transform.position;
			position2.z = position.z;
			float sqrMagnitude = (position2 - position).sqrMagnitude;
			if (trackingPoint.m_guardTowerIndex == -1 || sqrMagnitude < num)
			{
				int gunLineOfSight2 = m_GuardTowers[i].GetGunLineOfSight(character, m_GuardTowerVisionMask);
				if (gunLineOfSight2 >= 0)
				{
					num = sqrMagnitude;
					trackingPoint.m_guardTowerIndex = i;
					trackingPoint.m_gunIndex = gunLineOfSight2;
				}
			}
		}
		return trackingPoint.m_guardTowerIndex != -1;
	}

	public bool IsCharacterTracked(Character character)
	{
		for (int i = 0; i < m_TrackedCharacters.Count; i++)
		{
			if (m_TrackedCharacters[i].GetTrackedCharacter() == character)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsCharacterTrackable(Character character)
	{
		if (character.GetIsKnockedOut())
		{
			return false;
		}
		if (character.m_CurrentLocation == null)
		{
			return false;
		}
		if (character.m_CurrentLocation.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors || !character.m_CurrentLocation.m_AllowSniping)
		{
			return false;
		}
		if (character.CurrentFloor.IsUnderGround() || character.CurrentFloor.IsVent())
		{
			return false;
		}
		return true;
	}

	public bool EnterSpotlight(Character character, GuardTowerSpotlight spotlight)
	{
		bool immediateLockdown = false;
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			Routines currentRoutineBaseType = instance.GetCurrentRoutineBaseType();
			if ((currentRoutineBaseType == Routines.LightsOut || currentRoutineBaseType == Routines.Lockdown) && !character.m_bIsDisguised)
			{
				immediateLockdown = true;
			}
		}
		AlertGuardTowerRPC(character, AIEvent.EventType.Event_Count, immediateLockdown, spotlight);
		return true;
	}

	public void FreeSpotlights(Character character, float fX, float fY, float fZ)
	{
		for (int num = m_GuardTowers.Length - 1; num >= 0; num--)
		{
			if (m_GuardTowers[num] != null)
			{
				m_GuardTowers[num].FreeSpotlights(character, fX, fY, fZ);
			}
		}
	}

	public MeshRenderer GetTargetRenderer(GameObject requestingObject)
	{
		MeshRenderer meshRenderer = null;
		if (m_AvailableTargetRenderers.Count > 0)
		{
			meshRenderer = m_AvailableTargetRenderers[0];
			m_AvailableTargetRenderers.Remove(meshRenderer);
		}
		else
		{
			GameObject gameObject = Resources.Load("Prefabs/SniperTarget") as GameObject;
			if (gameObject != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, Vector3.zero, Quaternion.identity);
				if (gameObject2 != null)
				{
					meshRenderer = gameObject2.GetComponent<MeshRenderer>();
				}
				gameObject2.name += UnityEngine.Random.Range(0, 1000);
			}
		}
		if (meshRenderer != null)
		{
			meshRenderer.transform.position = requestingObject.transform.position;
			meshRenderer.transform.parent = requestingObject.transform;
			meshRenderer.gameObject.SetActive(value: true);
		}
		return meshRenderer;
	}

	public void ReturnTargetRenderer(MeshRenderer targetRenderer)
	{
		if (targetRenderer != null)
		{
			m_AvailableTargetRenderers.Add(targetRenderer);
			targetRenderer.transform.SetParent(base.transform);
			targetRenderer.gameObject.SetActive(value: false);
		}
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

	public static void DEBUG_PrintGuardTowerInformation()
	{
		string message = "DEBUG_PrintGuardTowerInformation\n\n\n\n";
		Debug.Log(message);
		message = "Interleaved History:\n\n " + DEBUG_InterleavedLog + "\n\n";
		Debug.Log(message);
		message = "Guard Tower Manager Log: \n\n" + m_Instance.DEBUG_ManagerLog + "\n\n";
		Debug.Log(message);
		GuardTower[] guardTowers = m_Instance.m_GuardTowers;
		foreach (GuardTower guardTower in guardTowers)
		{
			message = "Logs for " + guardTower.transform.name + "\n\n";
			message += (guardTower.DEBUG_TowerLog += "\n\n");
			Debug.Log(message);
		}
	}

	public static void DEBUG_AddTowerLog(GuardTower sender, string log)
	{
		string dEBUG_InterleavedLog = DEBUG_InterleavedLog;
		DEBUG_InterleavedLog = dEBUG_InterleavedLog + sender.transform.name + ": " + log + "\n";
	}

	protected void DEBUG_LogManager(string log)
	{
		string text = ((!T17NetManager.IsMasterClient) ? string.Empty : "(M) ");
		DEBUG_ManagerLog = DEBUG_ManagerLog + text + log + "\n";
		string dEBUG_InterleavedLog = DEBUG_InterleavedLog;
		DEBUG_InterleavedLog = dEBUG_InterleavedLog + "Manager: " + text + log + "\n";
	}

	private static string LogTrackedCharacters(List<TrackedCharacter> characters)
	{
		string text = "  Total Count " + characters.Count + "\n";
		foreach (TrackedCharacter character in characters)
		{
			if (character == null || character.GetTrackedCharacter() == null)
			{
				text += "    Looped object is null / character is null \n";
				continue;
			}
			string text2 = text;
			text = text2 + "    Tower " + character.GetTrackingTower().transform.name + " is targetting character " + character.GetTrackedCharacter().m_NetView.viewID + "\n";
		}
		return text;
	}

	public string CreateSnapshot()
	{
		SaveData_GuardTowerManager_V1 saveData_GuardTowerManager_V = new SaveData_GuardTowerManager_V1();
		FastList<byte> data = new FastList<byte>();
		CollectData(ref data);
		saveData_GuardTowerManager_V.m_Data = data.ToArray();
		return JsonUtility.ToJson(saveData_GuardTowerManager_V);
	}

	public void CollectData(ref FastList<byte> data)
	{
		BitStreamWriter dataStream = new BitStreamWriter(data);
		dataStream.Write(0, 6);
		int usedBitCount = dataStream.GetUsedBitCount();
		dataStream.Write(byte.MaxValue, 8);
		int num = 0;
		int count = m_TrackedCharacters.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_TrackedCharacters[i] == null)
			{
				continue;
			}
			GuardTower trackingTower = m_TrackedCharacters[i].GetTrackingTower();
			Character trackedCharacter = m_TrackedCharacters[i].GetTrackedCharacter();
			if (!(trackingTower != null) || !(trackedCharacter != null))
			{
				continue;
			}
			int bits = 0;
			int bits2 = 0;
			int gunIndexForTrackedCharacter = trackingTower.GetGunIndexForTrackedCharacter(trackedCharacter);
			if (gunIndexForTrackedCharacter != -1 && gunIndexForTrackedCharacter <= 15)
			{
				T17NetView netView = trackingTower.GetNetView();
				if (netView != null)
				{
					bits = netView.viewID & 0xFFF;
				}
				netView = trackedCharacter.m_NetView;
				if (netView != null)
				{
					bits2 = netView.viewID & 0xFFF;
				}
				dataStream.Write((uint)bits2, 12);
				dataStream.Write((uint)bits, 12);
				dataStream.Write((uint)gunIndexForTrackedCharacter, 4);
				dataStream.Write(trackingTower.GetTimeBeforeShot(trackedCharacter));
				num++;
			}
		}
		dataStream.Overwrite((byte)num, 8, usedBitCount);
		dataStream.Write(1, 6);
		SaveData_GuardTowerManager_V1.Bools bools = SaveData_GuardTowerManager_V1.Bools.NOTHING;
		if (m_SearchedGuardTowers)
		{
			bools |= SaveData_GuardTowerManager_V1.Bools.SearchedGuardTowers;
		}
		if (m_bSpotlightsActiveTimed)
		{
			bools |= SaveData_GuardTowerManager_V1.Bools.SpotlightsActiveTimed;
		}
		if (m_bSpotlightsActiveGenerator)
		{
			bools |= SaveData_GuardTowerManager_V1.Bools.SpotlightsActiveGenerator;
		}
		dataStream.Write((byte)bools, 8);
		dataStream.Write(2, 6);
		usedBitCount = dataStream.GetUsedBitCount();
		dataStream.Write(byte.MaxValue, 8);
		num = 0;
		int num2 = m_GuardTowers.Length;
		int bits3 = 0;
		for (int j = 0; j < num2; j++)
		{
			if (m_GuardTowers[j] != null)
			{
				dataStream.Write((uint)j, 8);
				T17NetView netView2 = m_GuardTowers[j].GetNetView();
				if (netView2 != null)
				{
					bits3 = netView2.viewID & 0xFFF;
				}
				dataStream.Write((uint)bits3, 12);
				m_GuardTowers[j].CollectSnapshot(ref dataStream);
				num++;
			}
		}
		dataStream.Overwrite((byte)num, 8, usedBitCount);
		dataStream.Write(63, 6);
	}

	private void InitFromSnapshot()
	{
		SaveData_GuardTowerManager_V1 snapshotData = GetSnapshotData();
		if (snapshotData != null)
		{
			AllowUpdates(bAllowUpdates: false);
			RestoreData(ref snapshotData.m_Data, out var _);
		}
	}

	public virtual void StartedFromSnapshot()
	{
		AllowUpdates(bAllowUpdates: true);
		UpdateSpotlightsActive();
	}

	public virtual void AllowUpdates(bool bAllowUpdates)
	{
		m_bAllowUpdatesDuringRestore = bAllowUpdates;
		for (int num = m_GuardTowers.Length - 1; num >= 0; num--)
		{
			if (m_GuardTowers[num] != null)
			{
				m_GuardTowers[num].AllowUpdates(bAllowUpdates);
			}
		}
	}

	public bool RestoreData(ref byte[] data, out string strError)
	{
		Debug.Log(" ***  GuardTowerManager   RestoreData");
		BitStreamReader dataStream = new BitStreamReader(data);
		bool flag = false;
		while (!dataStream.EndOfStream && !flag)
		{
			SaveData_GuardTowerManager_V1.DataType dataType = (SaveData_GuardTowerManager_V1.DataType)dataStream.ReadByte(6);
			switch (dataType)
			{
			case SaveData_GuardTowerManager_V1.DataType.TrackedCharacters:
			{
				int num3 = (int)dataStream.ReadUInt32(8);
				m_TrackedCharacters.Clear();
				for (int j = 0; j < num3; j++)
				{
					int viewID2 = (int)dataStream.ReadUInt32(12);
					int viewID3 = (int)dataStream.ReadUInt32(12);
					int gunIndex = (int)dataStream.ReadUInt32(4);
					float fShotCountdown = (int)dataStream.ReadFloat32();
					Character character = T17NetView.Find<Character>(viewID2);
					GuardTower guardTower2 = T17NetView.Find<GuardTower>(viewID3);
					if (character != null && guardTower2 != null)
					{
						TrackedCharacter trackedCharacter = new TrackedCharacter(character);
						trackedCharacter.SetTrackingTower(guardTower2, gunIndex, fShotCountdown);
						m_TrackedCharacters.Add(trackedCharacter);
					}
				}
				if (m_TrackedCharacters.Count > 0)
				{
					base.enabled = true;
				}
				break;
			}
			case SaveData_GuardTowerManager_V1.DataType.VariousFlags:
			{
				SaveData_GuardTowerManager_V1.Bools bools = (SaveData_GuardTowerManager_V1.Bools)dataStream.ReadByte(8);
				m_SearchedGuardTowers = (bools & SaveData_GuardTowerManager_V1.Bools.SearchedGuardTowers) != 0;
				m_bSpotlightsActiveTimed = (bools & SaveData_GuardTowerManager_V1.Bools.SpotlightsActiveTimed) != 0;
				m_bSpotlightsActiveGenerator = (bools & SaveData_GuardTowerManager_V1.Bools.SpotlightsActiveGenerator) != 0;
				break;
			}
			case SaveData_GuardTowerManager_V1.DataType.Towers:
			{
				int num = (int)dataStream.ReadUInt32(8);
				for (int i = 0; i < num; i++)
				{
					int num2 = (int)dataStream.ReadUInt32(8);
					int viewID = (int)dataStream.ReadUInt32(12);
					GuardTower guardTower = T17NetView.Find<GuardTower>(viewID);
					if (guardTower != null)
					{
						guardTower.RestoreSnapshot(ref dataStream);
						continue;
					}
					if (m_GuardTowers != null && num2 < m_GuardTowers.Length)
					{
						m_GuardTowers[num2].RestoreSnapshot(ref dataStream);
						continue;
					}
					Debug.Log(" ***  Guard Tower index  error " + num2 + "   " + m_GuardTowers.Length);
				}
				break;
			}
			case SaveData_GuardTowerManager_V1.DataType.END:
				flag = true;
				Debug.Log(" ***  GuardTowerManager   RestoreData   END");
				break;
			default:
				strError = "GuardTowerManager - Unknown Header type(" + dataType.ToString() + ")";
				return false;
			}
		}
		strError = null;
		return true;
	}

	private SaveData_GuardTowerManager_V1 GetSnapshotData()
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
		if (snapshotData_Base != null && snapshotData_Base.m_Version == 1)
		{
			string saveData = m_SaveData.GetSaveData();
			SaveData_GuardTowerManager_V1 saveData_GuardTowerManager_V = null;
			try
			{
				saveData_GuardTowerManager_V = JsonUtility.FromJson<SaveData_GuardTowerManager_V1>(saveData);
			}
			catch
			{
			}
			if (saveData_GuardTowerManager_V != null)
			{
				return saveData_GuardTowerManager_V;
			}
		}
		return null;
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
				FastList<byte> data = new FastList<byte>();
				CollectData(ref data);
				m_NetView.RPC("RPC_RequestStateResponce_Yes_GuardTowerManager", player, data.ToArray());
			}
			else
			{
				m_NetView.RPC("RPC_RequestStateResponce_No_GuardTowerManager", player);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_GuardTowerManager(byte[] questData, PhotonMessageInfo info)
	{
		if (RestoreData(ref questData, out var strError))
		{
			m_LoadState = LOADSTATE.Finished_OK;
			UpdateSpotlightsActive();
		}
		else
		{
			m_LoadState = LOADSTATE.Finished_Error;
			m_LoadError += strError;
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_GuardTowerManager(PhotonMessageInfo info)
	{
		m_LoadError = "GuardTowerManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}
}
