using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class EnableInputObjective : BaseObjective
{
	public int m_InputsToEnable;

	public int m_InputsToDisable;

	public bool m_bHasSetInputs;

	private const int BYTE_LENGTH = 8;

	protected override void Child_PickAllTargets()
	{
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
		if (m_PlayerOwner != null)
		{
			int num = 32;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 = 1 << i;
				if (num2 >= 65537)
				{
					break;
				}
				bool flag = (num2 & m_InputsToEnable) > 0;
				bool flag2 = (num2 & m_InputsToDisable) > 0;
				if (flag || flag2)
				{
					Player.PlayerInputs inputEnum = (Player.PlayerInputs)num2;
					if (flag)
					{
						m_PlayerOwner.SetInputEnabled(inputEnum, enabled: true);
					}
					else if (flag2)
					{
						m_PlayerOwner.SetInputEnabled(inputEnum, enabled: false);
					}
				}
			}
		}
		m_bHasSetInputs = true;
	}

	protected override bool Child_EvaluateDependencies()
	{
		return m_bHasSetInputs;
	}

	protected override bool Child_EvaluateStatus()
	{
		return Child_EvaluateDependencies();
	}

	public override int SetHUDInfo(ref ObjectiveSubGoalHUD[] infoList)
	{
		return 0;
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
			baseObj.Add(new JProperty("HasSetInputs", m_bHasSetInputs));
		}
		baseObj.Add(new JProperty("ToEnable", m_InputsToEnable));
		baseObj.Add(new JProperty("ToDisable", m_InputsToDisable));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("HasSetInputs");
			if (jProperty != null)
			{
				m_bHasSetInputs = (bool)jProperty.Value;
			}
		}
		JProperty jProperty2 = json.Property("ToEnable");
		if (jProperty2 != null)
		{
			m_InputsToEnable = (int)jProperty2.Value;
		}
		JProperty jProperty3 = json.Property("ToDisable");
		if (jProperty3 != null)
		{
			m_InputsToDisable = (int)jProperty3.Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.EnableInputObjective;
	}
}
