using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class CombatObjective : BaseObjective
{
	public enum CombatObjectiveTarget
	{
		QuestGiver,
		AnotherPlayer,
		RandomInmate,
		RandomGuard,
		InmateOfQuestGiversGang,
		InmateOfQuestGiversRivalGang,
		SpecifiedCharacter
	}

	public enum CombatObjectiveType
	{
		KnockOut,
		Punch,
		TieUp,
		CauseRuckus
	}

	public enum AllowedRuckusTypes
	{
		MorningRollCall,
		MidDayRollCall,
		EveningRollCall,
		BreakfastTime,
		LunchTime,
		DinnerTime,
		ExcerciseTime,
		ShowerTime,
		NextRollCall,
		NextMealTime,
		COUNT
	}

	public CombatObjectiveTarget m_CombatTarget = CombatObjectiveTarget.RandomInmate;

	public CombatObjectiveType m_CombatType;

	public ObjectiveSceneElement m_CharacterSceneRef;

	public List<AllowedRuckusTypes> m_PossibleRoutinesForRuckus = new List<AllowedRuckusTypes>(1);

	private AllowedRuckusTypes m_RoutineForRuckus = AllowedRuckusTypes.COUNT;

	private Routines m_RuckusRoutine;

	private RoutineSubTypes m_RoutineSubTypeForRuckus;

	private RoomBlob.eLocation m_RuckusLocation = RoomBlob.eLocation.MealHall;

	private Character m_TargetCharacter;

	private bool m_bKnockedOut;

	private bool m_bTiedUp;

	private bool m_bCausedRuckus;

	private bool m_bPunched;

	private bool m_bIsTargetTargetted;

	private const string RUCKUSTOKEN = "$RuckusType";

	private const string COMBATTOKEN = "$CombatTarget";

	protected override void Child_PickAllTargets()
	{
		switch (m_CombatType)
		{
		case CombatObjectiveType.CauseRuckus:
		{
			int index2 = UnityEngine.Random.Range(0, m_PossibleRoutinesForRuckus.Count);
			m_RoutineForRuckus = m_PossibleRoutinesForRuckus[index2];
			if (m_RoutineForRuckus == AllowedRuckusTypes.NextMealTime)
			{
				m_RuckusRoutine = Routines.MealTime;
				RoutineManager.GetInstance().GetRoutineThatMatchesBaseType(m_RuckusRoutine, out m_RoutineSubTypeForRuckus);
				m_RuckusLocation = RoomBlob.eLocation.MealHall;
			}
			else if (m_RoutineForRuckus == AllowedRuckusTypes.NextRollCall)
			{
				m_RuckusRoutine = Routines.RollCall;
				RoutineManager.GetInstance().GetRoutineThatMatchesBaseType(m_RuckusRoutine, out m_RoutineSubTypeForRuckus);
				m_RuckusLocation = RoomBlob.eLocation.RollCall;
			}
			else
			{
				TranslateAllowedRuckusRoutine();
			}
			if (m_PossibleRoutinesForRuckus.Count > 1)
			{
				m_bHasRandomInformation = true;
			}
			string tokenReplacement = "Text.QuestRoutine." + m_RoutineSubTypeForRuckus;
			InternalTokenUpdate("$RuckusType", tokenReplacement, string.Empty);
			break;
		}
		case CombatObjectiveType.KnockOut:
		case CombatObjectiveType.Punch:
		case CombatObjectiveType.TieUp:
			switch (m_CombatTarget)
			{
			case CombatObjectiveTarget.QuestGiver:
				m_TargetCharacter = m_QuestGiver;
				break;
			case CombatObjectiveTarget.AnotherPlayer:
			{
				List<Player> allPlayers = Player.GetAllPlayers();
				if (allPlayers.Count > 1)
				{
					int index;
					do
					{
						index = UnityEngine.Random.Range(0, allPlayers.Count);
					}
					while (!(allPlayers[index] != m_PlayerOwner));
					m_TargetCharacter = allPlayers[index];
				}
				else
				{
					m_ObjectiveStatus = ObjectiveStatus.Canceled;
				}
				break;
			}
			case CombatObjectiveTarget.RandomInmate:
				m_TargetCharacter = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver);
				break;
			case CombatObjectiveTarget.RandomGuard:
				m_TargetCharacter = QuestManager.GetInstance().GetRandomGuard();
				break;
			case CombatObjectiveTarget.InmateOfQuestGiversGang:
				m_ObjectiveStatus = ObjectiveStatus.Canceled;
				break;
			case CombatObjectiveTarget.InmateOfQuestGiversRivalGang:
				m_ObjectiveStatus = ObjectiveStatus.Canceled;
				break;
			case CombatObjectiveTarget.SpecifiedCharacter:
				if (m_CharacterSceneRef != null && m_CharacterSceneRef.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.Character)
				{
					m_TargetCharacter = m_CharacterSceneRef.GetComponent<Character>();
				}
				else
				{
					m_ObjectiveStatus = ObjectiveStatus.Invalid;
				}
				break;
			}
			InternalTokenUpdate("$CombatTarget", m_TargetCharacter.m_CharacterCustomisation.m_DisplayName, string.Empty);
			break;
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$RuckusType", Localization.TokenReplaceType.TextID);
		AddTokenInternal("$CombatTarget", Localization.TokenReplaceType.Character);
	}

	protected override void Child_Reset()
	{
		m_bKnockedOut = false;
		m_bTiedUp = false;
		m_bCausedRuckus = false;
		m_bPunched = false;
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		switch (m_CombatType)
		{
		case CombatObjectiveType.KnockOut:
			if (m_TargetCharacter != null && !m_bKnockedOut)
			{
				Character targetCharacter = m_TargetCharacter;
				targetCharacter.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(OnCharacterKnockedOut));
				Character targetCharacter2 = m_TargetCharacter;
				targetCharacter2.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Combine(targetCharacter2.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(OnCharacterKnockedOut));
			}
			break;
		case CombatObjectiveType.Punch:
			if (m_TargetCharacter != null && !m_bPunched)
			{
				Character targetCharacter5 = m_TargetCharacter;
				targetCharacter5.OnCharacterTookDamage = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter5.OnCharacterTookDamage, new Character.CharacterToCharacterEvent(OnCharacterTookDamage));
				Character targetCharacter6 = m_TargetCharacter;
				targetCharacter6.OnCharacterTookDamage = (Character.CharacterToCharacterEvent)Delegate.Combine(targetCharacter6.OnCharacterTookDamage, new Character.CharacterToCharacterEvent(OnCharacterTookDamage));
			}
			break;
		case CombatObjectiveType.TieUp:
			if (m_TargetCharacter != null && !m_bTiedUp)
			{
				Character targetCharacter3 = m_TargetCharacter;
				targetCharacter3.OnCharacterTiedUp = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter3.OnCharacterTiedUp, new Character.CharacterToCharacterEvent(OnCharacterTiedUp));
				Character targetCharacter4 = m_TargetCharacter;
				targetCharacter4.OnCharacterTiedUp = (Character.CharacterToCharacterEvent)Delegate.Combine(targetCharacter4.OnCharacterTiedUp, new Character.CharacterToCharacterEvent(OnCharacterTiedUp));
			}
			break;
		case CombatObjectiveType.CauseRuckus:
			if (!m_bCausedRuckus)
			{
				ObjectiveManager.GetInstance().RequestListenForRuckusEvents(m_RuckusLocation, m_PlayerOwner.m_NetView.viewID, m_RuckusRoutine, m_RoutineSubTypeForRuckus);
				ObjectiveManager instance = ObjectiveManager.GetInstance();
				instance.OnRuckusEventHappened = (ObjectiveManager.ObjectiveManagerRuckusEvent)Delegate.Remove(instance.OnRuckusEventHappened, new ObjectiveManager.ObjectiveManagerRuckusEvent(OnRuckusEventHappened));
				ObjectiveManager instance2 = ObjectiveManager.GetInstance();
				instance2.OnRuckusEventHappened = (ObjectiveManager.ObjectiveManagerRuckusEvent)Delegate.Combine(instance2.OnRuckusEventHappened, new ObjectiveManager.ObjectiveManagerRuckusEvent(OnRuckusEventHappened));
			}
			break;
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		switch (m_CombatType)
		{
		case CombatObjectiveType.KnockOut:
			if (m_TargetCharacter != null && m_TargetCharacter.GetIsKnockedOut())
			{
				return true;
			}
			break;
		case CombatObjectiveType.Punch:
			if (!(m_TargetCharacter != null))
			{
			}
			break;
		case CombatObjectiveType.TieUp:
			if (!(m_TargetCharacter != null))
			{
			}
			break;
		}
		return false;
	}

	protected override bool Child_EvaluateStatus()
	{
		bool bIsTargetTargetted = m_bIsTargetTargetted;
		m_bIsTargetTargetted = m_PlayerOwner.m_CharacterTarget == m_TargetCharacter;
		if (m_bIsTargetTargetted != bIsTargetTargetted)
		{
			Child_SetHUDArrow(m_bArrowOn);
		}
		return m_CombatType switch
		{
			CombatObjectiveType.KnockOut => m_bKnockedOut, 
			CombatObjectiveType.Punch => m_bPunched, 
			CombatObjectiveType.TieUp => m_bTiedUp, 
			CombatObjectiveType.CauseRuckus => m_bCausedRuckus, 
			_ => true, 
		};
	}

	protected override void Child_SetHUDPins(bool on)
	{
		if (on)
		{
			if (m_TargetCharacter != null && m_PlayerOwner != null && m_CombatType != CombatObjectiveType.CauseRuckus)
			{
				m_TargetCharacter.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
			}
		}
		else if (m_TargetCharacter != null)
		{
			m_TargetCharacter.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (base.PlayerOwner != null)
		{
			if (on && m_TargetCharacter != null && m_TargetCharacter.m_NetView != null && !m_bIsTargetTargetted)
			{
				base.PlayerOwner.SetObjectiveArrowTarget(m_TargetCharacter.m_NetView);
			}
			else
			{
				base.PlayerOwner.CancelObjectiveArrow();
			}
		}
	}

	protected override void Child_PostAction()
	{
		switch (m_CombatType)
		{
		case CombatObjectiveType.KnockOut:
			if (m_TargetCharacter != null)
			{
				Character targetCharacter2 = m_TargetCharacter;
				targetCharacter2.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter2.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(OnCharacterKnockedOut));
			}
			break;
		case CombatObjectiveType.Punch:
			if (m_TargetCharacter != null)
			{
				Character targetCharacter3 = m_TargetCharacter;
				targetCharacter3.OnCharacterTookDamage = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter3.OnCharacterTookDamage, new Character.CharacterToCharacterEvent(OnCharacterTookDamage));
			}
			break;
		case CombatObjectiveType.TieUp:
			if (m_TargetCharacter != null)
			{
				Character targetCharacter = m_TargetCharacter;
				targetCharacter.OnCharacterTiedUp = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter.OnCharacterTiedUp, new Character.CharacterToCharacterEvent(OnCharacterTiedUp));
			}
			break;
		case CombatObjectiveType.CauseRuckus:
		{
			ObjectiveManager instance = ObjectiveManager.GetInstance();
			instance.OnRuckusEventHappened = (ObjectiveManager.ObjectiveManagerRuckusEvent)Delegate.Remove(instance.OnRuckusEventHappened, new ObjectiveManager.ObjectiveManagerRuckusEvent(OnRuckusEventHappened));
			break;
		}
		}
	}

	private void OnRuckusEventHappened(bool succesfull, RoomBlob.eLocation locationToObserve, int playerViewID, Routines BaseRoutineType, RoutineSubTypes SubRoutineType)
	{
		if (playerViewID == m_PlayerOwner.m_NetView.viewID && m_RuckusLocation == locationToObserve && m_RuckusRoutine == BaseRoutineType && m_RoutineSubTypeForRuckus == SubRoutineType)
		{
			if (succesfull)
			{
				m_bCausedRuckus = true;
			}
			else
			{
				m_ObjectiveStatus = ObjectiveStatus.Canceled;
			}
			ObjectiveManager instance = ObjectiveManager.GetInstance();
			instance.OnRuckusEventHappened = (ObjectiveManager.ObjectiveManagerRuckusEvent)Delegate.Remove(instance.OnRuckusEventHappened, new ObjectiveManager.ObjectiveManagerRuckusEvent(OnRuckusEventHappened));
		}
	}

	private void OnCharacterKnockedOut(Character observed, Character attacker)
	{
		if (observed == m_TargetCharacter && attacker == m_PlayerOwner)
		{
			m_bKnockedOut = true;
			Character targetCharacter = m_TargetCharacter;
			targetCharacter.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(OnCharacterKnockedOut));
		}
	}

	private void OnCharacterTookDamage(Character observed, Character attacker)
	{
		if (observed == m_TargetCharacter && attacker == m_PlayerOwner)
		{
			m_bPunched = true;
			Character targetCharacter = m_TargetCharacter;
			targetCharacter.OnCharacterTookDamage = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter.OnCharacterTookDamage, new Character.CharacterToCharacterEvent(OnCharacterTookDamage));
		}
	}

	private void OnCharacterTiedUp(Character observed, Character attacker)
	{
		if (observed == m_TargetCharacter && attacker == m_PlayerOwner)
		{
			m_bTiedUp = true;
			Character targetCharacter = m_TargetCharacter;
			targetCharacter.OnCharacterTiedUp = (Character.CharacterToCharacterEvent)Delegate.Remove(targetCharacter.OnCharacterTiedUp, new Character.CharacterToCharacterEvent(OnCharacterTiedUp));
		}
	}

	private void TranslateAllowedRuckusRoutine()
	{
		switch (m_RoutineForRuckus)
		{
		case AllowedRuckusTypes.MorningRollCall:
			m_RuckusRoutine = Routines.RollCall;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.MorningRollCall;
			m_RuckusLocation = RoomBlob.eLocation.RollCall;
			break;
		case AllowedRuckusTypes.MidDayRollCall:
			m_RuckusRoutine = Routines.RollCall;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.MidDayRollCall;
			m_RuckusLocation = RoomBlob.eLocation.RollCall;
			break;
		case AllowedRuckusTypes.EveningRollCall:
			m_RuckusRoutine = Routines.RollCall;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.EveningRollCall;
			m_RuckusLocation = RoomBlob.eLocation.RollCall;
			break;
		case AllowedRuckusTypes.BreakfastTime:
			m_RuckusRoutine = Routines.MealTime;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.BreakfastTime;
			m_RuckusLocation = RoomBlob.eLocation.MealHall;
			break;
		case AllowedRuckusTypes.LunchTime:
			m_RuckusRoutine = Routines.MealTime;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.LunchTime;
			m_RuckusLocation = RoomBlob.eLocation.MealHall;
			break;
		case AllowedRuckusTypes.DinnerTime:
			m_RuckusRoutine = Routines.MealTime;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.DinnerTime;
			m_RuckusLocation = RoomBlob.eLocation.MealHall;
			break;
		case AllowedRuckusTypes.ExcerciseTime:
			m_RuckusRoutine = Routines.Exercise;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.ExcerciseTime;
			m_RuckusLocation = RoomBlob.eLocation.Gym;
			break;
		case AllowedRuckusTypes.ShowerTime:
			m_RuckusRoutine = Routines.ShowerTime;
			m_RoutineSubTypeForRuckus = RoutineSubTypes.ShowerTime;
			m_RuckusLocation = RoomBlob.eLocation.Shower;
			break;
		}
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			if (m_TargetCharacter != null)
			{
				baseObj.Add(new JProperty("TargetCharacter", m_TargetCharacter.m_NetView.viewID));
			}
			baseObj.Add(new JProperty("RoutineForRuckus", (int)m_RoutineForRuckus));
			baseObj.Add(new JProperty("RuckusRoutine", (int)m_RuckusRoutine));
			baseObj.Add(new JProperty("RoutineSubTypeForRuckus", (int)m_RoutineSubTypeForRuckus));
			baseObj.Add(new JProperty("RuckusLocation", (int)m_RuckusLocation));
			int num = 0;
			num |= (m_bKnockedOut ? 1 : num);
			num |= ((!m_bTiedUp) ? num : 2);
			num |= ((!m_bCausedRuckus) ? num : 4);
			num |= ((!m_bPunched) ? num : 8);
			baseObj.Add(new JProperty("Bools", num));
		}
		baseObj.Add(new JProperty("CombatTarget", (int)m_CombatTarget));
		baseObj.Add(new JProperty("CombatType", (int)m_CombatType));
		if (m_CharacterSceneRef != null)
		{
			baseObj.Add(new JProperty("CharacterTargetRef", m_CharacterSceneRef.m_ObjectiveElementID));
			baseObj.Add(new JProperty("CharacterTargetRef_Scene", m_CharacterSceneRef.m_UsedInScene));
		}
		JProperty jProperty = new JProperty("AllowedRoutines");
		JArray jArray = new JArray();
		for (int i = 0; i < m_PossibleRoutinesForRuckus.Count; i++)
		{
			jArray.Add((int)m_PossibleRoutinesForRuckus[i]);
		}
		jProperty.Add(jArray);
		baseObj.Add(jProperty);
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("TargetCharacter");
			if (jProperty != null)
			{
				int viewID = (int)jProperty.Value;
				m_TargetCharacter = PhotonView.Find(viewID).GetComponent<Character>();
			}
			JProperty jProperty2 = json.Property("RoutineForRuckus");
			if (jProperty2 != null)
			{
				m_RoutineForRuckus = (AllowedRuckusTypes)(int)jProperty2.Value;
			}
			JProperty jProperty3 = json.Property("RuckusRoutine");
			if (jProperty3 != null)
			{
				m_RuckusRoutine = (Routines)(int)jProperty3.Value;
			}
			JProperty jProperty4 = json.Property("RoutineSubTypeForRuckus");
			if (jProperty4 != null)
			{
				m_RoutineSubTypeForRuckus = (RoutineSubTypes)(int)jProperty4.Value;
			}
			JProperty jProperty5 = json.Property("RuckusLocation");
			if (jProperty5 != null)
			{
				m_RuckusLocation = (RoomBlob.eLocation)(int)jProperty5.Value;
			}
			JProperty jProperty6 = json.Property("Bools");
			if (jProperty6 != null)
			{
				int num = (int)jProperty6.Value;
				m_bKnockedOut = (num & 1) != 0;
				m_bTiedUp = (num & 0x10) != 0;
				m_bCausedRuckus = (num & 0x100) != 0;
				m_bPunched = (num & 0x1000) != 0;
			}
			InternalTokenUpdate("$RuckusType", m_RoutineSubTypeForRuckus.ToString(), string.Empty);
			if (m_TargetCharacter != null)
			{
				InternalTokenUpdate("$CombatTarget", m_TargetCharacter.m_CharacterCustomisation.m_DisplayName, string.Empty);
			}
		}
		if (json.Property("CombatTarget") != null)
		{
			m_CombatTarget = (CombatObjectiveTarget)(int)json.Property("CombatTarget").Value;
		}
		if (json.Property("CombatType") != null)
		{
			m_CombatType = (CombatObjectiveType)(int)json.Property("CombatType").Value;
		}
		JProperty jProperty7 = json.Property("CharacterTargetRef");
		if (jProperty7 != null)
		{
			string text = (string)json.Property("CharacterTargetRef_Scene").Value;
			int id = (int)jProperty7.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_CharacterSceneRef = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
		JProperty jProperty8 = json.Property("AllowedRoutines");
		if (jProperty8 != null && jProperty8.Value.Type == JTokenType.Array)
		{
			m_PossibleRoutinesForRuckus.Clear();
			JArray source = (JArray)jProperty8.Value;
			m_PossibleRoutinesForRuckus = source.Select((JToken c) => (AllowedRuckusTypes)(int)c).ToList();
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.CombatObjective;
	}
}
