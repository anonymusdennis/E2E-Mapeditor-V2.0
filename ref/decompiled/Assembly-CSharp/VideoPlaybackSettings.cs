using UnityEngine;

[CreateAssetMenu(fileName = "VideoClipSettings", menuName = "Team17/Create Video Clip Settings")]
public class VideoPlaybackSettings : ScriptableObject
{
	[Header("General")]
	public string m_MovieName = string.Empty;

	public bool m_PlaysMusic;

	[Header("PC Settings")]
	public MovieTexture m_MovieTexture;

	[Header("Xbox One")]
	public int m_MovieWidth = 1920;

	public int m_MovieHeight = 1080;

	[Header("PS4 Settings")]
	public Shader m_PS4Shader;

	public Texture m_PS4LuminanceTexture;

	public Material m_PS4VideoMaterial;

	[Header("Switch Settings")]
	public Texture m_SwitchLuminanceTexture;

	public Material m_SwitchVideoMaterial;
}
