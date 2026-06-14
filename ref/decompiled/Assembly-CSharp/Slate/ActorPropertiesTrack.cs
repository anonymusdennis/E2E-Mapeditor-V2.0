using System;
using UnityEngine;

namespace Slate;

[Attachable(new Type[] { typeof(ActorGroup) })]
public class ActorPropertiesTrack : PropertiesTrack
{
	protected override void OnCreate()
	{
		base.OnCreate();
		base.animationData.TryAddParameter(typeof(Transform).RTGetProperty("localPosition"), base.actor, null);
	}
}
