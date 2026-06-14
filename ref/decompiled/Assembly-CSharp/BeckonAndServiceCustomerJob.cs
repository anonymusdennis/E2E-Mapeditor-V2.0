using System.Collections.Generic;
using SaveHelpers;

public class BeckonAndServiceCustomerJob : ServiceItemJob
{
	private Dictionary<BeckonCustomer, BeckonAndMinigameServeCustomerInteraction> m_BeckonInteractionMaps;

	private List<ServiceItemInteractiveObject> m_InteractionsServicedFrom = new List<ServiceItemInteractiveObject>();

	private const int NUM_INTERACTIONS_SERVICED_FROM_HEADER = 5;

	private const int INTERACTION_SERVICED_FROM_ID_SIZE = 12;

	protected override void CustomerReadyForServicing(CustomerViaProxy customer, bool isSaveOrNetworkSync)
	{
		base.CustomerReadyForServicing(customer, isSaveOrNetworkSync);
		if (!isSaveOrNetworkSync)
		{
			return;
		}
		for (int num = m_ServiceItemObjects.Count - 1; num >= 0; num--)
		{
			if (m_ServiceItemObjects[num].m_ServicePointLinker.m_CustomerPool.Contains(customer.m_AiCustomer))
			{
				BeckonCustomer component = m_ServiceItemObjects[num].m_ServicePointLinker.m_ServiceInteractiveObject.GetComponent<BeckonCustomer>();
				if (component != null)
				{
					component.Local_SetBeckonedCustomer(customer.m_AiCustomer);
				}
			}
		}
	}

	protected override void CreateNewCustomerForCustomerJustServiced()
	{
	}

	protected override void CreateNewCustomerForJobTimeStarted()
	{
	}

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		m_BeckonInteractionMaps = new Dictionary<BeckonCustomer, BeckonAndMinigameServeCustomerInteraction>();
		for (int num = m_ServiceItemObjects.Count - 1; num >= 0; num--)
		{
			BeckonAndMinigameServeCustomerInteraction beckonAndMinigameServeCustomerInteraction = (BeckonAndMinigameServeCustomerInteraction)m_ServiceItemObjects[num];
			if (!(beckonAndMinigameServeCustomerInteraction == null))
			{
				BeckonCustomer component = beckonAndMinigameServeCustomerInteraction.GetComponent<BeckonCustomer>();
				if (component != null)
				{
					component.LinkToJob(this);
					m_BeckonInteractionMaps.Add(component, beckonAndMinigameServeCustomerInteraction);
				}
			}
		}
	}

	protected override void OnCustomerServiced(CustomerViaProxy customer, ServiceCustomer sender, Character servicingCharacter)
	{
		base.OnCustomerServiced(customer, sender, servicingCharacter);
		ServiceItemInteractiveObject component = sender.GetComponent<ServiceItemInteractiveObject>();
		m_InteractionsServicedFrom.Add(component);
	}

	protected override CustomerViaProxy GetCustomerToService(ServiceCustomer sender, Character servicer)
	{
		ServiceItemInteractiveObject serviceItemInteractiveObject = m_ServiceMaps[sender];
		return GetWaitingCustomer(serviceItemInteractiveObject.m_ServicePointLinker.m_CustomerPool[0]);
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore)
		{
			m_InteractionsServicedFrom.Clear();
		}
	}

	public void CallForNextCustomerRPC(BeckonCustomer sender)
	{
		BeckonAndMinigameServeCustomerInteraction beckonAndMinigameServeCustomerInteraction = m_BeckonInteractionMaps[sender];
		if (beckonAndMinigameServeCustomerInteraction.m_ServicePointLinker != null)
		{
			AICharacter_JobCustomer aICharacter_JobCustomer = beckonAndMinigameServeCustomerInteraction.m_ServicePointLinker.m_CustomerPool[0];
			beckonAndMinigameServeCustomerInteraction.m_BeckonCustomerComponent.SetBeckoningCustomerRPC(aICharacter_JobCustomer);
			CreateNewCustomer(aICharacter_JobCustomer);
			m_CustomerRequester.MarkCustomerAsTakenRPC(aICharacter_JobCustomer, m_bCustomerWantsRandomCustomisation);
		}
	}

	public void CancelRequestForCustomerRPC(BeckonCustomer sender, AICharacter_JobCustomer customer)
	{
		DismissCustomerRPC(customer);
	}

	public bool IsAllowedToBeckonNewCustomer(BeckonCustomer sender)
	{
		ServiceItemInteractiveObject component = sender.GetComponent<ServiceItemInteractiveObject>();
		return CanServiceInteractionBeckonNewCustomer(component);
	}

	public bool CanServiceInteractionBeckonNewCustomer(ServiceItemInteractiveObject serviceInteraction)
	{
		if (m_bInfiniteCustomers)
		{
			return true;
		}
		return !m_InteractionsServicedFrom.Contains(serviceInteraction);
	}

	protected override void DeserializeJobSpecificData(ulong[] jobData, ref int headIndex)
	{
		base.DeserializeJobSpecificData(jobData, ref headIndex);
		BitField bitField = new BitField(jobData[headIndex++]);
		int uInt = (int)bitField.GetUInt(5);
		int num = 0;
		int num2 = 5;
		while (num < uInt)
		{
			if (num2 + 12 < 64)
			{
				int uInt2 = (int)bitField.GetUInt(12);
				ServiceItemInteractiveObject serviceItemInteractiveObject = T17NetView.Find<ServiceItemInteractiveObject>(uInt2);
				num++;
				num2 += 12;
				if (!(serviceItemInteractiveObject == null))
				{
					m_InteractionsServicedFrom.Add(serviceItemInteractiveObject);
				}
			}
			else
			{
				bitField = new BitField(jobData[headIndex++]);
				num2 = 0;
			}
		}
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		BitField bitField = new BitField();
		bitField.Set(5, (uint)m_InteractionsServicedFrom.Count);
		int num = 59;
		List<ServiceItemInteractiveObject> list2 = new List<ServiceItemInteractiveObject>(m_InteractionsServicedFrom);
		while (list2.Count > 0)
		{
			if (num > 12)
			{
				bitField.Set(12, (uint)list2[0].m_NetViewID.viewID);
				num -= 12;
				list2.RemoveAt(0);
			}
			else
			{
				list.Add((ulong)bitField);
				bitField = new BitField();
				num = 64;
			}
		}
		list.Add((ulong)bitField);
		return list;
	}
}
