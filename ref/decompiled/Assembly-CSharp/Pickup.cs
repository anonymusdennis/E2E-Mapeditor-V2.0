using UnityEngine;

public class Pickup : MonoBehaviour
{
	private Transform m_OldParent;

	private void Start()
	{
	}

	public bool CanPickup()
	{
		return true;
	}

	public bool PickupObject(GameObject goPickedup)
	{
		m_OldParent = base.transform.parent;
		base.transform.parent = goPickedup.transform;
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.Sleep();
		}
		return true;
	}

	public bool PutDownObject()
	{
		base.transform.parent = m_OldParent;
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.WakeUp();
		}
		return true;
	}
}
