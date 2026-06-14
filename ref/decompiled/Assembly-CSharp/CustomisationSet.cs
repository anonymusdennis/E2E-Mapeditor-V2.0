using System;
using System.Collections.Generic;

[Serializable]
public class CustomisationSet
{
	public List<CustomisationData.BodyType> bodyTypes = new List<CustomisationData.BodyType>();

	public List<CustomisationData.SkinColour> skinColours = new List<CustomisationData.SkinColour>();

	public List<CustomisationData.Hair> hairs = new List<CustomisationData.Hair>();

	public List<CustomisationData.Hat> hats = new List<CustomisationData.Hat>();

	public List<CustomisationData.UpperFaceAccessory> upperFaces = new List<CustomisationData.UpperFaceAccessory>();

	public List<CustomisationData.LowerFaceAccessory> lowerFaces = new List<CustomisationData.LowerFaceAccessory>();

	public int count => bodyTypes.Count + skinColours.Count + hairs.Count + hats.Count + upperFaces.Count + lowerFaces.Count;

	public void CopyData(CustomisationSet other)
	{
		Clear();
		bodyTypes.AddRange(other.bodyTypes);
		skinColours.AddRange(other.skinColours);
		hairs.AddRange(other.hairs);
		hats.AddRange(other.hats);
		upperFaces.AddRange(other.upperFaces);
		lowerFaces.AddRange(other.lowerFaces);
	}

	public void Clear()
	{
		bodyTypes.Clear();
		skinColours.Clear();
		hairs.Clear();
		hats.Clear();
		upperFaces.Clear();
		lowerFaces.Clear();
	}
}
