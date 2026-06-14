using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Description("Check the Prison Alertness Level")]
[Category("★T17 Events")]
public class CheckAlertness : ConditionTask<AICharacter>
{
	private bool m_bSetActive;

	protected override string info
	{
		get
		{
			if (base.agent != null)
			{
				int activeAlertness = (int)base.agent.m_ActiveAlertness;
				int count = (int)Mathf.Floor((float)activeAlertness / 2f);
				bool flag = activeAlertness % 2 != 0;
				return "Prison Alertness " + new string('★', count) + ((!flag) ? string.Empty : "☆");
			}
			return "Prison Alertness";
		}
	}

	protected override string OnInit()
	{
		PrisonAlertnessManager.GetInstance().OnPrisonAlertnessChanged += PrisonAlertnessChanged;
		PrisonAlertnessChanged(PrisonAlertnessManager.GetInstance().GetCurrentAlertness());
		return null;
	}

	protected override bool OnCheck()
	{
		return m_bSetActive;
	}

	private void PrisonAlertnessChanged(PrisonAlertness alertness)
	{
		m_bSetActive = shouldBeActive(alertness);
	}

	private bool shouldBeActive(PrisonAlertness alertness)
	{
		if ((int)alertness >= 10)
		{
			return true;
		}
		return (int)base.agent.m_ActiveAlertness <= (int)alertness;
	}
}
