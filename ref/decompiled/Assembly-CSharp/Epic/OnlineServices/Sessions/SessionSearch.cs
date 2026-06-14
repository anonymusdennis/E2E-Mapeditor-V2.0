using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionSearch : Handle
{
	public SessionSearch()
		: base(IntPtr.Zero)
	{
	}

	public SessionSearch(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetSessionId(SessionSearchSetSessionIdOptions options)
	{
		SessionSearchSetSessionIdOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchSetSessionIdOptionsInternal>(options);
		Result result = EOS_SessionSearch_SetSessionId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetTargetUserId(SessionSearchSetTargetUserIdOptions options)
	{
		SessionSearchSetTargetUserIdOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchSetTargetUserIdOptionsInternal>(options);
		Result result = EOS_SessionSearch_SetTargetUserId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetParameter(SessionSearchSetParameterOptions options)
	{
		SessionSearchSetParameterOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchSetParameterOptionsInternal>(options);
		Result result = EOS_SessionSearch_SetParameter(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result RemoveParameter(SessionSearchRemoveParameterOptions options)
	{
		SessionSearchRemoveParameterOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchRemoveParameterOptionsInternal>(options);
		Result result = EOS_SessionSearch_RemoveParameter(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetMaxResults(SessionSearchSetMaxResultsOptions options)
	{
		SessionSearchSetMaxResultsOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchSetMaxResultsOptionsInternal>(options);
		Result result = EOS_SessionSearch_SetMaxResults(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void Find(SessionSearchFindOptions options, object clientData, SessionSearchOnFindCallback completionDelegate)
	{
		SessionSearchFindOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchFindOptionsInternal>(options);
		SessionSearchOnFindCallbackInternal sessionSearchOnFindCallbackInternal = SessionSearchOnFind;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, sessionSearchOnFindCallbackInternal);
		EOS_SessionSearch_Find(base.InnerHandle, ref options2, clientDataAddress, sessionSearchOnFindCallbackInternal);
		options2.Dispose();
	}

	public uint GetSearchResultCount(SessionSearchGetSearchResultCountOptions options)
	{
		SessionSearchGetSearchResultCountOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchGetSearchResultCountOptionsInternal>(options);
		uint result = EOS_SessionSearch_GetSearchResultCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopySearchResultByIndex(SessionSearchCopySearchResultByIndexOptions options, out SessionDetails outSessionHandle)
	{
		SessionSearchCopySearchResultByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionSearchCopySearchResultByIndexOptionsInternal>(options);
		outSessionHandle = Helper.GetDefault<SessionDetails>();
		IntPtr outSessionHandle2 = IntPtr.Zero;
		Result result = EOS_SessionSearch_CopySearchResultByIndex(base.InnerHandle, ref options2, ref outSessionHandle2);
		options2.Dispose();
		outSessionHandle = Helper.GetHandle<SessionDetails>(outSessionHandle2);
		return result;
	}

	public void Release()
	{
		EOS_SessionSearch_Release(base.InnerHandle);
	}

	[MonoPInvokeCallback]
	internal static void SessionSearchOnFind(IntPtr address)
	{
		SessionSearchOnFindCallback callDelegate = null;
		SessionSearchFindCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<SessionSearchOnFindCallback, SessionSearchFindCallbackInfoInternal, SessionSearchFindCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_SessionSearch_Release(IntPtr sessionSearchHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionSearch_CopySearchResultByIndex(IntPtr handle, ref SessionSearchCopySearchResultByIndexOptionsInternal options, ref IntPtr outSessionHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_SessionSearch_GetSearchResultCount(IntPtr handle, ref SessionSearchGetSearchResultCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_SessionSearch_Find(IntPtr handle, ref SessionSearchFindOptionsInternal options, IntPtr clientData, SessionSearchOnFindCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionSearch_SetMaxResults(IntPtr handle, ref SessionSearchSetMaxResultsOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionSearch_RemoveParameter(IntPtr handle, ref SessionSearchRemoveParameterOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionSearch_SetParameter(IntPtr handle, ref SessionSearchSetParameterOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionSearch_SetTargetUserId(IntPtr handle, ref SessionSearchSetTargetUserIdOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionSearch_SetSessionId(IntPtr handle, ref SessionSearchSetSessionIdOptionsInternal options);
}
