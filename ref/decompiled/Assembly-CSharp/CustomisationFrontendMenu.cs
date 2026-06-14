using UnityEngine;
using UnityEngine.EventSystems;

public class CustomisationFrontendMenu : FrontendMenuBehaviour, ICustomisableCharacters
{
	private class RT
	{
		public RenderTexture m_RenderTexture;

		public int m_ID;
	}

	[Header("Settings")]
	public int m_CustomisationDialogIndex;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public T17Button[] m_Avatars = new T17Button[0];

	private Customisation[] m_Customisations = new Customisation[0];

	private RT[] m_RenderTextures;

	private int m_CurrentlySelectedIndex = -1;

	private int m_CustomisationToModifyIndex = -1;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndCustomisation");
		SetupButtons();
		UpdateCustomisations();
		ShowAllNames();
		m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		SetupRenderTextures(m_Customisations.Length);
		DrawAllCharacters();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndGeneral");
		m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		DestroyAllRenderTextures();
		m_CurrentlySelectedIndex = -1;
		m_CustomisationToModifyIndex = -1;
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (m_RenderTextures != null)
		{
			RenderTexture renderTexture = m_RenderTextures[0].m_RenderTexture;
			if (renderTexture != null && !renderTexture.IsCreated())
			{
				SetupRenderTextures(m_Customisations.Length);
				DrawAllCharacters();
			}
		}
		if (m_CurrentlySelectedIndex >= 0)
		{
			int num = Mathf.Min(m_RenderTextures.Length, m_Customisations.Length);
			if (m_CurrentlySelectedIndex < num)
			{
				Customisation customisation = m_Customisations[m_CurrentlySelectedIndex];
				RenderTexture renderTexture2 = m_RenderTextures[m_CurrentlySelectedIndex].m_RenderTexture;
				m_CharacterRenderer.UpdateAnimation(Time.deltaTime);
				DrawCharacter(customisation, renderTexture2);
			}
		}
	}

	private void DrawCharacter(Customisation customisation, RenderTexture texture)
	{
		m_CharacterRenderer.SetCustomisation(customisation);
		m_CharacterRenderer.DrawCharacter(texture);
	}

	private void DrawAllCharacters()
	{
		int num = Mathf.Min(m_RenderTextures.Length, m_Customisations.Length);
		for (int i = 0; i < num; i++)
		{
			DrawCharacter(m_Customisations[i], m_RenderTextures[i].m_RenderTexture);
		}
	}

	private void UpdateCustomisations()
	{
		m_Customisations = CustomisationManager.GetInstance().GetPlayerPresets();
	}

	private void SetupRenderTextures(int requested)
	{
		DestroyAllRenderTextures();
		m_RenderTextures = new RT[m_Avatars.Length];
		for (int i = 0; i < m_Avatars.Length; i++)
		{
			m_RenderTextures[i] = new RT();
			m_Avatars[i].gameObject.SetActive(i < requested);
			if (i < requested)
			{
				T17RawImage componentInChildren = m_Avatars[i].GetComponentInChildren<T17RawImage>();
				int num = Mathf.FloorToInt(componentInChildren.rectTransform.rect.width);
				int num2 = Mathf.FloorToInt(componentInChildren.rectTransform.rect.height);
				if (num > 0 && num2 > 0)
				{
					m_RenderTextures[i].m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextures[i].m_ID);
				}
				componentInChildren.texture = m_RenderTextures[i].m_RenderTexture;
			}
		}
	}

	private void DestroyAllRenderTextures()
	{
		if (m_RenderTextures != null && m_RenderTextures.Length > 0)
		{
			for (int i = 0; i < m_RenderTextures.Length; i++)
			{
				m_CharacterRenderer.CleanupRenderTexture(ref m_RenderTextures[i].m_ID);
				m_RenderTextures[i].m_RenderTexture = null;
			}
		}
		for (int j = 0; j < m_Avatars.Length; j++)
		{
			T17RawImage componentInChildren = m_Avatars[j].GetComponentInChildren<T17RawImage>();
			componentInChildren.texture = null;
		}
	}

	private void ShowName(int index, Customisation customisation)
	{
		T17Text t17Text = null;
		if (index >= 0 && index < m_Avatars.Length)
		{
			t17Text = m_Avatars[index].GetComponentInChildren<T17Text>();
		}
		if (t17Text != null)
		{
			string localised = string.Empty;
			if (!Localization.GetWithKeySwap(customisation.namePrefixKey, out localised, "$name", customisation.name))
			{
				localised = customisation.name;
			}
			t17Text.text = localised;
			t17Text.m_bNeedsLocalization = false;
		}
	}

	private void ShowAllNames()
	{
		int num = Mathf.Min(m_Avatars.Length, m_Customisations.Length);
		for (int i = 0; i < num; i++)
		{
			ShowName(i, m_Customisations[i]);
		}
	}

	private void SetupButtons()
	{
		for (int i = 0; i < m_Avatars.Length; i++)
		{
			int buttonIndex = i;
			T17Button t17Button = m_Avatars[i];
			t17Button.onClick.RemoveAllListeners();
			t17Button.onClick.AddListener(delegate
			{
				OnAvatarClicked(buttonIndex);
			});
			EventTrigger component = t17Button.GetComponent<EventTrigger>();
			if (component != null)
			{
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.Select;
				entry.callback.AddListener(delegate
				{
					OnAvatarSelected(buttonIndex);
				});
				component.triggers.Insert(0, entry);
				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.Deselect;
				entry.callback.AddListener(delegate
				{
					OnAvatarDeselected(buttonIndex);
				});
				component.triggers.Insert(0, entry);
			}
		}
	}

	private void OnAvatarClicked(int index)
	{
		if (index >= 0 && index < m_Customisations.Length)
		{
			m_CustomisationToModifyIndex = index;
		}
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.OpenChildOnTopOfMenu(m_CustomisationDialogIndex);
		}
	}

	public void OnAvatarSelected(int index)
	{
		m_CurrentlySelectedIndex = index;
	}

	public void OnAvatarDeselected(int index)
	{
		if (m_CurrentlySelectedIndex == index)
		{
			m_CurrentlySelectedIndex = -1;
		}
	}

	public Customisation GetCustomisationToModify()
	{
		Customisation result = null;
		if (m_CustomisationToModifyIndex >= 0 && m_CustomisationToModifyIndex < m_Customisations.Length)
		{
			result = m_Customisations[m_CustomisationToModifyIndex];
		}
		return result;
	}

	public CustomisationConstraint GetCustomisationConstraint()
	{
		return null;
	}

	public void OnCustomisationModified()
	{
		if (m_CustomisationToModifyIndex >= 0 && m_CustomisationToModifyIndex < m_Customisations.Length)
		{
			DrawCharacter(m_Customisations[m_CustomisationToModifyIndex], m_RenderTextures[m_CustomisationToModifyIndex].m_RenderTexture);
			ShowName(m_CustomisationToModifyIndex, m_Customisations[m_CustomisationToModifyIndex]);
		}
		CustomisationManager.GetInstance().OnPlayerPresetChanged(m_CustomisationToModifyIndex);
	}

	public int GetCurrentCustomsiationIndex()
	{
		return -1;
	}
}
