using UnityEngine;

public class TargetHUD : MonoBehaviour
{
	public Sprite m_ValidSprite;

	public Sprite m_InvalidSprite;

	private T17Image m_HUDImage;

	private Sprite m_CurrentValidSprite;

	private Sprite m_CurrentInvalidSprite;

	private void Awake()
	{
		m_CurrentValidSprite = m_ValidSprite;
		m_CurrentInvalidSprite = m_InvalidSprite;
		m_HUDImage = GetComponent<T17Image>();
		if ((bool)m_HUDImage && (bool)m_CurrentValidSprite)
		{
			m_HUDImage.sprite = m_CurrentValidSprite;
		}
		Hide();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetValidSpriteOverride(Sprite validSpriteOverride)
	{
		m_CurrentValidSprite = ((!(validSpriteOverride != null)) ? m_ValidSprite : validSpriteOverride);
	}

	public void SetPosition(Vector2 pos)
	{
		float z = base.transform.position.z;
		base.transform.position = new Vector3(pos.x, pos.y, z);
	}

	public void SetPositionIsValid(bool bIsValid)
	{
		if ((bool)m_HUDImage && (bool)m_CurrentValidSprite && (bool)m_CurrentInvalidSprite)
		{
			if (bIsValid)
			{
				m_HUDImage.sprite = m_CurrentValidSprite;
			}
			else
			{
				m_HUDImage.sprite = m_CurrentInvalidSprite;
			}
		}
	}
}
