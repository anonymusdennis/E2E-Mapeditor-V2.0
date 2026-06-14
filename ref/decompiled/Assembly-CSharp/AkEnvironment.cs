using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[AddComponentMenu("Wwise/AkEnvironment")]
public class AkEnvironment : MonoBehaviour
{
	public class AkEnvironment_CompareByPriority : IComparer<AkEnvironment>
	{
		public int Compare(AkEnvironment a, AkEnvironment b)
		{
			int num = a.priority.CompareTo(b.priority);
			if (num == 0 && a != b)
			{
				return 1;
			}
			return num;
		}
	}

	public class AkEnvironment_CompareBySelectionAlgorithm : IComparer<AkEnvironment>
	{
		private int compareByPriority(AkEnvironment a, AkEnvironment b)
		{
			int num = a.priority.CompareTo(b.priority);
			if (num == 0 && a != b)
			{
				return 1;
			}
			return num;
		}

		public int Compare(AkEnvironment a, AkEnvironment b)
		{
			if (a.isDefault)
			{
				if (b.isDefault)
				{
					return compareByPriority(a, b);
				}
				return 1;
			}
			if (b.isDefault)
			{
				return -1;
			}
			if (a.excludeOthers)
			{
				if (b.excludeOthers)
				{
					return compareByPriority(a, b);
				}
				return -1;
			}
			if (b.excludeOthers)
			{
				return 1;
			}
			return compareByPriority(a, b);
		}
	}

	public const int MAX_NB_ENVIRONMENTS = 4;

	public static AkEnvironment_CompareByPriority s_compareByPriority = new AkEnvironment_CompareByPriority();

	public static AkEnvironment_CompareBySelectionAlgorithm s_compareBySelectionAlgorithm = new AkEnvironment_CompareBySelectionAlgorithm();

	[SerializeField]
	private int m_auxBusID;

	private Collider m_Collider;

	public int priority;

	public bool isDefault;

	public bool excludeOthers;

	public uint GetAuxBusID()
	{
		return (uint)m_auxBusID;
	}

	public void SetAuxBusID(int in_auxBusID)
	{
		m_auxBusID = in_auxBusID;
	}

	public float GetAuxSendValueForPosition(Vector3 in_position)
	{
		return 1f;
	}

	public Collider GetCollider()
	{
		return m_Collider;
	}

	public void Awake()
	{
		m_Collider = GetComponent<Collider>();
	}
}
