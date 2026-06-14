using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class SetVisitorState : ActionTask<AICharacter>
{
	public BBParameter<VisitorCustomisationManager.VisitorState> m_TargetState;

	protected override void OnExecute()
	{
		VisitorCustomisationManager instance = VisitorCustomisationManager.GetInstance();
		if (instance != null)
		{
			instance.SetVisitorStateRPC(base.agent.m_Character, m_TargetState.value);
		}
		EndAction(true);
	}
}
