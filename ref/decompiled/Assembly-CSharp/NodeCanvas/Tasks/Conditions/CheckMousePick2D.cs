using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("Input")]
public class CheckMousePick2D : ConditionTask
{
	public ButtonKeys buttonKey;

	public LayerMask mask = -1;

	[BlackboardOnly]
	public BBParameter<GameObject> saveGoAs;

	[BlackboardOnly]
	public BBParameter<float> saveDistanceAs;

	[BlackboardOnly]
	public BBParameter<Vector3> savePosAs;

	private int buttonID;

	private RaycastHit2D hit;

	protected override string info
	{
		get
		{
			string text = buttonKey.ToString() + " Click";
			if (!savePosAs.isNone)
			{
				text = text + "\nSavePos As " + savePosAs;
			}
			if (!saveGoAs.isNone)
			{
				text = text + "\nSaveGo As " + saveGoAs;
			}
			return text;
		}
	}

	protected override bool OnCheck()
	{
		buttonID = (int)buttonKey;
		if (Input.GetMouseButtonDown(buttonID))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			hit = Physics2D.Raycast(ray.origin, ray.direction, float.PositiveInfinity, mask);
			if (hit.collider != null)
			{
				savePosAs.value = hit.point;
				saveGoAs.value = hit.collider.gameObject;
				saveDistanceAs.value = hit.distance;
				return true;
			}
		}
		return false;
	}
}
