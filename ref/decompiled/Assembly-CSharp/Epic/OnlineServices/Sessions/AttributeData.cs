using System;

namespace Epic.OnlineServices.Sessions;

public class AttributeData
{
	public int ApiVersion => 1;

	public string Key { get; set; }

	public IntPtr Value { get; set; }

	public AttributeType ValueType { get; set; }
}
