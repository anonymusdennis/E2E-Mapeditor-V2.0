using UnityEngine;

public class OvenUI : MonoBehaviour
{
	public GameObject m_ProgressContainer;

	public T17Slider m_ProgressSlider;

	public RectTransform m_SliderRectTransform;

	public T17Image m_UndercookedSection;

	public T17Image m_CookedSection;

	public T17Image m_OvercookedSection;

	public float m_CookedLowerBound;

	public float m_CookedUpperBound;

	[SerializeField]
	public WorldSpaceHudScalePODO m_WorldSpacePositionInfo;

	private OvensHudContainer m_ParentContainer;

	private void Awake()
	{
		m_ParentContainer = GetComponentInParent<OvensHudContainer>();
	}

	public void SetupUI(float totalProcessingTime, float lowerBound, float upperBound)
	{
		m_CookedLowerBound = lowerBound;
		m_CookedUpperBound = upperBound;
		SetupProgressUIForProcessingTime(totalProcessingTime);
	}

	private void SetupProgressUIForProcessingTime(float totalProcessingTime)
	{
		if (m_UndercookedSection != null && m_CookedSection != null && m_OvercookedSection != null && m_SliderRectTransform != null)
		{
			float width = m_SliderRectTransform.rect.width;
			m_ProgressSlider.minValue = 0f;
			m_ProgressSlider.maxValue = 1f;
			float num = m_CookedLowerBound / totalProcessingTime;
			float num2 = (m_CookedUpperBound - m_CookedLowerBound) / totalProcessingTime;
			float num3 = 1f - (num + num2);
			SetThresholdMarkerWidth(m_UndercookedSection, width * num);
			SetThresholdMarkerWidth(m_CookedSection, width * num2);
			SetThresholdMarkerWidth(m_OvercookedSection, width * num3);
		}
	}

	private void SetThresholdMarkerWidth(T17Image marker, float width)
	{
		if (marker != null)
		{
			Vector2 sizeDelta = marker.rectTransform.sizeDelta;
			sizeDelta.x = width;
			marker.rectTransform.sizeDelta = sizeDelta;
		}
	}

	public void UpdateForNormalisedProgress(float progress)
	{
		if (m_ProgressSlider != null)
		{
			m_ProgressSlider.value = progress;
		}
	}

	public void SetPosition(Vector3 position)
	{
		if (m_ParentContainer == null)
		{
			m_ParentContainer = GetComponentInParent<OvensHudContainer>();
		}
		bool hasHorizontallySplitscreen = false;
		if (m_ParentContainer != null && HUDMenuFlow.Instance != null)
		{
			hasHorizontallySplitscreen = HUDMenuFlow.Instance.HasHorizontallySplitscreen(m_ParentContainer.GetCameraBinding());
		}
		m_WorldSpacePositionInfo.PositionTransform(base.transform, position, hasHorizontallySplitscreen);
	}
}
