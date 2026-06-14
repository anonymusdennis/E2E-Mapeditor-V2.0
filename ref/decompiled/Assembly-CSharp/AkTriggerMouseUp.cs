public class AkTriggerMouseUp : AkTriggerBase
{
	private void OnMouseUp()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}
}
