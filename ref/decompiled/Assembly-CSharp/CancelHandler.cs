using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CancelHandler : MonoBehaviour, ISubmitHandler, ICancelHandler, IEventSystemHandler
{
	public EventSystem System;

	public List<ConfirmCancelEntry> m_Delegates;

	public void OnSubmit(BaseEventData data)
	{
		Debug.Log(" **********  OnSubmit  ********");
	}

	public void OnCancel(BaseEventData data)
	{
		Debug.Log(" **********  OnCancel  ********");
		if (m_Delegates == null)
		{
			return;
		}
		int count = m_Delegates.Count;
		for (int i = 0; i < count; i++)
		{
			ConfirmCancelEntry confirmCancelEntry = m_Delegates[i];
			if (confirmCancelEntry.callback != null)
			{
				confirmCancelEntry.callback.Invoke(data);
			}
		}
	}
}
