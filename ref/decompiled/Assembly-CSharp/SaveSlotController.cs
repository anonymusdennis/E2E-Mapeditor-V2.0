using UnityEngine;

public class SaveSlotController : MonoBehaviour
{
	public enum ControllingWhat
	{
		SaveSlot,
		ContinueButton,
		LoadGameButton,
		NewGame,
		NewGameCustom
	}

	[HideInInspector]
	public ControllingWhat m_Controlling;

	[HideInInspector]
	public int m_iSlotNumber;

	[HideInInspector]
	public T17Text m_Title;

	[HideInInspector]
	public T17Text m_Extra;

	[HideInInspector]
	public T17Image m_GameTypeImage;

	private T17Button m_ButtonClass;

	private SaveManager m_Manager;

	private bool m_IsLocked;

	[HideInInspector]
	public bool m_bLockedForCustomLevel;

	private FrontendMenuBehaviour m_ParentMenuBehaviour;

	private bool m_bIsButtonInteractable;

	public bool isLocked
	{
		set
		{
			m_IsLocked = value;
		}
	}

	private void Start()
	{
		m_Manager = SaveManager.GetInstance();
		if (m_Manager == null)
		{
			base.enabled = false;
		}
		else if (m_iSlotNumber < -1 || m_iSlotNumber >= SaveManager.GetTotalSlots())
		{
			base.enabled = false;
		}
		m_ButtonClass = GetComponent<T17Button>();
		m_ParentMenuBehaviour = GetComponentInParent<FrontendMenuBehaviour>();
	}

	protected virtual void OnDestroy()
	{
		m_Manager = null;
	}

	private void Update()
	{
		UpdateText();
		UpdateEnabledState();
	}

	private void UpdateText()
	{
		switch (m_Controlling)
		{
		case ControllingWhat.ContinueButton:
			break;
		case ControllingWhat.LoadGameButton:
			break;
		case ControllingWhat.NewGame:
			break;
		case ControllingWhat.SaveSlot:
		{
			if (!SaveManager.GetInstance().HasSomethingChanged(m_iSlotNumber))
			{
				break;
			}
			string strExtra = string.Empty;
			string strTitle = string.Empty;
			bool bTitleLocalized = false;
			bool bExtraLocalized = false;
			Sprite icon = null;
			SaveManager.GetInstance().GetSlotInfo(m_iSlotNumber, out strTitle, out bTitleLocalized, out strExtra, out bExtraLocalized, out icon);
			if (m_Title != null)
			{
				if (bTitleLocalized)
				{
					m_Title.m_bNeedsLocalization = false;
					m_Title.text = strTitle;
				}
				else
				{
					m_Title.m_bNeedsLocalization = true;
					m_Title.SetLocalisedTextCatchAll(strTitle);
				}
			}
			if (m_Extra != null)
			{
				if (bExtraLocalized)
				{
					m_Extra.m_bNeedsLocalization = false;
					m_Extra.m_LocalizationTag = null;
					m_Extra.text = strExtra;
				}
				else
				{
					m_Extra.m_bNeedsLocalization = true;
					m_Extra.SetLocalisedTextCatchAll(strExtra);
				}
			}
			if (m_GameTypeImage != null)
			{
				bool active = icon != null;
				m_GameTypeImage.gameObject.SetActive(active);
				m_GameTypeImage.sprite = icon;
			}
			break;
		}
		}
	}

	private void UpdateEnabledState()
	{
		if (m_ButtonClass == null)
		{
			return;
		}
		m_bIsButtonInteractable = true;
		switch (m_Controlling)
		{
		case ControllingWhat.ContinueButton:
			m_bIsButtonInteractable = m_Manager.CanWeContinue() && !CheckSelectedPrisonTutorial() && !m_IsLocked;
			break;
		case ControllingWhat.LoadGameButton:
			m_bIsButtonInteractable = m_Manager.CanWeContinue() && !CheckSelectedPrisonTutorial() && !m_IsLocked;
			break;
		case ControllingWhat.NewGame:
			m_bIsButtonInteractable = !m_IsLocked;
			break;
		case ControllingWhat.SaveSlot:
			m_bIsButtonInteractable = m_Manager.ShouldSaveSlotBeEnabled(m_iSlotNumber);
			break;
		}
		if (m_bLockedForCustomLevel)
		{
			m_bIsButtonInteractable = false;
		}
		if (m_bIsButtonInteractable != m_ButtonClass.interactable)
		{
			bool flag = m_ParentMenuBehaviour != null && m_ParentMenuBehaviour.CachedEventSystem != null;
			GameObject gameObject = null;
			if (!m_bIsButtonInteractable && flag && m_ParentMenuBehaviour.CachedEventSystem.currentSelectedGameObject == m_ButtonClass.gameObject)
			{
				gameObject = m_ParentMenuBehaviour.CachedEventSystem.currentSelectedGameObject;
				m_ParentMenuBehaviour.CachedEventSystem.ForceDeselectSelectionObject();
			}
			m_ButtonClass.interactable = m_bIsButtonInteractable;
			if (!m_bIsButtonInteractable && flag && gameObject == m_ButtonClass.gameObject)
			{
				m_ParentMenuBehaviour.CachedEventSystem.SetSelectedGameObject(m_ParentMenuBehaviour.FindValidSelectableForLostFocus());
			}
		}
	}

	public void OnSlotClicked()
	{
		if (!(m_Manager != null))
		{
			return;
		}
		switch (m_Controlling)
		{
		case ControllingWhat.ContinueButton:
			m_Manager.ContinueSelected();
			break;
		case ControllingWhat.LoadGameButton:
			if (m_Manager.CanSlotBeUsed(m_iSlotNumber))
			{
				m_Manager.LoadGameSelected();
			}
			break;
		case ControllingWhat.SaveSlot:
			m_Manager.SetSlotSelected(m_iSlotNumber);
			break;
		case ControllingWhat.NewGame:
			if (!m_IsLocked)
			{
				m_Manager.NewGameSelected(CheckSelectedPrisonTutorial());
			}
			break;
		case ControllingWhat.NewGameCustom:
			m_Manager.NewGameSelected(prisonTutorial: false);
			break;
		}
	}

	public void SetThisSlotsAsSelected()
	{
		m_Manager.SetDeleteSlotSelected(m_iSlotNumber);
	}

	private bool CheckSelectedPrisonTutorial()
	{
		if (GlobalStart.GetInstance() != null && GlobalStart.GetInstance().GetCurrentSelectedPrisonData() != null)
		{
			return GlobalStart.GetInstance().GetCurrentSelectedPrisonData().m_LevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Tutorial;
		}
		return false;
	}
}
