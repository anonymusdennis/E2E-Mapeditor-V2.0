using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkAmbient")]
[RequireComponent(typeof(AkGameObj))]
public class AkAmbient : AkEvent
{
	public MultiPositionTypeLabel multiPositionTypeLabel;

	public List<Vector3> multiPositionArray = new List<Vector3>();

	public static Dictionary<int, AkMultiPosEvent> multiPosEventTree = new Dictionary<int, AkMultiPosEvent>();

	public AkAmbient ParentAkAmbience { get; set; }

	private void OnEnable()
	{
		if (multiPositionTypeLabel == MultiPositionTypeLabel.Simple_Mode)
		{
			AkGameObj[] components = base.gameObject.GetComponents<AkGameObj>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = true;
			}
		}
		else if (multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
		{
			AkGameObj[] components2 = base.gameObject.GetComponents<AkGameObj>();
			for (int j = 0; j < components2.Length; j++)
			{
				components2[j].enabled = false;
			}
			AkPositionArray akPositionArray = BuildAkPositionArray();
			AkSoundEngine.SetMultiplePositions(base.gameObject, akPositionArray, (ushort)akPositionArray.Count, MultiPositionType.MultiPositionType_MultiSources);
		}
		else
		{
			if (multiPositionTypeLabel != MultiPositionTypeLabel.MultiPosition_Mode)
			{
				return;
			}
			AkGameObj[] components3 = base.gameObject.GetComponents<AkGameObj>();
			for (int k = 0; k < components3.Length; k++)
			{
				components3[k].enabled = false;
			}
			if (multiPosEventTree.TryGetValue(eventID, out var value))
			{
				if (!value.list.Contains(this))
				{
					value.list.Add(this);
				}
			}
			else
			{
				value = new AkMultiPosEvent();
				value.list.Add(this);
				multiPosEventTree.Add(eventID, value);
			}
			AkPositionArray akPositionArray2 = BuildMultiDirectionArray(ref value);
			AkSoundEngine.SetMultiplePositions(value.list[0].gameObject, akPositionArray2, (ushort)akPositionArray2.Count, MultiPositionType.MultiPositionType_MultiSources);
		}
	}

	private void OnDisable()
	{
		if (multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode)
		{
			AkMultiPosEvent eventPosList = multiPosEventTree[eventID];
			if (eventPosList.list.Count == 1)
			{
				multiPosEventTree.Remove(eventID);
				return;
			}
			eventPosList.list.Remove(this);
			AkPositionArray akPositionArray = BuildMultiDirectionArray(ref eventPosList);
			AkSoundEngine.SetMultiplePositions(eventPosList.list[0].gameObject, akPositionArray, (ushort)akPositionArray.Count, MultiPositionType.MultiPositionType_MultiSources);
		}
	}

	public override void HandleEvent(GameObject in_gameObject)
	{
		if (multiPositionTypeLabel != MultiPositionTypeLabel.MultiPosition_Mode)
		{
			base.HandleEvent(in_gameObject);
			return;
		}
		AkMultiPosEvent akMultiPosEvent = multiPosEventTree[eventID];
		if (!akMultiPosEvent.eventIsPlaying)
		{
			akMultiPosEvent.eventIsPlaying = true;
			soundEmitterObject = akMultiPosEvent.list[0].gameObject;
			if (enableActionOnEvent)
			{
				AkSoundEngine.ExecuteActionOnEvent((uint)eventID, actionOnEventType, akMultiPosEvent.list[0].gameObject, (int)transitionDuration * 1000, curveInterpolation);
			}
			else
			{
				playingId = AkSoundEngine.PostEvent((uint)eventID, akMultiPosEvent.list[0].gameObject, 1u, akMultiPosEvent.FinishedPlaying, null, 0u, null, 0u);
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawIcon(base.transform.position, "WwiseAudioSpeaker.png", allowScaling: false);
	}

	public AkPositionArray BuildMultiDirectionArray(ref AkMultiPosEvent eventPosList)
	{
		AkPositionArray akPositionArray = new AkPositionArray((uint)eventPosList.list.Count);
		for (int i = 0; i < eventPosList.list.Count; i++)
		{
			akPositionArray.Add(eventPosList.list[i].transform.position, eventPosList.list[i].transform.forward, eventPosList.list[i].transform.up);
		}
		return akPositionArray;
	}

	private AkPositionArray BuildAkPositionArray()
	{
		AkPositionArray akPositionArray = new AkPositionArray((uint)multiPositionArray.Count);
		for (int i = 0; i < multiPositionArray.Count; i++)
		{
			akPositionArray.Add(base.transform.position + multiPositionArray[i], base.transform.forward, base.transform.up);
		}
		return akPositionArray;
	}
}
