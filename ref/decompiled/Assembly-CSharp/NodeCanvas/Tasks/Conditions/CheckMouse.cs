using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[EventReceiver(new string[] { "OnMouseEnter", "OnMouseExit", "OnMouseOver" })]
[Category("System Events")]
public class CheckMouse : ConditionTask<Collider>
{
	public MouseInteractionTypes checkType;

	protected override string info => checkType.ToString();

	protected override bool OnCheck()
	{
		return false;
	}

	public void OnMouseEnter()
	{
		if (checkType == MouseInteractionTypes.MouseEnter)
		{
			YieldReturn(value: true);
		}
	}

	public void OnMouseExit()
	{
		if (checkType == MouseInteractionTypes.MouseExit)
		{
			YieldReturn(value: true);
		}
	}

	public void OnMouseOver()
	{
		if (checkType == MouseInteractionTypes.MouseOver)
		{
			YieldReturn(value: true);
		}
	}
}
