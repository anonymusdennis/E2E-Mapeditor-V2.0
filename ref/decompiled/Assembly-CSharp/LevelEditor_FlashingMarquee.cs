using System;
using UnityEngine;

public class LevelEditor_FlashingMarquee : MonoBehaviour
{
	[Serializable]
	public class FlashPoint
	{
		[Tooltip("How long this step should last")]
		public float m_Duration = 0.5f;

		public string m_MarqueeInteraction = "PLEASE FILL";

		public string m_MarqueeColor = "PLEASE FILL";
	}

	public MeshRenderer m_MeshRenderer;

	public LevelEditor_Marquee m_MarqueeComponent;

	[Tooltip("How long this marquee should flash, enter 0 if you want it to just run the steps once")]
	public float m_LifeTime = 3f;

	public FlashPoint[] m_Steps = new FlashPoint[0];

	[NonSerialized]
	public bool[] m_Map = new bool[0];

	[NonSerialized]
	public int m_MapWidth;

	[NonSerialized]
	private BaseLevelManager.LevelLayers m_layer = BaseLevelManager.LevelLayers.GroundFloor;

	private float m_fTimer;

	private float m_fStepTimer;

	private int m_Current;

	private BaseLevelManager m_LevelManager;

	private void Start()
	{
		if (m_MarqueeComponent == null && (m_MarqueeComponent = GetComponent<LevelEditor_Marquee>()) == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (m_LevelManager == null && (m_LevelManager = BaseLevelManager.GetInstance()) == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (m_MeshRenderer == null && (m_MeshRenderer = GetComponent<MeshRenderer>()) == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		int num = m_Steps.Length;
		if (m_Map.Length == 0 || m_MapWidth <= 0 || num == 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		m_MarqueeComponent.GenerateMarqueeFromMap(m_Map, m_MapWidth);
		m_MarqueeComponent.SetColourState(m_Steps[0].m_MarqueeColor);
		m_MarqueeComponent.SetInteractionState(m_Steps[0].m_MarqueeInteraction);
		m_MarqueeComponent.enabled = true;
		m_Map = new bool[0];
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			num2 += m_Steps[i].m_Duration;
			m_Steps[i].m_Duration = num2;
		}
	}

	private void Update()
	{
		if (m_LevelManager != null)
		{
			m_MeshRenderer.enabled = m_LevelManager.m_CurrentLayer == m_layer;
		}
		bool flag = false;
		m_fTimer += Time.deltaTime;
		m_fStepTimer += Time.deltaTime;
		if (m_LifeTime != 0f && m_fTimer > m_LifeTime)
		{
			flag = true;
		}
		else
		{
			bool flag2 = false;
			while (!flag && m_fStepTimer > m_Steps[m_Current].m_Duration)
			{
				if (m_Current + 1 >= m_Steps.Length)
				{
					if (m_LifeTime == 0f)
					{
						flag = true;
						continue;
					}
					flag2 = true;
					m_fStepTimer -= m_Steps[m_Current].m_Duration;
					m_Current = 0;
				}
				else
				{
					m_Current++;
					flag2 = true;
				}
			}
			if (flag2)
			{
				m_MarqueeComponent.SetColourState(m_Steps[m_Current].m_MarqueeColor);
				m_MarqueeComponent.SetInteractionState(m_Steps[m_Current].m_MarqueeInteraction);
			}
		}
		if (flag)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static LevelEditor_FlashingMarquee CreateFlashingMarquee(UnityEngine.Object prefab, bool[] map, int iWidth, int X, int Y, BaseLevelManager.LevelLayers eLayer)
	{
		LevelEditorHighLightManager instance = LevelEditorHighLightManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (prefab == null)
		{
			return null;
		}
		UnityEngine.Object @object = UnityEngine.Object.Instantiate(prefab);
		if (@object != null)
		{
			GameObject gameObject = @object as GameObject;
			if (gameObject != null)
			{
				LevelEditor_FlashingMarquee componentInChildren = gameObject.GetComponentInChildren<LevelEditor_FlashingMarquee>();
				if (componentInChildren != null)
				{
					componentInChildren.m_layer = eLayer;
					componentInChildren.m_Map = map;
					componentInChildren.m_MapWidth = iWidth;
					gameObject.transform.SetParent(instance.m_MasterLayers[(uint)eLayer].transform);
					gameObject.transform.localPosition = new Vector3(-60f + (float)X, -60f + (float)Y, -5f);
					return componentInChildren;
				}
			}
		}
		return null;
	}

	public static LevelEditor_FlashingMarquee CreateFlashingMarquee(UnityEngine.Object prefab, LevelEditor_Controller.ScanBits[] scanMap, int iWidth, int X, int Y, BaseLevelManager.LevelLayers eLayer, LevelEditor_Controller.ScanBits lookFor)
	{
		LevelEditorHighLightManager instance = LevelEditorHighLightManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (prefab == null)
		{
			return null;
		}
		UnityEngine.Object @object = UnityEngine.Object.Instantiate(prefab);
		if (@object != null)
		{
			GameObject gameObject = @object as GameObject;
			if (gameObject != null)
			{
				LevelEditor_FlashingMarquee componentInChildren = gameObject.GetComponentInChildren<LevelEditor_FlashingMarquee>();
				if (componentInChildren != null)
				{
					int num = scanMap.Length;
					bool[] array = new bool[num];
					for (int i = 0; i < num; i++)
					{
						if (scanMap[i] == lookFor)
						{
							array[i] = true;
						}
					}
					componentInChildren.m_layer = eLayer;
					componentInChildren.m_Map = array;
					componentInChildren.m_MapWidth = iWidth;
					gameObject.transform.SetParent(instance.m_MasterLayers[(uint)eLayer].transform);
					gameObject.transform.localPosition = new Vector3(-60f + (float)X, -60f + (float)Y, -5f);
					return componentInChildren;
				}
			}
		}
		return null;
	}

	public static LevelEditor_FlashingMarquee CreateFlashingMarquee(UnityEngine.Object prefab, int iWidth, int iHeight, int X, int Y, BaseLevelManager.LevelLayers eLayer)
	{
		LevelEditorHighLightManager instance = LevelEditorHighLightManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (iWidth <= 0 && iHeight <= 0)
		{
			return null;
		}
		if (prefab == null)
		{
			return null;
		}
		UnityEngine.Object @object = UnityEngine.Object.Instantiate(prefab);
		if (@object != null)
		{
			GameObject gameObject = @object as GameObject;
			if (gameObject != null)
			{
				LevelEditor_FlashingMarquee componentInChildren = gameObject.GetComponentInChildren<LevelEditor_FlashingMarquee>();
				if (componentInChildren != null)
				{
					int num = iWidth * iHeight;
					bool[] array = new bool[num];
					for (int i = 0; i < num; i++)
					{
						array[i] = true;
					}
					componentInChildren.m_layer = eLayer;
					componentInChildren.m_Map = array;
					componentInChildren.m_MapWidth = iWidth;
					gameObject.transform.SetParent(instance.m_MasterLayers[(uint)eLayer].transform);
					gameObject.transform.localPosition = new Vector3(-60f + (float)X, -60f + (float)Y, -5f);
					return componentInChildren;
				}
			}
		}
		return null;
	}
}
