public class CarryObjectInteractionDeserialiser : IDeserializable
{
	public bool Deserialize(string data, ref string error)
	{
		return CarryObjectInteraction.DeserializeAll(data, ref error);
	}

	public string GetSerializationData()
	{
		return NetPrisonViewDetails.Instance.CarriedObjectsData;
	}
}
