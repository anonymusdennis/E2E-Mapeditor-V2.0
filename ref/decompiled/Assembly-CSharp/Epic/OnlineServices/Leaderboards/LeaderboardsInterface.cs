using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Leaderboards;

public sealed class LeaderboardsInterface : Handle
{
	public LeaderboardsInterface()
		: base(IntPtr.Zero)
	{
	}

	public LeaderboardsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryLeaderboardDefinitions(QueryLeaderboardDefinitionsOptions options, object clientData, OnQueryLeaderboardDefinitionsCompleteCallback completionDelegate)
	{
		QueryLeaderboardDefinitionsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryLeaderboardDefinitionsOptionsInternal>(options);
		OnQueryLeaderboardDefinitionsCompleteCallbackInternal onQueryLeaderboardDefinitionsCompleteCallbackInternal = OnQueryLeaderboardDefinitionsComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryLeaderboardDefinitionsCompleteCallbackInternal);
		EOS_Leaderboards_QueryLeaderboardDefinitions(base.InnerHandle, ref options2, clientDataAddress, onQueryLeaderboardDefinitionsCompleteCallbackInternal);
		options2.Dispose();
	}

	public uint GetLeaderboardDefinitionCount(GetLeaderboardDefinitionCountOptions options)
	{
		GetLeaderboardDefinitionCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetLeaderboardDefinitionCountOptionsInternal>(options);
		uint result = EOS_Leaderboards_GetLeaderboardDefinitionCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyLeaderboardDefinitionByIndex(CopyLeaderboardDefinitionByIndexOptions options, out Definition outLeaderboardDefinition)
	{
		CopyLeaderboardDefinitionByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLeaderboardDefinitionByIndexOptionsInternal>(options);
		outLeaderboardDefinition = Helper.GetDefault<Definition>();
		IntPtr outLeaderboardDefinition2 = IntPtr.Zero;
		Result result = EOS_Leaderboards_CopyLeaderboardDefinitionByIndex(base.InnerHandle, ref options2, ref outLeaderboardDefinition2);
		options2.Dispose();
		if (Helper.TryMarshal<DefinitionInternal, Definition>(outLeaderboardDefinition2, out outLeaderboardDefinition))
		{
		}
		return result;
	}

	public Result CopyLeaderboardDefinitionByLeaderboardId(CopyLeaderboardDefinitionByLeaderboardIdOptions options, out Definition outLeaderboardDefinition)
	{
		CopyLeaderboardDefinitionByLeaderboardIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLeaderboardDefinitionByLeaderboardIdOptionsInternal>(options);
		outLeaderboardDefinition = Helper.GetDefault<Definition>();
		IntPtr outLeaderboardDefinition2 = IntPtr.Zero;
		Result result = EOS_Leaderboards_CopyLeaderboardDefinitionByLeaderboardId(base.InnerHandle, ref options2, ref outLeaderboardDefinition2);
		options2.Dispose();
		if (Helper.TryMarshal<DefinitionInternal, Definition>(outLeaderboardDefinition2, out outLeaderboardDefinition))
		{
		}
		return result;
	}

	public void QueryLeaderboardRanks(QueryLeaderboardRanksOptions options, object clientData, OnQueryLeaderboardRanksCompleteCallback completionDelegate)
	{
		QueryLeaderboardRanksOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryLeaderboardRanksOptionsInternal>(options);
		OnQueryLeaderboardRanksCompleteCallbackInternal onQueryLeaderboardRanksCompleteCallbackInternal = OnQueryLeaderboardRanksComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryLeaderboardRanksCompleteCallbackInternal);
		EOS_Leaderboards_QueryLeaderboardRanks(base.InnerHandle, ref options2, clientDataAddress, onQueryLeaderboardRanksCompleteCallbackInternal);
		options2.Dispose();
	}

	public uint GetLeaderboardRecordCount(GetLeaderboardRecordCountOptions options)
	{
		GetLeaderboardRecordCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetLeaderboardRecordCountOptionsInternal>(options);
		uint result = EOS_Leaderboards_GetLeaderboardRecordCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyLeaderboardRecordByIndex(CopyLeaderboardRecordByIndexOptions options, out LeaderboardRecord outLeaderboardRecord)
	{
		CopyLeaderboardRecordByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLeaderboardRecordByIndexOptionsInternal>(options);
		outLeaderboardRecord = Helper.GetDefault<LeaderboardRecord>();
		IntPtr outLeaderboardRecord2 = IntPtr.Zero;
		Result result = EOS_Leaderboards_CopyLeaderboardRecordByIndex(base.InnerHandle, ref options2, ref outLeaderboardRecord2);
		options2.Dispose();
		if (Helper.TryMarshal<LeaderboardRecordInternal, LeaderboardRecord>(outLeaderboardRecord2, out outLeaderboardRecord))
		{
			EOS_Leaderboards_LeaderboardRecord_Release(outLeaderboardRecord2);
		}
		return result;
	}

	public Result CopyLeaderboardRecordByUserId(CopyLeaderboardRecordByUserIdOptions options, out LeaderboardRecord outLeaderboardRecord)
	{
		CopyLeaderboardRecordByUserIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLeaderboardRecordByUserIdOptionsInternal>(options);
		outLeaderboardRecord = Helper.GetDefault<LeaderboardRecord>();
		IntPtr outLeaderboardRecord2 = IntPtr.Zero;
		Result result = EOS_Leaderboards_CopyLeaderboardRecordByUserId(base.InnerHandle, ref options2, ref outLeaderboardRecord2);
		options2.Dispose();
		if (Helper.TryMarshal<LeaderboardRecordInternal, LeaderboardRecord>(outLeaderboardRecord2, out outLeaderboardRecord))
		{
			EOS_Leaderboards_LeaderboardRecord_Release(outLeaderboardRecord2);
		}
		return result;
	}

	public void QueryLeaderboardUserScores(QueryLeaderboardUserScoresOptions options, object clientData, OnQueryLeaderboardUserScoresCompleteCallback completionDelegate)
	{
		QueryLeaderboardUserScoresOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryLeaderboardUserScoresOptionsInternal>(options);
		OnQueryLeaderboardUserScoresCompleteCallbackInternal onQueryLeaderboardUserScoresCompleteCallbackInternal = OnQueryLeaderboardUserScoresComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryLeaderboardUserScoresCompleteCallbackInternal);
		EOS_Leaderboards_QueryLeaderboardUserScores(base.InnerHandle, ref options2, clientDataAddress, onQueryLeaderboardUserScoresCompleteCallbackInternal);
		options2.Dispose();
	}

	public uint GetLeaderboardUserScoreCount(GetLeaderboardUserScoreCountOptions options)
	{
		GetLeaderboardUserScoreCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetLeaderboardUserScoreCountOptionsInternal>(options);
		uint result = EOS_Leaderboards_GetLeaderboardUserScoreCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyLeaderboardUserScoreByIndex(CopyLeaderboardUserScoreByIndexOptions options, out LeaderboardUserScore outLeaderboardUserScore)
	{
		CopyLeaderboardUserScoreByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLeaderboardUserScoreByIndexOptionsInternal>(options);
		outLeaderboardUserScore = Helper.GetDefault<LeaderboardUserScore>();
		IntPtr outLeaderboardUserScore2 = IntPtr.Zero;
		Result result = EOS_Leaderboards_CopyLeaderboardUserScoreByIndex(base.InnerHandle, ref options2, ref outLeaderboardUserScore2);
		options2.Dispose();
		if (Helper.TryMarshal<LeaderboardUserScoreInternal, LeaderboardUserScore>(outLeaderboardUserScore2, out outLeaderboardUserScore))
		{
			EOS_Leaderboards_LeaderboardUserScore_Release(outLeaderboardUserScore2);
		}
		return result;
	}

	public Result CopyLeaderboardUserScoreByUserId(CopyLeaderboardUserScoreByUserIdOptions options, out LeaderboardUserScore outLeaderboardUserScore)
	{
		CopyLeaderboardUserScoreByUserIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyLeaderboardUserScoreByUserIdOptionsInternal>(options);
		outLeaderboardUserScore = Helper.GetDefault<LeaderboardUserScore>();
		IntPtr outLeaderboardUserScore2 = IntPtr.Zero;
		Result result = EOS_Leaderboards_CopyLeaderboardUserScoreByUserId(base.InnerHandle, ref options2, ref outLeaderboardUserScore2);
		options2.Dispose();
		if (Helper.TryMarshal<LeaderboardUserScoreInternal, LeaderboardUserScore>(outLeaderboardUserScore2, out outLeaderboardUserScore))
		{
			EOS_Leaderboards_LeaderboardUserScore_Release(outLeaderboardUserScore2);
		}
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnQueryLeaderboardUserScoresComplete(IntPtr address)
	{
		OnQueryLeaderboardUserScoresCompleteCallback callDelegate = null;
		OnQueryLeaderboardUserScoresCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryLeaderboardUserScoresCompleteCallback, OnQueryLeaderboardUserScoresCompleteCallbackInfoInternal, OnQueryLeaderboardUserScoresCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryLeaderboardRanksComplete(IntPtr address)
	{
		OnQueryLeaderboardRanksCompleteCallback callDelegate = null;
		OnQueryLeaderboardRanksCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryLeaderboardRanksCompleteCallback, OnQueryLeaderboardRanksCompleteCallbackInfoInternal, OnQueryLeaderboardRanksCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryLeaderboardDefinitionsComplete(IntPtr address)
	{
		OnQueryLeaderboardDefinitionsCompleteCallback callDelegate = null;
		OnQueryLeaderboardDefinitionsCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryLeaderboardDefinitionsCompleteCallback, OnQueryLeaderboardDefinitionsCompleteCallbackInfoInternal, OnQueryLeaderboardDefinitionsCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Leaderboards_LeaderboardRecord_Release(IntPtr leaderboardRecord);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Leaderboards_LeaderboardUserScore_Release(IntPtr leaderboardUserScore);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Leaderboards_LeaderboardDefinition_Release(IntPtr leaderboardDefinition);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Leaderboards_CopyLeaderboardUserScoreByUserId(IntPtr handle, ref CopyLeaderboardUserScoreByUserIdOptionsInternal options, ref IntPtr outLeaderboardUserScore);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Leaderboards_CopyLeaderboardUserScoreByIndex(IntPtr handle, ref CopyLeaderboardUserScoreByIndexOptionsInternal options, ref IntPtr outLeaderboardUserScore);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Leaderboards_GetLeaderboardUserScoreCount(IntPtr handle, ref GetLeaderboardUserScoreCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Leaderboards_QueryLeaderboardUserScores(IntPtr handle, ref QueryLeaderboardUserScoresOptionsInternal options, IntPtr clientData, OnQueryLeaderboardUserScoresCompleteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Leaderboards_CopyLeaderboardRecordByUserId(IntPtr handle, ref CopyLeaderboardRecordByUserIdOptionsInternal options, ref IntPtr outLeaderboardRecord);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Leaderboards_CopyLeaderboardRecordByIndex(IntPtr handle, ref CopyLeaderboardRecordByIndexOptionsInternal options, ref IntPtr outLeaderboardRecord);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Leaderboards_GetLeaderboardRecordCount(IntPtr handle, ref GetLeaderboardRecordCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Leaderboards_QueryLeaderboardRanks(IntPtr handle, ref QueryLeaderboardRanksOptionsInternal options, IntPtr clientData, OnQueryLeaderboardRanksCompleteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Leaderboards_CopyLeaderboardDefinitionByLeaderboardId(IntPtr handle, ref CopyLeaderboardDefinitionByLeaderboardIdOptionsInternal options, ref IntPtr outLeaderboardDefinition);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Leaderboards_CopyLeaderboardDefinitionByIndex(IntPtr handle, ref CopyLeaderboardDefinitionByIndexOptionsInternal options, ref IntPtr outLeaderboardDefinition);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Leaderboards_GetLeaderboardDefinitionCount(IntPtr handle, ref GetLeaderboardDefinitionCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Leaderboards_QueryLeaderboardDefinitions(IntPtr handle, ref QueryLeaderboardDefinitionsOptionsInternal options, IntPtr clientData, OnQueryLeaderboardDefinitionsCompleteCallbackInternal completionDelegate);
}
