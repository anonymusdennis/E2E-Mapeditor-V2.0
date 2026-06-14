using UnityEngine;

public class CellBed_ItemInteraction : ItemInteraction
{
	public BedEventManager m_BedEventManager;

	public ItemData m_BedSheet;

	public ItemData m_Pillow;

	public ItemData m_BedDummy;

	public Animator m_BedAnimator;

	private const string DUMMY_PARAM = "Dummy";

	private const string PILLOW_PARAM = "Pillow";

	private const string SHEET_PARAM = "Sheet";

	private const string NO_SHEET_PILLOW_STATE = "Pillow On No Sheet";

	private const string NO_SHEET_NO_PILLOW_STATE = "No Pillow No Sheet";

	private const string DUMMY_STATE = "Dummy";

	private bool m_bHasBedSheet = true;

	private bool m_bHasPillow = true;

	private bool m_bHasDummy;

	protected override void Init()
	{
		ItemData[] transferItemTypes = new ItemData[3] { m_BedDummy, m_BedSheet, m_Pillow };
		SetTransferItemTypes(transferItemTypes);
		if (m_BedEventManager == null)
		{
			m_BedEventManager = GetComponent<BedEventManager>();
		}
		if (m_BedAnimator == null)
		{
			m_BedAnimator = GetComponentInChildren<Animator>();
		}
		base.Init();
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (base.AllowedToInteract(localCharacter))
		{
			return m_bHasBedSheet || m_bHasDummy || m_bHasPillow;
		}
		return false;
	}

	public override void TransferEquippedItem(Character character)
	{
		if (character == null)
		{
			return;
		}
		bool flag = true;
		Item equippedItem = character.GetEquippedItem();
		if (equippedItem != null)
		{
			if (!m_bHasPillow && equippedItem.ItemDataID == m_BedSheet.m_ItemDataID)
			{
				flag = false;
				if (character.m_CharacterStats != null && character.m_CharacterStats.m_bIsPlayer)
				{
					SpeechManager.GetInstance().SaySomething(character, "Text.Player.BedMissingPillow", SpeechTone.Negative, 3f, 10);
				}
			}
			else if (equippedItem.ItemDataID == m_BedDummy.m_ItemDataID && (!m_bHasPillow || !m_bHasBedSheet))
			{
				flag = false;
				if (character.m_CharacterStats != null && character.m_CharacterStats.m_bIsPlayer)
				{
					SpeechManager.GetInstance().SaySomething(character, "Text.Player.CannotPlaceBedDummy", SpeechTone.Negative, 3f, 10);
				}
			}
		}
		if (flag)
		{
			base.TransferEquippedItem(character);
		}
	}

	protected override void UpdateState(bool isFromLoad)
	{
		if (!(m_BedSheet == null) && !(m_Pillow == null) && !(m_BedDummy == null))
		{
			m_bHasBedSheet = m_ItemContainer.HasItem(m_BedSheet.m_ItemDataID) > 0;
			m_bHasPillow = m_ItemContainer.HasItem(m_Pillow.m_ItemDataID) > 0;
			m_bHasDummy = m_ItemContainer.HasItem(m_BedDummy.m_ItemDataID) > 0;
			UpdateVisuals(isFromLoad);
			if (m_BedEventManager != null)
			{
				m_BedEventManager.HasDummy(m_bHasDummy);
			}
		}
	}

	private void UpdateVisuals(bool isFromLoad)
	{
		if (!(m_BedAnimator != null))
		{
			return;
		}
		m_BedAnimator.SetBool("Dummy", m_bHasDummy);
		m_BedAnimator.SetBool("Sheet", m_bHasBedSheet);
		m_BedAnimator.SetBool("Pillow", m_bHasPillow);
		if (!isFromLoad)
		{
			return;
		}
		if (m_bHasDummy)
		{
			m_BedAnimator.CrossFade("Dummy", 0f);
		}
		if (!m_bHasBedSheet)
		{
			if (!m_bHasPillow)
			{
				m_BedAnimator.CrossFade("No Pillow No Sheet", 0f);
			}
			else
			{
				m_BedAnimator.CrossFade("Pillow On No Sheet", 0f);
			}
		}
	}

	public bool HasRealPillowAndSheet()
	{
		return m_bHasBedSheet && m_bHasPillow;
	}

	public bool HasBedDummy()
	{
		return m_bHasDummy;
	}

	public override bool ShouldShowNameplateWhenNearby()
	{
		return false;
	}
}
