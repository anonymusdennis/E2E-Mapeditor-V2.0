using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AkUnityEventHandler : MonoBehaviour
{
	public const int AWAKE_TRIGGER_ID = 1151176110;

	public const int START_TRIGGER_ID = 1281810935;

	public const int DESTROY_TRIGGER_ID = -358577003;

	public const int MAX_NB_TRIGGERS = 32;

	public static Dictionary<uint, string> triggerTypes = AkTriggerBase.GetAllDerivedTypes();

	public List<int> triggerList = new List<int> { 1281810935 };

	public bool useOtherObject;

	private bool didDestroy;

	public abstract void HandleEvent(GameObject in_gameObject);

	protected virtual void Awake()
	{
		RegisterTriggers(triggerList, HandleEvent);
		if (triggerList.Contains(1151176110))
		{
			HandleEvent(null);
		}
	}

	protected virtual void Start()
	{
		if (triggerList.Contains(1281810935))
		{
			HandleEvent(null);
		}
	}

	protected virtual void OnDestroy()
	{
		if (!didDestroy)
		{
			DoDestroy();
		}
	}

	public void DoDestroy()
	{
		UnregisterTriggers(triggerList, HandleEvent);
		if (triggerList.Contains(-358577003))
		{
			HandleEvent(null);
		}
		didDestroy = true;
	}

	protected void RegisterTriggers(List<int> in_triggerList, AkTriggerBase.Trigger in_delegate)
	{
		foreach (int in_trigger in in_triggerList)
		{
			string value = string.Empty;
			if (!triggerTypes.TryGetValue((uint)in_trigger, out value))
			{
				continue;
			}
			switch (value)
			{
			case "Awake":
			case "Start":
			case "Destroy":
				continue;
			}
			AkTriggerBase akTriggerBase = (AkTriggerBase)GetComponent(Type.GetType(value));
			if (akTriggerBase == null)
			{
				akTriggerBase = (AkTriggerBase)base.gameObject.AddComponent(Type.GetType(value));
			}
			AkTriggerBase akTriggerBase2 = akTriggerBase;
			akTriggerBase2.triggerDelegate = (AkTriggerBase.Trigger)Delegate.Combine(akTriggerBase2.triggerDelegate, in_delegate);
		}
	}

	protected void UnregisterTriggers(List<int> in_triggerList, AkTriggerBase.Trigger in_delegate)
	{
		foreach (int in_trigger in in_triggerList)
		{
			string value = string.Empty;
			if (!triggerTypes.TryGetValue((uint)in_trigger, out value))
			{
				continue;
			}
			switch (value)
			{
			case "Awake":
			case "Start":
			case "Destroy":
				continue;
			}
			AkTriggerBase akTriggerBase = (AkTriggerBase)GetComponent(Type.GetType(value));
			if (akTriggerBase != null)
			{
				akTriggerBase.triggerDelegate = (AkTriggerBase.Trigger)Delegate.Remove(akTriggerBase.triggerDelegate, in_delegate);
				if (akTriggerBase.triggerDelegate == null)
				{
					UnityEngine.Object.Destroy(akTriggerBase);
				}
			}
		}
	}
}
