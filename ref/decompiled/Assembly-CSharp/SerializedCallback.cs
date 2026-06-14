public class SerializedCallback
{
	private SerializedCallbackManager.OnSerializeView OnSerializeView;

	private byte Index;

	public SerializedCallback(byte index, SerializedCallbackManager.OnSerializeView callback)
	{
		Index = index;
		OnSerializeView = callback;
	}

	public void WriteSerialize(PhotonStream stream, ref PhotonMessageInfo info)
	{
		stream.SendNext(Index);
		OnSerializeView(stream, ref info);
	}

	public void ReadSerialize(PhotonStream stream, ref PhotonMessageInfo info)
	{
		OnSerializeView(stream, ref info);
	}
}
