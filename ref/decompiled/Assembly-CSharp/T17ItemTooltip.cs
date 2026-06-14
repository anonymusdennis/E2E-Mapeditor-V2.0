using System;
using UnityEngine;

public class T17ItemTooltip : BaseTooltip
{
	public enum DisplaySetups
	{
		Unassigned = -1,
		NoButtons,
		SecondaryAndCancel,
		PrimaryOnly,
		CancelOnly,
		ItemFunctionalitiesAndCancel,
		PrimaryAndSecondary,
		PrimaryAndCancel
	}

	public T17Text m_ItemName;

	public T17Text m_ItemType;

	public T17Image m_ToolTipImage;

	public Sprite m_IllegalBackground;

	public Sprite m_LegalBackground;

	public GameObject m_SecondaryInputContainer;

	public T17Text m_SecondaryInputLabel;

	public GameObject m_CancelContainer;

	public T17Text m_CancelInputLabel;

	public GameObject m_PrimaryInputContainer;

	public T17Text m_PrimaryInputLabel;

	[HideInInspector]
	public bool m_IsTaken;

	private Func<int> m_GetRewiredIdDelegate;

	protected void Awake()
	{
		if (m_CancelInputLabel != null)
		{
			m_CancelInputLabel.SetGetRewiredIndexDelegate(GetHoldersRewiredId);
		}
		if (m_SecondaryInputLabel != null)
		{
			m_SecondaryInputLabel.SetGetRewiredIndexDelegate(GetHoldersRewiredId);
		}
		if (m_PrimaryInputLabel != null)
		{
			m_PrimaryInputLabel.SetGetRewiredIndexDelegate(GetHoldersRewiredId);
		}
	}

	private int GetHoldersRewiredId()
	{
		if (m_GetRewiredIdDelegate == null)
		{
			return 0;
		}
		return m_GetRewiredIdDelegate();
	}

	public void SetDelegateToHoldersRewiredId(Func<int> theDelegate)
	{
		m_GetRewiredIdDelegate = theDelegate;
	}

	public void SetTooltipBackground(bool itemIsIllegal)
	{
		if (itemIsIllegal)
		{
			if (m_ToolTipImage != null && m_IllegalBackground != null)
			{
				m_ToolTipImage.sprite = m_IllegalBackground;
			}
		}
		else if (m_ToolTipImage != null && m_LegalBackground != null)
		{
			m_ToolTipImage.sprite = m_LegalBackground;
		}
	}

	public void SetDisplayForContext(DisplaySetups context)
	{
		bool flag = false;
		if (m_CancelContainer == null)
		{
			flag = true;
		}
		if (m_SecondaryInputContainer == null)
		{
			flag = true;
		}
		if (m_PrimaryInputContainer == null)
		{
			flag = true;
		}
		if (!flag)
		{
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			switch (context)
			{
			case DisplaySetups.PrimaryOnly:
				flag4 = true;
				break;
			case DisplaySetups.SecondaryAndCancel:
			case DisplaySetups.ItemFunctionalitiesAndCancel:
				flag3 = true;
				flag2 = true;
				break;
			case DisplaySetups.CancelOnly:
				flag2 = true;
				break;
			case DisplaySetups.PrimaryAndSecondary:
				flag4 = true;
				flag3 = true;
				break;
			case DisplaySetups.PrimaryAndCancel:
				flag4 = true;
				flag2 = true;
				break;
			}
			int num = 0;
			if (m_PrimaryInputContainer.activeSelf != flag4)
			{
				m_PrimaryInputContainer.SetActive(flag4);
				m_PrimaryInputContainer.transform.SetSiblingIndex(num++);
			}
			if (m_SecondaryInputContainer.activeSelf != flag3)
			{
				m_SecondaryInputContainer.SetActive(flag3);
				m_SecondaryInputContainer.transform.SetSiblingIndex(num++);
			}
			if (m_CancelContainer.activeSelf != flag2)
			{
				m_CancelContainer.SetActive(flag2);
				m_CancelContainer.transform.SetSiblingIndex(num++);
			}
		}
	}
}
