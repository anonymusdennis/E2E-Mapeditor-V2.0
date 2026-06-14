using System;
using System.Runtime.InteropServices;
using System.Text;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Leaderboards;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Metrics;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.PlayerDataStorage;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.Stats;
using Epic.OnlineServices.UserInfo;

namespace Epic.OnlineServices.Platform;

public sealed class PlatformInterface : Handle
{
	public PlatformInterface()
		: base(IntPtr.Zero)
	{
	}

	public PlatformInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public static Result Initialize(InitializeOptions options)
	{
		InitializeOptionsInternal options2 = Helper.CopyPropertiesToNew<InitializeOptionsInternal>(options);
		int[] memory = new int[2] { 1, 1 };
		IntPtr address = IntPtr.Zero;
		Helper.RegisterAllocation(ref address, memory);
		options2.Reserved = address;
		Result result = EOS_Initialize(ref options2);
		options2.Dispose();
		Helper.ReleaseAllocation(ref address);
		return result;
	}

	public static Result Shutdown()
	{
		return EOS_Shutdown();
	}

	public static PlatformInterface Create(Options options)
	{
		OptionsInternal options2 = Helper.CopyPropertiesToNew<OptionsInternal>(options);
		IntPtr innerHandle = EOS_Platform_Create(ref options2);
		options2.Dispose();
		return Helper.GetHandle<PlatformInterface>(innerHandle);
	}

	public void Release()
	{
		EOS_Platform_Release(base.InnerHandle);
	}

	public void Tick()
	{
		EOS_Platform_Tick(base.InnerHandle);
	}

	public MetricsInterface GetMetricsInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetMetricsInterface(base.InnerHandle);
		return Helper.GetHandle<MetricsInterface>(innerHandle);
	}

	public AuthInterface GetAuthInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetAuthInterface(base.InnerHandle);
		return Helper.GetHandle<AuthInterface>(innerHandle);
	}

	public ConnectInterface GetConnectInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetConnectInterface(base.InnerHandle);
		return Helper.GetHandle<ConnectInterface>(innerHandle);
	}

	public EcomInterface GetEcomInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetEcomInterface(base.InnerHandle);
		return Helper.GetHandle<EcomInterface>(innerHandle);
	}

	public FriendsInterface GetFriendsInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetFriendsInterface(base.InnerHandle);
		return Helper.GetHandle<FriendsInterface>(innerHandle);
	}

	public PresenceInterface GetPresenceInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetPresenceInterface(base.InnerHandle);
		return Helper.GetHandle<PresenceInterface>(innerHandle);
	}

	public SessionsInterface GetSessionsInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetSessionsInterface(base.InnerHandle);
		return Helper.GetHandle<SessionsInterface>(innerHandle);
	}

	public LobbyInterface GetLobbyInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetLobbyInterface(base.InnerHandle);
		return Helper.GetHandle<LobbyInterface>(innerHandle);
	}

	public UserInfoInterface GetUserInfoInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetUserInfoInterface(base.InnerHandle);
		return Helper.GetHandle<UserInfoInterface>(innerHandle);
	}

	public P2PInterface GetP2PInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetP2PInterface(base.InnerHandle);
		return Helper.GetHandle<P2PInterface>(innerHandle);
	}

	public PlayerDataStorageInterface GetPlayerDataStorageInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetPlayerDataStorageInterface(base.InnerHandle);
		return Helper.GetHandle<PlayerDataStorageInterface>(innerHandle);
	}

	public AchievementsInterface GetAchievementsInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetAchievementsInterface(base.InnerHandle);
		return Helper.GetHandle<AchievementsInterface>(innerHandle);
	}

	public StatsInterface GetStatsInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetStatsInterface(base.InnerHandle);
		return Helper.GetHandle<StatsInterface>(innerHandle);
	}

	public LeaderboardsInterface GetLeaderboardsInterface()
	{
		IntPtr innerHandle = EOS_Platform_GetLeaderboardsInterface(base.InnerHandle);
		return Helper.GetHandle<LeaderboardsInterface>(innerHandle);
	}

	public Result GetActiveCountryCode(EpicAccountId localUserId, StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetActiveCountryCode(base.InnerHandle, localUserId.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result GetActiveLocaleCode(EpicAccountId localUserId, StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetActiveLocaleCode(base.InnerHandle, localUserId.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result GetOverrideCountryCode(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetOverrideCountryCode(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result GetOverrideLocaleCode(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetOverrideLocaleCode(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result SetOverrideCountryCode(string newCountryCode)
	{
		return EOS_Platform_SetOverrideCountryCode(base.InnerHandle, newCountryCode);
	}

	public Result SetOverrideLocaleCode(string newLocaleCode)
	{
		return EOS_Platform_SetOverrideLocaleCode(base.InnerHandle, newLocaleCode);
	}

	public Result CheckForLauncherAndRestart()
	{
		return EOS_Platform_CheckForLauncherAndRestart(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Platform_CheckForLauncherAndRestart(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Platform_SetOverrideLocaleCode(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string newLocaleCode);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Platform_SetOverrideCountryCode(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string newCountryCode);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Platform_GetOverrideLocaleCode(IntPtr handle, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Platform_GetOverrideCountryCode(IntPtr handle, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Platform_GetActiveLocaleCode(IntPtr handle, IntPtr localUserId, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Platform_GetActiveCountryCode(IntPtr handle, IntPtr localUserId, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetLeaderboardsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetStatsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetAchievementsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetPlayerDataStorageInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetP2PInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetUserInfoInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetLobbyInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetSessionsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetPresenceInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetFriendsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetEcomInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetConnectInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetAuthInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_GetMetricsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Platform_Tick(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Platform_Release(IntPtr handle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_Platform_Create(ref OptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Shutdown();

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Initialize(ref InitializeOptionsInternal options);
}
