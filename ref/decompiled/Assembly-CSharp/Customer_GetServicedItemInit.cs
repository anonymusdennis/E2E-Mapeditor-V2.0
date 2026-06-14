using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Jobs")]
public class Customer_GetServicedItemInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<BaseCustomerJob> m_EnactingJob;

	[BlackboardOnly]
	public BBParameter<InteractiveObject> m_WaitingPointInteractiveObject;

	[BlackboardOnly]
	public BBParameter<Vector3> m_WaitingPointPosition;

	public BBParameter<int> m_WaitingFacingDirection;

	protected override void OnExecute()
	{
		base.OnExecute();
		BaseCustomerJob value = m_EnactingJob.value;
		if (value != null)
		{
			ServiceCustomerViaProxyJob serviceCustomerViaProxyJob = value as ServiceCustomerViaProxyJob;
			if (serviceCustomerViaProxyJob == null)
			{
				EndAction(false);
				return;
			}
			InteractiveObject objectToInteractWith = null;
			GameObject spotToWaitAt = null;
			AICharacter_JobCustomer customer = base.agent as AICharacter_JobCustomer;
			serviceCustomerViaProxyJob.GetServiceLocationData(out objectToInteractWith, out spotToWaitAt, out var facingDirection, customer);
			m_WaitingPointInteractiveObject.value = objectToInteractWith;
			if (objectToInteractWith != null)
			{
				spotToWaitAt = objectToInteractWith.gameObject;
			}
			m_WaitingPointPosition.value = spotToWaitAt.transform.position;
			m_WaitingFacingDirection.value = (int)facingDirection;
			EndAction(true);
		}
		else
		{
			EndAction(false);
		}
	}
}
