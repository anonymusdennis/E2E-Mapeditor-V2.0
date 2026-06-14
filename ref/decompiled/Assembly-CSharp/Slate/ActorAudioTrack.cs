using System;
using UnityEngine;

namespace Slate;

[Attachable(new Type[] { typeof(ActorGroup) })]
public class ActorAudioTrack : AudioTrack
{
	[SerializeField]
	private bool _useAudioSourceOnActor;

	public override bool useAudioSourceOnActor => _useAudioSourceOnActor;
}
