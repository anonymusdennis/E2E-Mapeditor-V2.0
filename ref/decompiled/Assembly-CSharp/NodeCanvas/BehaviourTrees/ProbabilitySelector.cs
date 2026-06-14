using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("ProbabilitySelector", false)]
[Color("b3ff7f")]
[Description("Select a child to execute based on it's chance to be selected and return Success if it returns Success, otherwise pick another.\nReturns Failure if no child returns Success or a direct 'Failure Chance' is introduced")]
[Category("Composites")]
public class ProbabilitySelector : BTComposite
{
	public List<BBParameter<float>> childWeights = new List<BBParameter<float>>();

	public BBParameter<float> failChance = new BBParameter<float>();

	private float probability;

	private float currentProbability;

	private List<int> failedIndeces = new List<int>();

	public override string name => base.name.ToUpper();

	public override void OnChildConnected(int index)
	{
		childWeights.Insert(index, new BBParameter<float>
		{
			value = 1f,
			bb = base.graphBlackboard
		});
	}

	public override void OnChildDisconnected(int index)
	{
		childWeights.RemoveAt(index);
	}

	public override void OnGraphStarted()
	{
		OnReset();
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		currentProbability = probability;
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			if (failedIndeces.Contains(i))
			{
				continue;
			}
			if (currentProbability > childWeights[i].value)
			{
				currentProbability -= childWeights[i].value;
				continue;
			}
			base.status = base.outConnections[i].Execute(agent, blackboard);
			if (base.status == Status.Success || base.status == Status.Running)
			{
				return base.status;
			}
			if (base.status == Status.Failure)
			{
				failedIndeces.Add(i);
				float num = GetTotal();
				for (int j = 0; j < failedIndeces.Count; j++)
				{
					num -= childWeights[j].value;
				}
				probability = Random.Range(0f, num);
				return Status.Running;
			}
		}
		return Status.Failure;
	}

	protected override void OnReset()
	{
		failedIndeces.Clear();
		probability = Random.Range(0f, GetTotal());
	}

	private float GetTotal()
	{
		float num = failChance.value;
		foreach (BBParameter<float> childWeight in childWeights)
		{
			num += childWeight.value;
		}
		return num;
	}
}
