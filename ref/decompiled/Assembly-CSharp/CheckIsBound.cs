using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if a character is bound")]
[Category("★T17 Events")]
public class CheckIsBound : ConditionTask<Character>
{
	protected override bool OnCheck()
	{
		return base.agent.m_bIsBound;
	}
}
