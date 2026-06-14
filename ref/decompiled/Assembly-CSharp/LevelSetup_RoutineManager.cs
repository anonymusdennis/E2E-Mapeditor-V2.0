using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class LevelSetup_RoutineManager : BaseComponentSetup
{
	public RoutineManager m_RoutineManager;

	public RoutinesData m_Easy;

	public RoutinesData m_Medium;

	public RoutinesData m_Hard;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_9;
	}

	public override SetupReturnState Setup()
	{
		if (m_RoutineManager == null)
		{
			return FinishedAndRemove();
		}
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		if (m_Easy == null)
		{
			return FinishedAndRemove();
		}
		if (m_Medium == null)
		{
			return FinishedAndRemove();
		}
		if (m_Hard == null)
		{
			return FinishedAndRemove();
		}
		LevelDetailsManager instance2 = LevelDetailsManager.GetInstance();
		if (instance2 == null)
		{
			return FinishedAndRemove();
		}
		RoutinesData destination = null;
		switch (instance2.GetLevelDifficulty())
		{
		case LevelDetailsManager.DiffecultyLevel.Easy:
			destination = Object.Instantiate(m_Easy);
			break;
		case LevelDetailsManager.DiffecultyLevel.Medium:
			destination = Object.Instantiate(m_Medium);
			break;
		case LevelDetailsManager.DiffecultyLevel.Hard:
			destination = Object.Instantiate(m_Hard);
			break;
		}
		if (destination == null)
		{
			return FinishedAndRemove();
		}
		LevelScript.PRISON_ENUM musicType = instance.GetMusicType();
		List<RoutinesData.Routine> list = new List<RoutinesData.Routine>();
		list.AddRange(destination.m_Routines);
		SimplifyAndCopyRoutineData(ref destination, instance.GetRoutines(), list);
		destination.m_PurpleDoorControllers.Clear();
		int count = destination.m_Routines.Count;
		bool flag = false;
		bool flag2 = false;
		int num = -1;
		for (int i = 0; i < count; i++)
		{
			RoutinesData.Routine routine = destination.m_Routines[i];
			routine.m_RoutineMusic = GetCorrectMusic(routine.m_RoutineMusic, musicType);
			if (routine.m_BaseRoutineType != Routines.LightsOut)
			{
				if (flag && !flag2)
				{
					flag2 = true;
					destination.m_StartOfTheDayHour = routine.m_StartHour - 1;
				}
				if (routine.m_BaseRoutineType == Routines.RollCall && num == -1)
				{
					num = i;
				}
				RoutinesData.PurpleDoorControlData purpleDoorControlData = new RoutinesData.PurpleDoorControlData();
				purpleDoorControlData.m_StartHour = routine.m_StartHour;
				purpleDoorControlData.m_StartMinutes = 0;
				purpleDoorControlData.m_EndHour = routine.m_EndHour;
				purpleDoorControlData.m_EndMinutes = 0;
				destination.m_PurpleDoorControllers.Add(purpleDoorControlData);
			}
			else
			{
				flag = true;
			}
		}
		destination.m_Ambience = GetCorrectMusic(destination.m_Ambience, musicType);
		if (num == -1)
		{
			num = 0;
		}
		if (!flag2)
		{
			RoutinesData.Routine routine2 = destination.m_Routines[num];
			destination.m_StartOfTheDayHour = routine2.m_StartHour - 1;
		}
		if (destination.m_StartOfTheDayHour <= 0)
		{
			destination.m_StartOfTheDayHour = 23;
		}
		destination.m_StartOfTheDayMinutes = 45;
		m_RoutineManager.m_RoutinesData = destination;
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	private void SimplifyAndCopyRoutineData(ref RoutinesData destination, Routines[] routinesSchedule, List<RoutinesData.Routine> routineTemplates)
	{
		destination.m_Routines.Clear();
		RoutinesData.Routine routine = null;
		int num = routinesSchedule.Length;
		for (int i = 0; i < num; i++)
		{
			Routines routines = routinesSchedule[i];
			int num2 = i;
			int endHour = num2 + 1;
			RoutinesData.Routine routine2 = FindRoutineInTemplate(routines, GetRoutineSubType(routines, num2), routineTemplates);
			if (i == 0)
			{
				routine = CloneRoutineAndAddToRoutinesData(destination, routine2, num2, endHour);
				continue;
			}
			if (AreRoutineTypesIdentical(routine, routine2))
			{
				routine.m_EndHour = endHour;
				routine.m_EndMinutes = 0;
			}
			else
			{
				routine = CloneRoutineAndAddToRoutinesData(destination, routine2, num2, endHour);
			}
			if (i == num - 1)
			{
				RoutinesData.Routine routine3 = destination.m_Routines[0];
				if (AreRoutineTypesIdentical(routine, routine3) && destination.m_Routines.Count > 1)
				{
					routine3.m_StartHour = routine.m_StartHour;
					routine3.m_StartMinutes = routine.m_StartMinutes;
					destination.m_Routines.Remove(routine);
				}
			}
		}
	}

	private RoutinesData.Routine FindRoutineInTemplate(Routines baseType, RoutineSubTypes subType, List<RoutinesData.Routine> routineTemplates)
	{
		for (int num = routineTemplates.Count - 1; num >= 0; num--)
		{
			RoutinesData.Routine routine = routineTemplates[num];
			if (baseType == routine.m_BaseRoutineType && subType == routine.m_SubRoutineType)
			{
				return routine;
			}
		}
		return null;
	}

	private static RoutinesData.Routine CloneRoutineAndAddToRoutinesData(RoutinesData destination, RoutinesData.Routine routineToClone, int startHour, int endHour)
	{
		RoutinesData.Routine routine = new RoutinesData.Routine(routineToClone);
		routine.m_StartHour = startHour;
		routine.m_StartMinutes = 0;
		routine.m_EndHour = endHour;
		routine.m_EndMinutes = 0;
		routine.m_SubRoutineType = GetRoutineSubType(routine.m_BaseRoutineType, routine.m_StartHour);
		routine.m_Index = destination.m_Routines.Count;
		destination.m_Routines.Add(routine);
		return routine;
	}

	private static bool AreRoutineTypesIdentical(RoutinesData.Routine a, RoutinesData.Routine b)
	{
		return a.m_BaseRoutineType == b.m_BaseRoutineType;
	}

	private static RoutineSubTypes GetRoutineSubType(Routines routineType, int startHour)
	{
		RoutineSubTypes result = RoutineSubTypes.NoRoutine;
		switch (routineType)
		{
		case Routines.RollCall:
			result = ((startHour >= 12) ? ((startHour >= 18) ? RoutineSubTypes.EveningRollCall : RoutineSubTypes.MidDayRollCall) : RoutineSubTypes.MorningRollCall);
			break;
		case Routines.MealTime:
			result = ((startHour >= 11) ? ((startHour >= 15) ? RoutineSubTypes.DinnerTime : RoutineSubTypes.LunchTime) : RoutineSubTypes.BreakfastTime);
			break;
		case Routines.JobTime:
			result = RoutineSubTypes.JobTime;
			break;
		case Routines.FreeTime:
			result = ((startHour >= 12) ? ((startHour >= 18) ? RoutineSubTypes.EveningFreeTime : RoutineSubTypes.AfternoonFreeTime) : RoutineSubTypes.MorningFreeTime);
			break;
		case Routines.Exercise:
			result = RoutineSubTypes.ExcerciseTime;
			break;
		case Routines.ShowerTime:
			result = RoutineSubTypes.ShowerTime;
			break;
		case Routines.LightsOut:
			result = RoutineSubTypes.LightsOut;
			break;
		}
		return result;
	}

	private Events GetCorrectMusic(Events music, LevelScript.PRISON_ENUM musicType)
	{
		Events result = music;
		if (musicType != LevelScript.PRISON_ENUM.Centre_Perks)
		{
			switch (music)
			{
			case Events.Play_Music_Prison_01_Routine_A:
				result = Events.Play_Music_Prison_03_Routine_A;
				break;
			case Events.Play_Music_Prison_01_Routine_B:
				result = Events.Play_Music_Prison_03_Routine_B;
				break;
			case Events.Play_Music_Prison_01_Routine_C:
				result = Events.Play_Music_Prison_03_Routine_C;
				break;
			case Events.Play_Music_Prison_01_Routine_D:
				result = Events.Play_Music_Prison_03_Routine_D;
				break;
			case Events.Play_Music_Prison_01_Routine_E:
				result = Events.Play_Music_Prison_03_Routine_E;
				break;
			case Events.Play_Music_Prison_01_Routine_F:
				result = Events.Play_Music_Prison_03_Routine_F;
				break;
			case Events.Play_Music_Prison_01_Routine_G:
				result = Events.Play_Music_Prison_03_Routine_G;
				break;
			case Events.Play_Prison_01_Ambience_General:
				result = Events.Play_Prison_03_Ambience_General;
				break;
			}
		}
		return result;
	}
}
