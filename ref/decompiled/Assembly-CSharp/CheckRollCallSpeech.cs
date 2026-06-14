using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
[Description("Conditional returns true if character is set to give speech")]
public class CheckRollCallSpeech : ConditionTask<AICharacter_Guard>
{
	protected override bool OnCheck()
	{
		return base.agent.GivesRollCallSpeech();
	}
}
