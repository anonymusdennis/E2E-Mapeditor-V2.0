using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

public sealed class LobbySearch : Handle
{
	public LobbySearch()
		: base(IntPtr.Zero)
	{
	}

	public LobbySearch(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void Find(LobbySearchFindOptions options, object clientData, LobbySearchOnFindCallback completionDelegate)
	{
		LobbySearchFindOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchFindOptionsInternal>(options);
		LobbySearchOnFindCallbackInternal lobbySearchOnFindCallbackInternal = LobbySearchOnFind;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, lobbySearchOnFindCallbackInternal);
		EOS_LobbySearch_Find(base.InnerHandle, ref options2, clientDataAddress, lobbySearchOnFindCallbackInternal);
		options2.Dispose();
	}

	public Result SetLobbyId(LobbySearchSetLobbyIdOptions options)
	{
		LobbySearchSetLobbyIdOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchSetLobbyIdOptionsInternal>(options);
		Result result = EOS_LobbySearch_SetLobbyId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetTargetUserId(LobbySearchSetTargetUserIdOptions options)
	{
		LobbySearchSetTargetUserIdOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchSetTargetUserIdOptionsInternal>(options);
		Result result = EOS_LobbySearch_SetTargetUserId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetParameter(LobbySearchSetParameterOptions options)
	{
		LobbySearchSetParameterOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchSetParameterOptionsInternal>(options);
		Result result = EOS_LobbySearch_SetParameter(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result RemoveParameter(LobbySearchRemoveParameterOptions options)
	{
		LobbySearchRemoveParameterOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchRemoveParameterOptionsInternal>(options);
		Result result = EOS_LobbySearch_RemoveParameter(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetMaxResults(LobbySearchSetMaxResultsOptions options)
	{
		LobbySearchSetMaxResultsOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchSetMaxResultsOptionsInternal>(options);
		Result result = EOS_LobbySearch_SetMaxResults(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public uint GetSearchResultCount(LobbySearchGetSearchResultCountOptions options)
	{
		LobbySearchGetSearchResultCountOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchGetSearchResultCountOptionsInternal>(options);
		uint result = EOS_LobbySearch_GetSearchResultCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopySearchResultByIndex(LobbySearchCopySearchResultByIndexOptions options, out LobbyDetails outLobbyDetailsHandle)
	{
		LobbySearchCopySearchResultByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbySearchCopySearchResultByIndexOptionsInternal>(options);
		outLobbyDetailsHandle = Helper.GetDefault<LobbyDetails>();
		IntPtr outLobbyDetailsHandle2 = IntPtr.Zero;
		Result result = EOS_LobbySearch_CopySearchResultByIndex(base.InnerHandle, ref options2, ref outLobbyDetailsHandle2);
		options2.Dispose();
		outLobbyDetailsHandle = Helper.GetHandle<LobbyDetails>(outLobbyDetailsHandle2);
		return result;
	}

	public void Release()
	{
		EOS_LobbySearch_Release(base.InnerHandle);
	}

	[MonoPInvokeCallback]
	internal static void LobbySearchOnFind(IntPtr address)
	{
		LobbySearchOnFindCallback callDelegate = null;
		LobbySearchFindCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<LobbySearchOnFindCallback, LobbySearchFindCallbackInfoInternal, LobbySearchFindCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_LobbySearch_Release(IntPtr lobbySearchHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbySearch_CopySearchResultByIndex(IntPtr handle, ref LobbySearchCopySearchResultByIndexOptionsInternal options, ref IntPtr outLobbyDetailsHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_LobbySearch_GetSearchResultCount(IntPtr handle, ref LobbySearchGetSearchResultCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbySearch_SetMaxResults(IntPtr handle, ref LobbySearchSetMaxResultsOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbySearch_RemoveParameter(IntPtr handle, ref LobbySearchRemoveParameterOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbySearch_SetParameter(IntPtr handle, ref LobbySearchSetParameterOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbySearch_SetTargetUserId(IntPtr handle, ref LobbySearchSetTargetUserIdOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbySearch_SetLobbyId(IntPtr handle, ref LobbySearchSetLobbyIdOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_LobbySearch_Find(IntPtr handle, ref LobbySearchFindOptionsInternal options, IntPtr clientData, LobbySearchOnFindCallbackInternal completionDelegate);
}
