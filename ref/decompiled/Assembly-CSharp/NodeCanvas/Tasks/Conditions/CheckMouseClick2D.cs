using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[EventReceiver(new string[] { "OnMouseDown", "OnMouseUp" })]
[Category("System Events")]
[Name("Check Mouse Click 2D")]
public class CheckMouseClick2D : ConditionTask<Collider2D>
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
