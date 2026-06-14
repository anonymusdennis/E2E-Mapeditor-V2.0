using UnityEngine;

[AddComponentMenu("Wwise/AkEvent")]
public class AkEvent : AkUnityEventHandler
{
	public int eventID;

	public GameObject soundEmitterObject;

	public bool enableActionOnEvent;

	public AkActionOnEventType actionOnEventType;

	public AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear;

	public float transitionDuration;

	public AkEventCallbackData m_callbackData;

	public uint playingId;

	private void Callback(object in_cookie, AkCallbackType in_type, object in_info)
	{
		for (int i = 0; i < m_callbackData.callbackFunc.Count; i++)
		{
			if (((uint)in_type & (uint)m_callbackData.callbackFlags[i]) != 0 && m_callbackData.callbackGameObj[i] != null)
			{
				AkEventCallbackMsg akEventCallbackMsg = default(AkEventCallbackMsg);
				akEventCallbackMsg.type = in_type;
				akEventCallbackMsg.sender = base.gameObject;
				akEventCallbackMsg.info = in_info;
				m_callbackData.callbackGameObj[i].SendMessage(m_callbackData.callbackFunc[i], akEventCallbackMsg);
			}
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		GameObject in_gameObjectID = (soundEmitterObject = ((!useOtherObject || !(in_gameObject != null)) ? base.gameObject : in_gameObject));
		if (enableActionOnEvent)
		{
			AkSoundEngine.ExecuteActionOnEvent((uint)eventID, actionOnEventType, in_gameObjectID, (int)transitionDuration * 1000, curveInterpolation);
			return;
		}
		if (m_callbackData != null)
		{
			playingId = AkSoundEngine.PostEvent((uint)eventID, in_gameObjectID, (uint)m_callbackData.uFlags, Callback, null, 0u, null, 0u);
		}
		else
		{
			playingId = AkSoundEngine.PostEvent((uint)eventID, in_gameObjectID);
		}
		if (playingId == 0)
		{
			Debug.LogError("Could not post event ID \"" + eventID + "\". Did you make sure to load the appropriate SoundBank?");
		}
	}

	public void Stop(int _transitionDuration, AkCurveInterpolation _curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear)
	{
		AkSoundEngine.ExecuteActionOnEvent((uint)eventID, AkActionOnEventType.AkActionOnEventType_Stop, soundEmitterObject, _transitionDuration, _curveInterpolation);
	}
}
