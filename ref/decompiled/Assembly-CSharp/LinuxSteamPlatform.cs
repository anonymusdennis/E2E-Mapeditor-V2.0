using System.IO;
using UnityEngine;

public class LinuxSteamPlatform : SteamPlatform
{
	public override bool GetSaveDirectory()
	{
		return true;
	}

	public override string StreamingAssetsPath()
	{
		return Path.Combine(Application.streamingAssetsPath, "Steam\\Linux");
	}
}
