using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Blackboard/Lists")]
public class GetListCount : ActionTask
{
	[BlackboardOnly]
	[RequiredField]
	public BBParameter<IList> targetList;

	[BlackboardOnly]
	public BBParameter<int> saveAs;

	protected override string info => string.Format("{0} = {0}.Count", saveAs, targetList);

	protected override void OnExecute()
	{
		saveAs.value = targetList.value.Count;
		EndAction(true);
	}
}
