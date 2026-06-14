using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class T17TrackedUIElement : MonoBehaviour
{
	public delegate void TrackedUIElementEvent(T17TrackedUIElement element);

	public enum NameplateStyle
	{
		Default,
		Positive,
		Negative
	}

	public TrackedUIElementEvent OnElementReleased;

	public static Vector3 DefaultTrackedElementScale = Vector3.one;

	public NamePlateHUD m_NamePlate;

	public SpeechBubbleHUD m_SpeechBubble;

	public IconDisplayHUD m_Icon;

	public ProgressBarHUD m_ProgressBar;

	public NameTagIconHud m_NameTagIconHUD;

	public Color m_TextColorDefault = Color.white;

	public Color m_TextColorPositive = Color.white;

	public Color m_TextColorNegative = Color.white;

	public WorldCanvasTrackedUIElements m_OwnerCanvas;

	private CanvasAlphaChanger m_GhostAlphaChanger;

	private CanvasAlphaChanger m_ActiveButFadingAlphaChanger;

	private NameplateStyle m_CurrentNameplateStyle;

	private TrackableUIElementsReporter m_AttachedToObject;

	private TrackableUIElementsReporter m_LastValidAttachedTo;

	private TrackableUIElementsReporter m_GhostedToObject;

	private Vector3 m_AttachedPositionOffset = Vector3.zero;

	public const uint NAMEPLATE_FLAG = 1u;

	public const uint SPEECHBUBBLE_FLAG = 2u;

	public const uint ICONDISPLAY_FLAG = 4u;

	public const uint PROGRESSBAR_FLAG = 8u;

	public const uint NAME_AND_ICON_FLAG = 5u;

	private uint m_UsageFlag;

	public CameraManager.PlayerBindingID m_CameraBindingID;

	public Material m_WorldHUDMaterial;

	private bool m_bIsPlateSetAsFarAway;

	public TrackableUIElementsReporter AttachedTo => m_AttachedToObject;

	public TrackableUIElementsReporter LastValidAttachedTo => m_LastValidAttachedTo;

	public TrackableUIElementsReporter GhostedTo => m_GhostedToObject;

	public Vector3 AttachedPosition
	{
		get
		{
			if (m_AttachedToObject != null)
			{
				return m_AttachedToObject.transform.position + m_AttachedPositionOffset;
			}
			if (m_GhostedToObject != null)
			{
				return m_GhostedToObject.transform.position + m_AttachedPositionOffset;
			}
			return m_AttachedPositionOffset;
		}
	}

	public GameObject AttachedToGO => m_AttachedToObject.gameObject;

	public uint Flags => m_UsageFlag;

	public bool HasFlag(uint flag)
	{
		return (m_UsageFlag & flag) != 0;
	}

	private void Awake()
	{
		m_GhostAlphaChanger = GetComponent<CanvasAlphaChanger>();
		if (m_GhostAlphaChanger != null)
		{
			m_GhostAlphaChanger.LerpFinishedEvent += CanvasAlphaChanger_LerpFinishedEvent;
			m_GhostAlphaChanger.m_bDisableOnFinish = true;
			m_GhostAlphaChanger.m_bSkipLerpingUsingCurrentAlpha = true;
		}
		m_ActiveButFadingAlphaChanger = base.gameObject.AddComponent<CanvasAlphaChanger>();
		m_ActiveButFadingAlphaChanger.m_bDisableOnFinish = true;
		m_ActiveButFadingAlphaChanger.m_bSkipLerpingUsingCurrentAlpha = true;
		if (m_WorldHUDMaterial != null)
		{
			T17Image[] componentsInChildren = GetComponentsInChildren<T17Image>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material = m_WorldHUDMaterial;
			}
			Text[] componentsInChildren2 = GetComponentsInChildren<Text>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].material = m_WorldHUDMaterial;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_GhostAlphaChanger != null)
		{
			m_GhostAlphaChanger.LerpFinishedEvent -= CanvasAlphaChanger_LerpFinishedEvent;
		}
	}

	private void DebugZeroFlagsButAttached()
	{
		if (m_UsageFlag == 0 && !(AttachedTo != null))
		{
		}
	}

	private void CanvasAlphaChanger_LerpFinishedEvent(TimedLerpingMonobehaviour sender)
	{
		m_GhostAlphaChanger.Stop();
		SafeDisableAll();
		base.gameObject.SetActive(value: false);
		m_AttachedToObject = null;
		m_LastValidAttachedTo = null;
		TrackableUIElementsReporter ghostedToObject = m_GhostedToObject;
		m_GhostedToObject = null;
		if (ghostedToObject != null)
		{
			ghostedToObject.RemoveTrackerUIElement(this);
		}
	}

	public void SetAttachedToReporter(TrackableUIElementsReporter obj)
	{
		if (obj == null && m_AttachedToObject != null)
		{
			m_AttachedToObject.RemoveTrackerUIElement(this);
			if (OnElementReleased != null)
			{
				OnElementReleased(this);
			}
		}
		if (obj != null)
		{
			m_LastValidAttachedTo = obj;
		}
		m_AttachedToObject = obj;
	}

	public void SetIsPlateFarAway(bool isFarAway)
	{
		m_bIsPlateSetAsFarAway = isFarAway;
	}

	public void SetAttachedPositionOffset(float x, float y)
	{
		m_AttachedPositionOffset.Set(x, y, 0f);
	}

	public void ResetAll()
	{
		m_bIsPlateSetAsFarAway = false;
		if (m_UsageFlag != 0)
		{
		}
		m_UsageFlag = 0u;
		if (m_NamePlate != null)
		{
			m_NamePlate.gameObject.SetActive(value: false);
		}
		if (m_SpeechBubble != null)
		{
			m_SpeechBubble.gameObject.SetActive(value: false);
		}
		if (m_Icon != null)
		{
			m_Icon.gameObject.SetActive(value: false);
		}
		if (m_ProgressBar != null)
		{
			m_ProgressBar.gameObject.SetActive(value: false);
		}
		if (m_NameTagIconHUD != null)
		{
			m_NameTagIconHUD.gameObject.SetActive(value: false);
		}
		m_AttachedPositionOffset.Set(0f, 0f, 0f);
		m_LastValidAttachedTo = null;
		m_GhostedToObject = null;
		if (m_GhostAlphaChanger != null)
		{
			m_GhostAlphaChanger.Stop();
		}
		SetAttachedToReporter(null);
		DebugZeroFlagsButAttached();
	}

	public void SafeDisableAll()
	{
		if (m_UsageFlag != 0)
		{
			if (HasFlag(1u))
			{
				DisableNamePlate();
			}
			if (HasFlag(4u))
			{
				DisableIcon();
			}
			if (HasFlag(2u))
			{
				DisableSpeechBubble();
			}
			if (HasFlag(8u))
			{
				DisableProgressBar();
			}
		}
	}

	private void EnableIconNameplateHUD()
	{
		m_NamePlate.gameObject.SetActive(value: false);
		m_Icon.gameObject.SetActive(value: false);
		m_NameTagIconHUD.gameObject.SetActive(value: true);
		m_NameTagIconHUD.CopyFrom(m_Icon, m_NamePlate);
		DebugZeroFlagsButAttached();
	}

	public void BeginFadeOut()
	{
		m_AttachedToObject = null;
		StopActiveFading();
		if (m_GhostAlphaChanger != null && m_LastValidAttachedTo != null)
		{
			m_GhostedToObject = m_LastValidAttachedTo;
			m_LastValidAttachedTo = null;
			T17TrackedUIElement element = this;
			m_GhostedToObject.AttachUITrackedElement(ref element, m_bIsPlateSetAsFarAway);
			m_GhostAlphaChanger.FadeToTransparent();
		}
		else
		{
			m_GhostedToObject = null;
			m_LastValidAttachedTo = null;
			CanvasAlphaChanger_LerpFinishedEvent(null);
		}
	}

	public void EnableNamePlate()
	{
		if (m_NamePlate != null && !IsNameplateActive())
		{
			m_UsageFlag |= 1u;
			if ((m_UsageFlag & 5) == 5 && m_NameTagIconHUD != null)
			{
				EnableIconNameplateHUD();
			}
			else
			{
				m_NamePlate.gameObject.SetActive(value: true);
			}
		}
	}

	public void DisableNamePlate()
	{
		if (m_NamePlate != null)
		{
			if (!IsNameplateActive())
			{
				return;
			}
			bool flag = (m_UsageFlag & 4) != 0;
			m_UsageFlag &= 4294967294u;
			m_NamePlate.gameObject.SetActive(value: false);
			if (flag)
			{
				m_Icon.gameObject.SetActive(value: true);
				if (m_NameTagIconHUD != null)
				{
					m_NameTagIconHUD.gameObject.SetActive(value: false);
				}
			}
		}
		DebugZeroFlagsButAttached();
	}

	public void SetNamePlateText(string text)
	{
		if (m_NamePlate != null && m_NamePlate.m_Text != null)
		{
			m_NamePlate.m_Text.text = text;
			if (m_NameTagIconHUD != null)
			{
				m_NameTagIconHUD.CopyNamePlate(m_NamePlate);
			}
		}
	}

	public bool IsNameplateActive()
	{
		return (m_UsageFlag & 1) != 0;
	}

	public void SetNameplateStyle(NameplateStyle style)
	{
		m_CurrentNameplateStyle = style;
		if (m_NamePlate != null && m_NamePlate.m_Text != null)
		{
			Color color = m_TextColorDefault;
			switch (style)
			{
			case NameplateStyle.Default:
				color = m_TextColorDefault;
				break;
			case NameplateStyle.Positive:
				color = m_TextColorPositive;
				break;
			case NameplateStyle.Negative:
				color = m_TextColorNegative;
				break;
			}
			m_NamePlate.m_Text.color = color;
			if (m_NameTagIconHUD != null)
			{
				m_NameTagIconHUD.CopyNamePlate(m_NamePlate);
			}
		}
	}

	public NameplateStyle GetNameplateStyle()
	{
		return m_CurrentNameplateStyle;
	}

	public void SetNameplateHighlight(bool highlight)
	{
		if (highlight)
		{
			if (m_bIsPlateSetAsFarAway)
			{
				m_NamePlate.m_Background.sprite = m_NamePlate.m_NormalImage;
			}
			else
			{
				m_NamePlate.m_Background.sprite = m_NamePlate.m_HighLightImage;
			}
		}
		else if (m_bIsPlateSetAsFarAway)
		{
			m_NamePlate.m_Background.sprite = m_NamePlate.m_NormalImage;
		}
		else
		{
			m_NamePlate.m_Background.sprite = m_NamePlate.m_NormalImage;
		}
		if (m_NameTagIconHUD != null)
		{
			m_NameTagIconHUD.CopyNamePlate(m_NamePlate);
		}
	}

	public bool EnableSpeechBubble()
	{
		if (m_SpeechBubble != null)
		{
			if ((m_UsageFlag & 2u) != 0)
			{
				return true;
			}
			m_UsageFlag |= 2u;
			m_SpeechBubble.gameObject.SetActive(value: true);
			if (m_SpeechBubble.m_SpeechBubbleTail != null)
			{
				m_SpeechBubble.m_SpeechBubbleTail.SetActive(value: true);
			}
			return true;
		}
		T17NetManager.LogGoogleException("A speech bubble is missing from " + base.name + "!");
		return false;
	}

	public void DisableSpeechBubble()
	{
		if (m_SpeechBubble != null && (m_UsageFlag & 2u) != 0)
		{
			m_UsageFlag &= 4294967293u;
			m_SpeechBubble.gameObject.SetActive(value: false);
			if (m_SpeechBubble.m_SpeechBubbleTail != null)
			{
				m_SpeechBubble.m_SpeechBubbleTail.SetActive(value: false);
			}
		}
	}

	public void EnableIcon()
	{
		if (m_Icon != null && (m_UsageFlag & 4) == 0)
		{
			m_UsageFlag |= 4u;
			if ((m_UsageFlag & 5) == 5 && m_NameTagIconHUD != null)
			{
				EnableIconNameplateHUD();
			}
			else
			{
				m_Icon.gameObject.SetActive(value: true);
			}
		}
	}

	public void DisableIcon()
	{
		if (!(m_Icon != null) || (m_UsageFlag & 4) == 0)
		{
			return;
		}
		bool flag = (m_UsageFlag & 1) != 0;
		m_UsageFlag &= 4294967291u;
		m_Icon.gameObject.SetActive(value: false);
		if (flag)
		{
			if (m_NameTagIconHUD != null)
			{
				m_NameTagIconHUD.gameObject.SetActive(value: false);
			}
			m_NamePlate.gameObject.SetActive(value: true);
		}
	}

	public void SetIconImage(CharacterIconHandler.IconType IType)
	{
		if (m_Icon != null && IType != CharacterIconHandler.IconType.MAX)
		{
			m_Icon.SetIconType(IType);
			if (m_NameTagIconHUD != null)
			{
				m_NameTagIconHUD.CopyIcon(m_Icon);
			}
		}
	}

	public void EnableProgressBar()
	{
		if (m_ProgressBar != null && (m_UsageFlag & 8) == 0)
		{
			m_UsageFlag |= 8u;
			m_ProgressBar.SetVisible(visible: true);
		}
	}

	public void DisableProgressBar()
	{
		if (m_ProgressBar != null && (m_UsageFlag & 8u) != 0)
		{
			m_UsageFlag &= 4294967287u;
			m_ProgressBar.SetVisible(visible: false);
		}
	}

	public void SetProgressBarText(string localizationTag)
	{
		if (m_ProgressBar != null)
		{
			m_ProgressBar.SetText(localizationTag);
		}
	}

	public void SetProgressBarProgress(float percentage)
	{
		if (m_ProgressBar != null)
		{
			m_ProgressBar.SetCurrentPercentage(percentage);
		}
	}

	public void SetAlphaTo(float value)
	{
		if (m_GhostAlphaChanger != null)
		{
			m_GhostAlphaChanger.SetAlphaTo(value);
		}
	}

	public void StartActiveFading(float time)
	{
		m_ActiveButFadingAlphaChanger.FadeToTransparent(time);
	}

	public void StopActiveFading()
	{
		m_ActiveButFadingAlphaChanger.enabled = false;
	}
}
