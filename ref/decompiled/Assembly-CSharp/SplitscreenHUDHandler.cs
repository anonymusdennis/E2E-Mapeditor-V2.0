using UnityEngine;

public class SplitscreenHUDHandler : MonoBehaviour
{
	public HUDItemLayoutGroup[] m_LayoutGroups;

	private HUDItemLayoutGroup m_activeLayoutGroup;

	private float m_fActiveAspectRatio;

	public HUDItemLayoutGroup ActiveLayoutGroup => m_activeLayoutGroup;

	public HUDItemsLayout GetPosScale(int playerNumber, int playerIndex)
	{
		CheckLayoutGroup();
		switch (playerNumber)
		{
		case 1:
			return m_activeLayoutGroup.m_OnePlayerConfig;
		case 2:
			if (playerIndex < 2 && playerIndex >= 0)
			{
				return m_activeLayoutGroup.m_TwoPlayerConfig[playerIndex];
			}
			break;
		case 3:
			if (playerIndex < 3 && playerIndex >= 0)
			{
				return m_activeLayoutGroup.m_ThreePlayerConfig[playerIndex];
			}
			break;
		case 4:
			if (playerIndex < 4 && playerIndex >= 0)
			{
				return m_activeLayoutGroup.m_FourPlayerConfig[playerIndex];
			}
			break;
		}
		return null;
	}

	private void CheckLayoutGroup()
	{
		float num = (float)Screen.width / (float)Screen.height;
		if (!Mathf.Approximately(num, m_fActiveAspectRatio))
		{
			m_activeLayoutGroup = GetLayoutGroup(num);
			m_fActiveAspectRatio = num;
		}
	}

	private HUDItemLayoutGroup GetLayoutGroup(float fAspectRatio)
	{
		int num = -1;
		for (int i = 0; i != m_LayoutGroups.Length; i++)
		{
			if (m_LayoutGroups[i] != null && m_LayoutGroups[i].CoversRatio(fAspectRatio) && m_LayoutGroups[i].m_PlatformOverride == HUDItemLayoutGroup.PlatformOverride.none && num == -1)
			{
				num = i;
			}
		}
		Debug.Log("Using default layout group.  Aspect=" + fAspectRatio + "   group=" + num);
		if (num == -1)
		{
			num = 0;
		}
		return m_LayoutGroups[num];
	}

	public HUDItemLayoutGroup UpdateActiveLayoutGroup()
	{
		CheckLayoutGroup();
		return m_activeLayoutGroup;
	}
}
