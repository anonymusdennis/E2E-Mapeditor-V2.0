using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

public sealed class AuthInterface : Handle
{
	public AuthInterface()
		: base(IntPtr.Zero)
	{
	}

	public AuthInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void Login(LoginOptions options, object clientData, OnLoginCallback completionDelegate)
	{
		LoginOptionsInternal options2 = Helper.CopyPropertiesToNew<LoginOptionsInternal>(options);
		OnLoginCallbackInternal onLoginCallbackInternal = OnLogin;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onLoginCallbackInternal);
		EOS_Auth_Login(base.InnerHandle, ref options2, clientDataAddress, onLoginCallbackInternal);
		options2.Dispose();
	}

	public void Logout(LogoutOptions options, object clientData, OnLogoutCallback completionDelegate)
	{
		LogoutOptionsInternal options2 = Helper.CopyPropertiesToNew<LogoutOptionsInternal>(options);
		OnLogoutCallbackInternal onLogoutCallbackInternal = OnLogout;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onLogoutCallbackInternal);
		EOS_Auth_Logout(base.InnerHandle, ref options2, clientDataAddress, onLogoutCallbackInternal);
		options2.Dispose();
	}

	public void VerifyUserAuth(VerifyUserAuthOptions options, object clientData, OnVerifyUserAuthCallback completionDelegate)
	{
		VerifyUserAuthOptionsInternal options2 = Helper.CopyPropertiesToNew<VerifyUserAuthOptionsInternal>(options);
		OnVerifyUserAuthCallbackInternal onVerifyUserAuthCallbackInternal = OnVerifyUserAuth;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onVerifyUserAuthCallbackInternal);
		EOS_Auth_VerifyUserAuth(base.InnerHandle, ref options2, clientDataAddress, onVerifyUserAuthCallbackInternal);
		options2.Dispose();
	}

	public int GetLoggedInAccountsCount()
	{
		return EOS_Auth_GetLoggedInAccountsCount(base.InnerHandle);
	}

	public EpicAccountId GetLoggedInAccountByIndex(int index)
	{
		IntPtr innerHandle = EOS_Auth_GetLoggedInAccountByIndex(base.InnerHandle, index);
		return Helper.GetHandle<EpicAccountId>(innerHandle);
	}

	public LoginStatus GetLoginStatus(EpicAccountId localUserId)
	{
		return EOS_Auth_GetLoginStatus(base.InnerHandle, localUserId.InnerHandle);
	}

	public Result CopyUserAuthToken(CopyUserAuthTokenOptions options, EpicAccountId localUserId, out Token outUserAuthToken)
	{
		CopyUserAuthTokenOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyUserAuthTokenOptionsInternal>(options);
		outUserAuthToken = Helper.GetDefault<Token>();
		IntPtr outUserAuthToken2 = IntPtr.Zero;
		Result result = EOS_Auth_CopyUserAuthToken(base.InnerHandle, ref options2, localUserId.InnerHandle, ref outUserAuthToken2);
		options2.Dispose();
		if (Helper.TryMarshal<TokenInternal, Token>(outUserAuthToken2, out outUserAuthToken))
		{
			EOS_Auth_Token_Release(outUserAuthToken2);
		}
		return result;
	}

	public ulong AddNotifyLoginStatusChanged(AddNotifyLoginStatusChangedOptions options, object clientData, OnLoginStatusChangedCallback notification)
	{
		AddNotifyLoginStatusChangedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyLoginStatusChangedOptionsInternal>(options);
		OnLoginStatusChangedCallbackInternal onLoginStatusChangedCallbackInternal = OnLoginStatusChanged;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notification, onLoginStatusChangedCallbackInternal);
		ulong result = EOS_Auth_AddNotifyLoginStatusChanged(base.InnerHandle, ref options2, clientDataAddress, onLoginStatusChangedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLoginStatusChanged(ulong inId)
	{
		EOS_Auth_RemoveNotifyLoginStatusChanged(base.InnerHandle, inId);
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
	internal static void OnVerifyUserAuth(IntPtr address)
	{
		OnVerifyUserAuthCallback callDelegate = null;
		VerifyUserAuthCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnVerifyUserAuthCallback, VerifyUserAuthCallbackInfoInternal, VerifyUserAuthCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnLogout(IntPtr address)
	{
		OnLogoutCallback callDelegate = null;
		LogoutCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnLogoutCallback, LogoutCallbackInfoInternal, LogoutCallbackInfo>(address, out callDelegate, out callbackInfo))
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
	private static extern void EOS_Auth_Token_Release(IntPtr authToken);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Auth_RemoveNotifyLoginStatusChanged(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Auth_AddNotifyLoginStatusChanged(IntPtr handle, ref AddNotifyLoginStatusChangedOptionsInternal options, IntPtr clientData, OnLoginStatusChangedCallbackInternal notification);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Auth_CopyUserAuthToken(IntPtr handle, ref CopyUserAuthTokenOptionsInternal options, IntPtr localUserId, ref IntPtr outUserAuthToken);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern LoginStatus EOS_Auth_GetLoginStatus(IntPtr handle, IntPtr localUserId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Auth_GetLoggedInAccountByIndex(IntPtr handle, int index);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern int EOS_Auth_GetLoggedInAccountsCount(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Auth_VerifyUserAuth(IntPtr handle, ref VerifyUserAuthOptionsInternal options, IntPtr clientData, OnVerifyUserAuthCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Auth_Logout(IntPtr handle, ref LogoutOptionsInternal options, IntPtr clientData, OnLogoutCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Auth_Login(IntPtr handle, ref LoginOptionsInternal options, IntPtr clientData, OnLoginCallbackInternal completionDelegate);
}
