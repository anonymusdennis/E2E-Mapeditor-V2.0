using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices;

public sealed class ByteArray : Handle
{
	public ByteArray()
		: base(IntPtr.Zero)
	{
	}

	public ByteArray(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public static Result ToString(byte byteArray, uint length, StringBuilder outBuffer, ref uint inOutBufferLength)
	{
		return EOS_ByteArray_ToString(ref byteArray, length, outBuffer, ref inOutBufferLength);
	}

	[DllImport("EOSSDK-Win32-Shipping")]
	private static extern Result EOS_ByteArray_ToString(ref byte byteArray, uint length, StringBuilder outBuffer, ref uint inOutBufferLength);
}
