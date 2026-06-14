using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices.Presence;

public sealed class PresenceInterface : Handle
{
	public PresenceInterface()
		: base(IntPtr.Zero)
	{
	}

	public PresenceInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryPresence(QueryPresenceOptions options, object clientData, OnQueryPresenceCompleteCallback completionDelegate)
	{
		QueryPresenceOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryPresenceOptionsInternal>(options);
		OnQueryPresenceCompleteCallbackInternal onQueryPresenceCompleteCallbackInternal = OnQueryPresenceComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryPresenceCompleteCallbackInternal);
		EOS_Presence_QueryPresence(base.InnerHandle, ref options2, clientDataAddress, onQueryPresenceCompleteCallbackInternal);
		options2.Dispose();
	}

	public int HasPresence(HasPresenceOptions options)
	{
		HasPresenceOptionsInternal options2 = Helper.CopyPropertiesToNew<HasPresenceOptionsInternal>(options);
		int result = EOS_Presence_HasPresence(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyPresence(CopyPresenceOptions options, out Info outPresence)
	{
		CopyPresenceOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyPresenceOptionsInternal>(options);
		outPresence = Helper.GetDefault<Info>();
		IntPtr outPresence2 = IntPtr.Zero;
		Result result = EOS_Presence_CopyPresence(base.InnerHandle, ref options2, ref outPresence2);
		options2.Dispose();
		if (Helper.TryMarshal<InfoInternal, Info>(outPresence2, out outPresence))
		{
			EOS_Presence_Info_Release(outPresence2);
		}
		return result;
	}

	public Result CreatePresenceModification(CreatePresenceModificationOptions options, out PresenceModification outPresenceModificationHandle)
	{
		CreatePresenceModificationOptionsInternal options2 = Helper.CopyPropertiesToNew<CreatePresenceModificationOptionsInternal>(options);
		outPresenceModificationHandle = Helper.GetDefault<PresenceModification>();
		IntPtr outPresenceModificationHandle2 = IntPtr.Zero;
		Result result = EOS_Presence_CreatePresenceModification(base.InnerHandle, ref options2, ref outPresenceModificationHandle2);
		options2.Dispose();
		outPresenceModificationHandle = Helper.GetHandle<PresenceModification>(outPresenceModificationHandle2);
		return result;
	}

	public void SetPresence(SetPresenceOptions options, object clientData, SetPresenceCompleteCallback completionDelegate)
	{
		SetPresenceOptionsInternal options2 = Helper.CopyPropertiesToNew<SetPresenceOptionsInternal>(options);
		SetPresenceCompleteCallbackInternal setPresenceCompleteCallbackInternal = SetPresenceComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, setPresenceCompleteCallbackInternal);
		EOS_Presence_SetPresence(base.InnerHandle, ref options2, clientDataAddress, setPresenceCompleteCallbackInternal);
		options2.Dispose();
	}

	public ulong AddNotifyOnPresenceChanged(AddNotifyOnPresenceChangedOptions options, object clientData, OnPresenceChangedCallback notificationHandler)
	{
		AddNotifyOnPresenceChangedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyOnPresenceChangedOptionsInternal>(options);
		OnPresenceChangedCallbackInternal onPresenceChangedCallbackInternal = OnPresenceChanged;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationHandler, onPresenceChangedCallbackInternal);
		ulong result = EOS_Presence_AddNotifyOnPresenceChanged(base.InnerHandle, ref options2, clientDataAddress, onPresenceChangedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyOnPresenceChanged(ulong notificationId)
	{
		EOS_Presence_RemoveNotifyOnPresenceChanged(base.InnerHandle, notificationId);
	}

	public ulong AddNotifyJoinGameAccepted(AddNotifyJoinGameAcceptedOptions options, object clientData, OnJoinGameAcceptedCallback notificationFn)
	{
		AddNotifyJoinGameAcceptedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyJoinGameAcceptedOptionsInternal>(options);
		OnJoinGameAcceptedCallbackInternal onJoinGameAcceptedCallbackInternal = OnJoinGameAccepted;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onJoinGameAcceptedCallbackInternal);
		ulong result = EOS_Presence_AddNotifyJoinGameAccepted(base.InnerHandle, ref options2, clientDataAddress, onJoinGameAcceptedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyJoinGameAccepted(ulong inId)
	{
		EOS_Presence_RemoveNotifyJoinGameAccepted(base.InnerHandle, inId);
	}

	public Result GetJoinInfo(GetJoinInfoOptions options, StringBuilder outBuffer, ref int inOutBufferLength)
	{
		GetJoinInfoOptionsInternal options2 = Helper.CopyPropertiesToNew<GetJoinInfoOptionsInternal>(options);
		Result result = EOS_Presence_GetJoinInfo(base.InnerHandle, ref options2, outBuffer, ref inOutBufferLength);
		options2.Dispose();
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnJoinGameAccepted(IntPtr address)
	{
		OnJoinGameAcceptedCallback callDelegate = null;
		JoinGameAcceptedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnJoinGameAcceptedCallback, JoinGameAcceptedCallbackInfoInternal, JoinGameAcceptedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnPresenceChanged(IntPtr address)
	{
		OnPresenceChangedCallback callDelegate = null;
		PresenceChangedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnPresenceChangedCallback, PresenceChangedCallbackInfoInternal, PresenceChangedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void SetPresenceComplete(IntPtr address)
	{
		SetPresenceCompleteCallback callDelegate = null;
		SetPresenceCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<SetPresenceCompleteCallback, SetPresenceCallbackInfoInternal, SetPresenceCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryPresenceComplete(IntPtr address)
	{
		OnQueryPresenceCompleteCallback callDelegate = null;
		QueryPresenceCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryPresenceCompleteCallback, QueryPresenceCallbackInfoInternal, QueryPresenceCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Presence_Info_Release(IntPtr presenceInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Presence_GetJoinInfo(IntPtr handle, ref GetJoinInfoOptionsInternal options, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Presence_RemoveNotifyJoinGameAccepted(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Presence_AddNotifyJoinGameAccepted(IntPtr handle, ref AddNotifyJoinGameAcceptedOptionsInternal options, IntPtr clientData, OnJoinGameAcceptedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Presence_RemoveNotifyOnPresenceChanged(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Presence_AddNotifyOnPresenceChanged(IntPtr handle, ref AddNotifyOnPresenceChangedOptionsInternal options, IntPtr clientData, OnPresenceChangedCallbackInternal notificationHandler);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Presence_SetPresence(IntPtr handle, ref SetPresenceOptionsInternal options, IntPtr clientData, SetPresenceCompleteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Presence_CreatePresenceModification(IntPtr handle, ref CreatePresenceModificationOptionsInternal options, ref IntPtr outPresenceModificationHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Presence_CopyPresence(IntPtr handle, ref CopyPresenceOptionsInternal options, ref IntPtr outPresence);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern int EOS_Presence_HasPresence(IntPtr handle, ref HasPresenceOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Presence_QueryPresence(IntPtr handle, ref QueryPresenceOptionsInternal options, IntPtr clientData, OnQueryPresenceCompleteCallbackInternal completionDelegate);
}
