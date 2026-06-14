using UnityEngine;

public class UIAnimatedCoinTicker : MonoBehaviour
{
	public T17Text m_OurMoneyLabel;

	public Animator m_OurMoneyLabelAnimator;

	public Animator m_OurMoneyAnimator;

	public float m_CoinsPerSecond = 20f;

	public string m_MoneyLabelTriggerStart = "MoneyLarge";

	public string m_MoneyLabelTriggerStop = "MoneySmall";

	public string m_CoinStartTrigger = "CoinAnimate";

	public string m_CoinStopTrigger = "CoinIdle";

	private float m_TargetCoinValue;

	private float m_CurrentCoinValue;

	private bool m_bIsTickingDown;

	private Player m_Player;

	public void SetPlayer(Player player)
	{
		m_Player = player;
	}

	public void Reset()
	{
		if (m_Player != null)
		{
			m_TargetCoinValue = m_Player.m_CharacterStats.Money;
			m_CurrentCoinValue = m_TargetCoinValue;
			if (m_OurMoneyLabel != null)
			{
				m_OurMoneyLabel.text = m_CurrentCoinValue.ToString("000");
			}
		}
		if (m_OurMoneyLabelAnimator != null)
		{
			m_OurMoneyLabelAnimator.SetTrigger(m_MoneyLabelTriggerStop);
		}
		if (m_OurMoneyAnimator != null)
		{
			m_OurMoneyAnimator.SetTrigger(m_CoinStopTrigger);
		}
		m_bIsTickingDown = false;
	}

	public void TickToNewValue(float newTargetCoinValue)
	{
		if (m_TargetCoinValue == newTargetCoinValue)
		{
			return;
		}
		m_TargetCoinValue = newTargetCoinValue;
		if (!m_bIsTickingDown)
		{
			m_bIsTickingDown = true;
			if (m_OurMoneyLabelAnimator != null)
			{
				m_OurMoneyLabelAnimator.ResetTrigger(m_MoneyLabelTriggerStop);
				m_OurMoneyLabelAnimator.SetTrigger(m_MoneyLabelTriggerStart);
			}
			if (m_OurMoneyAnimator != null)
			{
				m_OurMoneyAnimator.ResetTrigger(m_CoinStopTrigger);
				m_OurMoneyAnimator.SetTrigger(m_CoinStartTrigger);
			}
		}
	}

	private void Update()
	{
		if (m_TargetCoinValue != m_CurrentCoinValue)
		{
			m_CurrentCoinValue -= Time.deltaTime * m_CoinsPerSecond;
			if (m_CurrentCoinValue < m_TargetCoinValue)
			{
				m_CurrentCoinValue = m_TargetCoinValue;
				Reset();
			}
			else if (m_OurMoneyLabel != null)
			{
				m_OurMoneyLabel.text = Mathf.RoundToInt(m_CurrentCoinValue).ToString("D3");
			}
		}
	}
}
