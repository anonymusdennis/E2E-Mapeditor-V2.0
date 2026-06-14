using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionModification : Handle
{
	public SessionModification()
		: base(IntPtr.Zero)
	{
	}

	public SessionModification(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetBucketId(SessionModificationSetBucketIdOptions options)
	{
		SessionModificationSetBucketIdOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationSetBucketIdOptionsInternal>(options);
		Result result = EOS_SessionModification_SetBucketId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetHostAddress(SessionModificationSetHostAddressOptions options)
	{
		SessionModificationSetHostAddressOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationSetHostAddressOptionsInternal>(options);
		Result result = EOS_SessionModification_SetHostAddress(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetPermissionLevel(SessionModificationSetPermissionLevelOptions options)
	{
		SessionModificationSetPermissionLevelOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationSetPermissionLevelOptionsInternal>(options);
		Result result = EOS_SessionModification_SetPermissionLevel(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetJoinInProgressAllowed(SessionModificationSetJoinInProgressAllowedOptions options)
	{
		SessionModificationSetJoinInProgressAllowedOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationSetJoinInProgressAllowedOptionsInternal>(options);
		Result result = EOS_SessionModification_SetJoinInProgressAllowed(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetMaxPlayers(SessionModificationSetMaxPlayersOptions options)
	{
		SessionModificationSetMaxPlayersOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationSetMaxPlayersOptionsInternal>(options);
		Result result = EOS_SessionModification_SetMaxPlayers(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetInvitesAllowed(SessionModificationSetInvitesAllowedOptions options)
	{
		SessionModificationSetInvitesAllowedOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationSetInvitesAllowedOptionsInternal>(options);
		Result result = EOS_SessionModification_SetInvitesAllowed(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result AddAttribute(SessionModificationAddAttributeOptions options)
	{
		SessionModificationAddAttributeOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationAddAttributeOptionsInternal>(options);
		Result result = EOS_SessionModification_AddAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result RemoveAttribute(SessionModificationRemoveAttributeOptions options)
	{
		SessionModificationRemoveAttributeOptionsInternal options2 = Helper.CopyPropertiesToNew<SessionModificationRemoveAttributeOptionsInternal>(options);
		Result result = EOS_SessionModification_RemoveAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void Release()
	{
		EOS_SessionModification_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_SessionModification_Release(IntPtr sessionModificationHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_RemoveAttribute(IntPtr handle, ref SessionModificationRemoveAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_AddAttribute(IntPtr handle, ref SessionModificationAddAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_SetInvitesAllowed(IntPtr handle, ref SessionModificationSetInvitesAllowedOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_SetMaxPlayers(IntPtr handle, ref SessionModificationSetMaxPlayersOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_SetJoinInProgressAllowed(IntPtr handle, ref SessionModificationSetJoinInProgressAllowedOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_SetPermissionLevel(IntPtr handle, ref SessionModificationSetPermissionLevelOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_SetHostAddress(IntPtr handle, ref SessionModificationSetHostAddressOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_SessionModification_SetBucketId(IntPtr handle, ref SessionModificationSetBucketIdOptionsInternal options);
}
