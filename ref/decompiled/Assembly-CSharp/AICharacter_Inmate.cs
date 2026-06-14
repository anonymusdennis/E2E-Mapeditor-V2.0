using System;
using System.Collections.Generic;
using Rotorz.Tile;
using UnityEngine;

public class AICharacter_Inmate : AICharacter
{
	private float m_fRandomAttackTimer;

	private float m_fLowOpinionAttackTimer;

	private AIEventMemory m_LowOpinionAttackMemory;

	private int m_InmateSnitchLikeOpinion = 75;

	private float m_fInmateSnitchToGuardMaxDistance = 20f;

	private bool m_bAllowedToGetChanged;

	private bool m_bShouldReevaluateEquipped;

	protected override void OnStart()
	{
		m_fRandomAttackTimer = ConfigManager.GetInstance().aiConfig.GetRandomAttackTime(m_CharacterPersonality);
		m_fLowOpinionAttackTimer = OpinionManager.GetInstance().GetInmateLowOpinionAttackInterval();
		m_InmateSnitchLikeOpinion = ConfigManager.GetInstance().aiConfig.GetInmateSnitchLikeOpinion();
		m_fInmateSnitchToGuardMaxDistance = ConfigManager.GetInstance().aiConfig.GetInmateSnitchToGuardMaxDistance();
		if (m_ItemContainer != null)
		{
			ItemContainer itemContainer = m_ItemContainer;
			itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(QueueEquipBestWeapon));
		}
		RoutineManager.GetInstance().OnRoutineEnded += OnRoutineEnd;
	}

	protected override void OnBecameMasterClientBody()
	{
		base.OnBecameMasterClientBody();
		QueueEquipBestWeapon();
	}

	protected override void OnUpdate()
	{
		if (m_Character.GetIsKnockedOut() || m_Character.GetIsDisabled() || m_Character.GetIsMedicalSleeping())
		{
			return;
		}
		if (m_fRandomAttackTimer > 0f)
		{
			m_fRandomAttackTimer -= UpdateManager.deltaTime;
			if (m_fRandomAttackTimer <= 0f)
			{
				AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
				m_fRandomAttackTimer = aiConfig.GetRandomAttackTime(m_CharacterPersonality);
				RandomAttack();
			}
		}
		if (m_fLowOpinionAttackTimer > 0f)
		{
			m_fLowOpinionAttackTimer -= UpdateManager.deltaTime;
			if (m_fLowOpinionAttackTimer <= 0f && !LowOpinionAttack())
			{
				m_fLowOpinionAttackTimer = OpinionManager.GetInstance().GetInmateLowOpinionAttackInterval();
			}
		}
		if (m_bShouldReevaluateEquipped && RoutineManager.GetInstance() != null && RoutineManager.GetInstance().GetCurrentRoutineBaseType() != Routines.JobTime)
		{
			EquipBestWeapon();
			m_bShouldReevaluateEquipped = false;
		}
	}

	protected bool LowOpinionAttack()
	{
		if (m_LowOpinionAttackMemory != null)
		{
			return false;
		}
		if (RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.LightsOut)
		{
			return false;
		}
		if (RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.RollCall)
		{
			return false;
		}
		if (m_CharacterStats.Energy < 30f || m_CharacterStats.Health < 30f)
		{
			return false;
		}
		IList<Character> allHatedCharacters = m_Character.m_CharacterOpinions.GetAllHatedCharacters();
		if (allHatedCharacters.Count > 0)
		{
			for (int i = 0; i < allHatedCharacters.Count; i++)
			{
				bool haveCollisionData = false;
				if (m_CharacterUtil.LineOfSight(allHatedCharacters[i].gameObject, out haveCollisionData))
				{
					Character character = allHatedCharacters[i];
					if (character != null && character.m_CharacterEventManager != null)
					{
						AIEvent attackingAIEvent = character.m_CharacterEventManager.GetAttackingAIEvent();
						AddEvent(attackingAIEvent, silent: true);
						m_LowOpinionAttackMemory = FindEventInMemory(attackingAIEvent);
						return true;
					}
					break;
				}
			}
		}
		return false;
	}

	protected override void OnMemoryForgotten(AIEventMemory memory)
	{
		if (memory == m_LowOpinionAttackMemory)
		{
			m_fLowOpinionAttackTimer = OpinionManager.GetInstance().GetInmateLowOpinionAttackInterval();
			m_LowOpinionAttackMemory = null;
		}
	}

	public GameObject GetMyBed()
	{
		RoomBlob myCell = m_Character.GetMyCell();
		if (myCell == null)
		{
			return null;
		}
		RoomBlob_Cell roomBlobData = myCell.GetRoomBlobData<RoomBlob_Cell>();
		if (roomBlobData == null)
		{
			return null;
		}
		InteractiveObject cellObject = roomBlobData.GetCellObject(typeof(BedInteraction), m_Character);
		if (cellObject == null)
		{
			return null;
		}
		return cellObject.gameObject;
	}

	public void Snitch(AIEventMemory aiEventMemory)
	{
		if (aiEventMemory == null)
		{
			return;
		}
		if (aiEventMemory.m_TargetCharacter != null)
		{
			int opinionOf = m_Character.GetOpinionOf(aiEventMemory.m_TargetCharacter);
			if (opinionOf >= m_InmateSnitchLikeOpinion)
			{
				m_Character.PauseMovement(3f);
				SpeechManager.GetInstance().SaySomething(m_Character, "Text.Inmate.Snitch.Character.Like", SpeechTone.Positive, 3f, 10);
				aiEventMemory.m_AIEvent.StartCooldown(2f, CharacterRole.Inmate);
			}
			else
			{
				m_Character.PauseMovement(3f);
				SpeechManager.GetInstance().SaySomething(m_Character, "Text.Inmate.Snitch.Character.Dislike", SpeechTone.Negative, 3f, 10);
				CallGuard(aiEventMemory.m_TargetCharacter.transform.position);
				aiEventMemory.m_AIEvent.StartCooldown(2f, CharacterRole.Inmate);
			}
		}
		else
		{
			m_Character.PauseMovement(2f);
			SpeechManager.GetInstance().SaySomething(m_Character, "Text.Inmate.Snitch.Prison", SpeechTone.Negative, 2f, 10);
			aiEventMemory.m_AIEvent.StartCooldown(20f, CharacterRole.Inmate);
		}
		ForgetEvent(aiEventMemory);
	}

	private void CallGuard(Vector3 targetLocation)
	{
		int row = -1;
		int column = -1;
		if (FloorManager.GetInstance().GetTileGridPoint(m_Character.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Wall, targetLocation, out row, out column))
		{
			TileData tile = FloorManager.GetInstance().GetTile(m_Character.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, row, column);
			if (tile != null && tile.gameObject != null)
			{
				AIEvent investigateLocationEvent = AIEventManager.GetInstance().GetInvestigateLocationEvent(tile.gameObject.transform);
				NPCManager.GetInstance().CallGuards(investigateLocationEvent, m_fInmateSnitchToGuardMaxDistance);
			}
		}
	}

	public void OnRoutineEnd(RoutinesData.Routine routine, bool forceEnd)
	{
		if (routine.m_BaseRoutineType == Routines.LightsOut && !HasOutfit())
		{
			m_bAllowedToGetChanged = true;
		}
	}

	public bool AllowedToGetChanged()
	{
		return m_bAllowedToGetChanged;
	}

	public override void EquipDefaultOutfit()
	{
		base.EquipDefaultOutfit();
		m_bAllowedToGetChanged = false;
	}

	public override void OnRegainConsciousness()
	{
		base.OnRegainConsciousness();
		QueueEquipBestWeapon();
	}

	public void QueueEquipBestWeapon()
	{
		m_bShouldReevaluateEquipped = true;
	}

	public void EquipBestWeapon()
	{
		if (m_Character.GetIsKnockedOut() || m_Character.m_bIsBound || m_Character.GetIsDisabled() || m_Character.m_ItemContainer == null)
		{
			return;
		}
		int itemCount = m_Character.m_ItemContainer.GetItemCount();
		Item item = m_Character.GetEquippedItem();
		Item item2 = item;
		float num = -1f;
		if (item != null)
		{
			num = GetItemWeaponScore(item);
		}
		for (int i = 0; i < itemCount; i++)
		{
			Item item3 = m_ItemContainer.GetItem(i);
			float itemWeaponScore = GetItemWeaponScore(item3);
			if (itemWeaponScore > num)
			{
				item = item3;
				num = itemWeaponScore;
			}
		}
		if (item != null && item2 != item)
		{
			m_Character.SetEquippedItem(item);
		}
	}

	private float GetItemWeaponScore(Item item)
	{
		if (item == null)
		{
			return -1f;
		}
		if (item.CombatData == null || !item.m_ItemData.m_CanBeEquiped || item.IsQuestItem())
		{
			return -1f;
		}
		if ((bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Key))
		{
			return -1f;
		}
		if ((bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Keycard))
		{
			return -1f;
		}
		float num = item.CombatData.m_fAttackRange;
		CombatConfig combatConfig = item.CombatData.m_CombatConfig;
		if (combatConfig != null)
		{
			num += combatConfig.GetNormalAttackDamage(StrengthModifier.Strength3);
		}
		return num;
	}
}
