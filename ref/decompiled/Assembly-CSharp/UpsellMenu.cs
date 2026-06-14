using UnityEngine;

public class UpsellMenu : FrontendMenuBehaviour
{
	public float m_ScreenShowTime = 5f;

	private float m_CloseTime = -1f;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		m_CloseTime = Time.time + m_ScreenShowTime;
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (T17NetManager.IsMasterClient && m_CloseTime > 0f && Time.time > m_CloseTime)
		{
			m_CloseTime = -1f;
			ResultsFlow instance = ResultsFlow.Instance;
			if (instance != null)
			{
				instance.SetExitRequested();
			}
		}
	}
}
