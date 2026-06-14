using UnityEngine;

public class NameTagIconHud : MonoBehaviour
{
	public NamePlateHUD m_NamePlateHUD;

	public AnimatingImage m_Icon;

	public void CopyFrom(IconDisplayHUD displayHud, NamePlateHUD plateToCopy)
	{
		CopyNamePlate(plateToCopy);
		CopyIcon(displayHud);
	}

	public void CopyNamePlate(NamePlateHUD plateToCopy)
	{
		m_NamePlateHUD.CopyFrom(plateToCopy);
	}

	public void CopyIcon(IconDisplayHUD displayHud)
	{
		m_Icon.CopyFrom(displayHud.m_AnimatingImage);
	}
}
