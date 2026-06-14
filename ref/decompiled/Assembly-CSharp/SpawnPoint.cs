using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public static int c_HighestSpawnPoint = 10;

	public List<ItemData> m_StartingItems = new List<ItemData>();

	public DeskInteraction m_AttachedDesk;

	public BedInteraction m_AttachedBed;

	public ToiletInteraction m_AttachedToilet;

	public CalendarInteraction m_AttachedCalendar;

	[ReadOnly]
	public int m_SpawnPointID = -999;

	private Character m_CharacterToAddTo;

	private RoomBlob m_RoomBlob;

	private List<int> m_ItemMgrResponseIDs = new List<int>();

	private int m_ImmediateItemMgrResponseID = -1;

	public RoomBlob MyRoomBlob => m_RoomBlob;

	private void Awake()
	{
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			Object.Destroy(component);
		}
	}

	private void Start()
	{
		if (!(GetComponent<SpriteRenderer>() != null))
		{
		}
	}

	public Character GetCharacterOwner()
	{
		return m_CharacterToAddTo;
	}

	public void SetCharacterOwner(Character character)
	{
		m_CharacterToAddTo = character;
		if (m_AttachedDesk != null)
		{
			m_AttachedDesk.SetOwner(m_CharacterToAddTo);
		}
		if (m_AttachedBed != null)
		{
			m_AttachedBed.SetOwner(m_CharacterToAddTo);
			m_CharacterToAddTo.SetMyBed(m_AttachedBed);
		}
		if (m_AttachedCalendar != null)
		{
			m_AttachedCalendar.SetOwner(m_CharacterToAddTo);
		}
	}

	public void SetRoomblobIBelongTo(RoomBlob blob)
	{
		m_RoomBlob = blob;
	}

	public void AddStartingItems()
	{
		if (!(m_CharacterToAddTo.m_NetView != null) || m_CharacterToAddTo.m_NetView.ownerId != T17NetManager.PhotonPlayerID)
		{
			return;
		}
		m_ItemMgrResponseIDs.Clear();
		for (int i = 0; i < m_StartingItems.Count; i++)
		{
			if (m_StartingItems[i] != null)
			{
				m_ItemMgrResponseIDs.Add(ItemManager.GetInstance().AssignItemRPC(m_CharacterToAddTo.m_NetView.ownerId, m_StartingItems[i].m_ItemDataID, OnItemMgrResponseAddToCharacter, ref m_ImmediateItemMgrResponseID));
			}
		}
	}

	private void OnItemMgrResponseAddToCharacter(Item item, int eventID)
	{
		if (!(m_CharacterToAddTo == null) && item != null && (eventID == m_ImmediateItemMgrResponseID || m_ItemMgrResponseIDs.Contains(eventID)) && !m_CharacterToAddTo.m_ItemContainer.AddItemRPC(item))
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
	}

	private void OnDrawGizmos()
	{
		if (m_AttachedBed != null)
		{
			Gizmos.DrawLine(base.transform.position, m_AttachedBed.transform.position);
		}
		if (m_AttachedDesk != null)
		{
			Gizmos.DrawLine(base.transform.position, m_AttachedDesk.transform.position);
		}
		if (m_AttachedToilet != null)
		{
			Gizmos.DrawLine(base.transform.position, m_AttachedToilet.transform.position);
		}
	}
}
