using UnityEngine;

public class BedSaveMenu : GameMenuBehaviour
{
	public T17Button m_SaveButton;

	public T17Button m_CancelButton;

	private Player m_Player;

	private CameraManager.PlayerBindingID m_BindingID;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			if (currentGamer != null)
			{
				m_Player = currentGamer.m_PlayerObject;
				m_BindingID = m_Player.m_PlayerCameraManagerBindingID;
				T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(currentGamer);
				if (eventSystemForGamer != null && m_SaveButton != null)
				{
					eventSystemForGamer.SetSelectedGameObject(m_SaveButton.gameObject);
				}
			}
			return true;
		}
		return false;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (base.CurrentGamer != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
			if (eventSystemForGamer != null)
			{
				eventSystemForGamer.SetSelectedGameObject(null);
			}
		}
		m_Player = null;
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	public void Close()
	{
		if (base.gameObject.activeSelf && m_Player != null)
		{
			InGameMenuFlow.Instance.HideBedSave(m_Player, m_BindingID);
		}
	}

	public void OnSaveClicked(T17Button button)
	{
		SaveManager instance = SaveManager.GetInstance();
		if (instance != null)
		{
			instance.SaveGame(null);
		}
		InGameMenuFlow.Instance.HideBedSave(m_Player, m_BindingID);
	}

	public void OnCancelClicked(T17Button button)
	{
		InGameMenuFlow.Instance.HideBedSave(m_Player, m_BindingID);
	}
}
