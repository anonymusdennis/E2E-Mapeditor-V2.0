using Slate;
using UnityEngine;

public class PrisonEscapeCollider : ColliderEvents
{
	[Header("If escape cutscene is null, then generic cutscene will play")]
	public Cutscene m_EscapeCutscene;

	public EscapeMethod m_EscapeMethod = EscapeMethod.NothingSpecial;

	protected override void Start()
	{
		base.Start();
		if (m_EscapeCutscene != null)
		{
			CutsceneManagerBase instance = CutsceneManagerBase.GetInstance();
			if (instance != null && instance.GetCutsceneIndex(m_EscapeCutscene) != -1)
			{
			}
		}
		else if (CutsceneManagerBase.GetInstance() != null)
		{
			m_EscapeCutscene = CutsceneManagerBase.GetInstance().m_GenericEscapeCutscene;
		}
		if (m_Collider != null)
		{
			switch (m_FireOnEventType)
			{
			case ColliderEvent.OnCollisionEnter:
			case ColliderEvent.OnCollisionExit:
			case ColliderEvent.OnCollisionStay:
				m_EventTypesToFireOn.Add(ColliderEvent.OnCollisionEnter);
				m_EventTypesToFireOn.Add(ColliderEvent.OnCollisionExit);
				break;
			case ColliderEvent.OnTriggerEnter:
			case ColliderEvent.OnTriggerExit:
			case ColliderEvent.OnTriggerStay:
				m_EventTypesToFireOn.Add(ColliderEvent.OnTriggerEnter);
				m_EventTypesToFireOn.Add(ColliderEvent.OnTriggerExit);
				break;
			}
		}
	}

	protected override void FireEvent(Transform colliderTransform, ColliderEvent colliderEvent)
	{
		base.FireEvent(colliderTransform, colliderEvent);
		Player player = ((!(colliderTransform != null) || !(colliderTransform.parent != null)) ? null : colliderTransform.parent.GetComponent<Player>());
		if (player != null && EscapePrisonFunctionality.GetInstance() != null)
		{
			switch (colliderEvent)
			{
			case ColliderEvent.OnCollisionEnter:
			case ColliderEvent.OnTriggerEnter:
				EscapePrisonFunctionality.GetInstance().CharacterReachedEscapeTriggerRPC(player, m_EscapeMethod, m_EscapeCutscene);
				break;
			case ColliderEvent.OnCollisionExit:
			case ColliderEvent.OnTriggerExit:
				EscapePrisonFunctionality.GetInstance().CharacterLeftEscapeTriggerRPC(player, m_EscapeMethod);
				break;
			}
		}
	}
}
