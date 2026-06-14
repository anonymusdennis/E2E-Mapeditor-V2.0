using System;

namespace Slate;

[AttributeUsage(AttributeTargets.Class)]
public class AttachableAttribute : Attribute
{
	public Type[] types;

	public AttachableAttribute(params Type[] types)
	{
		this.types = types;
	}
}
