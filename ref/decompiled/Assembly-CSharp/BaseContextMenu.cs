using AUTOGEN_T17Wwise_Enums;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseContextMenu : MonoBehaviour
{
	public T17Button[] m_Items;

	private GameObject m_SelectedBefore;

	private bool m_IsContextMenuOpen;

	public bool isContextMenuOpen => m_IsContextMenuOpen;

	public void ShowContextMenu(int index)
	{
		m_IsContextMenuOpen = true;
		base.gameObject.SetActive(value: true);
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(Gamer.GetPrimaryGamer());
		if (eventSystemForGamer != null)
		{
			m_SelectedBefore = eventSystemForGamer.currentSelectedGameObject;
			eventSystemForGamer.SetSelectedGameObject(null);
			if (m_Items != null && index < m_Items.Length)
			{
				eventSystemForGamer.SetSelectedGameObject(m_Items[index].gameObject);
			}
		}
	}

	private void Update()
	{
		if (Gamer.GetPrimaryGamer().m_RewiredPlayer.GetButtonUp("UI_Cancel"))
		{
			Hide();
			NavigateOnUICancel component = GetComponent<NavigateOnUICancel>();
			if (component != null)
			{
				component.m_DoThisOnUICancel.Invoke();
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Reject, AudioController.UI_Audio_GO);
		}
	}

	public void Hide()
	{
		if (m_IsContextMenuOpen)
		{
			m_IsContextMenuOpen = false;
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(Gamer.GetPrimaryGamer());
			if (eventSystemForGamer != null)
			{
				eventSystemForGamer.SetSelectedGameObject(null);
				eventSystemForGamer.SetSelectedGameObject(m_SelectedBefore);
			}
			m_SelectedBefore = null;
		}
		base.gameObject.SetActive(value: false);
	}

	private void MouseClick(RewiredStandaloneInputModule module, PointerInputModule.MouseButtonEventData data)
	{
		Vector2 position = data.buttonData.position;
		RectTransform rect = (RectTransform)base.transform;
		if (!RectTransformUtility.RectangleContainsScreenPoint(rect, position))
		{
			Hide();
			NavigateOnUICancel component = GetComponent<NavigateOnUICancel>();
			if (component != null)
			{
				component.m_DoThisOnUICancel.Invoke();
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Reject, AudioController.UI_Audio_GO);
		}
	}

	protected virtual void OnDestroy()
	{
		m_SelectedBefore = null;
		int num = m_Items.Length;
		for (int i = 0; i < num; i++)
		{
			m_Items[i] = null;
		}
		m_Items = null;
	}
}
