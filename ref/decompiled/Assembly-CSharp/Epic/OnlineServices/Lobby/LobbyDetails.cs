using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby;

public sealed class LobbyDetails : Handle
{
	public LobbyDetails()
		: base(IntPtr.Zero)
	{
	}

	public LobbyDetails(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public ProductUserId GetLobbyOwner(LobbyDetailsGetLobbyOwnerOptions options)
	{
		LobbyDetailsGetLobbyOwnerOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsGetLobbyOwnerOptionsInternal>(options);
		IntPtr innerHandle = EOS_LobbyDetails_GetLobbyOwner(base.InnerHandle, ref options2);
		options2.Dispose();
		return Helper.GetHandle<ProductUserId>(innerHandle);
	}

	public Result CopyInfo(LobbyDetailsCopyInfoOptions options, out LobbyDetailsInfo outLobbyDetailsInfo)
	{
		LobbyDetailsCopyInfoOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsCopyInfoOptionsInternal>(options);
		outLobbyDetailsInfo = Helper.GetDefault<LobbyDetailsInfo>();
		IntPtr outLobbyDetailsInfo2 = IntPtr.Zero;
		Result result = EOS_LobbyDetails_CopyInfo(base.InnerHandle, ref options2, ref outLobbyDetailsInfo2);
		options2.Dispose();
		if (Helper.TryMarshal<LobbyDetailsInfoInternal, LobbyDetailsInfo>(outLobbyDetailsInfo2, out outLobbyDetailsInfo))
		{
			EOS_LobbyDetails_Info_Release(outLobbyDetailsInfo2);
		}
		return result;
	}

	public uint GetAttributeCount(LobbyDetailsGetAttributeCountOptions options)
	{
		LobbyDetailsGetAttributeCountOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsGetAttributeCountOptionsInternal>(options);
		uint result = EOS_LobbyDetails_GetAttributeCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyAttributeByIndex(LobbyDetailsCopyAttributeByIndexOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyAttributeByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsCopyAttributeByIndexOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result result = EOS_LobbyDetails_CopyAttributeByIndex(base.InnerHandle, ref options2, ref outAttribute2);
		options2.Dispose();
		if (Helper.TryMarshal<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		return result;
	}

	public Result CopyAttributeByKey(LobbyDetailsCopyAttributeByKeyOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyAttributeByKeyOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsCopyAttributeByKeyOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result result = EOS_LobbyDetails_CopyAttributeByKey(base.InnerHandle, ref options2, ref outAttribute2);
		options2.Dispose();
		if (Helper.TryMarshal<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		return result;
	}

	public uint GetMemberCount(LobbyDetailsGetMemberCountOptions options)
	{
		LobbyDetailsGetMemberCountOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsGetMemberCountOptionsInternal>(options);
		uint result = EOS_LobbyDetails_GetMemberCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public ProductUserId GetMemberByIndex(LobbyDetailsGetMemberByIndexOptions options)
	{
		LobbyDetailsGetMemberByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsGetMemberByIndexOptionsInternal>(options);
		IntPtr innerHandle = EOS_LobbyDetails_GetMemberByIndex(base.InnerHandle, ref options2);
		options2.Dispose();
		return Helper.GetHandle<ProductUserId>(innerHandle);
	}

	public uint GetMemberAttributeCount(LobbyDetailsGetMemberAttributeCountOptions options)
	{
		LobbyDetailsGetMemberAttributeCountOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsGetMemberAttributeCountOptionsInternal>(options);
		uint result = EOS_LobbyDetails_GetMemberAttributeCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyMemberAttributeByIndex(LobbyDetailsCopyMemberAttributeByIndexOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyMemberAttributeByIndexOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsCopyMemberAttributeByIndexOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result result = EOS_LobbyDetails_CopyMemberAttributeByIndex(base.InnerHandle, ref options2, ref outAttribute2);
		options2.Dispose();
		if (Helper.TryMarshal<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		return result;
	}

	public Result CopyMemberAttributeByKey(LobbyDetailsCopyMemberAttributeByKeyOptions options, out Attribute outAttribute)
	{
		LobbyDetailsCopyMemberAttributeByKeyOptionsInternal options2 = Helper.CopyPropertiesToNew<LobbyDetailsCopyMemberAttributeByKeyOptionsInternal>(options);
		outAttribute = Helper.GetDefault<Attribute>();
		IntPtr outAttribute2 = IntPtr.Zero;
		Result result = EOS_LobbyDetails_CopyMemberAttributeByKey(base.InnerHandle, ref options2, ref outAttribute2);
		options2.Dispose();
		if (Helper.TryMarshal<AttributeInternal, Attribute>(outAttribute2, out outAttribute))
		{
			EOS_Lobby_Attribute_Release(outAttribute2);
		}
		return result;
	}

	public void Release()
	{
		EOS_LobbyDetails_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_Lobby_Attribute_Release(IntPtr lobbyAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_LobbyDetails_Info_Release(IntPtr lobbyDetailsInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_LobbyDetails_Release(IntPtr lobbyHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyMemberAttributeByKey(IntPtr handle, ref LobbyDetailsCopyMemberAttributeByKeyOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyMemberAttributeByIndex(IntPtr handle, ref LobbyDetailsCopyMemberAttributeByIndexOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_LobbyDetails_GetMemberAttributeCount(IntPtr handle, ref LobbyDetailsGetMemberAttributeCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_LobbyDetails_GetMemberByIndex(IntPtr handle, ref LobbyDetailsGetMemberByIndexOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_LobbyDetails_GetMemberCount(IntPtr handle, ref LobbyDetailsGetMemberCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyAttributeByKey(IntPtr handle, ref LobbyDetailsCopyAttributeByKeyOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyAttributeByIndex(IntPtr handle, ref LobbyDetailsCopyAttributeByIndexOptionsInternal options, ref IntPtr outAttribute);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern uint EOS_LobbyDetails_GetAttributeCount(IntPtr handle, ref LobbyDetailsGetAttributeCountOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_LobbyDetails_CopyInfo(IntPtr handle, ref LobbyDetailsCopyInfoOptionsInternal options, ref IntPtr outLobbyDetailsInfo);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern IntPtr EOS_LobbyDetails_GetLobbyOwner(IntPtr handle, ref LobbyDetailsGetLobbyOwnerOptionsInternal options);
}
