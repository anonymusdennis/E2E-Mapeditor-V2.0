using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomerServicePointLinker : MonoBehaviour
{
	[Serializable]
	public class CustomerServicePoint
	{
		public InteractiveObject m_WaitingObject;

		public GameObject m_WaitingPosition;

		public FacingDirectionIncInvalid m_FaceDirection;
	}

	public CustomerServicePoint m_ServicePointInfo;

	public ServiceItemInteractiveObject m_ServiceInteractiveObject;

	public List<AICharacter_JobCustomer> m_CustomerPool;

	private void Awake()
	{
		if (m_ServiceInteractiveObject != null)
		{
			m_ServiceInteractiveObject.m_ServicePointLinker = this;
		}
	}

	protected void OnDestroy()
	{
		m_CustomerPool.Clear();
		m_ServicePointInfo = null;
		m_ServiceInteractiveObject = null;
	}
}
