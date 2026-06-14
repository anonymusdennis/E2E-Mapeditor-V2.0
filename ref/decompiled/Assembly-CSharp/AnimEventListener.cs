using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class AnimEventListener : MonoBehaviour
{
	public GameObject m_Owner;

	public virtual void Start()
	{
		if (m_Owner == null)
		{
			m_Owner = base.transform.parent.gameObject;
		}
	}

	public virtual void SoundEvent(int animationEvent)
	{
		if (!(m_Owner == null) && animationEvent >= 0)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, (Events)animationEvent, m_Owner);
		}
	}
}
