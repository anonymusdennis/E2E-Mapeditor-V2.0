using System;
using System.Linq;
using UnityEngine;

namespace Slate.ActionClips;

[Attachable(new Type[] { typeof(MecanimTrack) })]
public abstract class MecanimBaseClip : ActorActionClip<Animator>
{
	public override bool isValid => base.actor != null && base.actor.runtimeAnimatorController != null;

	protected bool HasParameter(string name)
	{
		if (base.actor == null)
		{
			return false;
		}
		if (!base.actor.isInitialized)
		{
			return true;
		}
		AnimatorControllerParameter[] parameters = base.actor.parameters;
		return parameters != null && parameters.FirstOrDefault((AnimatorControllerParameter p) => p.name == name) != null;
	}
}
