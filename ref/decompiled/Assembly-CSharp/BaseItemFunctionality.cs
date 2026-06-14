using UnityEngine;

public abstract class BaseItemFunctionality : ScriptableObject
{
	public delegate void ItemFuncEvent();

	public enum Functionality
	{
		None = -1,
		Key,
		Dig,
		UNUSED_01,
		Chip,
		Cut,
		Unscrew,
		Repair,
		FillHole,
		BraceTunnel,
		StatChange,
		HideContraband,
		Climb,
		Bind,
		CoverTile,
		ItemTransfer,
		Ladder,
		Keycard,
		Sharpen,
		Garden
	}

	public string m_ActionVerb = "Text.Interaction.Use";

	public Sprite m_LeftValidTargetHUDSpriteOverride;

	public Sprite m_RightValidTargetHUDSpriteOverride;

	public Sprite m_UpValidTargetHUDSpriteOverride;

	public Sprite m_DownValidTargetHUDSpriteOverride;

	public bool m_PressAndHoldForMultipleUses;

	public ItemFuncEvent OnStartOfUse;

	public ItemFuncEvent OnEndOfUse;

	protected bool m_bIsCancelInProgress;

	protected Character m_Owner;

	protected Item m_ParentItem;

	private float m_AnimUsingTime;

	private float m_AnimUsingLength;

	public Character Owner
	{
		get
		{
			return m_Owner;
		}
		set
		{
			m_Owner = value;
		}
	}

	public Item ParentItem
	{
		get
		{
			return m_ParentItem;
		}
		set
		{
			m_ParentItem = value;
		}
	}

	public abstract void Init();

	public abstract bool RequiresTargetting();

	public abstract bool RequiresPositioning();

	public abstract bool ImmobilisesOwner();

	public abstract bool IsImmediateUse();

	public abstract bool CanUse(bool intendsOnUsingImmediately = false);

	public abstract Functionality GetFunctionalityType();

	public virtual bool StartUsing(AnimState useAnimation, float useTime)
	{
		m_bIsCancelInProgress = false;
		m_AnimUsingTime = 0f;
		if (m_Owner != null)
		{
			m_AnimUsingLength = m_Owner.m_CharacterAnimator.GetAnimationLength(useAnimation);
		}
		else
		{
			m_AnimUsingLength = 0f;
		}
		if (m_Owner != null && m_Owner.IsPlayer())
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Items Used", m_ParentItem.m_ItemData.m_ItemLocalizationTag + " Used", string.Empty, 0L);
		}
		return true;
	}

	public virtual bool UpdateUsing()
	{
		m_AnimUsingTime += UpdateManager.deltaTime;
		if (m_AnimUsingTime >= m_AnimUsingLength)
		{
			AnimationSinglePlayDone();
			m_AnimUsingTime -= m_AnimUsingLength;
		}
		return true;
	}

	protected virtual void AnimationSinglePlayDone()
	{
	}

	public virtual bool CancelUsing()
	{
		if (!m_bIsCancelInProgress)
		{
			m_bIsCancelInProgress = true;
			return true;
		}
		return false;
	}

	public bool IsCancelInProgress()
	{
		return m_bIsCancelInProgress;
	}

	public Sprite GetValidTargetHUDSpriteOverride(Directionx4 facingDirection)
	{
		Sprite sprite = null;
		return facingDirection switch
		{
			Directionx4.Left => m_LeftValidTargetHUDSpriteOverride, 
			Directionx4.Right => m_RightValidTargetHUDSpriteOverride, 
			Directionx4.Up => m_UpValidTargetHUDSpriteOverride, 
			Directionx4.Down => m_DownValidTargetHUDSpriteOverride, 
			_ => null, 
		};
	}

	protected bool CheckForCharactersAtTilePosition(int row, int column, FloorManager.Floor floor)
	{
		return CharacterAtTilePosition(row, column, floor) != null;
	}

	protected Character CharacterAtTilePosition(int row, int column, FloorManager.Floor floor)
	{
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			int num = instance.BoxCollideTileAreaNonAlloc(floor, row, column, FloorManager.TileSystem_Type.TileSystem_Ground, checkTriggers: true);
			for (int i = 0; i < num; i++)
			{
				if (EscapistsRaycast.ColliderOverlapList[i].transform.parent != null)
				{
					Character component = EscapistsRaycast.ColliderOverlapList[i].transform.parent.GetComponent<Character>();
					if (component != null && component != m_Owner)
					{
						return component;
					}
				}
			}
		}
		return null;
	}

	public virtual bool IsDegradedByDetector()
	{
		return false;
	}
}
