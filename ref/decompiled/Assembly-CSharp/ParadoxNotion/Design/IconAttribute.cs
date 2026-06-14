using System;

namespace ParadoxNotion.Design;

[AttributeUsage(AttributeTargets.Class)]
public class IconAttribute : Attribute
{
	public string iconName;

	public bool fixedColor;

	public IconAttribute(string iconName, bool fixedColor = false)
	{
		this.iconName = iconName;
		this.fixedColor = fixedColor;
	}
}
