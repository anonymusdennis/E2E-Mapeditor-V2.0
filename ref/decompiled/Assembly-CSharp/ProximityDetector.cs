using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityDetector : MonoBehaviour
{
	private class ColliderEntityInfo
	{
		public bool isCharacter;

		public NetObjectLock netObjectLock;

		public TrackableUIElementsReporter trackable;

		public bool isItem;
	}

	public int m_LayerMask;

	private int m_ItemLayerMask;

	private bool m_bBufferA = true;

	private List<ItemContainer> m_A_NearbyItemContainers = new List<ItemContainer>();

	private List<Item> m_A_NearbyItems = new List<Item>();

	private List<NetObjectLock> m_A_NearbyNetObjectLocks = new List<NetObjectLock>();

	private List<TrackableUIElementsReporter> m_A_NearbyCharacters = new List<TrackableUIElementsReporter>();

	private List<ItemContainer> m_B_NearbyItemContainers = new List<ItemContainer>();

	private List<Item> m_B_NearbyItems = new List<Item>();

	private List<NetObjectLock> m_B_NearbyNetObjectLocks = new List<NetObjectLock>();

	private List<TrackableUIElementsReporter> m_B_NearbyCharacters = new List<TrackableUIElementsReporter>();

	private Character m_Owner;

	private Dictionary<Collider, ColliderEntityInfo> m_ColliderToEntityInfo = new Dictionary<Collider, ColliderEntityInfo>();

	private List<ItemContainer> m_Read_NearbyItemContainers => (!m_bBufferA) ? m_B_NearbyItemContainers : m_A_NearbyItemContainers;

	private List<Item> m_Read_NearbyItems => (!m_bBufferA) ? m_B_NearbyItems : m_A_NearbyItems;

	private List<NetObjectLock> m_Read_NearbyNetObjectLocks => (!m_bBufferA) ? m_B_NearbyNetObjectLocks : m_A_NearbyNetObjectLocks;

	private List<TrackableUIElementsReporter> m_Read_NearbyCharacters => (!m_bBufferA) ? m_B_NearbyCharacters : m_A_NearbyCharacters;

	private List<Item> m_Write_NearbyItems => (!m_bBufferA) ? m_A_NearbyItems : m_B_NearbyItems;

	private List<TrackableUIElementsReporter> m_Write_NearbyCharacters => (!m_bBufferA) ? m_A_NearbyCharacters : m_B_NearbyCharacters;

	private List<NetObjectLock> m_Write_NearbyNetObjectLocks => (!m_bBufferA) ? m_A_NearbyNetObjectLocks : m_B_NearbyNetObjectLocks;

	public string DebugInfo()
	{
		return "PD:" + m_Read_NearbyItemContainers.Count + "," + m_Read_NearbyItems.Count + "," + m_Read_NearbyNetObjectLocks.Count;
	}

	private void Awake()
	{
		m_Owner = GetComponentInParent<Character>();
	}

	private void Start()
	{
		m_LayerMask = (1 << LayerMask.NameToLayer("Wall")) | (1 << LayerMask.NameToLayer("Fence")) | (1 << LayerMask.NameToLayer("Door"));
		m_ItemLayerMask = LayerMask.NameToLayer("Items");
	}

	private void FixedUpdate()
	{
		m_bBufferA = !m_bBufferA;
		m_Write_NearbyItems.Clear();
		m_Write_NearbyCharacters.Clear();
		m_Write_NearbyNetObjectLocks.Clear();
	}

	private ColliderEntityInfo GetColliderEntityInfo(Collider collider)
	{
		if (m_ColliderToEntityInfo.ContainsKey(collider))
		{
			return m_ColliderToEntityInfo[collider];
		}
		ColliderEntityInfo colliderEntityInfo = new ColliderEntityInfo();
		colliderEntityInfo.isCharacter = collider.CompareTag("Character");
		if (colliderEntityInfo.isCharacter)
		{
			colliderEntityInfo.netObjectLock = collider.GetComponentInParent<NetObjectLock>();
			colliderEntityInfo.trackable = collider.GetComponentInParent<TrackableUIElementsReporter>();
		}
		else
		{
			colliderEntityInfo.netObjectLock = collider.GetComponent<NetObjectLock>();
			if (collider.GetComponent<Item>() != null)
			{
				colliderEntityInfo.isItem = true;
			}
		}
		m_ColliderToEntityInfo[collider] = colliderEntityInfo;
		return colliderEntityInfo;
	}

	private void OnTriggerStay(Collider other)
	{
		if (m_Owner.GetIsDisabled())
		{
			return;
		}
		Vector3 position = base.transform.position;
		Vector2 vector = other.bounds.center;
		float distance = Vector2.Distance(position, vector);
		Vector2 vector2 = vector - (Vector2)position;
		int num = EscapistsRaycast.RaycastAll(position, vector2, distance, m_LayerMask, QueryTriggerInteraction.Ignore);
		if (num > 1 || (num == 1 && EscapistsRaycast.RaycastHitList[0].collider != other))
		{
			return;
		}
		if (other.gameObject.layer == m_ItemLayerMask)
		{
			Item component = other.GetComponent<Item>();
			if (component != null)
			{
				if (!m_Write_NearbyItems.Contains(component))
				{
					m_Write_NearbyItems.Add(component);
				}
				return;
			}
		}
		ColliderEntityInfo colliderEntityInfo = GetColliderEntityInfo(other);
		NetObjectLock netObjectLock = colliderEntityInfo.netObjectLock;
		if (netObjectLock != null)
		{
			if (((netObjectLock.AnyInteractionVisible() && netObjectLock.AnyInteractionAllowed(position)) || colliderEntityInfo.isCharacter) && !m_Write_NearbyNetObjectLocks.Contains(netObjectLock))
			{
				m_Write_NearbyNetObjectLocks.Add(netObjectLock);
			}
		}
		else if (colliderEntityInfo.isCharacter)
		{
			TrackableUIElementsReporter trackable = colliderEntityInfo.trackable;
			if (trackable != null && !m_Write_NearbyCharacters.Contains(trackable))
			{
				m_Write_NearbyCharacters.Add(trackable);
			}
		}
		else if (colliderEntityInfo.isItem)
		{
			Item component2 = other.GetComponent<Item>();
			if (component2 != null && !m_Write_NearbyItems.Contains(component2))
			{
				m_Write_NearbyItems.Add(component2);
			}
		}
	}

	public int GetAnyItems(ref List<Item> list)
	{
		list = m_Read_NearbyItems;
		return list.Count;
	}

	public int GetSortedNearestHoles(ref Hole[] holes, bool includeFullyDug)
	{
		int i = 0;
		if (holes != null)
		{
			Vector3 position = base.transform.position;
			bool flag = false;
			if (m_Owner != null && m_Owner.m_bIsStandingOnDesk)
			{
				FloorManager instance = FloorManager.GetInstance();
				FloorManager.Floor floor = instance.UpAFloor(m_Owner.CurrentFloor);
				if (floor != m_Owner.CurrentFloor && floor.IsVent())
				{
					flag = true;
				}
			}
			if (!flag)
			{
				for (List<Hole> sortedHolesWithinProximity = FloorManager.GetInstance().GetSortedHolesWithinProximity(position, 2, includeFullyDug, includeAboveWalls: true); i < sortedHolesWithinProximity.Count && i < holes.Length; i++)
				{
					holes[i] = sortedHolesWithinProximity[i];
				}
			}
			for (int j = i; j < holes.Length; j++)
			{
				holes[j] = null;
			}
		}
		return i;
	}

	public int GetSortedNearestHoles_FromUnderground(ref Hole[] holes, bool includeFullyDug)
	{
		int i = 0;
		if (holes != null)
		{
			Vector3 position = base.transform.position;
			bool flag = false;
			if (m_Owner != null && m_Owner.CurrentFloor.IsUnderGround())
			{
				FloorManager instance = FloorManager.GetInstance();
				FloorManager.Floor floor = instance.UpAFloor(m_Owner.CurrentFloor);
				if (floor != m_Owner.CurrentFloor && floor.IsAboveUnderGround())
				{
					position.z = floor.m_zPos;
					flag = true;
				}
			}
			if (flag)
			{
				for (List<Hole> sortedHolesWithinProximity = FloorManager.GetInstance().GetSortedHolesWithinProximity(position, 2, includeFullyDug, includeAboveWalls: false); i < sortedHolesWithinProximity.Count && i < holes.Length; i++)
				{
					holes[i] = sortedHolesWithinProximity[i];
				}
			}
			for (int j = i; j < holes.Length; j++)
			{
				holes[j] = null;
			}
		}
		return i;
	}

	public int GetSortedNearestDamagedTiles(ref DamagableTile[] tiles)
	{
		int i = 0;
		if (tiles != null)
		{
			Vector3 position = base.transform.position;
			if (m_Owner != null && m_Owner.m_bIsStandingOnDesk)
			{
				FloorManager instance = FloorManager.GetInstance();
				FloorManager.Floor floor = instance.UpAFloor(m_Owner.CurrentFloor);
				if (floor != m_Owner.CurrentFloor && floor.IsVent())
				{
					position.z = floor.m_zPos;
				}
			}
			List<DamagableTile> list = FloorManager.GetInstance().AdditiveDamagedTilesSearchWithinProximity(position, 2, clearCachedList: true, null);
			if (m_Owner.CurrentFloor.IsVent())
			{
				FloorManager.Floor floor2 = FloorManager.GetInstance().UpAFloor(m_Owner.CurrentFloor);
				FloorManager.GetInstance().AdditiveDamagedTilesSearchWithinProximity(new Vector3(position.x, position.y + 1f, floor2.m_zPos), 1, clearCachedList: false, DamagableTile.DamageAction.Unscrew);
			}
			FloorManager.GetInstance().SortMultiFlooredDamagedTileListForPosition(list, position);
			for (; i < list.Count && i < tiles.Length; i++)
			{
				tiles[i] = list[i];
			}
			for (int j = i; j < tiles.Length; j++)
			{
				tiles[j] = null;
			}
		}
		return i;
	}

	public int GetAnyItemContainers(ref List<ItemContainer> list)
	{
		list = m_Read_NearbyItemContainers;
		return list.Count;
	}

	public int GetAnyInteractiveObjects(ref List<NetObjectLock> list)
	{
		list = m_Read_NearbyNetObjectLocks;
		return list.Count;
	}

	public int GetAnyCharacters(ref List<TrackableUIElementsReporter> list)
	{
		list = m_Read_NearbyCharacters;
		return list.Count;
	}

	public ItemContainer GetNearestItemContainer()
	{
		return GetNearest(m_Read_NearbyItemContainers);
	}

	public NetObjectLock GetNearestInteractiveObject()
	{
		return GetNearest(m_Read_NearbyNetObjectLocks);
	}

	private T GetNearest<T>(List<T> objects)
	{
		int count = objects.Count;
		if (count == 0)
		{
			return default(T);
		}
		float num = float.MaxValue;
		int index = 0;
		for (int i = 0; i < count; i++)
		{
			MonoBehaviour monoBehaviour = (MonoBehaviour)(object)objects[i];
			float num2 = Vector3.Distance(monoBehaviour.transform.position, base.transform.position);
			if (num2 < num)
			{
				index = i;
				num = num2;
			}
		}
		return objects[index];
	}

	public IEnumerator SortListByDistance<T>(List<T> objects, Vector3 position)
	{
		if (objects.Count == 0)
		{
			yield return null;
		}
		objects.Sort(delegate(T p1, T p2)
		{
			MonoBehaviour monoBehaviour = (MonoBehaviour)(object)p1;
			MonoBehaviour monoBehaviour2 = (MonoBehaviour)(object)p2;
			float sqrMagnitude = (monoBehaviour.transform.position - base.transform.position).sqrMagnitude;
			float sqrMagnitude2 = (monoBehaviour2.transform.position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude > sqrMagnitude2)
			{
				return 1;
			}
			return (sqrMagnitude != sqrMagnitude2) ? (-1) : 0;
		});
		yield return null;
	}

	public IEnumerator SortListByDistance(List<TrackableUIElementsReporter> objects, Vector3 position)
	{
		if (objects.Count == 0)
		{
			yield return null;
		}
		objects.Sort(delegate(TrackableUIElementsReporter p1, TrackableUIElementsReporter p2)
		{
			if (p1.m_ProximityLayer < p2.m_ProximityLayer)
			{
				return -1;
			}
			if (p1.m_ProximityLayer > p2.m_ProximityLayer)
			{
				return 1;
			}
			float num = (p1.transform.position - base.transform.position).sqrMagnitude * p1.m_ProximityPenalty;
			float num2 = (p2.transform.position - base.transform.position).sqrMagnitude * p2.m_ProximityPenalty;
			if (num > num2)
			{
				return 1;
			}
			return (num != num2) ? (-1) : 0;
		});
		yield return null;
	}

	public bool SortListByDirection(List<TrackableUIElementsReporter> objects, Vector3 position, Vector2 facingDirection)
	{
		if (objects.Count == 0)
		{
			return false;
		}
		objects.Sort(delegate(TrackableUIElementsReporter p1, TrackableUIElementsReporter p2)
		{
			if (p1 == p2)
			{
				return 0;
			}
			if (p1 == null)
			{
				return 1;
			}
			if (p2 == null)
			{
				return -1;
			}
			Vector3 vector = p1.transform.position - base.transform.position;
			Vector3 vector2 = p2.transform.position - base.transform.position;
			float num = Vector2.Dot(facingDirection.normalized, ((Vector2)vector).normalized);
			float num2 = Vector2.Dot(facingDirection.normalized, ((Vector2)vector2).normalized);
			if (num > num2)
			{
				return -1;
			}
			return (num < num2) ? 1 : 0;
		});
		return true;
	}

	public void ClearAllNearbyItems()
	{
		m_Read_NearbyItems.Clear();
	}
}
