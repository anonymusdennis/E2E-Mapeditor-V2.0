using UnityEngine;

[CreateAssetMenu(fileName = "DLCFrontendData", menuName = "Team17/DLC/Create Frontend Data")]
public class DLCFrontendData : ScriptableObject
{
	[Localization]
	public string m_NameLocalizationKey;

	[Localization]
	public string m_DescriptionLocalizationKey;

	[SerializeField]
	private string m_PreviewImageAssetPath;

	[SerializeField]
	private string m_PopupImageAssetPath;

	private Sprite m_PreviewImageCache;

	private Sprite m_PopupImageCache;

	public string m_DLCID;

	public int m_PopupOrder;

	public bool m_bFreeDLC;

	public bool m_bShowOnDlcPage = true;

	public bool m_bOnlyShowIfPrisonSavesExist;

	public bool m_PC = true;

	public bool m_PS4_SCEE = true;

	public bool m_PS4_SCEA = true;

	public bool m_PS4_SCEJ = true;

	public bool m_XboxOne = true;

	public bool m_Switch = true;

	public bool m_PopupPC = true;

	public bool m_PopupPS4_SCEE = true;

	public bool m_PopupPS4_SCEA = true;

	public bool m_PopupPS4_SCEJ = true;

	public bool m_PopupXboxOne = true;

	public bool m_PopupSwitch = true;

	public string m_ForceDialogTitle = string.Empty;

	public Sprite m_PreviewImage => (!(m_PreviewImageCache != null)) ? (m_PreviewImageCache = Resources.Load<Sprite>(m_PreviewImageAssetPath)) : m_PreviewImageCache;

	public Sprite m_PopupImage => (!(m_PopupImageCache != null)) ? (m_PopupImageCache = Resources.Load<Sprite>(m_PopupImageAssetPath)) : m_PopupImageCache;

	public bool IsAvailableOnThisPlatform()
	{
		return m_PC;
	}

	public bool ShowPopupOnThisPlatform()
	{
		return m_PopupPC;
	}
}
