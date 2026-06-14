using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LobbyUpdateReceivedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_LobbyId;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public string LobbyId => m_LobbyId;

	public void Dispose()
	{
	}
}
