using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
public class CheckCrowdShowTime : ConditionTask<AICharacter_CrowdNPC>
{
	protected override bool OnCheck()
	{
		return base.agent.IsShowTime();
	}
}
