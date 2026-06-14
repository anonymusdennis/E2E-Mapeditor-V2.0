public class CutsceneHUDManager : T17MonoBehaviour
{
	private static CutsceneHUDManager m_Instance;

	public static CutsceneHUDManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public void FadePlayerHUDToOpaque(CameraManager.PlayerBindingID playerBinding, float time)
	{
		HUDMenuFlow.Instance.GetHudContainingObjects(playerBinding, out var hudParentObject, out var hudWorldSpaceParent);
		CanvasAlphaChanger component = hudParentObject.GetComponent<CanvasAlphaChanger>();
		CanvasAlphaChanger component2 = hudWorldSpaceParent.GetComponent<CanvasAlphaChanger>();
		if (!(component == null) && !(component2 == null))
		{
			component.FadeToOpaque(time);
			component2.FadeToOpaque(time);
		}
	}

	public void FadePlayerHUDToTransparent(CameraManager.PlayerBindingID playerBinding, float time)
	{
		HUDMenuFlow.Instance.GetHudContainingObjects(playerBinding, out var hudParentObject, out var hudWorldSpaceParent);
		CanvasAlphaChanger component = hudParentObject.GetComponent<CanvasAlphaChanger>();
		CanvasAlphaChanger component2 = hudWorldSpaceParent.GetComponent<CanvasAlphaChanger>();
		if (!(component == null) && !(component2 == null))
		{
			component.FadeToTransparent(time);
			component2.FadeToTransparent(time);
		}
	}

	private void PlayEffectOnPlayersHUD(int playerIndex, float time, UIAnimatedEffectController.Effects effect)
	{
		UIAnimatedEffectController fadeEffects = HUDMenuFlow.Instance.m_PlayersHUDData[playerIndex].m_FadeEffects;
		if (!(fadeEffects == null))
		{
			fadeEffects.PlayEffect(effect, time);
		}
	}

	public void StartLetterBoxInOut(CameraManager.PlayerBindingID playerBinding, float letterboxAnimateTime, float letterboxHoldTime, AnimatedEffectPingPong.StartingHoldHandler startingHoldCallback = null, AnimatedEffectPingPong.FinishedHoldHandler finishedHoldCallback = null)
	{
		UIAnimatedEffectController playerEffectsController = HUDMenuFlow.Instance.GetPlayerEffectsController(playerBinding);
		AnimatedEffectPingPong component = playerEffectsController.m_LetterboxEffect.GetComponent<AnimatedEffectPingPong>();
		if (component == null)
		{
			return;
		}
		GetInstance().FadePlayerHUDToTransparent(playerBinding, letterboxAnimateTime);
		component.StartEffectForPlayer(letterboxAnimateTime, letterboxHoldTime, delegate
		{
			if (startingHoldCallback != null)
			{
				startingHoldCallback();
			}
		}, delegate
		{
			GetInstance().FadePlayerHUDToOpaque(playerBinding, letterboxAnimateTime);
			if (finishedHoldCallback != null)
			{
				finishedHoldCallback();
			}
		});
	}

	public void StartFadeAllHUDsToTransparent(float time)
	{
		int count = HUDMenuFlow.Instance.m_PlayersHUDData.Count;
		for (int i = 0; i < count; i++)
		{
			FadePlayerHUDToTransparent(HUDMenuFlow.Instance.m_PlayersHUDData[i].m_PlayerBindingID, time);
		}
	}

	public void FadeAllHUDsToOpaque(float time)
	{
		int count = HUDMenuFlow.Instance.m_PlayersHUDData.Count;
		for (int i = 0; i < count; i++)
		{
			FadePlayerHUDToOpaque(HUDMenuFlow.Instance.m_PlayersHUDData[i].m_PlayerBindingID, time);
		}
	}
}
