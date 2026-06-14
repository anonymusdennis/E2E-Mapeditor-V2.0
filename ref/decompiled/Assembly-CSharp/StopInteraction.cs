using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class StopInteraction : ActionTask<AICharacter>
{
	private float m_fGiveUpTime = 3f;

	private float m_fGiveUpTimer;

	protected override void OnExecute()
	{
		if (!base.agent.m_Character.IsInteracting())
		{
			EndAction(true);
			return;
		}
		m_fGiveUpTimer = 0f;
		base.agent.m_Character.RequestStopInteraction();
	}

	protected override void OnStop()
	{
	}

	protected override void OnUpdate()
	{
		if (base.agent.m_Character.IsInteracting())
		{
			if (m_fGiveUpTimer < m_fGiveUpTime)
			{
				m_fGiveUpTimer += BehaviourTree.CurrentTimeSlicedDeltaTime;
				if (m_fGiveUpTimer >= m_fGiveUpTime)
				{
					EndAction(false);
				}
			}
		}
		else
		{
			EndAction(true);
		}
	}
}
