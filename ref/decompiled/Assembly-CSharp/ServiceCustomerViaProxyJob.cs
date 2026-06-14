using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class ServiceCustomerViaProxyJob : CustomerJob<CustomerViaProxy>
{
	public delegate void CustomerServiceHandler(ServiceCustomerViaProxyJob sender, CustomerViaProxy customer, bool isWaitingForService);

	[Header("Customer impatience")]
	public SpeechPODO m_RequestServiceProximitySpeech;

	public float m_RequestServiceSpeechDistance = 3.5f;

	public float m_RequestServiceSpeechCooldown = 10f;

	[Header("Customer sounds")]
	public string m_CustomerServicedSound;

	protected List<CustomerServicePointLinker.CustomerServicePoint> m_ServicePoints = new List<CustomerServicePointLinker.CustomerServicePoint>();

	protected Dictionary<AICharacter_JobCustomer, CustomerServicePointLinker.CustomerServicePoint> m_CustomerServicePointMap;

	protected List<CustomerViaProxy> m_CustomersWaitingForService = new List<CustomerViaProxy>();

	private const int NUM_WAITING_CUSTOMERS_HEADER = 5;

	private const int WAITING_CUSTOMER_ID_SIZE = 12;

	public event CustomerServiceHandler CustomerWaitingForServiceChangedEvent;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		m_CustomerServicePointMap = new Dictionary<AICharacter_JobCustomer, CustomerServicePointLinker.CustomerServicePoint>();
		if (!(base.RoomData != null))
		{
			return;
		}
		if (base.RoomData.m_CustomerServicePointLinks.Count == 0)
		{
			CustomerServicePointLinker.CustomerServicePoint customerServicePoint = new CustomerServicePointLinker.CustomerServicePoint();
			customerServicePoint.m_WaitingObject = base.RoomData.m_CustomerWaitObject;
			customerServicePoint.m_WaitingPosition = base.RoomData.m_CustomerWaitPosition;
			m_ServicePoints.Add(customerServicePoint);
			return;
		}
		for (int num = base.RoomData.m_CustomerServicePointLinks.Count - 1; num >= 0; num--)
		{
			CustomerServicePointLinker customerServicePointLinker = base.RoomData.m_CustomerServicePointLinks[num];
			if (customerServicePointLinker == null)
			{
			}
			CustomerServicePointLinker.CustomerServicePoint customerServicePoint2 = new CustomerServicePointLinker.CustomerServicePoint();
			customerServicePoint2.m_WaitingObject = customerServicePointLinker.m_ServicePointInfo.m_WaitingObject;
			customerServicePoint2.m_WaitingPosition = customerServicePointLinker.m_ServicePointInfo.m_WaitingPosition;
			m_ServicePoints.Add(customerServicePoint2);
			for (int num2 = customerServicePointLinker.m_CustomerPool.Count - 1; num2 >= 0; num2--)
			{
				m_CustomerServicePointMap.Add(customerServicePointLinker.m_CustomerPool[num2], customerServicePointLinker.m_ServicePointInfo);
			}
		}
	}

	protected override CustomerViaProxy CreateCustomerFromSerialisedData(ulong[] data)
	{
		return new CustomerViaProxy(data);
	}

	protected override CustomerViaProxy CreateCustomerFromSerialisedData(ulong[] data, ref int headIndex)
	{
		return new CustomerViaProxy(data, ref headIndex);
	}

	protected override void SetUpCustomer(CustomerViaProxy customer)
	{
		base.SetUpCustomer(customer);
		customer.m_AiCustomer.SetProximitySpeech(m_RequestServiceProximitySpeech, m_RequestServiceSpeechDistance, m_RequestServiceSpeechCooldown);
	}

	protected CustomerViaProxy CreateNewCustomer(AICharacter_JobCustomer.PatronTypes customerType, bool wantsRandomCustomisation)
	{
		AICharacter_JobCustomer customerCharacter = RequestNewCustomerCharacter(customerType, wantsRandomCustomisation);
		return CreateNewCustomer(customerCharacter);
	}

	protected CustomerViaProxy CreateNewCustomer(AICharacter_JobCustomer customerCharacter)
	{
		CustomerViaProxy customerViaProxy = new CustomerViaProxy(customerCharacter);
		RegisterCustomerRPC(customerViaProxy);
		return customerViaProxy;
	}

	private void NotifyAllCustomerReadyForServicingRPC(AICharacter_JobCustomer character)
	{
		m_NetView.RPC("RPC_ALL_CharacterReadyForServicing", NetTargets.All, character.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_ALL_CharacterReadyForServicing(int characterViewId)
	{
		CustomerViaProxy customerViaProxy = FindCustomer(characterViewId);
		if (customerViaProxy != null)
		{
			CustomerReadyForServicing(customerViaProxy, isSaveOrNetworkSync: false);
		}
	}

	protected virtual void CustomerReadyForServicing(CustomerViaProxy customer, bool isSaveOrNetworkSync)
	{
		customer.m_bIsReadyForService = true;
		if (!m_CustomersWaitingForService.Contains(customer))
		{
			m_CustomersWaitingForService.Add(customer);
		}
		if (isSaveOrNetworkSync)
		{
			customer.m_AiCustomer.SoftSetReadyToBeServed();
		}
		if (this.CustomerWaitingForServiceChangedEvent != null)
		{
			this.CustomerWaitingForServiceChangedEvent(this, customer, isWaitingForService: true);
		}
		base.RequiresSerialization = true;
	}

	protected override void OnCustomerDismissed(CustomerViaProxy customer)
	{
		base.OnCustomerDismissed(customer);
		customer.m_bIsReadyForService = false;
		if (m_CustomersWaitingForService.Contains(customer))
		{
			m_CustomersWaitingForService.Remove(customer);
			if (this.CustomerWaitingForServiceChangedEvent != null)
			{
				this.CustomerWaitingForServiceChangedEvent(this, customer, isWaitingForService: false);
			}
		}
	}

	public bool HasWaitingCustomer()
	{
		return m_CustomersWaitingForService.Count > 0;
	}

	public CustomerViaProxy GetWaitingCustomer()
	{
		if (m_CustomersWaitingForService.Count > 0)
		{
			return m_CustomersWaitingForService[0];
		}
		return null;
	}

	public CustomerViaProxy GetWaitingCustomer(AICharacter customer)
	{
		for (int num = m_CustomersWaitingForService.Count - 1; num >= 0; num--)
		{
			if (m_CustomersWaitingForService[num].m_AiCustomer == customer)
			{
				return m_CustomersWaitingForService[num];
			}
		}
		return null;
	}

	public void ServiceWaitingCustomerRPC(ServiceCustomer sender, Character servicingCharacter)
	{
		m_NetView.RPC("RPC_All_ServiceWaitingCustomer", NetTargets.All, sender.GetNetViewId(), servicingCharacter.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_All_ServiceWaitingCustomer(int netViewId, int servicingCharacterId)
	{
		ServiceCustomer serviceCustomer = T17NetView.Find<ServiceCustomer>(netViewId);
		Character servicer = T17NetView.Find<Character>(servicingCharacterId);
		if (serviceCustomer != null)
		{
			Local_ServiceWaitingCustomer(serviceCustomer, servicer);
		}
	}

	protected virtual CustomerViaProxy GetCustomerToService(ServiceCustomer sender, Character servicer)
	{
		return GetWaitingCustomer();
	}

	protected virtual void Local_ServiceWaitingCustomer(ServiceCustomer sender, Character servicer)
	{
		CustomerViaProxy customerToService = GetCustomerToService(sender, servicer);
		if (customerToService != null)
		{
			OnCustomerServiced(customerToService, sender, servicer);
		}
	}

	public void ServiceWaitingCustomerRPC(ServiceCustomer sender, ItemData itemGiven, Character servicingCharacter)
	{
		m_NetView.RPC("RPC_All_ServiceWaitingCustomer", NetTargets.All, sender.GetNetViewId(), itemGiven.m_ItemDataID, servicingCharacter.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_All_ServiceWaitingCustomer(int netViewId, int itemDataId, int servicingCharacterId)
	{
		ServiceCustomer serviceCustomer = T17NetView.Find<ServiceCustomer>(netViewId);
		ItemData itemDataWithID = ItemManager.GetInstance().GetItemDataWithID(itemDataId);
		Character servicingCharacter = T17NetView.Find<Character>(servicingCharacterId);
		if (serviceCustomer != null && itemDataWithID != null)
		{
			Local_ServiceWaitingCustomer(serviceCustomer, itemDataWithID, servicingCharacter);
		}
	}

	protected virtual void Local_ServiceWaitingCustomer(ServiceCustomer sender, ItemData itemGiven, Character servicingCharacter)
	{
		CustomerViaProxy customerToService = GetCustomerToService(sender, servicingCharacter);
		if (customerToService != null)
		{
			OnCustomerServiced(customerToService, sender, servicingCharacter);
		}
	}

	protected virtual void OnCustomerServiced(CustomerViaProxy customer, ServiceCustomer sender, Character servicingCharacter)
	{
		if (T17NetManager.IsMasterClient)
		{
			IncrementQuotaAchieved();
			DismissCustomerRPC(customer.m_AiCustomer);
		}
		if (!string.IsNullOrEmpty(m_CustomerServicedSound))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_CustomerServicedSound, customer.m_AiCustomer.gameObject);
		}
		base.RequiresSerialization = true;
	}

	public void SetCustomerReadyToBeServedRPC(AICharacter_JobCustomer character)
	{
		NotifyAllCustomerReadyForServicingRPC(character);
	}

	public virtual void GetServiceLocationData(out InteractiveObject objectToInteractWith, out GameObject spotToWaitAt, out FacingDirectionIncInvalid facingDirection, AICharacter_JobCustomer customer)
	{
		if (!m_CustomerServicePointMap.TryGetValue(customer, out var value))
		{
			value = m_ServicePoints[Random.Range(0, m_ServicePoints.Count)];
		}
		objectToInteractWith = value.m_WaitingObject;
		spotToWaitAt = value.m_WaitingPosition;
		facingDirection = value.m_FaceDirection;
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		BitField bitField = new BitField();
		bitField.Set(5, (uint)m_CustomersWaitingForService.Count);
		int num = 59;
		List<CustomerViaProxy> list2 = new List<CustomerViaProxy>(m_CustomersWaitingForService);
		while (list2.Count > 0)
		{
			if (num > 12)
			{
				bitField.Set(12, (uint)list2[0].m_AiCustomer.m_NetView.viewID);
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

	protected override void DeserializeJobSpecificData(ulong[] jobData, ref int headIndex)
	{
		base.DeserializeJobSpecificData(jobData, ref headIndex);
		if (m_DeserialiseDataVersion == 1)
		{
			DeserializeJobSpecificData_Version_1(jobData, ref headIndex);
		}
		else if (m_DeserialiseDataVersion == 0)
		{
			DeserializeJobSpecificData_Version_0(jobData, ref headIndex);
		}
	}

	private void DeserializeJobSpecificData_Version_1(ulong[] jobData, ref int headIndex)
	{
		BitField bitField = new BitField(jobData[headIndex++]);
		int uInt = (int)bitField.GetUInt(5);
		int num = 0;
		int num2 = 5;
		while (num < uInt)
		{
			if (num2 + 12 < 64)
			{
				int uInt2 = (int)bitField.GetUInt(12);
				CustomerViaProxy customerViaProxy = FindCustomer(uInt2);
				num++;
				num2 += 12;
				if (customerViaProxy != null)
				{
					CustomerReadyForServicing(customerViaProxy, isSaveOrNetworkSync: true);
				}
			}
			else
			{
				bitField = new BitField(jobData[headIndex++]);
				num2 = 0;
			}
		}
	}

	protected void DeserializeJobSpecificData_Version_0(ulong[] jobData, ref int headIndex)
	{
		BitField bitField = new BitField(jobData[headIndex++]);
		if (bitField.GetBool())
		{
			int uInt = (int)bitField.GetUInt(12);
			CustomerViaProxy customerViaProxy = FindCustomer(uInt);
			if (customerViaProxy != null)
			{
				CustomerReadyForServicing(customerViaProxy, isSaveOrNetworkSync: true);
			}
		}
	}

	public override int GetSaveDataVersion()
	{
		return 1;
	}
}
