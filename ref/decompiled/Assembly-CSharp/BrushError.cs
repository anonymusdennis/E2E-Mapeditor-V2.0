using System;

[Serializable]
public struct BrushError
{
	[ReadOnly]
	public BaseLevelManager.BrushError m_BrushError;

	public string Description;
}
