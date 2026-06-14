public class ToiletInteractionDeserializer : IDeserializable
{
	public bool Deserialize(string data, ref string error)
	{
		return ToiletInteraction.GlobalDeserialize(data, ref error);
	}

	public string GetSerializationData()
	{
		return NetPrisonViewDetails.Instance.ToiletInteractionData;
	}
}
