using UnityEngine;

[RequireComponent(typeof(StonemasonDesk))]
public class StonemasonDeskConsumer : CarryableObjectConsumer
{
	public StonemasonDesk m_StonemasonDesk;

	protected override void Awake()
	{
		base.Awake();
		if (m_StonemasonDesk == null)
		{
			m_StonemasonDesk = GetComponent<StonemasonDesk>();
		}
	}

	protected override void OnInputDroppedOnUs(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
		base.OnInputDroppedOnUs(consumer, theObject);
		m_StonemasonDesk.OnStoneConsumed();
	}

	public override bool WillAcceptInput(CarryObjectInteraction theObject)
	{
		if (!m_StonemasonDesk.StoneConsumerEnabled())
		{
			return false;
		}
		return base.WillAcceptInput(theObject);
	}
}
