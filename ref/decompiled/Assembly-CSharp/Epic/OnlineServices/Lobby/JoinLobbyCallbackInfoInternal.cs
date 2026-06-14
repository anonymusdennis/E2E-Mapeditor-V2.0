using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct JoinLobbyCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LobbyId;

	public Result ResultCode => m_ResultCode;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public string LobbyId => m_LobbyId;

	public void Dispose()
	{
	}
}
