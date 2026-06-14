using System;

namespace Slate;

[AttributeUsage(AttributeTargets.Class)]
public class NameAttribute : Attribute
{
	public string name;

	public NameAttribute(string name)
	{
		this.name = name;
	}
}
