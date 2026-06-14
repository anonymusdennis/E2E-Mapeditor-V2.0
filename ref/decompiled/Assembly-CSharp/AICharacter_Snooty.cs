public class AICharacter_Snooty : AICharacter_Generic
{
	public ItemData m_QuestTriggerItem;

	public ConstructEndgameInteraction m_ConstructEndGame;

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)m_Character)
		{
			m_Character.ReceivedGiftEvent += OnReceivedGift;
			QuestManager.GetInstance().RegisterSpecificQuestableCharacter(m_Character);
		}
	}

	protected override void OnDestroy()
	{
		if (m_Character != null)
		{
			m_Character.ReceivedGiftEvent -= OnReceivedGift;
		}
		m_QuestTriggerItem = null;
		m_ConstructEndGame = null;
		base.OnDestroy();
	}

	public override void ControlledUpdate()
	{
		base.ControlledUpdate();
		if (m_ConstructEndGame != null)
		{
			base.gameObject.SetActive(!m_ConstructEndGame.m_IsInteractionEnabled);
		}
	}

	private void OnReceivedGift(Character gifter, int[] itemDataIDs, int money)
	{
		if (!(m_Character != null) || !(m_Character.m_NetView != null) || !m_Character.m_NetView.isMine || !(m_QuestTriggerItem != null) || itemDataIDs == null)
		{
			return;
		}
		for (int i = 0; i < itemDataIDs.Length; i++)
		{
			if (itemDataIDs[i] == m_QuestTriggerItem.m_ItemDataID)
			{
				QuestManager.GetInstance().CreateSpecificCharacterQuest(m_Character);
				break;
			}
		}
	}
}
