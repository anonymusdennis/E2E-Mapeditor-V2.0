using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends;

public sealed class FriendsInterface : Handle
{
	public FriendsInterface()
		: base(IntPtr.Zero)
	{
	}

	public FriendsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryFriends(QueryFriendsOptions options, object clientData, OnQueryFriendsCallback completionDelegate)
	{
		QueryFriendsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryFriendsOptionsInternal>(options);
		OnQueryFriendsCallbackInternal onQueryFriendsCallbackInternal = OnQueryFriends;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryFriendsCallbackInternal);
		EOS_Friends_QueryFriends(base.InnerHandle, ref options2, clientDataAddress, onQueryFriendsCallbackInternal);
		options2.Dispose();
	}

	public void SendInvite(SendInviteOptions options, object clientData, OnSendInviteCallback completionDelegate)
	{
		SendInviteOptionsInternal options2 = Helper.CopyPropertiesToNew<SendInviteOptionsInternal>(options);
		OnSendInviteCallbackInternal onSendInviteCallbackInternal = OnSendInvite;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onSendInviteCallbackInternal);
		EOS_Friends_SendInvite(base.InnerHandle, ref options2, clientDataAddress, onSendInviteCallbackInternal);
		options2.Dispose();
	}

	public void AcceptInvite(AcceptInviteOptions options, object clientData, OnAcceptInviteCallback completionDelegate)
	{
		AcceptInviteOptionsInternal options2 = Helper.CopyPropertiesToNew<AcceptInviteOptionsInternal>(options);
		OnAcceptInviteCallbackInternal onAcceptInviteCallbackInternal = OnAcceptInvite;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onAcceptInviteCallbackInternal);
		EOS_Friends_AcceptInvite(base.InnerHandle, ref options2, clientDataAddress, onAcceptInviteCallbackInternal);
		options2.Dispose();
	}

	public void RejectInvite(RejectInviteOptions options, object clientData, OnRejectInviteCallback completionDelegate)
	{
		RejectInviteOptionsInternal options2 = Helper.CopyPropertiesToNew<RejectInviteOptionsInternal>(options);
		OnRejectInviteCallbackInternal onRejectInviteCallbackInternal = OnRejectInvite;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onRejectInviteCallbackInternal);
		EOS_Friends_RejectInvite(base.InnerHandle, ref options2, clientDataAddress, onRejectInviteCallbackInternal);
		options2.Dispose();
	}

	public int GetFriendsCount(GetFriendsCountOptions options)
	{
		GetFriendsCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetFriendsCountOptionsInternal>(options);
		int result = EOS_Friends_GetFriendsCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public EpicAccountId GetFriendAtIndex(GetFriendAtIndexOptions options)
	{
		GetFriendAtIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<GetFriendAtIndexOptionsInternal>(options);
		IntPtr innerHandle = EOS_Friends_GetFriendAtIndex(base.InnerHandle, ref options2);
		options2.Dispose();
		return Helper.GetHandle<EpicAccountId>(innerHandle);
	}

	public FriendsStatus GetStatus(GetStatusOptions options)
	{
		GetStatusOptionsInternal options2 = Helper.CopyPropertiesToNew<GetStatusOptionsInternal>(options);
		FriendsStatus result = EOS_Friends_GetStatus(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public ulong AddNotifyFriendsUpdate(AddNotifyFriendsUpdateOptions options, object clientData, OnFriendsUpdateCallback friendsUpdateHandler)
	{
		AddNotifyFriendsUpdateOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyFriendsUpdateOptionsInternal>(options);
		OnFriendsUpdateCallbackInternal onFriendsUpdateCallbackInternal = OnFriendsUpdate;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, friendsUpdateHandler, onFriendsUpdateCallbackInternal);
		ulong result = EOS_Friends_AddNotifyFriendsUpdate(base.InnerHandle, ref options2, clientDataAddress, onFriendsUpdateCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyFriendsUpdate(ulong notificationId)
	{
		EOS_Friends_RemoveNotifyFriendsUpdate(base.InnerHandle, notificationId);
	}

	[MonoPInvokeCallback]
	internal static void OnFriendsUpdate(IntPtr address)
	{
		OnFriendsUpdateCallback callDelegate = null;
		OnFriendsUpdateInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnFriendsUpdateCallback, OnFriendsUpdateInfoInternal, OnFriendsUpdateInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnRejectInvite(IntPtr address)
	{
		OnRejectInviteCallback callDelegate = null;
		RejectInviteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnRejectInviteCallback, RejectInviteCallbackInfoInternal, RejectInviteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnAcceptInvite(IntPtr address)
	{
		OnAcceptInviteCallback callDelegate = null;
		AcceptInviteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnAcceptInviteCallback, AcceptInviteCallbackInfoInternal, AcceptInviteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnSendInvite(IntPtr address)
	{
		OnSendInviteCallback callDelegate = null;
		SendInviteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnSendInviteCallback, SendInviteCallbackInfoInternal, SendInviteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryFriends(IntPtr address)
	{
		OnQueryFriendsCallback callDelegate = null;
		QueryFriendsCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryFriendsCallback, QueryFriendsCallbackInfoInternal, QueryFriendsCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Friends_RemoveNotifyFriendsUpdate(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Friends_AddNotifyFriendsUpdate(IntPtr handle, ref AddNotifyFriendsUpdateOptionsInternal options, IntPtr clientData, OnFriendsUpdateCallbackInternal friendsUpdateHandler);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern FriendsStatus EOS_Friends_GetStatus(IntPtr handle, ref GetStatusOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Friends_GetFriendAtIndex(IntPtr handle, ref GetFriendAtIndexOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern int EOS_Friends_GetFriendsCount(IntPtr handle, ref GetFriendsCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Friends_RejectInvite(IntPtr handle, ref RejectInviteOptionsInternal options, IntPtr clientData, OnRejectInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Friends_AcceptInvite(IntPtr handle, ref AcceptInviteOptionsInternal options, IntPtr clientData, OnAcceptInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Friends_SendInvite(IntPtr handle, ref SendInviteOptionsInternal options, IntPtr clientData, OnSendInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Friends_QueryFriends(IntPtr handle, ref QueryFriendsOptionsInternal options, IntPtr clientData, OnQueryFriendsCallbackInternal completionDelegate);
}
