using UnityEngine;

public class PlayerMapsSnapshot
{
	public Gamer m_Gamer;

	private T17EventSystem.InputCateogryStates m_CapturedState = T17EventSystem.InputCateogryStates.Disabled;

	private T17EventSystem.InputCateogryStates m_PrevState = T17EventSystem.InputCateogryStates.Disabled;

	private T17EventSystem.InputCateogryStates m_LastReqState = T17EventSystem.InputCateogryStates.Disabled;

	public GameObject m_CurrentSelectedGameobject;

	public bool m_bFullSnapshot;

	public PlayerMapsSnapshot(Gamer gamer, bool disableAll, bool bFullSnapshot)
	{
		m_Gamer = gamer;
		m_bFullSnapshot = bFullSnapshot;
		if (m_Gamer.IsLocal() && m_Gamer.m_RewiredPlayer != null)
		{
			m_CapturedState = T17EventSystem.GetStateForRewiredPlayer(m_Gamer.m_RewiredPlayer);
			if (bFullSnapshot)
			{
				m_PrevState = T17EventSystem.GetPrevStateForRewiredPlayer(m_Gamer.m_RewiredPlayer);
				m_LastReqState = T17EventSystem.GetLastReqStateForRewiredPlayer(m_Gamer.m_RewiredPlayer);
			}
		}
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(gamer);
		if (eventSystemForGamer != null)
		{
			m_CurrentSelectedGameobject = eventSystemForGamer.currentSelectedGameObject;
		}
	}

	public void RestoreControllerMaps()
	{
		if (m_Gamer.IsLocal() && m_Gamer.m_RewiredPlayer != null && m_CapturedState != T17EventSystem.InputCateogryStates.Loading)
		{
			T17EventSystem.ApplyCategories(m_Gamer.m_RewiredPlayer, m_CapturedState);
			if (m_bFullSnapshot)
			{
				T17EventSystem.SetPrevStateForRewiredPlayer(m_Gamer.m_RewiredPlayer, m_PrevState);
				T17EventSystem.SetLastReqStateForRewiredPlayer(m_Gamer.m_RewiredPlayer, m_LastReqState);
			}
		}
	}

	public void RestoreSelectedGameobject()
	{
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Gamer);
		if (eventSystemForGamer != null)
		{
			eventSystemForGamer.SetSelectedGameObject(m_CurrentSelectedGameobject);
		}
	}

	public static PlayerMapsSnapshot CreateSnapshotForGamer(Gamer gamer, bool disableAllMaps, bool bFullSnapshot = false)
	{
		if (gamer == null)
		{
			return null;
		}
		return new PlayerMapsSnapshot(gamer, disableAllMaps, bFullSnapshot);
	}

	public void OverrideCapturedSelected(T17EventSystem.InputCateogryStates newCategory)
	{
		m_PrevState = m_CapturedState;
		m_CapturedState = newCategory;
	}
}
