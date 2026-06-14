using System.Collections.Generic;
using UnityEngine;

public class CookingItemProcessor : DelayedPassiveItemProcessor
{
	public float m_CookedLowerBound;

	public float m_CookedUpperBound;

	public ItemData m_OvercookedItem;

	private ItemData m_CachedPerfectlyCookedItem;

	private Dictionary<Player, OvenUI> m_OvenUIs = new Dictionary<Player, OvenUI>();

	public Vector2 m_CookingUiOffset = new Vector3(0f, 1f, 0f);

	private bool m_bShowHudsForPlayers;

	public string m_AnimatorCookingFlag = "IsCooking";

	protected override void Awake()
	{
		base.Awake();
		Gamer.OnDeleteImminent += Gamer_OnDeleteImminent;
		Gamer.OnCreate += Gamer_OnCreate;
		GatherLocalGamerHuds();
		SetupUIsForTotalProcessingTime();
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (HUDMenuFlow.Instance != null)
		{
			HUDMenuFlow.Instance.SplitscreenScaleChangedEvent += HudMenuFlow_SplitscreenScaleChangedEvent;
		}
		return base.StartInit();
	}

	protected void OnDestroy()
	{
		Gamer.OnDeleteImminent -= Gamer_OnDeleteImminent;
		Gamer.OnCreate -= Gamer_OnCreate;
		if (HUDMenuFlow.Instance != null)
		{
			HUDMenuFlow.Instance.SplitscreenScaleChangedEvent -= HudMenuFlow_SplitscreenScaleChangedEvent;
		}
	}

	private void Gamer_OnCreate(Gamer gamer)
	{
		GatherLocalGamerHuds();
	}

	private void Gamer_OnDeleteImminent(Gamer gamer)
	{
		Player playerObject = gamer.m_PlayerObject;
		if (null != playerObject)
		{
			m_OvenUIs.Remove(playerObject);
		}
	}

	private void HudMenuFlow_SplitscreenScaleChangedEvent(bool isNowScaled)
	{
		foreach (KeyValuePair<Player, OvenUI> ovenUI in m_OvenUIs)
		{
			SetupOvenUI(ovenUI.Value);
		}
	}

	private void GatherLocalGamerHuds()
	{
		Gamer[] localGamers = Gamer.GetLocalGamers();
		for (int i = 0; i < localGamers.Length; i++)
		{
			Player playerObject = localGamers[i].m_PlayerObject;
			if (playerObject == null)
			{
				continue;
			}
			OvensHudContainer ovensHudContainer = HUDMenuFlow.Instance.GetOvensHudContainer(playerObject.m_PlayerCameraManagerBindingID);
			if (ovensHudContainer == null)
			{
			}
			if (m_OvenUIs.ContainsKey(playerObject))
			{
				continue;
			}
			OvenUI ovenUI = ovensHudContainer.RequestUI();
			if (ovenUI != null)
			{
				m_OvenUIs.Add(playerObject, ovenUI);
				if (GetState() == State.ProcessingItem && m_bShowHudsForPlayers)
				{
					ovenUI.gameObject.SetActive(value: true);
				}
				else
				{
					ovenUI.gameObject.SetActive(value: false);
				}
				SetupOvenUI(ovenUI);
			}
		}
	}

	public override void SetProcessingTime(float time)
	{
		base.SetProcessingTime(time);
		SetupUIsForTotalProcessingTime();
	}

	public void SetupUIsForTotalProcessingTime()
	{
		foreach (KeyValuePair<Player, OvenUI> ovenUI in m_OvenUIs)
		{
			SetupOvenUI(ovenUI.Value);
		}
	}

	private void SetupOvenUI(OvenUI ui)
	{
		ui.SetupUI(m_ProcessingTime, m_CookedLowerBound, m_CookedUpperBound);
		float z = ui.transform.position.z;
		ui.SetPosition(new Vector3(base.transform.position.x + m_CookingUiOffset.x, base.transform.position.y + m_CookingUiOffset.y, z));
	}

	protected override void UpdateProcessingItemState()
	{
		base.UpdateProcessingItemState();
		foreach (KeyValuePair<Player, OvenUI> ovenUI in m_OvenUIs)
		{
			ovenUI.Value.UpdateForNormalisedProgress(GetNormalisedProgress());
		}
		if (T17NetManager.IsMasterClient)
		{
			Master_UpdateProducedItemForProcessingTime();
		}
	}

	private void Master_UpdateProducedItemForProcessingTime()
	{
		if (IsItemSpawnInProgress() || JobsManager.GetInstance() == null || ItemManager.GetInstance() == null || m_ItemContainer == null)
		{
			return;
		}
		Item currentlyProducedItem = m_ItemContainer.GetItem(0);
		if (currentlyProducedItem == null)
		{
			return;
		}
		bool flag = false;
		if (JobsManager.GetInstance() != null)
		{
			Character jobEmployee = JobsManager.GetInstance().GetJobEmployee(JobType.Kitchen);
			if (jobEmployee != null && jobEmployee.m_CharacterStats != null)
			{
				flag = jobEmployee.m_CharacterStats.m_bIsPlayer;
			}
		}
		if (!(m_ProcessingTimer > m_CookedLowerBound))
		{
			return;
		}
		if (m_ProcessingTimer > m_CookedUpperBound && flag)
		{
			if (m_OvercookedItem != null && (currentlyProducedItem == null || currentlyProducedItem.ItemDataID != m_OvercookedItem.m_ItemDataID))
			{
				RequestItemCreation(0, m_OvercookedItem.m_ItemDataID);
				if (currentlyProducedItem != null)
				{
					m_ItemContainer.RemoveItemRPC(currentlyProducedItem, releaseToManager: true);
				}
			}
			return;
		}
		if (m_CachedPerfectlyCookedItem == null)
		{
			m_CachedPerfectlyCookedItem = GetOutputItem(currentlyProducedItem);
		}
		if (m_CachedPerfectlyCookedItem == null)
		{
			ItemData itemData = GetOutputItemTypes().Find((ItemData x) => x.m_ItemDataID == currentlyProducedItem.ItemDataID);
			if (itemData != null)
			{
				m_CachedPerfectlyCookedItem = currentlyProducedItem.m_ItemData;
			}
		}
		if (!(m_CachedPerfectlyCookedItem == null) && (currentlyProducedItem == null || currentlyProducedItem.ItemDataID != m_CachedPerfectlyCookedItem.m_ItemDataID))
		{
			RequestItemCreation(0, m_CachedPerfectlyCookedItem.m_ItemDataID);
			if (currentlyProducedItem != null)
			{
				m_ItemContainer.RemoveItemRPC(currentlyProducedItem, releaseToManager: true);
			}
		}
	}

	protected override ItemData GetInitialSpawnItem(Item inputItem)
	{
		return inputItem.m_ItemData;
	}

	public override void Local_SetState(State newState)
	{
		base.Local_SetState(newState);
		switch (newState)
		{
		case State.StartCreateItem:
			foreach (KeyValuePair<Player, OvenUI> ovenUI in m_OvenUIs)
			{
				SetupOvenUI(ovenUI.Value);
			}
			break;
		case State.ProcessingItem:
			GatherLocalGamerHuds();
			if (!m_bShowHudsForPlayers)
			{
				break;
			}
			foreach (KeyValuePair<Player, OvenUI> ovenUI2 in m_OvenUIs)
			{
				ovenUI2.Value.gameObject.SetActive(value: true);
			}
			break;
		case State.Idle:
		case State.Interrupted:
			foreach (KeyValuePair<Player, OvenUI> ovenUI3 in m_OvenUIs)
			{
				ovenUI3.Value.gameObject.SetActive(value: false);
			}
			break;
		}
		bool flag = newState == State.ProcessingItem;
		if (m_Animator != null)
		{
			m_Animator.SetBool(m_AnimatorCookingFlag, flag);
		}
		if (!flag)
		{
			m_Animator.SetBool("HoldSpecialAnim", value: false);
		}
	}

	protected override List<ItemData> GetPossibleOutputs()
	{
		List<ItemData> possibleOutputs = base.GetPossibleOutputs();
		possibleOutputs.Add(m_OvercookedItem);
		return possibleOutputs;
	}

	public void SetUIVisibleForPlayers(bool isVisible)
	{
		m_bShowHudsForPlayers = isVisible;
		if (m_bShowHudsForPlayers)
		{
			return;
		}
		foreach (KeyValuePair<Player, OvenUI> ovenUI in m_OvenUIs)
		{
			ovenUI.Value.gameObject.SetActive(isVisible);
		}
	}
}
