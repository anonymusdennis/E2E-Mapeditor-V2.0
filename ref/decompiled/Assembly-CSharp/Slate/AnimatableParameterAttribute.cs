using System;
using UnityEngine;

namespace Slate;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class AnimatableParameterAttribute : PropertyAttribute
{
	public string link;

	public float? min;

	public float? max;

	public AnimatableParameterAttribute()
	{
	}

	public AnimatableParameterAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}
