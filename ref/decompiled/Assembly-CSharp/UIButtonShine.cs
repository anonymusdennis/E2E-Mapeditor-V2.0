using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class UIButtonShine : MonoBehaviour
{
	public T17Image m_MainShine;

	public T17Image m_VerticalShine;

	public T17Image m_HorizontalShine;

	public float m_DeltaTimeSpeedFactor = 0.75f;

	private float m_LerpValue;

	public bool m_Loop;

	public bool m_RegisterWithShimmerManager = true;

	private void Awake()
	{
		if (m_RegisterWithShimmerManager)
		{
			UIShineRegister component = GetComponent<UIShineRegister>();
			if (component == null)
			{
				component = base.gameObject.AddComponent<UIShineRegister>();
			}
			else
			{
				component.enabled = true;
			}
		}
	}

	private void Update()
	{
		m_LerpValue += Time.deltaTime * m_DeltaTimeSpeedFactor;
		if (m_LerpValue > 1f)
		{
			if (m_Loop)
			{
				m_LerpValue -= 1f;
			}
			else
			{
				m_LerpValue = 1f;
				Disable();
			}
		}
		SetShineEffectLerp(m_LerpValue);
	}

	public void Disable()
	{
		base.enabled = false;
		SetShimmersActiveTo(state: false);
	}

	private void SetShimmersActiveTo(bool state)
	{
		m_MainShine.gameObject.SetActive(state);
		m_VerticalShine.gameObject.SetActive(state);
		m_HorizontalShine.gameObject.SetActive(state);
	}

	public void Shine()
	{
		base.enabled = true;
		SetShimmersActiveTo(state: true);
		m_LerpValue = 0f;
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Shimmer, base.gameObject);
	}

	public void Reset()
	{
		m_LerpValue = 0f;
		SetShineEffectLerp(m_LerpValue);
	}

	private void SetShineEffectLerp(float lerpValue)
	{
		m_MainShine.material.SetFloat("_EffectLerp", lerpValue);
		m_VerticalShine.material.SetFloat("_EffectLerp", lerpValue);
		m_HorizontalShine.material.SetFloat("_EffectLerp", lerpValue);
	}
}
