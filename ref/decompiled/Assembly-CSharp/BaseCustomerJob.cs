using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(T17NetView))]
public abstract class BaseCustomerJob : BaseJob
{
	protected JobCustomerRequester m_CustomerRequester;

	protected T17NetView m_NetView;

	protected AICharacter_JobCustomer.PatronTypes m_CustomerType;

	[Header("Customer Job")]
	public bool m_bCustomerWantsRandomCustomisation = true;

	public bool m_bCustomerWaitsAtExitPoint = true;

	public bool m_bInfiniteCustomers = true;

	public List<RoutineSubTypes> m_IdleActiveRoutines = new List<RoutineSubTypes>();

	private List<RoomBlob> m_RoomsWhenIdleActive;

	private GameObject m_NotIdleActivePosition;

	[FormerlySerializedAs("m_WaitingForServiceLines")]
	public string m_CustomerWaitingForServiceLines = "&Text.Customer.WaitingForService";

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
		m_NetView.viewID = JobsManager.GetInstance().TakeAReservedIdForJobs();
		if (!m_IdleActiveRoutines.Contains(RoutineSubTypes.JobTime))
		{
			m_IdleActiveRoutines.Add(RoutineSubTypes.JobTime);
		}
	}

	protected override void OnDestroy()
	{
		m_NetView = null;
		m_CustomerRequester = null;
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= OnRoutineChanged;
		}
		base.OnDestroy();
	}

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		m_CustomerRequester = JobCustomerRequester.GetInstance();
		m_CustomerType = base.RoomData.m_CustomerPatronType;
		if (!m_bCustomerWaitsAtExitPoint)
		{
			List<AICharacter_JobCustomer> outCustomersList = new List<AICharacter_JobCustomer>();
			int allCustomersOfType = m_CustomerRequester.GetAllCustomersOfType(m_CustomerType, ref outCustomersList);
			for (int i = 0; i < allCustomersOfType; i++)
			{
				outCustomersList[i].SetWaitingPoint(null);
			}
		}
		m_RoomsWhenIdleActive = base.RoomData.m_CustomerIdleActiveRooms;
		m_NotIdleActivePosition = base.RoomData.m_CustomerNotActiveRoutinePosition;
		RoutineManager.GetInstance().OnRoutineChanged += OnRoutineChanged;
	}

	private void OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine == null)
		{
			return;
		}
		List<AICharacter_JobCustomer> outCustomersList = new List<AICharacter_JobCustomer>();
		int allCustomersOfType = m_CustomerRequester.GetAllCustomersOfType(m_CustomerType, ref outCustomersList);
		bool flag = m_IdleActiveRoutines.Contains(newRoutine.m_SubRoutineType);
		for (int i = 0; i < allCustomersOfType; i++)
		{
			AICharacter_JobCustomer aICharacter_JobCustomer = outCustomersList[i];
			if (flag && m_RoomsWhenIdleActive.Count != 0)
			{
				if (!aICharacter_JobCustomer.IsBeingUsedOrServed() && !m_RoomsWhenIdleActive.Contains(aICharacter_JobCustomer.m_Character.m_CurrentLocation))
				{
					RoomBlob roomBlob = m_RoomsWhenIdleActive[i % m_RoomsWhenIdleActive.Count];
					Vector3 position = Vector3.zero;
					roomBlob.GetRandomPositionInRoom(CharacterRole.Visitor, ref position);
					aICharacter_JobCustomer.SetRoutineChangedTargetPosition(position, shouldRun: false);
				}
			}
			else if (!flag && m_NotIdleActivePosition != null)
			{
				aICharacter_JobCustomer.SetRoutineChangedTargetPosition(m_NotIdleActivePosition.transform.position, shouldRun: false);
			}
		}
	}

	protected AICharacter_JobCustomer RequestNewCustomerCharacter(AICharacter_JobCustomer.PatronTypes customerType, bool wantsRandomCustomisation)
	{
		if (m_CustomerRequester != null)
		{
			return m_CustomerRequester.TakeAvailableCustomerRPC(customerType, wantsRandomCustomisation);
		}
		return null;
	}

	public void DismissCustomerRPC(AICharacter_JobCustomer aiCustomer)
	{
		m_NetView.RPC("RPC_ALL_DismissCustomer", NetTargets.All, aiCustomer.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_ALL_DismissCustomer(int aiCharacterViewId)
	{
		AICharacter_JobCustomer aiCustomer = T17NetView.Find<AICharacter_JobCustomer>(aiCharacterViewId);
		Local_DismissCustomer(aiCustomer);
	}

	protected virtual void Local_DismissCustomer(AICharacter_JobCustomer aiCustomer)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_CustomerRequester.MarkCustomerAsFreeRPC(aiCustomer);
		}
		base.RequiresSerialization = true;
	}
}
