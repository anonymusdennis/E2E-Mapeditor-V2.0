using System.Runtime.InteropServices;

namespace nn.friends;

public struct Friend
{
	public ulong NetworkServiceAccountId;

	public string Nickname;

	[MarshalAs(UnmanagedType.U1)]
	public bool IsValid;

	public string AppDescription;

	public PresenceStatus PresenceStatus;

	[MarshalAs(UnmanagedType.U1)]
	public bool IsSamePresenceGroupApplication;

	[MarshalAs(UnmanagedType.U1)]
	public bool IsFavorite;
}
