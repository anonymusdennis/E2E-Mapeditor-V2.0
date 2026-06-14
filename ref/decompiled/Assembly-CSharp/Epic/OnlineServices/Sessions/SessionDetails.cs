using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionDetails : Handle
{
	public SessionDetails()
		: base(IntPtr.Zero)
	{
	}

	public SessionDetails(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result CopyInfo(SessionDetailsCopyInfoOptions options, out SessionDetailsInfo outSessionInfo)
	{
		SessionDetailsCopyInfoOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionDetailsCopyInfoOptionsInternal>(options);
		outSessionInfo = Helper.GetDefault<SessionDetailsInfo>();
		IntPtr outSessionInfo2 = IntPtr.Zero;
		Result result = EOS_SessionDetails_CopyInfo(base.InnerHandle, ref options2, ref outSessionInfo2);
		options2.Dispose();
		if (Helper.TryMarshal<SessionDetailsInfoInternal, SessionDetailsInfo>(outSessionInfo2, out outSessionInfo))
		{
			EOS_SessionDetails_Info_Release(outSessionInfo2);
		}
		return result;
	}

	public uint GetSessionAttributeCount(SessionDetailsGetSessionAttributeCountOptions options)
	{
		SessionDetailsGetSessionAttributeCountOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionDetailsGetSessionAttributeCountOptionsInternal>(options);
		uint result = EOS_SessionDetails_GetSessionAttributeCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopySessionAttributeByIndex(SessionDetailsCopySessionAttributeByIndexOptions options, out SessionDetailsAttribute outSessionAttribute)
	{
		SessionDetailsCopySessionAttributeByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionDetailsCopySessionAttributeByIndexOptionsInternal>(options);
		outSessionAttribute = Helper.GetDefault<SessionDetailsAttribute>();
		IntPtr outSessionAttribute2 = IntPtr.Zero;
		Result result = EOS_SessionDetails_CopySessionAttributeByIndex(base.InnerHandle, ref options2, ref outSessionAttribute2);
		options2.Dispose();
		if (Helper.TryMarshal<SessionDetailsAttributeInternal, SessionDetailsAttribute>(outSessionAttribute2, out outSessionAttribute))
		{
			EOS_SessionDetails_Attribute_Release(outSessionAttribute2);
		}
		return result;
	}

	public Result CopySessionAttributeByKey(SessionDetailsCopySessionAttributeByKeyOptions options, out SessionDetailsAttribute outSessionAttribute)
	{
		SessionDetailsCopySessionAttributeByKeyOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionDetailsCopySessionAttributeByKeyOptionsInternal>(options);
		outSessionAttribute = Helper.GetDefault<SessionDetailsAttribute>();
		IntPtr outSessionAttribute2 = IntPtr.Zero;
		Result result = EOS_SessionDetails_CopySessionAttributeByKey(base.InnerHandle, ref options2, ref outSessionAttribute2);
		options2.Dispose();
		if (Helper.TryMarshal<SessionDetailsAttributeInternal, SessionDetailsAttribute>(outSessionAttribute2, out outSessionAttribute))
		{
			EOS_SessionDetails_Attribute_Release(outSessionAttribute2);
		}
		return result;
	}

	public void Release()
	{
		EOS_SessionDetails_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_SessionDetails_Info_Release(IntPtr sessionInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_SessionDetails_Attribute_Release(IntPtr sessionAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_SessionDetails_Release(IntPtr sessionHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionDetails_CopySessionAttributeByKey(IntPtr handle, ref SessionDetailsCopySessionAttributeByKeyOptionsInternal options, ref IntPtr outSessionAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionDetails_CopySessionAttributeByIndex(IntPtr handle, ref SessionDetailsCopySessionAttributeByIndexOptionsInternal options, ref IntPtr outSessionAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_SessionDetails_GetSessionAttributeCount(IntPtr handle, ref SessionDetailsGetSessionAttributeCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionDetails_CopyInfo(IntPtr handle, ref SessionDetailsCopyInfoOptionsInternal options, ref IntPtr outSessionInfo);
}
