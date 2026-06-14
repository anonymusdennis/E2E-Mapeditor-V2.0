using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

[CreateAssetMenu(fileName = "AIConfig", menuName = "Team17/Config/Create AI Config")]
public class AIConfig : ScriptableObject
{
	[SerializeField]
	[Header("Random Attack")]
	private Personality.FloatSetting RandomAttackMinTime = new Personality.FloatSetting();

	[SerializeField]
	private Personality.FloatSetting RandomAttackMaxTime = new Personality.FloatSetting();

	[SerializeField]
	[Header("Guard Heat")]
	private int m_GuardSuspiciousHeat;

	[SerializeField]
	private int m_GuardWantedHeat;

	[Tooltip("During this time a guard will follow you if they see you. This is not the length of time that a guard will follow you for.")]
	[SerializeField]
	private float m_GuardSuspiciousFollowMinTime;

	[SerializeField]
	private float m_GuardSuspiciousFollowMaxTime;

	[SerializeField]
	private float m_GuardSuspiciousIgnoreMinTime;

	[SerializeField]
	private float m_GuardSuspiciousIgnoreMaxTime;

	[SerializeField]
	[Header("Roll Call Data")]
	private float ChanceToSearchPlayerDesk = 0.05f;

	[SerializeField]
	private int NumberOfDesksToSearch = 2;

	[SerializeField]
	[Header("Lights Out")]
	private PrisonAlertness AlertnessIncreaseAtLightsout = PrisonAlertness.Lockdown;

	[SerializeField]
	[Range(0f, 100f)]
	private int HeatIncreaseAtLightsout = 100;

	[SerializeField]
	[Header("Doggie")]
	public float MaxDogPauseTime = 2f;

	public float DogPauseReduction = 2f;

	[Range(0f, 100f)]
	public int DogLoveOpinion = 60;

	[Range(0f, 100f)]
	public float DogContrabandDetectionHeat = 100f;

	[Range(0f, 100f)]
	[SerializeField]
	[Header("Medic Bed Convalesce")]
	public float InmateConvalesceMinTime = 5f;

	[Range(0f, 100f)]
	[SerializeField]
	public float InmateConvalesceMaxTime = 20f;

	[Range(0f, 100f)]
	[SerializeField]
	[Header("Inmate Snitching")]
	public int InmateSnitchLikeOpinion = 75;

	[Range(0f, 100f)]
	[SerializeField]
	public float InmateSnitchToGuardMaxDistance = 20f;

	[Header("Inmate Disguises")]
	public List<AIEvent.EventType> DisguiseableEvents = new List<AIEvent.EventType>();

	public int DisguiseBreakHeat = 80;

	[SerializeField]
	[Header("Bed Sheets on Bars")]
	private PrisonAlertness TakeDownBedSheetAlertness = PrisonAlertness.FourStars;

	[SerializeField]
	[Header("Combat Behaviours")]
	public BehaviourTree[] m_CombatBehaviours;

	[SerializeField]
	[Header("Swag Bag Detection")]
	public float SwagBagInvisibleTime;

	[SerializeField]
	[Header("Opinions")]
	private int OpinionDecreaseWhenKickedOffInterativeObject = 10;

	[SerializeField]
	[Header("Medic")]
	private float MedicWaitForPickUpTime = 3f;

	public int GuardSuspiciousHeat => m_GuardSuspiciousHeat;

	public int GuardWantedHeat => m_GuardWantedHeat;

	public float GetRandomAttackTime(Personality.PersonalityType personality)
	{
		return Random.Range(RandomAttackMinTime.GetValue(personality), RandomAttackMaxTime.GetValue(personality));
	}

	public float GetGuardFollowTime()
	{
		return Random.Range(m_GuardSuspiciousFollowMinTime, m_GuardSuspiciousFollowMaxTime);
	}

	public float GetGuardIgnoreTime()
	{
		return Random.Range(m_GuardSuspiciousIgnoreMinTime, m_GuardSuspiciousIgnoreMaxTime);
	}

	public float GetChanceToSearchPlayersDesk()
	{
		return ChanceToSearchPlayerDesk;
	}

	public int GetNumberOfDesksToSearch()
	{
		return NumberOfDesksToSearch;
	}

	public int GetAlertnessIncreaseAtLightsout()
	{
		return (int)AlertnessIncreaseAtLightsout;
	}

	public int GetHeatIncreaseAtLightsout()
	{
		return HeatIncreaseAtLightsout;
	}

	public float GetDogMaxPauseTime()
	{
		return MaxDogPauseTime;
	}

	public float GetDogPauseTimeReduction()
	{
		return DogPauseReduction;
	}

	public int GetDogLoveOpinion()
	{
		return DogLoveOpinion;
	}

	public float GetDogContrabandHeat()
	{
		return DogContrabandDetectionHeat;
	}

	public float GetInmateConvalesceMinTime()
	{
		return InmateConvalesceMinTime;
	}

	public float GetInmateConvalesceMaxTime()
	{
		return InmateConvalesceMaxTime;
	}

	public int GetInmateSnitchLikeOpinion()
	{
		return InmateSnitchLikeOpinion;
	}

	public float GetInmateSnitchToGuardMaxDistance()
	{
		return InmateSnitchToGuardMaxDistance;
	}

	public PrisonAlertness GetTakeDownBedSheetAlertness()
	{
		return TakeDownBedSheetAlertness;
	}

	public float GetSwagBagInvisibleTime()
	{
		return SwagBagInvisibleTime;
	}

	public BehaviourTree GetCombatBehaviour(Personality.PersonalityType personality)
	{
		if (m_CombatBehaviours == null || m_CombatBehaviours.Length == 0)
		{
			return null;
		}
		int combatStyle = (int)Personality.GetCombatStyle(personality);
		combatStyle = Mathf.Clamp(combatStyle, 0, m_CombatBehaviours.Length - 1);
		return m_CombatBehaviours[combatStyle];
	}

	public int GetOpinionDecreaseWhenKickedOffInterativeObject()
	{
		return OpinionDecreaseWhenKickedOffInterativeObject;
	}

	public float GetMedicWaitForPickUpTime()
	{
		return MedicWaitForPickUpTime;
	}
}
