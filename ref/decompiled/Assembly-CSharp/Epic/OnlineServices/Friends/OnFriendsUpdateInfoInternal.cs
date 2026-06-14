using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct OnFriendsUpdateInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	private FriendsStatus m_PreviousStatus;

	private FriendsStatus m_CurrentStatus;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public EpicAccountId LocalUserId => Helper.GetHandle<EpicAccountId>(m_LocalUserId);

	public EpicAccountId TargetUserId => Helper.GetHandle<EpicAccountId>(m_TargetUserId);

	public FriendsStatus PreviousStatus => m_PreviousStatus;

	public FriendsStatus CurrentStatus => m_CurrentStatus;

	public void Dispose()
	{
	}
}
