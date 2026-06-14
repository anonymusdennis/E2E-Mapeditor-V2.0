using UnityEngine;

public class CoopPlayerObject : MonoBehaviour
{
	public GameObject m_JoinParent;

	public GameObject m_JoinedParent;

	public T17Text m_PlayerName;

	public int m_PlayerControllerIndex = -1;

	public string m_JoinString = "Text.Menu.Join";

	private static Sprite joinedCharacterTexture;

	public bool IsWaitingForJoin()
	{
		return m_PlayerControllerIndex == -1;
	}

	public void SetWaitingForJoin()
	{
		if (m_JoinParent != null)
		{
			m_JoinParent.SetActive(value: true);
		}
		if (m_JoinedParent != null)
		{
			m_JoinedParent.SetActive(value: false);
		}
		if (m_PlayerName != null)
		{
			m_PlayerName.SetLocalisedTextCatchAll(m_JoinString);
		}
		m_PlayerControllerIndex = -1;
	}

	public void SetJoined(string charName, int controllerIndex, Texture2D icon = null)
	{
		if (controllerIndex < 0)
		{
		}
		m_PlayerControllerIndex = controllerIndex;
		if (m_JoinParent != null)
		{
			m_JoinParent.SetActive(value: false);
		}
		if (m_JoinedParent != null)
		{
			m_JoinedParent.SetActive(value: true);
			T17Image componentInChildren = m_JoinedParent.GetComponentInChildren<T17Image>();
			if (componentInChildren != null)
			{
				if (joinedCharacterTexture == null)
				{
					joinedCharacterTexture = componentInChildren.sprite;
				}
				if (icon == null)
				{
					componentInChildren.sprite = joinedCharacterTexture;
				}
				else
				{
					componentInChildren.sprite = Sprite.Create(icon, new Rect(0f, 0f, 256f, 256f), new Vector2(0.5f, 0.5f));
				}
			}
		}
		if (m_PlayerName != null)
		{
			m_PlayerName.m_bNeedsLocalization = false;
			m_PlayerName.text = charName;
		}
	}
}
