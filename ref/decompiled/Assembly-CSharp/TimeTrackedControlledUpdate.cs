public class TimeTrackedControlledUpdate
{
	public float m_fPreviousDeltaTime;

	public float m_fPreviousFixedDeltaTime;

	public IControlledUpdate m_Behaviour;

	public TimeTrackedControlledUpdate(IControlledUpdate behaviour)
	{
		m_fPreviousDeltaTime = UpdateManager.deltaTimeSinceStart;
		m_fPreviousFixedDeltaTime = UpdateManager.fixedDeltaTimeSinceStart;
		m_Behaviour = behaviour;
	}
}
