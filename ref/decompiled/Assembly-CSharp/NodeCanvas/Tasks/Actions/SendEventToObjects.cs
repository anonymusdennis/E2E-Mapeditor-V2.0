using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Send a Graph Event to multiple gameobjects which should have a GraphOwner component attached.")]
[Category("✫ Utility")]
public class SendEventToObjects : ActionTask
{
	[RequiredField]
	public BBParameter<List<GameObject>> targetObjects;

	[RequiredField]
	public BBParameter<string> eventName;

	protected override string info => $"Send Event [{eventName}] to {targetObjects}";

	protected override void OnExecute()
	{
		foreach (GameObject item in targetObjects.value)
		{
			if (item != null)
			{
				GraphOwner component = item.GetComponent<GraphOwner>();
				if (component != null)
				{
					component.SendEvent(eventName.value);
				}
			}
		}
		EndAction();
	}
}
[Description("Send a Graph Event to multiple gameobjects which should have a GraphOwner component attached.")]
[Category("✫ Utility")]
public class SendEventToObjects<T> : ActionTask
{
	[RequiredField]
	public BBParameter<List<GameObject>> targetObjects;

	[RequiredField]
	public BBParameter<string> eventName;

	public BBParameter<T> eventValue;

	protected override string info => $"Send Event [{eventName}]({eventValue}) to {targetObjects}";

	protected override void OnExecute()
	{
		foreach (GameObject item in targetObjects.value)
		{
			GraphOwner component = item.GetComponent<GraphOwner>();
			if (component != null)
			{
				component.SendEvent(eventName.value, eventValue.value);
			}
		}
		EndAction();
	}
}
