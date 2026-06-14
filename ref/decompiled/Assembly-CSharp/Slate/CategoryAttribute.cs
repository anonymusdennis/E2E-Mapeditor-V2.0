using System;

namespace Slate;

[AttributeUsage(AttributeTargets.Class)]
public class CategoryAttribute : Attribute
{
	public string category;

	public CategoryAttribute(string category)
	{
		this.category = category;
	}
}
