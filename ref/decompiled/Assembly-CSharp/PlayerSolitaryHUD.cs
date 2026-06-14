using UnityEngine;

public class PlayerSolitaryHUD : BaseMenuBehaviour
{
	public GameObject m_Root;

	public T17Slider m_ProgressBar;

	public T17Text m_RemainingMinutes;

	public T17Text m_RemainingSeconds;

	private float m_StartRemainingTime;

	private bool isVisible = true;

	private bool isUIActive;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentGamePlayer != null)
		{
			UpdateTimer();
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		DeactivateUI();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		DeactivateUI();
		return true;
	}

	public override void SetGamePlayer(Player gamePlayer)
	{
		base.SetGamePlayer(gamePlayer);
		if (base.CurrentGamePlayer != null)
		{
			UpdateTimer();
		}
	}

	private void UpdateTimer()
	{
		float num = 0f;
		if (base.CurrentGamePlayer.m_bIsWantedForSolitary && SolitaryManager.GetInstance() != null)
		{
			num = SolitaryManager.GetInstance().GetTimeRemainining(base.CurrentGamePlayer);
		}
		if (num > 0f)
		{
			if (!isUIActive)
			{
				ActivateUI();
				m_StartRemainingTime = num;
			}
			if (m_RemainingMinutes != null)
			{
				int value = Mathf.FloorToInt(num / 60f);
				m_RemainingMinutes.text = NumberToStringCache.GetIntAsString(value, bSingleAs2: true);
			}
			if (m_RemainingSeconds != null)
			{
				int value2 = Mathf.FloorToInt(num) % 60;
				m_RemainingSeconds.text = NumberToStringCache.GetIntAsString(value2, bSingleAs2: true);
			}
			if (m_ProgressBar != null)
			{
				float value3 = num / m_StartRemainingTime;
				m_ProgressBar.value = value3;
			}
		}
		else if (isUIActive)
		{
			DeactivateUI();
		}
	}

	private void ActivateUI()
	{
		isUIActive = true;
		if (isVisible && m_Root != null)
		{
			m_Root.SetActive(value: true);
		}
	}

	private void DeactivateUI()
	{
		isUIActive = false;
		if (m_Root != null)
		{
			m_Root.SetActive(value: false);
		}
	}

	public void SetVisibility(bool visible)
	{
		isVisible = visible;
		if (isUIActive && m_Root != null)
		{
			m_Root.SetActive(visible);
		}
	}
}
