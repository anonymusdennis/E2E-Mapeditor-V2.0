using UnityEngine;

public class AkTriggerCollisionEnter : AkTriggerBase
{
	private void OnCollisionEnter(Collision in_other)
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
