using UnityEngine;

public class AnimationEffectHandler : MonoBehaviour
{
	public float m_AnimationLength;

	private float m_AnimationTime;

	private void Awake()
	{
	}

	private void Update()
	{
		m_AnimationTime += UpdateManager.deltaTime;
		if (m_AnimationTime > m_AnimationLength)
		{
			ResetAndRemove();
		}
	}

	private void ResetAndRemove()
	{
		ResetValues();
		if (base.gameObject.transform.parent.gameObject.activeSelf)
		{
			base.gameObject.transform.parent.gameObject.SetActive(value: false);
		}
		CullingObjectCollector instance = CullingObjectCollector.GetInstance();
		if (instance != null)
		{
			instance.RemoveRuntimeEffect(base.gameObject.transform.parent.gameObject);
		}
	}

	public void ResetValues()
	{
		m_AnimationTime = 0f;
	}

	private void OnDisable()
	{
		ResetAndRemove();
	}

	public void PrepareForCullerVisiblity(bool isVisible)
	{
	}
}
