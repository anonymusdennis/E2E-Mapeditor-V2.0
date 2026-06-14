using System;

namespace Epic.OnlineServices;

internal interface ICallbackInfo : IDisposable
{
	IntPtr ClientDataAddress { get; }
}
