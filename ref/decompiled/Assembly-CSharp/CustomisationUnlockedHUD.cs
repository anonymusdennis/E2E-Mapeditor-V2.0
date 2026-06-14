using System.Collections.Generic;
using UnityEngine;

public class CustomisationUnlockedHUD : BaseMenuBehaviour
{
	[Header("UI References")]
	public GameObject m_Root;

	public T17RawImage m_Avatar;

	public Animator m_SlideAnimator;

	public string m_SlideTriggerStart = "SlideLeft";

	[Header("Icon Rendering")]
	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public Texture m_PlaceholderIconTexture;

	[Header("Settings")]
	public float m_ShowDuration = 2f;

	public float m_ReshowDelay = 2f;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID = -1;

	private float m_ShowTimer;

	private float m_ReshowTimer;

	private bool m_bIsUIShowing;

	private CustomisationSet m_Unshown = new CustomisationSet();

	private Customisation m_PlayerAppearance = new Customisation();

	protected override void Awake()
	{
		base.Awake();
		UnlockManager instance = UnlockManager.GetInstance();
		if (instance != null)
		{
			instance.onSetUnlocked -= OnCustomisationSetUnlocked_AddToQueue;
			instance.onSetUnlocked += OnCustomisationSetUnlocked_AddToQueue;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnlockManager instance = UnlockManager.GetInstance();
		if (instance != null)
		{
			instance.onSetUnlocked -= OnCustomisationSetUnlocked_AddToQueue;
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		HideUnlockUI();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		HideUnlockUI();
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (m_bIsUIShowing)
		{
			if (m_ShowTimer < m_ShowDuration)
			{
				m_ShowTimer += UpdateManager.deltaTime;
				if (m_ShowTimer >= m_ShowDuration)
				{
					m_ShowTimer = m_ShowDuration;
					HideUnlockUI();
				}
			}
		}
		else if (m_Unshown != null && m_Unshown.count > 0 && m_ReshowTimer <= 0f)
		{
			Customisation appearance = null;
			if (SelectUnseenCustomisation(m_Unshown, out appearance))
			{
				ShowUnlockUI(appearance);
			}
		}
		if (m_ReshowTimer > 0f)
		{
			m_ReshowTimer -= UpdateManager.deltaTime;
			if (m_ReshowTimer <= 0f)
			{
				m_ReshowTimer = 0f;
			}
		}
	}

	public bool ShowUnlockUI(Customisation toShow)
	{
		if (toShow == null || m_PlayerAppearance == null)
		{
			return false;
		}
		if (base.CurrentGamePlayer != null && base.CurrentGamePlayer.m_CharacterCustomisation != null)
		{
			m_PlayerAppearance.body = base.CurrentGamePlayer.m_CharacterCustomisation.m_BodyType;
			m_PlayerAppearance.skin = base.CurrentGamePlayer.m_CharacterCustomisation.m_SkinColour;
		}
		Customisation customisation = new Customisation();
		if (base.CurrentGamePlayer != null && base.CurrentGamePlayer.m_CharacterCustomisation != null)
		{
			customisation.body = ((toShow.body == CustomisationData.BodyType.NULL) ? base.CurrentGamePlayer.m_CharacterCustomisation.m_BodyType : toShow.body);
			customisation.skin = ((toShow.skin == CustomisationData.SkinColour.NULL) ? base.CurrentGamePlayer.m_CharacterCustomisation.m_SkinColour : toShow.skin);
		}
		else
		{
			customisation.body = toShow.body;
			customisation.skin = toShow.skin;
		}
		customisation.hair = toShow.hair;
		customisation.hat = toShow.hat;
		customisation.upperFace = toShow.upperFace;
		customisation.lowerFace = toShow.lowerFace;
		if (m_Avatar != null && m_CharacterRenderer != null)
		{
			if (m_RenderTexture == null)
			{
				int num = Mathf.FloorToInt(m_Avatar.rectTransform.rect.width);
				int num2 = Mathf.FloorToInt(m_Avatar.rectTransform.rect.height);
				if (num > 0 && num2 > 0)
				{
					m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextureID);
				}
			}
			if (m_RenderTexture != null)
			{
				m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
				m_CharacterRenderer.SetCustomisation(customisation);
				m_CharacterRenderer.DrawCharacter(m_RenderTexture);
				m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
				m_Avatar.texture = m_RenderTexture;
			}
		}
		if (m_Root != null)
		{
			m_Root.gameObject.SetActive(value: true);
		}
		if (m_SlideAnimator != null && m_SlideAnimator.isInitialized)
		{
			m_SlideAnimator.SetTrigger(m_SlideTriggerStart);
			m_SlideAnimator.Update(UpdateManager.deltaTime);
		}
		m_ShowTimer = 0f;
		m_bIsUIShowing = true;
		return true;
	}

	public void HideUnlockUI()
	{
		if (m_Root != null)
		{
			m_Root.gameObject.SetActive(value: false);
		}
		if (m_Avatar != null)
		{
			m_Avatar.texture = m_PlaceholderIconTexture;
		}
		m_ReshowTimer = m_ReshowDelay;
		m_bIsUIShowing = false;
	}

	private void OnCustomisationSetUnlocked_AddToQueue(CustomisationSet unlocked)
	{
		if (unlocked != null && unlocked.count > 0)
		{
			int num = 0;
			num += MergeEnumLists(ref m_Unshown.bodyTypes, unlocked.bodyTypes);
			num += MergeEnumLists(ref m_Unshown.skinColours, unlocked.skinColours);
			num += MergeEnumLists(ref m_Unshown.hairs, unlocked.hairs);
			num += MergeEnumLists(ref m_Unshown.hats, unlocked.hats);
			num += MergeEnumLists(ref m_Unshown.upperFaces, unlocked.upperFaces);
			num += MergeEnumLists(ref m_Unshown.lowerFaces, unlocked.lowerFaces);
			if (num > 0)
			{
			}
		}
	}

	private bool SelectUnseenCustomisation(CustomisationSet unseen, out Customisation appearance)
	{
		appearance = null;
		if (appearance == null && unseen.bodyTypes.Count > 0)
		{
			appearance = new Customisation
			{
				body = unseen.bodyTypes[0]
			};
			unseen.bodyTypes.RemoveAt(0);
		}
		if (appearance == null && unseen.skinColours.Count > 0)
		{
			appearance = new Customisation
			{
				skin = unseen.skinColours[0]
			};
			unseen.skinColours.RemoveAt(0);
		}
		if (appearance == null && unseen.hairs.Count > 0)
		{
			appearance = new Customisation
			{
				hair = unseen.hairs[0]
			};
			unseen.hairs.RemoveAt(0);
		}
		if (appearance == null && unseen.hats.Count > 0)
		{
			appearance = new Customisation
			{
				hat = unseen.hats[0]
			};
			unseen.hats.RemoveAt(0);
		}
		if (appearance == null && unseen.upperFaces.Count > 0)
		{
			appearance = new Customisation
			{
				upperFace = unseen.upperFaces[0]
			};
			unseen.upperFaces.RemoveAt(0);
		}
		if (appearance == null && unseen.lowerFaces.Count > 0)
		{
			appearance = new Customisation
			{
				lowerFace = unseen.lowerFaces[0]
			};
			unseen.lowerFaces.RemoveAt(0);
		}
		return appearance != null;
	}

	private static int MergeEnumLists<T>(ref List<T> values, List<T> toAdd)
	{
		int num = 0;
		int count = toAdd.Count;
		for (int i = 0; i < count; i++)
		{
			T item = toAdd[i];
			if (!values.Contains(item))
			{
				values.Add(item);
				num++;
			}
		}
		if (num > 0)
		{
			values.Sort();
		}
		return num;
	}
}
