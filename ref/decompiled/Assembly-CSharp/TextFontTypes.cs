using UnityEngine;

[CreateAssetMenu(fileName = "NEW_RootMenuFonts", menuName = "Team17/Create Font Types")]
public class TextFontTypes : ScriptableObject
{
	[SerializeField]
	[HideInInspector]
	public Font[] m_SerializedFonts;
}
