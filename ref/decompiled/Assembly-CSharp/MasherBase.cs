using MinigameMashers;
using Rewired;
using UnityEngine;

public class MasherBase : MonoBehaviour
{
	public T17Text m_StatToChangeLabel;

	public string m_FitnessLocalisedString = "Masher.Title.Fitness";

	public string m_StrengthLocalisedString = "Masher.Title.Strength";

	public string m_IntelligenceLocalisedString = "Masher.Title.Intelligence";

	[Header("Main image properties")]
	public T17Image m_MainIcon;

	public Sprite m_FitnessSprite;

	public Sprite m_StrengthSprite;

	public Sprite m_IntelligenceSprite;

	[Header("Progress")]
	public T17Slider m_ProgressSlider;

	public T17Text m_ProgressLabel;

	public Transform m_EffectOriginTransform;

	protected Player m_Player;

	protected Rewired.Player m_RewiredPlayer;

	public virtual void Reset()
	{
		UpdateProgress(0f, 0f, 0f);
	}

	public virtual void SetPlayerToCheck(Player player)
	{
		if (player != null)
		{
			m_Player = player;
			m_RewiredPlayer = m_Player.m_Gamer.m_RewiredPlayer;
		}
	}

	public void UpdateProgress(float value, float min, float max)
	{
		if (m_ProgressSlider != null)
		{
			m_ProgressSlider.minValue = min;
			m_ProgressSlider.maxValue = max;
			m_ProgressSlider.value = value;
		}
		if (m_ProgressLabel != null)
		{
			m_ProgressLabel.m_bNeedsLocalization = false;
			m_ProgressLabel.text = Mathf.RoundToInt(value).ToString();
		}
	}

	public void SetStyle(StylePreset stat)
	{
		Sprite sprite = null;
		if (m_MainIcon != null)
		{
			switch (stat)
			{
			case StylePreset.Fitness:
				sprite = m_FitnessSprite;
				m_StatToChangeLabel.SetLocalisedTextCatchAll(m_FitnessLocalisedString);
				break;
			case StylePreset.Stength:
				sprite = m_StrengthSprite;
				m_StatToChangeLabel.SetLocalisedTextCatchAll(m_StrengthLocalisedString);
				break;
			case StylePreset.Intelligence:
				sprite = m_IntelligenceSprite;
				m_StatToChangeLabel.SetLocalisedTextCatchAll(m_IntelligenceLocalisedString);
				break;
			}
			m_MainIcon.sprite = sprite;
		}
	}

	public Vector3 GetEffectSpawnPosition()
	{
		if (m_EffectOriginTransform != null)
		{
			return m_EffectOriginTransform.position;
		}
		return Vector3.zero;
	}
}
