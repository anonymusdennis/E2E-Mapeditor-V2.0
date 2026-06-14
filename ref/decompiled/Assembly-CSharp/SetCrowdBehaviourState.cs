using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class SetCrowdBehaviourState : ActionTask<AICharacter_CrowdNPC>
{
	public bool m_bActiveState;

	protected override void OnExecute()
	{
		base.agent.SetBehaviourIsActive(m_bActiveState);
		EndAction(true);
	}
}
