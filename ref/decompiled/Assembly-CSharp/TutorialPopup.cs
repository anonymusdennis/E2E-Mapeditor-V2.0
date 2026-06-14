using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
	[ReadOnly]
	public int m_TutorialPopupID = 999;

	private UIButtonShine m_ButtonShine;

	private void Awake()
	{
		m_ButtonShine = GetComponentInChildren<UIButtonShine>(includeInactive: true);
		if (m_ButtonShine != null)
		{
			m_ButtonShine.m_RegisterWithShimmerManager = false;
		}
	}

	private void Start()
	{
		UIShineRegister componentInChildren = GetComponentInChildren<UIShineRegister>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
	}

	public void PlayShimmer()
	{
		if (m_ButtonShine != null)
		{
			m_ButtonShine.Shine();
		}
	}
}
