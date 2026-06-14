using System.Collections.Generic;
using UnityEngine;

public class PrisonAlterationSaveFixer : MonoBehaviour
{
	public BoxCollider[] m_InValidTiles;

	public RoomBlob[] m_InValidRoom;

	public RoomWaypoint m_SafeWayPoint;

	private static PrisonAlterationSaveFixer m_Instance;

	public static PrisonAlterationSaveFixer GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		m_Instance = null;
	}

	public void RunAllChecks()
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		int count = allCharacters.Count;
		int num;
		if (m_InValidTiles != null)
		{
			num = m_InValidTiles.Length;
			for (int i = 0; i < num; i++)
			{
				if (!(m_InValidTiles[i] != null))
				{
					continue;
				}
				Vector3 center = m_InValidTiles[i].bounds.center;
				Vector3 size = m_InValidTiles[i].size;
				Vector3 center2 = m_InValidTiles[i].center;
				Vector3 vector = center + center2 - size;
				Vector3 vector2 = center + center2 + size;
				for (int j = 0; j < count; j++)
				{
					bool flag = false;
					Vector3 position = allCharacters[j].transform.position;
					if (allCharacters[j].IsPlayer())
					{
					}
					if (position.x > vector.x && position.x < vector2.x && position.y > vector.y && position.y < vector2.y && position.z > vector.z && position.z < vector2.z)
					{
						flag = true;
					}
					if (flag)
					{
						RepositionCharacter(allCharacters[j]);
					}
				}
			}
		}
		if (m_InValidRoom == null)
		{
			return;
		}
		num = m_InValidRoom.Length;
		for (int j = 0; j < count; j++)
		{
			RoomBlob currentLocation = allCharacters[j].m_CurrentLocation;
			if (!(currentLocation != null))
			{
				continue;
			}
			for (int i = 0; i < num; i++)
			{
				if (m_InValidRoom[i] != null && m_InValidRoom[i] == currentLocation)
				{
					RepositionCharacter(allCharacters[j]);
					break;
				}
			}
		}
	}

	private void RepositionCharacter(Character character)
	{
		switch (character.m_CharacterRole)
		{
		case CharacterRole.Inmate:
		{
			RoomBlob myCell = character.GetMyCell();
			if (myCell != null)
			{
				RoomBlob_Cell roomBlobData = myCell.GetRoomBlobData<RoomBlob_Cell>();
				if (roomBlobData != null)
				{
					SpawnPoint spawnPointForCharacter = roomBlobData.GetSpawnPointForCharacter(character);
					character.Teleport(spawnPointForCharacter.transform.position);
				}
			}
			break;
		}
		case CharacterRole.Guard:
			if (m_SafeWayPoint != null)
			{
				character.Teleport(m_SafeWayPoint.GetPosition());
			}
			break;
		default:
			if (m_SafeWayPoint != null)
			{
				character.Teleport(m_SafeWayPoint.GetPosition());
			}
			break;
		}
	}
}
