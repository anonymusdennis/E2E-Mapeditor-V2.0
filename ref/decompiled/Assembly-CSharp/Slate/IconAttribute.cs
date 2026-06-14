using System;

namespace Slate;

[AttributeUsage(AttributeTargets.Class)]
public class IconAttribute : Attribute
{
	public string iconName;

	public IconAttribute(string iconName)
	{
		this.iconName = iconName;
	}
}
