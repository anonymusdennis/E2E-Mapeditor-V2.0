using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Utility")]
[Description("Sends an event to all GraphOwners within range of the agent and over time like a shockwave.")]
public class ShoutEvent : ActionTask<Transform>
{
	[RequiredField]
	public BBParameter<string> eventName;

	public BBParameter<float> shoutRange = 10f;

	public BBParameter<float> completionTime = 1f;

	private GraphOwner[] owners;

	private List<GraphOwner> receivedOwners = new List<GraphOwner>();

	private float traveledDistance;

	protected override string info => $"Shout Event [{eventName.ToString()}]";

	protected override void OnExecute()
	{
		owners = Object.FindObjectsOfType<GraphOwner>();
		receivedOwners.Clear();
	}

	protected override void OnUpdate()
	{
		traveledDistance = Mathf.Lerp(0f, shoutRange.value, base.elapsedTime / completionTime.value);
		GraphOwner[] array = owners;
		foreach (GraphOwner graphOwner in array)
		{
			float magnitude = (base.agent.position - graphOwner.transform.position).magnitude;
			if (magnitude <= traveledDistance && !receivedOwners.Contains(graphOwner))
			{
				graphOwner.SendEvent(eventName.value);
				receivedOwners.Add(graphOwner);
			}
		}
		if (base.elapsedTime >= completionTime.value)
		{
			EndAction();
		}
	}

	public override void OnDrawGizmosSelected()
	{
		if (base.agent != null)
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
			Gizmos.DrawWireSphere(base.agent.position, traveledDistance);
			Gizmos.DrawWireSphere(base.agent.position, shoutRange.value);
		}
	}
}
