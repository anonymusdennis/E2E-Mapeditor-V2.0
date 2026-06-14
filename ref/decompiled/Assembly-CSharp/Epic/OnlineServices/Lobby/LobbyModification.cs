using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

public sealed class LobbyModification : Handle
{
	public LobbyModification()
		: base(IntPtr.Zero)
	{
	}

	public LobbyModification(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetPermissionLevel(LobbyModificationSetPermissionLevelOptions options)
	{
		LobbyModificationSetPermissionLevelOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyModificationSetPermissionLevelOptionsInternal>(options);
		Result result = EOS_LobbyModification_SetPermissionLevel(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetMaxMembers(LobbyModificationSetMaxMembersOptions options)
	{
		LobbyModificationSetMaxMembersOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyModificationSetMaxMembersOptionsInternal>(options);
		Result result = EOS_LobbyModification_SetMaxMembers(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result AddAttribute(LobbyModificationAddAttributeOptions options)
	{
		LobbyModificationAddAttributeOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyModificationAddAttributeOptionsInternal>(options);
		Result result = EOS_LobbyModification_AddAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result RemoveAttribute(LobbyModificationRemoveAttributeOptions options)
	{
		LobbyModificationRemoveAttributeOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyModificationRemoveAttributeOptionsInternal>(options);
		Result result = EOS_LobbyModification_RemoveAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result AddMemberAttribute(LobbyModificationAddMemberAttributeOptions options)
	{
		LobbyModificationAddMemberAttributeOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyModificationAddMemberAttributeOptionsInternal>(options);
		Result result = EOS_LobbyModification_AddMemberAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result RemoveMemberAttribute(LobbyModificationRemoveMemberAttributeOptions options)
	{
		LobbyModificationRemoveMemberAttributeOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyModificationRemoveMemberAttributeOptionsInternal>(options);
		Result result = EOS_LobbyModification_RemoveMemberAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void Release()
	{
		EOS_LobbyModification_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_LobbyModification_Release(IntPtr lobbyModificationHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyModification_RemoveMemberAttribute(IntPtr handle, ref LobbyModificationRemoveMemberAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyModification_AddMemberAttribute(IntPtr handle, ref LobbyModificationAddMemberAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyModification_RemoveAttribute(IntPtr handle, ref LobbyModificationRemoveAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyModification_AddAttribute(IntPtr handle, ref LobbyModificationAddAttributeOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyModification_SetMaxMembers(IntPtr handle, ref LobbyModificationSetMaxMembersOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyModification_SetPermissionLevel(IntPtr handle, ref LobbyModificationSetPermissionLevelOptionsInternal options);
}
