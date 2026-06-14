using UnityEngine;

public class FacadeFloor : MonoBehaviour
{
	public const int kFloorWidth = 120;

	public const int kFloorHeight = 120;

	public int[] m_FloorMap = new int[14400];

	[ReadOnly]
	public int m_FloorWidth = 120;

	[ReadOnly]
	public int m_FloorHeight = 120;

	public void SetDims(int width, int height)
	{
		m_FloorMap = new int[width * height];
		m_FloorWidth = width;
		m_FloorHeight = height;
	}

	public int FloorMap(int x, int y)
	{
		int num = x * m_FloorHeight + y;
		if (num > 0 && num < m_FloorMap.Length)
		{
			return m_FloorMap[num];
		}
		return 0;
	}

	public void FloorMap(int x, int y, int val)
	{
		int num = x * m_FloorHeight + y;
		if (num > 0 && num < m_FloorMap.Length)
		{
			m_FloorMap[num] = val;
		}
	}
}
