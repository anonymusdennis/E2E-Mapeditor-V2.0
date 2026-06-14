public class AkTriggerMouseExit : AkTriggerBase
{
	private void OnMouseExit()
	{
		if (triggerDelegate != null)
		{
			triggerDelegate(null);
		}
	}
}
