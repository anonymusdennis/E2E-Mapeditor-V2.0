using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Logging;

public static class LoggingInterface
{
	private static LogMessageFuncInternal s_LogMessageFuncInternal;

	private static LogMessageFunc s_LogMessageFunc;

	public static Result SetCallback(LogMessageFunc callback)
	{
		LogMessageFuncInternal callback2 = LogMessageFunc;
		s_LogMessageFunc = callback;
		s_LogMessageFuncInternal = callback2;
		return EOS_Logging_SetCallback(callback2);
	}

	public static Result SetLogLevel(LogCategory logCategory, LogLevel logLevel)
	{
		return EOS_Logging_SetLogLevel(logCategory, logLevel);
	}

	[MonoPInvokeCallback]
	internal static void LogMessageFunc(IntPtr address)
	{
		LogMessage to = null;
		if (Helper.TryMarshal<LogMessageInternal, LogMessage>(address, out to))
		{
			s_LogMessageFunc(to);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Logging_SetLogLevel(LogCategory logCategory, LogLevel logLevel);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Logging_SetCallback(LogMessageFuncInternal callback);
}
