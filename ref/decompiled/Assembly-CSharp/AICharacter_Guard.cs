using System;
using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

public class AICharacter_Guard : AICharacter
{
	public enum RollCallStatus
	{
		None,
		DoesSpeech,
		DoesShakeDown
	}

	public struct ReportData
	{
		public AIEvent m_Event;

		public int m_PIPEventID;
	}

	[Serializable]
	public class RoutineBanter
	{
		public Routines m_Routine = Routines.UNASSIGNED;

		public string m_SpeechID;

		public bool m_RoomCheck;

		public RoomBlob.eLocation m_RoomRequired;

		public RoutineBanter(Routines routine, string speechID)
		{
			m_Routine = routine;
			m_SpeechID = speechID;
		}
	}

	public RollCallStatus m_RollCallStatus;

	private float m_fRandomAttackTimer;

	private float m_fLowOpinionFollowTimer;

	private AIEventMemory m_LowOpinionFollowMemory;

	private List<AIEvent.EventType> m_ReportableEvents = new List<AIEvent.EventType>();

	private bool m_PlayerResponsibleReportableEvents;

	public List<RoutineBanter> m_RoutineBanterSpeechIDs = new List<RoutineBanter>();

	private Dictionary<Routines, RoutineBanter> m_RoutineBanterSpeechLookup = new Dictionary<Routines, RoutineBanter>();

	private RoutineBanter m_DefaultBanter;

	private RoutineBanter m_CurrentBanter;

	private static List<AICharacter_Guard> m_ShakeDownGuards = null;

	private static List<DeskInteraction> m_DesksToSearchThisRoutine = new List<DeskInteraction>();

	private int m_bDefaultWeaponId = -1;

	private float m_fDayTimeFov = 1.2f;

	public float m_fLightsOutFoV = 0.8f;

	private float m_fDayTimeViewDistance = 1.2f;

	public float m_fLightsOutViewDistance = 5f;

	public string m_AlertnessRaisedMissingKeyLocalisation;

	private static bool m_bAIDebug_AlwaysFindContraband = false;

	protected override void OnAwake()
	{
		BehaviourTreeOwner component = GetComponent<BehaviourTreeOwner>();
		if (component != null)
		{
			string text = ((UnityEngine.Object)component.graph).name;
			text = text.Replace("(Clone)", string.Empty);
			object assetFromBundle = AssetManager.instance.GetAssetFromBundle("aibehaviours", text);
			if (assetFromBundle != null && assetFromBundle is BehaviourTree)
			{
				BehaviourTree newGraph = assetFromBundle as BehaviourTree;
				component.SwitchBehaviour(newGraph);
			}
		}
		m_fDayTimeFov = m_Character.m_fFoV;
		m_fDayTimeViewDistance = m_Character.m_fVisionDistance;
		if (m_RollCallStatus == RollCallStatus.DoesShakeDown)
		{
			if (m_ShakeDownGuards == null)
			{
				m_ShakeDownGuards = new List<AICharacter_Guard>();
			}
			m_ShakeDownGuards.Add(this);
		}
		m_DefaultBanter = new RoutineBanter(Routines.FreeTime, "Text.Inmates.GuardBanter");
		m_CurrentBanter = m_DefaultBanter;
		for (int i = 0; i < m_RoutineBanterSpeechIDs.Count; i++)
		{
			RoutineBanter routineBanter = m_RoutineBanterSpeechIDs[i];
			m_RoutineBanterSpeechLookup.Add(routineBanter.m_Routine, routineBanter);
		}
	}

	protected override void OnStart()
	{
		NPCManager.GetInstance().AddGuard(this);
		RoutineManager.GetInstance().OnRoutineChanged += OnRoutineChanged;
		RoutineManager.GetInstance().OnRoutineEnded += SearchDesks;
		LightingManager instance = LightingManager.GetInstance();
		instance.OnTimeOverridden = (LightingManager.TimeOverridden)Delegate.Combine(instance.OnTimeOverridden, new LightingManager.TimeOverridden(OnTimeOverridden));
		m_Character.OnRoomChanged += RoomChanged;
		if (m_ItemContainer != null)
		{
			ItemContainer itemContainer = m_ItemContainer;
			itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(MissingKeyCheck));
		}
		m_fRandomAttackTimer = ConfigManager.GetInstance().aiConfig.GetRandomAttackTime(m_CharacterPersonality);
		m_fLowOpinionFollowTimer = OpinionManager.GetInstance().GetGuardLowOpinionFollowInterval();
		ItemData defaultStartingItem = m_Character.GetDefaultStartingItem(outfit: false);
		m_bDefaultWeaponId = ((!(defaultStartingItem == null)) ? defaultStartingItem.m_ItemDataID : (-1));
	}

	protected override void OnBecameMasterClientBody()
	{
		base.OnBecameMasterClientBody();
		MissingKeyCheck();
	}

	public new static void CleanUp()
	{
		if (m_ShakeDownGuards != null)
		{
			m_ShakeDownGuards.Clear();
			m_ShakeDownGuards = null;
		}
		if (m_DesksToSearchThisRoutine != null)
		{
			m_DesksToSearchThisRoutine.Clear();
		}
	}

	protected override void OnDestroy()
	{
		m_Character.OnRoomChanged -= RoomChanged;
		base.OnDestroy();
	}

	protected override void OnUpdate()
	{
		if (m_Character.GetIsKnockedOut() || m_Character.GetIsDisabled() || m_Character.GetIsMedicalSleeping())
		{
			return;
		}
		if (m_fRandomAttackTimer > 0f)
		{
			m_fRandomAttackTimer -= UpdateManager.deltaTime;
			if (m_fRandomAttackTimer <= 0f)
			{
				AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
				m_fRandomAttackTimer = aiConfig.GetRandomAttackTime(m_CharacterPersonality);
				RandomAttack();
			}
		}
		if (m_fLowOpinionFollowTimer > 0f)
		{
			m_fLowOpinionFollowTimer -= UpdateManager.deltaTime;
			if (m_fLowOpinionFollowTimer <= 0f && !LowOpinionFollow())
			{
				m_fLowOpinionFollowTimer = OpinionManager.GetInstance().GetGuardLowOpinionFollowInterval();
			}
		}
	}

	public override void OnMedicBedInteractionStarted()
	{
		m_bIsDueMedicBedMissingKeyCheck = true;
		m_bRequiresSerialization = true;
	}

	public override void OnMedicBedInteractionEnded()
	{
		MissingKeyCheck();
		m_bIsDueMedicBedMissingKeyCheck = false;
		m_bRequiresSerialization = true;
	}

	public override void OnEscapeBindings()
	{
		MissingKeyCheck();
	}

	public GameObject GetGuardComputer()
	{
		List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.ControlRoom);
		if (allRoomsByLocation == null)
		{
			return null;
		}
		for (int i = 0; i < allRoomsByLocation.Count; i++)
		{
			RoomBlob_ControlRoom roomBlobData = allRoomsByLocation[i].GetRoomBlobData<RoomBlob_ControlRoom>();
			if (!(roomBlobData == null) && !(roomBlobData.Computer == null))
			{
				return roomBlobData.Computer.gameObject;
			}
		}
		return null;
	}

	public void OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		PickDesksToSearch(oldRoutine, newRoutine, forceEnd);
		ToggleSpotlight(oldRoutine, newRoutine, forceEnd);
		SetBanterSpeech(oldRoutine, newRoutine, forceEnd);
	}

	public void OnTimeOverridden(bool overridden, int overrideHours, int overrideMins)
	{
		RoutineManager instance = RoutineManager.GetInstance();
		RoutinesData.Routine currentRoutine = instance.GetCurrentRoutine();
		RoutinesData.Routine routineForTime = instance.GetRoutineForTime(overrideHours, overrideMins);
		ToggleSpotlight(currentRoutine, (!overridden) ? currentRoutine : routineForTime, forceEnd: true);
	}

	public void PickDesksToSearch(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType != 0)
		{
			return;
		}
		List<DeskInteraction> inmateDesks = DeskInteraction.GetInmateDesks();
		List<DeskInteraction> playerDesks = DeskInteraction.GetPlayerDesks();
		float chanceToSearchPlayersDesk = ConfigManager.GetInstance().aiConfig.GetChanceToSearchPlayersDesk();
		int numberOfDesksToSearch = ConfigManager.GetInstance().aiConfig.GetNumberOfDesksToSearch();
		if (inmateDesks != null && inmateDesks.Count > 0)
		{
			inmateDesks.Shuffle();
		}
		if (playerDesks != null && playerDesks.Count > 0)
		{
			playerDesks.Shuffle();
		}
		m_DesksToSearchThisRoutine.Clear();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < numberOfDesksToSearch; i++)
		{
			if (UnityEngine.Random.value <= chanceToSearchPlayersDesk && playerDesks != null && num < playerDesks.Count)
			{
				m_DesksToSearchThisRoutine.Add(playerDesks[num]);
				num++;
			}
			else if (inmateDesks != null && num2 < inmateDesks.Count)
			{
				m_DesksToSearchThisRoutine.Add(inmateDesks[num2]);
				num2++;
			}
		}
	}

	public void SearchDesks(RoutinesData.Routine routine, bool forceEnd)
	{
		if (routine.m_BaseRoutineType != 0 || m_DesksToSearchThisRoutine == null || m_DesksToSearchThisRoutine.Count == 0 || m_ShakeDownGuards == null || m_ShakeDownGuards.Count == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_DesksToSearchThisRoutine.Count; i++)
		{
			DeskEventManager component = m_DesksToSearchThisRoutine[i].GetComponent<DeskEventManager>();
			if (!(component == null))
			{
				AIEvent investigateObjectEvent = component.GetInvestigateObjectEvent();
				m_ShakeDownGuards[num].AddEvent(investigateObjectEvent);
				ChatFeedManager instance = ChatFeedManager.GetInstance();
				DeskInteraction desk = component.m_Desk;
				if ((bool)instance && (bool)desk && (bool)desk.m_DeskOwner)
				{
					instance.SendSystemMessageLocalized_RPC("Text.HUD.DeskSearchMessage", "$CharacterName", desk.m_DeskOwner.m_NetView.viewID, ChatFeedManager.MessageTag.Prison);
				}
				num++;
				if (num >= m_ShakeDownGuards.Count)
				{
					num = 0;
				}
			}
		}
		m_DesksToSearchThisRoutine.Clear();
	}

	public void SearchCurrentlyOpenedDesk()
	{
		if (m_Character.m_OpenContainer == null)
		{
			return;
		}
		ItemContainer openContainer = m_Character.m_OpenContainer;
		List<Item> contrabandItems = null;
		if (!openContainer.HasContrabandItems(ref contrabandItems) && !m_bAIDebug_AlwaysFindContraband)
		{
			return;
		}
		Character characterOwner = openContainer.GetCharacterOwner();
		bool flag = false;
		if (openContainer.GetCharacterOwner() != null && openContainer.GetCharacterOwner().m_CharacterStats.m_bIsPlayer)
		{
			flag = true;
		}
		int num = ((!flag) ? 5 : 10);
		SpeechManager instance = SpeechManager.GetInstance();
		Character character = m_Character;
		string textID = "Text.Guard.SentToContrabandDesk";
		SpeechTone tone = SpeechTone.Negative;
		float duration = 2f;
		int priority = num;
		bool bAllowTextRecolour = flag;
		instance.SaySomething(character, textID, tone, duration, priority, -1, ignoreStatus: false, bAllowTextRecolour);
		PrisonAlertnessManager.IncreaseAlertnessForContrabandItems(contrabandItems, PrisonAlertnessManager.AlertnessReason.ContrabandInContainer, characterOwner, shouldPunishCharacter: true);
		if (contrabandItems != null)
		{
			for (int num2 = contrabandItems.Count - 1; num2 >= 0; num2--)
			{
				Item item = contrabandItems[num2];
				bool flag2 = MoveItemToContrabandDesk(item);
				openContainer.RemoveItemRPC(item, !flag2);
			}
		}
		if (flag)
		{
			DeskEventManager component = m_Character.m_OpenContainer.GetComponent<DeskEventManager>();
			AIEvent contrabandInDeskEvent = component.GetContrabandInDeskEvent();
			AddEvent(contrabandInDeskEvent);
		}
	}

	public void SearchCurrentlyOpenedToilet()
	{
		if (m_Character.m_OpenContainer == null)
		{
			return;
		}
		ItemContainer openContainer = m_Character.m_OpenContainer;
		ToiletEventManager component = openContainer.GetComponent<ToiletEventManager>();
		if (component == null)
		{
			return;
		}
		AIEvent toiletFloodEvent = component.GetToiletFloodEvent();
		AIEvent contrabandInToiletEvent = component.GetContrabandInToiletEvent();
		NPCManager.GetInstance().CallMaintenanceMen(toiletFloodEvent);
		List<Item> contrabandItems = null;
		if (!openContainer.HasContrabandItems(ref contrabandItems) && !m_bAIDebug_AlwaysFindContraband)
		{
			return;
		}
		RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(openContainer.transform.position);
		bool flag = false;
		Character characterResponsible = null;
		if (roomBlob != null && roomBlob.location == RoomBlob.eLocation.InmateCell)
		{
			RoomBlob_Cell component2 = roomBlob.GetComponent<RoomBlob_Cell>();
			for (int i = 0; i < component2.m_SpawnPoints.Count; i++)
			{
				float guardHeatIncrease = contrabandInToiletEvent.m_EventData.m_GuardHeatIncrease;
				SpawnPoint spawnPoint = component2.m_SpawnPoints[i];
				if (spawnPoint == null)
				{
					continue;
				}
				Character characterOwner = spawnPoint.GetCharacterOwner();
				if (!(characterOwner == null))
				{
					characterResponsible = characterOwner;
					characterOwner.m_CharacterStats.IncreaseHeat(guardHeatIncrease);
					if (characterOwner.m_CharacterStats.m_bIsPlayer)
					{
						flag = true;
						EffectManager.PlayEffect(EffectManager.effectType.HeatIncreased, m_Character.GetStatChangeEffectPosition(), m_NetView.photonView);
					}
				}
			}
		}
		int num = ((!flag) ? 5 : 10);
		SpeechManager instance = SpeechManager.GetInstance();
		Character character = m_Character;
		string textID = "Text.Guard.SentToContrabandDesk";
		SpeechTone tone = SpeechTone.Negative;
		float duration = 1f;
		int priority = num;
		bool bAllowTextRecolour = flag;
		instance.SaySomething(character, textID, tone, duration, priority, -1, ignoreStatus: false, bAllowTextRecolour);
		PrisonAlertnessManager.IncreaseAlertnessForContrabandItems(contrabandItems, PrisonAlertnessManager.AlertnessReason.ContrabandInContainer, characterResponsible, shouldPunishCharacter: true);
		if (contrabandItems != null)
		{
			for (int num2 = contrabandItems.Count - 1; num2 >= 0; num2--)
			{
				Item item = contrabandItems[num2];
				bool flag2 = MoveItemToContrabandDesk(item);
				openContainer.RemoveItemRPC(item, !flag2);
			}
		}
	}

	public void ToggleSpotlight(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType == Routines.LightsOut)
		{
			m_Character.m_CharacterAnimator.ActivateSpotlightRoutine(enabled: true);
			m_Character.m_fVisionDistance = m_fLightsOutViewDistance;
			m_Character.m_fFoV = m_fLightsOutFoV;
		}
		else if (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.LightsOut && newRoutine.m_BaseRoutineType != Routines.Lockdown)
		{
			m_Character.m_CharacterAnimator.ActivateSpotlightRoutine(enabled: false);
			m_Character.m_fVisionDistance = m_fDayTimeViewDistance;
			m_Character.m_fFoV = m_fDayTimeFov;
		}
	}

	public void SetBanterSpeech(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (m_RoutineBanterSpeechLookup.TryGetValue(newRoutine.m_BaseRoutineType, out var value))
		{
			m_CurrentBanter = value;
		}
		else
		{
			m_CurrentBanter = m_DefaultBanter;
		}
		bool flag = true;
		if (m_CurrentBanter.m_RoomCheck && m_Character.m_CurrentLocation != null)
		{
			flag = m_Character.m_CurrentLocation.location == m_CurrentBanter.m_RoomRequired;
		}
		m_AIBlackboard.SetValue("RoutineSpeechID", (!flag) ? m_DefaultBanter.m_SpeechID : m_CurrentBanter.m_SpeechID);
	}

	public void RoomChanged(RoomBlob oldRoom, RoomBlob newRoom)
	{
		if (m_CurrentBanter.m_RoomCheck && m_Character.m_CurrentLocation != null)
		{
			bool flag = m_Character.m_CurrentLocation.location == m_CurrentBanter.m_RoomRequired;
			m_AIBlackboard.SetValue("RoutineSpeechID", (!flag) ? m_DefaultBanter.m_SpeechID : m_CurrentBanter.m_SpeechID);
		}
	}

	protected bool LowOpinionFollow()
	{
		if (m_LowOpinionFollowMemory != null)
		{
			return false;
		}
		if (RoutineManager.GetInstance() != null && RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.LightsOut)
		{
			return false;
		}
		if (m_Character.m_CharacterOpinions == null)
		{
			return false;
		}
		IList<Character> allHatedCharacters = m_Character.m_CharacterOpinions.GetAllHatedCharacters();
		if (allHatedCharacters != null && allHatedCharacters.Count > 0)
		{
			for (int i = 0; i < allHatedCharacters.Count; i++)
			{
				Character character = allHatedCharacters[i];
				if (character == null)
				{
					continue;
				}
				bool haveCollisionData = false;
				if (!character.GetIsKnockedOut() && m_CharacterUtil.LineOfSight(character.gameObject, out haveCollisionData))
				{
					if (character.m_CharacterEventManager != null)
					{
						AIEvent suspiciousAIEvent = character.m_CharacterEventManager.GetSuspiciousAIEvent();
						AddEvent(suspiciousAIEvent);
						m_LowOpinionFollowMemory = FindEventInMemory(suspiciousAIEvent);
						return true;
					}
					break;
				}
			}
		}
		return false;
	}

	public override void OnRegainConsciousness()
	{
		base.OnRegainConsciousness();
	}

	private void MissingKeyCheck()
	{
		if (m_Character.m_bIsKnockedOut)
		{
			return;
		}
		ItemManager instance = ItemManager.GetInstance();
		List<int> list = null;
		if (instance != null)
		{
			list = instance.GetTrackedItemsForContainer(m_ItemContainer);
		}
		if (list == null)
		{
			return;
		}
		bool flag = false;
		int num = ((!(m_Character.GetEquippedItem() != null)) ? (-1) : m_Character.GetEquippedItem().m_NetView.viewID);
		for (int i = 0; i < list.Count; i++)
		{
			Item itemFromUsedListByNetView = instance.GetItemFromUsedListByNetView(list[i]);
			if (itemFromUsedListByNetView == null || (!itemFromUsedListByNetView.HasFunctionality(BaseItemFunctionality.Functionality.Key) && !itemFromUsedListByNetView.HasFunctionality(BaseItemFunctionality.Functionality.Keycard)))
			{
				continue;
			}
			ItemEventManager component = itemFromUsedListByNetView.GetComponent<ItemEventManager>();
			if (component == null)
			{
				continue;
			}
			if (m_ItemContainer.HasSpecificItem(list[i]) || num == list[i])
			{
				if (SolitaryManager.GetInstance().IsKeyByNetviewMissing(list[i]))
				{
					SolitaryManager.GetInstance().SetKeyFound(itemFromUsedListByNetView.m_NetView.viewID);
				}
				continue;
			}
			flag = true;
			AIEvent itemMissingEvent = component.GetItemMissingEvent();
			if (itemMissingEvent != null)
			{
				NPCManager.GetInstance().CallDoggies(itemMissingEvent);
			}
			SolitaryManager.GetInstance().SetKeyIsMissing(itemFromUsedListByNetView.m_NetView.viewID);
		}
		if (m_bWasKeyRespawnedOnUs)
		{
			m_bWasKeyRespawnedOnUs = false;
			if (!flag)
			{
				PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(11, PrisonAlertnessManager.AlertnessReason.ItemMissing);
				SolitaryManager.GetInstance().TriggerLockdown(isMiniLockdown: true);
			}
		}
	}

	protected override void OnAddingEventToMemory(AIEvent aiEvent, AIEventMemory memory, bool silent)
	{
		if (aiEvent == null || aiEvent.m_EventData == null || silent)
		{
			return;
		}
		Character characterResponsible = aiEvent.m_CharacterResponsible;
		if (!(characterResponsible != null) || characterResponsible.m_CharacterRole != 0)
		{
			return;
		}
		int prisonAlertnessIncrease = (int)aiEvent.m_EventData.m_PrisonAlertnessIncrease;
		float num = aiEvent.m_EventData.m_GuardHeatIncrease;
		if (!m_ReportableEvents.Contains(aiEvent.m_EventData.m_eEventType))
		{
			PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(prisonAlertnessIncrease, characterResponsible, aiEvent.m_EventData.m_eEventType);
		}
		if (aiEvent.m_EventData.m_eEventType == AIEvent.EventType.Character_Tardy && RoutineManager.GetInstance().GetCurrentRoutine().m_BaseRoutineType == Routines.LightsOut)
		{
			AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
			if (aiConfig != null)
			{
				PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(aiConfig.GetAlertnessIncreaseAtLightsout(), characterResponsible, PrisonAlertnessManager.AlertnessReason.OutDuringLightsOut);
				num = aiConfig.GetHeatIncreaseAtLightsout();
			}
		}
		characterResponsible.m_CharacterStats.IncreaseHeat(num);
		if (characterResponsible.m_CharacterStats.m_bIsPlayer)
		{
			EffectManager.PlayEffect(EffectManager.effectType.HeatIncreased, m_Character.GetStatChangeEffectPosition(), m_NetView.photonView);
		}
		if (PrisonAlertnessManager.GetInstance().AtFiveStars() && num > 0f)
		{
			SolitaryManager.GetInstance().SetWantedForSolitary(characterResponsible);
		}
	}

	protected override void OnMemoryForgotten(AIEventMemory memory)
	{
		if (memory == null)
		{
			return;
		}
		if (memory == m_LowOpinionFollowMemory)
		{
			m_fLowOpinionFollowTimer = OpinionManager.GetInstance().GetGuardLowOpinionFollowInterval();
			m_LowOpinionFollowMemory = null;
		}
		if (memory.m_eEventType == AIEvent.EventType.Character_Disguised && !memory.m_CharacterResponsible.m_bIsDisguised)
		{
			bool flag = memory.m_TargetCharacter != null && memory.m_TargetCharacter.m_CharacterStats.m_bIsPlayer;
			int num = ((!flag) ? 5 : 10);
			SpeechManager instance = SpeechManager.GetInstance();
			Character character = m_Character;
			string textID = "Text.GuardAlert.DisguiseCaught";
			SpeechTone tone = SpeechTone.Negative;
			float duration = 1f;
			int priority = num;
			bool bAllowTextRecolour = flag;
			instance.SaySomething(character, textID, tone, duration, priority, -1, ignoreStatus: false, bAllowTextRecolour);
		}
		if (memory.m_AIEvent != null && memory.m_AIEvent.m_EventData != null && memory.m_AIEvent.m_EventData.m_eEventType == AIEvent.EventType.InvestigateLocation && memory.m_AIEvent.m_Manager != null)
		{
			GenericEventManager genericEventManager = memory.m_AIEvent.m_Manager as GenericEventManager;
			if (genericEventManager != null)
			{
				genericEventManager.DisableEventVisability(memory.m_AIEvent.m_EventData);
			}
		}
	}

	protected override void OnForgetEverything()
	{
		ForgetReportableEvents();
	}

	public void GenerateReport(AIEventMemory aiEventMemory)
	{
		GenerateReport(aiEventMemory.m_AIEvent);
	}

	public void GenerateReport(AIEvent aiEvent)
	{
		if (!m_Character.m_bIsKnockedOut)
		{
			ReportData item = default(ReportData);
			item.m_Event = aiEvent;
			switch (aiEvent.m_EventData.m_eEventType)
			{
			case AIEvent.EventType.Tile_DamagedTile:
			case AIEvent.EventType.Tile_MissingTile:
			case AIEvent.EventType.Tile_DugHole:
				item.m_PIPEventID = PIPManager.GetInstance().NewGlobalPIPEvent(PIPManager.PIPEventType.GuardAlertDamagedPrison, m_NetView.viewID, 1);
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Damage Reported", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Damage Reported", Gamer.GetGamerCount() + " Player", 0L);
				break;
			case AIEvent.EventType.Item_ContrabandInContainer:
				item.m_PIPEventID = PIPManager.GetInstance().NewGlobalPIPEvent(PIPManager.PIPEventType.GuardAlertDesk, m_NetView.viewID, 1);
				break;
			default:
				item.m_PIPEventID = PIPManager.GetInstance().NewGlobalPIPEvent(PIPManager.PIPEventType.GuardAlert, m_NetView.viewID, 1);
				break;
			}
			m_EventsToReport.Add(item);
			if (aiEvent != null && aiEvent.m_CharacterResponsible != null && aiEvent.m_CharacterResponsible.m_CharacterStats != null)
			{
				m_PlayerResponsibleReportableEvents |= aiEvent.m_CharacterResponsible.m_CharacterStats.m_bIsPlayer;
			}
			m_bRequiresSerialization = true;
		}
	}

	public bool HaveEventsToReport(out bool havePlayerResponsible)
	{
		havePlayerResponsible = m_PlayerResponsibleReportableEvents;
		return m_EventsToReport != null && m_EventsToReport.Count > 0;
	}

	public void SubmitReportableEvents()
	{
		NPCManager.GetInstance().SubmitReports(m_EventsToReport);
		m_PlayerResponsibleReportableEvents = false;
		for (int i = 0; i < m_EventsToReport.Count; i++)
		{
			AIEvent @event = m_EventsToReport[i].m_Event;
			if (@event.m_CharacterResponsible != null)
			{
				Character characterResponsible = @event.m_CharacterResponsible;
				bool punishCharacter = true;
				if (@event.m_EventData.m_eEventType == AIEvent.EventType.Tile_DamagedTile || @event.m_EventData.m_eEventType == AIEvent.EventType.Tile_DugHole || @event.m_EventData.m_eEventType == AIEvent.EventType.Tile_Flooded || @event.m_EventData.m_eEventType == AIEvent.EventType.Tile_MissingTile)
				{
					punishCharacter = false;
				}
				PrisonAlertnessManager.GetInstance().IncrementAlertnessBy((int)@event.m_EventData.m_PrisonAlertnessIncrease, characterResponsible, @event.m_EventData.m_eEventType, punishCharacter);
			}
		}
		ForgetReportableEvents();
	}

	public void ForgetReportableEvents()
	{
		PIPManager instance = PIPManager.GetInstance();
		if (instance != null)
		{
			for (int i = 0; i < m_EventsToReport.Count; i++)
			{
				if (m_EventsToReport[i].m_PIPEventID != -1)
				{
					instance.EndPIPEvent(m_EventsToReport[i].m_PIPEventID);
				}
			}
		}
		m_EventsToReport.Clear();
	}

	public void AddReportableEvent(AIEvent.EventType eventType, OnAIEventCallback callback)
	{
		ListenForEvent(callback, eventType);
		m_ReportableEvents.Add(eventType);
	}

	public bool IsTargetDisguised(AIEventMemory target)
	{
		if (target != null && target.m_CharacterResponsible != null)
		{
			return target.m_CharacterResponsible.m_bIsDisguised;
		}
		return false;
	}

	public void ForgetDisguise(AIEventMemory target)
	{
		if (target == null || target.m_CharacterResponsible == null)
		{
			return;
		}
		List<AIEventMemory> knownEvents = GetKnownEvents(AIEvent.EventType.Character_Disguised);
		for (int i = 0; i < knownEvents.Count; i++)
		{
			if (knownEvents[i].m_CharacterResponsible != null && knownEvents[i].m_CharacterResponsible == target.m_CharacterResponsible)
			{
				ForgetEvent(knownEvents[i]);
			}
		}
	}

	public static List<int> GetNamesOfInmatesToSearch()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < m_DesksToSearchThisRoutine.Count; i++)
		{
			if (m_DesksToSearchThisRoutine[i] != null && (bool)m_DesksToSearchThisRoutine[i].GetOwner() && m_DesksToSearchThisRoutine[i].GetOwner().m_CharacterCustomisation != null)
			{
				list.Add(m_DesksToSearchThisRoutine[i].GetOwner().m_NetView.viewID);
			}
		}
		return list;
	}

	public bool GivesRollCallSpeech()
	{
		if (m_RollCallStatus == RollCallStatus.DoesSpeech)
		{
			return true;
		}
		return false;
	}

	public bool HasDefaultWeapon()
	{
		Item equippedItem = m_Character.GetEquippedItem();
		if (equippedItem != null)
		{
			return m_bDefaultWeaponId == equippedItem.ItemDataID;
		}
		return m_bDefaultWeaponId <= 0;
	}

	public void EquipDefaultWeapon()
	{
		m_Character.EquipStartingWeapon();
	}

	public void RemoveSwagBagEvent(AIEventMemory eventMemory)
	{
		SwagBagEventManager swagBagEventManager = (SwagBagEventManager)eventMemory.m_AIEvent.m_Manager;
		if (swagBagEventManager != null)
		{
			swagBagEventManager.ClearSwagBagEvent();
		}
	}

	public void ClearCurrentlyOpenSwagBag()
	{
		if (m_Character.m_OpenContainer != null)
		{
			m_Character.m_OpenContainer.RemoveAllItems(releaseToManager: true, exemptQuestItems: false, bLeaveKeys: false, includeHidden: true);
		}
	}

	public void CheckSecurityRoomTutorial()
	{
		for (int i = 0; i < m_EventsToReport.Count; i++)
		{
			if ((int)m_EventsToReport[i].m_Event.m_EventData.m_PrisonAlertnessIncrease <= 0)
			{
				continue;
			}
			TutorialManager instance = TutorialManager.GetInstance();
			if (!(instance != null))
			{
				continue;
			}
			List<Player> allPlayers = Player.GetAllPlayers();
			if (allPlayers == null)
			{
				break;
			}
			for (int j = 0; j < allPlayers.Count; j++)
			{
				if (allPlayers[j] != null && allPlayers[j].m_Gamer != null)
				{
					instance.StartTutorialRPC(allPlayers[j], TutorialSubject.SecurityRoom);
				}
			}
			break;
		}
	}

	public override void Post_RealDeserialize(bool isFromSaveFile)
	{
		base.Post_RealDeserialize(isFromSaveFile);
		if (isFromSaveFile && m_bIsDueMedicBedMissingKeyCheck)
		{
			MissingKeyCheck();
			m_bIsDueMedicBedMissingKeyCheck = false;
			m_bRequiresSerialization = true;
		}
	}

	public static bool AIGuard_AlwaysFindContraband(bool bPos, bool bJustRead)
	{
		if (!bJustRead)
		{
			m_bAIDebug_AlwaysFindContraband = bPos;
		}
		return m_bAIDebug_AlwaysFindContraband;
	}
}
