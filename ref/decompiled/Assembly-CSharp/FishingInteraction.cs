using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class FishingInteraction : AnimatedInteraction
{
	private const float kGiveFishTime = 2.3f;

	private const float kCompletionTime = 3f;

	public ItemData m_FishingRodItem;

	public ItemData m_BaitedFishingRodItem;

	public ItemData m_FishRewardItem;

	public string m_RequireFishingRodDialogID = "Text.Fishing.NeedFishingRod";

	public string m_RequireBaitedFishingRodDialogID = "Text.Fishing.NeedBaitedFishingRod";

	public string m_InventoryFullDialogID = "Text.Fishing.FullInventory";

	public float m_InteractingZOffset = -0.1f;

	private float m_Timer;

	private bool m_FishGiven;

	private FastList<MeshRenderer> m_Renderers = new FastList<MeshRenderer>();

	private Dictionary<int, Character> m_FishRewardMap = new Dictionary<int, Character>();

	protected override void Init()
	{
		base.Init();
		if (m_InteractionObjectAnimator != null)
		{
			MeshRenderer[] componentsInChildren = m_InteractionObjectAnimator.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				m_Renderers.AddRange(componentsInChildren);
			}
		}
		RPC_ShowFishingRod(bShow: false);
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (localCharacter == null)
		{
			return false;
		}
		Item equippedItem = localCharacter.GetEquippedItem();
		if (equippedItem == null || equippedItem.ItemDataID != m_BaitedFishingRodItem.m_ItemDataID)
		{
			if (equippedItem != null && equippedItem.ItemDataID == m_FishingRodItem.m_ItemDataID)
			{
				SpeechManager.GetInstance().SaySomething(localCharacter, m_RequireBaitedFishingRodDialogID, SpeechTone.Negative);
			}
			else
			{
				SpeechManager.GetInstance().SaySomething(localCharacter, m_RequireFishingRodDialogID, SpeechTone.Negative);
			}
			return false;
		}
		return true;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_Timer = 0f;
		m_FishGiven = false;
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		ShowFishingRod(bShow: true);
		if (m_interactingCharacter != null && m_interactingCharacter.m_CharacterStats != null && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Pickup_Item, m_interactingCharacter.gameObject);
		}
	}

	public override void InteractionReadyUpdate()
	{
		m_Timer += UpdateManager.deltaTime;
		if (m_Timer >= 2.3f && !m_FishGiven)
		{
			m_FishGiven = true;
			if (m_FishRewardItem != null && m_interactingCharacter != null)
			{
				int requestID = ItemManager.GetInstance().GetNextRequestID();
				m_FishRewardMap.Add(requestID, m_interactingCharacter);
				ItemManager.GetInstance().AssignItemRPC(m_interactingCharacter.m_NetView.ownerId, m_FishRewardItem.m_ItemDataID, OnItemMgrResponse, ref requestID);
			}
		}
		if (m_Timer >= 3f)
		{
			RequestStopInteraction(m_interactingCharacter);
		}
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		ShowFishingRod(bShow: false);
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	private void OnItemMgrResponse(Item item, int eventID)
	{
		if (m_FishRewardMap.ContainsKey(eventID))
		{
			Character character = m_FishRewardMap[eventID];
			if (item != null && character != null && !character.m_ItemContainer.AddItemRPC(item))
			{
				SpeechManager.GetInstance().SaySomething(character, m_InventoryFullDialogID, SpeechTone.Negative);
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
			m_FishRewardMap.Remove(eventID);
		}
	}

	protected void ShowFishingRod(bool bShow)
	{
		m_NetViewID.RPC("RPC_ShowFishingRod", NetTargets.All, bShow);
	}

	[PunRPC]
	public virtual void RPC_ShowFishingRod(bool bShow)
	{
		int count = m_Renderers.Count;
		for (int i = 0; i < count; i++)
		{
			m_Renderers._items[i].gameObject.SetActive(bShow);
		}
	}

	protected override void UpdateInteractionZ_PreTransitionStart()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_Interacting()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_PostTransitionEnd()
	{
		SetInteractionZ();
	}

	private void SetInteractionZ()
	{
		if (m_VisualTransform != null && m_interactingCharacter != null)
		{
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + m_InteractingZOffset);
		}
	}
}
