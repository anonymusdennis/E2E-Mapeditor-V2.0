using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices.Connect;

public sealed class ConnectInterface : Handle
{
	public ConnectInterface()
		: base(IntPtr.Zero)
	{
	}

	public ConnectInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void Login(LoginOptions options, object clientData, OnLoginCallback completionDelegate)
	{
		LoginOptionsInternal options2 = Helper.CopyPropertiesToNew<LoginOptionsInternal>(options);
		OnLoginCallbackInternal onLoginCallbackInternal = OnLogin;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onLoginCallbackInternal);
		EOS_Connect_Login(base.InnerHandle, ref options2, clientDataAddress, onLoginCallbackInternal);
		options2.Dispose();
	}

	public void CreateUser(CreateUserOptions options, object clientData, OnCreateUserCallback completionDelegate)
	{
		CreateUserOptionsInternal options2 = Helper.CopyPropertiesToNew<CreateUserOptionsInternal>(options);
		OnCreateUserCallbackInternal onCreateUserCallbackInternal = OnCreateUser;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onCreateUserCallbackInternal);
		EOS_Connect_CreateUser(base.InnerHandle, ref options2, clientDataAddress, onCreateUserCallbackInternal);
		options2.Dispose();
	}

	public void LinkAccount(LinkAccountOptions options, object clientData, OnLinkAccountCallback completionDelegate)
	{
		LinkAccountOptionsInternal options2 = Helper.CopyPropertiesToNew<LinkAccountOptionsInternal>(options);
		OnLinkAccountCallbackInternal onLinkAccountCallbackInternal = OnLinkAccount;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onLinkAccountCallbackInternal);
		EOS_Connect_LinkAccount(base.InnerHandle, ref options2, clientDataAddress, onLinkAccountCallbackInternal);
		options2.Dispose();
	}

	public void QueryExternalAccountMappings(QueryExternalAccountMappingsOptions options, object clientData, OnQueryExternalAccountMappingsCallback completionDelegate)
	{
		QueryExternalAccountMappingsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryExternalAccountMappingsOptionsInternal>(options);
		OnQueryExternalAccountMappingsCallbackInternal onQueryExternalAccountMappingsCallbackInternal = OnQueryExternalAccountMappings;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryExternalAccountMappingsCallbackInternal);
		EOS_Connect_QueryExternalAccountMappings(base.InnerHandle, ref options2, clientDataAddress, onQueryExternalAccountMappingsCallbackInternal);
		options2.Dispose();
	}

	public void QueryProductUserIdMappings(QueryProductUserIdMappingsOptions options, object clientData, OnQueryProductUserIdMappingsCallback completionDelegate)
	{
		QueryProductUserIdMappingsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryProductUserIdMappingsOptionsInternal>(options);
		OnQueryProductUserIdMappingsCallbackInternal onQueryProductUserIdMappingsCallbackInternal = OnQueryProductUserIdMappings;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryProductUserIdMappingsCallbackInternal);
		EOS_Connect_QueryProductUserIdMappings(base.InnerHandle, ref options2, clientDataAddress, onQueryProductUserIdMappingsCallbackInternal);
		options2.Dispose();
	}

	public ProductUserId GetExternalAccountMapping(GetExternalAccountMappingsOptions options)
	{
		GetExternalAccountMappingsOptionsInternal options2 = Helper.CopyPropertiesToNew<GetExternalAccountMappingsOptionsInternal>(options);
		IntPtr innerHandle = EOS_Connect_GetExternalAccountMapping(base.InnerHandle, ref options2);
		options2.Dispose();
		return Helper.GetHandle<ProductUserId>(innerHandle);
	}

	public Result GetProductUserIdMapping(GetProductUserIdMappingOptions options, StringBuilder outBuffer, ref int inOutBufferLength)
	{
		GetProductUserIdMappingOptionsInternal options2 = Helper.CopyPropertiesToNew<GetProductUserIdMappingOptionsInternal>(options);
		Result result = EOS_Connect_GetProductUserIdMapping(base.InnerHandle, ref options2, outBuffer, ref inOutBufferLength);
		options2.Dispose();
		return result;
	}

	public int GetLoggedInUsersCount()
	{
		return EOS_Connect_GetLoggedInUsersCount(base.InnerHandle);
	}

	public ProductUserId GetLoggedInUserByIndex(int index)
	{
		IntPtr innerHandle = EOS_Connect_GetLoggedInUserByIndex(base.InnerHandle, index);
		return Helper.GetHandle<ProductUserId>(innerHandle);
	}

	public LoginStatus GetLoginStatus(ProductUserId localUserId)
	{
		return EOS_Connect_GetLoginStatus(base.InnerHandle, localUserId.InnerHandle);
	}

	public ulong AddNotifyAuthExpiration(AddNotifyAuthExpirationOptions options, object clientData, OnAuthExpirationCallback notification)
	{
		AddNotifyAuthExpirationOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyAuthExpirationOptionsInternal>(options);
		OnAuthExpirationCallbackInternal onAuthExpirationCallbackInternal = OnAuthExpiration;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notification, onAuthExpirationCallbackInternal);
		ulong result = EOS_Connect_AddNotifyAuthExpiration(base.InnerHandle, ref options2, clientDataAddress, onAuthExpirationCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyAuthExpiration(ulong inId)
	{
		EOS_Connect_RemoveNotifyAuthExpiration(base.InnerHandle, inId);
	}

	public ulong AddNotifyLoginStatusChanged(AddNotifyLoginStatusChangedOptions options, object clientData, OnLoginStatusChangedCallback notification)
	{
		AddNotifyLoginStatusChangedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyLoginStatusChangedOptionsInternal>(options);
		OnLoginStatusChangedCallbackInternal onLoginStatusChangedCallbackInternal = OnLoginStatusChanged;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notification, onLoginStatusChangedCallbackInternal);
		ulong result = EOS_Connect_AddNotifyLoginStatusChanged(base.InnerHandle, ref options2, clientDataAddress, onLoginStatusChangedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLoginStatusChanged(ulong inId)
	{
		EOS_Connect_RemoveNotifyLoginStatusChanged(base.InnerHandle, inId);
	}

	[MonoPInvokeCallback]
	internal static void OnLoginStatusChanged(IntPtr address)
	{
		OnLoginStatusChangedCallback callDelegate = null;
		LoginStatusChangedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLoginStatusChangedCallback, LoginStatusChangedCallbackInfoInternal, LoginStatusChangedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnAuthExpiration(IntPtr address)
	{
		OnAuthExpirationCallback callDelegate = null;
		AuthExpirationCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnAuthExpirationCallback, AuthExpirationCallbackInfoInternal, AuthExpirationCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryProductUserIdMappings(IntPtr address)
	{
		OnQueryProductUserIdMappingsCallback callDelegate = null;
		QueryProductUserIdMappingsCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryProductUserIdMappingsCallback, QueryProductUserIdMappingsCallbackInfoInternal, QueryProductUserIdMappingsCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryExternalAccountMappings(IntPtr address)
	{
		OnQueryExternalAccountMappingsCallback callDelegate = null;
		QueryExternalAccountMappingsCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryExternalAccountMappingsCallback, QueryExternalAccountMappingsCallbackInfoInternal, QueryExternalAccountMappingsCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnLinkAccount(IntPtr address)
	{
		OnLinkAccountCallback callDelegate = null;
		LinkAccountCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLinkAccountCallback, LinkAccountCallbackInfoInternal, LinkAccountCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnCreateUser(IntPtr address)
	{
		OnCreateUserCallback callDelegate = null;
		CreateUserCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnCreateUserCallback, CreateUserCallbackInfoInternal, CreateUserCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnLogin(IntPtr address)
	{
		OnLoginCallback callDelegate = null;
		LoginCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLoginCallback, LoginCallbackInfoInternal, LoginCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Connect_RemoveNotifyLoginStatusChanged(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Connect_AddNotifyLoginStatusChanged(IntPtr handle, ref AddNotifyLoginStatusChangedOptionsInternal options, IntPtr clientData, OnLoginStatusChangedCallbackInternal notification);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Connect_RemoveNotifyAuthExpiration(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Connect_AddNotifyAuthExpiration(IntPtr handle, ref AddNotifyAuthExpirationOptionsInternal options, IntPtr clientData, OnAuthExpirationCallbackInternal notification);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern LoginStatus EOS_Connect_GetLoginStatus(IntPtr handle, IntPtr localUserId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Connect_GetLoggedInUserByIndex(IntPtr handle, int index);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern int EOS_Connect_GetLoggedInUsersCount(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Connect_GetProductUserIdMapping(IntPtr handle, ref GetProductUserIdMappingOptionsInternal options, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Connect_GetExternalAccountMapping(IntPtr handle, ref GetExternalAccountMappingsOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Connect_QueryProductUserIdMappings(IntPtr handle, ref QueryProductUserIdMappingsOptionsInternal options, IntPtr clientData, OnQueryProductUserIdMappingsCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Connect_QueryExternalAccountMappings(IntPtr handle, ref QueryExternalAccountMappingsOptionsInternal options, IntPtr clientData, OnQueryExternalAccountMappingsCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Connect_LinkAccount(IntPtr handle, ref LinkAccountOptionsInternal options, IntPtr clientData, OnLinkAccountCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Connect_CreateUser(IntPtr handle, ref CreateUserOptionsInternal options, IntPtr clientData, OnCreateUserCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Connect_Login(IntPtr handle, ref LoginOptionsInternal options, IntPtr clientData, OnLoginCallbackInternal completionDelegate);
}
