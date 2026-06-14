public class AkTriggerDisable : AkTriggerBase
{
	private void OnDisable()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}
}
