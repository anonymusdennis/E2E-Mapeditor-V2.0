using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Metrics;

public sealed class MetricsInterface : Handle
{
	public MetricsInterface()
		: base(IntPtr.Zero)
	{
	}

	public MetricsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result BeginPlayerSession(BeginPlayerSessionOptions options)
	{
		BeginPlayerSessionOptionsInternal options2 = Helper.CopyPropertiesToNew<BeginPlayerSessionOptionsInternal>(options);
		Result result = EOS_Metrics_BeginPlayerSession(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result EndPlayerSession(EndPlayerSessionOptions options)
	{
		EndPlayerSessionOptionsInternal options2 = Helper.CopyPropertiesToNew<EndPlayerSessionOptionsInternal>(options);
		Result result = EOS_Metrics_EndPlayerSession(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Metrics_EndPlayerSession(IntPtr handle, ref EndPlayerSessionOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Metrics_BeginPlayerSession(IntPtr handle, ref BeginPlayerSessionOptionsInternal options);
}
