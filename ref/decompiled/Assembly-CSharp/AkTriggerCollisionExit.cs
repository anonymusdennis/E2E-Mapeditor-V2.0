using UnityEngine;

public class AkTriggerCollisionExit : AkTriggerBase
{
	private void OnCollisionExit(Collision in_other)
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
