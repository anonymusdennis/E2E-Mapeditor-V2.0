using UnityEngine;

public class AkTriggerEnter : AkTriggerBase
{
	private void OnTriggerEnter(Collider in_other)
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
