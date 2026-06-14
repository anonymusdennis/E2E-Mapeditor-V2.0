using System;
using UnityEngine;

namespace Slate;

[AttributeUsage(AttributeTargets.Field)]
public class ShaderPropertyPopupAttribute : PropertyAttribute
{
	public Type propertyType;

	public ShaderPropertyPopupAttribute()
	{
	}

	public ShaderPropertyPopupAttribute(Type propertyType)
	{
		this.propertyType = propertyType;
	}
}
