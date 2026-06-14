using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if a character has an outfit")]
[Category("★T17 Events")]
public class CheckHasOutfit : ConditionTask<AICharacter>
{
	protected override bool OnCheck()
	{
		return base.agent.HasDefaultOutfit();
	}
}
