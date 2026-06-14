using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class BeckonCustomer : T17MonoBehaviour
{
	public delegate void CustomerWaitingChangedHandler(BeckonCustomer sender, AICharacter_JobCustomer customer);

	private BeckonAndServiceCustomerJob m_LinkedJob;

	private T17NetView m_NetView;

	private AICharacter_JobCustomer m_BeckonedCustomer;

	public event CustomerWaitingChangedHandler BeckonedCustomerChangedEvent;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_NetView.RPC("RPC_Master_SyncWaitingCustomer", NetTargets.MasterClient);
		return base.StartInit();
	}

	protected void OnDestroy()
	{
		m_LinkedJob = null;
		m_NetView = null;
	}

	[PunRPC]
	private void RPC_Master_SyncWaitingCustomer(PhotonMessageInfo messageInfo)
	{
		m_NetView.RPC("RPC_CLIENT_RecieveWaitingCustomer", messageInfo.sender, (!(m_BeckonedCustomer == null)) ? m_BeckonedCustomer.m_Character.m_NetView.viewID : (-1));
	}

	[PunRPC]
	private void RPC_CLIENT_RecieveWaitingCustomer(int netViewId)
	{
		RPC_ALL_SetBeckoningCustomer(-1, netViewId);
	}

	public void LinkToJob(BeckonAndServiceCustomerJob job)
	{
		m_LinkedJob = job;
	}

	public void CallForNextCustomer()
	{
		m_LinkedJob.CallForNextCustomerRPC(this);
	}

	public void CancelRequestForCustomer()
	{
		if (m_BeckonedCustomer != null)
		{
			AICharacter_JobCustomer beckonedCustomer = m_BeckonedCustomer;
			FreeBeckonedCustomerRPC();
			m_LinkedJob.CancelRequestForCustomerRPC(this, beckonedCustomer);
		}
	}

	public int GetNetViewId()
	{
		return m_NetView.viewID;
	}

	public void SetBeckoningCustomerRPC(AICharacter_JobCustomer customer)
	{
		if (customer != null)
		{
			if (m_BeckonedCustomer != null)
			{
			}
			m_NetView.RPCQuestion("RPC_ALL_SetBeckoningCustomer", NetTargets.All, customer.m_NetView.viewID);
		}
		else
		{
			FreeBeckonedCustomerRPC();
		}
	}

	public void FreeBeckonedCustomerRPC()
	{
		m_NetView.RPCQuestion("RPC_ALL_SetBeckoningCustomer", NetTargets.All, -1);
	}

	[PunRPC]
	private void RPC_ALL_SetBeckoningCustomer(int RpcID, int viewId)
	{
		if (viewId == -1)
		{
			m_BeckonedCustomer = null;
		}
		else
		{
			m_BeckonedCustomer = T17NetView.Find<AICharacter_JobCustomer>(viewId);
		}
		if (this.BeckonedCustomerChangedEvent != null)
		{
			this.BeckonedCustomerChangedEvent(this, m_BeckonedCustomer);
		}
		m_NetView.RPCResponse(null, RpcID);
	}

	public void Local_SetBeckonedCustomer(AICharacter_JobCustomer customer)
	{
		m_BeckonedCustomer = customer;
		if (this.BeckonedCustomerChangedEvent != null)
		{
			this.BeckonedCustomerChangedEvent(this, m_BeckonedCustomer);
		}
	}

	public AICharacter_JobCustomer GetBeckonedCustomer()
	{
		return m_BeckonedCustomer;
	}

	public bool IsAllowedToBeckonNewCustomer()
	{
		return m_LinkedJob.IsAllowedToBeckonNewCustomer(this);
	}
}
