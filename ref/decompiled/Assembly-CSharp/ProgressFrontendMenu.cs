using System.Collections.Generic;
using UnityEngine;

public class ProgressFrontendMenu : FrontendMenuBehaviour
{
	[Header("Stat Settings")]
	public T17Text m_UsernameLabel;

	public T17Text m_TimeServedLabel;

	public T17Text m_UnlocksLabel;

	public T17Text m_CompletionPercentageLabel;

	public T17Text m_TitleLabel;

	[Header("Milestone Settings")]
	public MilestoneCarousel m_MilestoneCarousel;

	public List<MilestonePage> m_MilestonePages = new List<MilestonePage>();

	[Header("Avatar Settings")]
	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public T17RawImage m_Avatar;

	private Customisation m_PlayerAppearance = new Customisation();

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		SetupMilestoneCarouselOptions();
		UpdateStats();
		CustomisationManager instance = CustomisationManager.GetInstance();
		if (instance != null)
		{
			Customisation[] playerPresets = instance.GetPlayerPresets();
			int playerLastChosenPreset = instance.GetPlayerLastChosenPreset();
			if (playerLastChosenPreset >= 0 && playerLastChosenPreset < playerPresets.Length)
			{
				m_PlayerAppearance = playerPresets[playerLastChosenPreset];
			}
			else
			{
				m_PlayerAppearance = instance.DefaultCustomisation;
			}
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
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (m_CharacterRenderer != null && m_RenderTexture != null && m_PlayerAppearance != null)
		{
			m_CharacterRenderer.UpdateAnimation(Time.deltaTime);
			m_CharacterRenderer.SetCustomisation(m_PlayerAppearance);
			m_CharacterRenderer.DrawCharacter(m_RenderTexture);
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

	private void UpdateStats()
	{
		if (m_UsernameLabel != null)
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null)
			{
				m_UsernameLabel.m_bNeedsLocalization = false;
				m_UsernameLabel.text = primaryGamer.m_GamerName;
			}
			else
			{
				m_UsernameLabel.m_bNeedsLocalization = false;
				m_UsernameLabel.text = string.Empty;
			}
		}
		float percentage = 0f;
		string newLocalizationTag = string.Empty;
		int num = 0;
		int num2 = 0;
		ProgressManager instance = ProgressManager.GetInstance();
		if (instance != null)
		{
			percentage = instance.CalculateOverallProgress();
			newLocalizationTag = instance.CalculateTitleFromCompletion(percentage);
			num = instance.GetHoursInPrison();
			num2 = instance.GetTimedUnlocksUnlocked();
		}
		if (m_TimeServedLabel != null)
		{
			float num3 = (float)num / 24f;
			string localized = string.Empty;
			if (Localization.Get(m_TimeServedLabel.m_LocalizationTag, out localized))
			{
				string text = localized.Replace("$days", num3.ToString("####0.00"));
				m_TimeServedLabel.m_bNeedsLocalization = false;
				m_TimeServedLabel.text = text;
			}
			else
			{
				m_TimeServedLabel.m_bNeedsLocalization = false;
				m_TimeServedLabel.text = num3.ToString("####0.00");
			}
		}
		if (m_CompletionPercentageLabel != null)
		{
			m_CompletionPercentageLabel.m_bNeedsLocalization = false;
			m_CompletionPercentageLabel.text = percentage.ToString("P2");
		}
		if (m_TitleLabel != null)
		{
			m_TitleLabel.m_bNeedsLocalization = true;
			m_TitleLabel.SetNewLocalizationTag(newLocalizationTag);
		}
		if (m_UnlocksLabel != null)
		{
			m_UnlocksLabel.m_bNeedsLocalization = false;
			m_UnlocksLabel.text = num2.ToString("####0");
		}
	}

	private void SetupMilestoneCarouselOptions()
	{
		LevelDataManager instance = LevelDataManager.GetInstance();
		if (!(m_MilestoneCarousel != null) || !(instance != null))
		{
			return;
		}
		List<MilestonePage> list = new List<MilestonePage>();
		int num = 0;
		int num2 = 0;
		MilestonePage milestonePage = null;
		while (num < m_MilestonePages.Count && num2 < m_MilestonePages[num].m_Milestones.Count)
		{
			MilestonePage milestonePage2 = m_MilestonePages[num];
			if (milestonePage == null || milestonePage.m_Milestones.Count == 3)
			{
				milestonePage = new MilestonePage();
				milestonePage.m_HeaderTag = milestonePage2.m_HeaderTag;
				list.Add(milestonePage);
			}
			ProgressMilestone progressMilestone = milestonePage2.m_Milestones[num2];
			if (instance.IsLevelAvailable(progressMilestone.m_prison))
			{
				milestonePage.m_MilestonePrefabs.Add(milestonePage2.m_MilestonePrefabs[num2]);
				milestonePage.m_Milestones.Add(milestonePage2.m_Milestones[num2]);
			}
			num2++;
			if (num2 >= milestonePage2.m_Milestones.Count)
			{
				num++;
				num2 = 0;
			}
		}
		m_MilestoneCarousel.SetCarouselOptions(list);
	}
}
