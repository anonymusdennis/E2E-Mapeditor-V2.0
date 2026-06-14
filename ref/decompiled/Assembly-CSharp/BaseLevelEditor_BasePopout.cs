using System;
using UnityEngine;

public abstract class BaseLevelEditor_BasePopout : MonoBehaviour
{
	[Flags]
	public enum Action
	{
		Delete = 1,
		Move = 2,
		HideUI = 4,
		GridToggle = 8,
		LayerChange = 0x10,
		TabChange = 0x20,
		Escape = 0x40,
		File = 0x80,
		UndoRedo = 0x100,
		Center = 0x200,
		Zoom = 0x400
	}

	[HideInInspector]
	public Action m_BlockAction = Action.Escape;

	[HideInInspector]
	public Action m_HideOnAction = Action.Delete | Action.Move | Action.HideUI | Action.TabChange | Action.Escape | Action.File;

	public void OnShow()
	{
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		if (instance != null)
		{
			instance.OnPopupShown(this);
		}
	}

	public void OnHide()
	{
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		if (instance != null)
		{
			instance.OnPopupHidden(this);
		}
	}

	public bool IsActionBlocked(Action eAction)
	{
		return (m_BlockAction & eAction) == eAction;
	}

	public bool IsActionAHideAction(Action eAction)
	{
		return (m_HideOnAction & eAction) == eAction;
	}

	public abstract void Hide();
}
