using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

public sealed class AchievementsInterface : Handle
{
	public AchievementsInterface()
		: base(IntPtr.Zero)
	{
	}

	public AchievementsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryDefinitions(QueryDefinitionsOptions options, object clientData, OnQueryDefinitionsCompleteCallback completionDelegate)
	{
		QueryDefinitionsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryDefinitionsOptionsInternal>(options);
		OnQueryDefinitionsCompleteCallbackInternal onQueryDefinitionsCompleteCallbackInternal = OnQueryDefinitionsComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryDefinitionsCompleteCallbackInternal);
		EOS_Achievements_QueryDefinitions(base.InnerHandle, ref options2, clientDataAddress, onQueryDefinitionsCompleteCallbackInternal);
		options2.Dispose();
	}

	public uint GetAchievementDefinitionCount(GetAchievementDefinitionCountOptions options)
	{
		GetAchievementDefinitionCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetAchievementDefinitionCountOptionsInternal>(options);
		uint result = EOS_Achievements_GetAchievementDefinitionCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyAchievementDefinitionByIndex(CopyAchievementDefinitionByIndexOptions options, out Definition outDefinition)
	{
		CopyAchievementDefinitionByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyAchievementDefinitionByIndexOptionsInternal>(options);
		outDefinition = Helper.GetDefault<Definition>();
		IntPtr outDefinition2 = IntPtr.Zero;
		Result result = EOS_Achievements_CopyAchievementDefinitionByIndex(base.InnerHandle, ref options2, ref outDefinition2);
		options2.Dispose();
		if (Helper.TryMarshal<DefinitionInternal, Definition>(outDefinition2, out outDefinition))
		{
			EOS_Achievements_Definition_Release(outDefinition2);
		}
		return result;
	}

	public Result CopyAchievementDefinitionByAchievementId(CopyAchievementDefinitionByAchievementIdOptions options, out Definition outDefinition)
	{
		CopyAchievementDefinitionByAchievementIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyAchievementDefinitionByAchievementIdOptionsInternal>(options);
		outDefinition = Helper.GetDefault<Definition>();
		IntPtr outDefinition2 = IntPtr.Zero;
		Result result = EOS_Achievements_CopyAchievementDefinitionByAchievementId(base.InnerHandle, ref options2, ref outDefinition2);
		options2.Dispose();
		if (Helper.TryMarshal<DefinitionInternal, Definition>(outDefinition2, out outDefinition))
		{
			EOS_Achievements_Definition_Release(outDefinition2);
		}
		return result;
	}

	public void QueryPlayerAchievements(QueryPlayerAchievementsOptions options, object clientData, OnQueryPlayerAchievementsCompleteCallback completionDelegate)
	{
		QueryPlayerAchievementsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryPlayerAchievementsOptionsInternal>(options);
		OnQueryPlayerAchievementsCompleteCallbackInternal onQueryPlayerAchievementsCompleteCallbackInternal = OnQueryPlayerAchievementsComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryPlayerAchievementsCompleteCallbackInternal);
		EOS_Achievements_QueryPlayerAchievements(base.InnerHandle, ref options2, clientDataAddress, onQueryPlayerAchievementsCompleteCallbackInternal);
		options2.Dispose();
	}

	public uint GetPlayerAchievementCount(GetPlayerAchievementCountOptions options)
	{
		GetPlayerAchievementCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetPlayerAchievementCountOptionsInternal>(options);
		uint result = EOS_Achievements_GetPlayerAchievementCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyPlayerAchievementByIndex(CopyPlayerAchievementByIndexOptions options, out PlayerAchievement outAchievement)
	{
		CopyPlayerAchievementByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyPlayerAchievementByIndexOptionsInternal>(options);
		outAchievement = Helper.GetDefault<PlayerAchievement>();
		IntPtr outAchievement2 = IntPtr.Zero;
		Result result = EOS_Achievements_CopyPlayerAchievementByIndex(base.InnerHandle, ref options2, ref outAchievement2);
		options2.Dispose();
		if (Helper.TryMarshal<PlayerAchievementInternal, PlayerAchievement>(outAchievement2, out outAchievement))
		{
			EOS_Achievements_PlayerAchievement_Release(outAchievement2);
		}
		return result;
	}

	public Result CopyPlayerAchievementByAchievementId(CopyPlayerAchievementByAchievementIdOptions options, out PlayerAchievement outAchievement)
	{
		CopyPlayerAchievementByAchievementIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyPlayerAchievementByAchievementIdOptionsInternal>(options);
		outAchievement = Helper.GetDefault<PlayerAchievement>();
		IntPtr outAchievement2 = IntPtr.Zero;
		Result result = EOS_Achievements_CopyPlayerAchievementByAchievementId(base.InnerHandle, ref options2, ref outAchievement2);
		options2.Dispose();
		if (Helper.TryMarshal<PlayerAchievementInternal, PlayerAchievement>(outAchievement2, out outAchievement))
		{
			EOS_Achievements_PlayerAchievement_Release(outAchievement2);
		}
		return result;
	}

	public void UnlockAchievements(UnlockAchievementsOptions options, object clientData, OnUnlockAchievementsCompleteCallback completionDelegate)
	{
		UnlockAchievementsOptionsInternal options2 = Helper.CopyPropertiesToNew<UnlockAchievementsOptionsInternal>(options);
		OnUnlockAchievementsCompleteCallbackInternal onUnlockAchievementsCompleteCallbackInternal = OnUnlockAchievementsComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onUnlockAchievementsCompleteCallbackInternal);
		EOS_Achievements_UnlockAchievements(base.InnerHandle, ref options2, clientDataAddress, onUnlockAchievementsCompleteCallbackInternal);
		options2.Dispose();
	}

	public uint GetUnlockedAchievementCount(GetUnlockedAchievementCountOptions options)
	{
		GetUnlockedAchievementCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetUnlockedAchievementCountOptionsInternal>(options);
		uint result = EOS_Achievements_GetUnlockedAchievementCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyUnlockedAchievementByIndex(CopyUnlockedAchievementByIndexOptions options, out UnlockedAchievement outAchievement)
	{
		CopyUnlockedAchievementByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyUnlockedAchievementByIndexOptionsInternal>(options);
		outAchievement = Helper.GetDefault<UnlockedAchievement>();
		IntPtr outAchievement2 = IntPtr.Zero;
		Result result = EOS_Achievements_CopyUnlockedAchievementByIndex(base.InnerHandle, ref options2, ref outAchievement2);
		options2.Dispose();
		if (Helper.TryMarshal<UnlockedAchievementInternal, UnlockedAchievement>(outAchievement2, out outAchievement))
		{
			EOS_Achievements_UnlockedAchievement_Release(outAchievement2);
		}
		return result;
	}

	public Result CopyUnlockedAchievementByAchievementId(CopyUnlockedAchievementByAchievementIdOptions options, out UnlockedAchievement outAchievement)
	{
		CopyUnlockedAchievementByAchievementIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyUnlockedAchievementByAchievementIdOptionsInternal>(options);
		outAchievement = Helper.GetDefault<UnlockedAchievement>();
		IntPtr outAchievement2 = IntPtr.Zero;
		Result result = EOS_Achievements_CopyUnlockedAchievementByAchievementId(base.InnerHandle, ref options2, ref outAchievement2);
		options2.Dispose();
		if (Helper.TryMarshal<UnlockedAchievementInternal, UnlockedAchievement>(outAchievement2, out outAchievement))
		{
			EOS_Achievements_UnlockedAchievement_Release(outAchievement2);
		}
		return result;
	}

	public ulong AddNotifyAchievementsUnlocked(AddNotifyAchievementsUnlockedOptions options, object clientData, OnAchievementsUnlockedCallback notificationFn)
	{
		AddNotifyAchievementsUnlockedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyAchievementsUnlockedOptionsInternal>(options);
		OnAchievementsUnlockedCallbackInternal onAchievementsUnlockedCallbackInternal = OnAchievementsUnlocked;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, notificationFn, onAchievementsUnlockedCallbackInternal);
		ulong result = EOS_Achievements_AddNotifyAchievementsUnlocked(base.InnerHandle, ref options2, clientDataAddress, onAchievementsUnlockedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyAchievementsUnlocked(ulong inId)
	{
		EOS_Achievements_RemoveNotifyAchievementsUnlocked(base.InnerHandle, inId);
	}

	[MonoPInvokeCallback]
	internal static void OnAchievementsUnlocked(IntPtr address)
	{
		OnAchievementsUnlockedCallback callDelegate = null;
		OnAchievementsUnlockedCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnAchievementsUnlockedCallback, OnAchievementsUnlockedCallbackInfoInternal, OnAchievementsUnlockedCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnUnlockAchievementsComplete(IntPtr address)
	{
		OnUnlockAchievementsCompleteCallback callDelegate = null;
		OnUnlockAchievementsCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnUnlockAchievementsCompleteCallback, OnUnlockAchievementsCompleteCallbackInfoInternal, OnUnlockAchievementsCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryPlayerAchievementsComplete(IntPtr address)
	{
		OnQueryPlayerAchievementsCompleteCallback callDelegate = null;
		OnQueryPlayerAchievementsCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryPlayerAchievementsCompleteCallback, OnQueryPlayerAchievementsCompleteCallbackInfoInternal, OnQueryPlayerAchievementsCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryDefinitionsComplete(IntPtr address)
	{
		OnQueryDefinitionsCompleteCallback callDelegate = null;
		OnQueryDefinitionsCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryDefinitionsCompleteCallback, OnQueryDefinitionsCompleteCallbackInfoInternal, OnQueryDefinitionsCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Achievements_UnlockedAchievement_Release(IntPtr achievement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Achievements_PlayerAchievement_Release(IntPtr achievement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Achievements_Definition_Release(IntPtr achievementDefinition);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Achievements_RemoveNotifyAchievementsUnlocked(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_Achievements_AddNotifyAchievementsUnlocked(IntPtr handle, ref AddNotifyAchievementsUnlockedOptionsInternal options, IntPtr clientData, OnAchievementsUnlockedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Achievements_CopyUnlockedAchievementByAchievementId(IntPtr handle, ref CopyUnlockedAchievementByAchievementIdOptionsInternal options, ref IntPtr outAchievement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Achievements_CopyUnlockedAchievementByIndex(IntPtr handle, ref CopyUnlockedAchievementByIndexOptionsInternal options, ref IntPtr outAchievement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Achievements_GetUnlockedAchievementCount(IntPtr handle, ref GetUnlockedAchievementCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Achievements_UnlockAchievements(IntPtr handle, ref UnlockAchievementsOptionsInternal options, IntPtr clientData, OnUnlockAchievementsCompleteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Achievements_CopyPlayerAchievementByAchievementId(IntPtr handle, ref CopyPlayerAchievementByAchievementIdOptionsInternal options, ref IntPtr outAchievement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Achievements_CopyPlayerAchievementByIndex(IntPtr handle, ref CopyPlayerAchievementByIndexOptionsInternal options, ref IntPtr outAchievement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Achievements_GetPlayerAchievementCount(IntPtr handle, ref GetPlayerAchievementCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Achievements_QueryPlayerAchievements(IntPtr handle, ref QueryPlayerAchievementsOptionsInternal options, IntPtr clientData, OnQueryPlayerAchievementsCompleteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Achievements_CopyAchievementDefinitionByAchievementId(IntPtr handle, ref CopyAchievementDefinitionByAchievementIdOptionsInternal options, ref IntPtr outDefinition);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Achievements_CopyAchievementDefinitionByIndex(IntPtr handle, ref CopyAchievementDefinitionByIndexOptionsInternal options, ref IntPtr outDefinition);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Achievements_GetAchievementDefinitionCount(IntPtr handle, ref GetAchievementDefinitionCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Achievements_QueryDefinitions(IntPtr handle, ref QueryDefinitionsOptionsInternal options, IntPtr clientData, OnQueryDefinitionsCompleteCallbackInternal completionDelegate);
}
