using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

public sealed class EcomInterface : Handle
{
	public EcomInterface()
		: base(IntPtr.Zero)
	{
	}

	public EcomInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryOwnership(QueryOwnershipOptions options, object clientData, OnQueryOwnershipCallback completionDelegate)
	{
		QueryOwnershipOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryOwnershipOptionsInternal>(options);
		OnQueryOwnershipCallbackInternal onQueryOwnershipCallbackInternal = OnQueryOwnership;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryOwnershipCallbackInternal);
		EOS_Ecom_QueryOwnership(base.InnerHandle, ref options2, clientDataAddress, onQueryOwnershipCallbackInternal);
		options2.Dispose();
	}

	public void QueryOwnershipToken(QueryOwnershipTokenOptions options, object clientData, OnQueryOwnershipTokenCallback completionDelegate)
	{
		QueryOwnershipTokenOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryOwnershipTokenOptionsInternal>(options);
		OnQueryOwnershipTokenCallbackInternal onQueryOwnershipTokenCallbackInternal = OnQueryOwnershipToken;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryOwnershipTokenCallbackInternal);
		EOS_Ecom_QueryOwnershipToken(base.InnerHandle, ref options2, clientDataAddress, onQueryOwnershipTokenCallbackInternal);
		options2.Dispose();
	}

	public void QueryEntitlements(QueryEntitlementsOptions options, object clientData, OnQueryEntitlementsCallback completionDelegate)
	{
		QueryEntitlementsOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryEntitlementsOptionsInternal>(options);
		OnQueryEntitlementsCallbackInternal onQueryEntitlementsCallbackInternal = OnQueryEntitlements;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryEntitlementsCallbackInternal);
		EOS_Ecom_QueryEntitlements(base.InnerHandle, ref options2, clientDataAddress, onQueryEntitlementsCallbackInternal);
		options2.Dispose();
	}

	public void QueryOffers(QueryOffersOptions options, object clientData, OnQueryOffersCallback completionDelegate)
	{
		QueryOffersOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryOffersOptionsInternal>(options);
		OnQueryOffersCallbackInternal onQueryOffersCallbackInternal = OnQueryOffers;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onQueryOffersCallbackInternal);
		EOS_Ecom_QueryOffers(base.InnerHandle, ref options2, clientDataAddress, onQueryOffersCallbackInternal);
		options2.Dispose();
	}

	public void Checkout(CheckoutOptions options, object clientData, OnCheckoutCallback completionDelegate)
	{
		CheckoutOptionsInternal options2 = Helper.CopyPropertiesToNew<CheckoutOptionsInternal>(options);
		OnCheckoutCallbackInternal onCheckoutCallbackInternal = OnCheckout;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onCheckoutCallbackInternal);
		EOS_Ecom_Checkout(base.InnerHandle, ref options2, clientDataAddress, onCheckoutCallbackInternal);
		options2.Dispose();
	}

	public void RedeemEntitlements(RedeemEntitlementsOptions options, object clientData, OnRedeemEntitlementsCallback completionDelegate)
	{
		RedeemEntitlementsOptionsInternal options2 = Helper.CopyPropertiesToNew<RedeemEntitlementsOptionsInternal>(options);
		OnRedeemEntitlementsCallbackInternal onRedeemEntitlementsCallbackInternal = OnRedeemEntitlements;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, completionDelegate, onRedeemEntitlementsCallbackInternal);
		EOS_Ecom_RedeemEntitlements(base.InnerHandle, ref options2, clientDataAddress, onRedeemEntitlementsCallbackInternal);
		options2.Dispose();
	}

	public uint GetEntitlementsCount(GetEntitlementsCountOptions options)
	{
		GetEntitlementsCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetEntitlementsCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetEntitlementsCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public uint GetEntitlementsByNameCount(GetEntitlementsByNameCountOptions options)
	{
		GetEntitlementsByNameCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetEntitlementsByNameCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetEntitlementsByNameCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyEntitlementByIndex(CopyEntitlementByIndexOptions options, out Entitlement outEntitlement)
	{
		CopyEntitlementByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyEntitlementByIndexOptionsInternal>(options);
		outEntitlement = Helper.GetDefault<Entitlement>();
		IntPtr outEntitlement2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyEntitlementByIndex(base.InnerHandle, ref options2, ref outEntitlement2);
		options2.Dispose();
		if (Helper.TryMarshal<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
		{
			EOS_Ecom_Entitlement_Release(outEntitlement2);
		}
		return result;
	}

	public Result CopyEntitlementByNameAndIndex(CopyEntitlementByNameAndIndexOptions options, out Entitlement outEntitlement)
	{
		CopyEntitlementByNameAndIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyEntitlementByNameAndIndexOptionsInternal>(options);
		outEntitlement = Helper.GetDefault<Entitlement>();
		IntPtr outEntitlement2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyEntitlementByNameAndIndex(base.InnerHandle, ref options2, ref outEntitlement2);
		options2.Dispose();
		if (Helper.TryMarshal<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
		{
			EOS_Ecom_Entitlement_Release(outEntitlement2);
		}
		return result;
	}

	public Result CopyEntitlementById(CopyEntitlementByIdOptions options, out Entitlement outEntitlement)
	{
		CopyEntitlementByIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyEntitlementByIdOptionsInternal>(options);
		outEntitlement = Helper.GetDefault<Entitlement>();
		IntPtr outEntitlement2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyEntitlementById(base.InnerHandle, ref options2, ref outEntitlement2);
		options2.Dispose();
		if (Helper.TryMarshal<EntitlementInternal, Entitlement>(outEntitlement2, out outEntitlement))
		{
			EOS_Ecom_Entitlement_Release(outEntitlement2);
		}
		return result;
	}

	public uint GetOfferCount(GetOfferCountOptions options)
	{
		GetOfferCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetOfferCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetOfferCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyOfferByIndex(CopyOfferByIndexOptions options, out CatalogOffer outOffer)
	{
		CopyOfferByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyOfferByIndexOptionsInternal>(options);
		outOffer = Helper.GetDefault<CatalogOffer>();
		IntPtr outOffer2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyOfferByIndex(base.InnerHandle, ref options2, ref outOffer2);
		options2.Dispose();
		if (Helper.TryMarshal<CatalogOfferInternal, CatalogOffer>(outOffer2, out outOffer))
		{
			EOS_Ecom_CatalogOffer_Release(outOffer2);
		}
		return result;
	}

	public Result CopyOfferById(CopyOfferByIdOptions options, out CatalogOffer outOffer)
	{
		CopyOfferByIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyOfferByIdOptionsInternal>(options);
		outOffer = Helper.GetDefault<CatalogOffer>();
		IntPtr outOffer2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyOfferById(base.InnerHandle, ref options2, ref outOffer2);
		options2.Dispose();
		if (Helper.TryMarshal<CatalogOfferInternal, CatalogOffer>(outOffer2, out outOffer))
		{
			EOS_Ecom_CatalogOffer_Release(outOffer2);
		}
		return result;
	}

	public uint GetOfferItemCount(GetOfferItemCountOptions options)
	{
		GetOfferItemCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetOfferItemCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetOfferItemCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyOfferItemByIndex(CopyOfferItemByIndexOptions options, out CatalogItem outItem)
	{
		CopyOfferItemByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyOfferItemByIndexOptionsInternal>(options);
		outItem = Helper.GetDefault<CatalogItem>();
		IntPtr outItem2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyOfferItemByIndex(base.InnerHandle, ref options2, ref outItem2);
		options2.Dispose();
		if (Helper.TryMarshal<CatalogItemInternal, CatalogItem>(outItem2, out outItem))
		{
			EOS_Ecom_CatalogItem_Release(outItem2);
		}
		return result;
	}

	public Result CopyItemById(CopyItemByIdOptions options, out CatalogItem outItem)
	{
		CopyItemByIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyItemByIdOptionsInternal>(options);
		outItem = Helper.GetDefault<CatalogItem>();
		IntPtr outItem2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyItemById(base.InnerHandle, ref options2, ref outItem2);
		options2.Dispose();
		if (Helper.TryMarshal<CatalogItemInternal, CatalogItem>(outItem2, out outItem))
		{
			EOS_Ecom_CatalogItem_Release(outItem2);
		}
		return result;
	}

	public uint GetOfferImageInfoCount(GetOfferImageInfoCountOptions options)
	{
		GetOfferImageInfoCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetOfferImageInfoCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetOfferImageInfoCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyOfferImageInfoByIndex(CopyOfferImageInfoByIndexOptions options, out KeyImageInfo outImageInfo)
	{
		CopyOfferImageInfoByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyOfferImageInfoByIndexOptionsInternal>(options);
		outImageInfo = Helper.GetDefault<KeyImageInfo>();
		IntPtr outImageInfo2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyOfferImageInfoByIndex(base.InnerHandle, ref options2, ref outImageInfo2);
		options2.Dispose();
		if (Helper.TryMarshal<KeyImageInfoInternal, KeyImageInfo>(outImageInfo2, out outImageInfo))
		{
			EOS_Ecom_KeyImageInfo_Release(outImageInfo2);
		}
		return result;
	}

	public uint GetItemImageInfoCount(GetItemImageInfoCountOptions options)
	{
		GetItemImageInfoCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetItemImageInfoCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetItemImageInfoCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyItemImageInfoByIndex(CopyItemImageInfoByIndexOptions options, out KeyImageInfo outImageInfo)
	{
		CopyItemImageInfoByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyItemImageInfoByIndexOptionsInternal>(options);
		outImageInfo = Helper.GetDefault<KeyImageInfo>();
		IntPtr outImageInfo2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyItemImageInfoByIndex(base.InnerHandle, ref options2, ref outImageInfo2);
		options2.Dispose();
		if (Helper.TryMarshal<KeyImageInfoInternal, KeyImageInfo>(outImageInfo2, out outImageInfo))
		{
			EOS_Ecom_KeyImageInfo_Release(outImageInfo2);
		}
		return result;
	}

	public uint GetItemReleaseCount(GetItemReleaseCountOptions options)
	{
		GetItemReleaseCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetItemReleaseCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetItemReleaseCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyItemReleaseByIndex(CopyItemReleaseByIndexOptions options, out CatalogRelease outRelease)
	{
		CopyItemReleaseByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyItemReleaseByIndexOptionsInternal>(options);
		outRelease = Helper.GetDefault<CatalogRelease>();
		IntPtr outRelease2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyItemReleaseByIndex(base.InnerHandle, ref options2, ref outRelease2);
		options2.Dispose();
		if (Helper.TryMarshal<CatalogReleaseInternal, CatalogRelease>(outRelease2, out outRelease))
		{
			EOS_Ecom_CatalogRelease_Release(outRelease2);
		}
		return result;
	}

	public uint GetTransactionCount(GetTransactionCountOptions options)
	{
		GetTransactionCountOptionsInternal options2 = Helper.CopyPropertiesToNew<GetTransactionCountOptionsInternal>(options);
		uint result = EOS_Ecom_GetTransactionCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyTransactionByIndex(CopyTransactionByIndexOptions options, out Transaction outTransaction)
	{
		CopyTransactionByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyTransactionByIndexOptionsInternal>(options);
		outTransaction = Helper.GetDefault<Transaction>();
		IntPtr outTransaction2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyTransactionByIndex(base.InnerHandle, ref options2, ref outTransaction2);
		options2.Dispose();
		outTransaction = Helper.GetHandle<Transaction>(outTransaction2);
		return result;
	}

	public Result CopyTransactionById(CopyTransactionByIdOptions options, out Transaction outTransaction)
	{
		CopyTransactionByIdOptionsInternal options2 = Helper.CopyPropertiesToNew<CopyTransactionByIdOptionsInternal>(options);
		outTransaction = Helper.GetDefault<Transaction>();
		IntPtr outTransaction2 = IntPtr.Zero;
		Result result = EOS_Ecom_CopyTransactionById(base.InnerHandle, ref options2, ref outTransaction2);
		options2.Dispose();
		outTransaction = Helper.GetHandle<Transaction>(outTransaction2);
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnRedeemEntitlements(IntPtr address)
	{
		OnRedeemEntitlementsCallback callDelegate = null;
		RedeemEntitlementsCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnRedeemEntitlementsCallback, RedeemEntitlementsCallbackInfoInternal, RedeemEntitlementsCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnCheckout(IntPtr address)
	{
		OnCheckoutCallback callDelegate = null;
		CheckoutCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnCheckoutCallback, CheckoutCallbackInfoInternal, CheckoutCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryOffers(IntPtr address)
	{
		OnQueryOffersCallback callDelegate = null;
		QueryOffersCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryOffersCallback, QueryOffersCallbackInfoInternal, QueryOffersCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryEntitlements(IntPtr address)
	{
		OnQueryEntitlementsCallback callDelegate = null;
		QueryEntitlementsCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryEntitlementsCallback, QueryEntitlementsCallbackInfoInternal, QueryEntitlementsCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryOwnershipToken(IntPtr address)
	{
		OnQueryOwnershipTokenCallback callDelegate = null;
		QueryOwnershipTokenCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryOwnershipTokenCallback, QueryOwnershipTokenCallbackInfoInternal, QueryOwnershipTokenCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnQueryOwnership(IntPtr address)
	{
		OnQueryOwnershipCallback callDelegate = null;
		QueryOwnershipCallbackInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryOwnershipCallback, QueryOwnershipCallbackInfoInternal, QueryOwnershipCallbackInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_CatalogRelease_Release(IntPtr catalogRelease);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_KeyImageInfo_Release(IntPtr keyImageInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_CatalogOffer_Release(IntPtr catalogOffer);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_CatalogItem_Release(IntPtr catalogItem);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_Entitlement_Release(IntPtr entitlement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyTransactionById(IntPtr handle, ref CopyTransactionByIdOptionsInternal options, ref IntPtr outTransaction);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyTransactionByIndex(IntPtr handle, ref CopyTransactionByIndexOptionsInternal options, ref IntPtr outTransaction);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetTransactionCount(IntPtr handle, ref GetTransactionCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyItemReleaseByIndex(IntPtr handle, ref CopyItemReleaseByIndexOptionsInternal options, ref IntPtr outRelease);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetItemReleaseCount(IntPtr handle, ref GetItemReleaseCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyItemImageInfoByIndex(IntPtr handle, ref CopyItemImageInfoByIndexOptionsInternal options, ref IntPtr outImageInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetItemImageInfoCount(IntPtr handle, ref GetItemImageInfoCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyOfferImageInfoByIndex(IntPtr handle, ref CopyOfferImageInfoByIndexOptionsInternal options, ref IntPtr outImageInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetOfferImageInfoCount(IntPtr handle, ref GetOfferImageInfoCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyItemById(IntPtr handle, ref CopyItemByIdOptionsInternal options, ref IntPtr outItem);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyOfferItemByIndex(IntPtr handle, ref CopyOfferItemByIndexOptionsInternal options, ref IntPtr outItem);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetOfferItemCount(IntPtr handle, ref GetOfferItemCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyOfferById(IntPtr handle, ref CopyOfferByIdOptionsInternal options, ref IntPtr outOffer);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyOfferByIndex(IntPtr handle, ref CopyOfferByIndexOptionsInternal options, ref IntPtr outOffer);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetOfferCount(IntPtr handle, ref GetOfferCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyEntitlementById(IntPtr handle, ref CopyEntitlementByIdOptionsInternal options, ref IntPtr outEntitlement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyEntitlementByNameAndIndex(IntPtr handle, ref CopyEntitlementByNameAndIndexOptionsInternal options, ref IntPtr outEntitlement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_Ecom_CopyEntitlementByIndex(IntPtr handle, ref CopyEntitlementByIndexOptionsInternal options, ref IntPtr outEntitlement);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetEntitlementsByNameCount(IntPtr handle, ref GetEntitlementsByNameCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_Ecom_GetEntitlementsCount(IntPtr handle, ref GetEntitlementsCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_RedeemEntitlements(IntPtr handle, ref RedeemEntitlementsOptionsInternal options, IntPtr clientData, OnRedeemEntitlementsCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_Checkout(IntPtr handle, ref CheckoutOptionsInternal options, IntPtr clientData, OnCheckoutCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_QueryOffers(IntPtr handle, ref QueryOffersOptionsInternal options, IntPtr clientData, OnQueryOffersCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_QueryEntitlements(IntPtr handle, ref QueryEntitlementsOptionsInternal options, IntPtr clientData, OnQueryEntitlementsCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_QueryOwnershipToken(IntPtr handle, ref QueryOwnershipTokenOptionsInternal options, IntPtr clientData, OnQueryOwnershipTokenCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Ecom_QueryOwnership(IntPtr handle, ref QueryOwnershipOptionsInternal options, IntPtr clientData, OnQueryOwnershipCallbackInternal completionDelegate);
}
