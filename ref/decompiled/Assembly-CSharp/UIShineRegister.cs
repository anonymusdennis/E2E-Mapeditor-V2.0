using UnityEngine;

[RequireComponent(typeof(UIButtonShine))]
public class UIShineRegister : MonoBehaviour
{
	private UIButtonShine m_LinkedShine;

	private void Awake()
	{
		m_LinkedShine = GetComponent<UIButtonShine>();
		if (m_LinkedShine == null)
		{
			Object.Destroy(base.gameObject);
		}
		if (base.isActiveAndEnabled && base.gameObject.activeInHierarchy)
		{
			OnEnable();
		}
	}

	private void OnEnable()
	{
		UIShimmerManager.Register(m_LinkedShine);
	}

	private void OnDisable()
	{
		UIShimmerManager.UnRegister(m_LinkedShine);
	}
}
