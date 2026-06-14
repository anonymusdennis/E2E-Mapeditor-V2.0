using UnityEngine;

public class LevelEditor_FilterButton : BaseLevelEditor_BasePopout
{
	public T17Image m_ThemeImage;

	public T17Text m_ThemeTextbox;

	private BaseBuildingBlock.BlockSet m_CurrentTheme;

	public T17Image m_BGImage;

	public Sprite m_SpriteBGActive;

	public Sprite m_SpriteBGInactive;

	public GameObject m_FilterMenu;

	private bool m_bMenuOpen;

	private void Update()
	{
		if (!(BuildingBlock_FilterManager.GetInstance() != null) || !(BuildingBlockManager.GetInstance() != null))
		{
			return;
		}
		BaseBuildingBlock.BlockSet currentRoomBlockSetFilter = BuildingBlock_FilterManager.GetInstance().GetCurrentRoomBlockSetFilter();
		if (m_CurrentTheme != currentRoomBlockSetFilter)
		{
			m_CurrentTheme = currentRoomBlockSetFilter;
			if (m_ThemeImage != null)
			{
				m_ThemeImage.sprite = BuildingBlock_FilterManager.GetInstance().GetThemeSprite(m_CurrentTheme);
			}
			if (m_ThemeTextbox != null)
			{
				m_ThemeTextbox.SetNewLocalizationTag(BuildingBlock_FilterManager.GetInstance().GetThemeTextResource(m_CurrentTheme));
			}
			LevelEditor_Controller.GetInstance().ExternalSelectBlock(-1);
		}
	}

	public void ToggleActiveState()
	{
		m_bMenuOpen = !m_bMenuOpen;
		m_FilterMenu.SetActive(m_bMenuOpen);
		if (m_bMenuOpen)
		{
			m_BGImage.sprite = m_SpriteBGActive;
			OnShow();
		}
		else
		{
			m_BGImage.sprite = m_SpriteBGInactive;
			OnHide();
		}
	}

	public override void Hide()
	{
		if (m_bMenuOpen)
		{
			m_bMenuOpen = false;
			m_FilterMenu.SetActive(m_bMenuOpen);
			m_BGImage.sprite = m_SpriteBGInactive;
			OnHide();
		}
	}
}
