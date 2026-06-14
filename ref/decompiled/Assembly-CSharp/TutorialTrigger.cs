using UnityEngine;

public class TutorialTrigger : ColliderEvents
{
	public TutorialSubject m_TutorialSubject = TutorialSubject.COUNT;

	public bool m_bForceShowTutorial;

	protected override void FireEvent(Transform colliderTransform, ColliderEvent colliderEvent)
	{
		base.FireEvent(colliderTransform, colliderEvent);
		Player player = ((!(colliderTransform != null) || !(colliderTransform.parent != null)) ? null : colliderTransform.parent.GetComponent<Player>());
		TutorialManager instance = TutorialManager.GetInstance();
		if (player != null && instance != null && m_TutorialSubject != TutorialSubject.COUNT)
		{
			instance.StartTutorialRPC(player, m_TutorialSubject, m_bForceShowTutorial);
		}
	}
}
