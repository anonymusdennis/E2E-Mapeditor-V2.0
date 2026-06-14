using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if a guard has reportable events")]
[Category("★T17 Events")]
public class HaveEventsToReport : ConditionTask<AICharacter_Guard>
{
	[BlackboardOnly]
	public BBParameter<CharacterIconHandler.IconType> m_IconToShow;

	protected override bool OnCheck()
	{
		bool havePlayerResponsible = false;
		bool result = base.agent.HaveEventsToReport(out havePlayerResponsible);
		m_IconToShow.value = ((!havePlayerResponsible) ? CharacterIconHandler.IconType.GuardAlert : CharacterIconHandler.IconType.GuardReport);
		return result;
	}
}
