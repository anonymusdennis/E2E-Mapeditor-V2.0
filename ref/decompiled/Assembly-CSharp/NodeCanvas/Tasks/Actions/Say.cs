using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("You can use a variable inline with the text by using brackets likeso: [myVarName] or [Global/myVarName].\nThe bracket will be replaced with the variable value ToString")]
[Category("Dialogue")]
[Icon("Dialogue", false)]
[AgentType(typeof(IDialogueActor))]
public class Say : ActionTask
{
	public Statement statement = new Statement("This is a dialogue text...");

	protected override string info => string.Format("<i>' {0} '</i>", (statement.text.Length <= 30) ? statement.text : (statement.text.Substring(0, 30) + "..."));

	protected override void OnExecute()
	{
		Statement statement = this.statement.BlackboardReplace(base.blackboard);
		DialogueTree.RequestSubtitles(new SubtitlesRequestInfo((IDialogueActor)base.agent, statement, base.EndAction));
	}
}
