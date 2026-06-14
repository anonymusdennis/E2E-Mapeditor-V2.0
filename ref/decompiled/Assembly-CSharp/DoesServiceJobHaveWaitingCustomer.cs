using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class DoesServiceJobHaveWaitingCustomer : ConditionTask<AICharacter>
{
	protected override string info => "Does ServiceJobHaveWaitingCustomer have a waiting customer?";

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
		ServiceCustomerViaProxyJob serviceCustomerViaProxyJob = charactersJob as ServiceCustomerViaProxyJob;
		if (serviceCustomerViaProxyJob != null)
		{
			return serviceCustomerViaProxyJob.HasWaitingCustomer();
		}
		return false;
	}
}
