using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check to see if an AI Inmate is allowed to get changed.")]
[Category("★T17 Events")]
public class CheckAllowedToGetChanged : ConditionTask<AICharacter_Inmate>
{
	protected override bool OnCheck()
	{
		return base.agent.AllowedToGetChanged();
	}
}
