using System;

public class TalkInteraction : InteractiveObject
{
	public Character m_ThisCharacter;

	private bool m_bDeliveredSomething;

	private bool m_bTooManyQuests;

	protected override void Init()
	{
		base.Init();
		if (!(m_ThisCharacter == null))
		{
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (!localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		Player player = (Player)localCharacter;
		if (player.TryDeliverItem != null)
		{
			Delegate[] invocationList = player.TryDeliverItem.GetInvocationList();
			m_bDeliveredSomething = false;
			for (int i = 0; i < invocationList.Length; i++)
			{
				Player.DeliverItem deliverItem = (Player.DeliverItem)invocationList[i];
				if (deliverItem != null)
				{
					m_bDeliveredSomething |= deliverItem(player, m_ThisCharacter, onlyCheck: false);
				}
			}
			if (m_bDeliveredSomething)
			{
				SpeechManager.GetInstance().SaySomething(m_ThisCharacter, "Text.Player.ThanksForThat", SpeechTone.Positive, 3f, 10);
				return;
			}
		}
		bool flag = IsThereRobinsonQuestMessage(localCharacter);
		m_bTooManyQuests = player.ActiveQuests >= 6 || m_ThisCharacter == null || (!m_ThisCharacter.m_bHasQuestAvailable && !flag);
		if (!m_bTooManyQuests)
		{
			localCharacter.m_OpenContainer = m_ThisCharacter.m_ItemContainer;
			if (m_ThisCharacter.m_bIsRobinsonCharacter)
			{
				if (flag)
				{
					((Player)localCharacter).ViewContainer(localCharacter.m_OpenContainer, InGameRootMenu.InGameMenuTypeToOpen.RobinsonContinueFavour);
				}
				else
				{
					((Player)localCharacter).ViewContainer(localCharacter.m_OpenContainer, InGameRootMenu.InGameMenuTypeToOpen.RobinsonFavour);
				}
			}
			else
			{
				((Player)localCharacter).ViewContainer(localCharacter.m_OpenContainer, InGameRootMenu.InGameMenuTypeToOpen.FavourInmate);
			}
		}
		else
		{
			SpeechManager.GetInstance().SaySomething(m_ThisCharacter, "Text.Player.YouSeemTooBusy", SpeechTone.Positive, 5f);
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public override void Server_OnLockStatusChanged(int characterId, bool getLock)
	{
		PhotonView photonView = PhotonView.Find(characterId);
		if (photonView == null)
		{
			return;
		}
		Character component = photonView.GetComponent<Character>();
		if (component == null)
		{
			return;
		}
		if (getLock)
		{
			m_ThisCharacter.SetBusyRPC(busy: true);
			component.SetBusyRPC(busy: true);
			bool flag = m_ThisCharacter.IsInteracting();
			bool isKnockedOut = m_ThisCharacter.GetIsKnockedOut();
			if (!flag && !isKnockedOut && m_ThisCharacter.m_CharacterAnimator.m_CharacterAnimator != null)
			{
				m_ThisCharacter.FaceCharacter(component);
			}
		}
		else
		{
			m_ThisCharacter.SetBusyRPC(busy: false);
			component.SetBusyRPC(busy: false);
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_interactingCharacter != null && (m_bDeliveredSomething || m_bTooManyQuests))
		{
			RequestStopInteraction(m_interactingCharacter);
			m_bDeliveredSomething = false;
			m_bTooManyQuests = false;
		}
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (m_ThisCharacter.m_bIsKnockedOut)
		{
			return false;
		}
		if (!m_ThisCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AICharacter component = m_ThisCharacter.GetComponent<AICharacter>();
			if (component != null && component.IsInCombatState())
			{
				return false;
			}
		}
		bool flag = false;
		if (localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)localCharacter;
			if (player.TryDeliverItem != null)
			{
				Delegate[] invocationList = player.TryDeliverItem.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					Player.DeliverItem deliverItem = (Player.DeliverItem)invocationList[i];
					if (deliverItem != null)
					{
						flag |= deliverItem(player, m_ThisCharacter, onlyCheck: true);
					}
				}
			}
			else if (!player.m_NetView.isMine)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			flag = m_ThisCharacter.m_bHasQuestAvailable || IsThereRobinsonQuestMessage(localCharacter);
		}
		return m_ThisCharacter != null && flag;
	}

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}

	private bool IsThereRobinsonQuestMessage(Character localCharacter)
	{
		if (m_ThisCharacter.m_bIsRobinsonCharacter && localCharacter.IsPlayer())
		{
			QuestManager instance = QuestManager.GetInstance();
			if (instance != null)
			{
				QuestManager.QuestGiver questGiver = instance.GetQuestGiver(m_ThisCharacter);
				if (questGiver != null && questGiver.IsAccepted && object.ReferenceEquals(questGiver.m_PlayerDoingQuest, localCharacter) && !string.IsNullOrEmpty(questGiver.m_QuestGiverMessage))
				{
					return true;
				}
			}
		}
		return false;
	}
}
