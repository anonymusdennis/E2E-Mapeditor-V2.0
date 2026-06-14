using System;

namespace NexPlugin;

public static class Common
{
	[Flags]
	public enum ThreadMode
	{
		ThreadModeSafeTransportBuffer = 1,
		ThreadModeUnsafeTransportBuffer = 2
	}

	[Flags]
	public enum DispachFlag
	{
		ContinueWhenEmpty = 1,
		DispatchKeepAliveOnly = 2
	}
}
