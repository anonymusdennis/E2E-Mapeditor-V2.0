using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Remove an element from the target list")]
[Category("✫ Blackboard/Lists")]
public class RemoveElementFromList<T> : ActionTask
{
	[BlackboardOnly]
	[RequiredField]
	public BBParameter<List<T>> targetList;

	public BBParameter<T> targetElement;

	protected override string info => $"Remove {targetElement} From {targetList}";

	protected override void OnExecute()
	{
		targetList.value.Remove(targetElement.value);
		EndAction(true);
	}
}
