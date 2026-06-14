using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
[Description("Check if a character has a tray")]
public class CheckHasTray : ConditionTask<Character>
{
	protected override bool OnCheck()
	{
		return base.agent.GetHasTray();
	}
}
