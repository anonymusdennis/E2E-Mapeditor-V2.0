using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if a character has an outfit")]
[Category("★T17 Events")]
public class CheckCombatEnded : ConditionTask<AICharacter>
{
	protected override bool OnCheck()
	{
		return base.agent.CombatEnded();
	}
}
