internal interface ISaveableTileComponent
{
	bool RequiresSaving();

	string SerializeData();

	void DeserializeData(string data);
}
