using System.Collections.Generic;
using UnityEngine;

public class LevelEditor_CheckList : MonoBehaviour
{
	public GameObject m_CheckListPrefab;

	private bool m_bInitialSetupDone;

	private BuildingBlockManager m_BlockMan;

	private LevelDetailsManager m_DetailsMan;

	private List<LevelEditor_CheckList_Entry> m_Entries = new List<LevelEditor_CheckList_Entry>();

	public T17ScrollView m_ScrollView;

	private bool m_UpdateRequired = true;

	private void Awake()
	{
		m_bInitialSetupDone = false;
	}

	private void Update()
	{
		if (m_BlockMan == null)
		{
			m_BlockMan = BuildingBlockManager.GetInstance();
			if (!m_bInitialSetupDone && m_BlockMan != null && m_CheckListPrefab != null && m_BlockMan.m_LimitationGroups.Length > 0)
			{
				SetupDefaultLimitations();
				m_BlockMan.RegisterLimitationChange(OnLimitationChange);
			}
		}
		if (m_DetailsMan == null)
		{
			m_DetailsMan = LevelDetailsManager.GetInstance();
			if (m_DetailsMan != null)
			{
				m_DetailsMan.RegisterRoutineChange(OnRoutineChanged);
			}
		}
		if (m_UpdateRequired)
		{
			UpdateCheckList();
		}
	}

	public void OnLimitationChange(int iGroup)
	{
		m_UpdateRequired = true;
	}

	public void OnRoutineChanged(int iHour)
	{
		m_UpdateRequired = true;
	}

	public void UpdateCheckList()
	{
		bool flag = false;
		m_UpdateRequired = false;
		for (int num = m_Entries.Count - 1; num >= 0; num--)
		{
			if (m_Entries[num] != null)
			{
				bool flag2 = m_Entries[num].UpdateState();
				if (flag2 != m_Entries[num].gameObject.activeInHierarchy)
				{
					flag = true;
				}
				m_Entries[num].gameObject.SetActive(flag2);
			}
		}
		if (flag && m_ScrollView != null)
		{
			m_ScrollView.verticalNormalizedPosition = 1f;
		}
	}

	private void SetupDefaultLimitations()
	{
		List<int> list = new List<int>();
		m_bInitialSetupDone = true;
		for (int i = 0; i < m_BlockMan.m_LimitationGroups.Length; i++)
		{
			BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(i);
			if (limitationGroup == null || !limitationGroup.m_bValid)
			{
				continue;
			}
			if (limitationGroup.m_Min > 0)
			{
				if (!list.Contains(i) && AddEntry(i))
				{
					list.Add(i);
				}
			}
			else if (limitationGroup.m_TotalAutoMinimums != 0)
			{
				for (int j = 0; j < limitationGroup.m_TotalAutoMinimums; j++)
				{
					int num = limitationGroup.m_AutoMinimums[j];
					if (!list.Contains(num) && AddEntry(num))
					{
						list.Add(num);
					}
				}
			}
			else if (limitationGroup.m_Min == 0 && !list.Contains(i) && AddEntry(i))
			{
				list.Add(i);
			}
		}
	}

	private bool AddEntry(int iID)
	{
		GameObject gameObject = Object.Instantiate(m_CheckListPrefab, base.transform);
		if (gameObject != null)
		{
			LevelEditor_CheckList_Entry component = gameObject.GetComponent<LevelEditor_CheckList_Entry>();
			if (component != null)
			{
				component.SetLimitationID(iID);
				m_Entries.Add(component);
			}
			return true;
		}
		return false;
	}
}
