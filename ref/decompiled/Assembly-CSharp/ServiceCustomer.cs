using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class ServiceCustomer : MonoBehaviour
{
	private ServiceCustomerViaProxyJob m_LinkedJob;

	private T17NetView m_NetView;

	protected void Awake()
	{
		m_NetView = GetComponent<T17NetView>();
	}

	protected void OnDestroy()
	{
		m_LinkedJob = null;
		m_NetView = null;
	}

	public void LinkToJob(ServiceCustomerViaProxyJob job)
	{
		m_LinkedJob = job;
	}

	public ServiceCustomerViaProxyJob GetLinkedJob()
	{
		return m_LinkedJob;
	}

	public void ServiceActionPerformed(Character server)
	{
		if (m_LinkedJob != null)
		{
			m_LinkedJob.ServiceWaitingCustomerRPC(this, server);
		}
	}

	public void ServiceActionPerformed(ItemData transferedItem, Character server)
	{
		if (m_LinkedJob != null)
		{
			m_LinkedJob.ServiceWaitingCustomerRPC(this, transferedItem, server);
		}
	}

	public int GetNetViewId()
	{
		return m_NetView.viewID;
	}

	public bool DoesLinkedJobHavePendingCustomer()
	{
		if (m_LinkedJob != null)
		{
			return m_LinkedJob.HasWaitingCustomer();
		}
		return false;
	}
}
