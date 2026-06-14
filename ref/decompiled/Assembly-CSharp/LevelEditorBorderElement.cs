using System.Collections.Generic;
using UnityEngine;

public class LevelEditorBorderElement : MonoBehaviour
{
	public enum BorderState
	{
		INVALID,
		None,
		Animating,
		Freeze,
		Blackout,
		Flash,
		Red,
		Zone
	}

	public const float m_AnimDelay = 0.4f;

	public const float m_FlashOn = 0.8f;

	public const float m_FlashOff = 0.5f;

	public BorderState m_State;

	private float m_TimeOfNextChange;

	public int m_FrameCount;

	private int m_FrameNumber;

	public MeshRenderer[] m_MeshRender = new MeshRenderer[0];

	private void Start()
	{
	}

	private void OnEnable()
	{
		m_TimeOfNextChange = Time.realtimeSinceStartup;
	}

	public void InitializeBorder(BorderState startingState, int iFrames, params Material[] values)
	{
		if (values.Length != iFrames)
		{
			base.enabled = false;
			return;
		}
		if (iFrames != m_MeshRender.Length)
		{
			base.enabled = false;
			return;
		}
		m_FrameCount = iFrames;
		for (int i = 0; i < m_FrameCount; i++)
		{
			if (m_MeshRender[i] != null)
			{
				m_MeshRender[i].material = values[i];
			}
		}
		SetState(BorderState.INVALID);
		SetState(startingState);
	}

	private void Update()
	{
		if (m_MeshRender.Length != m_FrameCount)
		{
			base.enabled = false;
			return;
		}
		switch (m_State)
		{
		case BorderState.INVALID:
		case BorderState.None:
		case BorderState.Freeze:
		case BorderState.Blackout:
		case BorderState.Red:
		case BorderState.Zone:
			break;
		case BorderState.Animating:
			if (m_TimeOfNextChange < Time.realtimeSinceStartup)
			{
				m_TimeOfNextChange += 0.4f;
				if (m_TimeOfNextChange < Time.realtimeSinceStartup)
				{
					m_TimeOfNextChange = Time.realtimeSinceStartup + 0.4f;
				}
				m_FrameNumber = (m_FrameNumber + 1) % m_FrameCount;
				for (int j = 0; j < m_FrameCount; j++)
				{
					m_MeshRender[j].enabled = j == m_FrameNumber;
				}
			}
			break;
		case BorderState.Flash:
		{
			if (!(m_TimeOfNextChange < Time.realtimeSinceStartup))
			{
				break;
			}
			m_FrameNumber = (m_FrameNumber + 1) % 2;
			for (int i = 0; i < m_FrameCount; i++)
			{
				m_MeshRender[i].enabled = m_FrameNumber == 0;
			}
			if (m_FrameNumber == 0)
			{
				m_TimeOfNextChange += 0.8f;
				if (m_TimeOfNextChange < Time.realtimeSinceStartup)
				{
					m_TimeOfNextChange = Time.realtimeSinceStartup + 0.8f;
				}
			}
			else
			{
				m_TimeOfNextChange += 0.5f;
				if (m_TimeOfNextChange < Time.realtimeSinceStartup)
				{
					m_TimeOfNextChange = Time.realtimeSinceStartup + 0.5f;
				}
			}
			break;
		}
		}
	}

	public void SetState(BorderState newState)
	{
		if (m_State == newState)
		{
			return;
		}
		if (m_State == BorderState.Red)
		{
			for (int i = 0; i < m_FrameCount; i++)
			{
				Material material = m_MeshRender[i].material;
				Color color = material.color;
				color.r = 1f;
				color.g = 1f;
				color.b = 1f;
				material.color = color;
			}
		}
		m_State = newState;
		switch (m_State)
		{
		case BorderState.None:
		{
			for (int num = 0; num < m_FrameCount; num++)
			{
				m_MeshRender[num].enabled = false;
			}
			break;
		}
		case BorderState.Animating:
		{
			m_TimeOfNextChange = Time.realtimeSinceStartup + 0.4f;
			for (int k = 0; k < m_FrameCount; k++)
			{
				m_MeshRender[k].enabled = k == m_FrameNumber;
			}
			break;
		}
		case BorderState.Freeze:
		{
			for (int m = 0; m < m_FrameCount; m++)
			{
				m_MeshRender[m].enabled = m == m_FrameNumber;
			}
			break;
		}
		case BorderState.Red:
		{
			for (int num2 = 0; num2 < m_FrameCount; num2++)
			{
				m_MeshRender[num2].enabled = num2 == m_FrameNumber;
				Material material3 = m_MeshRender[num2].material;
				Color color3 = material3.color;
				color3.r = 40f / 51f;
				color3.g = 0.11764706f;
				color3.b = 0.11764706f;
				material3.color = color3;
			}
			break;
		}
		case BorderState.Zone:
		{
			for (int n = 0; n < m_FrameCount; n++)
			{
				m_MeshRender[n].enabled = true;
				Material material2 = m_MeshRender[n].material;
				Color color2 = material2.color;
				color2.r = 40f / 51f;
				color2.g = 0.11764706f;
				color2.b = 40f / 51f;
				material2.color = color2;
			}
			break;
		}
		case BorderState.Blackout:
		{
			for (int l = 0; l < m_FrameCount; l++)
			{
				m_MeshRender[l].enabled = l == 0;
			}
			break;
		}
		case BorderState.Flash:
		{
			m_TimeOfNextChange = Time.realtimeSinceStartup + 0.8f;
			m_FrameNumber = 0;
			for (int j = 0; j < m_FrameCount; j++)
			{
				m_MeshRender[j].enabled = true;
			}
			break;
		}
		case BorderState.INVALID:
			break;
		}
	}

	public static bool CreateBorderPieces(Transform parentGameObject, ref Footprint footPrint, List<LevelEditorBorderElement> store, BorderState state = BorderState.Animating)
	{
		if (parentGameObject == null || footPrint == null || footPrint.m_iW == 0 || footPrint.m_iH == 0)
		{
			return false;
		}
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (instance.m_BorderPrefab == null)
		{
			return false;
		}
		bool[] map = new bool[(footPrint.m_iW + 2) * (footPrint.m_iH + 2)];
		int num = footPrint.m_iW + 2 + 1;
		int num2 = 2;
		int num3 = 0;
		int num4 = footPrint.m_iH * footPrint.m_iW;
		for (int i = 0; i < footPrint.m_iH; i++)
		{
			for (int j = 0; j < footPrint.m_iW; j++)
			{
				if (footPrint.m_bMultiLevel)
				{
					for (int k = 1; k < 6; k++)
					{
						if (footPrint.m_UsedTiles[num3 + num4 * k] != 0)
						{
							map[num] = true;
							break;
						}
					}
				}
				else if (footPrint.m_UsedTiles[num3] != 0)
				{
					map[num] = true;
				}
				num3++;
				num++;
			}
			num += num2;
		}
		return CreateBorderPiecesFromMap(parentGameObject, ref map, store, footPrint.m_iW + 2, footPrint.m_iLeft - 1, footPrint.m_iBottom - 1, state);
	}

	public static bool CreateBorderPiecesForRoom(Transform parentGameObject, int iRoomNumber, BaseLevelManager.LevelLayers layer, List<LevelEditorBorderElement> store, BorderState state = BorderState.Animating)
	{
		if (parentGameObject == null || (int)layer < 1 || (int)layer > 5)
		{
			return false;
		}
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		BaseLevelManager instance2 = BaseLevelManager.GetInstance();
		if (instance2 == null)
		{
			return false;
		}
		if (instance.m_BorderPrefab == null)
		{
			return false;
		}
		bool[] map = new bool[14400];
		BaseLevelManager.LayerDataCollection data = instance2.m_BuildingLayers[(uint)layer];
		int num = 0;
		for (int i = 0; i < 120; i++)
		{
			for (int j = 0; j < 120; j++)
			{
				if (data.m_RoomPropertiesMasks[num] != 0)
				{
					map[num] = BaseLevelManager.IsRoomNumberInProperty(ref data, num, iRoomNumber);
				}
				num++;
			}
		}
		return CreateBorderPiecesFromMap(parentGameObject, ref map, store, 120, -60, -60, state);
	}

	public static bool CreateBorderPiecesFromMap(Transform parentGameObject, ref bool[] map, List<LevelEditorBorderElement> store, int iWidth, int xOffset, int yOffset, BorderState state = BorderState.Animating)
	{
		if (state == BorderState.Blackout)
		{
			return CreateBlackoutPiecesFromMap(parentGameObject, ref map, store, iWidth, xOffset, yOffset);
		}
		if (parentGameObject == null || map == null || map.Length == 0 || iWidth == 0)
		{
			return false;
		}
		int num = map.Length / iWidth;
		if (map.Length != num * iWidth)
		{
			return false;
		}
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (instance.m_BorderPrefab == null)
		{
			return false;
		}
		float num2 = xOffset;
		float num3 = yOffset;
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				BuildingBlockManager.DashedBorderEnum dashedBorderEnum = BuildingBlockManager.DashedBorderEnum.Dash_Empty;
				BuildingBlockManager.DashedBorderEnum dashedBorderEnum2 = BuildingBlockManager.DashedBorderEnum.Dash_TBLR;
				if (!map[num4])
				{
					if (j != 0)
					{
						if (map[num4 - 1])
						{
							dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_TB;
							dashedBorderEnum2 &= BuildingBlockManager.DashedBorderEnum.invL;
						}
						else
						{
							if (i != 0 && map[num4 - iWidth - 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.B;
							}
							if (i < num - 1 && map[num4 + iWidth - 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.T;
							}
						}
					}
					if (j < iWidth - 1)
					{
						if (map[num4 + 1])
						{
							dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_TB;
							dashedBorderEnum2 &= BuildingBlockManager.DashedBorderEnum.invR;
						}
						else
						{
							if (i != 0 && map[num4 - iWidth + 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.B;
							}
							if (i < num - 1 && map[num4 + iWidth + 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.T;
							}
						}
					}
					if (i != 0)
					{
						if (map[num4 - iWidth])
						{
							dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_LR;
							dashedBorderEnum2 &= BuildingBlockManager.DashedBorderEnum.invB;
						}
						else
						{
							if (j != 0 && map[num4 - iWidth - 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.L;
							}
							if (j < iWidth - 1 && map[num4 - iWidth + 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.R;
							}
						}
					}
					if (i < num - 1)
					{
						if (map[num4 + iWidth])
						{
							dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_LR;
							dashedBorderEnum2 &= BuildingBlockManager.DashedBorderEnum.invT;
						}
						else
						{
							if (j != 0 && map[num4 + iWidth - 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.L;
							}
							if (j < iWidth - 1 && map[num4 + iWidth + 1])
							{
								dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.R;
							}
						}
					}
				}
				dashedBorderEnum &= dashedBorderEnum2;
				if (dashedBorderEnum != 0)
				{
					GameObject gameObject = Object.Instantiate(instance.m_BorderPrefab, parentGameObject);
					gameObject.transform.localPosition = new Vector3(num2 + (float)j, num3 + (float)i, -20f);
					LevelEditorBorderElement component = gameObject.GetComponent<LevelEditorBorderElement>();
					if (component == null)
					{
						if (Application.isPlaying)
						{
							Object.Destroy(gameObject);
						}
						else
						{
							Object.DestroyImmediate(gameObject);
						}
						return false;
					}
					component.InitializeBorder(state, 2, instance.m_DashLines_1[(int)dashedBorderEnum], instance.m_DashLines_2[(int)dashedBorderEnum]);
					gameObject.SetActive(value: true);
					store?.Add(component);
				}
				num4++;
			}
		}
		return true;
	}

	public static bool CreateBlackoutPiecesFromMap(Transform parentGameObject, ref bool[] map, List<LevelEditorBorderElement> store, int iWidth, int xOffset, int yOffset)
	{
		if (parentGameObject == null || map == null || map.Length == 0 || iWidth == 0)
		{
			return false;
		}
		int num = map.Length / iWidth;
		if (map.Length != num * iWidth)
		{
			return false;
		}
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (instance.m_BorderPrefab == null)
		{
			return false;
		}
		float num2 = xOffset;
		float num3 = yOffset;
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				BuildingBlockManager.DashedBorderEnum dashedBorderEnum = BuildingBlockManager.DashedBorderEnum.Dash_Empty;
				if (!map[num4])
				{
					if (j != 0 && map[num4 - 1])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.L;
					}
					if (j < iWidth - 1 && map[num4 + 1])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.R;
					}
					if (i != 0 && map[num4 - iWidth])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.B;
					}
					if (i < num - 1 && map[num4 + iWidth])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.T;
					}
					BuildingBlockManager.DashedBorderEnum dashedBorderEnum2 = dashedBorderEnum;
					if ((dashedBorderEnum2 & BuildingBlockManager.DashedBorderEnum.Dash_TR) == 0 && i < num - 1 && j < iWidth - 1 && map[num4 + 1 + iWidth])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_TR;
					}
					if ((dashedBorderEnum2 & BuildingBlockManager.DashedBorderEnum.Dash_TL) == 0 && i < num - 1 && j != 0 && map[num4 - 1 + iWidth])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_TL;
					}
					if ((dashedBorderEnum2 & BuildingBlockManager.DashedBorderEnum.Dash_BL) == 0 && i != 0 && j != 0 && map[num4 - 1 - iWidth])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_BL;
					}
					if ((dashedBorderEnum2 & BuildingBlockManager.DashedBorderEnum.Dash_BR) == 0 && i != 0 && j < iWidth - 1 && map[num4 + 1 - iWidth])
					{
						dashedBorderEnum |= BuildingBlockManager.DashedBorderEnum.Dash_BR;
					}
				}
				if (dashedBorderEnum != 0)
				{
					GameObject gameObject = Object.Instantiate(instance.m_BorderPrefab, parentGameObject);
					gameObject.transform.localPosition = new Vector3(num2 + (float)j, num3 + (float)i, -20f);
					LevelEditorBorderElement component = gameObject.GetComponent<LevelEditorBorderElement>();
					if (component == null)
					{
						if (Application.isPlaying)
						{
							Object.Destroy(gameObject);
						}
						else
						{
							Object.DestroyImmediate(gameObject);
						}
						return false;
					}
					component.InitializeBorder(BorderState.Blackout, 2, instance.m_DashLines_3[(int)dashedBorderEnum], instance.m_DashLines_3[(int)dashedBorderEnum]);
					gameObject.SetActive(value: true);
					store?.Add(component);
				}
				num4++;
			}
		}
		return true;
	}
}
