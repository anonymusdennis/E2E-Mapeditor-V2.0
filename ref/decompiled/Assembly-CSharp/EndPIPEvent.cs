using NodeCanvas.Framework;

public class EndPIPEvent : ActionTask<Character>
{
	[BlackboardOnly]
	public BBParameter<int> m_PIPEventID;

	protected override void OnExecute()
	{
		if (m_PIPEventID.value > 0)
		{
			PIPManager.GetInstance().EndPIPEvent(m_PIPEventID.value);
			m_PIPEventID.value = -1;
		}
		EndAction(true);
	}
}
