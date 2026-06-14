using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

public sealed class LobbyInterface : Handle
{
	public LobbyInterface()
		: base(IntPtr.Zero)
	{
	}

	public LobbyInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void CreateLobby(CreateLobbyOptions options, object clientData, OnCreateLobbyCallback completionDelegate)
	{
		CreateLobbyOptionsInternal options2 = Helper.CopyPropertiesToNew<CreateLobbyOptionsInternal>(options);
		OnCreateLobbyCallbackInternal onCreateLobbyCallbackInternal = OnCreateLobby;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onCreateLobbyCallbackInternal);
		EOS_Lobby_CreateLobby(base.InnerHandle, ref options2, clientDataAddress, onCreateLobbyCallbackInternal);
		options2.Dispose();
	}

	public void DestroyLobby(DestroyLobbyOptions options, object clientData, OnDestroyLobbyCallback completionDelegate)
	{
		DestroyLobbyOptionsInternal options2 = Helper.CopyPropertiesToNew<DestroyLobbyOptionsInternal>(options);
		OnDestroyLobbyCallbackInternal onDestroyLobbyCallbackInternal = OnDestroyLobby;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onDestroyLobbyCallbackInternal);
		EOS_Lobby_DestroyLobby(base.InnerHandle, ref options2, clientDataAddress, onDestroyLobbyCallbackInternal);
		options2.Dispose();
	}

	public void JoinLobby(JoinLobbyOptions options, object clientData, OnJoinLobbyCallback completionDelegate)
	{
		JoinLobbyOptionsInternal options2 = Helper.CopyPropertiesToNew<JoinLobbyOptionsInternal>(options);
		OnJoinLobbyCallbackInternal onJoinLobbyCallbackInternal = OnJoinLobby;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onJoinLobbyCallbackInternal);
		EOS_Lobby_JoinLobby(base.InnerHandle, ref options2, clientDataAddress, onJoinLobbyCallbackInternal);
		options2.Dispose();
	}

	public void LeaveLobby(LeaveLobbyOptions options, object clientData, OnLeaveLobbyCallback completionDelegate)
	{
		LeaveLobbyOptionsInternal options2 = Helper.CopyPropertiesToNew<LeaveLobbyOptionsInternal>(options);
		OnLeaveLobbyCallbackInternal onLeaveLobbyCallbackInternal = OnLeaveLobby;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onLeaveLobbyCallbackInternal);
		EOS_Lobby_LeaveLobby(base.InnerHandle, ref options2, clientDataAddress, onLeaveLobbyCallbackInternal);
		options2.Dispose();
	}

	public Result UpdateLobbyModification(UpdateLobbyModificationOptions options, out LobbyModification outLobbyModificationHandle)
	{
		UpdateLobbyModificationOptionsInternal options2 = Helper.CopyPropertiesToNew<UpdateLobbyModificationOptionsInternal>(options);
		outLobbyModificationHandle = Helper.GetDefault<LobbyModification>();
		IntPtr outLobbyModificationHandle2 = IntPtr.Zero;
		Result result = EOS_Lobby_UpdateLobbyModification(base.InnerHandle, ref options2, ref outLobbyModificationHandle2);
		options2.Dispose();
		outLobbyModificationHandle = Helper.GetHandle<LobbyModification>(outLobbyModificationHandle2);
		return result;
	}

	public void UpdateLobby(UpdateLobbyOptions options, object clientData, OnUpdateLobbyCallback completionDelegate)
	{
		UpdateLobbyOptionsInternal options2 = Helper.CopyPropertiesToNew<UpdateLobbyOptionsInternal>(options);
		OnUpdateLobbyCallbackInternal onUpdateLobbyCallbackInternal = OnUpdateLobby;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onUpdateLobbyCallbackInternal);
		EOS_Lobby_UpdateLobby(base.InnerHandle, ref options2, clientDataAddress, onUpdateLobbyCallbackInternal);
		options2.Dispose();
	}

	public void PromoteMember(PromoteMemberOptions options, object clientData, OnPromoteMemberCallback completionDelegate)
	{
		PromoteMemberOptionsInternal options2 = Helper.CopyPropertiesToNew<PromoteMemberOptionsInternal>(options);
		OnPromoteMemberCallbackInternal onPromoteMemberCallbackInternal = OnPromoteMember;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onPromoteMemberCallbackInternal);
		EOS_Lobby_PromoteMember(base.InnerHandle, ref options2, clientDataAddress, onPromoteMemberCallbackInternal);
		options2.Dispose();
	}

	public void KickMember(KickMemberOptions options, object clientData, OnKickMemberCallback completionDelegate)
	{
		KickMemberOptionsInternal options2 = Helper.CopyPropertiesToNew<KickMemberOptionsInternal>(options);
		OnKickMemberCallbackInternal onKickMemberCallbackInternal = OnKickMember;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onKickMemberCallbackInternal);
		EOS_Lobby_KickMember(base.InnerHandle, ref options2, clientDataAddress, onKickMemberCallbackInternal);
		options2.Dispose();
	}

	public ulong AddNotifyLobbyUpdateReceived(AddNotifyLobbyUpdateReceivedOptions options, object clientData, OnLobbyUpdateReceivedCallback notificationFn)
	{
		AddNotifyLobbyUpdateReceivedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyLobbyUpdateReceivedOptionsInternal>(options);
		OnLobbyUpdateReceivedCallbackInternal onLobbyUpdateReceivedCallbackInternal = OnLobbyUpdateReceived;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onLobbyUpdateReceivedCallbackInternal);
		ulong result = EOS_Lobby_AddNotifyLobbyUpdateReceived(base.InnerHandle, ref options2, clientDataAddress, onLobbyUpdateReceivedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLobbyUpdateReceived(ulong inId)
	{
		EOS_Lobby_RemoveNotifyLobbyUpdateReceived(base.InnerHandle, inId);
	}

	public ulong AddNotifyLobbyMemberUpdateReceived(AddNotifyLobbyMemberUpdateReceivedOptions options, object clientData, OnLobbyMemberUpdateReceivedCallback notificationFn)
	{
		AddNotifyLobbyMemberUpdateReceivedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyLobbyMemberUpdateReceivedOptionsInternal>(options);
		OnLobbyMemberUpdateReceivedCallbackInternal onLobbyMemberUpdateReceivedCallbackInternal = OnLobbyMemberUpdateReceived;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onLobbyMemberUpdateReceivedCallbackInternal);
		ulong result = EOS_Lobby_AddNotifyLobbyMemberUpdateReceived(base.InnerHandle, ref options2, clientDataAddress, onLobbyMemberUpdateReceivedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLobbyMemberUpdateReceived(ulong inId)
	{
		EOS_Lobby_RemoveNotifyLobbyMemberUpdateReceived(base.InnerHandle, inId);
	}

	public ulong AddNotifyLobbyMemberStatusReceived(AddNotifyLobbyMemberStatusReceivedOptions options, object clientData, OnLobbyMemberStatusReceivedCallback notificationFn)
	{
		AddNotifyLobbyMemberStatusReceivedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyLobbyMemberStatusReceivedOptionsInternal>(options);
		OnLobbyMemberStatusReceivedCallbackInternal onLobbyMemberStatusReceivedCallbackInternal = OnLobbyMemberStatusReceived;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onLobbyMemberStatusReceivedCallbackInternal);
		ulong result = EOS_Lobby_AddNotifyLobbyMemberStatusReceived(base.InnerHandle, ref options2, clientDataAddress, onLobbyMemberStatusReceivedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLobbyMemberStatusReceived(ulong inId)
	{
		EOS_Lobby_RemoveNotifyLobbyMemberStatusReceived(base.InnerHandle, inId);
	}

	public void SendInvite(SendInviteOptions options, object clientData, OnSendInviteCallback completionDelegate)
	{
		SendInviteOptionsInternal options2 = Helper.CopyPropertiesToNew<SendInviteOptionsInternal>(options);
		OnSendInviteCallbackInternal onSendInviteCallbackInternal = OnSendInvite;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onSendInviteCallbackInternal);
		EOS_Lobby_SendInvite(base.InnerHandle, ref options2, clientDataAddress, onSendInviteCallbackInternal);
		options2.Dispose();
	}

	public void RejectInvite(RejectInviteOptions options, object clientData, OnRejectInviteCallback completionDelegate)
	{
		RejectInviteOptionsInternal options2 = Helper.CopyPropertiesToNew<RejectInviteOptionsInternal>(options);
		OnRejectInviteCallbackInternal onRejectInviteCallbackInternal = OnRejectInvite;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onRejectInviteCallbackInternal);
		EOS_Lobby_RejectInvite(base.InnerHandle, ref options2, clientDataAddress, onRejectInviteCallbackInternal);
		options2.Dispose();
	}

	public Result CreateLobbySearch(CreateLobbySearchOptions options, out LobbySearch outLobbySearchHandle)
	{
		CreateLobbySearchOptionsInternal options2 = Helper.CopyPropertiesToNew<CreateLobbySearchOptionsInternal>(options);
		outLobbySearchHandle = Helper.GetDefault<LobbySearch>();
		IntPtr outLobbySearchHandle2 = IntPtr.Zero;
		Result result = EOS_Lobby_CreateLobbySearch(base.InnerHandle, ref options2, ref outLobbySearchHandle2);
		options2.Dispose();
		outLobbySearchHandle = Helper.GetHandle<LobbySearch>(outLobbySearchHandle2);
		return result;
	}

	public ulong AddNotifyLobbyInviteReceived(AddNotifyLobbyInviteReceivedOptions options, object clientData, OnLobbyInviteReceivedCallback notificationFn)
	{
		AddNotifyLobbyInviteReceivedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyLobbyInviteReceivedOptionsInternal>(options);
		OnLobbyInviteReceivedCallbackInternal onLobbyInviteReceivedCallbackInternal = OnLobbyInviteReceived;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onLobbyInviteReceivedCallbackInternal);
		ulong result = EOS_Lobby_AddNotifyLobbyInviteReceived(base.InnerHandle, ref options2, clientDataAddress, onLobbyInviteReceivedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLobbyInviteReceived(ulong inId)
	{
		EOS_Lobby_RemoveNotifyLobbyInviteReceived(base.InnerHandle, inId);
	}

	public Result CopyLobbyDetailsHandleByInviteId(CopyLobbyDetailsHandleByInviteIdOptions options, out LobbyDetails outLobbyDetailsHandle)
	{
		CopyLobbyDetailsHandleByInviteIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLobbyDetailsHandleByInviteIdOptionsInternal>(options);
		outLobbyDetailsHandle = Helper.GetDefault<LobbyDetails>();
		IntPtr outLobbyDetailsHandle2 = IntPtr.Zero;
		Result result = EOS_Lobby_CopyLobbyDetailsHandleByInviteId(base.InnerHandle, ref options2, ref outLobbyDetailsHandle2);
		options2.Dispose();
		outLobbyDetailsHandle = Helper.GetHandle<LobbyDetails>(outLobbyDetailsHandle2);
		return result;
	}

	public Result CopyLobbyDetailsHandle(CopyLobbyDetailsHandleOptions options, out LobbyDetails outLobbyDetailsHandle)
	{
		CopyLobbyDetailsHandleOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLobbyDetailsHandleOptionsInternal>(options);
		outLobbyDetailsHandle = Helper.GetDefault<LobbyDetails>();
		IntPtr outLobbyDetailsHandle2 = IntPtr.Zero;
		Result result = EOS_Lobby_CopyLobbyDetailsHandle(base.InnerHandle, ref options2, ref outLobbyDetailsHandle2);
		options2.Dispose();
		outLobbyDetailsHandle = Helper.GetHandle<LobbyDetails>(outLobbyDetailsHandle2);
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnLobbyInviteReceived(IntPtr address)
	{
		OnLobbyInviteReceivedCallback callDelegate = null;
		LobbyInviteReceivedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLobbyInviteReceivedCallback, LobbyInviteReceivedCallbackInfoInternal, LobbyInviteReceivedCallbackInfo>(address, out callDelegate, out callbackInfo))
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
	internal static void OnLobbyMemberStatusReceived(IntPtr address)
	{
		OnLobbyMemberStatusReceivedCallback callDelegate = null;
		LobbyMemberStatusReceivedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLobbyMemberStatusReceivedCallback, LobbyMemberStatusReceivedCallbackInfoInternal, LobbyMemberStatusReceivedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnLobbyMemberUpdateReceived(IntPtr address)
	{
		OnLobbyMemberUpdateReceivedCallback callDelegate = null;
		LobbyMemberUpdateReceivedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLobbyMemberUpdateReceivedCallback, LobbyMemberUpdateReceivedCallbackInfoInternal, LobbyMemberUpdateReceivedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnLobbyUpdateReceived(IntPtr address)
	{
		OnLobbyUpdateReceivedCallback callDelegate = null;
		LobbyUpdateReceivedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLobbyUpdateReceivedCallback, LobbyUpdateReceivedCallbackInfoInternal, LobbyUpdateReceivedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnKickMember(IntPtr address)
	{
		OnKickMemberCallback callDelegate = null;
		KickMemberCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnKickMemberCallback, KickMemberCallbackInfoInternal, KickMemberCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnPromoteMember(IntPtr address)
	{
		OnPromoteMemberCallback callDelegate = null;
		PromoteMemberCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnPromoteMemberCallback, PromoteMemberCallbackInfoInternal, PromoteMemberCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnUpdateLobby(IntPtr address)
	{
		OnUpdateLobbyCallback callDelegate = null;
		UpdateLobbyCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnUpdateLobbyCallback, UpdateLobbyCallbackInfoInternal, UpdateLobbyCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnLeaveLobby(IntPtr address)
	{
		OnLeaveLobbyCallback callDelegate = null;
		LeaveLobbyCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLeaveLobbyCallback, LeaveLobbyCallbackInfoInternal, LeaveLobbyCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnJoinLobby(IntPtr address)
	{
		OnJoinLobbyCallback callDelegate = null;
		JoinLobbyCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnJoinLobbyCallback, JoinLobbyCallbackInfoInternal, JoinLobbyCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnDestroyLobby(IntPtr address)
	{
		OnDestroyLobbyCallback callDelegate = null;
		DestroyLobbyCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnDestroyLobbyCallback, DestroyLobbyCallbackInfoInternal, DestroyLobbyCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnCreateLobby(IntPtr address)
	{
		OnCreateLobbyCallback callDelegate = null;
		CreateLobbyCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnCreateLobbyCallback, CreateLobbyCallbackInfoInternal, CreateLobbyCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_Attribute_Release(IntPtr lobbyAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Lobby_CopyLobbyDetailsHandle(IntPtr handle, ref CopyLobbyDetailsHandleOptionsInternal options, ref IntPtr outLobbyDetailsHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Lobby_CopyLobbyDetailsHandleByInviteId(IntPtr handle, ref CopyLobbyDetailsHandleByInviteIdOptionsInternal options, ref IntPtr outLobbyDetailsHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_RemoveNotifyLobbyInviteReceived(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Lobby_AddNotifyLobbyInviteReceived(IntPtr handle, ref AddNotifyLobbyInviteReceivedOptionsInternal options, IntPtr clientData, OnLobbyInviteReceivedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Lobby_CreateLobbySearch(IntPtr handle, ref CreateLobbySearchOptionsInternal options, ref IntPtr outLobbySearchHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_RejectInvite(IntPtr handle, ref RejectInviteOptionsInternal options, IntPtr clientData, OnRejectInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_SendInvite(IntPtr handle, ref SendInviteOptionsInternal options, IntPtr clientData, OnSendInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_RemoveNotifyLobbyMemberStatusReceived(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Lobby_AddNotifyLobbyMemberStatusReceived(IntPtr handle, ref AddNotifyLobbyMemberStatusReceivedOptionsInternal options, IntPtr clientData, OnLobbyMemberStatusReceivedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_RemoveNotifyLobbyMemberUpdateReceived(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Lobby_AddNotifyLobbyMemberUpdateReceived(IntPtr handle, ref AddNotifyLobbyMemberUpdateReceivedOptionsInternal options, IntPtr clientData, OnLobbyMemberUpdateReceivedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_RemoveNotifyLobbyUpdateReceived(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Lobby_AddNotifyLobbyUpdateReceived(IntPtr handle, ref AddNotifyLobbyUpdateReceivedOptionsInternal options, IntPtr clientData, OnLobbyUpdateReceivedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_KickMember(IntPtr handle, ref KickMemberOptionsInternal options, IntPtr clientData, OnKickMemberCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_PromoteMember(IntPtr handle, ref PromoteMemberOptionsInternal options, IntPtr clientData, OnPromoteMemberCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_UpdateLobby(IntPtr handle, ref UpdateLobbyOptionsInternal options, IntPtr clientData, OnUpdateLobbyCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Lobby_UpdateLobbyModification(IntPtr handle, ref UpdateLobbyModificationOptionsInternal options, ref IntPtr outLobbyModificationHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_LeaveLobby(IntPtr handle, ref LeaveLobbyOptionsInternal options, IntPtr clientData, OnLeaveLobbyCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_JoinLobby(IntPtr handle, ref JoinLobbyOptionsInternal options, IntPtr clientData, OnJoinLobbyCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_DestroyLobby(IntPtr handle, ref DestroyLobbyOptionsInternal options, IntPtr clientData, OnDestroyLobbyCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_CreateLobby(IntPtr handle, ref CreateLobbyOptionsInternal options, IntPtr clientData, OnCreateLobbyCallbackInternal completionDelegate);
}
