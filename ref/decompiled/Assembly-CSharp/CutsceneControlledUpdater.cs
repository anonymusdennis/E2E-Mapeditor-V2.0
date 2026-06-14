public class CutsceneControlledUpdater : T17MonoBehaviour
{
	private IControlledUpdate[] m_ControlledUpdates;

	protected override void Awake()
	{
		base.Awake();
		m_ControlledUpdates = GetComponentsInChildren<IControlledUpdate>(includeInactive: true);
	}

	protected virtual void OnDestroy()
	{
		for (int i = 0; i < m_ControlledUpdates.Length; i++)
		{
			m_ControlledUpdates[i] = null;
		}
		m_ControlledUpdates = null;
	}

	private void Update()
	{
		for (int i = 0; i < m_ControlledUpdates.Length; i++)
		{
			m_ControlledUpdates[i].ControlledUpdate();
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < m_ControlledUpdates.Length; i++)
		{
			m_ControlledUpdates[i].ControlledFixedUpdate();
		}
	}
}
