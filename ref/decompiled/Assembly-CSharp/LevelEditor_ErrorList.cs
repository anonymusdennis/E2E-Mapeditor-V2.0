using System.Collections.Generic;
using UnityEngine;

public class LevelEditor_ErrorList : MonoBehaviour
{
	public T17Text m_ErrorPrefab;

	private List<T17Text> m_ErrorPrefabs = new List<T17Text>();

	public T17ScrollView m_ScrollView;

	public T17Text m_Passed;

	public void RemoveAllErrors()
	{
		for (int i = 0; i < m_ErrorPrefabs.Count; i++)
		{
			Object.Destroy(m_ErrorPrefabs[i].gameObject);
			m_ErrorPrefabs[i] = null;
		}
		m_ErrorPrefabs.Clear();
	}

	public void CreateErrors(List<LevelDetailsManager.ErrorData> errorList)
	{
		bool flag = true;
		RemoveAllErrors();
		int num = -1;
		if (m_ErrorPrefab != null)
		{
			for (int i = 0; i < errorList.Count; i++)
			{
				if (errorList[i].m_Severity == LevelDetailsManager.ErrorData.Severity.Warning)
				{
					if (num == -1)
					{
						num = i;
					}
					continue;
				}
				T17Text t17Text = Object.Instantiate(m_ErrorPrefab, base.transform);
				if (t17Text != null)
				{
					t17Text.m_bNeedsLocalization = false;
					t17Text.text = errorList[i].m_ErrorString;
					t17Text.transform.localScale = Vector3.one;
					flag = false;
					m_ErrorPrefabs.Add(t17Text);
				}
			}
		}
		if (m_Passed != null)
		{
			if (flag)
			{
				if (num == -1)
				{
					m_Passed.m_bNeedsLocalization = true;
					m_Passed.SetNewLocalizationTag("Text.Editor.CheckerPassed");
				}
				else
				{
					m_Passed.SetNonLocalizedText(errorList[num].m_ErrorString);
				}
			}
			m_Passed.gameObject.SetActive(flag);
		}
		if (m_ScrollView != null)
		{
			m_ScrollView.verticalNormalizedPosition = 1f;
		}
	}
}
