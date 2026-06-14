using System.Collections.Generic;
using UnityEngine;

public class ZoneSetup_MealHall : BaseZoneSetup
{
	public RoomWaypoint m_UpWaypoint;

	public RoomWaypoint m_DownWaypoint;

	public RoomWaypoint m_LeftWaypoint;

	public RoomWaypoint m_RightWaypoint;

	private BaseLevelManager m_LevelMan;

	private int[] m_MyZoneMap = new int[0];

	private LevelEditor_ZoneManager.Zone m_MyZone;

	private BaseLevelManager.TileProperty[] m_MyLayerData;

	private List<int> m_Added = new List<int>();

	public override void SetupZone(LevelEditor_ZoneManager.Zone myZone, LevelEditor_ZoneManager.Zone.ObjectsInZone objInZone, ref List<Object> tempZoneData)
	{
		m_LevelMan = BaseLevelManager.GetInstance();
		if (!(m_LevelMan == null))
		{
			LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
			if (!(instance == null) && !(m_UpWaypoint == null) && !(m_DownWaypoint == null) && !(m_LeftWaypoint == null) && !(m_RightWaypoint == null))
			{
				CreateWaypoints(instance, myZone, objInZone);
			}
			m_MyZoneMap = new int[0];
			m_MyZone = null;
			m_MyLayerData = null;
			m_Added.Clear();
		}
	}

	private void CreateWaypoints(LevelEditor_ZoneManager zoneManager, LevelEditor_ZoneManager.Zone myZone, LevelEditor_ZoneManager.Zone.ObjectsInZone objInZone)
	{
		m_MyZone = myZone;
		m_MyLayerData = m_LevelMan.m_BuildingLayers[(uint)myZone.m_Layer].m_TileProperties;
		m_MyZoneMap = zoneManager.GetZoneMap(myZone.m_Layer).m_Map;
		List<int> list = new List<int>();
		for (int num = objInZone.m_InteractPoints.Count - 1; num >= 0; num--)
		{
			if ((m_MyLayerData[objInZone.m_InteractPoints[num]] & BaseLevelManager.TileProperty.CanReachRollCallMask) == BaseLevelManager.TileProperty.CanReachRollCallMask)
			{
				list.Add(objInZone.m_InteractPoints[num]);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		int[] array = new int[2] { 1, -1 };
		int num2 = -1;
		int num3 = 10000;
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				int num4 = 0;
				for (int k = list[i]; k < 14400 && k >= 0 && m_MyZoneMap[k] == myZone.m_ID; k += 120 * array[j])
				{
					num4++;
				}
				if (num4 < num3)
				{
					num2 = i;
					num3 = num4;
				}
				else if (num4 == num3 && array[j] == 1)
				{
					num2 = i;
				}
			}
		}
		if (num2 == -1)
		{
			return;
		}
		if (list.Count == 2)
		{
			num2 = ((num2 == 0) ? 1 : 0);
		}
		bool flag = list[num2] / 120 > objInZone.m_Y + m_MyZone.m_Bottom;
		int iPreviousX = objInZone.m_X + m_MyZone.m_Left;
		int iPreviousY = objInZone.m_Y + m_MyZone.m_Bottom;
		int iPoint = list[num2];
		if (!CreateWayPointObject(iPoint, list[num2] % 120, list[num2] / 120, objInZone.m_Object, ref iPreviousX, ref iPreviousY, ref iPoint))
		{
		}
		int num5 = 14;
		while (num5 > 0)
		{
			if (CreateWayPointObject(iPoint + 1, iPreviousX + 1, iPreviousY, objInZone.m_Object, ref iPreviousX, ref iPreviousY, ref iPoint))
			{
				num5--;
				continue;
			}
			if (flag && CreateWayPointObject(iPoint + 120, iPreviousX, iPreviousY + 1, objInZone.m_Object, ref iPreviousX, ref iPreviousY, ref iPoint))
			{
				num5--;
				continue;
			}
			if (!flag && CreateWayPointObject(iPoint - 120, iPreviousX, iPreviousY - 1, objInZone.m_Object, ref iPreviousX, ref iPreviousY, ref iPoint))
			{
				num5--;
				continue;
			}
			if (CreateWayPointObject(iPoint - 1, iPreviousX - 1, iPreviousY, objInZone.m_Object, ref iPreviousX, ref iPreviousY, ref iPoint))
			{
				num5--;
				continue;
			}
			break;
		}
		LevelSetup_ObjectMover component = objInZone.m_Object.GetComponent<LevelSetup_ObjectMover>();
		if (component != null)
		{
			if (flag)
			{
				component.m_GameObjectChange.y = 0.1f;
			}
			else
			{
				component.m_GameObjectChange.y = -0.1f;
			}
		}
	}

	private bool CreateWayPointObject(int iIndex, int iX, int iY, GameObject parent, ref int iPreviousX, ref int iPreviousY, ref int iPoint)
	{
		if (iX < 0 || iX >= 120 || iY < 0 || iY >= 120)
		{
			return false;
		}
		if (m_MyZoneMap[iIndex] != m_MyZone.m_ID)
		{
			return false;
		}
		if ((m_MyLayerData[iIndex] & (BaseLevelManager.TileProperty.Blocked_All_Mask | BaseLevelManager.TileProperty.BlockingMask)) != 0)
		{
			return false;
		}
		if (m_Added.Contains(iIndex))
		{
			return false;
		}
		GameObject gameObject = null;
		if (iX > iPreviousX)
		{
			gameObject = Object.Instantiate(m_LeftWaypoint.gameObject, parent.transform.parent);
		}
		else if (iX < iPreviousX)
		{
			gameObject = Object.Instantiate(m_RightWaypoint.gameObject, parent.transform.parent);
		}
		else if (iY > iPreviousY)
		{
			gameObject = Object.Instantiate(m_DownWaypoint.gameObject, parent.transform.parent);
		}
		else if (iY < iPreviousY)
		{
			gameObject = Object.Instantiate(m_UpWaypoint.gameObject, parent.transform.parent);
		}
		if (gameObject != null)
		{
			float num = iX;
			float num2 = iY;
			float x = num + m_LevelMan.m_fPositionOffsetsX[4];
			float y = num2 + m_LevelMan.m_fPositionOffsetsY[4];
			gameObject.transform.localPosition = new Vector3(x, y, 0f);
			LevelEditor_ZoneManager.Zone.ObjectsInZone objectsInZone = new LevelEditor_ZoneManager.Zone.ObjectsInZone();
			objectsInZone.m_Object = gameObject;
			objectsInZone.m_BeingBlocked = false;
			objectsInZone.m_X = iX;
			objectsInZone.m_Y = iY;
			m_MyZone.m_BlocksInZone.Add(objectsInZone);
			iPreviousX = iX;
			iPreviousY = iY;
			iPoint = iY * 120 + iX;
			m_Added.Add(iPoint);
			return true;
		}
		return false;
	}
}
