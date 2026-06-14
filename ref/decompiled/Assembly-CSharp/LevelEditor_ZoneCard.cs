using UnityEngine;
using UnityEngine.UI;

public class LevelEditor_ZoneCard : MonoBehaviour
{
	[SerializeField]
	protected RawImage m_ZoneIcon;

	[SerializeField]
	protected RawImage m_ZoneBacker;

	[SerializeField]
	protected T17Text m_ZoneText;

	[SerializeField]
	protected ZoneIconBackerData m_ZoneIconBackerData;

	public virtual void SetCardDataForZone(LevelEditor_ZoneManager.Zone newZone)
	{
		if (newZone == null)
		{
			m_ZoneText.text = string.Empty;
		}
		else
		{
			SetCardDataFromDetails(newZone.m_ZoneDetails, newZone.IsFullyValid());
		}
	}

	public virtual void SetCardDataFromDetails(ZoneDetailsManager.ZoneDetails newZoneDetails, bool bisValid)
	{
		if (newZoneDetails != null)
		{
			if (bisValid)
			{
				Material zoneImage = newZoneDetails.m_ZoneImage;
				m_ZoneIcon.texture = newZoneDetails.m_ZoneImage.mainTexture;
				Rect uvRect = new Rect(zoneImage.GetTextureOffset("_MainTex"), zoneImage.GetTextureScale("_MainTex"));
				m_ZoneIcon.uvRect = uvRect;
				m_ZoneBacker.texture = m_ZoneIconBackerData.ZoneIconBacker_Valid.mainTexture;
				uvRect = new Rect(m_ZoneIconBackerData.ZoneIconBacker_Valid.mainTextureOffset, m_ZoneIconBackerData.ZoneIconBacker_Valid.mainTextureScale);
				m_ZoneBacker.uvRect = uvRect;
			}
			else
			{
				Material zoneImageInvalid = newZoneDetails.m_ZoneImageInvalid;
				m_ZoneIcon.texture = newZoneDetails.m_ZoneImageInvalid.mainTexture;
				Rect uvRect2 = new Rect(zoneImageInvalid.GetTextureOffset("_MainTex"), zoneImageInvalid.GetTextureScale("_MainTex"));
				m_ZoneIcon.uvRect = uvRect2;
				m_ZoneBacker.texture = m_ZoneIconBackerData.ZoneIconBacker_Invalid.mainTexture;
				uvRect2 = new Rect(m_ZoneIconBackerData.ZoneIconBacker_Invalid.mainTextureOffset, m_ZoneIconBackerData.ZoneIconBacker_Invalid.mainTextureScale);
				m_ZoneBacker.uvRect = uvRect2;
			}
			m_ZoneText.SetNonLocalizedText(newZoneDetails.GetZoneNameText());
		}
	}
}
