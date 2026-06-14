using System;
using ExitGames.Client.Photon;

public static class T17CustomTypes
{
	private static bool m_bAlreadyRegistered = false;

	public static readonly byte[] uLongBuffer = new byte[8];

	public static void Register()
	{
		if (!m_bAlreadyRegistered)
		{
			m_bAlreadyRegistered = true;
			PhotonPeer.RegisterType(typeof(ulong), 85, SerialiseULong, DeserialiseULong);
		}
	}

	private static short SerialiseULong(StreamBuffer outStream, object customobject)
	{
		ulong value = (ulong)customobject;
		outStream.Write(BitConverter.GetBytes(value), 0, 8);
		return 8;
	}

	private static object DeserialiseULong(StreamBuffer inStream, short length)
	{
		ulong num = 0uL;
		lock (uLongBuffer)
		{
			inStream.Read(uLongBuffer, 0, 8);
			num = BitConverter.ToUInt64(uLongBuffer, 0);
		}
		return num;
	}
}
