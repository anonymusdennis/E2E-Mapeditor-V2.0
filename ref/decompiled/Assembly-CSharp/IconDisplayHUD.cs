using UnityEngine;

public class IconDisplayHUD : MonoBehaviour
{
	public AnimatingImage.IconAnimInfo m_MenuIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_RobinsonQuestIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_QuestIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_VendorIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_ClimbUpIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_ClimbDownIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_GuardAlertIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_GuardInvestigateIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_GuardReportIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_MultiplayerSingleIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_MultiplayerDoubleIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_MultiplayerTripleIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_MultiplayerQuadIconAnimInfo;

	public AnimatingImage.IconAnimInfo m_DogMissingKeyIconAnimInfo;

	public AnimatingImage m_AnimatingImage;

	private AnimatingImage.IconAnimInfo m_ActiveInfo;

	private void Awake()
	{
		m_AnimatingImage = GetComponent<AnimatingImage>();
	}

	public void SetIconType(CharacterIconHandler.IconType IType)
	{
		m_ActiveInfo = default(AnimatingImage.IconAnimInfo);
		switch (IType)
		{
		case CharacterIconHandler.IconType.InMenus:
			m_ActiveInfo = m_MenuIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.Quest:
			m_ActiveInfo = m_QuestIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.Vendor:
			m_ActiveInfo = m_VendorIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.ClimbUp:
			m_ActiveInfo = m_ClimbUpIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.ClimbDown:
			m_ActiveInfo = m_ClimbDownIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.RobinsonQuest:
			m_ActiveInfo = m_RobinsonQuestIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.GuardAlert:
			m_ActiveInfo = m_GuardAlertIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.GuardInvestigate:
			m_ActiveInfo = m_GuardInvestigateIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.GuardReport:
			m_ActiveInfo = m_GuardReportIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.MultiplayerSingle:
			m_ActiveInfo = m_MultiplayerSingleIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.MultiplayerDouble:
			m_ActiveInfo = m_MultiplayerDoubleIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.MultiplayerTriple:
			m_ActiveInfo = m_MultiplayerTripleIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.MultiplayerQuad:
			m_ActiveInfo = m_MultiplayerQuadIconAnimInfo;
			break;
		case CharacterIconHandler.IconType.DogMissingKey:
			m_ActiveInfo = m_DogMissingKeyIconAnimInfo;
			break;
		}
		if (m_AnimatingImage == null)
		{
			m_AnimatingImage = GetComponent<AnimatingImage>();
		}
		if (m_AnimatingImage != null)
		{
			m_AnimatingImage.SetAnim(m_ActiveInfo);
		}
	}

	public AnimatingImage.IconAnimInfo GetActiveInfo()
	{
		return m_ActiveInfo;
	}
}
