using UnityEngine;

public class AkTriggerExit : AkTriggerBase
{
	private void OnTriggerExit(Collider in_other)
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(in_other.gameObject);
		}
	}
}
