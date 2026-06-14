using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ContinueGameValidator : MonoBehaviour
{
	[Serializable]
	public class myClickEvent : UnityEvent
	{
	}

	public T17Text m_ContinueText;

	[SerializeField]
	public myClickEvent OnContinueSuccessful;

	[ReadOnly]
	public string m_CurrentPrison = string.Empty;

	[ReadOnly]
	public bool m_SaveGameAvailable;

	private void Start()
	{
	}

	private void Update()
	{
		UpdateContinueText();
	}

	private void UpdateContinueText()
	{
		string currentSelectedLevel = GlobalStart.GetInstance().GetCurrentSelectedLevel();
		if (!(m_CurrentPrison != currentSelectedLevel))
		{
			return;
		}
		m_CurrentPrison = currentSelectedLevel;
		m_SaveGameAvailable = IsThereASaveFile();
		if (m_ContinueText != null)
		{
			if (m_SaveGameAvailable)
			{
				m_ContinueText.text = "Saved on: " + File.GetLastWriteTime("Save_" + m_CurrentPrison + ".json").ToString();
			}
			else
			{
				m_ContinueText.text = "No save file";
			}
		}
	}

	public void ContinuePressed()
	{
		UpdateContinueText();
		if (m_SaveGameAvailable)
		{
			OnContinueSuccessful.Invoke();
		}
	}

	private bool IsThereASaveFile()
	{
		return File.Exists("Save_" + m_CurrentPrison + ".json");
	}
}
