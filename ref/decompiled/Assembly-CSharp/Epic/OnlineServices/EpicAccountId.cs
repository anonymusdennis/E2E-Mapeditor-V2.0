using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices;

public sealed class EpicAccountId : Handle
{
	public EpicAccountId()
		: base(IntPtr.Zero)
	{
	}

	public EpicAccountId(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public int IsValid()
	{
		return EOS_EpicAccountId_IsValid(base.InnerHandle);
	}

	public Result ToString(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_EpicAccountId_ToString(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public static EpicAccountId FromString(string accountIdString)
	{
		IntPtr innerHandle = EOS_EpicAccountId_FromString(accountIdString);
		return Helper.GetHandle<EpicAccountId>(innerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_EpicAccountId_FromString([MarshalAs(UnmanagedType.LPStr)] string accountIdString);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_EpicAccountId_ToString(IntPtr accountId, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern int EOS_EpicAccountId_IsValid(IntPtr accountId);
}
