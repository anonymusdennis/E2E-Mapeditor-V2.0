using System;

[Serializable]
public class Customisation
{
	public string namePrefixKey = string.Empty;

	public string name = string.Empty;

	public string safeName = string.Empty;

	public bool bUseSafeName;

	public string filteredName = string.Empty;

	public CustomisationData.BodyType body = CustomisationData.BodyType.NULL;

	public CustomisationData.SkinColour skin = CustomisationData.SkinColour.NULL;

	public CustomisationData.Hair hair = CustomisationData.Hair.NULL;

	public CustomisationData.Hat hat = CustomisationData.Hat.NULL;

	public CustomisationData.UpperFaceAccessory upperFace = CustomisationData.UpperFaceAccessory.NULL;

	public CustomisationData.LowerFaceAccessory lowerFace = CustomisationData.LowerFaceAccessory.NULL;

	public CustomisationData.Outfit defaultOutfit = CustomisationData.Outfit.NULL;

	public string FinalName
	{
		get
		{
			if (!GlobalStart.GetInstance().ProfanityFilterEnabled)
			{
				return name;
			}
			if (string.IsNullOrEmpty(filteredName))
			{
				filteredName = name;
				Platform.GetInstance().FilterString(ref filteredName);
			}
			return filteredName;
		}
	}

	public Customisation()
	{
	}

	public Customisation(Customisation other, bool takeFilteredName = true)
	{
		namePrefixKey = other.namePrefixKey;
		name = other.name;
		safeName = other.safeName;
		body = other.body;
		skin = other.skin;
		bUseSafeName = other.bUseSafeName;
		if (takeFilteredName)
		{
			filteredName = other.filteredName;
		}
		hair = other.hair;
		hat = other.hat;
		upperFace = other.upperFace;
		lowerFace = other.lowerFace;
		defaultOutfit = other.defaultOutfit;
	}

	public void DuplicateCustomisation(Customisation other)
	{
		namePrefixKey = other.namePrefixKey;
		name = other.name;
		safeName = other.safeName;
		body = other.body;
		skin = other.skin;
		bUseSafeName = other.bUseSafeName;
		filteredName = other.filteredName;
		hair = other.hair;
		hat = other.hat;
		upperFace = other.upperFace;
		lowerFace = other.lowerFace;
		defaultOutfit = other.defaultOutfit;
	}
}
