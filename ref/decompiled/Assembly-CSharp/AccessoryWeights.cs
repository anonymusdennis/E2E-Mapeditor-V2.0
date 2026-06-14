using System;

[Serializable]
public class AccessoryWeights
{
	public float hats;

	public float upperFaces;

	public float lowerFaces;

	public CustomisationData.Hat defaultHat;

	public CustomisationData.UpperFaceAccessory defaultUpperFace;

	public CustomisationData.LowerFaceAccessory defaultLowerFace;

	public void CopyData(AccessoryWeights other)
	{
		Clear();
		hats = other.hats;
		upperFaces = other.upperFaces;
		lowerFaces = other.lowerFaces;
		defaultHat = other.defaultHat;
		defaultUpperFace = other.defaultUpperFace;
		defaultLowerFace = other.defaultLowerFace;
	}

	public void Clear()
	{
		hats = 0f;
		upperFaces = 0f;
		lowerFaces = 0f;
		defaultHat = CustomisationData.Hat.NONE;
		defaultUpperFace = CustomisationData.UpperFaceAccessory.NONE;
		defaultLowerFace = CustomisationData.LowerFaceAccessory.NONE;
	}
}
