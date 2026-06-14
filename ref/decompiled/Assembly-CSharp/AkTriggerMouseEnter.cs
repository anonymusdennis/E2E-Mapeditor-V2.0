public class AkTriggerMouseEnter : AkTriggerBase
{
	private void OnMouseEnter()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}
}
