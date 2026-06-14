using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class SetAnimationState : ActionTask<Character>
{
	public BBParameter<AnimState> m_AnimState;

	public BBParameter<float> m_AnimLength;

	private float m_AnimTime;

	private bool m_bAnimRunning;

	protected override void OnExecute()
	{
		base.agent.m_CharacterAnimator.StartAnimation(m_AnimState.value);
		m_bAnimRunning = true;
		m_AnimTime = m_AnimLength.value;
		base.agent.PauseMovement(m_AnimTime);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		m_AnimTime -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (m_AnimTime < 0f)
		{
			m_bAnimRunning = false;
			base.agent.m_CharacterAnimator.StopAnimation(m_AnimState.value);
			EndAction(true);
		}
	}

	protected override void OnStop()
	{
		if (m_bAnimRunning)
		{
			base.agent.m_CharacterAnimator.StopAnimation(m_AnimState.value);
			m_bAnimRunning = false;
		}
		base.OnStop();
	}
}
