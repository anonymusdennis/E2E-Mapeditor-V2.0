using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomUtility : MonoBehaviour
{
	[Serializable]
	public class LocationRoomBlobMap
	{
		public RoomBlob.eLocation m_Location;

		public UnityEngine.Object data;

		public bool m_ShowMapIcon;

		public Sprite m_DefaultMapIcon;

		public string m_DefaultMapToolTip = string.Empty;
	}

	public Dictionary<int, float> m_Connections = new Dictionary<int, float>();

	public List<ConnectionData> m_ConnectionData;

	private ushort m_pathID;

	[SerializeField]
	public List<Vector3> m_InmateSafeSpaces = new List<Vector3>();

	[SerializeField]
	public List<Vector3> m_GuardSafeSpaces = new List<Vector3>();

	[SerializeField]
	public List<Vector3> m_SupportSafeSpaces = new List<Vector3>();

	public List<LocationRoomBlobMap> map = new List<LocationRoomBlobMap>();

	private static RoomUtility m_Instance;

	public static RoomUtility GetInstance()
	{
		return m_Instance;
	}

	private void Start()
	{
		m_Instance = this;
		m_Connections = new Dictionary<int, float>();
		for (int i = 0; i < m_ConnectionData.Count; i++)
		{
			ConnectionData connectionData = m_ConnectionData[i];
			m_Connections[connectionData.ID] = connectionData.ConnectionCost;
		}
	}

	private void OnEnable()
	{
		m_Instance = this;
	}

	public void Load()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		m_Instance = null;
	}

	public bool GetRandomGuardNodePosition(int startIndex, int endIndex, ref Vector3 pos)
	{
		return GetRandomNodePosition(ref m_GuardSafeSpaces, startIndex, endIndex, ref pos);
	}

	public bool GetRandomInmateNodePosition(int startIndex, int endIndex, ref Vector3 pos)
	{
		return GetRandomNodePosition(ref m_InmateSafeSpaces, startIndex, endIndex, ref pos);
	}

	public bool GetRandomSupportNodePosition(int startIndex, int endIndex, ref Vector3 pos)
	{
		return GetRandomNodePosition(ref m_SupportSafeSpaces, startIndex, endIndex, ref pos);
	}

	public bool GetRandomNodePosition(ref List<Vector3> list, int startIndex, int endIndex, ref Vector3 pos)
	{
		if (list.Count == 0)
		{
			return false;
		}
		if (startIndex < 0 || endIndex > list.Count || startIndex == endIndex)
		{
			return false;
		}
		pos = list[UnityEngine.Random.Range(startIndex, endIndex)];
		return true;
	}

	public UnityEngine.Object GetDataForLocation(RoomBlob.eLocation loc)
	{
		for (int i = 0; i < map.Count; i++)
		{
			if (loc == map[i].m_Location)
			{
				return map[i].data;
			}
		}
		return null;
	}

	public LocationRoomBlobMap GetDataMapForLocation(RoomBlob.eLocation loc)
	{
		for (int i = 0; i < map.Count; i++)
		{
			if (loc == map[i].m_Location)
			{
				return map[i];
			}
		}
		return null;
	}

	private void OnDrawGizmosSelected()
	{
		for (int i = 0; i < m_InmateSafeSpaces.Count; i++)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(m_InmateSafeSpaces[i], 0.25f);
		}
		for (int j = 0; j < m_GuardSafeSpaces.Count; j++)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_GuardSafeSpaces[j], 0.25f);
		}
		for (int k = 0; k < m_SupportSafeSpaces.Count; k++)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(m_SupportSafeSpaces[k], 0.25f);
		}
	}

	public static Vector3 WorldToRoomGrid(Vector3 pos, RoomFloor roomFloor)
	{
		float num = (float)roomFloor.m_FloorWidth / 2f;
		float num2 = (float)roomFloor.m_FloorHeight / 2f;
		return new Vector3(pos.x + num, num2 - pos.y, pos.z);
	}

	public static Vector3 RoomGridToWorld(Vector3 pos, RoomFloor roomFloor)
	{
		float num = (float)roomFloor.m_FloorWidth / 2f;
		float num2 = (float)roomFloor.m_FloorHeight / 2f;
		return new Vector3(pos.x - num, num2 - pos.y, pos.z);
	}

	public float GetDistanceEstimate(ref RoomBlob roomA, ref RoomBlob roomB)
	{
		if (roomA == null || roomB == null)
		{
			return float.MaxValue;
		}
		if (roomA == roomB)
		{
			return 0f;
		}
		return RoomAStar(ref roomA, ref roomB);
	}

	public bool HasConnection(ref RoomBlob a, ref RoomBlob b)
	{
		return m_Connections.ContainsKey(a.m_ID ^ b.m_ID);
	}

	public float RoomAStar(ref RoomBlob a, ref RoomBlob b)
	{
		if (a == null || b == null)
		{
			return float.MaxValue;
		}
		if (m_pathID <= ushort.MaxValue)
		{
			m_pathID++;
		}
		else
		{
			m_pathID = 0;
		}
		ResetNode(ref a);
		a.m_ACostSoFar = 0f;
		List<RoomBlob> list = new List<RoomBlob>();
		list.Add(a);
		while (list.Count > 0)
		{
			RoomBlob a2 = list[0];
			float num = float.MaxValue;
			int index = 0;
			for (int i = 0; i < list.Count; i++)
			{
				RoomBlob roomBlob = list[i];
				float num2 = roomBlob.m_ACostSoFar + Vector3.Distance(roomBlob.position, b.position);
				if (num2 < num)
				{
					index = i;
					a2 = roomBlob;
					num = num2;
				}
			}
			list.RemoveAt(index);
			a2.m_AVisited = true;
			if (a2.Equals(b))
			{
				break;
			}
			for (int j = 0; j < a2.m_ARoomConnections.Count; j++)
			{
				RoomBlob node = a2.m_ARoomConnections[j];
				if (node.m_APathID != m_pathID)
				{
					ResetNode(ref node);
				}
				float num3 = a2.m_ACostSoFar + GetCost(ref a2, ref node);
				if (!node.m_AVisited && num3 < node.m_ACostSoFar)
				{
					node.m_ACostSoFar = num3;
					list.Add(node);
				}
			}
		}
		if (b.m_APathID != m_pathID)
		{
			ResetNode(ref b);
		}
		if (b.m_AVisited)
		{
			return b.m_ACostSoFar;
		}
		return float.MaxValue;
	}

	public void ResetNode(ref RoomBlob node)
	{
		node.m_ACostSoFar = float.MaxValue;
		node.m_APathID = m_pathID;
		node.m_AVisited = false;
	}

	public float GetCost(ref RoomBlob a, ref RoomBlob b)
	{
		if (a == null || b == null)
		{
			return float.MaxValue;
		}
		int key = a.m_ID ^ b.m_ID;
		if (!m_Connections.ContainsKey(key))
		{
			return 500f;
		}
		return m_Connections[key];
	}

	public void SetConnection(ref RoomBlob a, ref RoomBlob b, float value)
	{
		int key = a.m_ID ^ b.m_ID;
		m_Connections[key] = value;
	}

	public void SerialiseConnections()
	{
		m_ConnectionData.Clear();
		foreach (KeyValuePair<int, float> connection in m_Connections)
		{
			ConnectionData connectionData = new ConnectionData();
			connectionData.ConnectionCost = connection.Value;
			connectionData.ID = connection.Key;
			m_ConnectionData.Add(connectionData);
		}
	}
}
