using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Logging;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LogMessageInternal : IDisposable
{
	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Category;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Message;

	private LogLevel m_Level;

	public string Category => m_Category;

	public string Message => m_Message;

	public LogLevel Level => m_Level;

	public void Dispose()
	{
	}
}
