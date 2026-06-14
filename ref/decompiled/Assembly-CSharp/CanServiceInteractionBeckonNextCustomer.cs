using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class CanServiceInteractionBeckonNextCustomer : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<InteractiveObject> m_ServiceInteraction;

	protected override string info
	{
		get
		{
			string text = "UNASSIGNED";
			if (m_ServiceInteraction != null)
			{
				if (!string.IsNullOrEmpty(m_ServiceInteraction.name))
				{
					text = m_ServiceInteraction.name;
				}
				else if (m_ServiceInteraction.value != null)
				{
					text = m_ServiceInteraction.value.name;
				}
			}
			return "Can " + text + " beckon a customer?";
		}
	}

	protected override bool OnCheck()
	{
		JobsManager instance = JobsManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		BaseJob charactersJob = instance.GetCharactersJob(base.agent.m_Character);
		if (charactersJob == null)
		{
			return false;
		}
		BeckonAndServiceCustomerJob beckonAndServiceCustomerJob = charactersJob as BeckonAndServiceCustomerJob;
		if (beckonAndServiceCustomerJob != null)
		{
			ServiceItemInteractiveObject serviceItemInteractiveObject = m_ServiceInteraction.value as ServiceItemInteractiveObject;
			if (serviceItemInteractiveObject == null)
			{
				return false;
			}
			return beckonAndServiceCustomerJob.CanServiceInteractionBeckonNewCustomer(serviceItemInteractiveObject);
		}
		return false;
	}
}
