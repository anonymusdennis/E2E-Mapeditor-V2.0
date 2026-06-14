using UnityEngine;

public class LevelSetup_ExtraCollision : BaseComponentSetup
{
	public GameObject m_CollisionPrefab;

	public GameObject[] m_CollisionParents = new GameObject[6];

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_3;
	}

	public override SetupReturnState Setup()
	{
		if (m_CollisionPrefab == null)
		{
			return FinishedAndRemove();
		}
		sbyte[] map = new sbyte[14400];
		sbyte b = (sbyte)(m_CollisionParents.Length - 1);
		for (sbyte b2 = 0; b2 < b; b2++)
		{
			if (m_CollisionParents[b2] != null && m_CollisionParents[b2].transform.parent != null)
			{
				CollisionMarker[] componentsInChildren = m_CollisionParents[b2].transform.parent.GetComponentsInChildren<CollisionMarker>(includeInactive: true);
				if (componentsInChildren.Length != 0)
				{
					for (int num = componentsInChildren.Length - 1; num >= 0; num--)
					{
						Vector3 localPosition = componentsInChildren[num].transform.localPosition;
						int num2 = ((int)(localPosition.y - 0.5f) + 120) * 120 + (int)(localPosition.x - 0.5f);
						map[num2] = b2;
						Object.Destroy(componentsInChildren[num]);
					}
					int num3 = 0;
					int num4 = 0;
					int num5 = 0;
					for (int i = 0; i < 120; i++)
					{
						for (int j = 0; j < 120; j++)
						{
							if (map[num3++] == b2)
							{
								num4 = ScanAcross(ref map, b2, j, i);
								for (num5 = 1; i + num5 < 120 && ScanAcross(ref map, b2, j, i + num5, num4) == num4; num5++)
								{
								}
								ClearArea(ref map, j, i, num4, num5);
								GameObject gameObject = Object.Instantiate(m_CollisionPrefab, m_CollisionParents[b2].transform);
								BoxCollider component = gameObject.GetComponent<BoxCollider>();
								if (component != null)
								{
									float num6 = num4;
									float num7 = num5;
									component.size = new Vector3(num6, num7, 1f);
									float num8 = j;
									float num9 = i - 120;
									gameObject.transform.localPosition = new Vector3(num8 + num6 / 2f, num9 + num7 / 2f, 0.5f);
								}
							}
						}
					}
				}
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	private int ScanAcross(ref sbyte[] map, sbyte value, int iX, int iY, int iLimit = 120)
	{
		int num = iY * 120 + iX;
		int num2 = Mathf.Min(iLimit, 120 - iX);
		int i;
		for (i = 0; i < num2; i++)
		{
			if (map[num++] != value)
			{
				break;
			}
		}
		return i;
	}

	private void ClearArea(ref sbyte[] map, int iX, int iY, int iWidth, int iHeight)
	{
		int num = iY * 120 + iX;
		int num2 = 120 - iWidth;
		for (int i = 0; i < iHeight; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				map[num++] = -1;
			}
			num += num2;
		}
	}
}
