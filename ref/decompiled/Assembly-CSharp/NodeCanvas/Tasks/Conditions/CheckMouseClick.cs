using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("System Events")]
[EventReceiver(new string[] { "OnMouseDown", "OnMouseUp" })]
public class CheckMouseClick : ConditionTask<Collider>
{
	public MouseClickEvent checkType;

	protected override string info => checkType.ToString();

	protected override bool OnCheck()
	{
		return false;
	}

	public void OnMouseDown()
	{
		if (checkType == MouseClickEvent.MouseDown)
		{
			YieldReturn(value: true);
		}
	}

	public void OnMouseUp()
	{
		if (checkType == MouseClickEvent.MouseUp)
		{
			YieldReturn(value: true);
		}
	}
}
