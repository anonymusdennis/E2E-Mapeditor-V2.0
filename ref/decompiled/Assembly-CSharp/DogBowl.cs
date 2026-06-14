using System;
using UnityEngine;

public class DogBowl : AnimatedInteraction
{
	public enum Stages
	{
		Invalid = -1,
		NoBowl,
		BowlPlaced,
		FoodPlaced
	}

	public delegate void DogBowlStageHandler(DogBowl sender);

	public ItemData m_FoodReceptacleItem;

	public ItemData m_DogFood;

	public GameObject m_BowlPlacedGO;

	public GameObject m_FoodPlacedGO;

	public string m_NoBowlInteractionText = "Text.Interact.CanineA";

	public string m_NoFoodInteractionText = "Text.Interact.CanineB";

	public Animator m_MainAnimator;

	public string m_IdleStateName = "Idle";

	public string m_GoToBowlPlacedTrigger = "GoToBowlPlaced";

	public string m_FoodPlacedTrigger = "GoToFoodPlaced";

	public string m_BowlPlacedSound = "Play_Jobs_Dog_Food_Bowl";

	public string m_FoodPlacedSound = "Play_Jobs_Dog_Food_Fill";

	private Stages m_Stage = Stages.Invalid;

	private ItemInteraction m_ItemInteraction;

	private ItemContainer m_ItemContainer;

	public event DogBowlStageHandler OnDogBowlFedEvent;

	protected override void Awake()
	{
		base.Awake();
		m_ItemContainer = GetComponent<ItemContainer>();
		m_ItemContainer.m_MaxSize = 4;
		m_ItemInteraction = GetComponent<ItemInteraction>();
		m_NetObjectLock = GetComponent<NetObjectLock>();
	}

	protected override void Init()
	{
		base.Init();
		if (m_FoodReceptacleItem == null)
		{
		}
		m_ItemInteraction.SetTransferItemTypes(new ItemData[1] { m_FoodReceptacleItem });
		m_ItemInteraction.SetTransferDirection(TransferItemsInteraction.TransferDirection.FromCharacter);
		ItemContainer itemContainer = m_ItemContainer;
		itemContainer.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Combine(itemContainer.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(ItemContainer_OnItemAdded));
		RPC_SetState(Stages.NoBowl, playEffects: false);
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return localCharacter.m_CharacterRole == CharacterRole.Dog && m_Stage == Stages.FoodPlaced;
	}

	public override bool InteractionVisibility()
	{
		return m_Stage != Stages.FoodPlaced;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	private void OnFoodPlaced()
	{
		SetStateRPC(Stages.FoodPlaced, playAudio: true);
		if (this.OnDogBowlFedEvent != null)
		{
			this.OnDogBowlFedEvent(this);
		}
	}

	protected override void OnDestroy()
	{
		this.OnDogBowlFedEvent = null;
		m_ItemInteraction = null;
		ItemContainer itemContainer = m_ItemContainer;
		itemContainer.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Remove(itemContainer.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(ItemContainer_OnItemAdded));
		m_ItemContainer = null;
		base.OnDestroy();
	}

	private void ItemContainer_OnItemAdded(ItemContainer container, Item item, bool intoHidden)
	{
		if (item.ItemDataID == m_FoodReceptacleItem.m_ItemDataID && m_Stage == Stages.NoBowl)
		{
			SetStateRPC(Stages.BowlPlaced, playAudio: true);
		}
		else if (item.ItemDataID == m_DogFood.m_ItemDataID && m_Stage == Stages.BowlPlaced)
		{
			OnFoodPlaced();
		}
	}

	private void EnableDisableObjectsForStage(Stages stage)
	{
		if (m_BowlPlacedGO != null)
		{
			if (stage >= Stages.BowlPlaced)
			{
				m_BowlPlacedGO.SetActive(value: true);
			}
			else
			{
				m_BowlPlacedGO.SetActive(value: false);
			}
		}
		if (m_FoodPlacedGO != null)
		{
			if (stage >= Stages.FoodPlaced)
			{
				m_FoodPlacedGO.SetActive(value: true);
			}
			else
			{
				m_FoodPlacedGO.SetActive(value: false);
			}
		}
		if (m_NetObjectLock != null)
		{
			m_NetObjectLock.m_bIsVisibleToProximityDetector = true;
			switch (stage)
			{
			case Stages.NoBowl:
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_NoBowlInteractionText, localise: true);
				break;
			case Stages.BowlPlaced:
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(m_NoFoodInteractionText, localise: true);
				break;
			case Stages.FoodPlaced:
				m_NetObjectLock.m_bIsVisibleToProximityDetector = false;
				break;
			}
		}
	}

	private void SetStateRPC(Stages stage, bool playAudio)
	{
		m_NetViewID.PostLevelLoadRPC("RPC_SetState", NetTargets.All, stage, playAudio);
	}

	[PunRPC]
	public void RPC_SetState(Stages stage, bool playEffects)
	{
		if (m_Stage != stage)
		{
			m_Stage = stage;
			EnableDisableObjectsForStage(m_Stage);
			if (m_MainAnimator != null)
			{
				m_MainAnimator.ResetTrigger(m_FoodPlacedTrigger);
				m_MainAnimator.ResetTrigger(m_GoToBowlPlacedTrigger);
				switch (stage)
				{
				case Stages.NoBowl:
					if (m_MainAnimator.GetCurrentAnimatorStateInfo(0).IsName(m_IdleStateName))
					{
						m_MainAnimator.Play(m_IdleStateName, 0, UnityEngine.Random.Range(0f, 1f));
					}
					else
					{
						m_MainAnimator.CrossFade(m_IdleStateName, 0f, 0, UnityEngine.Random.Range(0f, 1f));
					}
					break;
				case Stages.BowlPlaced:
				case Stages.FoodPlaced:
					if (stage == Stages.FoodPlaced && playEffects)
					{
						m_MainAnimator.SetTrigger(m_FoodPlacedTrigger);
					}
					else
					{
						m_MainAnimator.SetTrigger(m_GoToBowlPlacedTrigger);
					}
					break;
				}
			}
			if (playEffects)
			{
				switch (stage)
				{
				case Stages.BowlPlaced:
					if (!string.IsNullOrEmpty(m_BowlPlacedSound))
					{
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_BowlPlacedSound, base.gameObject);
					}
					break;
				case Stages.FoodPlaced:
					if (!string.IsNullOrEmpty(m_FoodPlacedSound))
					{
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_FoodPlacedSound, base.gameObject);
					}
					break;
				}
			}
		}
		switch (stage)
		{
		case Stages.BowlPlaced:
			m_ItemInteraction.SetTransferItemTypes(new ItemData[1] { m_DogFood });
			break;
		case Stages.NoBowl:
			m_ItemInteraction.SetTransferItemTypes(new ItemData[1] { m_FoodReceptacleItem });
			break;
		}
	}

	public Stages GetStage()
	{
		return m_Stage;
	}

	public void RemoveAllItems()
	{
		m_ItemContainer.RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
		SetStateRPC(Stages.NoBowl, playAudio: false);
	}

	public ItemInteraction GetItemInteraction()
	{
		return m_ItemInteraction;
	}
}
