using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationPlayer : MonoBehaviour
{
	public Animator m_Animator;

	public string m_TriggerName;

	private void Awake()
	{
		if (m_Animator == null)
		{
			m_Animator = GetComponent<Animator>();
		}
	}

	public void Play(string triggerToPlay = null)
	{
		if (string.IsNullOrEmpty(triggerToPlay))
		{
			triggerToPlay = m_TriggerName;
		}
		if (m_Animator != null && !string.IsNullOrEmpty(triggerToPlay))
		{
			m_Animator.SetTrigger(triggerToPlay);
		}
	}
}
