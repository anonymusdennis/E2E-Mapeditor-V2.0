using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices;

public sealed class ProductUserId : Handle
{
	public ProductUserId()
		: base(IntPtr.Zero)
	{
	}

	public ProductUserId(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public int IsValid()
	{
		return EOS_ProductUserId_IsValid(base.InnerHandle);
	}

	public Result ToString(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_ProductUserId_ToString(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public static ProductUserId FromString(string accountIdString)
	{
		IntPtr innerHandle = EOS_ProductUserId_FromString(accountIdString);
		return Helper.GetHandle<ProductUserId>(innerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_ProductUserId_FromString([MarshalAs(UnmanagedType.LPStr)] string accountIdString);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_ProductUserId_ToString(IntPtr accountId, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern int EOS_ProductUserId_IsValid(IntPtr accountId);
}
