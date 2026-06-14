public class AkTriggerMouseDown : AkTriggerBase
{
	private void OnMouseDown()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}
}
