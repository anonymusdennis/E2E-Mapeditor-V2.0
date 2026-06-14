using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class SetJobCustomerCustomisation : ActionTask<AICharacter>
{
	protected override void OnExecute()
	{
		bool flag = ApplyCustomisation(base.agent);
		if (!flag)
		{
		}
		EndAction(flag);
	}

	public static bool ApplyCustomisation(AICharacter customer)
	{
		JobCustomerRequester instance = JobCustomerRequester.GetInstance();
		if (instance != null)
		{
			instance.SetupNextCustomerCustomisationRPC(customer.m_Character);
			JobCustomerRequester.CustomerAstheticInfo infoForCustomer = instance.GetInfoForCustomer(customer.m_Character);
			if (infoForCustomer != null)
			{
				customer.m_Character.m_CharacterCustomisation.SetCustomisation(infoForCustomer.appearance);
				customer.m_Character.m_CharacterCustomisation.SetOutfit(infoForCustomer.appearance.defaultOutfit);
				return true;
			}
		}
		return false;
	}
}
