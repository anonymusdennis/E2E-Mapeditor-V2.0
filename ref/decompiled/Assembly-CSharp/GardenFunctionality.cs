using UnityEngine;

[CreateAssetMenu(fileName = "Garden Functionality", menuName = "Team17/Items/Functionalities/Create Garden Functionality")]
public class GardenFunctionality : BaseItemFunctionality
{
	private AnimState m_Animation = AnimState.UseLow;

	private float m_AnimTime = 0.3f;

	private float m_TotalUsingTime;

	private int m_LastResolvedTileRow = -1;

	private int m_LastResolvedTileColumn = -1;

	private PlantPatch m_Plant;

	private static int m_StaticObjectMask = -1;

	private const string STATIC_MAP_OBJECT = "StaticMapObject";

	public override void Init()
	{
		if (m_StaticObjectMask == -1)
		{
			m_StaticObjectMask = LayerMask.GetMask("StaticMapObject");
		}
		m_Plant = null;
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Garden;
	}

	public override bool RequiresTargetting()
	{
		return true;
	}

	public override bool RequiresPositioning()
	{
		return true;
	}

	public override bool ImmobilisesOwner()
	{
		return true;
	}

	public override bool IsImmediateUse()
	{
		return false;
	}

	public override bool CanUse(bool intendsOnUsingImmediately = false)
	{
		if (base.ParentItem == null)
		{
			return false;
		}
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			if (m_LastResolvedTileRow != targetTileRow || m_LastResolvedTileColumn != targetTileColumn)
			{
				m_Plant = null;
			}
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				m_LastResolvedTileRow = targetTileRow;
				m_LastResolvedTileColumn = targetTileColumn;
				Vector3 worldPosition = Vector3.zero;
				if (FloorManager.GetInstance().GetTileCentrePosition(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn, out worldPosition))
				{
					int num = EscapistsRaycast.OverlapSphereNonAlloc(worldPosition, 0.4f, m_StaticObjectMask);
					if (num > 0)
					{
						for (int i = 0; i < num; i++)
						{
							if (!(m_Plant == null))
							{
								break;
							}
							Collider collider = EscapistsRaycast.ColliderOverlapList[i];
							m_Plant = collider.GetComponent<PlantPatch>();
						}
						if (m_Plant != null)
						{
							PlantPatch.State plantState = m_Plant.GetPlantState();
							return plantState == PlantPatch.State.EmptyPatch || plantState == PlantPatch.State.SeedsPlaced;
						}
					}
					else
					{
						m_Plant = null;
					}
				}
			}
		}
		return false;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		if (base.ParentItem != null && m_Owner != null)
		{
			m_AnimTime = useTime;
			m_TotalUsingTime = 0f;
			m_Animation = useAnimation;
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: true);
			m_Owner.m_CharacterAnimator.StartAnimation(m_Animation);
			if (m_Plant != null)
			{
				m_Plant.GardenFunctionalityInteractRPC();
				m_Plant = null;
			}
			if (OnStartOfUse != null)
			{
				OnStartOfUse();
			}
			return true;
		}
		return false;
	}

	public override bool UpdateUsing()
	{
		m_TotalUsingTime += UpdateManager.deltaTime;
		if (m_TotalUsingTime >= m_AnimTime)
		{
			if (m_Owner != null)
			{
				m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: false);
				m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
			}
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			return false;
		}
		return true;
	}

	public override bool CancelUsing()
	{
		if (!base.CancelUsing())
		{
			return false;
		}
		m_Plant = null;
		if (m_Owner != null)
		{
			m_Owner.UseEquippedItemRPC(m_Owner.GetEquippedItem(), bUse: false);
			m_Owner.m_CharacterAnimator.StopAnimation(m_Animation);
		}
		return true;
	}
}
