using UnityEngine;
using UnityEngine.EventSystems;

public class Results_CharacterProfile : MonoBehaviour
{
	public T17Button m_AvatarButton;

	public T17RawImage m_Avatar;

	public T17Text m_UsernameLabel;

	public T17Text m_CharacterNameLabel;

	public T17Image m_WinnerPlaque;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	[Header("Awards")]
	public GameObject m_MostCraftedAward;

	public GameObject m_MostKnockoutsAward;

	public GameObject m_MostFavoursAward;

	public GameObject m_MostPrisonTilesAward;

	private bool m_bIsHighlighted;

	private Player m_Player;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	private int m_RenderCount;

	protected virtual void OnDestroy()
	{
		DestroyAvatarRenderTexture();
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		}
	}

	protected void Update()
	{
		if (m_Player != null && m_CharacterRenderer != null)
		{
			m_CharacterRenderer.UpdateAnimation(Time.deltaTime);
			m_CharacterRenderer.SetCustomisation(m_Player.m_CharacterCustomisation);
			m_CharacterRenderer.SetHighlighted(m_bIsHighlighted);
			m_CharacterRenderer.DrawCharacter(m_RenderTexture, m_RenderCount > 0);
			m_RenderCount++;
		}
	}

	public void OnAvatarSelected()
	{
		m_bIsHighlighted = true;
	}

	public void OnAvatarDeselected()
	{
		m_bIsHighlighted = false;
	}

	public void SetupForPlayer(Player player)
	{
		m_Player = player;
		if (m_UsernameLabel != null && player != null)
		{
			m_UsernameLabel.m_bNeedsLocalization = false;
			m_UsernameLabel.text = player.m_Gamer.m_GamerName;
		}
		if (m_CharacterNameLabel != null)
		{
			m_CharacterNameLabel.m_bNeedsLocalization = false;
			m_CharacterNameLabel.text = player.m_CharacterCustomisation.m_DisplayName;
		}
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
			SetupAvatarRenderTexture();
			SetupAvatarButton();
		}
		if (m_WinnerPlaque != null)
		{
			ConfigManager instance = ConfigManager.GetInstance();
			if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus && EscapePrisonFunctionality.GetInstance() != null)
			{
				Character escapingCharacter = EscapePrisonFunctionality.GetInstance().GetEscapingCharacter();
				if (escapingCharacter == player)
				{
					m_WinnerPlaque.gameObject.SetActive(value: true);
				}
				else
				{
					m_WinnerPlaque.gameObject.SetActive(value: false);
				}
			}
		}
		m_RenderCount = 0;
	}

	private void SetupAvatarButton()
	{
		m_AvatarButton.onClick.RemoveAllListeners();
		EventTrigger component = m_AvatarButton.GetComponent<EventTrigger>();
		if (component != null)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Select;
			entry.callback.AddListener(delegate
			{
				OnAvatarSelected();
			});
			component.triggers.Insert(0, entry);
			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Deselect;
			entry.callback.AddListener(delegate
			{
				OnAvatarDeselected();
			});
			component.triggers.Insert(0, entry);
		}
	}

	private void SetupAvatarRenderTexture()
	{
		DestroyAvatarRenderTexture();
		if (m_Avatar != null && m_CharacterRenderer != null)
		{
			T17RawImage componentInChildren = m_Avatar.GetComponentInChildren<T17RawImage>();
			int num = Mathf.FloorToInt(componentInChildren.rectTransform.rect.width);
			int num2 = Mathf.FloorToInt(componentInChildren.rectTransform.rect.height);
			if (num > 0 && num2 > 0)
			{
				m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextureID);
			}
			componentInChildren.texture = m_RenderTexture;
		}
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

	public void ShowGamerCard()
	{
		if (m_Player != null && m_Player.m_Gamer != null && Platform.GetInstance().m_CrossplayLobbyManager.IsGamerForMyPlatformOffline(m_Player.m_Gamer) && !string.IsNullOrEmpty(m_Player.m_Gamer.m_GamerName))
		{
			Platform.GetInstance().ShowGamerCard(Gamer.GetPrimaryGamer(), m_Player.m_Gamer.m_PlatformUniqueID);
		}
	}

	public void SetMostCraftedAwardActive(bool state)
	{
		SetAwardActive(m_MostCraftedAward, state);
	}

	public void SetMostKnockoutsAwardActive(bool state)
	{
		SetAwardActive(m_MostKnockoutsAward, state);
	}

	public void SetMostFavoursAwardActive(bool state)
	{
		SetAwardActive(m_MostFavoursAward, state);
	}

	public void SetMostDestroyedTilesAwardActive(bool state)
	{
		SetAwardActive(m_MostPrisonTilesAward, state);
	}

	private void SetAwardActive(GameObject awardContainer, bool state)
	{
		if (awardContainer != null)
		{
			awardContainer.SetActive(state);
		}
	}
}
