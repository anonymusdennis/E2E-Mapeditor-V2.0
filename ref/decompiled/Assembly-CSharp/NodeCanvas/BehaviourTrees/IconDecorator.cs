using NodeCanvas.Framework;
using ParadoxNotion;

namespace NodeCanvas.BehaviourTrees;

public class IconDecorator : StateDecorator
{
	public BBParameter<CharacterIconHandler.IconType> m_eIconToShow;

	private CharacterIconHandler.IconType m_IconShown;

	private CharacterIconHandler m_IconHandler;

	private bool m_bVisible;

	public override string name
	{
		get
		{
			return "IconDecorator [" + m_eIconToShow.ToStringAdvanced() + "]";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void OnEnter()
	{
		if (m_IconHandler == null && m_AICharacter != null && m_AICharacter.m_Character != null)
		{
			m_IconHandler = m_AICharacter.m_Character.m_IconHandler;
		}
		m_IconShown = m_eIconToShow.value;
		m_bVisible = false;
	}

	protected override void OnExit()
	{
		RemoveIcon();
	}

	protected override void OnUpdate()
	{
		if (!m_AICharacter.m_Character.GetIsKnockedOut() && !m_AICharacter.m_Character.GetIsSleeping() && !m_AICharacter.m_Character.GetIsDisabled())
		{
			if (m_eIconToShow.value != m_IconShown)
			{
				RemoveIcon();
				m_eIconToShow.value = m_IconShown;
			}
			ShowIcon();
		}
	}

	private void ShowIcon()
	{
		if (!m_bVisible)
		{
			m_bVisible = true;
			if (m_IconHandler != null)
			{
				m_IconHandler.DisplayIconRPC(m_IconShown);
			}
		}
	}

	private void RemoveIcon()
	{
		if (m_bVisible)
		{
			m_bVisible = false;
			if (m_IconHandler != null)
			{
				m_IconHandler.RemoveIconRPC(m_IconShown);
			}
		}
	}
}
