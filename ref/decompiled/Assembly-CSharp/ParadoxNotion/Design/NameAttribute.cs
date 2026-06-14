using System;

namespace ParadoxNotion.Design;

public class NameAttribute : Attribute
{
	public string name;

	public NameAttribute(string name)
	{
		this.name = name;
	}
}
