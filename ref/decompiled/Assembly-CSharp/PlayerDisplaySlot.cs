using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplaySlot : MonoBehaviour
{
	private Player m_TargetPlayer;

	public FriendsContextMenu m_ContextMenu;

	public T17RawImage m_Avatar;

	public T17Text m_SlotStatusText;

	public T17Text m_SlotPlayerName;

	public T17Image m_VoiceChat;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	private float m_AnimTimescale = 1f;

	private int m_RenderCounter;

	private Rect m_currentRect;

	private Customisation m_SilhouetteCustomisation = new Customisation
	{
		hair = CustomisationData.Hair.NONE,
		hat = CustomisationData.Hat.HIDDENVERSUS_WHITE,
		upperFace = CustomisationData.UpperFaceAccessory.NONE,
		lowerFace = CustomisationData.LowerFaceAccessory.NONE
	};

	private bool m_bIsAvatarSilhouetted;

	private bool m_bIsAvatarHighlighted;

	public Action onSlotHighlighted;

	public Action onSlotPressed;

	protected Gamer m_Gamer;

	private Platform.DisplayableFriend m_DisplayableFriend;

	private Selectable m_OurSelectable;

	public Player TargetPlayer => m_TargetPlayer;

	public Selectable GetSelectable
	{
		get
		{
			if (m_OurSelectable == null)
			{
				m_OurSelectable = GetComponentInChildren<Selectable>(includeInactive: true);
			}
			return m_OurSelectable;
		}
	}

	protected virtual void Awake()
	{
	}

	protected virtual void OnEnable()
	{
		m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		SetupRenderTexture();
	}

	protected virtual void OnDisable()
	{
		DestroyRenderTexture();
		m_CharacterRenderer.SetHighlighted(bIsHighlighted: false);
		m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
	}

	protected virtual void Update()
	{
		if (m_TargetPlayer != null && m_CharacterRenderer != null && m_RenderTexture != null)
		{
			if (m_Avatar != null && m_currentRect != m_Avatar.rectTransform.rect)
			{
				SetupRenderTexture();
			}
			float num = Time.unscaledDeltaTime * m_AnimTimescale;
			if (m_bIsAvatarSilhouetted)
			{
				m_CharacterRenderer.SetCustomisation(m_SilhouetteCustomisation);
				m_CharacterRenderer.UpdateAnimation(num);
				m_CharacterRenderer.SetHighlighted(m_bIsAvatarHighlighted);
				m_CharacterRenderer.DrawCharacter(m_RenderTexture, m_RenderCounter > 0);
			}
			else
			{
				m_CharacterRenderer.SetHighlighted(m_bIsAvatarHighlighted);
				m_CharacterRenderer.SetAndDrawCharacter(m_TargetPlayer.m_CharacterCustomisation, m_RenderTexture, num, m_RenderCounter > 0);
			}
			if (!m_Avatar.enabled)
			{
				m_Avatar.enabled = true;
			}
		}
		if (m_Gamer == null)
		{
			if (m_VoiceChat != null && m_VoiceChat.enabled)
			{
				m_VoiceChat.enabled = false;
			}
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		m_RenderCounter++;
	}

	public virtual void SetRewiredPlayerIndex(Gamer gamer)
	{
		m_Gamer = gamer;
	}

	public void SetAnimTimescale(float scale)
	{
		m_AnimTimescale = scale;
	}

	public void SetPlayerTarget(Player targetPlayer)
	{
		m_TargetPlayer = targetPlayer;
		if (!(m_TargetPlayer != null))
		{
			return;
		}
		if (m_TargetPlayer.m_Gamer != null)
		{
			base.gameObject.SetActive(value: true);
			m_DisplayableFriend = new Platform.DisplayableFriend();
			m_DisplayableFriend.m_Gamer = m_TargetPlayer.m_Gamer;
			m_DisplayableFriend.m_OnlineID = targetPlayer.m_Gamer.m_PlatformUniqueID;
			m_DisplayableFriend.m_Name = targetPlayer.m_Gamer.m_GamerName;
			if (m_bIsAvatarSilhouetted)
			{
				m_SlotPlayerName.m_bNeedsLocalization = false;
				m_SlotPlayerName.text = string.Empty;
			}
			else
			{
				m_SlotPlayerName.m_bNeedsLocalization = false;
				m_SlotPlayerName.text = m_TargetPlayer.m_CharacterCustomisation.m_DisplayName;
			}
			m_SlotStatusText.text = targetPlayer.m_Gamer.m_GamerName;
		}
		else
		{
			base.gameObject.SetActive(value: false);
			m_SlotPlayerName.m_bNeedsLocalization = true;
			m_SlotPlayerName.SetNewLocalizationTag("Text.PlayerSelect.Available");
			m_SlotStatusText.text = " - ";
		}
		if (m_TargetPlayer.m_CharacterCustomisation != null)
		{
			m_SilhouetteCustomisation.body = m_TargetPlayer.m_CharacterCustomisation.m_BodyType;
			m_SilhouetteCustomisation.skin = m_TargetPlayer.m_CharacterCustomisation.m_SkinColour;
			m_SilhouetteCustomisation.defaultOutfit = m_TargetPlayer.m_CharacterCustomisation.m_Outfit;
		}
	}

	public void SetAvatarSilhouetted(bool silhouetted)
	{
		m_bIsAvatarSilhouetted = silhouetted;
	}

	private void SetupRenderTexture()
	{
		DestroyRenderTexture();
		m_currentRect = m_Avatar.rectTransform.rect;
		if (m_Avatar != null)
		{
			int num = Mathf.FloorToInt(m_currentRect.width);
			int num2 = Mathf.FloorToInt(m_currentRect.height);
			if (num > 0 && num2 > 0)
			{
				m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextureID);
			}
			m_Avatar.enabled = false;
			m_Avatar.texture = m_RenderTexture;
		}
		m_RenderCounter = 0;
	}

	private void DestroyRenderTexture()
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

	public virtual void OnSelectButtonPressed()
	{
		if (onSlotPressed != null)
		{
			onSlotPressed();
		}
		else if (m_ContextMenu != null && m_DisplayableFriend != null)
		{
			RectTransform rectTransform = m_ContextMenu.transform as RectTransform;
			RectTransform from = base.transform as RectTransform;
			Vector2 anchoredPosition = SwitchToRectTransform(from, rectTransform);
			rectTransform.anchoredPosition = anchoredPosition;
			if (m_ContextMenu != null)
			{
				m_ContextMenu.ShowContextMenu(ref m_DisplayableFriend, m_Gamer);
			}
		}
	}

	private Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
	{
		Vector2 vector = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, from.position);
		screenPoint += vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenPoint, null, out var localPoint);
		Vector2 vector2 = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
		return to.anchoredPosition + localPoint - vector2;
	}

	public virtual void OnSelectButtonHighlighted()
	{
		if (onSlotHighlighted != null)
		{
			onSlotHighlighted();
		}
	}

	public void SetIsHighlighted(bool isHighlighted)
	{
		m_bIsAvatarHighlighted = isHighlighted;
	}
}
