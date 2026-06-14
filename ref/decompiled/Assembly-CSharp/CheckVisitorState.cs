using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
[Description("Check the current state of a Visitor (via the VisitorManager)")]
public class CheckVisitorState : ConditionTask<AICharacter>
{
	public BBParameter<VisitorCustomisationManager.VisitorState> m_State;

	protected override string info => "Check Visitor State: " + m_State.value;

	protected override bool OnCheck()
	{
		if (base.agent.m_Character != null)
		{
			VisitorCustomisationManager instance = VisitorCustomisationManager.GetInstance();
			if (instance != null)
			{
				VisitorCustomisationManager.VisitorState visitorState = instance.GetVisitorState(base.agent.m_Character);
				return visitorState == m_State.value;
			}
		}
		return false;
	}
}
