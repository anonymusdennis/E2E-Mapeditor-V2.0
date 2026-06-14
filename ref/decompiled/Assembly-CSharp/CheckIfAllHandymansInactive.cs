using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if all the handyman interactions are inactive")]
[Category("★T17 Action")]
public class CheckIfAllHandymansInactive : ConditionTask<AICharacter>
{
	public BBParameter<List<InteractiveObject>> m_HandymanInteractions;

	protected override string info => "Check if handyman interactions inactive " + '\n' + "$" + m_HandymanInteractions.name;

	protected override bool OnCheck()
	{
		if (m_HandymanInteractions.value == null)
		{
			return false;
		}
		for (int num = m_HandymanInteractions.value.Count - 1; num >= 0; num--)
		{
			if (CheckIfHandymanActive.IsHandymanInteractionActive(m_HandymanInteractions.value[num]))
			{
				return false;
			}
		}
		return true;
	}
}
