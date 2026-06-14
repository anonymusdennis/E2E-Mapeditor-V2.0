using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class ActiveSession : Handle
{
	public ActiveSession()
		: base(IntPtr.Zero)
	{
	}

	public ActiveSession(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result CopyInfo(ActiveSessionCopyInfoOptions options, out ActiveSessionInfo outActiveSessionInfo)
	{
		ActiveSessionCopyInfoOptionsInternal options2 = Helper.CopyPropertiesToNew<ActiveSessionCopyInfoOptionsInternal>(options);
		outActiveSessionInfo = Helper.GetDefault<ActiveSessionInfo>();
		IntPtr outActiveSessionInfo2 = IntPtr.Zero;
		Result result = EOS_ActiveSession_CopyInfo(base.InnerHandle, ref options2, ref outActiveSessionInfo2);
		options2.Dispose();
		if (Helper.TryMarshal<ActiveSessionInfoInternal, ActiveSessionInfo>(outActiveSessionInfo2, out outActiveSessionInfo))
		{
			EOS_ActiveSession_Info_Release(outActiveSessionInfo2);
		}
		return result;
	}

	public uint GetRegisteredPlayerCount(ActiveSessionGetRegisteredPlayerCountOptions options)
	{
		ActiveSessionGetRegisteredPlayerCountOptionsInternal options2 = Helper.CopyPropertiesToNew<ActiveSessionGetRegisteredPlayerCountOptionsInternal>(options);
		uint result = EOS_ActiveSession_GetRegisteredPlayerCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public ProductUserId GetRegisteredPlayerByIndex(ActiveSessionGetRegisteredPlayerByIndexOptions options)
	{
		ActiveSessionGetRegisteredPlayerByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<ActiveSessionGetRegisteredPlayerByIndexOptionsInternal>(options);
		IntPtr innerHandle = EOS_ActiveSession_GetRegisteredPlayerByIndex(base.InnerHandle, ref options2);
		options2.Dispose();
		return Helper.GetHandle<ProductUserId>(innerHandle);
	}

	public void Release()
	{
		EOS_ActiveSession_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_ActiveSession_Info_Release(IntPtr activeSessionInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_ActiveSession_Release(IntPtr activeSessionHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_ActiveSession_GetRegisteredPlayerByIndex(IntPtr handle, ref ActiveSessionGetRegisteredPlayerByIndexOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_ActiveSession_GetRegisteredPlayerCount(IntPtr handle, ref ActiveSessionGetRegisteredPlayerCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_ActiveSession_CopyInfo(IntPtr handle, ref ActiveSessionCopyInfoOptionsInternal options, ref IntPtr outActiveSessionInfo);
}
