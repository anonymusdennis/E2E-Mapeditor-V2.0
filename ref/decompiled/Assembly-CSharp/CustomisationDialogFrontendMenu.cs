using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class CustomisationDialogFrontendMenu : FrontendMenuBehaviour
{
	public Selectable m_ConfirmButton;

	public Selectable m_CancelButton;

	[Header("Tab Settings")]
	public T17TabPanel m_CustomizationTabPanel;

	[Header("Avatar Settings")]
	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public T17RawImage m_Avatar;

	private CustomisationSet m_AvailableAppearances = new CustomisationSet();

	private CustomisationSet m_NewAppearances = new CustomisationSet();

	private CustomisationSet m_SeenAppearances = new CustomisationSet();

	private CustomisationCategorisedSet m_CategorisedAvailableAppearances = new CustomisationCategorisedSet();

	private Customisation m_Customisation;

	private Customisation m_TempCustomisation;

	private CustomisationConstraint m_Constraint;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	private int m_CurrentCustomisationIndex = -1;

	private bool m_bShowingCancelDialog;

	private ICustomisableCharacters m_ParentMenu;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (!(m_CustomizationTabPanel != null))
		{
			return;
		}
		for (int i = 0; i < m_CustomizationTabPanel.m_MenuBodies.Length; i++)
		{
			CustomisationDialogTabMenu customisationDialogTabMenu = (CustomisationDialogTabMenu)m_CustomizationTabPanel.m_MenuBodies[i];
			if (customisationDialogTabMenu != null)
			{
				customisationDialogTabMenu.SetDialogButtons(m_ConfirmButton, m_CancelButton);
			}
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		m_ParentMenu = (ICustomisableCharacters)parent;
		if (m_ParentMenu != null)
		{
			m_CurrentCustomisationIndex = m_ParentMenu.GetCurrentCustomsiationIndex();
			m_Customisation = m_ParentMenu.GetCustomisationToModify();
			m_Constraint = m_ParentMenu.GetCustomisationConstraint();
		}
		if (m_Customisation != null)
		{
			m_TempCustomisation = new Customisation(m_Customisation);
		}
		UnlockManager instance = UnlockManager.GetInstance();
		if (instance != null)
		{
			m_AvailableAppearances.Clear();
			m_AvailableAppearances.CopyData(instance.GetAllUnlockedCustomisations());
			m_NewAppearances.Clear();
			m_NewAppearances.CopyData(instance.GetNewCustomisations());
			m_SeenAppearances.Clear();
			if (m_Constraint != null)
			{
				CustomisationManager.ApplyConstraint(ref m_AvailableAppearances, m_Constraint);
				CustomisationManager.ApplyConstraint(ref m_NewAppearances, m_Constraint);
			}
			if (!CustomisationManager.FilterAppearance(ref m_TempCustomisation, m_AvailableAppearances, (!(m_Constraint != null)) ? null : m_Constraint.fallback))
			{
			}
			instance.CategoriseSet(m_AvailableAppearances, ref m_CategorisedAvailableAppearances);
		}
		if (m_CustomizationTabPanel != null)
		{
			List<BaseMenuBehaviour> list = new List<BaseMenuBehaviour>(m_CustomizationTabPanel.m_MenuBodies);
			for (int i = 0; i < list.Count; i++)
			{
				CustomisationDialogTabMenu customisationDialogTabMenu = (CustomisationDialogTabMenu)list[i];
				if (customisationDialogTabMenu != null)
				{
					customisationDialogTabMenu.SetAvailableOptions(m_AvailableAppearances, m_CategorisedAvailableAppearances);
					customisationDialogTabMenu.SetVisibilityOptions(m_NewAppearances, m_SeenAppearances);
					customisationDialogTabMenu.SetModifiableCustomisation(m_TempCustomisation);
					customisationDialogTabMenu.UpdateNewOptionsIcon();
					customisationDialogTabMenu.m_bMenuIsEnabled = customisationDialogTabMenu.HasAvailableOptions();
				}
			}
			m_CustomizationTabPanel.Show(currentGamer, this, null);
			m_CustomizationTabPanel.SetMenuBodies(list);
			m_CustomizationTabPanel.AttemptToSetTabIndex(0);
		}
		m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		SetupRenderTexture();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		DestroyRenderTexture();
		for (int i = 0; i < m_CustomizationTabPanel.m_MenuBodies.Length; i++)
		{
			CustomisationDialogTabMenu customisationDialogTabMenu = (CustomisationDialogTabMenu)m_CustomizationTabPanel.m_MenuBodies[i];
			if (customisationDialogTabMenu != null)
			{
				customisationDialogTabMenu.SetAvailableOptions(null, null);
				customisationDialogTabMenu.SetVisibilityOptions(null, null);
				customisationDialogTabMenu.SetModifiableCustomisation(null);
			}
		}
		UnlockManager instance = UnlockManager.GetInstance();
		if (instance != null)
		{
			instance.MarkCustomisationSetAsSeen(m_SeenAppearances);
		}
		m_SeenAppearances.Clear();
		m_TempCustomisation = null;
		m_Customisation = null;
		m_CurrentCustomisationIndex = -1;
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (m_CharacterRenderer != null && m_RenderTexture != null && m_TempCustomisation != null)
		{
			m_CharacterRenderer.UpdateAnimation(Time.deltaTime);
			if (m_CurrentCustomisationIndex != -1)
			{
				Customisation customisation = new Customisation(m_TempCustomisation);
				OverrideCustomisation(ref customisation, m_CurrentCustomisationIndex);
				m_CharacterRenderer.SetCustomisation(customisation);
			}
			else
			{
				m_CharacterRenderer.SetCustomisation(m_TempCustomisation);
			}
			m_CharacterRenderer.DrawCharacter(m_RenderTexture);
		}
		if (base.CurrentRewiredPlayer != null && !m_bShowingCancelDialog && base.CurrentRewiredPlayer.GetButtonDown("UI_Save"))
		{
			Confirm();
		}
	}

	private void SetupRenderTexture()
	{
		DestroyRenderTexture();
		int num = Mathf.FloorToInt(m_Avatar.rectTransform.rect.width);
		int num2 = Mathf.FloorToInt(m_Avatar.rectTransform.rect.height);
		if (num > 0 && num2 > 0)
		{
			m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextureID);
		}
		m_Avatar.texture = m_RenderTexture;
	}

	private void DestroyRenderTexture()
	{
		if (m_RenderTexture != null)
		{
			m_CharacterRenderer.CleanupRenderTexture(ref m_RenderTextureID);
			m_RenderTexture = null;
		}
		m_Avatar.texture = null;
	}

	public void Confirm()
	{
		if (m_TempCustomisation != null && m_Customisation != null)
		{
			if (!string.IsNullOrEmpty(m_TempCustomisation.name))
			{
				string text = Regex.Replace(m_TempCustomisation.name, "[^\\p{L}0-9 _!]", string.Empty);
				if (!string.IsNullOrEmpty(text) && Regex.IsMatch(text, "[^\\s]"))
				{
					m_Customisation.name = text;
				}
			}
			m_Customisation.body = m_TempCustomisation.body;
			m_Customisation.skin = m_TempCustomisation.skin;
			m_Customisation.hair = m_TempCustomisation.hair;
			m_Customisation.hat = m_TempCustomisation.hat;
			m_Customisation.upperFace = m_TempCustomisation.upperFace;
			m_Customisation.lowerFace = m_TempCustomisation.lowerFace;
			if (m_ParentMenu != null)
			{
				m_ParentMenu.OnCustomisationModified();
			}
		}
		Close();
	}

	public void AskIfClose()
	{
		bool flag = false;
		if (m_TempCustomisation != null && m_Customisation != null)
		{
			flag |= string.Compare(m_TempCustomisation.name, m_Customisation.name, StringComparison.InvariantCultureIgnoreCase) != 0;
			flag |= m_TempCustomisation.body != m_Customisation.body;
			flag |= m_TempCustomisation.skin != m_Customisation.skin;
			flag |= m_TempCustomisation.hair != m_Customisation.hair;
			flag |= m_TempCustomisation.hat != m_Customisation.hat;
			flag |= m_TempCustomisation.upperFace != m_Customisation.upperFace;
			flag |= m_TempCustomisation.lowerFace != m_Customisation.lowerFace;
		}
		m_bShowingCancelDialog = false;
		if (flag)
		{
			ShowDialog("Text.Menu.Customisation.CancelDialog.Title", "Text.Menu.Customisation.CancelDialog.Message", OverwriteResponceYes, OverwriteResponceNo, T17DialogBox.Symbols.Warning);
			m_bShowingCancelDialog = true;
		}
		else
		{
			Close();
		}
	}

	private void OverwriteResponceYes(T17DialogBox dialog)
	{
		m_bShowingCancelDialog = false;
		Close();
	}

	private void OverwriteResponceNo(T17DialogBox dialog)
	{
		m_bShowingCancelDialog = false;
	}

	public void ShowDialog(string strTitle, string strMessage, T17DialogBox.DialogEvent eventYes, T17DialogBox.DialogEvent eventNo, T17DialogBox.Symbols symbol)
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, strTitle, strMessage, "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			dialog.SetSymbol(symbol);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, (T17DialogBox.DialogEvent)delegate(T17DialogBox sender)
			{
				if (eventYes != null)
				{
					eventYes(sender);
				}
			});
			dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, (T17DialogBox.DialogEvent)delegate(T17DialogBox sender)
			{
				if (eventNo != null)
				{
					eventNo(sender);
				}
			});
			dialog.Show();
		}
		else if (eventNo != null)
		{
			eventNo(dialog);
		}
	}

	public Customisation GetModifiableCustomisation()
	{
		return m_TempCustomisation;
	}

	private void OverrideCustomisation(ref Customisation customisation, int index)
	{
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (!(currentSelectedPrisonData != null) || index < 0 || index >= currentSelectedPrisonData.m_RoleStartingOutfitData.Length)
		{
			return;
		}
		Item_Outfit item_Outfit = null;
		if (!(currentSelectedPrisonData.m_RoleStartingOutfitData[index] != null))
		{
			return;
		}
		item_Outfit = currentSelectedPrisonData.m_RoleStartingOutfitData[index].m_OutfitData;
		List<ItemDataConfig> list = null;
		for (int i = 0; i < currentSelectedPrisonData.m_Configs.Count; i++)
		{
			if (currentSelectedPrisonData.m_Configs[i].m_ConfigType == PrisonConfig.ConfigType.Cooperative)
			{
				list = currentSelectedPrisonData.m_Configs[i].m_ItemDataOverrides;
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].m_ItemDataID == currentSelectedPrisonData.m_RoleStartingOutfitData[index].m_ItemDataID)
				{
					item_Outfit.m_HairOverride = list[j].m_HairOverride;
					item_Outfit.m_HatOverride = list[j].m_HatOverride;
					item_Outfit.m_LowerFaceOverride = list[j].m_LowerFaceOverride;
					item_Outfit.m_OutfitAppearance = list[j].m_OutfitAppearance;
					item_Outfit.m_UpperFaceOverride = list[j].m_UpperFaceOverride;
				}
			}
		}
		if (item_Outfit.m_HairOverride != CustomisationData.Hair.NULL)
		{
			customisation.hair = item_Outfit.m_HairOverride;
		}
		if (item_Outfit.m_HatOverride != CustomisationData.Hat.NULL)
		{
			customisation.hat = item_Outfit.m_HatOverride;
		}
		if (item_Outfit.m_LowerFaceOverride != CustomisationData.LowerFaceAccessory.NULL)
		{
			customisation.lowerFace = item_Outfit.m_LowerFaceOverride;
		}
		if (item_Outfit.m_OutfitAppearance != CustomisationData.Outfit.NULL)
		{
			customisation.defaultOutfit = item_Outfit.m_OutfitAppearance;
		}
		if (item_Outfit.m_UpperFaceOverride != CustomisationData.UpperFaceAccessory.NULL)
		{
			customisation.upperFace = item_Outfit.m_UpperFaceOverride;
		}
	}
}
