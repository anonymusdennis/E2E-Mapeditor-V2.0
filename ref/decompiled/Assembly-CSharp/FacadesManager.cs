using System.Collections.Generic;
using UnityEngine;

public class FacadesManager : MonoBehaviour
{
	private class FacadeCameraData
	{
		public int m_CurrentInBuildingID;

		public float m_SwitchTimer;

		public int m_SwitchToBuildingID;
	}

	[ReadOnly]
	public List<FacadeFloor> m_FacadeFloors = new List<FacadeFloor>();

	public int defaultWidth = 120;

	public int defaultHeight = 120;

	private Dictionary<int, FacadeCameraData> m_CameraData = new Dictionary<int, FacadeCameraData>();

	private static FacadesManager m_Instance;

	public static FacadesManager GetInstance()
	{
		return m_Instance;
	}

	public static void SetToolsInstance(FacadesManager instance)
	{
		m_Instance = instance;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void Init()
	{
		LoadFloors(FloorManager.GetInstance().GetFloors());
	}

	public void LoadFloors(FloorManager.Floor[] floors)
	{
		bool flag = false;
		int width = 0;
		int height = 0;
		m_FacadeFloors = new List<FacadeFloor>();
		FacadeFloor[] componentsInChildren = base.gameObject.GetComponentsInChildren<FacadeFloor>();
		for (int i = 0; i < floors.Length; i++)
		{
			if (!flag && floors[i].m_TileSystems != null && floors[i].m_TileSystems.Length > 1 && floors[i].m_TileSystems[0] != null)
			{
				width = floors[i].m_TileSystems[0].ColumnCount;
				height = floors[i].m_TileSystems[0].RowCount;
				flag = true;
			}
			bool flag2 = false;
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].name == floors[i].m_FloorName)
				{
					componentsInChildren[j].transform.localPosition = floors[i].m_FloorRootObject.position;
					m_FacadeFloors.Add(componentsInChildren[j]);
					flag2 = true;
				}
			}
			if (flag2 || floors[i].m_FloorName.Contains("Underground"))
			{
				continue;
			}
			GameObject gameObject = new GameObject();
			gameObject.name = floors[i].m_FloorName;
			gameObject.transform.parent = base.transform;
			if (floors[i].m_FloorRootObject != null)
			{
				gameObject.transform.localPosition = floors[i].m_FloorRootObject.position;
				FacadeFloor facadeFloor = gameObject.AddComponent<FacadeFloor>();
				if (flag)
				{
					facadeFloor.SetDims(width, height);
					defaultWidth = width;
					defaultHeight = height;
				}
				m_FacadeFloors.Add(facadeFloor);
			}
		}
	}

	public int NumFloors()
	{
		return m_FacadeFloors.Count;
	}

	public FacadeFloor GetFloorFromIndex(int index)
	{
		if (index >= 0 && index < m_FacadeFloors.Count)
		{
			return m_FacadeFloors[index];
		}
		return null;
	}

	public int LookUpCell(int x, int y, FacadeFloor floor)
	{
		if (floor != null)
		{
			return floor.FloorMap(x, y);
		}
		return 0;
	}

	public void SetFacadeInFloor(int x, int y, int val, FacadeFloor floor)
	{
		floor.FloorMap(x, y, val);
	}

	public string[] GetNameList()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < m_FacadeFloors.Count; i++)
		{
			if (m_FacadeFloors[i] != null)
			{
				list.Add(m_FacadeFloors[i].name);
			}
		}
		return list.ToArray();
	}

	public FacadeFloor GetFloorFromZ(float zPos)
	{
		FacadeFloor result = null;
		for (int num = m_FacadeFloors.Count - 1; num >= 0; num--)
		{
			result = m_FacadeFloors[num];
			if (zPos <= m_FacadeFloors[num].transform.position.z)
			{
				break;
			}
		}
		return result;
	}

	public bool ShouldShowFacade(int cameraID, Vector3 pos, bool bAdvanceTimers)
	{
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(pos.z);
		if (floor != null && floor.IsUnderGround())
		{
			return false;
		}
		FacadeCameraData cameraData = null;
		GetCameraData(cameraID, ref cameraData);
		if (bAdvanceTimers)
		{
			int switchToBuildingID = CalculateFacadeBuildingId(pos);
			cameraData.m_SwitchToBuildingID = switchToBuildingID;
			if (cameraData.m_SwitchTimer <= 0f)
			{
				if (cameraData.m_CurrentInBuildingID != cameraData.m_SwitchToBuildingID)
				{
					cameraData.m_CurrentInBuildingID = cameraData.m_SwitchToBuildingID;
					cameraData.m_SwitchTimer = 1f;
				}
			}
			else
			{
				cameraData.m_SwitchTimer -= UpdateManager.deltaTime;
			}
		}
		if (cameraData.m_CurrentInBuildingID == 0)
		{
			return true;
		}
		return false;
	}

	public int CalculateFacadeBuildingId(Vector3 pos, float widthOffset = 0.5f)
	{
		FacadeFloor floorFromZ = GetFloorFromZ(pos.z);
		float num = (float)floorFromZ.m_FloorWidth * widthOffset;
		float num2 = (float)floorFromZ.m_FloorHeight * widthOffset;
		pos = new Vector3(pos.x + num, num2 - pos.y, pos.z);
		return LookUpCell((int)pos.x, (int)pos.y, floorFromZ);
	}

	public int GetCurrentBuildingID(int cameraID)
	{
		FacadeCameraData cameraData = null;
		GetCameraData(cameraID, ref cameraData);
		return cameraData.m_CurrentInBuildingID;
	}

	public int GetBuildingIDAtPos(Vector3 pos)
	{
		FacadeFloor floorFromZ = GetFloorFromZ(pos.z);
		float num = (float)floorFromZ.m_FloorWidth / 2f;
		float num2 = (float)floorFromZ.m_FloorHeight / 2f;
		pos = new Vector3(pos.x + num, num2 - pos.y, pos.z);
		return LookUpCell((int)pos.x, (int)pos.y, floorFromZ);
	}

	private void GetCameraData(int cameraID, ref FacadeCameraData cameraData)
	{
		if (!m_CameraData.ContainsKey(cameraID))
		{
			m_CameraData.Add(cameraID, new FacadeCameraData());
		}
		cameraData = m_CameraData[cameraID];
	}
}
