using UnityEngine;

public class LevelEditor_ZoneTab : MonoBehaviour
{
	public LevelEditor_ZoneCard m_ZoneSelectedInvalidCard;

	public LevelEditor_ZoneCard m_ZoneSelectedValidCard;

	public LevelEditor_ZoneCard m_NoZoneSelectedCard;

	public LevelEditor_ZoneCard m_CreatingNewZoneCard;

	public LevelEditor_RequirementsPopulator m_RequirementBlocks;

	public LevelEditor_BlockSection m_RecommendedBlocks;

	private LevelEditor_GridCellPopulator m_RecommendedBlocksObjectGrid;

	private LevelEditor_ZoneManager.Zone m_CurrentZone;

	private int m_CurrentZoneUpdateCount = -1;

	private bool m_bCompletedFirstSetup;

	private LevelEditor_Controller m_LevelEditorController;

	private ZoneDetailsManager m_ZoneDeatilsManager;

	public GameObject m_CreateZoneButtonDisabled;

	public T17Button m_CreateZoneButton;

	private BaseLevelManager m_LevelManager;

	private BaseLevelManager.LevelLayers m_CurrentLayer = BaseLevelManager.LevelLayers.TOTAL;

	private ZoneDetailsManager.ZoneTypes m_CurrentZoneToCreateType;

	private LevelEditor_Controller.EditMode m_CurrentEditMode;

	private bool m_HasChangedEditMode;

	private void Awake()
	{
		m_RecommendedBlocksObjectGrid = m_RecommendedBlocks.GetComponentInChildren<LevelEditor_GridCellPopulator>();
	}

	private void Start()
	{
		m_LevelManager = BaseLevelManager.GetInstance();
		m_LevelEditorController = LevelEditor_Controller.GetInstance();
		m_ZoneDeatilsManager = ZoneDetailsManager.GetInstance();
		m_LevelEditorController.RegisterEditModeChange(EditModeChanged);
		m_bCompletedFirstSetup = false;
		Update();
	}

	private void OnEnable()
	{
		m_bCompletedFirstSetup = false;
		Update();
	}

	private void Update()
	{
		if (m_LevelEditorController == null)
		{
			m_LevelEditorController = LevelEditor_Controller.GetInstance();
		}
		if (m_LevelEditorController == null)
		{
			return;
		}
		if (m_LevelManager == null)
		{
			m_LevelManager = BaseLevelManager.GetInstance();
		}
		if (m_LevelManager == null)
		{
			return;
		}
		if (m_CurrentLayer != m_LevelManager.m_CurrentLayer)
		{
			if (m_LevelManager.m_CurrentLayer == BaseLevelManager.LevelLayers.FirstFloor_Vent || m_LevelManager.m_CurrentLayer == BaseLevelManager.LevelLayers.GroundFloor_Vent)
			{
				m_CreateZoneButton.interactable = false;
				m_CreateZoneButtonDisabled.SetActive(value: true);
			}
			else
			{
				m_CreateZoneButtonDisabled.SetActive(value: false);
				m_CreateZoneButton.interactable = true;
			}
			m_CurrentLayer = m_LevelManager.m_CurrentLayer;
		}
		if (m_bCompletedFirstSetup && !m_HasChangedEditMode && m_CurrentZoneToCreateType == m_LevelEditorController.ZoneToCreate && m_CurrentZone == m_LevelEditorController.CurrentZone && (m_CurrentZone == null || m_CurrentZoneUpdateCount == m_CurrentZone.m_ZoneUpdateCount))
		{
			return;
		}
		m_CurrentEditMode = m_LevelEditorController.GetNoneTemporaryEditMode();
		m_HasChangedEditMode = false;
		m_bCompletedFirstSetup = true;
		m_CurrentZone = m_LevelEditorController.CurrentZone;
		m_CurrentZoneToCreateType = m_LevelEditorController.ZoneToCreate;
		if (m_CurrentZone == null)
		{
			if (m_CurrentEditMode == LevelEditor_Controller.EditMode.Zone_WaitingToCreate || m_CurrentEditMode == LevelEditor_Controller.EditMode.Zone_Creating)
			{
				m_CreatingNewZoneCard.gameObject.SetActive(value: true);
				m_NoZoneSelectedCard.gameObject.SetActive(value: false);
				m_CreatingNewZoneCard.SetCardDataFromDetails(m_ZoneDeatilsManager.GetZoneDetails(m_CurrentZoneToCreateType), bisValid: true);
			}
			else
			{
				m_NoZoneSelectedCard.gameObject.SetActive(value: true);
				m_CreatingNewZoneCard.gameObject.SetActive(value: false);
			}
			m_ZoneSelectedValidCard.gameObject.SetActive(value: false);
			m_ZoneSelectedInvalidCard.gameObject.SetActive(value: false);
			m_RequirementBlocks.gameObject.SetActive(value: false);
			m_RequirementBlocks.SetZone(null);
			m_NoZoneSelectedCard.SetCardDataForZone(m_CurrentZone);
			m_RecommendedBlocksObjectGrid.Family = 0L;
			m_CurrentZoneUpdateCount = -1;
			return;
		}
		if (m_CurrentZone.IsFullyValid())
		{
			m_ZoneSelectedValidCard.gameObject.SetActive(value: true);
			m_NoZoneSelectedCard.gameObject.SetActive(value: false);
			m_ZoneSelectedInvalidCard.gameObject.SetActive(value: false);
			m_CreatingNewZoneCard.gameObject.SetActive(value: false);
			m_ZoneSelectedValidCard.SetCardDataForZone(m_CurrentZone);
		}
		else
		{
			m_ZoneSelectedInvalidCard.gameObject.SetActive(value: true);
			m_ZoneSelectedValidCard.gameObject.SetActive(value: false);
			m_NoZoneSelectedCard.gameObject.SetActive(value: false);
			m_CreatingNewZoneCard.gameObject.SetActive(value: false);
			m_ZoneSelectedInvalidCard.SetCardDataForZone(m_CurrentZone);
		}
		m_RequirementBlocks.gameObject.SetActive(m_CurrentZone.m_ZoneDetails.m_Requirements.Length != 0);
		m_RequirementBlocks.SetZone(m_CurrentZone);
		m_RecommendedBlocksObjectGrid.Family = m_CurrentZone.m_ZoneDetails.m_Family;
		m_CurrentZoneUpdateCount = m_CurrentZone.m_ZoneUpdateCount;
	}

	private void EditModeChanged(LevelEditor_Controller.EditMode newMode)
	{
		m_HasChangedEditMode = true;
	}
}
