using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class SetRoutineObjective : BaseObjective
{
	public enum Mode
	{
		Routine,
		Lockdown
	}

	public Mode m_Mode;

	public Routines m_BaseRoutine = Routines.UNASSIGNED;

	public RoutineSubTypes m_RoutineSubtype;

	public bool m_bFreezeTime;

	public bool m_bLockdown;

	public bool m_bSendPlayerToSolitary;

	public bool m_bSetPrisonAlertness;

	public PrisonAlertness m_PrisonAlertness;

	protected override void Child_PickAllTargets()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance == null)
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
		if (m_Mode == Mode.Routine)
		{
			int hours = 0;
			int mins = 0;
			bool nextDay = false;
			if (!instance.FindTimeOfNextRoutineType(m_BaseRoutine, m_RoutineSubtype, out hours, out mins, out nextDay))
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		if (m_Mode == Mode.Routine)
		{
			int hours = 0;
			int mins = 0;
			bool nextDay = false;
			if (instance.FindTimeOfNextRoutineType(m_BaseRoutine, m_RoutineSubtype, out hours, out mins, out nextDay))
			{
				instance.SetTime(hours, mins, nextDay);
			}
			else
			{
				m_ObjectiveStatus = ObjectiveStatus.Invalid;
			}
		}
		if (m_Mode == Mode.Lockdown)
		{
			SolitaryManager instance2 = SolitaryManager.GetInstance();
			if (instance2 != null)
			{
				if (m_bSendPlayerToSolitary)
				{
					instance2.SetWantedForSolitary_Quest(m_PlayerOwner, m_bLockdown);
				}
				instance2.SetLockdownActive_Quest(m_bLockdown);
			}
			else
			{
				instance.SetLockdownRoutine(m_bLockdown);
			}
		}
		instance.SetTimeFrozenRPC(m_bFreezeTime);
		if (m_bSetPrisonAlertness)
		{
			PrisonAlertnessManager instance3 = PrisonAlertnessManager.GetInstance();
			if (instance3 != null)
			{
				instance3.SetAlertness_Quest(m_PrisonAlertness);
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			if (m_Mode == Mode.Lockdown)
			{
				SolitaryManager instance2 = SolitaryManager.GetInstance();
				if (instance2 != null)
				{
					if (instance2.IsLockdownActive() != m_bLockdown)
					{
						return false;
					}
					if (m_bSendPlayerToSolitary && !instance2.IsWantedForSolitary(m_PlayerOwner))
					{
						return false;
					}
				}
				else if (instance.IsLockdownActive() != m_bLockdown)
				{
					return false;
				}
			}
			else
			{
				RoutinesData.Routine currentRoutine = instance.GetCurrentRoutine();
				if (currentRoutine == null || currentRoutine.m_BaseRoutineType != m_BaseRoutine || currentRoutine.m_SubRoutineType != m_RoutineSubtype)
				{
					return false;
				}
			}
			if (instance.IsTimeFrozen() != m_bFreezeTime)
			{
				return false;
			}
			if (m_bSetPrisonAlertness)
			{
				PrisonAlertnessManager instance3 = PrisonAlertnessManager.GetInstance();
				if (instance3 != null && instance3.GetCurrentAlertness() != m_PrisonAlertness)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override bool Child_EvaluateStatus()
	{
		return Child_EvaluateDependencies();
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override void Child_PostAction()
	{
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
		}
		baseObj.Add(new JProperty("Mode", (int)m_Mode));
		baseObj.Add(new JProperty("Routine", (int)m_BaseRoutine));
		baseObj.Add(new JProperty("RoutineSubtype", (int)m_RoutineSubtype));
		baseObj.Add(new JProperty("FreezeTime", m_bFreezeTime));
		baseObj.Add(new JProperty("Lockdown", m_bLockdown));
		baseObj.Add(new JProperty("SendToSolitary", m_bSendPlayerToSolitary));
		baseObj.Add(new JProperty("SetPrisonAlertness", m_bSetPrisonAlertness));
		baseObj.Add(new JProperty("PrisonAlertness", (int)m_PrisonAlertness));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
		}
		JProperty jProperty = json.Property("Mode");
		if (jProperty != null)
		{
			int mode = (int)jProperty.Value;
			m_Mode = (Mode)mode;
		}
		JProperty jProperty2 = json.Property("Routine");
		if (jProperty2 != null)
		{
			int baseRoutine = (int)jProperty2.Value;
			m_BaseRoutine = (Routines)baseRoutine;
		}
		JProperty jProperty3 = json.Property("RoutineSubtype");
		if (jProperty3 != null)
		{
			int num = (int)jProperty3.Value;
			m_RoutineSubtype = (RoutineSubTypes)num;
		}
		JProperty jProperty4 = json.Property("FreezeTime");
		if (jProperty4 != null)
		{
			m_bFreezeTime = (bool)jProperty4.Value;
		}
		JProperty jProperty5 = json.Property("Lockdown");
		if (jProperty5 != null)
		{
			m_bLockdown = (bool)jProperty5.Value;
		}
		JProperty jProperty6 = json.Property("SendToSolitary");
		if (jProperty6 != null)
		{
			m_bSendPlayerToSolitary = (bool)jProperty6.Value;
		}
		JProperty jProperty7 = json.Property("SetPrisonAlertness");
		if (jProperty7 != null)
		{
			m_bSetPrisonAlertness = (bool)jProperty7.Value;
		}
		JProperty jProperty8 = json.Property("PrisonAlertness");
		if (jProperty8 != null)
		{
			int num2 = (int)jProperty8.Value;
			m_PrisonAlertness = (PrisonAlertness)num2;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.SetRoutineObjective;
	}
}
