using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class ServiceItemAtRandomServicePointJob : ServiceItemJob
{
	public Vector3 m_ServiceInteractionOffset = new Vector3(0f, 0.5f, 0f);

	private int m_ServicePointIndexForWaitingCustomer;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		ServiceItemInteractiveObject firstValidServiceInteraction = GetFirstValidServiceInteraction();
		firstValidServiceInteraction.m_bInteractionVisibility = false;
		firstValidServiceInteraction.gameObject.SetActive(value: false);
	}

	public override void GetServiceLocationData(out InteractiveObject objectToInteractWith, out GameObject spotToWaitAt, out FacingDirectionIncInvalid facingDirection, AICharacter_JobCustomer customer)
	{
		CustomerViaProxy waitingCustomer = GetWaitingCustomer();
		if (waitingCustomer == null || waitingCustomer.m_AiCustomer != customer)
		{
			m_ServicePointIndexForWaitingCustomer = m_NetworkSycnedRandom.Next(0, m_ServicePoints.Count);
		}
		CustomerServicePointLinker.CustomerServicePoint customerServicePoint = m_ServicePoints[m_ServicePointIndexForWaitingCustomer];
		objectToInteractWith = customerServicePoint.m_WaitingObject;
		spotToWaitAt = customerServicePoint.m_WaitingPosition;
		facingDirection = customerServicePoint.m_FaceDirection;
	}

	protected override void CustomerReadyForServicing(CustomerViaProxy customer, bool isSaveOrNetworkSync)
	{
		base.CustomerReadyForServicing(customer, isSaveOrNetworkSync);
		ServiceItemInteractiveObject firstValidServiceInteraction = GetFirstValidServiceInteraction();
		firstValidServiceInteraction.gameObject.SetActive(value: true);
		firstValidServiceInteraction.m_bInteractionVisibility = true;
		firstValidServiceInteraction.transform.SetParent(customer.m_AiCustomer.transform, worldPositionStays: false);
		firstValidServiceInteraction.transform.localPosition = m_ServiceInteractionOffset;
		customer.m_AiCustomer.m_Character.m_CharacterSphereTrigger.enabled = false;
		SetRoutineInformationForCharacter(base.Employee);
	}

	protected override void OnCustomerDismissed(CustomerViaProxy customer)
	{
		base.OnCustomerDismissed(customer);
		ServiceItemInteractiveObject firstValidServiceInteraction = GetFirstValidServiceInteraction();
		firstValidServiceInteraction.gameObject.SetActive(value: false);
		firstValidServiceInteraction.m_bInteractionVisibility = false;
		firstValidServiceInteraction.transform.SetParent(null, worldPositionStays: true);
		customer.m_AiCustomer.m_Character.m_CharacterSphereTrigger.enabled = true;
		SetRoutineInformationForCharacter(base.Employee);
	}

	protected override void DeserializeJobSpecificData(ulong[] jobData, ref int headIndex)
	{
		base.DeserializeJobSpecificData(jobData, ref headIndex);
		BitField bitField = new BitField(jobData[headIndex++]);
		m_ServicePointIndexForWaitingCustomer = (int)bitField.GetUInt(5);
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		BitField bitField = new BitField();
		bitField.Set(5, (uint)m_ServicePointIndexForWaitingCustomer);
		list.Add((ulong)bitField);
		return list;
	}
}
