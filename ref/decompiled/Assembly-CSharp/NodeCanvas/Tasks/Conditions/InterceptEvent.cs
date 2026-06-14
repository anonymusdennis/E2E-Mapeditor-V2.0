using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NodeCanvas.Tasks.Conditions;

[Description("Returns true when the selected event is triggered on the selected agent.\nYou can use this for both GUI and 3D objects.\nPlease make sure that Unity Event Systems are setup correctly")]
[Category("UGUI")]
public class InterceptEvent : ConditionTask<Transform>
{
	public EventTriggerType eventType;

	protected override string info => $"{eventType.ToString()} on {base.agentInfo}";

	protected override string OnInit()
	{
		RegisterEvent("On" + eventType);
		return null;
	}

	protected override bool OnCheck()
	{
		return false;
	}

	private void OnPointerEnter(PointerEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnPointerExit(PointerEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnPointerDown(PointerEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnPointerUp(PointerEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnPointerClick(PointerEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnDrag(PointerEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnDrop(BaseEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnScroll(PointerEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnUpdateSelected(BaseEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnSelect(BaseEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnDeselect(BaseEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnMove(AxisEventData eventData)
	{
		YieldReturn(value: true);
	}

	private void OnSubmit(BaseEventData eventData)
	{
		YieldReturn(value: true);
	}
}
