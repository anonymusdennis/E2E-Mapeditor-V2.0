using UnityEngine;

[RequireComponent(typeof(StonemasonDesk))]
public class StonemasonDeskDispenser : CarriedObjectDispenser
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

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (!m_StonemasonDesk.StatueDispenserEnabled())
		{
			return false;
		}
		return base.AllowedToInteract(localCharacter);
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		m_StonemasonDesk.OnStatuePickedUp();
	}
}
