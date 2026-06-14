using UnityEngine;

public class LevelEditor_SavingIcon : MonoBehaviour
{
	public GameObject m_SavingIconToShow;

	public float m_MinTimeToShow = 1f;

	private float m_fTimeToHide;

	public void ShowSavingIcon()
	{
		if (m_SavingIconToShow != null)
		{
			m_SavingIconToShow.SetActive(value: true);
		}
		m_fTimeToHide = Time.realtimeSinceStartup + m_MinTimeToShow;
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup >= m_fTimeToHide && m_SavingIconToShow != null && m_SavingIconToShow.GetActive())
		{
			m_SavingIconToShow.SetActive(value: false);
		}
	}
}
