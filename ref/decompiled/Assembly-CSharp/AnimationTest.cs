using UnityEngine;

public class AnimationTest : MonoBehaviour
{
	public CharacterAnimator m_CharacterAnimator;

	private float m_fRotation;

	private float m_fSpeed;

	private int m_iState;

	private Vector2 m_fLastMousePos = Vector2.zero;

	private void Awake()
	{
		m_CharacterAnimator = base.gameObject.GetComponentInParent<CharacterAnimator>();
		m_iState = 5;
	}

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			if (m_fLastMousePos.x == 0f && m_fLastMousePos.y == 0f)
			{
				m_fLastMousePos.x = Mathf.Round(Input.mousePosition.x / 100f);
				m_fLastMousePos.y = Mathf.Round(Input.mousePosition.y / 200f);
			}
			float num = Mathf.Round(Input.mousePosition.x / 100f) - m_fLastMousePos.x;
			float num2 = Mathf.Round(Input.mousePosition.y / 200f) - m_fLastMousePos.y;
			m_fLastMousePos.x = Mathf.Round(Input.mousePosition.x / 100f);
			m_fLastMousePos.y = Mathf.Round(Input.mousePosition.y / 200f);
			m_fRotation += num;
			while (m_fRotation > 11f)
			{
				m_fRotation -= 12f;
			}
			while (m_fRotation < 0f)
			{
				m_fRotation += 12f;
			}
			m_fSpeed += num2;
			while (m_fSpeed > 3f)
			{
				m_fSpeed -= 3f;
			}
			while (m_fSpeed < 0f)
			{
				m_fSpeed += 3f;
			}
			float num3 = twelveToFour(m_fRotation);
			m_CharacterAnimator.HeadAndBodyFaceDirection((Directionx4)num3);
			m_CharacterAnimator.CharacterSpeedChanged((CharacterSpeed)m_fSpeed);
		}
		if (Input.GetMouseButtonUp(0))
		{
			m_fLastMousePos.x = 0f;
			m_fLastMousePos.y = 0f;
		}
		if (Input.GetMouseButtonDown(0))
		{
			m_CharacterAnimator.NetStateChanged((AnimState)m_iState);
		}
		if (Input.GetMouseButtonDown(1))
		{
			m_iState++;
			if (m_iState >= 150)
			{
				m_iState = 0;
			}
			m_CharacterAnimator.NetStateChanged((AnimState)m_iState);
		}
	}

	private float twelveToFour(float direction)
	{
		return Mathf.Round((direction - 1.5f) / 3f) * 2f;
	}

	private float twelveToEight(float direction)
	{
		if (direction == 0f || direction == 11f)
		{
			return 7f;
		}
		if (direction == 1f)
		{
			return 0f;
		}
		if (direction == 2f || direction == 3f)
		{
			return 1f;
		}
		if (direction == 4f)
		{
			return 2f;
		}
		if (direction == 5f || direction == 6f)
		{
			return 3f;
		}
		if (direction == 7f)
		{
			return 4f;
		}
		if (direction == 8f || direction == 9f)
		{
			return 5f;
		}
		if (direction == 10f)
		{
			return 6f;
		}
		return 0f;
	}
}
