using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if an event is still valid")]
[Category("★T17 Events")]
public class EventValidCheck : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<AIEventMemory> saveEventValue;

	protected override bool OnCheck()
	{
		if (saveEventValue.value == null)
		{
			return false;
		}
		return saveEventValue.value.m_bEventValid;
	}
}
