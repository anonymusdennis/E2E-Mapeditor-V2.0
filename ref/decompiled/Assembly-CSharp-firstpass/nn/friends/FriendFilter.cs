using System.Runtime.InteropServices;

namespace nn.friends;

public struct FriendFilter
{
	public PresenceStatusFilter presenceStatus;

	[MarshalAs(UnmanagedType.U1)]
	public bool isFavoriteOnly;

	[MarshalAs(UnmanagedType.U1)]
	public bool isSameAppPresenceOnly;

	[MarshalAs(UnmanagedType.U1)]
	public bool isSameAppPlayedOnly;

	[MarshalAs(UnmanagedType.U1)]
	public bool isArbitraryAppPlayedOnly;

	public ulong presenceGroupId;
}
