using UnityEngine;

public class T17AkEvent : AkEvent
{
	public AudioController.SOUND_AREA m_Area = AudioController.SOUND_AREA.SA_INGAME;

	public override void HandleEvent(GameObject in_gameObject)
	{
		GameObject in_GameObjectID = (soundEmitterObject = ((!useOtherObject || !(in_gameObject != null)) ? base.gameObject : in_gameObject));
		uint listenerMask = AudioController.GetListenerMask(m_Area);
		AkSoundEngine.SetActiveListeners(in_GameObjectID, listenerMask);
		base.HandleEvent(in_gameObject);
	}
}
