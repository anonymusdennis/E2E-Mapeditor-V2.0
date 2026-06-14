using System;
using System.Collections.Generic;

[Serializable]
public class CustomisationCategorisedSet
{
	public List<UnlockCategories> categories = new List<UnlockCategories>();

	public List<CustomisationSet> sets = new List<CustomisationSet>();

	public List<CustomisationData.BodyType>[] bodyTypes = new List<CustomisationData.BodyType>[0];

	public List<CustomisationData.SkinColour>[] skinColours = new List<CustomisationData.SkinColour>[0];

	public List<CustomisationData.Hair>[] hairs = new List<CustomisationData.Hair>[0];

	public List<CustomisationData.Hat>[] hats = new List<CustomisationData.Hat>[0];

	public List<CustomisationData.UpperFaceAccessory>[] upperFaces = new List<CustomisationData.UpperFaceAccessory>[0];

	public List<CustomisationData.LowerFaceAccessory>[] lowerFaces = new List<CustomisationData.LowerFaceAccessory>[0];
}
