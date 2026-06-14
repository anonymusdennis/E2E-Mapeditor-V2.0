using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo;

public sealed class UserInfoInterface : Handle
{
	public UserInfoInterface()
		: base(IntPtr.Zero)
	{
	}

	public UserInfoInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryUserInfo(QueryUserInfoOptions options, object clientData, OnQueryUserInfoCallback completionDelegate)
	{
		QueryUserInfoOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryUserInfoOptionsInternal>(options);
		OnQueryUserInfoCallbackInternal onQueryUserInfoCallbackInternal = OnQueryUserInfo;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryUserInfoCallbackInternal);
		EOS_UserInfo_QueryUserInfo(base.InnerHandle, ref options2, clientDataAddress, onQueryUserInfoCallbackInternal);
		options2.Dispose();
	}

	public void QueryUserInfoByDisplayName(QueryUserInfoByDisplayNameOptions options, object clientData, OnQueryUserInfoByDisplayNameCallback completionDelegate)
	{
		QueryUserInfoByDisplayNameOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryUserInfoByDisplayNameOptionsInternal>(options);
		OnQueryUserInfoByDisplayNameCallbackInternal onQueryUserInfoByDisplayNameCallbackInternal = OnQueryUserInfoByDisplayName;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryUserInfoByDisplayNameCallbackInternal);
		EOS_UserInfo_QueryUserInfoByDisplayName(base.InnerHandle, ref options2, clientDataAddress, onQueryUserInfoByDisplayNameCallbackInternal);
		options2.Dispose();
	}

	public Result CopyUserInfo(CopyUserInfoOptions options, out UserInfoData outUserInfo)
	{
		CopyUserInfoOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyUserInfoOptionsInternal>(options);
		outUserInfo = Helper.GetDefault<UserInfoData>();
		IntPtr outUserInfo2 = IntPtr.Zero;
		Result result = EOS_UserInfo_CopyUserInfo(base.InnerHandle, ref options2, ref outUserInfo2);
		options2.Dispose();
		if (Helper.TryMarshal<UserInfoDataInternal, UserInfoData>(outUserInfo2, out outUserInfo))
		{
			EOS_UserInfo_Release(outUserInfo2);
		}
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnQueryUserInfoByDisplayName(IntPtr address)
	{
		OnQueryUserInfoByDisplayNameCallback callDelegate = null;
		QueryUserInfoByDisplayNameCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryUserInfoByDisplayNameCallback, QueryUserInfoByDisplayNameCallbackInfoInternal, QueryUserInfoByDisplayNameCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryUserInfo(IntPtr address)
	{
		OnQueryUserInfoCallback callDelegate = null;
		QueryUserInfoCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryUserInfoCallback, QueryUserInfoCallbackInfoInternal, QueryUserInfoCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_UserInfo_Release(IntPtr userInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_UserInfo_CopyUserInfo(IntPtr handle, ref CopyUserInfoOptionsInternal options, ref IntPtr outUserInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_UserInfo_QueryUserInfoByDisplayName(IntPtr handle, ref QueryUserInfoByDisplayNameOptionsInternal options, IntPtr clientData, OnQueryUserInfoByDisplayNameCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_UserInfo_QueryUserInfo(IntPtr handle, ref QueryUserInfoOptionsInternal options, IntPtr clientData, OnQueryUserInfoCallbackInternal completionDelegate);
}
