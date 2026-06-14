using UnityEngine;

[CreateAssetMenu(fileName = "RoutineConfig", menuName = "Team17/Config/Create Routine Config")]
public class RoutineConfig : ScriptableObject
{
	public RoutinesData m_RoutineData;

	public RoutineManager.DayType[] m_CalendarEvents = new RoutineManager.DayType[30];

	public RoutineManager.EventData[] m_EventData = new RoutineManager.EventData[0];
}
