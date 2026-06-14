using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TYPE_AIEventData", menuName = "Team17/Events/Create Event Data")]
public class AIEventData : ScriptableObject
{
	public AIEvent.EventType m_eEventType;

	[Header("Memory")]
	public bool m_bAlwaysUpdateEventPosition;

	[FormerlySerializedAs("m_bForgettableEvent")]
	public bool m_bForgettableEventGuard;

	public bool m_bForgettableEventInmate;

	public bool m_bForgettableInSightGuard;

	public bool m_bForgettableInSightInmate;

	public float m_fEventForgetInSight = 10f;

	[FormerlySerializedAs("m_fEventForgetTime")]
	public float m_fEventOutOfSightForgetTime = 10f;

	public float m_fEventOracleTime = 3f;

	[Range(0f, 100f)]
	[Header("Heat / Alertness")]
	public float m_GuardHeatIncrease = 5f;

	public PrisonAlertness m_PrisonAlertnessIncrease;

	public float m_ReoccuringHeatTime;

	[Header("Responders")]
	public int m_MaxInmateResponders;

	public int m_MaxGuardResponders;

	public int m_MaxSupportResponders;

	public int m_MaxDogResponders;
}
