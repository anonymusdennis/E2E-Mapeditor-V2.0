using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyMemberUpdateReceivedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LobbyId;

	private IntPtr m_TargetUserId;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public string LobbyId => m_LobbyId;

	public ProductUserId TargetUserId => Helper.GetHandle<ProductUserId>(m_TargetUserId);

	public void Dispose()
	{
	}
}
