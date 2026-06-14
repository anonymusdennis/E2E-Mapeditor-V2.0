using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "RoutinesData", menuName = "Team17/Create Routines Data")]
public class RoutinesData : ScriptableObject
{
	[Serializable]
	public class Routine
	{
		public string m_LocalizationTag = "HUD.Routine.Name";

		public Routines m_BaseRoutineType = Routines.FreeTime;

		public RoutineSubTypes m_SubRoutineType = RoutineSubTypes.MorningFreeTime;

		public Events m_RoutineMusic = Events.Play_Music_Prison_01_Routine_A;

		public bool m_bHasSaveLoadRoutineMusic;

		public Events m_SaveLoadRoutineMusic = Events.Play_Music_Prison_01_Routine_A;

		public int m_StartHour = 7;

		public int m_StartMinutes = 30;

		public int m_EndHour = 10;

		public int m_EndMinutes = 30;

		[Range(0f, 100f)]
		public int m_AddedHeatWhenMissed;

		[Range(0f, 11f)]
		public int m_AddedAlertnessWhenMissed;

		[Range(0f, 10f)]
		public int m_PostLockdownAlertness = 6;

		public int m_TimeToGetToRoutine = 12;

		private int m_StartInMinutes = -1;

		private int m_EndInMinutes = -1;

		public int m_Index = -1;

		public int StartInMinutes
		{
			get
			{
				if (m_StartInMinutes == -1)
				{
					m_StartInMinutes = m_StartHour * 60 + m_StartMinutes;
				}
				return m_StartInMinutes;
			}
		}

		public int EndInMinutes
		{
			get
			{
				if (m_EndInMinutes == -1)
				{
					m_EndInMinutes = m_EndHour * 60 + m_EndMinutes;
				}
				return m_EndInMinutes;
			}
		}

		public Routine()
		{
		}

		public Routine(Routines baseRoutineType, RoutineSubTypes subRoutineType, string localizationTag)
		{
			m_BaseRoutineType = baseRoutineType;
			subRoutineType = m_SubRoutineType;
			m_LocalizationTag = localizationTag;
		}

		public Routine(Routine copyThis)
		{
			m_LocalizationTag = copyThis.m_LocalizationTag;
			m_BaseRoutineType = copyThis.m_BaseRoutineType;
			m_SubRoutineType = copyThis.m_SubRoutineType;
			m_RoutineMusic = copyThis.m_RoutineMusic;
			m_bHasSaveLoadRoutineMusic = copyThis.m_bHasSaveLoadRoutineMusic;
			m_SaveLoadRoutineMusic = copyThis.m_SaveLoadRoutineMusic;
			m_StartHour = copyThis.m_StartHour;
			m_StartMinutes = copyThis.m_StartMinutes;
			m_EndHour = copyThis.m_EndHour;
			m_EndMinutes = copyThis.m_EndMinutes;
			m_AddedHeatWhenMissed = copyThis.m_AddedHeatWhenMissed;
			m_AddedAlertnessWhenMissed = copyThis.m_AddedAlertnessWhenMissed;
			m_PostLockdownAlertness = copyThis.m_PostLockdownAlertness;
			m_TimeToGetToRoutine = copyThis.m_TimeToGetToRoutine;
			m_StartInMinutes = copyThis.m_StartInMinutes;
			m_EndInMinutes = copyThis.m_EndInMinutes;
			m_Index = copyThis.m_Index;
		}

		public bool IsTimeWithinRoutine(int hour, int min)
		{
			return TimeHelper.IsTimeWithinRange(hour, min, m_StartHour, m_StartMinutes, m_EndHour, m_EndMinutes);
		}

		public TimeSpan GetRoutineDuration()
		{
			TimeSpan timeSpan = new TimeSpan(0, m_StartHour, m_StartMinutes, 0);
			int num = 0;
			if (m_StartHour > m_EndHour)
			{
				num++;
			}
			TimeSpan timeSpan2 = new TimeSpan(num, m_EndHour, m_EndMinutes, 0);
			return timeSpan2 - timeSpan;
		}

		public override string ToString()
		{
			return m_BaseRoutineType.ToString() + " (" + m_StartHour + "h -> " + m_EndHour + "h)";
		}
	}

	[Serializable]
	public class PurpleDoorControlData
	{
		public int m_StartHour = 7;

		public int m_StartMinutes = 30;

		public int m_EndHour = 10;

		public int m_EndMinutes = 30;

		private int m_StartInMinutes = -1;

		private int m_EndInMinutes = -1;

		public int StartInMinutes
		{
			get
			{
				if (m_StartInMinutes == -1)
				{
					m_StartInMinutes = m_StartHour * 60 + m_StartMinutes;
				}
				return m_StartInMinutes;
			}
		}

		public int EndInMinutes
		{
			get
			{
				if (m_EndInMinutes == -1)
				{
					m_EndInMinutes = m_EndHour * 60 + m_EndMinutes;
				}
				return m_EndInMinutes;
			}
		}
	}

	public List<Routine> m_Routines = new List<Routine>();

	public Routine m_LockdownRoutine = new Routine(Routines.Lockdown, RoutineSubTypes.Lockdown, "HUD.Routine.Lockdown");

	public List<PurpleDoorControlData> m_PurpleDoorControllers = new List<PurpleDoorControlData>();

	public Events m_Ambience = Events.Play_Prison_01_Ambience_General;

	public int m_StartOfTheDayHour = 7;

	public int m_StartOfTheDayMinutes = 30;

	public int m_SunSetStartHour = 15;

	public int m_SunSetStartMinutes = 30;

	public int m_SunSetEndHour = 18;

	public int m_SunSetEndMinutes = 30;

	public int m_SunRiseStartHour = 6;

	public int m_SunRiseStartMinutes = 30;

	public int m_SunRiseEndHour = 8;

	public int m_SunRiseEndMinutes = 30;

	public int m_SpotlightsStartHour = 18;

	public int m_SpotlightsStartMinutes = 30;

	public int m_SpotlightsEndHour = 6;

	public int m_SpotlightsEndMinutes = 30;

	public int m_ItemContainerRefreshHour = 7;

	public int m_ItemContainerRefreshMinute;

	public bool m_bIsTimedPrison;

	public int m_TimedHoursDuration;

	public int m_TimedMinutesDuration;

	public RoutinesData()
	{
		m_Routines.Add(new Routine());
		m_Routines[0].m_StartHour = 0;
		m_Routines[0].m_StartMinutes = 0;
		m_Routines[0].m_EndHour = 7;
		m_Routines[0].m_EndMinutes = 0;
		m_Routines[0].m_BaseRoutineType = Routines.LightsOut;
		m_Routines[0].m_SubRoutineType = RoutineSubTypes.LightsOut;
		m_Routines.Add(new Routine());
		m_Routines[1].m_StartHour = 7;
		m_Routines[1].m_StartMinutes = 0;
		m_Routines[1].m_EndHour = 17;
		m_Routines[1].m_EndMinutes = 0;
		m_Routines[1].m_BaseRoutineType = Routines.FreeTime;
		m_Routines[1].m_SubRoutineType = RoutineSubTypes.AfternoonFreeTime;
		m_Routines.Add(new Routine());
		m_Routines[2].m_StartHour = 17;
		m_Routines[2].m_StartMinutes = 0;
		m_Routines[2].m_EndHour = 24;
		m_Routines[2].m_EndMinutes = 0;
		m_Routines[2].m_BaseRoutineType = Routines.LightsOut;
		m_Routines[2].m_SubRoutineType = RoutineSubTypes.LightsOut;
		m_PurpleDoorControllers.Add(new PurpleDoorControlData());
		m_PurpleDoorControllers[0].m_StartHour = 8;
		m_PurpleDoorControllers[0].m_StartMinutes = 0;
		m_PurpleDoorControllers[0].m_EndHour = 18;
		m_PurpleDoorControllers[0].m_EndMinutes = 0;
	}

	public Routine GetRoutine(int hour, int minutes)
	{
		for (int i = 0; i < m_Routines.Count; i++)
		{
			Routine routine = m_Routines[i];
			if (routine != null && routine.IsTimeWithinRoutine(hour, minutes))
			{
				return routine;
			}
		}
		return null;
	}

	public Routine GetLockdownRoutine()
	{
		return m_LockdownRoutine;
	}

	public bool PurpleDoorsOpen(int hour, int minutes)
	{
		for (int i = 0; i < m_PurpleDoorControllers.Count; i++)
		{
			PurpleDoorControlData purpleDoorControlData = m_PurpleDoorControllers[i];
			if (purpleDoorControlData != null && TimeHelper.IsTimeWithinRange(hour, minutes, purpleDoorControlData.m_StartHour, purpleDoorControlData.m_StartMinutes, purpleDoorControlData.m_EndHour, purpleDoorControlData.m_EndMinutes))
			{
				return true;
			}
		}
		return false;
	}

	public float GetSunriseStartInMins()
	{
		return m_SunRiseStartHour * 60 + m_SunRiseStartMinutes;
	}

	public float GetSunriseEndInMins()
	{
		return m_SunRiseEndHour * 60 + m_SunRiseEndMinutes;
	}

	public float GetSunsetStartInMins()
	{
		return m_SunSetStartHour * 60 + m_SunSetStartMinutes;
	}

	public float GetSunsetEndInMins()
	{
		return m_SunSetEndHour * 60 + m_SunSetEndMinutes;
	}

	public float GetSpotlightsStartInMins()
	{
		return m_SpotlightsStartHour * 60 + m_SpotlightsStartMinutes;
	}

	public float GetSpotlightsEndInMins()
	{
		return m_SpotlightsEndHour * 60 + m_SpotlightsEndMinutes;
	}
}
