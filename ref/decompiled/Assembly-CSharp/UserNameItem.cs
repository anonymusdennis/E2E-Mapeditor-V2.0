using UnityEngine;

public class UserNameItem : MonoBehaviour
{
	public T17Text m_UserNameText;

	private bool m_bRePrime;

	private void Start()
	{
		m_bRePrime = true;
	}

	private void Update()
	{
		if (m_bRePrime && m_UserNameText != null && Platform.GetInstance() != null)
		{
			m_UserNameText.text = Platform.GetInstance().GetPrimaryUserName();
			m_bRePrime = false;
		}
	}
}
