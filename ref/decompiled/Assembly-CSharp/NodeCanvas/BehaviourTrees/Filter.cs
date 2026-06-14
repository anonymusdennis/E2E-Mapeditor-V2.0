using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Filters the access of it's child node either a specific number of times, or every specific amount of time. By default the node is 'Treated as Inactive' to it's parent when child is Filtered. Unchecking this option will instead return Failure when Filtered.")]
[Icon("Lock", false)]
[Name("Filter")]
[Category("Decorators")]
public class Filter : BTDecorator
{
	public enum FilterMode
	{
		LimitNumberOfTimes,
		CoolDown
	}

	public FilterMode filterMode = FilterMode.CoolDown;

	public BBParameter<int> maxCount = new BBParameter<int>
	{
		value = 1
	};

	public BBParameter<float> coolDownTime = new BBParameter<float>
	{
		value = 5f
	};

	public bool inactiveWhenLimited = true;

	private int executedCount;

	private float currentTime;

	public override void OnGraphStarted()
	{
		executedCount = 0;
		currentTime = 0f;
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		switch (filterMode)
		{
		case FilterMode.CoolDown:
			if (currentTime > 0f)
			{
				return inactiveWhenLimited ? Status.Resting : Status.Failure;
			}
			base.status = base.decoratedConnection.Execute(agent, blackboard);
			if (base.status == Status.Success || base.status == Status.Failure)
			{
				StartCoroutine(Cooldown());
			}
			break;
		case FilterMode.LimitNumberOfTimes:
			if (executedCount >= maxCount.value)
			{
				return inactiveWhenLimited ? Status.Resting : Status.Failure;
			}
			base.status = base.decoratedConnection.Execute(agent, blackboard);
			if (base.status == Status.Success || base.status == Status.Failure)
			{
				executedCount++;
			}
			break;
		}
		return base.status;
	}

	private IEnumerator Cooldown()
	{
		for (currentTime = coolDownTime.value; currentTime > 0f; currentTime -= UpdateManager.deltaTime)
		{
			yield return null;
		}
	}
}
