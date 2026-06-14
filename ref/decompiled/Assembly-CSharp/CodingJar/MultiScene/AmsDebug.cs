using UnityEngine;

namespace CodingJar.MultiScene;

public static class AmsDebug
{
	internal static void EditorConditionalRestoreAllCrossSceneReferences()
	{
	}

	public static void Log(Object context, string message, params object[] parms)
	{
	}

	public static void LogWarning(Object context, string message, params object[] parms)
	{
		Debug.LogWarningFormat(context, "Ams Plugin: " + message, parms);
	}

	public static void LogError(Object context, string message, params object[] parms)
	{
		Debug.LogErrorFormat(context, "Ams Plugin: " + message, parms);
	}

	public static void LogPerf(Object context, string message, params object[] parms)
	{
	}
}
