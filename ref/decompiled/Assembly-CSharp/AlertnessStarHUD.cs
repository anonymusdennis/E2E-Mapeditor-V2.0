using System.Collections;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class AlertnessStarHUD : MonoBehaviour, IEventCleaner
{
	private struct AlertnessStar
	{
		public enum Fills
		{
			Unassigned,
			Empty,
			Half,
			Full
		}

		public T17Image m_UIStarImage;

		public T17Image m_UIInfoImage;

		public Fills m_Fill;

		public Fills m_PreviousFill;
	}

	[Header("UI References")]
	public T17Image[] m_StarArray;

	public T17Image[] m_AlertnessInfoImages;

	public Sprite m_StarOff;

	public Sprite m_StarHalf;

	public Sprite m_StarFull;

	[Header("UI Behaviour")]
	[Tooltip("When the alertness changes, how many blinks should we do")]
	public int m_NumAlertnessChangeBlinks = 5;

	[Tooltip("How often should the stars blink")]
	public float m_BlinkRate = 0.7f;

	[Tooltip("When told to blink, how long are the stars actually visible for?")]
	public float m_BlinkVisibleDuration = 0.3f;

	private AlertnessStar[] m_AlertnessStars;

	private PrisonAlertness m_SettledAlertness;

	private PrisonAlertness m_TargetAlertness;

	private PrisonAlertness m_NewTargetAlertness;

	private float m_BlinkCountdown;

	private float m_BlinkVisibleCountdown;

	private int m_BlinksDone;

	private void Start()
	{
		m_AlertnessStars = new AlertnessStar[m_StarArray.Length];
		for (int i = 0; i < m_StarArray.Length; i++)
		{
			m_AlertnessStars[i].m_UIStarImage = m_StarArray[i];
			if (i < m_AlertnessInfoImages.Length)
			{
				m_AlertnessStars[i].m_UIInfoImage = m_AlertnessInfoImages[i];
			}
		}
		if (null != PrisonAlertnessManager.GetInstance())
		{
			PrisonAlertnessManager.GetInstance().OnPrisonAlertnessChanged += UpdateStars;
			m_SettledAlertness = PrisonAlertnessManager.GetInstance().GetCurrentAlertness();
			SetStarFillForAlertness(m_SettledAlertness);
			UpdateStars(PrisonAlertnessManager.GetInstance().GetCurrentAlertness());
			SetStarsSprite();
		}
	}

	protected virtual void OnDestroy()
	{
		UnregisterEvents();
	}

	private void UnregisterEvents()
	{
		if (PrisonAlertnessManager.GetInstance() != null)
		{
			PrisonAlertnessManager.GetInstance().OnPrisonAlertnessChanged -= UpdateStars;
		}
	}

	private void UpdateStars(PrisonAlertness alertness)
	{
		m_NewTargetAlertness = alertness;
	}

	private void UpdateTargetAlertness()
	{
		if (m_TargetAlertness != m_NewTargetAlertness && UpdateManager.AquireHeavyCpuLock())
		{
			m_BlinksDone = 0;
			m_BlinkCountdown = m_BlinkRate;
			m_BlinkVisibleCountdown = m_BlinkVisibleDuration;
			if ((int)m_TargetAlertness < (int)m_NewTargetAlertness)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Star_Increase, base.gameObject);
			}
			else
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Star_Decrease, base.gameObject);
			}
			m_TargetAlertness = m_NewTargetAlertness;
			SetStarFillForAlertness(m_NewTargetAlertness);
		}
	}

	private void SetStarFillForAlertness(PrisonAlertness alertness)
	{
		int num = (int)alertness;
		int num2 = 0;
		for (int i = 0; i < m_AlertnessStars.Length; i++)
		{
			m_AlertnessStars[i].m_PreviousFill = m_AlertnessStars[i].m_Fill;
			num2 = num / 2;
			if (num2 > 0)
			{
				m_AlertnessStars[i].m_Fill = AlertnessStar.Fills.Full;
				num -= 2;
			}
			else if (num % 2 == 1)
			{
				m_AlertnessStars[i].m_Fill = AlertnessStar.Fills.Half;
				num--;
			}
			else if (num <= 0)
			{
				m_AlertnessStars[i].m_Fill = AlertnessStar.Fills.Empty;
			}
		}
	}

	protected void Update()
	{
		UpdateTargetAlertness();
		if (m_SettledAlertness == m_TargetAlertness)
		{
			return;
		}
		if (m_BlinksDone == m_NumAlertnessChangeBlinks)
		{
			m_SettledAlertness = m_TargetAlertness;
			SetStarsSprite();
			for (int i = 0; i < m_AlertnessInfoImages.Length; i++)
			{
				if (m_AlertnessInfoImages[i] != null)
				{
					m_AlertnessInfoImages[i].gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			ProcessStarBlinks();
		}
	}

	private void ProcessStarBlinks()
	{
		bool flag = false;
		if (m_BlinkCountdown <= 0f)
		{
			flag = true;
			m_BlinkVisibleCountdown -= UpdateManager.deltaTime;
			if (m_BlinkVisibleCountdown <= 0f)
			{
				m_BlinkCountdown = m_BlinkRate;
			}
		}
		else
		{
			m_BlinkCountdown -= UpdateManager.deltaTime;
			if (m_BlinkCountdown <= 0f)
			{
				m_BlinkVisibleCountdown = m_BlinkVisibleDuration;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Star_Flash, base.gameObject);
				m_BlinksDone++;
			}
		}
		SetStarsSprite(!flag);
	}

	private void SetStarsSprite(bool blinkUnsettledStars = false)
	{
		int num = (int)m_SettledAlertness;
		int num2 = (int)m_TargetAlertness;
		if (num > num2)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		if (num % 2 != 0)
		{
			num--;
		}
		if (num2 % 2 != 0)
		{
			num2++;
		}
		for (int i = 0; i < m_AlertnessStars.Length; i++)
		{
			AlertnessStar.Fills fills = m_AlertnessStars[i].m_Fill;
			int num4 = i * 2;
			if (blinkUnsettledStars && num4 >= num && num4 + 1 <= num2 && m_AlertnessStars[i].m_PreviousFill != 0)
			{
				fills = m_AlertnessStars[i].m_PreviousFill;
			}
			Sprite sprite = null;
			switch (fills)
			{
			case AlertnessStar.Fills.Empty:
				sprite = m_StarOff;
				break;
			case AlertnessStar.Fills.Half:
				sprite = m_StarHalf;
				break;
			case AlertnessStar.Fills.Full:
				sprite = m_StarFull;
				break;
			}
			if (m_AlertnessStars[i].m_UIInfoImage != null)
			{
				if (fills == AlertnessStar.Fills.Full && (int)m_TargetAlertness > (int)m_SettledAlertness)
				{
					m_AlertnessStars[i].m_UIInfoImage.gameObject.SetActive(value: true);
				}
				else
				{
					m_AlertnessStars[i].m_UIInfoImage.gameObject.SetActive(value: false);
				}
			}
			m_AlertnessStars[i].m_UIStarImage.sprite = sprite;
		}
	}

	public void CleanUpEvents()
	{
		UnregisterEvents();
	}

	private IEnumerator DelayedRefresh()
	{
		yield return null;
		yield return null;
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (null != PrisonAlertnessManager.GetInstance())
		{
			SetStarFillForAlertness(m_SettledAlertness);
			UpdateStars(PrisonAlertnessManager.GetInstance().GetCurrentAlertness());
			SetStarsSprite();
		}
	}
}
