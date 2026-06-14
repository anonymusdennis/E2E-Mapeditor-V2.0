using System;
using UnityEngine;

public class BuildingBlock_UIButton : MonoBehaviour
{
	[Flags]
	public enum ButtonHighlightState
	{
		Nothing = 0,
		Over = 1,
		Limited = 2,
		Selected = 4,
		HighlightFlags = 3
	}

	[Flags]
	public enum ChangedStates
	{
		Nothing = 0,
		HightLight = 1,
		Selected = 2,
		Block = 4,
		Number = 8,
		EVERTHING = 0xF
	}

	public T17RawImage m_BlockIcon;

	public T17Image m_ButtonTick;

	public T17Image m_ButtonHighlight;

	public T17Image m_ButtonDisabled;

	public T17Text m_ButtonNumberText;

	public GameObject m_ButtonNumber;

	public float m_SecondsBeforeToolTip = 2f;

	private ButtonHighlightState m_ButtonHighlightState;

	private ChangedStates m_SomethingHasChanged = ChangedStates.EVERTHING;

	private int m_BlockID = -1;

	private LevelEditor_Controller m_CachedController;

	private BuildingBlockManager m_CachedBlockManager;

	private int m_LimitationGroup = -1;

	private float m_ToolTipTimer = -1f;

	private void Start()
	{
	}

	private void Update()
	{
		if (m_ButtonDisabled != null && m_ButtonHighlight != null && m_ButtonTick != null && m_BlockIcon != null && m_ButtonNumber != null && m_ButtonNumberText != null && CacheController() && CachedBlockManager() && m_SomethingHasChanged != 0)
		{
			UpdateLook();
		}
		if (m_ToolTipTimer >= 0f)
		{
			m_ToolTipTimer -= Time.deltaTime;
			if (m_ToolTipTimer < 0f)
			{
				DisplayToolTip();
			}
		}
	}

	[ContextMenu("Test")]
	public void Test()
	{
		SetBlockID(0);
	}

	public void SetBlockID(int iID)
	{
		if (m_BlockID != iID)
		{
			m_BlockID = iID;
			if (m_BlockID != -1 && CacheController() && CachedBlockManager() && m_CachedBlockManager.IsBlockVarientOfThisBlock(m_CachedController.GetSelectedBlock(), m_BlockID))
			{
				m_ButtonHighlightState |= ButtonHighlightState.Selected;
			}
			else
			{
				m_ButtonHighlightState &= ~ButtonHighlightState.Selected;
			}
			m_ButtonHighlightState &= ~ButtonHighlightState.Over;
			m_ButtonHighlightState &= ~ButtonHighlightState.Limited;
			m_SomethingHasChanged |= ChangedStates.EVERTHING;
			if (base.gameObject.activeSelf != (m_BlockID != -1))
			{
				base.gameObject.SetActive(m_BlockID != -1);
			}
		}
	}

	private void SetBlockIcon(Material blockMaterial)
	{
		if (m_BlockIcon != null)
		{
			if (blockMaterial == null)
			{
				m_BlockIcon.texture = null;
				return;
			}
			m_BlockIcon.texture = blockMaterial.mainTexture;
			Rect uvRect = new Rect(blockMaterial.GetTextureOffset("_MainTex"), blockMaterial.GetTextureScale("_MainTex"));
			m_BlockIcon.uvRect = uvRect;
		}
	}

	private bool CacheController()
	{
		if (m_CachedController != null)
		{
			return true;
		}
		m_CachedController = LevelEditor_Controller.GetInstance();
		if (m_CachedController != null)
		{
			m_CachedController.RegisterBlockChange(BlockChanged);
			return true;
		}
		return false;
	}

	private bool CachedBlockManager()
	{
		if (m_CachedBlockManager != null)
		{
			return true;
		}
		m_CachedBlockManager = BuildingBlockManager.GetInstance();
		if (m_CachedBlockManager != null)
		{
			m_CachedBlockManager.RegisterLimitationChange(LimitationGroupChanged);
			return true;
		}
		return false;
	}

	private void UpdateLook()
	{
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_BlockID);
		if ((m_SomethingHasChanged & ChangedStates.Block) == ChangedStates.Block)
		{
			int num = -1;
			if (block == null)
			{
				SetBlockIcon(null);
			}
			else
			{
				SetBlockIcon(block.m_UIImage);
				num = block.m_LimitationGroup;
			}
			if (num != m_LimitationGroup)
			{
				m_LimitationGroup = num;
				LimitationGroupChanged(num);
			}
			m_SomethingHasChanged &= ~ChangedStates.Block;
		}
		if ((m_SomethingHasChanged & ChangedStates.Selected) == ChangedStates.Selected)
		{
			m_ButtonTick.gameObject.SetActive((m_ButtonHighlightState & ButtonHighlightState.Selected) == ButtonHighlightState.Selected);
			m_SomethingHasChanged &= ~ChangedStates.Selected;
		}
		if ((m_SomethingHasChanged & ChangedStates.HightLight) == ChangedStates.HightLight)
		{
			if ((m_ButtonHighlightState & ButtonHighlightState.Over) == ButtonHighlightState.Over)
			{
				m_ButtonHighlight.gameObject.SetActive(value: true);
				m_ButtonDisabled.gameObject.SetActive(value: false);
			}
			else if ((m_ButtonHighlightState & ButtonHighlightState.Limited) == ButtonHighlightState.Limited)
			{
				m_ButtonHighlight.gameObject.SetActive(value: false);
				m_ButtonDisabled.gameObject.SetActive(value: true);
			}
			else
			{
				m_ButtonHighlight.gameObject.SetActive(value: false);
				m_ButtonDisabled.gameObject.SetActive(value: false);
			}
			m_SomethingHasChanged &= ~ChangedStates.HightLight;
		}
		if ((m_SomethingHasChanged & ChangedStates.Number) == ChangedStates.Number)
		{
			if (block != null && block.m_VariationNumber != -1)
			{
				m_ButtonNumberText.SetNewLocalizationTag("Text.Editor.Block.Group." + block.m_VariationNumber);
				m_ButtonNumber.gameObject.SetActive(value: true);
			}
			else
			{
				m_ButtonNumber.gameObject.SetActive(value: false);
			}
			m_SomethingHasChanged &= ~ChangedStates.Number;
		}
	}

	public void MouseOver()
	{
		if ((m_ButtonHighlightState & ButtonHighlightState.Over) != ButtonHighlightState.Over)
		{
			m_ButtonHighlightState |= ButtonHighlightState.Over;
			m_SomethingHasChanged |= ChangedStates.HightLight;
		}
		m_ToolTipTimer = m_SecondsBeforeToolTip;
	}

	public void MouseLeave()
	{
		if ((m_ButtonHighlightState & ButtonHighlightState.Over) == ButtonHighlightState.Over)
		{
			m_ButtonHighlightState &= ~ButtonHighlightState.Over;
			m_SomethingHasChanged |= ChangedStates.HightLight;
		}
		HideToolTip();
	}

	public void OnSelected()
	{
		if (base.enabled && LevelEditor_Controller.GetInstance() != null)
		{
			LevelEditor_Controller.GetInstance().ExternalSelectBlock(m_BlockID);
		}
	}

	public void BlockChanged(int iNewBlock)
	{
		if (!AreWeAVariation(iNewBlock))
		{
			if ((m_ButtonHighlightState & ButtonHighlightState.Selected) == ButtonHighlightState.Selected)
			{
				m_ButtonHighlightState &= ~ButtonHighlightState.Selected;
				m_SomethingHasChanged |= ChangedStates.Selected;
			}
		}
		else if ((m_ButtonHighlightState & ButtonHighlightState.Selected) != ButtonHighlightState.Selected)
		{
			m_ButtonHighlightState |= ButtonHighlightState.Selected;
			m_SomethingHasChanged |= ChangedStates.Selected;
		}
	}

	public void LimitationGroupChanged(int iLimitGroup)
	{
		if (m_BlockID == -1 || m_LimitationGroup == -1 || iLimitGroup != m_LimitationGroup)
		{
			return;
		}
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_BlockID);
		if (!(block == null))
		{
			BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(iLimitGroup);
			if (limitationGroup.m_CurrentTotal + block.m_LimitationCount > limitationGroup.m_Max && limitationGroup.m_Max != 0)
			{
				m_ButtonHighlightState |= ButtonHighlightState.Limited;
			}
			else
			{
				m_ButtonHighlightState &= ~ButtonHighlightState.Limited;
			}
			m_SomethingHasChanged |= ChangedStates.HightLight;
		}
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

	private bool AreWeAVariation(int iBlockID)
	{
		if (iBlockID == m_BlockID)
		{
			return true;
		}
		if (CachedBlockManager())
		{
			return m_CachedBlockManager.IsBlockVarientOfThisBlock(m_BlockID, iBlockID);
		}
		return false;
	}

	public int GetLimitationGroup()
	{
		return m_LimitationGroup;
	}
}
