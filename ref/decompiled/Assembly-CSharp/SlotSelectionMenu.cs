using UnityEngine;

public class SlotSelectionMenu : FrontendMenuBehaviour
{
	public T17Text m_PrisonTitle;

	public T17Text m_PurposeTitle;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (SaveManager.GetInstance() == null)
		{
			return false;
		}
		if (m_PrisonTitle != null)
		{
			if (SaveManager.GetInstance().GetPrisonID() == SaveManager.PrisonsSaveInformation.PrisonData.PrisonType.eDefault)
			{
				m_PrisonTitle.SetLocalisedTextCatchAll("Text.Prison." + SaveManager.GetInstance().GetPrisonName());
			}
			else
			{
				m_PrisonTitle.SetNonLocalizedText(SaveManager.GetInstance().GetPrisonName());
			}
		}
		if (m_PurposeTitle != null)
		{
			m_PurposeTitle.SetNewLocalizationTag(SaveManager.GetInstance().GetPurposeText());
		}
		SaveManager.GetInstance().SetSlotSelectionMenu(this);
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (SaveManager.GetInstance() == null)
		{
			return false;
		}
		SaveManager.GetInstance().SetSlotSelectionMenu(null);
		return true;
	}

	public void ResetSaveManagerUIMode()
	{
		SaveManager.GetInstance().ResetUIMode();
	}
}
