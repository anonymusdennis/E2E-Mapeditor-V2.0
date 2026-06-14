using UnityEngine;

public class GlobalVision : MonoBehaviour
{
	public AICharacter m_AICharacter;

	public AIEvent.EventType[] m_ListenForEvents;

	private void Awake()
	{
		if (m_ListenForEvents == null)
		{
			return;
		}
		AIEventManager instance = AIEventManager.GetInstance();
		if (instance != null)
		{
			for (int i = 0; i < m_ListenForEvents.Length; i++)
			{
				instance.SubscribeToGlobalCallback(m_ListenForEvents[i], OnEvent);
			}
		}
	}

	public bool OnEvent(AIEvent aiEvent)
	{
		m_AICharacter.AddEvent(aiEvent);
		return true;
	}
}
