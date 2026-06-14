using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices.Ecom;

public sealed class Transaction : Handle
{
	public Transaction()
		: base(IntPtr.Zero)
	{
	}

	public Transaction(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result GetTransactionId(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Ecom_Transaction_GetTransactionId(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public uint GetEntitlementsCount(TransactionGetEntitlementsCountOptions options)
	{
		TransactionGetEntitlementsCountOptionsInternal options2 = Helper.CopyPropertiesToNew<TransactionGetEntitlementsCountOptionsInternal>(options);
		uint result = EOS_Ecom_Transaction_GetEntitlementsCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyEntitlementByIndex(TransactionCopyEntitlementByIndexOptions options, out Entitlement outEntitlement)
	{
		TransactionCopyEntitlementByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<TransactionCopyEntitlementByIndexOptionsInternal>(options);
		outEntitlement = Helper.GetDefault<Entitlement>();
		IntPtr outEntitlement2 = IntPtr.Zero;
		Result result = EOS_Ecom_Transaction_CopyEntitlementByIndex(base.InnerHandle, ref options2, ref outEntitlement2);
		options2.Dispose();
		if (Helper.TryMarshal<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
		{
			EOS_Ecom_Entitlement_Release(outEntitlement2);
		}
		return result;
	}

	public void Release()
	{
		EOS_Ecom_Transaction_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_Entitlement_Release(IntPtr entitlement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_Transaction_Release(IntPtr transaction);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_Transaction_CopyEntitlementByIndex(IntPtr handle, ref TransactionCopyEntitlementByIndexOptionsInternal options, ref IntPtr outEntitlement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_Transaction_GetEntitlementsCount(IntPtr handle, ref TransactionGetEntitlementsCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_Transaction_GetTransactionId(IntPtr handle, StringBuilder outBuffer, ref int inOutBufferLength);
}
