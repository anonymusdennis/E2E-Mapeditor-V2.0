using System;

namespace Slate;

[Attachable(new Type[] { typeof(ActorGroup) })]
public class ActorActionTrack : ActionTrack
{
	protected override void OnCreate()
	{
		base.OnCreate();
	}
}
