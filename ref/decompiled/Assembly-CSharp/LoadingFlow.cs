using System;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class LoadingFlow : BaseFlowBehaviour
{
	public delegate void LoadingFlowHandler();

	[Serializable]
	public class LoadingScreenData
	{
		public LevelScript.PRISON_ENUM m_Prison;

		public Animator m_ControllingAnimator;
	}

	public Canvas m_LoadingCanvas;

	public GameObject m_LoadingIconObject;

	private bool m_TransitionInProgress;

	private UI_AnimationToRenderTexture m_RenderToTextureAnimControl;

	public T17Text m_TipLabel;

	private int m_RefCount;

	private LoadingFlowHandler m_ShowLoadingReadyCallbacks;

	private LoadingFlowHandler m_HideLoadingReadyCallbacks;

	private bool m_bInvokeShowCallbacks;

	private bool m_bInvokeHideCallbacks;

	public LoadingScreenData[] m_PrisonLoadingScreenData;

	public bool IsTransitionInProgress => m_TransitionInProgress;

	protected override void Start()
	{
		base.Start();
		SetLoadingUIActive(state: false);
		m_RenderToTextureAnimControl = base.gameObject.GetComponentInChildren<UI_AnimationToRenderTexture>();
	}

	public bool ShowLoadingScreen(LoadingFlowHandler loadingScreenReadyCallback)
	{
		bool flag = m_RefCount == 0;
		m_RefCount++;
		if (loadingScreenReadyCallback != null)
		{
			m_ShowLoadingReadyCallbacks = (LoadingFlowHandler)Delegate.Combine(m_ShowLoadingReadyCallbacks, loadingScreenReadyCallback);
		}
		if (flag && !m_TransitionInProgress)
		{
			m_bInvokeShowCallbacks = false;
			m_bInvokeHideCallbacks = false;
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int i = 0; i < allGamers.Length; i++)
			{
				if (allGamers[i] != null && allGamers[i].m_RewiredPlayer != null)
				{
					T17EventSystem.ApplyCategories(allGamers[i].m_RewiredPlayer, T17EventSystem.InputCateogryStates.Loading);
				}
			}
			ShowNewTip();
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_All_Global, AudioController.InGameMusicAndAmbienceObject);
			if (FadeManager.GetInstance() == null)
			{
				if (m_ShowLoadingReadyCallbacks != null)
				{
					m_ShowLoadingReadyCallbacks();
					m_ShowLoadingReadyCallbacks = null;
				}
				m_bInvokeShowCallbacks = true;
				return true;
			}
			m_TransitionInProgress = true;
			FadeManager.GetInstance().StartCurtainLower(delegate
			{
				SetLoadingUIActive(state: true);
				FadeManager.GetInstance().StartCurtainRaise(delegate
				{
					m_TransitionInProgress = false;
					m_bInvokeShowCallbacks = true;
					m_bInvokeHideCallbacks = false;
					if (m_ShowLoadingReadyCallbacks != null)
					{
						m_ShowLoadingReadyCallbacks();
						m_ShowLoadingReadyCallbacks = null;
					}
				});
			});
			return true;
		}
		return true;
	}

	private void ShowNewTip()
	{
		if (m_TipLabel != null)
		{
			Localization.Get("Text.Tip", out var localized);
			Localization.Get("Text.LoadingTip", out var localized2);
			localized = localized + " " + localized2;
			m_TipLabel.m_bNeedsLocalization = false;
			m_TipLabel.text = localized;
		}
	}

	public void HideLoadingScreen(LoadingFlowHandler loadingScreenReadyCallback)
	{
		m_RefCount--;
		bool flag = m_RefCount == 0;
		if (loadingScreenReadyCallback != null)
		{
			m_HideLoadingReadyCallbacks = (LoadingFlowHandler)Delegate.Combine(m_HideLoadingReadyCallbacks, loadingScreenReadyCallback);
		}
		if (!flag || m_TransitionInProgress)
		{
			return;
		}
		m_bInvokeShowCallbacks = false;
		m_bInvokeHideCallbacks = false;
		if (FadeManager.GetInstance() == null)
		{
			if (m_HideLoadingReadyCallbacks != null)
			{
				m_HideLoadingReadyCallbacks();
				m_HideLoadingReadyCallbacks = null;
			}
			m_bInvokeHideCallbacks = true;
			return;
		}
		m_TransitionInProgress = true;
		FadeManager.GetInstance().StartCurtainLower(delegate
		{
			SetLoadingUIActive(state: false);
			m_bInvokeShowCallbacks = false;
			m_bInvokeHideCallbacks = true;
			if (m_HideLoadingReadyCallbacks != null)
			{
				m_HideLoadingReadyCallbacks();
				m_HideLoadingReadyCallbacks = null;
			}
			FadeManager.GetInstance().StartCurtainRaise(delegate
			{
				m_TransitionInProgress = false;
			});
		});
	}

	public void DecrementRefCountForImminentLoadingCall()
	{
		m_RefCount--;
		if (m_RefCount < 0)
		{
			m_RefCount = 0;
		}
	}

	public void DoBlackScreenOnlyLoad(LoadingFlowHandler loadingScreenReadyCallback)
	{
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int i = 0; i < allGamers.Length; i++)
		{
			if (allGamers[i] != null && allGamers[i].m_RewiredPlayer != null)
			{
				T17EventSystem.ApplyCategories(allGamers[i].m_RewiredPlayer, T17EventSystem.InputCateogryStates.Loading);
			}
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_All_Global, AudioController.InGameMusicAndAmbienceObject);
		if (FadeManager.GetInstance() == null)
		{
			if (loadingScreenReadyCallback != null)
			{
				loadingScreenReadyCallback();
			}
			return;
		}
		m_TransitionInProgress = true;
		FadeManager.GetInstance().StartCurtainLower(delegate
		{
			if (loadingScreenReadyCallback != null)
			{
				loadingScreenReadyCallback();
			}
		});
	}

	public void DoHideBlackScreenOnlyLoad(LoadingFlowHandler loadingScreenReadyCallback)
	{
		if (FadeManager.GetInstance() == null)
		{
			loadingScreenReadyCallback?.Invoke();
			return;
		}
		m_TransitionInProgress = true;
		loadingScreenReadyCallback?.Invoke();
		FadeManager.GetInstance().StartCurtainRaise(delegate
		{
			m_TransitionInProgress = false;
		});
	}

	protected override void Update()
	{
		base.Update();
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.m_RewiredPlayer != null && primaryGamer.m_RewiredPlayer.GetButtonDown("UI_Submit"))
		{
			ShowNewTip();
		}
		if (m_bInvokeShowCallbacks && m_ShowLoadingReadyCallbacks != null)
		{
			m_ShowLoadingReadyCallbacks();
			m_ShowLoadingReadyCallbacks = null;
		}
		if (m_bInvokeHideCallbacks && m_HideLoadingReadyCallbacks != null)
		{
			m_HideLoadingReadyCallbacks();
			m_HideLoadingReadyCallbacks = null;
		}
	}

	public void Reset()
	{
		SetLoadingUIActive(state: false);
		m_RefCount = 0;
	}

	public void SetLoadingUIActive(bool state)
	{
		if (m_LoadingCanvas != null)
		{
			m_LoadingCanvas.gameObject.SetActive(state);
		}
		if (m_LoadingIconObject != null)
		{
			m_LoadingIconObject.SetActive(state);
		}
		if (!(m_RenderToTextureAnimControl != null))
		{
			return;
		}
		if (state)
		{
			LevelScript.PRISON_ENUM pRISON_ENUM = LevelScript.PRISON_ENUM.Unassigned;
			if (GlobalStart.GetInstance() != null)
			{
				pRISON_ENUM = GlobalStart.GetInstance().GetCurrentSelectedPrisonEnum();
			}
			if (m_PrisonLoadingScreenData != null)
			{
				int num = 0;
				int num2 = 0;
				int i;
				for (i = 0; i < m_PrisonLoadingScreenData.Length; i++)
				{
					if (m_PrisonLoadingScreenData[i].m_Prison == LevelScript.PRISON_ENUM.Unassigned)
					{
						num = i;
					}
					if (m_PrisonLoadingScreenData[i].m_Prison == pRISON_ENUM)
					{
						break;
					}
				}
				if (i < m_PrisonLoadingScreenData.Length && m_PrisonLoadingScreenData[i].m_ControllingAnimator != null)
				{
					m_RenderToTextureAnimControl.m_ControllingAnimator = m_PrisonLoadingScreenData[i].m_ControllingAnimator;
					num2 = i;
				}
				else if (m_PrisonLoadingScreenData[num].m_ControllingAnimator != null)
				{
					m_RenderToTextureAnimControl.m_ControllingAnimator = m_PrisonLoadingScreenData[num].m_ControllingAnimator;
					num2 = num;
				}
				for (i = 0; i < m_PrisonLoadingScreenData.Length; i++)
				{
					if (num2 == i)
					{
						m_PrisonLoadingScreenData[i].m_ControllingAnimator.gameObject.SetActive(value: true);
					}
					else
					{
						m_PrisonLoadingScreenData[i].m_ControllingAnimator.gameObject.SetActive(value: false);
					}
				}
			}
			m_RenderToTextureAnimControl.StartAnimation();
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Music_Frontend, AudioController.UI_Audio_GO);
		}
		else
		{
			m_RenderToTextureAnimControl.StopAnimation();
			m_RenderToTextureAnimControl.ForceClearRenderTarget();
		}
	}
}
