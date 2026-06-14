using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class EventCheck : ActionTask<AICharacter>
{
	public BBParameter<AIEventMemory> m_Event;

	protected override void OnUpdate()
	{
		if (!m_Event.value.m_bEventValid)
		{
			base.agent.ForgetEvent(m_Event.value);
		}
		EndAction(m_Event.value.m_bEventValid);
	}
}
