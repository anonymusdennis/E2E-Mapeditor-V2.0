public interface IDeserializable
{
	string GetSerializationData();

	bool Deserialize(string data, ref string error);
}
