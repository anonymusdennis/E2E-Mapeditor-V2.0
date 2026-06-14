using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("GameObject")]
[Description("Check of agent is in line of sight with target by doing a linecast and optionaly save the distance")]
[Name("Target In Line Of Sight 2D")]
public class CheckLOS2D : ConditionTask<Transform>
{
	[RequiredField]
	public BBParameter<GameObject> LOSTarget;

	public BBParameter<LayerMask> layerMask = (LayerMask)(-1);

	[BlackboardOnly]
	public BBParameter<float> saveDistanceAs;

	[GetFromAgent]
	protected Collider2D agentCollider;

	private RaycastHit2D[] hits;

	protected override string info => "LOS with " + LOSTarget.ToString();

	protected override bool OnCheck()
	{
		hits = Physics2D.LinecastAll(base.agent.position, LOSTarget.value.transform.position, layerMask.value);
		foreach (Collider2D item in hits.Select((RaycastHit2D h) => h.collider))
		{
			if (item != agentCollider && item != LOSTarget.value.GetComponent<Collider2D>())
			{
				return false;
			}
		}
		return true;
	}

	public override void OnDrawGizmosSelected()
	{
		if ((bool)base.agent && (bool)LOSTarget.value)
		{
			Gizmos.DrawLine(base.agent.position, LOSTarget.value.transform.position);
		}
	}
}
