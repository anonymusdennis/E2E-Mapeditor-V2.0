using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

public sealed class PresenceModification : Handle
{
	public PresenceModification()
		: base(IntPtr.Zero)
	{
	}

	public PresenceModification(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetStatus(PresenceModificationSetStatusOptions options)
	{
		PresenceModificationSetStatusOptionsInternal options2 = Helper.CopyPropertiesToNew<PresenceModificationSetStatusOptionsInternal>(options);
		Result result = EOS_PresenceModification_SetStatus(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetRawRichText(PresenceModificationSetRawRichTextOptions options)
	{
		PresenceModificationSetRawRichTextOptionsInternal options2 = Helper.CopyPropertiesToNew<PresenceModificationSetRawRichTextOptionsInternal>(options);
		Result result = EOS_PresenceModification_SetRawRichText(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetData(PresenceModificationSetDataOptions options)
	{
		PresenceModificationSetDataOptionsInternal options2 = Helper.CopyPropertiesToNew<PresenceModificationSetDataOptionsInternal>(options);
		Result result = EOS_PresenceModification_SetData(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result DeleteData(PresenceModificationDeleteDataOptions options)
	{
		PresenceModificationDeleteDataOptionsInternal options2 = Helper.CopyPropertiesToNew<PresenceModificationDeleteDataOptionsInternal>(options);
		Result result = EOS_PresenceModification_DeleteData(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetJoinInfo(PresenceModificationSetJoinInfoOptions options)
	{
		PresenceModificationSetJoinInfoOptionsInternal options2 = Helper.CopyPropertiesToNew<PresenceModificationSetJoinInfoOptionsInternal>(options);
		Result result = EOS_PresenceModification_SetJoinInfo(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void Release()
	{
		EOS_PresenceModification_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern void EOS_PresenceModification_Release(IntPtr presenceModificationHandle);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PresenceModification_SetJoinInfo(IntPtr handle, ref PresenceModificationSetJoinInfoOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PresenceModification_DeleteData(IntPtr handle, ref PresenceModificationDeleteDataOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PresenceModification_SetData(IntPtr handle, ref PresenceModificationSetDataOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PresenceModification_SetRawRichText(IntPtr handle, ref PresenceModificationSetRawRichTextOptionsInternal options);

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_PresenceModification_SetStatus(IntPtr handle, ref PresenceModificationSetStatusOptionsInternal options);
}
