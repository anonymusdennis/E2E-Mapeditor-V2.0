using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionsInterface : Handle
{
	public SessionsInterface()
		: base(IntPtr.Zero)
	{
	}

	public SessionsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result CreateSessionModification(CreateSessionModificationOptions options, out SessionModification outSessionModificationHandle)
	{
		CreateSessionModificationOptionsInternal options2 = Helper.CopyPropertiesToNew<CreateSessionModificationOptionsInternal>(options);
		outSessionModificationHandle = Helper.GetDefault<SessionModification>();
		IntPtr outSessionModificationHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CreateSessionModification(base.InnerHandle, ref options2, ref outSessionModificationHandle2);
		options2.Dispose();
		outSessionModificationHandle = Helper.GetHandle<SessionModification>(outSessionModificationHandle2);
		return result;
	}

	public Result UpdateSessionModification(UpdateSessionModificationOptions options, out SessionModification outSessionModificationHandle)
	{
		UpdateSessionModificationOptionsInternal options2 = Helper.CopyPropertiesToNew<UpdateSessionModificationOptionsInternal>(options);
		outSessionModificationHandle = Helper.GetDefault<SessionModification>();
		IntPtr outSessionModificationHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_UpdateSessionModification(base.InnerHandle, ref options2, ref outSessionModificationHandle2);
		options2.Dispose();
		outSessionModificationHandle = Helper.GetHandle<SessionModification>(outSessionModificationHandle2);
		return result;
	}

	public void UpdateSession(UpdateSessionOptions options, object clientData, OnUpdateSessionCallback completionDelegate)
	{
		UpdateSessionOptionsInternal options2 = Helper.CopyPropertiesToNew<UpdateSessionOptionsInternal>(options);
		OnUpdateSessionCallbackInternal onUpdateSessionCallbackInternal = OnUpdateSession;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onUpdateSessionCallbackInternal);
		EOS_Sessions_UpdateSession(base.InnerHandle, ref options2, clientDataAddress, onUpdateSessionCallbackInternal);
		options2.Dispose();
	}

	public void DestroySession(DestroySessionOptions options, object clientData, OnDestroySessionCallback completionDelegate)
	{
		DestroySessionOptionsInternal options2 = Helper.CopyPropertiesToNew<DestroySessionOptionsInternal>(options);
		OnDestroySessionCallbackInternal onDestroySessionCallbackInternal = OnDestroySession;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onDestroySessionCallbackInternal);
		EOS_Sessions_DestroySession(base.InnerHandle, ref options2, clientDataAddress, onDestroySessionCallbackInternal);
		options2.Dispose();
	}

	public void JoinSession(JoinSessionOptions options, object clientData, OnJoinSessionCallback completionDelegate)
	{
		JoinSessionOptionsInternal options2 = Helper.CopyPropertiesToNew<JoinSessionOptionsInternal>(options);
		OnJoinSessionCallbackInternal onJoinSessionCallbackInternal = OnJoinSession;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onJoinSessionCallbackInternal);
		EOS_Sessions_JoinSession(base.InnerHandle, ref options2, clientDataAddress, onJoinSessionCallbackInternal);
		options2.Dispose();
	}

	public void StartSession(StartSessionOptions options, object clientData, OnStartSessionCallback completionDelegate)
	{
		StartSessionOptionsInternal options2 = Helper.CopyPropertiesToNew<StartSessionOptionsInternal>(options);
		OnStartSessionCallbackInternal onStartSessionCallbackInternal = OnStartSession;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onStartSessionCallbackInternal);
		EOS_Sessions_StartSession(base.InnerHandle, ref options2, clientDataAddress, onStartSessionCallbackInternal);
		options2.Dispose();
	}

	public void EndSession(EndSessionOptions options, object clientData, OnEndSessionCallback completionDelegate)
	{
		EndSessionOptionsInternal options2 = Helper.CopyPropertiesToNew<EndSessionOptionsInternal>(options);
		OnEndSessionCallbackInternal onEndSessionCallbackInternal = OnEndSession;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onEndSessionCallbackInternal);
		EOS_Sessions_EndSession(base.InnerHandle, ref options2, clientDataAddress, onEndSessionCallbackInternal);
		options2.Dispose();
	}

	public void RegisterPlayers(RegisterPlayersOptions options, object clientData, OnRegisterPlayersCallback completionDelegate)
	{
		RegisterPlayersOptionsInternal options2 = Helper.CopyPropertiesToNew<RegisterPlayersOptionsInternal>(options);
		OnRegisterPlayersCallbackInternal onRegisterPlayersCallbackInternal = OnRegisterPlayers;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onRegisterPlayersCallbackInternal);
		EOS_Sessions_RegisterPlayers(base.InnerHandle, ref options2, clientDataAddress, onRegisterPlayersCallbackInternal);
		options2.Dispose();
	}

	public void UnregisterPlayers(UnregisterPlayersOptions options, object clientData, OnUnregisterPlayersCallback completionDelegate)
	{
		UnregisterPlayersOptionsInternal options2 = Helper.CopyPropertiesToNew<UnregisterPlayersOptionsInternal>(options);
		OnUnregisterPlayersCallbackInternal onUnregisterPlayersCallbackInternal = OnUnregisterPlayers;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onUnregisterPlayersCallbackInternal);
		EOS_Sessions_UnregisterPlayers(base.InnerHandle, ref options2, clientDataAddress, onUnregisterPlayersCallbackInternal);
		options2.Dispose();
	}

	public void SendInvite(SendInviteOptions options, object clientData, OnSendInviteCallback completionDelegate)
	{
		SendInviteOptionsInternal options2 = Helper.CopyPropertiesToNew<SendInviteOptionsInternal>(options);
		OnSendInviteCallbackInternal onSendInviteCallbackInternal = OnSendInvite;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onSendInviteCallbackInternal);
		EOS_Sessions_SendInvite(base.InnerHandle, ref options2, clientDataAddress, onSendInviteCallbackInternal);
		options2.Dispose();
	}

	public void RejectInvite(RejectInviteOptions options, object clientData, OnRejectInviteCallback completionDelegate)
	{
		RejectInviteOptionsInternal options2 = Helper.CopyPropertiesToNew<RejectInviteOptionsInternal>(options);
		OnRejectInviteCallbackInternal onRejectInviteCallbackInternal = OnRejectInvite;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onRejectInviteCallbackInternal);
		EOS_Sessions_RejectInvite(base.InnerHandle, ref options2, clientDataAddress, onRejectInviteCallbackInternal);
		options2.Dispose();
	}

	public Result CreateSessionSearch(CreateSessionSearchOptions options, out SessionSearch outSessionSearchHandle)
	{
		CreateSessionSearchOptionsInternal options2 = Helper.CopyPropertiesToNew<CreateSessionSearchOptionsInternal>(options);
		outSessionSearchHandle = Helper.GetDefault<SessionSearch>();
		IntPtr outSessionSearchHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CreateSessionSearch(base.InnerHandle, ref options2, ref outSessionSearchHandle2);
		options2.Dispose();
		outSessionSearchHandle = Helper.GetHandle<SessionSearch>(outSessionSearchHandle2);
		return result;
	}

	public Result CopyActiveSessionHandle(CopyActiveSessionHandleOptions options, out ActiveSession outSessionHandle)
	{
		CopyActiveSessionHandleOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyActiveSessionHandleOptionsInternal>(options);
		outSessionHandle = Helper.GetDefault<ActiveSession>();
		IntPtr outSessionHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CopyActiveSessionHandle(base.InnerHandle, ref options2, ref outSessionHandle2);
		options2.Dispose();
		outSessionHandle = Helper.GetHandle<ActiveSession>(outSessionHandle2);
		return result;
	}

	public ulong AddNotifySessionInviteReceived(AddNotifySessionInviteReceivedOptions options, object clientData, OnSessionInviteReceivedCallback notificationFn)
	{
		AddNotifySessionInviteReceivedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifySessionInviteReceivedOptionsInternal>(options);
		OnSessionInviteReceivedCallbackInternal onSessionInviteReceivedCallbackInternal = OnSessionInviteReceived;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onSessionInviteReceivedCallbackInternal);
		ulong result = EOS_Sessions_AddNotifySessionInviteReceived(base.InnerHandle, ref options2, clientDataAddress, onSessionInviteReceivedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifySessionInviteReceived(ulong inId)
	{
		EOS_Sessions_RemoveNotifySessionInviteReceived(base.InnerHandle, inId);
	}

	public ulong AddNotifySessionInviteAccepted(AddNotifySessionInviteAcceptedOptions options, object clientData, OnSessionInviteAcceptedCallback notificationFn)
	{
		AddNotifySessionInviteAcceptedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifySessionInviteAcceptedOptionsInternal>(options);
		OnSessionInviteAcceptedCallbackInternal onSessionInviteAcceptedCallbackInternal = OnSessionInviteAccepted;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onSessionInviteAcceptedCallbackInternal);
		ulong result = EOS_Sessions_AddNotifySessionInviteAccepted(base.InnerHandle, ref options2, clientDataAddress, onSessionInviteAcceptedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifySessionInviteAccepted(ulong inId)
	{
		EOS_Sessions_RemoveNotifySessionInviteAccepted(base.InnerHandle, inId);
	}

	public Result CopySessionHandleByInviteId(CopySessionHandleByInviteIdOptions options, out SessionDetails outSessionHandle)
	{
		CopySessionHandleByInviteIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopySessionHandleByInviteIdOptionsInternal>(options);
		outSessionHandle = Helper.GetDefault<SessionDetails>();
		IntPtr outSessionHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CopySessionHandleByInviteId(base.InnerHandle, ref options2, ref outSessionHandle2);
		options2.Dispose();
		outSessionHandle = Helper.GetHandle<SessionDetails>(outSessionHandle2);
		return result;
	}

	public Result CopySessionHandleForPresence(CopySessionHandleForPresenceOptions options, out SessionDetails outSessionHandle)
	{
		CopySessionHandleForPresenceOptionsInternal options2 = Helper.CopyPropertiesToNew<CopySessionHandleForPresenceOptionsInternal>(options);
		outSessionHandle = Helper.GetDefault<SessionDetails>();
		IntPtr outSessionHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CopySessionHandleForPresence(base.InnerHandle, ref options2, ref outSessionHandle2);
		options2.Dispose();
		outSessionHandle = Helper.GetHandle<SessionDetails>(outSessionHandle2);
		return result;
	}

	public Result IsUserInSession(IsUserInSessionOptions options)
	{
		IsUserInSessionOptionsInternal options2 = Helper.CopyPropertiesToNew<IsUserInSessionOptionsInternal>(options);
		Result result = EOS_Sessions_IsUserInSession(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result DumpSessionState(DumpSessionStateOptions options)
	{
		DumpSessionStateOptionsInternal options2 = Helper.CopyPropertiesToNew<DumpSessionStateOptionsInternal>(options);
		Result result = EOS_Sessions_DumpSessionState(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnSessionInviteAccepted(IntPtr address)
	{
		OnSessionInviteAcceptedCallback callDelegate = null;
		SessionInviteAcceptedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnSessionInviteAcceptedCallback, SessionInviteAcceptedCallbackInfoInternal, SessionInviteAcceptedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnSessionInviteReceived(IntPtr address)
	{
		OnSessionInviteReceivedCallback callDelegate = null;
		SessionInviteReceivedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnSessionInviteReceivedCallback, SessionInviteReceivedCallbackInfoInternal, SessionInviteReceivedCallbackInfo>(address, out callDelegate, out callbackInfo))
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
	internal static void OnUnregisterPlayers(IntPtr address)
	{
		OnUnregisterPlayersCallback callDelegate = null;
		UnregisterPlayersCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnUnregisterPlayersCallback, UnregisterPlayersCallbackInfoInternal, UnregisterPlayersCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnRegisterPlayers(IntPtr address)
	{
		OnRegisterPlayersCallback callDelegate = null;
		RegisterPlayersCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnRegisterPlayersCallback, RegisterPlayersCallbackInfoInternal, RegisterPlayersCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnEndSession(IntPtr address)
	{
		OnEndSessionCallback callDelegate = null;
		EndSessionCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnEndSessionCallback, EndSessionCallbackInfoInternal, EndSessionCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnStartSession(IntPtr address)
	{
		OnStartSessionCallback callDelegate = null;
		StartSessionCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnStartSessionCallback, StartSessionCallbackInfoInternal, StartSessionCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnJoinSession(IntPtr address)
	{
		OnJoinSessionCallback callDelegate = null;
		JoinSessionCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnJoinSessionCallback, JoinSessionCallbackInfoInternal, JoinSessionCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnDestroySession(IntPtr address)
	{
		OnDestroySessionCallback callDelegate = null;
		DestroySessionCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnDestroySessionCallback, DestroySessionCallbackInfoInternal, DestroySessionCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnUpdateSession(IntPtr address)
	{
		OnUpdateSessionCallback callDelegate = null;
		UpdateSessionCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnUpdateSessionCallback, UpdateSessionCallbackInfoInternal, UpdateSessionCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_DumpSessionState(IntPtr handle, ref DumpSessionStateOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_IsUserInSession(IntPtr handle, ref IsUserInSessionOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_CopySessionHandleForPresence(IntPtr handle, ref CopySessionHandleForPresenceOptionsInternal options, ref IntPtr outSessionHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_CopySessionHandleByInviteId(IntPtr handle, ref CopySessionHandleByInviteIdOptionsInternal options, ref IntPtr outSessionHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_RemoveNotifySessionInviteAccepted(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Sessions_AddNotifySessionInviteAccepted(IntPtr handle, ref AddNotifySessionInviteAcceptedOptionsInternal options, IntPtr clientData, OnSessionInviteAcceptedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_RemoveNotifySessionInviteReceived(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Sessions_AddNotifySessionInviteReceived(IntPtr handle, ref AddNotifySessionInviteReceivedOptionsInternal options, IntPtr clientData, OnSessionInviteReceivedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_CopyActiveSessionHandle(IntPtr handle, ref CopyActiveSessionHandleOptionsInternal options, ref IntPtr outSessionHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_CreateSessionSearch(IntPtr handle, ref CreateSessionSearchOptionsInternal options, ref IntPtr outSessionSearchHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_RejectInvite(IntPtr handle, ref RejectInviteOptionsInternal options, IntPtr clientData, OnRejectInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_SendInvite(IntPtr handle, ref SendInviteOptionsInternal options, IntPtr clientData, OnSendInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_UnregisterPlayers(IntPtr handle, ref UnregisterPlayersOptionsInternal options, IntPtr clientData, OnUnregisterPlayersCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_RegisterPlayers(IntPtr handle, ref RegisterPlayersOptionsInternal options, IntPtr clientData, OnRegisterPlayersCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_EndSession(IntPtr handle, ref EndSessionOptionsInternal options, IntPtr clientData, OnEndSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_StartSession(IntPtr handle, ref StartSessionOptionsInternal options, IntPtr clientData, OnStartSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_JoinSession(IntPtr handle, ref JoinSessionOptionsInternal options, IntPtr clientData, OnJoinSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_DestroySession(IntPtr handle, ref DestroySessionOptionsInternal options, IntPtr clientData, OnDestroySessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Sessions_UpdateSession(IntPtr handle, ref UpdateSessionOptionsInternal options, IntPtr clientData, OnUpdateSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_UpdateSessionModification(IntPtr handle, ref UpdateSessionModificationOptionsInternal options, ref IntPtr outSessionModificationHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Sessions_CreateSessionModification(IntPtr handle, ref CreateSessionModificationOptionsInternal options, ref IntPtr outSessionModificationHandle);
}
