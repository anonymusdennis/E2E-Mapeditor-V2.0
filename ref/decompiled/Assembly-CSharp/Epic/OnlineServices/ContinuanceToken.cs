using System;

namespace Epic.OnlineServices;

public sealed class ContinuanceToken : Handle
{
	public ContinuanceToken()
		: base(IntPtr.Zero)
	{
	}

	public ContinuanceToken(IntPtr innerHandle)
		: base(innerHandle)
	{
	}
}
