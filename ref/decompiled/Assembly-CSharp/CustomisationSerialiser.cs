using SaveHelpers;
using UnityEngine;

public class CustomisationSerialiser
{
	public static string SerializeCustomisation_ToJSON(Customisation customisation)
	{
		return JsonUtility.ToJson(SerializeCustomisation_Internal(customisation));
	}

	public static Customisation DeserializeCustomisation_FromJSON(string serializedData)
	{
		CustomisationNetData customisationNetData = JsonUtility.FromJson<CustomisationNetData>(serializedData);
		if (customisationNetData == null)
		{
			return null;
		}
		return DeserializeCustomisation_Internal(customisationNetData);
	}

	public static string SerialiseCustomisations_ToJSON(Customisation[] customisations)
	{
		CustomisationCollectionNetData obj = SerialiseCustomisations(customisations);
		return JsonUtility.ToJson(obj);
	}

	public static Customisation[] DeserialiseCustomisations_FromJSON(string data)
	{
		CustomisationCollectionNetData customisationCollectionNetData = JsonUtility.FromJson<CustomisationCollectionNetData>(data);
		if (customisationCollectionNetData == null || customisationCollectionNetData.data == null)
		{
			return null;
		}
		return DeserialiseCustomisations(customisationCollectionNetData.data);
	}

	public static CustomisationCollectionNetData SerialiseCustomisations(Customisation[] customisations)
	{
		CustomisationCollectionNetData customisationCollectionNetData = new CustomisationCollectionNetData();
		if (customisations != null && customisations.Length > 0)
		{
			customisationCollectionNetData.data = new CustomisationNetData[customisations.Length];
			for (int i = 0; i < customisations.Length; i++)
			{
				customisationCollectionNetData.data[i] = SerializeCustomisation_Internal(customisations[i]);
			}
		}
		return customisationCollectionNetData;
	}

	public static Customisation[] DeserialiseCustomisations(CustomisationNetData[] data)
	{
		Customisation[] array = new Customisation[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = DeserializeCustomisation_Internal(data[i]);
		}
		return array;
	}

	private static CustomisationNetData SerializeCustomisation_Internal(Customisation customisation)
	{
		CustomisationNetData customisationNetData = new CustomisationNetData();
		customisationNetData.name = customisation.name;
		customisationNetData.safeName = customisation.safeName;
		customisationNetData.prefixKey = customisation.namePrefixKey;
		customisationNetData.bForceSafeName = customisation.bUseSafeName;
		customisationNetData.filteredName = customisation.filteredName;
		BitField bitField = new BitField();
		bitField.Set(4, (uint)customisation.body);
		bitField.Set(4, (uint)customisation.skin);
		bitField.Set(10, (uint)customisation.hair);
		bitField.Set(8, (uint)customisation.hat);
		bitField.Set(7, (uint)customisation.upperFace);
		bitField.Set(7, (uint)customisation.lowerFace);
		bitField.Set(7, (uint)customisation.defaultOutfit);
		customisationNetData.appearance = (long)bitField;
		return customisationNetData;
	}

	private static Customisation DeserializeCustomisation_Internal(CustomisationNetData data)
	{
		Customisation customisation = new Customisation();
		customisation.name = data.name;
		customisation.safeName = data.safeName;
		customisation.namePrefixKey = data.prefixKey;
		customisation.bUseSafeName = data.bForceSafeName;
		if (string.IsNullOrEmpty(data.filteredName))
		{
			customisation.filteredName = string.Empty;
		}
		BitField bitField = new BitField((ulong)data.appearance);
		customisation.body = (CustomisationData.BodyType)bitField.GetUInt(4);
		customisation.skin = (CustomisationData.SkinColour)bitField.GetUInt(4);
		customisation.hair = (CustomisationData.Hair)bitField.GetUInt(10);
		customisation.hat = (CustomisationData.Hat)bitField.GetUInt(8);
		customisation.upperFace = (CustomisationData.UpperFaceAccessory)bitField.GetUInt(7);
		customisation.lowerFace = (CustomisationData.LowerFaceAccessory)bitField.GetUInt(7);
		customisation.defaultOutfit = (CustomisationData.Outfit)bitField.GetUInt(7);
		return customisation;
	}
}
