using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LevelSetup_LightPlacer : BaseComponentSetup
{
	[Flags]
	private enum Occupier
	{
		EMPTY = 0,
		Outside = 1,
		Wall = 2,
		Light = 4,
		Room = 8,
		Zone = 0x10,
		LightShift = 5,
		LightColorTotal = 0x20,
		InverseZone = -17
	}

	public class RingTable
	{
		public int m_Distance;

		public List<int> m_Position_X = new List<int>();

		public List<int> m_Position_Y = new List<int>();

		public List<int> m_IndexOffset = new List<int>();
	}

	public enum State
	{
		Start,
		Prep_ZoneLights,
		Processing_ZoneLights,
		Prep_LevelLights,
		Processing_LevelLights,
		Processing_Finished
	}

	[Serializable]
	public class LayerLight
	{
		[NonSerialized]
		public int m_Limit;

		public int m_Size;

		public int m_NearestAllowLight;

		public int m_NearestAllowedWall;

		public int m_NearestAllowOutside;

		public int m_NearestAllowedRoom;

		public GameObject m_LightObject;
	}

	[Serializable]
	public class LayerLightGroup
	{
		public LayerLight[] m_Lights = new LayerLight[0];
	}

	public BaseLevelManager m_LevelManager;

	public LevelEditor_ZoneManager m_ZoneManager;

	private Stopwatch m_StopWatch = new Stopwatch();

	private long m_TimeOut = 300L;

	private Occupier[] m_Map = new Occupier[0];

	private int[] m_ZoneMap = new int[0];

	private List<Occupier[]> m_Maps = new List<Occupier[]>();

	private List<RingTable> m_RingTables = new List<RingTable>();

	private State m_State;

	public LayerLightGroup[] m_LayerLightGroups = new LayerLightGroup[6]
	{
		new LayerLightGroup(),
		new LayerLightGroup(),
		new LayerLightGroup(),
		new LayerLightGroup(),
		new LayerLightGroup(),
		new LayerLightGroup()
	};

	private int m_CurrentLayer = 1;

	private int m_CurrentZone;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_3;
	}

	public override SetupReturnState Setup()
	{
		if (m_LevelManager == null)
		{
			return FinishedAndRemove();
		}
		m_StopWatch.Reset();
		m_StopWatch.Start();
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			switch (m_State)
			{
			case State.Start:
				InitializeMaps(bZonedMap: false);
				m_State = State.Prep_LevelLights;
				break;
			case State.Prep_LevelLights:
				m_CurrentLayer = 1;
				m_State = State.Processing_LevelLights;
				break;
			case State.Processing_LevelLights:
				if (ProcessingLayerLights())
				{
					m_State = State.Processing_Finished;
				}
				break;
			case State.Processing_Finished:
				m_StopWatch.Stop();
				return Finished();
			}
		}
		m_StopWatch.Stop();
		return TakeABreak();
	}

	public override SetupReturnState SetupV2()
	{
		if (m_ZoneManager == null && (m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null)
		{
			return FinishedAndRemove();
		}
		if (m_LevelManager == null)
		{
			return FinishedAndRemove();
		}
		m_StopWatch.Reset();
		m_StopWatch.Start();
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			switch (m_State)
			{
			case State.Start:
				InitializeMaps(bZonedMap: true);
				m_State = State.Prep_ZoneLights;
				break;
			case State.Prep_ZoneLights:
				m_CurrentZone = 0;
				m_State = State.Processing_ZoneLights;
				break;
			case State.Processing_ZoneLights:
				if (ProcessingZoneLights())
				{
					m_State = State.Prep_LevelLights;
				}
				break;
			case State.Prep_LevelLights:
				m_CurrentLayer = 1;
				m_State = State.Processing_LevelLights;
				break;
			case State.Processing_LevelLights:
				if (ProcessingZoneLights())
				{
					m_State = State.Processing_Finished;
				}
				break;
			case State.Processing_Finished:
				return Finished();
			}
		}
		return TakeABreak();
	}

	private bool ProcessingLayerLights()
	{
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			FillLayerWithLights(m_CurrentLayer++);
			if (m_CurrentLayer == 6)
			{
				return true;
			}
		}
		return false;
	}

	private bool ProcessingZoneLights()
	{
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			if (m_CurrentZone >= m_ZoneManager.GetTotalZones())
			{
				return true;
			}
			LevelEditor_ZoneManager.Zone zone = m_ZoneManager.GetZone(m_CurrentZone++);
			if (zone != null)
			{
				FillZoneWithLights(zone);
			}
		}
		return false;
	}

	private void InitializeMaps(bool bZonedMap)
	{
		m_Maps.Clear();
		for (int i = 0; i < 6; i++)
		{
			int[] array = new int[0];
			if (bZonedMap)
			{
				array = m_ZoneManager.GetZoneMap((BaseLevelManager.LevelLayers)i).m_Map;
			}
			Occupier[] array2 = new Occupier[14400];
			for (int j = 0; j < 14400; j++)
			{
				array2[j] = Occupier.EMPTY;
				BaseLevelManager.TileProperty tileProperty = m_LevelManager.m_BuildingLayers[i].m_TileProperties[j];
				if (bZonedMap && array[j] != -1)
				{
					Occupier[] array3;
					int num;
					(array3 = array2)[num = j] = array3[num] | Occupier.Zone;
				}
				if (m_LevelManager.m_BuildingLayers[i].m_RoomPropertiesMasks[j] != 0)
				{
					Occupier[] array3;
					int num2;
					(array3 = array2)[num2 = j] = array3[num2] | Occupier.Room;
				}
				if ((tileProperty & BaseLevelManager.TileProperty.TileMask) == 0 || (tileProperty & BaseLevelManager.TileProperty.EnvironmentMask) == 0)
				{
					Occupier[] array3;
					int num3;
					(array3 = array2)[num3 = j] = array3[num3] | Occupier.Outside;
				}
				if ((tileProperty & BaseLevelManager.TileProperty.WallMask) != 0)
				{
					Occupier[] array3;
					int num4;
					(array3 = array2)[num4 = j] = array3[num4] | Occupier.Wall;
				}
			}
			m_Maps.Add(array2);
		}
	}

	public void FillZoneWithLights(LevelEditor_ZoneManager.Zone zone)
	{
		if (zone == null || zone.m_ZoneDetails == null || !zone.m_bValid)
		{
			return;
		}
		m_ZoneMap = m_ZoneManager.GetZoneMap(zone.m_Layer).m_Map;
		m_Map = m_Maps[(int)zone.m_Layer];
		int num = 0;
		int num2 = zone.m_ZoneDetails.m_Lighting.Length;
		bool flag = false;
		for (int i = 0; i < num2; i++)
		{
			ZoneDetailsManager.LayerLight layerLight = zone.m_ZoneDetails.m_Lighting[i];
			if (layerLight != null && layerLight.m_bValid)
			{
				flag = true;
				if (layerLight.m_Limit == 0 && (layerLight.m_Type & ZoneDetailsManager.LayerLight.LightRule.DistanceFromStuff) == ZoneDetailsManager.LayerLight.LightRule.DistanceFromStuff)
				{
					layerLight.m_Limit = layerLight.m_NearestAllowedWall;
					layerLight.m_Limit = Mathf.Max(layerLight.m_Limit, layerLight.m_NearestAllowLight);
					layerLight.m_Limit = Mathf.Max(layerLight.m_Limit, layerLight.m_NearestAllowOutside);
					layerLight.m_Limit = Mathf.Max(layerLight.m_Limit, layerLight.m_NearestAllowedToEdge);
				}
				num = Mathf.Max(num, layerLight.m_Limit);
			}
		}
		if (!flag)
		{
			return;
		}
		if (num >= m_RingTables.Count)
		{
			CreateRingTables(num);
		}
		for (int j = 0; j < num2; j++)
		{
			ZoneDetailsManager.LayerLight layerLight2 = zone.m_ZoneDetails.m_Lighting[j];
			if (layerLight2 != null && layerLight2.m_bValid)
			{
				if ((layerLight2.m_Type & ZoneDetailsManager.LayerLight.LightRule.AboveBlockGroup) == ZoneDetailsManager.LayerLight.LightRule.AboveBlockGroup)
				{
					CreateZoneBlockLights(layerLight2, zone);
				}
				else
				{
					CreateZoneLights(layerLight2, zone, num);
				}
			}
		}
	}

	public void FillLayerWithLights(int iLayer)
	{
		m_Map = m_Maps[iLayer];
		int num = 0;
		int num2 = m_LayerLightGroups[iLayer].m_Lights.Length;
		for (int i = 0; i < num2; i++)
		{
			if (m_LayerLightGroups[iLayer].m_Lights[i] != null && m_LayerLightGroups[iLayer].m_Lights[i].m_LightObject != null)
			{
				if (m_LayerLightGroups[iLayer].m_Lights[i].m_Limit == 0)
				{
					m_LayerLightGroups[iLayer].m_Lights[i].m_Limit = m_LayerLightGroups[iLayer].m_Lights[i].m_NearestAllowedWall;
					m_LayerLightGroups[iLayer].m_Lights[i].m_Limit = Mathf.Max(m_LayerLightGroups[iLayer].m_Lights[i].m_Limit, m_LayerLightGroups[iLayer].m_Lights[i].m_NearestAllowLight);
					m_LayerLightGroups[iLayer].m_Lights[i].m_Limit = Mathf.Max(m_LayerLightGroups[iLayer].m_Lights[i].m_Limit, m_LayerLightGroups[iLayer].m_Lights[i].m_NearestAllowOutside);
				}
				num = Mathf.Max(num, m_LayerLightGroups[iLayer].m_Lights[i].m_Limit);
			}
		}
		if (num == 0)
		{
			return;
		}
		if (num >= m_RingTables.Count)
		{
			CreateRingTables(num);
		}
		for (int j = 0; j < num2; j++)
		{
			if (m_LayerLightGroups[iLayer].m_Lights[j] != null && m_LayerLightGroups[iLayer].m_Lights[j].m_LightObject != null)
			{
				CreateLights(m_LayerLightGroups[iLayer].m_Lights[j], m_LevelManager.m_BuildingLayers[iLayer].m_Objects.transform, num);
			}
		}
	}

	public void CreateZoneLights(ZoneDetailsManager.LayerLight theLight, LevelEditor_ZoneManager.Zone zone, int iLargest)
	{
		float z = theLight.m_LightObject.transform.localPosition.z;
		int left = zone.m_Left;
		int bottom = zone.m_Bottom;
		int num = left + zone.m_Width;
		int num2 = bottom + zone.m_Height;
		int num3 = bottom * 120 + left;
		int num4 = 120 - zone.m_Width;
		int iD = zone.m_ID;
		Transform parent = m_LevelManager.m_BuildingLayers[(uint)zone.m_Layer].m_Objects.transform;
		for (int i = bottom; i < num2; i++)
		{
			for (int j = left; j < num; j++)
			{
				if (m_ZoneMap[num3] == iD && AnythingWithinZone(j, i, num3, theLight, iD, bMark: true))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(theLight.m_LightObject, parent);
					gameObject.transform.localPosition = new Vector3((float)j - 59.5f, (float)i - 59.5f, z);
					LevelDetailsManager instance = LevelDetailsManager.GetInstance();
					if (instance != null)
					{
						instance.RegisterSetupComponent(gameObject.GetComponentInChildren<LevelSetup_Light>());
					}
				}
				num3++;
			}
			num3 += num4;
		}
	}

	public void CreateZoneBlockLights(ZoneDetailsManager.LayerLight theLight, LevelEditor_ZoneManager.Zone zone)
	{
		int count = zone.m_BlocksInZone.Count;
		if (count == 0)
		{
			return;
		}
		int blockGroupIndex = theLight.GetBlockGroupIndex();
		if (blockGroupIndex == -1)
		{
			return;
		}
		BuildingBlockGroupManager instance = BuildingBlockGroupManager.GetInstance();
		if (instance == null)
		{
			return;
		}
		Transform parent = m_LevelManager.m_BuildingLayers[(uint)zone.m_Layer].m_Objects.transform;
		float z = theLight.m_LightObject.transform.localPosition.z;
		for (int i = 0; i < count; i++)
		{
			if (!instance.IsBlockInGroup(zone.m_BlocksInZone[i].m_BlockID, blockGroupIndex))
			{
				continue;
			}
			int x = zone.m_BlocksInZone[i].m_X;
			int y = zone.m_BlocksInZone[i].m_Y;
			if ((theLight.m_Type & ZoneDetailsManager.LayerLight.LightRule.DistanceFromStuff) != ZoneDetailsManager.LayerLight.LightRule.DistanceFromStuff || AnythingWithinZone(x, y, y * 120 + x, theLight, zone.m_ID, bMark: false))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(theLight.m_LightObject, parent);
				gameObject.transform.localPosition = new Vector3((float)x - 59.5f, (float)y - 59.5f, z);
				LevelDetailsManager instance2 = LevelDetailsManager.GetInstance();
				if (instance2 != null)
				{
					instance2.RegisterSetupComponent(gameObject.GetComponentInChildren<LevelSetup_Light>());
				}
				int iIndex = y * 120 + x;
				MarkAreaAsLight(iIndex, y, x, theLight.m_Size);
			}
		}
	}

	public void CreateLights(LayerLight theLight, Transform lightParent, int iLargest)
	{
		float z = theLight.m_LightObject.transform.localPosition.z;
		int num = 0;
		for (int i = 0; i < 120; i++)
		{
			for (int j = 0; j < 120; j++)
			{
				if (AnythingWithin(j, i, num, theLight))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(theLight.m_LightObject, lightParent);
					gameObject.transform.localPosition = new Vector3((float)j - 59.5f, (float)i - 59.5f, z);
					LevelDetailsManager instance = LevelDetailsManager.GetInstance();
					if (instance != null)
					{
						instance.RegisterSetupComponent(gameObject.GetComponentInChildren<LevelSetup_Light>());
					}
				}
				num++;
			}
		}
	}

	private bool AnythingWithin(int iX, int iY, int iIndex, LayerLight theLight)
	{
		if (m_Map[iIndex] != 0)
		{
			return false;
		}
		int limit = theLight.m_Limit;
		for (int i = 1; i < limit; i++)
		{
			int count = m_RingTables[i].m_Position_X.Count;
			for (int j = 0; j < count; j++)
			{
				int num = m_RingTables[i].m_Position_X[j];
				int num2 = m_RingTables[i].m_Position_Y[j];
				if (iX + num < 0 || iX + num >= 120 || iY + num2 < 0 || iY + num2 >= 120)
				{
					return false;
				}
				Occupier occupier = m_Map[iIndex + m_RingTables[i].m_IndexOffset[j]];
				if (occupier != 0)
				{
					if ((occupier & Occupier.Light) != 0 && theLight.m_NearestAllowLight >= i)
					{
						return false;
					}
					if ((occupier & Occupier.Wall) != 0 && theLight.m_NearestAllowedWall >= i)
					{
						return false;
					}
					if ((occupier & Occupier.Room) != 0 && theLight.m_NearestAllowedRoom >= i)
					{
						return false;
					}
					if ((occupier & Occupier.Outside) != 0 && theLight.m_NearestAllowOutside >= i)
					{
						return false;
					}
				}
			}
		}
		MarkAreaAsLight(iIndex, iY, iX, theLight.m_Size);
		return true;
	}

	private bool AnythingWithinZone(int iX, int iY, int iIndex, ZoneDetailsManager.LayerLight theLight, int iZoneID, bool bMark)
	{
		if (((uint)m_Map[iIndex] & 0xFFFFFFEFu) != 0)
		{
			return false;
		}
		int limit = theLight.m_Limit;
		for (int i = 1; i < limit; i++)
		{
			int count = m_RingTables[i].m_Position_X.Count;
			for (int j = 0; j < count; j++)
			{
				int num = m_RingTables[i].m_Position_X[j];
				int num2 = m_RingTables[i].m_Position_Y[j];
				if (iX + num < 0 || iX + num >= 120 || iY + num2 < 0 || iY + num2 >= 120)
				{
					return false;
				}
				int num3 = iIndex + m_RingTables[i].m_IndexOffset[j];
				Occupier occupier = m_Map[num3];
				if (m_ZoneMap[num3] != iZoneID && theLight.m_NearestAllowedToEdge >= i)
				{
					return false;
				}
				if (((uint)occupier & 0xFFFFFFEFu) != 0)
				{
					if ((occupier & Occupier.Light) != 0 && theLight.m_NearestAllowLight >= i)
					{
						return false;
					}
					if ((occupier & Occupier.Wall) != 0 && theLight.m_NearestAllowedWall >= i)
					{
						return false;
					}
					if ((occupier & Occupier.Outside) != 0 && theLight.m_NearestAllowOutside >= i)
					{
						return false;
					}
				}
			}
		}
		if (bMark)
		{
			MarkAreaAsLight(iIndex, iX, iY, theLight.m_Size + 1);
		}
		return true;
	}

	private void MarkAreaAsLight(int iIndex, int iX, int iY, int iSize)
	{
		if (iSize >= m_RingTables.Count)
		{
			CreateRingTables(iSize);
		}
		for (int i = 0; i < iSize; i++)
		{
			int count = m_RingTables[i].m_Position_X.Count;
			for (int j = 0; j < count; j++)
			{
				int num = m_RingTables[i].m_Position_X[j];
				int num2 = m_RingTables[i].m_Position_Y[j];
				if (iX + num >= 0 && iX + num < 120 && iY + num2 >= 0 && iY + num2 < 120)
				{
					Occupier occupier = m_Map[iIndex + m_RingTables[i].m_IndexOffset[j]];
					occupier |= Occupier.Light;
					m_Map[iIndex + num + num2 * 120] = occupier;
				}
			}
		}
	}

	public void CreateRingTables(int iTotalRings)
	{
		m_RingTables.Clear();
		if (iTotalRings > 10 || iTotalRings < 0)
		{
			iTotalRings = 10;
		}
		int num = iTotalRings * 2 + 1;
		int num2 = num * num;
		int[] array = new int[num2];
		for (int num3 = iTotalRings; num3 >= 1; num3--)
		{
			int num4 = ((((uint)num3 & (true ? 1u : 0u)) != 0) ? num3 : (num3 - 1));
			int num5 = (iTotalRings - num3) * num;
			for (int num6 = num3; num6 >= 0; num6--)
			{
				int num7 = (num - num4) / 2;
				num5 += num7;
				for (int i = 0; i < num4; i++)
				{
					array[num2 - num5 - 1] = num3 + 1;
					array[num5++] = num3 + 1;
				}
				num5 += num7;
				num4 = Mathf.Min(num4 + 2, num3 * 2 + 1);
			}
		}
		array[iTotalRings * num + iTotalRings] = 1;
		for (int j = 0; j <= iTotalRings; j++)
		{
			RingTable ringTable = new RingTable();
			ringTable.m_Distance = j;
			for (int k = 0; k < num2; k++)
			{
				if (array[k] == j + 1)
				{
					int num8 = k % num - iTotalRings;
					int num9 = iTotalRings - k / num;
					ringTable.m_Position_X.Add(num8);
					ringTable.m_Position_Y.Add(num9);
					ringTable.m_IndexOffset.Add(num9 * 120 + num8);
				}
			}
			m_RingTables.Add(ringTable);
		}
	}
}
