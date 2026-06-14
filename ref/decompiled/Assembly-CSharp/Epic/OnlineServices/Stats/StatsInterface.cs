using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats;

public sealed class StatsInterface : Handle
{
	public StatsInterface()
		: base(IntPtr.Zero)
	{
	}

	public StatsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void IngestStat(IngestStatOptions options, object clientData, OnIngestStatCompleteCallback completionDelegate)
	{
		IngestStatOptionsInternal options2 = Helper.CopyPropertiesToNew<IngestStatOptionsInternal>(options);
		OnIngestStatCompleteCallbackInternal onIngestStatCompleteCallbackInternal = OnIngestStatComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onIngestStatCompleteCallbackInternal);
		EOS_Stats_IngestStat(base.InnerHandle, ref options2, clientDataAddress, onIngestStatCompleteCallbackInternal);
		options2.Dispose();
	}

	public void QueryStats(QueryStatsOptions options, object clientData, OnQueryStatsCompleteCallback completionDelegate)
	{
		QueryStatsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryStatsOptionsInternal>(options);
		OnQueryStatsCompleteCallbackInternal onQueryStatsCompleteCallbackInternal = OnQueryStatsComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryStatsCompleteCallbackInternal);
		EOS_Stats_QueryStats(base.InnerHandle, ref options2, clientDataAddress, onQueryStatsCompleteCallbackInternal);
		options2.Dispose();
	}

	public uint GetStatsCount(GetStatCountOptions options)
	{
		GetStatCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetStatCountOptionsInternal>(options);
		uint result = EOS_Stats_GetStatsCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyStatByIndex(CopyStatByIndexOptions options, out Stat outStat)
	{
		CopyStatByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyStatByIndexOptionsInternal>(options);
		outStat = Helper.GetDefault<Stat>();
		IntPtr outStat2 = IntPtr.Zero;
		Result result = EOS_Stats_CopyStatByIndex(base.InnerHandle, ref options2, ref outStat2);
		options2.Dispose();
		if (Helper.TryMarshal<StatInternal, Stat>(outStat2, out outStat))
		{
			EOS_Stats_Stat_Release(outStat2);
		}
		return result;
	}

	public Result CopyStatByName(CopyStatByNameOptions options, out Stat outStat)
	{
		CopyStatByNameOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyStatByNameOptionsInternal>(options);
		outStat = Helper.GetDefault<Stat>();
		IntPtr outStat2 = IntPtr.Zero;
		Result result = EOS_Stats_CopyStatByName(base.InnerHandle, ref options2, ref outStat2);
		options2.Dispose();
		if (Helper.TryMarshal<StatInternal, Stat>(outStat2, out outStat))
		{
			EOS_Stats_Stat_Release(outStat2);
		}
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnQueryStatsComplete(IntPtr address)
	{
		OnQueryStatsCompleteCallback callDelegate = null;
		OnQueryStatsCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryStatsCompleteCallback, OnQueryStatsCompleteCallbackInfoInternal, OnQueryStatsCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnIngestStatComplete(IntPtr address)
	{
		OnIngestStatCompleteCallback callDelegate = null;
		IngestStatCompleteCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnIngestStatCompleteCallback, IngestStatCompleteCallbackInfoInternal, IngestStatCompleteCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Stats_Stat_Release(IntPtr stat);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Stats_CopyStatByName(IntPtr handle, ref CopyStatByNameOptionsInternal options, ref IntPtr outStat);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Stats_CopyStatByIndex(IntPtr handle, ref CopyStatByIndexOptionsInternal options, ref IntPtr outStat);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Stats_GetStatsCount(IntPtr handle, ref GetStatCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Stats_QueryStats(IntPtr handle, ref QueryStatsOptionsInternal options, IntPtr clientData, OnQueryStatsCompleteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Stats_IngestStat(IntPtr handle, ref IngestStatOptionsInternal options, IntPtr clientData, OnIngestStatCompleteCallbackInternal completionDelegate);
}
