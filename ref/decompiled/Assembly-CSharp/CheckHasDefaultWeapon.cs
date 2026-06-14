using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
[Description("Check if a guard has it's default weapon")]
public class CheckHasDefaultWeapon : ConditionTask<AICharacter_Guard>
{
	protected override bool OnCheck()
	{
		return base.agent.HasDefaultWeapon();
	}
}
