using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class CallPIPEvent : ActionTask<Character>
{
	[BlackboardOnly]
	public BBParameter<int> m_TargetCharacterID;

	[BlackboardOnly]
	public BBParameter<int> m_PIPEventID;

	public PIPManager.PIPEventType m_PIPEventType = PIPManager.PIPEventType.GuardAlert;

	public bool m_IsGlobal;

	protected override void OnExecute()
	{
		if (m_PIPEventID.value <= 0)
		{
			m_PIPEventID.value = PIPManager.GetInstance().NewPlayerPIPEvent(m_PIPEventType, m_TargetCharacterID.value, base.agent.m_NetView.viewID, 1);
		}
		else
		{
			PIPManager.GetInstance().EndPIPEvent(m_PIPEventID.value);
			m_PIPEventID.value = PIPManager.GetInstance().NewPlayerPIPEvent(m_PIPEventType, m_TargetCharacterID.value, base.agent.m_NetView.viewID, 1);
		}
		EndAction(true);
	}
}
