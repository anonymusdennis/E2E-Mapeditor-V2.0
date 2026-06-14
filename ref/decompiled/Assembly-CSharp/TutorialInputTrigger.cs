using UnityEngine;

public class TutorialInputTrigger : ColliderEvents
{
	private const int BYTE_LENGTH = 8;

	[HideInInspector]
	public int m_InputsToEnable;

	[HideInInspector]
	public int m_InputsToDisable;

	protected override void FireEvent(Transform colliderTransform, ColliderEvent colliderEvent)
	{
		base.FireEvent(colliderTransform, colliderEvent);
		Player player = ((!(colliderTransform != null) || !(colliderTransform.parent != null)) ? null : colliderTransform.parent.GetComponent<Player>());
		if (player != null)
		{
			switch (colliderEvent)
			{
			case ColliderEvent.OnCollisionEnter:
			case ColliderEvent.OnTriggerEnter:
				ApplyInputs(player, m_InputsToEnable, m_InputsToDisable);
				break;
			case ColliderEvent.OnCollisionExit:
			case ColliderEvent.OnTriggerExit:
				ApplyInputs(player, m_InputsToDisable, m_InputsToEnable);
				break;
			}
		}
	}

	private void ApplyInputs(Player player, int toEnable, int toDisable)
	{
		int num = 32;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 = 1 << i;
			if (num2 >= 65537)
			{
				break;
			}
			bool flag = (num2 & toEnable) > 0;
			bool flag2 = (num2 & toDisable) > 0;
			if (flag || flag2)
			{
				Player.PlayerInputs inputEnum = (Player.PlayerInputs)num2;
				if (flag)
				{
					player.SetInputEnabled(inputEnum, enabled: true);
				}
				else if (flag2)
				{
					player.SetInputEnabled(inputEnum, enabled: false);
				}
			}
		}
	}
}
