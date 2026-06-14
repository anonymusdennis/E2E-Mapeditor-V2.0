using System;
using UnityEngine;

public class LevelEditor_UIRequirement : MonoBehaviour
{
	[Flags]
	public enum UpdateProperties
	{
		Nothing = 0,
		Focussed = 1,
		Validity = 2,
		Count = 4,
		Zone = 8,
		Ticked = 0x10,
		WaitingForData = 0x20,
		EVERTHING = 0x1F
	}

	public bool m_NewVersion;

	public GameObject m_VisualMaster;

	public T17Image m_BackgroundRenderer;

	public Sprite m_NormalBackImage;

	public Sprite m_RedBackImage;

	public Color m_NormalColour = Color.white;

	public Color m_InvalidColor = new Color(47f / 51f, 0.7137255f, 4f / 85f);

	public Color m_MaxColor = new Color(47f / 51f, 72f / 85f, 0.6156863f);

	public T17RawImage m_IconRenderer;

	public GameObject m_Tick;

	public GameObject m_Highlight;

	public T17Text m_Text;

	public string m_CountString = "($CURRENT\\$MAX$MORE)";

	public string m_MoreString = "+";

	private bool m_CurrentlyFocused;

	private bool m_CurrentlyTicked;

	private bool m_CurrentlyValid = true;

	private bool m_MaxReached;

	private int m_CurrentCount = -1;

	private bool m_Initialized;

	private LevelEditor_ZoneManager.Zone m_Zone;

	private int m_CurrentZoneUpdateNumber = -1;

	private int m_Max = -1;

	private int m_Min = -1;

	private int m_RequirementIndex;

	private int m_BlockID = -1;

	private int m_CurrentBlock = -1;

	private int m_GroupIndex = -1;

	private BuildingBlockGroupManager.Group m_Group;

	private LevelEditor_Controller m_CachedController;

	public float m_SecondsBeforeToolTip = 2f;

	private float m_ToolTipTimer = -1f;

	private UpdateProperties m_UpdateFlags;

	private void Start()
	{
	}

	private void Update()
	{
		if (!m_Initialized)
		{
			return;
		}
		UpdateZoneIcon();
		UpdateHighlight();
		UpdateTicked();
		if (m_Zone.m_ZoneUpdateCount != m_CurrentZoneUpdateNumber || (m_UpdateFlags & UpdateProperties.WaitingForData) == UpdateProperties.WaitingForData)
		{
			m_CurrentZoneUpdateNumber = m_Zone.m_ZoneUpdateCount;
			GetData();
		}
		UpdateCount();
		UpdateValid();
		if (m_ToolTipTimer >= 0f)
		{
			m_ToolTipTimer -= Time.deltaTime;
			if (m_ToolTipTimer < 0f)
			{
				DisplayToolTip();
			}
		}
	}

	private void UpdateTicked()
	{
		if ((m_UpdateFlags & UpdateProperties.Ticked) == UpdateProperties.Ticked)
		{
			m_UpdateFlags &= ~UpdateProperties.Ticked;
			m_Tick.SetActive(m_CurrentlyTicked);
		}
	}

	private void UpdateHighlight()
	{
		if ((m_UpdateFlags & UpdateProperties.Focussed) == UpdateProperties.Focussed)
		{
			m_UpdateFlags &= ~UpdateProperties.Focussed;
			m_Highlight.SetActive(m_CurrentlyFocused);
		}
	}

	private void UpdateZoneIcon()
	{
		if ((m_UpdateFlags & UpdateProperties.Zone) == UpdateProperties.Zone)
		{
			m_UpdateFlags &= ~UpdateProperties.Zone;
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_BlockID);
			if (block != null && block.m_UIImage != null)
			{
				m_IconRenderer.texture = block.m_UIImage.mainTexture;
				Rect uvRect = new Rect(block.m_UIImage.GetTextureOffset("_MainTex"), block.m_UIImage.GetTextureScale("_MainTex"));
				m_IconRenderer.uvRect = uvRect;
			}
		}
	}

	private void UpdateValid()
	{
		if ((m_UpdateFlags & UpdateProperties.Validity) != UpdateProperties.Validity)
		{
			return;
		}
		m_UpdateFlags &= ~UpdateProperties.Validity;
		if (m_CurrentlyValid)
		{
			if (m_MaxReached)
			{
				m_Text.color = m_MaxColor;
			}
			else
			{
				m_Text.color = m_NormalColour;
			}
			m_BackgroundRenderer.sprite = m_NormalBackImage;
		}
		else
		{
			m_Text.color = m_InvalidColor;
			m_BackgroundRenderer.sprite = m_RedBackImage;
		}
	}

	private void UpdateCount()
	{
		if ((m_UpdateFlags & UpdateProperties.Count) != UpdateProperties.Count)
		{
			return;
		}
		if (!m_NewVersion)
		{
			m_UpdateFlags &= ~UpdateProperties.Count;
			string countString = m_CountString;
			countString = countString.Replace("$CURRENT", m_CurrentCount.ToString());
			bool flag = false;
			int num = -1;
			bool flag2 = false;
			if (m_Min > m_CurrentCount)
			{
				num = m_Min;
				if (m_Max == 0 || m_Max > m_Min)
				{
					flag = true;
				}
			}
			else if (m_Max == 0)
			{
				num = m_CurrentCount;
				flag = true;
			}
			else if (m_Max > m_CurrentCount)
			{
				num = m_CurrentCount;
				flag = true;
			}
			else
			{
				flag2 = true;
				num = m_Max;
			}
			countString = countString.Replace("$MAX", num.ToString());
			countString = ((!flag) ? countString.Replace("$MORE", string.Empty) : countString.Replace("$MORE", m_MoreString));
			m_Text.text = countString;
			if (m_Min == 0)
			{
				m_VisualMaster.SetActive(value: false);
			}
			else
			{
				m_VisualMaster.SetActive(value: true);
			}
			if (flag2 != m_MaxReached)
			{
				m_MaxReached = flag2;
				m_UpdateFlags |= ~UpdateProperties.Validity;
			}
			return;
		}
		m_UpdateFlags &= ~UpdateProperties.Count;
		string countString2 = m_CountString;
		countString2 = countString2.Replace("$CURRENT", m_CurrentCount.ToString());
		bool flag3 = false;
		int num2 = -1;
		bool flag4 = false;
		if (m_CurrentCount < m_Min)
		{
			num2 = m_Min;
		}
		else if (m_Max == 0)
		{
			num2 = m_CurrentCount;
			flag3 = true;
		}
		else
		{
			num2 = m_Max;
			if (m_CurrentCount == m_Max)
			{
				flag4 = true;
			}
		}
		countString2 = countString2.Replace("$MAX", num2.ToString());
		countString2 = ((!flag3) ? countString2.Replace("$MORE", string.Empty) : countString2.Replace("$MORE", m_MoreString));
		m_Text.text = countString2;
		if (m_Min == 0)
		{
			m_VisualMaster.SetActive(value: false);
		}
		else
		{
			m_VisualMaster.SetActive(value: true);
		}
		if (flag4 != m_MaxReached)
		{
			m_MaxReached = flag4;
			m_UpdateFlags |= ~UpdateProperties.Validity;
		}
	}

	public void OnSelected()
	{
		if (base.enabled && LevelEditor_Controller.GetInstance() != null)
		{
			LevelEditor_Controller.GetInstance().ExternalSelectBlock(m_BlockID);
		}
	}

	public void OnMouseEnter()
	{
		if (!m_CurrentlyFocused)
		{
			m_CurrentlyFocused = true;
			m_UpdateFlags |= UpdateProperties.Focussed;
		}
		m_ToolTipTimer = m_SecondsBeforeToolTip;
	}

	public void OnMouseExit()
	{
		if (m_CurrentlyFocused)
		{
			m_CurrentlyFocused = false;
			m_UpdateFlags |= UpdateProperties.Focussed;
		}
		HideToolTip();
	}

	public bool SetZoneAndRequirement(LevelEditor_ZoneManager.Zone zone, int requirementIndex)
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		BuildingBlockGroupManager instance2 = BuildingBlockGroupManager.GetInstance();
		if (!CacheController())
		{
			return false;
		}
		if (m_VisualMaster == null || instance == null || zone == null || instance2 == null || zone.m_ZoneDetails == null || zone.m_ZoneDetails.m_Requirements.Length <= m_RequirementIndex || requirementIndex < 0)
		{
			return false;
		}
		m_Zone = zone;
		m_CurrentZoneUpdateNumber = zone.m_ZoneUpdateCount - 1;
		m_CurrentCount = -1;
		m_Max = -1;
		m_Min = -1;
		m_RequirementIndex = requirementIndex;
		m_GroupIndex = m_Zone.m_ZoneDetails.m_Requirements[m_RequirementIndex].GetBlockSetIndex();
		m_Group = instance2.GetGroupByIndex(m_GroupIndex);
		if (m_Group != null)
		{
			m_BlockID = m_Group.m_Blocks[0];
		}
		if (!GetData())
		{
			m_UpdateFlags |= UpdateProperties.WaitingForData;
			m_Text.text = "---";
		}
		m_Initialized = true;
		m_UpdateFlags |= UpdateProperties.Zone;
		BlockChanged(m_CurrentBlock);
		return true;
	}

	private bool GetData()
	{
		LevelEditor_ZoneManager.Zone.StillRequired requireDataForBlockGroup = m_Zone.GetRequireDataForBlockGroup(m_GroupIndex);
		if (requireDataForBlockGroup == null)
		{
			return false;
		}
		m_UpdateFlags &= ~UpdateProperties.WaitingForData;
		if (requireDataForBlockGroup.m_Maximum != m_Max)
		{
			m_Max = requireDataForBlockGroup.m_Maximum;
			m_UpdateFlags |= UpdateProperties.Count;
		}
		if (requireDataForBlockGroup.m_Minimum != m_Min)
		{
			m_Min = requireDataForBlockGroup.m_Minimum;
			m_UpdateFlags |= UpdateProperties.Count;
		}
		if (requireDataForBlockGroup.m_CurrentTotal != m_CurrentCount)
		{
			m_CurrentCount = requireDataForBlockGroup.m_CurrentTotal;
			m_UpdateFlags |= UpdateProperties.Count;
		}
		if (m_CurrentlyValid != (requireDataForBlockGroup.m_Error == null))
		{
			m_CurrentlyValid = requireDataForBlockGroup.m_Error == null;
			m_UpdateFlags |= UpdateProperties.Validity;
		}
		return true;
	}

	public static LevelEditor_UIRequirement CreateUIRequirement(GameObject prefab, LevelEditor_ZoneManager.Zone zone, int requirementIndex, Transform ourParent)
	{
		if (prefab == null || zone == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, ourParent);
		if (gameObject != null)
		{
			gameObject.transform.localScale = Vector3.one;
			LevelEditor_UIRequirement componentInChildren = gameObject.GetComponentInChildren<LevelEditor_UIRequirement>();
			if (componentInChildren != null && componentInChildren.m_BackgroundRenderer != null && componentInChildren.m_NormalBackImage != null && componentInChildren.m_RedBackImage != null && componentInChildren.m_IconRenderer != null && componentInChildren.m_Tick != null && componentInChildren.m_Highlight != null && componentInChildren.m_Text != null && componentInChildren.SetZoneAndRequirement(zone, requirementIndex))
			{
				return componentInChildren;
			}
		}
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		return null;
	}

	public void BlockChanged(int iNewBlock)
	{
		m_CurrentBlock = iNewBlock;
		if (m_Group == null)
		{
			return;
		}
		for (int num = m_Group.m_Blocks.Length - 1; num >= 0; num--)
		{
			if (m_Group.m_Blocks[num] == iNewBlock)
			{
				if (!m_CurrentlyTicked)
				{
					m_CurrentlyTicked = true;
					m_UpdateFlags |= UpdateProperties.Ticked;
				}
				return;
			}
		}
		if (m_CurrentlyTicked)
		{
			m_CurrentlyTicked = false;
			m_UpdateFlags |= UpdateProperties.Ticked;
		}
	}

	private bool CacheController()
	{
		if (m_CachedController != null)
		{
			return true;
		}
		if ((m_CachedController = LevelEditor_Controller.GetInstance()) != null)
		{
			m_CachedController.RegisterBlockChange(BlockChanged);
			return true;
		}
		return false;
	}

	private void DisplayToolTip()
	{
		if (m_BlockID != -1 && LevelEditor_UIController.GetInstance() != null)
		{
			LevelEditor_UIController.GetInstance().DisplayToolTip(m_BlockID, Vector2.zero);
		}
	}

	private void HideToolTip()
	{
		m_ToolTipTimer = -1f;
		if (LevelEditor_UIController.GetInstance() != null)
		{
			LevelEditor_UIController.GetInstance().HideToolTip();
		}
	}
}
