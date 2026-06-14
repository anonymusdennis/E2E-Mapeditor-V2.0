using System;
using UnityEngine;

namespace Slate;

[AttributeUsage(AttributeTargets.Field)]
public class MinAttribute : PropertyAttribute
{
	public float min;

	public MinAttribute(float min)
	{
		this.min = min;
	}
}
