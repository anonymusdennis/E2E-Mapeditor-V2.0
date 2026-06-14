using UnityEngine;

public class PhotonRegionOptionSelector : MonoBehaviour
{
	public class LookupData
	{
		public float m_OptionValue;

		public CloudRegionCode m_PhotonRegion;

		public string m_Description;

		public LookupData(float optionValue, CloudRegionCode photonRegion, string description)
		{
			m_OptionValue = optionValue;
			m_PhotonRegion = photonRegion;
			m_Description = description;
		}
	}

	public const float m_DefaultOption = 1f;

	private static LookupData[] m_LookupTable = new LookupData[6]
	{
		new LookupData(1f, CloudRegionCode.none, "Text.UI.AutomaticRegion"),
		new LookupData(2f, CloudRegionCode.eu, "Text.UI.EuropeRegion"),
		new LookupData(3f, CloudRegionCode.us, "Text.UI.AmericaRegion"),
		new LookupData(4f, CloudRegionCode.asia, "Text.UI.AsiaRegion"),
		new LookupData(5f, CloudRegionCode.cn, "Text.UI.ChinaRegion"),
		new LookupData(6f, CloudRegionCode.rue, "Text.UI.RussiaRegion")
	};

	public T17Text m_UIObject;

	public int m_CurrentIndex;

	public PhotonRegionOptionItem m_TheOption;

	public T17Button m_LeftButton;

	public T17Button m_RightButton;

	public LookupData GetCurrent()
	{
		return m_LookupTable[m_CurrentIndex];
	}

	public static LookupData GetLookupDataFromOptionsSettingValue(float fValue)
	{
		int num = m_LookupTable.Length;
		for (int i = 0; i < num; i++)
		{
			if (fValue == m_LookupTable[i].m_OptionValue)
			{
				return m_LookupTable[i];
			}
		}
		return null;
	}

	public static float GetOptionsSettingValue(CloudRegionCode region)
	{
		int num = m_LookupTable.Length;
		for (int i = 0; i < num; i++)
		{
			if (region == m_LookupTable[i].m_PhotonRegion)
			{
				return m_LookupTable[i].m_OptionValue;
			}
		}
		return 1f;
	}

	public void SetFromRegion(CloudRegionCode region)
	{
		int num = m_LookupTable.Length;
		for (int i = 0; i < num; i++)
		{
			if (region == m_LookupTable[i].m_PhotonRegion)
			{
				m_CurrentIndex = i;
				m_UIObject.SetLocalisedTextCatchAll(m_LookupTable[i].m_Description);
			}
		}
	}

	public void SetFromOptionsSettingValue(float fValue)
	{
		int num = m_LookupTable.Length;
		for (int i = 0; i < num; i++)
		{
			if (object.Equals(fValue, m_LookupTable[i].m_OptionValue))
			{
				m_CurrentIndex = i;
				m_UIObject.SetLocalisedTextCatchAll(m_LookupTable[i].m_Description);
			}
		}
	}

	public void SelectPrevious()
	{
		m_CurrentIndex--;
		if (m_CurrentIndex < 0)
		{
			m_CurrentIndex = m_LookupTable.Length - 1;
		}
		if (null != m_UIObject)
		{
			m_UIObject.SetLocalisedTextCatchAll(m_LookupTable[m_CurrentIndex].m_Description);
		}
	}

	public void SelectNext()
	{
		m_CurrentIndex++;
		if (m_CurrentIndex >= m_LookupTable.Length)
		{
			m_CurrentIndex = 0;
		}
		if (null != m_UIObject)
		{
			m_UIObject.SetLocalisedTextCatchAll(m_LookupTable[m_CurrentIndex].m_Description);
		}
	}

	public void Show()
	{
	}

	public void Hide()
	{
		if (m_RightButton != null)
		{
			m_RightButton.gameObject.transform.parent.gameObject.SetActive(value: false);
		}
	}
}
