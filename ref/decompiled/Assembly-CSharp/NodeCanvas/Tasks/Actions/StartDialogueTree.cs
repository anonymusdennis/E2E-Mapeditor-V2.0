using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Starts the Dialogue Tree assigned on a Dialogue Tree Controller object with specified agent used for 'Instigator'.")]
[Icon("Dialogue", false)]
[AgentType(typeof(IDialogueActor))]
[Category("Dialogue")]
public class StartDialogueTree : ActionTask
{
	[RequiredField]
	public BBParameter<DialogueTreeController> dialogueTreeController;

	public bool waitActionFinish = true;

	protected override string info => $"Start Dialogue {dialogueTreeController}";

	protected override void OnExecute()
	{
		IDialogueActor instigator = (IDialogueActor)base.agent;
		if (waitActionFinish)
		{
			dialogueTreeController.value.StartDialogue(instigator, delegate(bool success)
			{
				EndAction(success);
			});
		}
		else
		{
			dialogueTreeController.value.StartDialogue(instigator);
			EndAction();
		}
	}
}
