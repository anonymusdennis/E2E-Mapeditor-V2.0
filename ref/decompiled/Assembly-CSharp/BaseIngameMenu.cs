using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseIngameMenu : MonoBehaviour
{
	protected Player m_Player;

	private IT17EventHelper[] m_EventHelperInterfaces;

	public GameObject m_OnOpenSelectObject;

	protected abstract void HideMenu();

	protected virtual void Awake()
	{
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
	}

	public virtual void Show(Player player)
	{
		m_Player = player;
		if (!base.gameObject.GetActive())
		{
			base.gameObject.SetActive(value: true);
		}
		SetEventHelpersToPlayer(m_Player);
	}

	public virtual void Hide()
	{
		if (base.gameObject.GetActive())
		{
			base.gameObject.SetActive(value: false);
		}
		m_Player = null;
	}

	private void SetEventHelpersToPlayer(Player targetPlayer)
	{
		if (m_EventHelperInterfaces != null)
		{
			T17EventSystem gamersEventSystem = null;
			if (targetPlayer != null && targetPlayer.m_Gamer != null)
			{
				gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(targetPlayer.m_Gamer);
			}
			for (int i = 0; i < m_EventHelperInterfaces.Length; i++)
			{
				if (m_EventHelperInterfaces[i] != null && targetPlayer.m_Gamer != null)
				{
					m_EventHelperInterfaces[i].SetGamerForEventSystem(targetPlayer.m_Gamer, gamersEventSystem);
				}
			}
		}
		if (EventSystem.current != null)
		{
			GameObject selectedGameObject = ((!(m_OnOpenSelectObject != null)) ? (m_EventHelperInterfaces[0] as MonoBehaviour).gameObject : m_OnOpenSelectObject);
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(selectedGameObject);
		}
	}

	protected virtual void Update()
	{
		CheckForMenuClose();
	}

	protected bool CheckForUICancel()
	{
		if (m_Player != null && m_Player.m_Gamer != null && m_Player.m_Gamer.m_RewiredPlayer != null && m_Player.m_Gamer.m_RewiredPlayer.GetButtonUp("UI_Cancel"))
		{
			return true;
		}
		return false;
	}

	protected virtual void CheckForMenuClose()
	{
		if (CheckForUICancel())
		{
			DismissWindow();
		}
	}

	public void DismissWindow()
	{
		HideMenu();
	}
}
