using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditor_Marquee : MonoBehaviour
{
	[Serializable]
	public struct MarqueeColourData
	{
		public string m_ColourName;

		public Color m_CenterColour;

		public Color m_Border1Colour;

		public Color m_Border2Colour;
	}

	[Serializable]
	public struct MarqueeStateData
	{
		public string m_StateName;

		public AnimationCurve m_Border1Threshold;

		public AnimationCurve m_Border2Threshold;

		public bool m_OutsideMode;

		public AnimationCurve m_FillAlpha;

		public AnimationCurve m_Border1Alpha;

		public AnimationCurve m_Border2Alpha;
	}

	private const byte kTop = 1;

	private const byte kBottom = 2;

	private const byte kLeft = 4;

	private const byte kRight = 8;

	private const byte kTL = 16;

	private const byte kTR = 32;

	private const byte kBR = 64;

	private const byte kBL = 128;

	private const int kBLIndex = 0;

	private const int kLIndex = 1;

	private const int kTLIndex = 2;

	private const int kTIndex = 3;

	private const int kTRIndex = 4;

	private const int kRIndex = 5;

	private const int kBRIndex = 6;

	private const int kBIndex = 7;

	private const int kCIndex = 8;

	private const int kVertIndexMax = 9;

	private static bool m_PropsSet = false;

	private static int m_PropCenterColour = -1;

	private static int m_PropBorder1Colour = -1;

	private static int m_PropBorder2Colour = -1;

	private static int m_PropBorder1Threshold = -1;

	private static int m_PropBorder2Threshold = -1;

	private static int m_PropFillAlpha = -1;

	private static int m_PropBorder1Alpha = -1;

	private static int m_PropBorder2Alpha = -1;

	private const int m_LayerWidth = 120;

	private const int m_LayerHeight = 120;

	private const int m_OutsideEdgeSize = 1;

	private const int m_OutsideEdgeTotal = 2;

	private const int m_OutsideLayerWidth = 122;

	private const int m_OutsideLayerHeight = 122;

	private static int[,,] m_VertLookUpTable = new int[122, 122, 9];

	public MarqueeColourData[] m_MarqueeColourStates = new MarqueeColourData[0];

	public MarqueeStateData[] m_MarqueeInteractionStates = new MarqueeStateData[0];

	private List<Vector3> m_MeshVertsInside = new List<Vector3>();

	private List<Vector2> m_MeshUVsInside = new List<Vector2>();

	private List<int> m_MeshTrisInside = new List<int>();

	private List<Vector3> m_MeshVertsOutside = new List<Vector3>();

	private List<Vector2> m_MeshUVsOutside = new List<Vector2>();

	private List<int> m_MeshTrisOutside = new List<int>();

	private MeshFilter m_MeshFilter;

	private MeshRenderer m_MeshRenderer;

	private Material m_MarqueeMat;

	private Mesh m_MeshRepresentation;

	private float m_CurrentZoomLevel = -1f;

	private MarqueeStateData m_InteractionState = default(MarqueeStateData);

	private MarqueeColourData m_ColourState = default(MarqueeColourData);

	private string m_PendingInteractionState = string.Empty;

	private string m_PendingColorState = string.Empty;

	private bool[] m_PendingMap = new bool[0];

	private int m_PendingWidth;

	private bool m_bCreated;

	private bool m_Show = true;

	private void Start()
	{
		if (!m_PropsSet)
		{
			m_PropCenterColour = Shader.PropertyToID("_CenterColour");
			m_PropBorder1Colour = Shader.PropertyToID("_Border1Colour");
			m_PropBorder2Colour = Shader.PropertyToID("_Border2Colour");
			m_PropBorder1Threshold = Shader.PropertyToID("_Border1Threshold");
			m_PropBorder2Threshold = Shader.PropertyToID("_Border2Threshold");
			m_PropFillAlpha = Shader.PropertyToID("_FillAlpha");
			m_PropBorder1Alpha = Shader.PropertyToID("_Border1Alpha");
			m_PropBorder2Alpha = Shader.PropertyToID("_Border2Alpha");
			m_PropsSet = true;
		}
		if (m_MeshFilter == null && (m_MeshFilter = GetComponent<MeshFilter>()) == null)
		{
			base.enabled = false;
			return;
		}
		if (m_MeshRenderer == null && (m_MeshRenderer = GetComponent<MeshRenderer>()) == null)
		{
			base.enabled = false;
			return;
		}
		if (string.IsNullOrEmpty(m_PendingInteractionState) || string.IsNullOrEmpty(m_PendingColorState))
		{
			base.enabled = false;
			return;
		}
		if (m_PendingWidth == 0)
		{
			base.enabled = false;
			return;
		}
		m_MarqueeMat = m_MeshRenderer.material;
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		instance.OnZoomChanged += SetZoomLevel;
		InternalGenerateMarqueeFromMap(m_PendingMap, m_PendingWidth);
		m_PendingMap = new bool[0];
		Internal_SetInteractionState(m_PendingInteractionState, bUpdateColour: false);
		m_PendingInteractionState = string.Empty;
		Internal_SetColourState(m_PendingColorState);
		m_PendingColorState = string.Empty;
		m_bCreated = true;
		m_MeshRenderer.enabled = m_Show;
	}

	private void OnDestroy()
	{
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		if (instance != null)
		{
			instance.OnZoomChanged -= SetZoomLevel;
		}
	}

	public void SetZoomLevel(float zoomLevel)
	{
		if (m_bCreated && zoomLevel != m_CurrentZoomLevel)
		{
			Internal_SetZoomLevel(zoomLevel);
		}
	}

	private void Internal_SetZoomLevel(float zoomLevel)
	{
		if (zoomLevel == -1f)
		{
			LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
			if (instance != null)
			{
				zoomLevel = instance.GetZoomPerc();
			}
		}
		if (m_MarqueeMat == null)
		{
			m_MarqueeMat = m_MeshRenderer.material;
			if (m_MarqueeMat == null)
			{
				return;
			}
		}
		float num = 0f;
		float num2 = 0f;
		if (m_InteractionState.m_OutsideMode)
		{
			num = m_InteractionState.m_Border2Threshold.Evaluate(m_CurrentZoomLevel);
			num2 = m_InteractionState.m_Border1Threshold.Evaluate(m_CurrentZoomLevel);
		}
		else
		{
			num = m_InteractionState.m_Border1Threshold.Evaluate(m_CurrentZoomLevel);
			num2 = m_InteractionState.m_Border2Threshold.Evaluate(m_CurrentZoomLevel);
		}
		m_MarqueeMat.SetFloat(m_PropBorder1Threshold, num);
		m_MarqueeMat.SetFloat(m_PropBorder2Threshold, num2);
		m_CurrentZoomLevel = zoomLevel;
		m_MarqueeMat.SetFloat(m_PropFillAlpha, m_InteractionState.m_FillAlpha.Evaluate(m_CurrentZoomLevel));
		m_MarqueeMat.SetFloat(m_PropBorder1Alpha, m_InteractionState.m_Border1Alpha.Evaluate(m_CurrentZoomLevel));
		m_MarqueeMat.SetFloat(m_PropBorder2Alpha, m_InteractionState.m_Border2Alpha.Evaluate(m_CurrentZoomLevel));
	}

	public void SetInteractionState(string marqueeStateName)
	{
		if (m_bCreated)
		{
			marqueeStateName = marqueeStateName.ToLower();
			if (!(marqueeStateName == m_InteractionState.m_StateName))
			{
				Internal_SetInteractionState(marqueeStateName);
			}
		}
		else
		{
			m_PendingInteractionState = marqueeStateName;
		}
	}

	private void Internal_SetInteractionState(string marqueeStateName, bool bUpdateColour = true)
	{
		int num = m_MarqueeInteractionStates.FindIndex((MarqueeStateData x) => x.m_StateName == marqueeStateName);
		if (num == -1)
		{
			return;
		}
		bool outsideMode = m_InteractionState.m_OutsideMode;
		MarqueeStateData marqueeStateData = (m_InteractionState = m_MarqueeInteractionStates[num]);
		if (m_MarqueeMat == null)
		{
			m_MarqueeMat = m_MeshRenderer.material;
			if (m_MarqueeMat == null)
			{
				return;
			}
		}
		if (outsideMode != marqueeStateData.m_OutsideMode)
		{
			Mesh mesh = m_MeshFilter.mesh;
			mesh.Clear();
			if (marqueeStateData.m_OutsideMode)
			{
				mesh.SetVertices(m_MeshVertsOutside);
				mesh.SetUVs(0, m_MeshUVsOutside);
				mesh.SetTriangles(m_MeshTrisOutside, 0);
			}
			else
			{
				mesh.SetVertices(m_MeshVertsInside);
				mesh.SetUVs(0, m_MeshUVsInside);
				mesh.SetTriangles(m_MeshTrisInside, 0);
			}
		}
		m_MarqueeMat.SetFloat(m_PropFillAlpha, marqueeStateData.m_FillAlpha.Evaluate(m_CurrentZoomLevel));
		m_MarqueeMat.SetFloat(m_PropBorder1Alpha, marqueeStateData.m_Border1Alpha.Evaluate(m_CurrentZoomLevel));
		m_MarqueeMat.SetFloat(m_PropBorder2Alpha, marqueeStateData.m_Border2Alpha.Evaluate(m_CurrentZoomLevel));
		Internal_SetZoomLevel(m_CurrentZoomLevel);
		if (bUpdateColour)
		{
			Internal_SetColourState(m_ColourState.m_ColourName);
		}
	}

	public void SetColourState(string marqueeColourName)
	{
		if (m_bCreated)
		{
			marqueeColourName = marqueeColourName.ToLower();
			if (!(marqueeColourName == m_ColourState.m_ColourName))
			{
				Internal_SetColourState(marqueeColourName);
			}
		}
		else
		{
			m_PendingColorState = marqueeColourName;
		}
	}

	private void Internal_SetColourState(string marqueeColourName)
	{
		int num = m_MarqueeColourStates.FindIndex((MarqueeColourData x) => x.m_ColourName == marqueeColourName);
		if (num == -1)
		{
			return;
		}
		MarqueeColourData marqueeColourData = (m_ColourState = m_MarqueeColourStates[num]);
		if (m_MarqueeMat == null)
		{
			m_MarqueeMat = m_MeshRenderer.material;
			if (m_MarqueeMat == null)
			{
				return;
			}
		}
		m_MarqueeMat.SetVector(m_PropCenterColour, marqueeColourData.m_CenterColour);
		if (m_InteractionState.m_OutsideMode)
		{
			m_MarqueeMat.SetVector(m_PropBorder2Colour, marqueeColourData.m_Border1Colour);
			m_MarqueeMat.SetVector(m_PropBorder1Colour, marqueeColourData.m_Border2Colour);
		}
		else
		{
			m_MarqueeMat.SetVector(m_PropBorder1Colour, marqueeColourData.m_Border1Colour);
			m_MarqueeMat.SetVector(m_PropBorder2Colour, marqueeColourData.m_Border2Colour);
		}
	}

	public void GenerateMarqueeFromMap(bool[] map, int iWidth)
	{
		if (!m_bCreated)
		{
			m_PendingMap = map;
			m_PendingWidth = iWidth;
		}
		else
		{
			InternalGenerateMarqueeFromMap(map, iWidth);
		}
	}

	public void InternalGenerateMarqueeFromMap(bool[] map, int iWidth)
	{
		int num = map.Length / iWidth;
		int num2 = iWidth * num;
		int num3 = num + 2;
		int num4 = iWidth + 2;
		m_MeshVertsInside.Clear();
		m_MeshUVsInside.Clear();
		m_MeshTrisInside.Clear();
		byte[] outsideMap = new byte[num3 * num4];
		bool[] array = new bool[num2];
		int[] array2 = new int[9];
		byte b = 0;
		byte b2 = 0;
		int num5 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				for (int k = 0; k < 9; k++)
				{
					m_VertLookUpTable[j, i, k] = -1;
				}
			}
		}
		for (int l = 0; l < num; l++)
		{
			for (int m = 0; m < iWidth; m++)
			{
				if (map[num5])
				{
					if (l == num - 1)
					{
					}
					b = 0;
					b2 = 0;
					for (int n = 0; n < 9; n++)
					{
						array2[n] = -1;
					}
					int num6 = (l + 1) * num4 + m + 1;
					array[num5] = true;
					if (m == 0 || !map[num5 - 1])
					{
						b = (byte)(b | 4u);
						outsideMap[num6 - 1] |= 8;
					}
					if (m == iWidth - 1 || !map[num5 + 1])
					{
						b = (byte)(b | 8u);
						outsideMap[num6 + 1] |= 4;
					}
					if (l == 0 || !map[num5 - iWidth])
					{
						b = (byte)(b | 2u);
						outsideMap[num6 - num4] |= 1;
					}
					if (l == num - 1 || !map[num5 + iWidth])
					{
						b = (byte)(b | 1u);
						outsideMap[num6 + num4] |= 2;
					}
					if (m > 0 && l > 0 && !map[num5 - iWidth - 1])
					{
						b = (byte)(b | 0x80u);
						outsideMap[num6 - num4 - 1] |= 32;
					}
					if (m > 0 && l < num - 1 && !map[num5 + iWidth - 1])
					{
						b = (byte)(b | 0x10u);
						outsideMap[num6 + num4 - 1] |= 64;
					}
					if (m < iWidth - 1 && l > 0 && !map[num5 - iWidth + 1])
					{
						b = (byte)(b | 0x40u);
						outsideMap[num6 - num4 + 1] |= 16;
					}
					if (m < iWidth - 1 && l < num - 1 && !map[num5 + iWidth + 1])
					{
						b = (byte)(b | 0x20u);
						outsideMap[num6 + num4 + 1] |= 128;
					}
					if ((b & 5) == 5)
					{
						outsideMap[num6 + num4 - 1] |= 64;
					}
					if ((b & 9) == 9)
					{
						outsideMap[num6 + num4 + 1] |= 128;
					}
					if ((b & 0xA) == 10)
					{
						outsideMap[num6 - num4 + 1] |= 16;
					}
					if ((b & 6) == 6)
					{
						outsideMap[num6 - num4 - 1] |= 32;
					}
					if (m > 0 && array[num5 - 1])
					{
						b2 = (byte)(b2 | 4u);
					}
					if (l > 0 && array[num5 - iWidth])
					{
						b2 = (byte)(b2 | 2u);
					}
					if (m > 0 && l > 0 && array[num5 - iWidth - 1])
					{
						b2 = (byte)(b2 | 0x80u);
					}
					if (m < iWidth - 1 && l > 0 && array[num5 - iWidth + 1])
					{
						b2 = (byte)(b2 | 0x40u);
					}
					GenerateTriangleFanForTile(m, l, 0, 0, iWidth, num, b, b2, ref m_MeshVertsInside, ref m_MeshUVsInside, ref m_MeshTrisInside);
				}
				num5++;
			}
		}
		if (m_MeshRepresentation == null)
		{
			m_MeshRepresentation = new Mesh();
			m_MeshRepresentation.MarkDynamic();
		}
		else
		{
			m_MeshRepresentation.Clear();
		}
		m_MeshRepresentation.SetVertices(m_MeshVertsInside);
		m_MeshRepresentation.SetTriangles(m_MeshTrisInside, 0);
		m_MeshRepresentation.SetUVs(0, m_MeshUVsInside);
		GenerateOutsideMesh(ref outsideMap, num4);
		m_MeshFilter.mesh = m_MeshRepresentation;
	}

	private void GenerateOutsideMesh(ref byte[] outsideMap, int width)
	{
		bool[] array = new bool[outsideMap.Length];
		int[] array2 = new int[9];
		int num = outsideMap.Length / width;
		m_MeshVertsOutside.Clear();
		m_MeshUVsOutside.Clear();
		m_MeshTrisOutside.Clear();
		byte b = 0;
		byte b2 = 0;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < width; j++)
			{
				for (int k = 0; k < 9; k++)
				{
					m_VertLookUpTable[j, i, k] = -1;
				}
			}
		}
		for (int l = 0; l < num; l++)
		{
			for (int m = 0; m < width; m++)
			{
				if (outsideMap[num2] != 0)
				{
					b = outsideMap[num2];
					b2 = 0;
					for (int n = 0; n < 9; n++)
					{
						array2[n] = -1;
					}
					array[num2] = true;
					if (m > 0 && array[num2 - 1])
					{
						b2 = (byte)(b2 | 4u);
					}
					if (m < width - 1 && array[num2 + 1])
					{
						b2 = (byte)(b2 | 8u);
					}
					if (l > 0 && array[num2 - width])
					{
						b2 = (byte)(b2 | 2u);
					}
					if (l < num - 1 && array[num2 + width])
					{
						b2 = (byte)(b2 | 1u);
					}
					if (m > 0 && l > 0 && array[num2 - width - 1])
					{
						b2 = (byte)(b2 | 0x80u);
					}
					if (m > 0 && l < num - 1 && array[num2 + width - 1])
					{
						b2 = (byte)(b2 | 0x10u);
					}
					if (m < width - 1 && l > 0 && array[num2 - width + 1])
					{
						b2 = (byte)(b2 | 0x40u);
					}
					if (m < width - 1 && l < num - 1 && array[num2 + width + 1])
					{
						b2 = (byte)(b2 | 0x20u);
					}
					GenerateTriangleFanForTile(m, l, -1, -1, width, num, b, b2, ref m_MeshVertsOutside, ref m_MeshUVsOutside, ref m_MeshTrisOutside);
				}
				num2++;
			}
		}
	}

	private void GenerateTriangleFanForTile(int iX, int iY, int xOffset, int yOffset, int iWidth, int iHeight, byte edge, byte neighbours, ref List<Vector3> meshVerts, ref List<Vector2> meshUVs, ref List<int> meshTris)
	{
		int[] array = new int[9];
		for (int i = 0; i < 9; i++)
		{
			array[i] = -1;
		}
		int num = iX + xOffset;
		int num2 = iY + yOffset;
		if ((neighbours & 0x86) == 0)
		{
			array[0] = meshVerts.Count;
			meshVerts.Add(new Vector3(num, num2, -20f));
			if ((edge & 0x86u) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else
		{
			if (iY > 0)
			{
				int num3 = (array[0] = m_VertLookUpTable[iX, iY - 1, 2]);
				if (iX > 0 && num3 == -1)
				{
					num3 = m_VertLookUpTable[iX - 1, iY - 1, 4];
					array[0] = num3;
				}
			}
			if (iX > 0 && array[0] == -1)
			{
				array[0] = m_VertLookUpTable[iX - 1, iY, 6];
			}
			if (array[0] == -1)
			{
				Debug.LogError("Failed to find vert index (BL)!");
			}
		}
		if ((neighbours & 4) == 0)
		{
			array[1] = meshVerts.Count;
			meshVerts.Add(new Vector3(num, (float)num2 + 0.5f, -20f));
			if ((edge & 4u) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else if ((edge & 0x93u) != 0)
			{
				meshUVs.Add(new Vector2(0.5f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else if (iX > 0)
		{
			array[1] = m_VertLookUpTable[iX - 1, iY, 5];
			if (array[1] == -1)
			{
				Debug.LogError("Failed to find vert index (L)!");
			}
		}
		if ((neighbours & 0x15) == 0)
		{
			array[2] = meshVerts.Count;
			meshVerts.Add(new Vector3(num, num2 + 1, -20f));
			if ((edge & 0x15u) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else
		{
			if (iY < iHeight - 1)
			{
				int num4 = (array[2] = m_VertLookUpTable[iX, iY + 1, 0]);
				if (iX > 0 && num4 == -1)
				{
					num4 = m_VertLookUpTable[iX - 1, iY + 1, 6];
					array[2] = num4;
				}
			}
			if (iX > 0 && array[2] == -1)
			{
				array[2] = m_VertLookUpTable[iX - 1, iY, 4];
			}
			if (array[2] == -1)
			{
				Debug.LogError("Failed to find vert index (TL)!");
			}
		}
		if ((neighbours & 1) == 0)
		{
			array[3] = meshVerts.Count;
			meshVerts.Add(new Vector3((float)num + 0.5f, num2 + 1, -20f));
			if (((uint)edge & (true ? 1u : 0u)) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else if ((edge & 0x3Cu) != 0)
			{
				meshUVs.Add(new Vector2(0.5f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else if (iY < iHeight - 1)
		{
			array[3] = m_VertLookUpTable[iX, iY + 1, 7];
			if (array[3] == -1)
			{
				Debug.LogError("Failed to find vert index (T)!");
			}
		}
		if ((neighbours & 0x29) == 0)
		{
			array[4] = meshVerts.Count;
			meshVerts.Add(new Vector3(num + 1, num2 + 1, -20f));
			if ((edge & 0x29u) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else
		{
			if (iY < iHeight - 1)
			{
				int num5 = (array[4] = m_VertLookUpTable[iX, iY + 1, 6]);
				if (iX < iWidth - 1 && num5 == -1)
				{
					num5 = m_VertLookUpTable[iX + 1, iY + 1, 0];
					array[4] = num5;
				}
			}
			if (iX < iWidth - 1 && array[4] == -1)
			{
				array[4] = m_VertLookUpTable[iX + 1, iY, 2];
			}
			if (array[4] == -1)
			{
				Debug.LogError("Failed to find vert index (TR)!");
			}
		}
		if ((neighbours & 8) == 0)
		{
			array[5] = meshVerts.Count;
			meshVerts.Add(new Vector3(num + 1, (float)num2 + 0.5f, -20f));
			if ((edge & 8u) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else if ((edge & 0x63u) != 0)
			{
				meshUVs.Add(new Vector2(0.5f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else if (iX < iWidth - 1)
		{
			array[5] = m_VertLookUpTable[iX + 1, iY, 1];
			if (array[5] == -1)
			{
				Debug.LogError("Failed to find vert index (R)!");
			}
		}
		if ((neighbours & 0x4A) == 0)
		{
			array[6] = meshVerts.Count;
			meshVerts.Add(new Vector3(num + 1, num2, -20f));
			if ((edge & 0x4Au) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else
		{
			if (iY > 0)
			{
				int num6 = (array[6] = m_VertLookUpTable[iX, iY - 1, 4]);
				if (iX < iWidth - 1 && num6 == -1)
				{
					num6 = m_VertLookUpTable[iX + 1, iY - 1, 2];
					array[6] = num6;
				}
			}
			if (iX < iWidth - 1 && array[6] == -1)
			{
				array[6] = m_VertLookUpTable[iX + 1, iY, 0];
			}
			if (array[6] == -1)
			{
				Debug.LogError("Failed to find vert index (BR)!");
			}
		}
		if ((neighbours & 2) == 0)
		{
			array[7] = meshVerts.Count;
			meshVerts.Add(new Vector3((float)num + 0.5f, num2, -20f));
			if ((edge & 2u) != 0)
			{
				meshUVs.Add(new Vector2(1f, 0f));
			}
			else if ((edge & 0xCCu) != 0)
			{
				meshUVs.Add(new Vector2(0.5f, 0f));
			}
			else
			{
				meshUVs.Add(new Vector2(0f, 0f));
			}
		}
		else if (iY > 0)
		{
			array[7] = m_VertLookUpTable[iX, iY - 1, 3];
			if (array[7] == -1)
			{
				Debug.LogError("Failed to find vert index (T)!");
			}
		}
		array[8] = meshVerts.Count;
		meshVerts.Add(new Vector3((float)num + 0.5f, (float)num2 + 0.5f, -20f));
		if (edge != 0)
		{
			meshUVs.Add(new Vector2(0.5f, 0f));
		}
		else
		{
			meshUVs.Add(new Vector2(0f, 0f));
		}
		m_VertLookUpTable[iX, iY, 0] = array[0];
		m_VertLookUpTable[iX, iY, 1] = array[1];
		m_VertLookUpTable[iX, iY, 2] = array[2];
		m_VertLookUpTable[iX, iY, 3] = array[3];
		m_VertLookUpTable[iX, iY, 4] = array[4];
		m_VertLookUpTable[iX, iY, 5] = array[5];
		m_VertLookUpTable[iX, iY, 6] = array[6];
		m_VertLookUpTable[iX, iY, 7] = array[7];
		m_VertLookUpTable[iX, iY, 8] = array[8];
		meshTris.Add(array[0]);
		meshTris.Add(array[1]);
		meshTris.Add(array[8]);
		meshTris.Add(array[1]);
		meshTris.Add(array[2]);
		meshTris.Add(array[8]);
		meshTris.Add(array[2]);
		meshTris.Add(array[3]);
		meshTris.Add(array[8]);
		meshTris.Add(array[3]);
		meshTris.Add(array[4]);
		meshTris.Add(array[8]);
		meshTris.Add(array[4]);
		meshTris.Add(array[5]);
		meshTris.Add(array[8]);
		meshTris.Add(array[5]);
		meshTris.Add(array[6]);
		meshTris.Add(array[8]);
		meshTris.Add(array[6]);
		meshTris.Add(array[7]);
		meshTris.Add(array[8]);
		meshTris.Add(array[7]);
		meshTris.Add(array[0]);
		meshTris.Add(array[8]);
	}

	public void RegenerateFromZone(LevelEditor_ZoneManager.Zone zone)
	{
		GenerateMarqueeFromMap(zone.GetMap(), zone.m_Width);
		base.transform.localPosition = new Vector3(-60 + zone.m_Left, -60 + zone.m_Bottom, 0f);
	}

	public void RegenerateFromMap(bool[] map, int iWidth)
	{
		GenerateMarqueeFromMap(map, iWidth);
	}

	public static LevelEditor_Marquee CreateMarqueeForZone(UnityEngine.Object prefab, string strInteraction, string strColor, LevelEditor_ZoneManager.Zone zone)
	{
		LevelEditorHighLightManager instance = LevelEditorHighLightManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (prefab != null && zone != null)
		{
			UnityEngine.Object @object = UnityEngine.Object.Instantiate(prefab);
			if (@object != null)
			{
				GameObject gameObject = @object as GameObject;
				if (gameObject != null)
				{
					LevelEditor_Marquee componentInChildren = gameObject.GetComponentInChildren<LevelEditor_Marquee>();
					if (componentInChildren != null)
					{
						zone.m_ZoneGraphic = componentInChildren;
						zone.m_ZoneGraphic.transform.SetParent(instance.m_MasterLayers[(uint)zone.m_Layer].transform);
						zone.m_ZoneGraphic.transform.localPosition = new Vector3(-60f + (float)zone.m_Left, -60f + (float)zone.m_Bottom, 0f);
						componentInChildren.m_PendingMap = zone.GetMap();
						componentInChildren.m_PendingWidth = zone.m_Width;
						if (string.IsNullOrEmpty(strInteraction))
						{
							strInteraction = componentInChildren.m_MarqueeInteractionStates[0].m_StateName;
						}
						if (string.IsNullOrEmpty(strColor))
						{
							strColor = componentInChildren.m_MarqueeColourStates[0].m_ColourName;
						}
						componentInChildren.m_PendingInteractionState = strInteraction.ToLower();
						componentInChildren.m_PendingColorState = strColor.ToLower();
						return componentInChildren;
					}
				}
			}
			if (@object != null)
			{
				UnityEngine.Object.Destroy(@object);
			}
		}
		return null;
	}

	public static LevelEditor_Marquee CreateMarqueeFromMap(UnityEngine.Object prefab, string strInteraction, string strColor, bool[] map, int iWidth, int iLeft, int iBottom, BaseLevelManager.LevelLayers eLayer)
	{
		LevelEditorHighLightManager instance = LevelEditorHighLightManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (prefab != null)
		{
			UnityEngine.Object @object = UnityEngine.Object.Instantiate(prefab);
			if (@object != null)
			{
				GameObject gameObject = @object as GameObject;
				if (gameObject != null)
				{
					LevelEditor_Marquee componentInChildren = gameObject.GetComponentInChildren<LevelEditor_Marquee>();
					if (componentInChildren != null)
					{
						componentInChildren.transform.SetParent(instance.m_MasterLayers[(uint)eLayer].transform);
						componentInChildren.transform.localPosition = new Vector3(-60f + (float)iLeft, -60f + (float)iBottom, 0f);
						componentInChildren.m_PendingMap = map;
						componentInChildren.m_PendingWidth = iWidth;
						if (string.IsNullOrEmpty(strInteraction))
						{
							strInteraction = componentInChildren.m_MarqueeInteractionStates[0].m_StateName;
						}
						if (string.IsNullOrEmpty(strColor))
						{
							strColor = componentInChildren.m_MarqueeColourStates[0].m_ColourName;
						}
						componentInChildren.m_PendingInteractionState = strInteraction.ToLower();
						componentInChildren.m_PendingColorState = strColor.ToLower();
						return componentInChildren;
					}
				}
			}
			if (@object != null)
			{
				UnityEngine.Object.Destroy(@object);
			}
		}
		return null;
	}

	public void ShowMarquee(bool bShow)
	{
		m_Show = bShow;
		if (m_bCreated)
		{
			m_MeshRenderer.enabled = m_Show;
		}
	}
}
