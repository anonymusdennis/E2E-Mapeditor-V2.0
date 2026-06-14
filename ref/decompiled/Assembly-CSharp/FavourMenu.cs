using UnityEngine;

public class FavourMenu : GameMenuBehaviour
{
	public delegate void DialogEvent();

	public DialogEvent OnConfirm;

	public DialogEvent OnDecline;

	public DialogEvent OnCancel;

	public T17Text m_QuestTitleText;

	public T17Text m_QuestDescriptionText;

	public T17Text m_QuestGiverNameText;

	public T17Text m_QuestRewardText;

	public T17Text m_MultiPartText;

	public T17RawImage m_Avatar;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	private QuestManager.QuestGiver m_QuestGiver;

	public T17Image m_AvatarSprite;

	public Sprite m_OverRideSprite;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		DisableMultipart();
		QuestManager.GetInstance().InteractWithQuestGiver(m_GameMenuInformation.m_MenuRepresentative, m_GameMenuInformation.m_Player, this);
		m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		SetupAvatarRenderTexture();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		DestroyAvatarRenderTexture();
		m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		if (m_QuestGiver != null)
		{
			m_QuestGiver.ConfirmMenuClose();
		}
		m_QuestGiver = null;
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (m_QuestGiver != null)
		{
			m_CharacterRenderer.SetAndDrawCharacter(m_QuestGiver.m_QuestGiver.m_CharacterCustomisation, m_RenderTexture, Time.deltaTime);
		}
	}

	private void SetupAvatarRenderTexture()
	{
		DestroyAvatarRenderTexture();
		if (!(m_Avatar != null) || !(m_CharacterRenderer != null))
		{
			return;
		}
		T17RawImage componentInChildren = m_Avatar.GetComponentInChildren<T17RawImage>();
		CharacterRole characterRole = m_GameMenuInformation.m_MenuRepresentative.m_CharacterRole;
		if (characterRole == CharacterRole.Dolphin)
		{
			if (m_OverRideSprite != null && m_AvatarSprite != null)
			{
				m_AvatarSprite.enabled = true;
				m_Avatar.enabled = false;
				m_AvatarSprite.sprite = m_OverRideSprite;
			}
			return;
		}
		if (m_AvatarSprite != null)
		{
			m_AvatarSprite.enabled = false;
		}
		m_Avatar.enabled = true;
		int num = Mathf.FloorToInt(componentInChildren.rectTransform.rect.width);
		int num2 = Mathf.FloorToInt(componentInChildren.rectTransform.rect.height);
		if (num > 0 && num2 > 0)
		{
			m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextureID);
		}
		componentInChildren.texture = m_RenderTexture;
	}

	private void DestroyAvatarRenderTexture()
	{
		if (m_RenderTexture != null)
		{
			m_CharacterRenderer.CleanupRenderTexture(ref m_RenderTextureID);
			m_RenderTexture = null;
		}
		if (m_Avatar != null)
		{
			m_Avatar.texture = null;
		}
	}

	public void SetTitleText(string title)
	{
		if (m_QuestTitleText != null)
		{
			m_QuestTitleText.m_bNeedsLocalization = false;
			m_QuestTitleText.text = title;
		}
	}

	public void SetDescriptionText(string description)
	{
		if (m_QuestDescriptionText != null)
		{
			m_QuestDescriptionText.m_bNeedsLocalization = false;
			m_QuestDescriptionText.text = description;
		}
	}

	public void SetQuestGiverNameText(string characterName)
	{
		if (m_QuestGiverNameText != null)
		{
			m_QuestGiverNameText.m_bNeedsLocalization = false;
			m_QuestGiverNameText.text = characterName;
		}
	}

	public void SetQuestGiver(QuestManager.QuestGiver questGiver)
	{
		m_QuestGiver = questGiver;
	}

	public void SetQuestRewardText(ref QuestIntroObjective rewardInfo)
	{
		if (rewardInfo == null)
		{
			return;
		}
		m_QuestRewardText.m_bNeedsLocalization = false;
		if ((rewardInfo.Reward & QuestIntroObjective.RewardType.Escape) != 0)
		{
			string localized = string.Empty;
			if (Localization.Get("Text.Interact.Escape", out localized))
			{
				m_QuestRewardText.text = localized;
			}
		}
		else if ((rewardInfo.Reward & QuestIntroObjective.RewardType.Item) != 0)
		{
			string localized2 = string.Empty;
			if (Localization.Get(rewardInfo.ItemReward.m_ItemLocalizationTag, out localized2))
			{
				m_QuestRewardText.text = localized2;
			}
		}
		else if ((rewardInfo.Reward & QuestIntroObjective.RewardType.Money) != 0)
		{
			m_QuestRewardText.text = rewardInfo.MoneyReward.ToString();
		}
	}

	public string SetMultipart(int part, int maxParts)
	{
		string text = string.Empty;
		if (part > 0 && maxParts > 0)
		{
			part = Mathf.Clamp(part, 1, maxParts);
			if (m_MultiPartText != null)
			{
				m_MultiPartText.gameObject.SetActive(value: true);
				m_MultiPartText.m_bNeedsLocalization = false;
				text = $"{part}/{maxParts}";
				m_MultiPartText.text = text;
			}
		}
		return text;
	}

	public void DisableMultipart()
	{
		if (m_MultiPartText != null)
		{
			m_MultiPartText.gameObject.SetActive(value: false);
		}
	}

	public void Confirm()
	{
		if (OnConfirm != null)
		{
			OnConfirm();
		}
	}

	public void Decline()
	{
		if (OnDecline != null)
		{
			OnDecline();
		}
	}

	public virtual void Cancel()
	{
		if (OnCancel != null)
		{
			OnCancel();
		}
	}
}
