using System;
using UnityEngine;

namespace Slate;

[AttributeUsage(AttributeTargets.Field)]
public class ExampleTextAttribute : PropertyAttribute
{
	public string text;

	public ExampleTextAttribute(string text)
	{
		this.text = text;
	}
}
