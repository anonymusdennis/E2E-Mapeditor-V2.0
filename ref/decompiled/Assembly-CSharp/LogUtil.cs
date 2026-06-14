using System;
using System.Diagnostics;
using UnityEngine;

public static class LogUtil
{
	private static GUIText m_ImmediateGUILogText;

	private static GUIText m_ImmediateUpdatingGUILogText;

	private static int m_GUICharacterClearLimit = 3000;

	private static int m_LineBreakLimit = 20;

	private static int m_LineBreaks;

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void Log(string strLog, string strSubject = "")
	{
		StartLogReflection();
		string message = strLog + GetStackTrace();
		UnityEngine.Debug.Log(message);
		StopLogReflection();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void Log(bool bMustBeTrue, string strLog, string strSubject = "")
	{
		if (bMustBeTrue)
		{
			string message = strLog + GetStackTrace();
			StartLogReflection();
			UnityEngine.Debug.Log(message);
			StopLogReflection();
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void Log(string strLog, int iStackIndex)
	{
		StartLogReflection();
		string message = strLog + GetStackTrace();
		UnityEngine.Debug.Log(message);
		StopLogReflection();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogError(string strLog, string strSubject = "")
	{
		StartLogReflection();
		string message = strLog + GetStackTrace();
		UnityEngine.Debug.LogError(message);
		StopLogReflection();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogError(bool bMustBeTrue, string strLog, string strSubject = "")
	{
		if (bMustBeTrue)
		{
			StartLogReflection();
			UnityEngine.Debug.LogError(strLog + GetStackTrace());
			StopLogReflection();
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogError(string strLog, int iStackIndex)
	{
		StartLogReflection();
		string message = strLog + GetStackTrace();
		UnityEngine.Debug.LogError(message);
		StopLogReflection();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogWarning(string strLog, string strSubject = "")
	{
		StartLogReflection();
		string message = strLog + GetStackTrace();
		UnityEngine.Debug.LogWarning(message);
		StopLogReflection();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogWarning(bool bMustBeTrue, string strLog, string strSubject = "")
	{
		if (bMustBeTrue)
		{
			StartLogReflection();
			string message = strLog + GetStackTrace();
			UnityEngine.Debug.LogWarning(message);
			StopLogReflection();
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogWarning(string strLog, int iStackIndex)
	{
		StartLogReflection();
		string message = strLog + GetStackTrace();
		UnityEngine.Debug.LogWarning(message);
		StopLogReflection();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogException(Exception e, string strSubject = "")
	{
		StartLogReflection();
		UnityEngine.Debug.LogException(e);
		StopLogReflection();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogException(bool bMustBeTrue, Exception e, string strSubject = "")
	{
		if (bMustBeTrue)
		{
			StartLogReflection();
			UnityEngine.Debug.LogException(e);
			StopLogReflection();
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogException(Exception e, int iStackIndex)
	{
		StartLogReflection();
		UnityEngine.Debug.LogException(e);
		StopLogReflection();
	}

	private static string GetStackTrace(int skipFrames = 2)
	{
		StackTrace stackTrace = new StackTrace(skipFrames, fNeedFileInfo: true);
		StackFrame frame = stackTrace.GetFrame(0);
		string text = frame.GetFileName();
		int num = text.LastIndexOf("Assets\\");
		if (num >= 0)
		{
			text = text.Substring(num + 7);
		}
		return string.Concat("\n", text, ": <b>", frame.GetMethod(), "</b> - Line <b>", frame.GetFileLineNumber(), "</b>\n\n");
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void GUILogError(string strLog, string strSubject)
	{
		if (m_ImmediateGUILogText == null)
		{
			CreateLogGUI();
		}
		if (m_ImmediateGUILogText != null)
		{
			CheckGUILimit();
			GUIText immediateGUILogText = m_ImmediateGUILogText;
			immediateGUILogText.text = immediateGUILogText.text + "<color=#800000ff>ERROR: " + strLog + "\n</color>";
			m_LineBreaks++;
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void GUILogWarning(string strLog, string strSubject)
	{
		if (m_ImmediateGUILogText == null)
		{
			CreateLogGUI();
		}
		if (m_ImmediateGUILogText != null)
		{
			CheckGUILimit();
			GUIText immediateGUILogText = m_ImmediateGUILogText;
			immediateGUILogText.text = immediateGUILogText.text + "<color=#ffa500ff>WARNING: " + strLog + "\n</color>";
			m_LineBreaks++;
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void GUILogDebug(string strLog, string strSubject)
	{
		UnityEngine.Debug.Log(strLog + GetStackTrace());
		if (m_ImmediateGUILogText == null)
		{
			CreateLogGUI();
		}
		if (m_ImmediateGUILogText != null)
		{
			CheckGUILimit();
			GUIText immediateGUILogText = m_ImmediateGUILogText;
			immediateGUILogText.text = immediateGUILogText.text + "<color=#222222ff>DEBUG: " + strLog + "</color>\n";
			m_LineBreaks++;
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void GUILogDebug_Updating(string strLog, string strSubject, string colour = "222222")
	{
		if (m_ImmediateUpdatingGUILogText == null)
		{
			CreateLogGUI_Updating();
		}
		if (m_ImmediateUpdatingGUILogText != null)
		{
			m_ImmediateUpdatingGUILogText.text = "DEBUG: <color=#" + colour + "ff>" + strLog + "</color>";
		}
	}

	private static void CreateLogGUI()
	{
		GameObject gameObject = new GameObject("LogUtil_GUITEXT");
		m_ImmediateGUILogText = gameObject.AddComponent<GUIText>();
		m_ImmediateGUILogText.anchor = TextAnchor.UpperLeft;
		m_ImmediateGUILogText.transform.position = new Vector3(0f, 0.85f, 0f);
		m_ImmediateGUILogText.fontSize = 15;
		m_ImmediateGUILogText.fontStyle = FontStyle.BoldAndItalic;
		m_ImmediateGUILogText.text = string.Empty;
		m_ImmediateGUILogText.richText = true;
	}

	private static void CreateLogGUI_Updating()
	{
		GameObject gameObject = new GameObject("LogUtil_GUITEXT_UPDATING");
		m_ImmediateUpdatingGUILogText = gameObject.AddComponent<GUIText>();
		m_ImmediateUpdatingGUILogText.anchor = TextAnchor.UpperRight;
		m_ImmediateUpdatingGUILogText.transform.position = new Vector3(0.99f, 0.85f, 0f);
		m_ImmediateUpdatingGUILogText.fontSize = 15;
		m_ImmediateUpdatingGUILogText.fontStyle = FontStyle.BoldAndItalic;
		m_ImmediateUpdatingGUILogText.text = string.Empty;
		m_ImmediateUpdatingGUILogText.richText = true;
	}

	private static void CheckGUILimit()
	{
		if (m_ImmediateGUILogText.text.Length > m_GUICharacterClearLimit || m_LineBreaks >= m_LineBreakLimit)
		{
			LogGUIClear();
		}
	}

	public static void LogGUIClear()
	{
		m_LineBreaks = 0;
		if (m_ImmediateGUILogText != null)
		{
			m_ImmediateGUILogText.text = string.Empty;
		}
	}

	public static void ToggleGUILog()
	{
		if (m_ImmediateUpdatingGUILogText != null)
		{
			m_ImmediateUpdatingGUILogText.enabled = !m_ImmediateUpdatingGUILogText.enabled;
		}
		if (m_ImmediateGUILogText != null)
		{
			m_ImmediateGUILogText.enabled = !m_ImmediateGUILogText.enabled;
		}
	}

	public static void SetGUIMaxLineBreaks(int maxLinebreaks = 20)
	{
		m_LineBreakLimit = maxLinebreaks;
	}

	public static void SetGUIMaxCharacters(int charLimit = 3000)
	{
		m_GUICharacterClearLimit = charLimit;
	}

	private static void StartLogReflection()
	{
	}

	private static void StopLogReflection()
	{
	}
}
