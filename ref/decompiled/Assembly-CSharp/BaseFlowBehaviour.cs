public class BaseFlowBehaviour : T17MonoBehaviour
{
	public enum FlowType
	{
		NotSet = -1,
		Boot,
		Loading,
		Frontend,
		Level,
		HUD,
		InGameMenu,
		Results
	}

	public FlowType m_FlowType = FlowType.NotSet;

	private bool m_bSignalledDone;

	private bool m_bIsHookedUp;

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void Start()
	{
		if (m_FlowType != FlowType.NotSet)
		{
			m_bIsHookedUp = GlobalStart.GetInstance().HookUpFlow(this, m_FlowType);
			if (m_bIsHookedUp)
			{
				m_bSignalledDone = false;
			}
		}
	}

	protected virtual void Update()
	{
	}

	protected virtual void OnDestroy()
	{
		if (!m_bSignalledDone)
		{
			SignalDoneWithFlow(fromDestroy: true);
		}
	}

	protected virtual void SignalDoneWithFlow(bool fromDestroy = false)
	{
		if (!m_bSignalledDone && GlobalStart.GetInstance() != null)
		{
			m_bSignalledDone = GlobalStart.GetInstance().DoneWithFlow(m_FlowType, fromDestroy);
		}
	}
}
