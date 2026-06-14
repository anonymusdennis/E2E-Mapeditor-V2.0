using UnityEngine;

public class LevelEditor_RoutineEntry : MonoBehaviour
{
	public int m_TimeSlot;

	public T17Text m_RoutineName;

	public Color m_MissingRoom = Color.red;

	public Color m_EverythingOK = Color.white;

	private string m_strRoutineName = string.Empty;

	private LevelDetailsManager m_DetailsManager;

	private void Start()
	{
	}

	private void OnEnable()
	{
		if (m_RoutineName == null)
		{
			m_RoutineName = GetComponentInChildren<T17Text>();
		}
		if (m_DetailsManager == null)
		{
			m_DetailsManager = LevelDetailsManager.GetInstance();
		}
		SetCurrentRoutine(GetCurrentRoutine());
		UpdateRoutine();
	}

	public void NextRoutine()
	{
		SetCurrentRoutine(GetCurrentRoutine() switch
		{
			Routines.RollCall => Routines.MealTime, 
			Routines.MealTime => Routines.JobTime, 
			Routines.JobTime => Routines.FreeTime, 
			Routines.FreeTime => Routines.Exercise, 
			Routines.Exercise => Routines.ShowerTime, 
			Routines.ShowerTime => Routines.LightsOut, 
			Routines.LightsOut => Routines.RollCall, 
			_ => Routines.RollCall, 
		});
	}

	public void PreviousRoutine()
	{
		SetCurrentRoutine(GetCurrentRoutine() switch
		{
			Routines.RollCall => Routines.LightsOut, 
			Routines.MealTime => Routines.RollCall, 
			Routines.JobTime => Routines.MealTime, 
			Routines.FreeTime => Routines.JobTime, 
			Routines.Exercise => Routines.FreeTime, 
			Routines.ShowerTime => Routines.Exercise, 
			Routines.LightsOut => Routines.ShowerTime, 
			_ => Routines.LightsOut, 
		});
	}

	private Routines GetCurrentRoutine()
	{
		if (m_DetailsManager != null)
		{
			return m_DetailsManager.GetRoutineAtTime(m_TimeSlot);
		}
		return Routines.UNASSIGNED;
	}

	private void SetCurrentRoutine(Routines newRoutine)
	{
		if (m_DetailsManager != null)
		{
			switch (newRoutine)
			{
			case Routines.RollCall:
				m_strRoutineName = "HUD.Routine.RollCall";
				break;
			case Routines.MealTime:
				m_strRoutineName = "Text.LE.Rout.H.MealTime";
				break;
			case Routines.JobTime:
				m_strRoutineName = "HUD.Routine.Job";
				break;
			case Routines.FreeTime:
				m_strRoutineName = "HUD.Routine.FreeTime";
				break;
			case Routines.Exercise:
				m_strRoutineName = "HUD.Routine.Exercise";
				break;
			case Routines.ShowerTime:
				m_strRoutineName = "HUD.Routine.Shower";
				break;
			case Routines.LightsOut:
				m_strRoutineName = "HUD.Routine.LightsOut";
				break;
			default:
				m_strRoutineName = "UNKNOWN";
				break;
			}
			m_DetailsManager.SetRoutineAtTime(m_TimeSlot, newRoutine);
			UpdateRoutine();
		}
	}

	private void UpdateRoutine()
	{
		if (!(m_RoutineName != null))
		{
			return;
		}
		m_RoutineName.SetNewLocalizationTag(m_strRoutineName);
		if (BuildingBlockManager.GetInstance() != null)
		{
			if (BuildingBlockManager.GetInstance().AreRoutineRequirementsMet(GetCurrentRoutine()))
			{
				m_RoutineName.color = m_EverythingOK;
			}
			else
			{
				m_RoutineName.color = m_MissingRoom;
			}
		}
	}
}
