using UnityEngine;
using UnityEngine.UI;

public class LevelEditor_CheckList_Entry : MonoBehaviour
{
	[HideInInspector]
	public int m_LimitationID = -1;

	public T17Text m_TitleText;

	public T17Text m_CountText;

	private T17Button m_Button;

	private bool m_bOverLimit;

	private LevelEditor_ZoneManager.Zone m_CurrentZone;

	private ZoneDetailsManager.ZoneTypes m_CurrentZoneType;

	private int m_LastIndexOfType;

	[Header("Normal Colour")]
	public ColorBlock m_NormalColourBlock;

	[Header("Error Colour")]
	public ColorBlock m_ErrorColourBlock;

	[Header("Over Max Colour")]
	public ColorBlock m_OverMaxColourBlock;

	private void Awake()
	{
		m_Button = GetComponent<T17Button>();
		if (m_Button != null)
		{
			m_Button.onClick.AddListener(OnClicked);
		}
		base.transform.localScale = Vector3.one;
	}

	public void OnClicked()
	{
		if (m_bOverLimit)
		{
			LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
			LevelEditor_Controller instance2 = LevelEditor_Controller.GetInstance();
			if (instance == null || instance2 == null)
			{
				return;
			}
			BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(m_LimitationID);
			if (limitationGroup == null || !limitationGroup.m_bValid)
			{
				return;
			}
			BaseLevelManager instance3 = BaseLevelManager.GetInstance();
			if (instance3 == null)
			{
				return;
			}
			BaseLevelManager.LevelLayers levelLayers = instance3.GetCurrentLayer();
			BaseLevelManager.LevelLayers levelLayers2 = levelLayers;
			LevelEditor_ZoneManager.Zone zone = null;
			do
			{
				zone = instance.GetZoneOfType(ref m_LastIndexOfType, limitationGroup.m_ZoneType);
				if (zone == null)
				{
					levelLayers++;
					levelLayers = (BaseLevelManager.LevelLayers)((int)levelLayers % 6);
					if (levelLayers == BaseLevelManager.LevelLayers.FirstFloor_Vent || levelLayers == BaseLevelManager.LevelLayers.GroundFloor_Vent)
					{
						levelLayers++;
					}
					m_LastIndexOfType = 0;
				}
				else if (zone.m_Layer == levelLayers && zone == m_CurrentZone)
				{
					break;
				}
			}
			while (zone == null || levelLayers != zone.m_Layer);
			if (zone != null)
			{
				m_CurrentZone = zone;
				if (levelLayers2 != zone.m_Layer)
				{
					instance2.ChangeLayer(zone.m_Layer);
				}
				instance2.ExternalSetCurrentZone(zone.m_ID);
				int num = zone.m_Left + zone.m_Width / 2;
				int num2 = zone.m_Bottom + zone.m_Height / 2;
				instance2.m_MainCamera.transform.localPosition = new Vector3((float)num - 60f, (float)num2 - 60f, instance2.m_MainCamera.transform.localPosition.z);
			}
		}
		else
		{
			LevelEditor_UIController instance4 = LevelEditor_UIController.GetInstance();
			if (instance4 != null)
			{
				instance4.SelectBlockFromLimitationsWindow(m_LimitationID);
			}
		}
	}

	public int GetGroupWeAreChecking()
	{
		return m_LimitationID;
	}

	public bool UpdateState()
	{
		m_bOverLimit = false;
		base.transform.localScale = Vector3.one;
		if (m_CountText != null && m_LimitationID != -1)
		{
			BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(m_LimitationID);
			if (limitationGroup != null && limitationGroup.m_bValid)
			{
				if (limitationGroup.m_CurrentTotal > limitationGroup.m_Max && limitationGroup.m_Max > 0)
				{
					m_bOverLimit = true;
					if (m_Button != null)
					{
						m_Button.colors = m_OverMaxColourBlock;
					}
					m_CountText.SetNonLocalizedText(limitationGroup.m_CurrentTotal + "/" + limitationGroup.m_Max);
					return true;
				}
				if (RoutineHelper.IsValid(limitationGroup.m_Routine) && LevelDetailsManager.GetInstance() != null && !LevelDetailsManager.GetInstance().DoWeHaveRoutineSet(limitationGroup.m_Routine))
				{
					return false;
				}
				int totalBlocks = BuildingBlockManager.GetInstance().GetTotalBlocks();
				for (int i = 0; i < totalBlocks; i++)
				{
					BaseBuildingBlock block = BuildingBlockManager.GetBlock(i);
					if (block != null && block.m_LimitationGroup == m_LimitationID && block.BlockType == BaseBuildingBlock.BuildingBlockType.Room && block != null && m_Button != null)
					{
						if (block.IsValidForLayer(BaseLevelManager.GetInstance().GetCurrentLayer()))
						{
							m_Button.colors = m_NormalColourBlock;
							break;
						}
						m_Button.colors = m_ErrorColourBlock;
					}
				}
				m_CountText.SetNonLocalizedText(limitationGroup.m_CurrentTotal + "/" + limitationGroup.m_Min);
				if (limitationGroup.m_CurrentTotal < limitationGroup.m_Min)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetLimitationID(int id)
	{
		m_LimitationID = id;
		if (m_LimitationID != -1)
		{
			if (m_TitleText != null)
			{
				BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(m_LimitationID);
				if (limitationGroup != null && limitationGroup.m_bValid)
				{
					m_TitleText.SetLocalisedTextCatchAll(limitationGroup.m_TextResourceName);
					if (limitationGroup.m_ZoneType != m_CurrentZoneType)
					{
						m_CurrentZone = null;
						m_LastIndexOfType = 0;
						m_CurrentZoneType = limitationGroup.m_ZoneType;
					}
				}
				else
				{
					base.gameObject.SetActive(value: false);
				}
			}
			base.gameObject.SetActive(UpdateState());
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
