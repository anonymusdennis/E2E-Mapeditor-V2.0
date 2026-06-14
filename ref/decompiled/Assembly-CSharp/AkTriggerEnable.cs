public class AkTriggerEnable : AkTriggerBase
{
	private void OnEnable()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}
}
