using UnityEngine;

public class UIFriendCampaignArrows : MonoBehaviour
{
	private GameObject m_LeftButton;

	private GameObject m_RightButton;

	private void Awake()
	{
		m_LeftButton = base.transform.FindChild("PlayersArrowLeft").gameObject;
		m_RightButton = base.transform.FindChild("PlayersArrowRight").gameObject;
	}

	public void EnableArrows(bool enable)
	{
		EnableArrow(m_LeftButton, enable);
		EnableArrow(m_RightButton, enable);
	}

	private void EnableArrow(GameObject arrow, bool enable)
	{
		if (arrow != null)
		{
			arrow.SetActive(enable);
		}
	}

	public void SetNumberOfPagesAvailable(int pages)
	{
		EnableArrows(enable: true);
		if (pages <= 1)
		{
			EnableArrows(enable: false);
		}
	}
}
