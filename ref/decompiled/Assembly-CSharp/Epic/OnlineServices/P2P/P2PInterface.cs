using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

public sealed class P2PInterface : Handle
{
	public P2PInterface()
		: base(IntPtr.Zero)
	{
	}

	public P2PInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SendPacket(SendPacketOptions options)
	{
		SendPacketOptionsInternal options2 = Helper.CopyPropertiesToNew<SendPacketOptionsInternal>(options);
		Result result = EOS_P2P_SendPacket(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result GetNextReceivedPacketSize(GetNextReceivedPacketSizeOptions options, out uint outPacketSizeBytes)
	{
		GetNextReceivedPacketSizeOptionsInternal options2 = Helper.CopyPropertiesToNew<GetNextReceivedPacketSizeOptionsInternal>(options);
		outPacketSizeBytes = Helper.GetDefault<uint>();
		Result result = EOS_P2P_GetNextReceivedPacketSize(base.InnerHandle, ref options2, ref outPacketSizeBytes);
		options2.Dispose();
		return result;
	}

	public Result ReceivePacket(ReceivePacketOptions options, out ProductUserId outPeerId, out SocketId outSocketId, out byte outChannel, ref byte[] outData, out uint outBytesWritten)
	{
		ReceivePacketOptionsInternal options2 = Helper.CopyPropertiesToNew<ReceivePacketOptionsInternal>(options);
		outPeerId = Helper.GetDefault<ProductUserId>();
		IntPtr outPeerId2 = IntPtr.Zero;
		outSocketId = Helper.GetDefault<SocketId>();
		SocketIdInternal outSocketId2 = default(SocketIdInternal);
		outChannel = Helper.GetDefault<byte>();
		outBytesWritten = Helper.GetDefault<uint>();
		Result result = EOS_P2P_ReceivePacket(base.InnerHandle, ref options2, ref outPeerId2, ref outSocketId2, ref outChannel, outData, ref outBytesWritten);
		options2.Dispose();
		outPeerId = Helper.GetHandle<ProductUserId>(outPeerId2);
		outSocketId = new SocketId();
		Helper.CopyProperties(outSocketId2, outSocketId);
		outSocketId2.Dispose();
		return result;
	}

	public ulong AddNotifyPeerConnectionRequest(AddNotifyPeerConnectionRequestOptions options, object clientData, OnIncomingConnectionRequestCallback connectionRequestHandler)
	{
		AddNotifyPeerConnectionRequestOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyPeerConnectionRequestOptionsInternal>(options);
		OnIncomingConnectionRequestCallbackInternal onIncomingConnectionRequestCallbackInternal = OnIncomingConnectionRequest;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, connectionRequestHandler, onIncomingConnectionRequestCallbackInternal);
		ulong result = EOS_P2P_AddNotifyPeerConnectionRequest(base.InnerHandle, ref options2, clientDataAddress, onIncomingConnectionRequestCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyPeerConnectionRequest(ulong notificationId)
	{
		EOS_P2P_RemoveNotifyPeerConnectionRequest(base.InnerHandle, notificationId);
	}

	public ulong AddNotifyPeerConnectionClosed(AddNotifyPeerConnectionClosedOptions options, object clientData, OnRemoteConnectionClosedCallback connectionClosedHandler)
	{
		AddNotifyPeerConnectionClosedOptionsInternal options2 = Helper.CopyPropertiesToNew<AddNotifyPeerConnectionClosedOptionsInternal>(options);
		OnRemoteConnectionClosedCallbackInternal onRemoteConnectionClosedCallbackInternal = OnRemoteConnectionClosed;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, connectionClosedHandler, onRemoteConnectionClosedCallbackInternal);
		ulong result = EOS_P2P_AddNotifyPeerConnectionClosed(base.InnerHandle, ref options2, clientDataAddress, onRemoteConnectionClosedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyPeerConnectionClosed(ulong notificationId)
	{
		EOS_P2P_RemoveNotifyPeerConnectionClosed(base.InnerHandle, notificationId);
	}

	public Result AcceptConnection(AcceptConnectionOptions options)
	{
		AcceptConnectionOptionsInternal options2 = Helper.CopyPropertiesToNew<AcceptConnectionOptionsInternal>(options);
		Result result = EOS_P2P_AcceptConnection(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CloseConnection(CloseConnectionOptions options)
	{
		CloseConnectionOptionsInternal options2 = Helper.CopyPropertiesToNew<CloseConnectionOptionsInternal>(options);
		Result result = EOS_P2P_CloseConnection(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CloseConnections(CloseConnectionsOptions options)
	{
		CloseConnectionsOptionsInternal options2 = Helper.CopyPropertiesToNew<CloseConnectionsOptionsInternal>(options);
		Result result = EOS_P2P_CloseConnections(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void QueryNATType(QueryNATTypeOptions options, object clientData, OnQueryNATTypeCompleteCallback nATTypeQueriedHandler)
	{
		QueryNATTypeOptionsInternal options2 = Helper.CopyPropertiesToNew<QueryNATTypeOptionsInternal>(options);
		OnQueryNATTypeCompleteCallbackInternal onQueryNATTypeCompleteCallbackInternal = OnQueryNATTypeComplete;
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, clientData, nATTypeQueriedHandler, onQueryNATTypeCompleteCallbackInternal);
		EOS_P2P_QueryNATType(base.InnerHandle, ref options2, clientDataAddress, onQueryNATTypeCompleteCallbackInternal);
		options2.Dispose();
	}

	public Result GetNATType(GetNATTypeOptions options, out NATType outNATType)
	{
		GetNATTypeOptionsInternal options2 = Helper.CopyPropertiesToNew<GetNATTypeOptionsInternal>(options);
		outNATType = Helper.GetDefault<NATType>();
		Result result = EOS_P2P_GetNATType(base.InnerHandle, ref options2, ref outNATType);
		options2.Dispose();
		return result;
	}

	[MonoPInvokeCallback]
	internal static void OnQueryNATTypeComplete(IntPtr address)
	{
		OnQueryNATTypeCompleteCallback callDelegate = null;
		OnQueryNATTypeCompleteInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnQueryNATTypeCompleteCallback, OnQueryNATTypeCompleteInfoInternal, OnQueryNATTypeCompleteInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnRemoteConnectionClosed(IntPtr address)
	{
		OnRemoteConnectionClosedCallback callDelegate = null;
		OnRemoteConnectionClosedInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnRemoteConnectionClosedCallback, OnRemoteConnectionClosedInfoInternal, OnRemoteConnectionClosedInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[MonoPInvokeCallback]
	internal static void OnIncomingConnectionRequest(IntPtr address)
	{
		OnIncomingConnectionRequestCallback callDelegate = null;
		OnIncomingConnectionRequestInfo callbackInfo = null;
		if (Helper.TryGetAndRemovePublicCallDelegate<OnIncomingConnectionRequestCallback, OnIncomingConnectionRequestInfoInternal, OnIncomingConnectionRequestInfo>(address, out callDelegate, out callbackInfo))
		{
			callDelegate(callbackInfo);
		}
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_P2P_GetNATType(IntPtr handle, ref GetNATTypeOptionsInternal options, ref NATType outNATType);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_P2P_QueryNATType(IntPtr handle, ref QueryNATTypeOptionsInternal options, IntPtr clientData, OnQueryNATTypeCompleteCallbackInternal nATTypeQueriedHandler);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_P2P_CloseConnections(IntPtr handle, ref CloseConnectionsOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_P2P_CloseConnection(IntPtr handle, ref CloseConnectionOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_P2P_AcceptConnection(IntPtr handle, ref AcceptConnectionOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_P2P_RemoveNotifyPeerConnectionClosed(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_P2P_AddNotifyPeerConnectionClosed(IntPtr handle, ref AddNotifyPeerConnectionClosedOptionsInternal options, IntPtr clientData, OnRemoteConnectionClosedCallbackInternal connectionClosedHandler);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_P2P_RemoveNotifyPeerConnectionRequest(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern ulong EOS_P2P_AddNotifyPeerConnectionRequest(IntPtr handle, ref AddNotifyPeerConnectionRequestOptionsInternal options, IntPtr clientData, OnIncomingConnectionRequestCallbackInternal connectionRequestHandler);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_P2P_ReceivePacket(IntPtr handle, ref ReceivePacketOptionsInternal options, ref IntPtr outPeerId, ref SocketIdInternal outSocketId, ref byte outChannel, byte[] outData, ref uint outBytesWritten);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_P2P_GetNextReceivedPacketSize(IntPtr handle, ref GetNextReceivedPacketSizeOptionsInternal options, ref uint outPacketSizeBytes);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_P2P_SendPacket(IntPtr handle, ref SendPacketOptionsInternal options);
}
