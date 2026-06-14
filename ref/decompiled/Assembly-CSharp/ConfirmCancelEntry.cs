using System;
using UnityEngine.EventSystems;

[Serializable]
public class ConfirmCancelEntry
{
	public EventTrigger.TriggerEvent callback = new EventTrigger.TriggerEvent();
}
