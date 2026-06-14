using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("GameObject")]
[Description("Check of agent is in line of sight with target by doing a linecast and optionaly save the distance")]
[Name("Target In Line Of Sight")]
public class CheckLOS : ConditionTask<Transform>
{
	[RequiredField]
	public BBParameter<GameObject> LOSTarget;

	public BBParameter<LayerMask> layerMask = (LayerMask)(-1);

	public Vector3 offset;

	[BlackboardOnly]
	public BBParameter<float> saveDistanceAs;

	private RaycastHit hit = default(RaycastHit);

	protected override string info => "LOS with " + LOSTarget.ToString();

	protected override bool OnCheck()
	{
		Transform transform = LOSTarget.value.transform;
		if (Physics.Linecast(base.agent.position + offset, transform.position + offset, out hit, layerMask.value))
		{
			Collider component = transform.GetComponent<Collider>();
			if (component == null || hit.collider != component)
			{
				saveDistanceAs.value = hit.distance;
				return false;
			}
		}
		return true;
	}

	public override void OnDrawGizmosSelected()
	{
		if ((bool)base.agent && (bool)LOSTarget.value)
		{
			Gizmos.DrawLine(base.agent.position + offset, LOSTarget.value.transform.position + offset);
		}
	}
}
