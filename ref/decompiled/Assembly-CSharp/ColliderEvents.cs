using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEvents : MonoBehaviour
{
	public enum ColliderEvent
	{
		OnCollisionEnter,
		OnCollisionExit,
		OnCollisionStay,
		OnTriggerEnter,
		OnTriggerExit,
		OnTriggerStay
	}

	[Tooltip("NEEDS TO BE ON THE SAME GAMEOBJECT\nIf not assigned, a GetComponentOnChildren will run to find a collider")]
	public Collider m_Collider;

	public List<ColliderEvent> m_EventTypesToFireOn = new List<ColliderEvent>();

	public ColliderEvent m_FireOnEventType = ColliderEvent.OnTriggerEnter;

	[Tooltip("If turned on, the object colliding/triggering will be checked for being the player")]
	public bool m_bNeedsToBePlayer = true;

	public UnityEvent m_Event;

	protected virtual void Start()
	{
		if (m_Collider == null)
		{
			m_Collider = GetComponentInChildren<Collider>(includeInactive: true);
		}
		if (m_Collider != null)
		{
			switch (m_FireOnEventType)
			{
			case ColliderEvent.OnCollisionEnter:
			case ColliderEvent.OnCollisionExit:
			case ColliderEvent.OnCollisionStay:
				m_Collider.isTrigger = false;
				break;
			case ColliderEvent.OnTriggerEnter:
			case ColliderEvent.OnTriggerExit:
			case ColliderEvent.OnTriggerStay:
				m_Collider.isTrigger = true;
				break;
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private bool ShouldProcessEvent(ColliderEvent targetEvent)
	{
		return m_FireOnEventType == targetEvent || m_EventTypesToFireOn.Contains(targetEvent);
	}

	private void OnCollisionEnter(Collision collisionInfo)
	{
		if (ShouldProcessEvent(ColliderEvent.OnCollisionEnter))
		{
			ProcessCollision(collisionInfo.transform, ColliderEvent.OnCollisionEnter);
		}
	}

	private void OnCollisionExit(Collision collisionInfo)
	{
		if (ShouldProcessEvent(ColliderEvent.OnCollisionExit))
		{
			ProcessCollision(collisionInfo.transform, ColliderEvent.OnCollisionExit);
		}
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		if (ShouldProcessEvent(ColliderEvent.OnCollisionStay))
		{
			ProcessCollision(collisionInfo.transform, ColliderEvent.OnCollisionStay);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (ShouldProcessEvent(ColliderEvent.OnTriggerEnter))
		{
			ProcessCollision(other.transform, ColliderEvent.OnTriggerEnter);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (ShouldProcessEvent(ColliderEvent.OnTriggerExit))
		{
			ProcessCollision(other.transform, ColliderEvent.OnTriggerExit);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (ShouldProcessEvent(ColliderEvent.OnTriggerStay))
		{
			ProcessCollision(other.transform, ColliderEvent.OnTriggerStay);
		}
	}

	private void ProcessCollision(Transform colliderTransform, ColliderEvent colliderEvent)
	{
		if (ShouldInvoke(colliderTransform.parent))
		{
			FireEvent(colliderTransform, colliderEvent);
		}
	}

	protected virtual void FireEvent(Transform colliderTransform, ColliderEvent colliderEvent)
	{
		m_Event.Invoke();
	}

	private bool ShouldInvoke(Transform toCheck)
	{
		bool result = true;
		if (m_bNeedsToBePlayer)
		{
			result = false;
			Player component = toCheck.GetComponent<Player>();
			if (component != null)
			{
				GlobalStart instance = GlobalStart.GetInstance();
				if (component.m_Gamer != null && instance != null && instance.IsWithinLevel() && null != component.m_NetView && component.GetIsGamerControlled() && T17NetManager.IsMasterClient && !component.GetIsKnockedOut() && !component.GetIsDisabled() && component.m_NetView.ownerId == component.m_Gamer.m_PhotonID)
				{
					result = true;
				}
			}
		}
		return result;
	}
}
