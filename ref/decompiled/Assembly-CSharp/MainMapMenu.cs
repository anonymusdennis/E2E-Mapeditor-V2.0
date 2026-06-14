using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class MainMapMenu : T17MonoBehaviour, IMenuEventDelegate
{
	public RectTransform m_Key;

	public RectTransform m_Floor;

	public RectTransform m_Legend;

	private MainMap m_MainMap;

	private Player m_Player;

	private Gamer m_Gamer;

	private Vector2 m_mapViewScale;

	public RoutineAndTimeTrackerHUD m_RoutineTracker;

	public event MenuChangedHandler MenuChangedEvent;

	protected virtual void OnDestroy()
	{
		m_MainMap = null;
		m_Player = null;
		m_Gamer = null;
		m_RoutineTracker = null;
		this.MenuChangedEvent = null;
	}

	public void ShowMap(Player player)
	{
		if (!base.gameObject.GetActive())
		{
			base.gameObject.SetActive(value: true);
			m_Player = player;
			m_Gamer = player.m_Gamer;
			if (m_MainMap == null)
			{
				FirstTimeSetup();
			}
			m_MainMap.InitMap(m_Player.transform.position, m_Player.CurrentFloor, m_Gamer, m_mapViewScale);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_In, base.gameObject);
		}
		m_Player.IsBrowsingMainMap = true;
		if (this.MenuChangedEvent != null)
		{
			this.MenuChangedEvent();
		}
		if (!(m_RoutineTracker != null))
		{
			return;
		}
		HUDMenuFlow instance = HUDMenuFlow.Instance;
		if (instance != null)
		{
			instance.GetCorrectHUDData(player.m_PlayerCameraManagerBindingID, out var data);
			if (data != null)
			{
				m_RoutineTracker.SynchFromTracker(data.m_MiniMapParent);
				m_RoutineTracker.SetGamePlayer(player);
			}
		}
	}

	public void HideMap()
	{
		if (m_MainMap != null)
		{
			m_MainMap.Hiding();
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_MiniMap_Out, base.gameObject);
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
		if (m_Player != null)
		{
			m_Player.IsBrowsingMainMap = false;
		}
		if (this.MenuChangedEvent != null)
		{
			this.MenuChangedEvent();
		}
		if (m_RoutineTracker != null && m_Player != null)
		{
			HUDMenuFlow instance = HUDMenuFlow.Instance;
			if (instance != null)
			{
				instance.GetCorrectHUDData(m_Player.m_PlayerCameraManagerBindingID, out var data);
				data?.m_MiniMapParent.SynchFromTracker(m_RoutineTracker);
			}
		}
		CustomLightRenderer.customLightManager.ForceUpdate();
	}

	public void SetScale(Vector2 mapViewScale)
	{
		m_mapViewScale = mapViewScale;
	}

	private void Update()
	{
		if (m_Gamer.m_RewiredPlayer.GetButtonDown("OpenMainMap") || m_Gamer.m_RewiredPlayer.GetButtonUp("CloseMap") || m_Gamer.m_RewiredPlayer.GetButtonUp("UI_Close") || m_Gamer.m_RewiredPlayer.GetButtonUp("UI_Cancel"))
		{
			InGameMenuFlow.Instance.HideMap(m_Player, m_Player.m_PlayerCameraManagerBindingID);
		}
	}

	public void ChildMenuChanged(IMenuEventDelegate sender = null, IMenuEventDelegate changedItem = null)
	{
		if (this.MenuChangedEvent != null)
		{
			this.MenuChangedEvent();
		}
	}

	public void FirstTimeSetup()
	{
		if (!(m_MainMap != null))
		{
			m_MainMap = GetComponentInChildren<MainMap>();
			m_MainMap.m_ActiveRawImage = m_MainMap.GetComponent<T17RawImage>();
			m_MainMap.FirstTimeSetup();
			RoutineAndTimeTrackerHUD componentInChildren = GetComponentInChildren<RoutineAndTimeTrackerHUD>();
			if (componentInChildren != null)
			{
				componentInChildren.StartInit();
			}
		}
	}
}
