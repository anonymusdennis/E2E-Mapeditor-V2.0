using System;
using System.Collections.Generic;
using UnityEngine;

public class DLCStoreFrontendMenu : FrontendMenuBehaviour
{
	public DLCCarousel m_DLCCarousel;

	public List<DLCFrontendData> m_DLCList = new List<DLCFrontendData>();

	protected override void Awake()
	{
		base.Awake();
		int num = 0;
		while (num < m_DLCList.Count)
		{
			if (m_DLCList[num] == null || !m_DLCList[num].IsAvailableOnThisPlatform())
			{
				m_DLCList.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public static Transform FindDeepChild(Transform aParent, string aName)
	{
		Transform transform = aParent.Find(aName);
		if (transform != null)
		{
			return transform;
		}
		foreach (Transform item in aParent)
		{
			transform = FindDeepChild(item, aName);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if ((bool)Platform.GetInstance())
		{
			Platform instance = Platform.GetInstance();
			instance.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Combine(instance.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(OnDLCUpdated));
		}
		UpdateDLCCarousel();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if ((bool)Platform.GetInstance())
		{
			Platform instance = Platform.GetInstance();
			instance.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Remove(instance.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(OnDLCUpdated));
		}
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		return true;
	}

	public void ShowSelectedDLCStore()
	{
		if (!(m_DLCCarousel != null) || !(Platform.GetInstance() != null))
		{
			return;
		}
		DLCFrontendData selectedItem = m_DLCCarousel.GetSelectedItem();
		if (selectedItem != null && !string.IsNullOrEmpty(selectedItem.m_DLCID) && !Platform.GetInstance().ShowDLCStorePage(selectedItem.m_DLCID))
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.FailedToOpenStore.Title", "Text.Dialog.FailedToOpenStore.Description", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Error);
				dialog.Show();
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentGamer != null && base.CurrentGamer.m_RewiredPlayer != null && !FrontEndFlow.Instance.m_MainMenu.IsChildMenuOpen() && !T17DialogBoxManager.HasAnyOpenDialogs())
		{
			if (base.CurrentGamer.m_RewiredPlayer.GetButtonDown("UI_CycleLeft"))
			{
				m_DLCCarousel.SelectPrevious();
			}
			if (base.CurrentGamer.m_RewiredPlayer.GetButtonDown("UI_CycleRight"))
			{
				m_DLCCarousel.SelectNext();
			}
		}
	}

	private void OnDLCUpdated()
	{
	}

	private void UpdateDLCCarousel()
	{
		if (m_DLCCarousel != null)
		{
			List<DLCFrontendData> list = new List<DLCFrontendData>(m_DLCList);
			list.RemoveAll((DLCFrontendData x) => !x.m_bShowOnDlcPage);
			m_DLCCarousel.SetCarouselOptions(list);
		}
	}
}
