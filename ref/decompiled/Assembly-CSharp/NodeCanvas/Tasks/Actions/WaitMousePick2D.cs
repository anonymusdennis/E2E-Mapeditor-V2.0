using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Input")]
public class WaitMousePick2D : ActionTask
{
	public enum ButtonKeys
	{
		Left,
		Right,
		Middle
	}

	public ButtonKeys buttonKey;

	public LayerMask mask = -1;

	[BlackboardOnly]
	public BBParameter<GameObject> saveObjectAs;

	[BlackboardOnly]
	public BBParameter<float> saveDistanceAs;

	[BlackboardOnly]
	public BBParameter<Vector3> savePositionAs;

	private int buttonID;

	private RaycastHit2D hit;

	protected override string info => $"Wait Object '{buttonKey}' Click. Save As {saveObjectAs}";

	protected override void OnUpdate()
	{
		buttonID = (int)buttonKey;
		if (Input.GetMouseButtonDown(buttonID))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			hit = Physics2D.Raycast(ray.origin, ray.direction, float.PositiveInfinity, mask);
			if (hit.collider != null)
			{
				savePositionAs.value = hit.point;
				saveObjectAs.value = hit.collider.gameObject;
				saveDistanceAs.value = hit.distance;
				EndAction(true);
			}
		}
	}
}
