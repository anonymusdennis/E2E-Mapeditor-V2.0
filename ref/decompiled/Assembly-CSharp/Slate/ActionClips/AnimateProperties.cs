using System;
using UnityEngine;

namespace Slate.ActionClips;

[Description("Animate any number of properties on any component of the actor.\nThis is identical to using a Properties Track, but instead the animated properties are stored within the clip and thus can be moved around as a group easier.")]
[Attachable(new Type[]
{
	typeof(ActorActionTrack),
	typeof(DirectorActionTrack)
})]
public class AnimateProperties : ActionClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 5f;

	[SerializeField]
	private string _name;

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public override bool isValid => base.animationData != null && base.animationData.isValid;

	public override string info => (!isValid) ? "No Properties Added" : ((!string.IsNullOrEmpty(_name)) ? _name : base.animationData.ToString());

	public override object animatedParametersTarget => base.actor;
}
