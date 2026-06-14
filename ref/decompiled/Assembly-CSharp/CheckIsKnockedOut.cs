using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if a character is knocked out")]
[Category("★T17 Events")]
public class CheckIsKnockedOut : ConditionTask<Character>
{
	protected override bool OnCheck()
	{
		return base.agent.GetIsKnockedOut();
	}
}
