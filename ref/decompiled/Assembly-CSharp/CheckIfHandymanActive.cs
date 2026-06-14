using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if an the given handyman interaction is active")]
[Category("★T17 Action")]
public class CheckIfHandymanActive : ConditionTask<AICharacter>
{
	public BBParameter<InteractiveObject> m_interactionObjTarget;

	protected override string info
	{
		get
		{
			string empty = string.Empty;
			return string.Concat(arg2: (!(m_interactionObjTarget.value != null)) ? (empty + "$" + m_interactionObjTarget.name) : m_interactionObjTarget.value.name, arg0: "Check if handyman active ", arg1: '\n');
		}
	}

	protected override bool OnCheck()
	{
		if (m_interactionObjTarget.value == null)
		{
			return false;
		}
		return IsHandymanInteractionActive(m_interactionObjTarget.value);
	}

	public static bool IsHandymanInteractionActive(InteractiveObject interaction)
	{
		HandymanInteraction handymanInteraction = interaction as HandymanInteraction;
		if (handymanInteraction == null)
		{
			return false;
		}
		return handymanInteraction.NeedsJobTimeFixing();
	}
}
