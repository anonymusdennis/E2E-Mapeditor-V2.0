using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "Bind Functionality", menuName = "Team17/Items/Functionalities/Create Bind Functionality")]
public class BindFunctionality : BaseItemFunctionality
{
	public int m_ItemDecayPerUse = 100;

	public bool m_Reclaim = true;

	public float m_BindDuration = 30f;

	private float m_BindingSetupTime = 3f;

	private AnimState m_BindCharacterAnimation = AnimState.UseLow;

	private AnimState m_BindCameraAnimation = AnimState.UseHigh;

	private AnimState m_ActiveAnim;

	private float m_ElapsedBindTime;

	private Character m_TargetCharacter;

	private CCTVCamera m_TargetCctvCamera;

	private Vector3 m_TargetPosition;

	public override void Init()
	{
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.Bind;
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
		if (base.ParentItem.Health <= 0)
		{
			return false;
		}
		if (m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				Vector3 worldPosition = Vector3.zero;
				if (FloorManager.GetInstance().GetTileCentrePosition(m_Owner.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn, out worldPosition))
				{
					Character nearestCharacterAtPosition = GetNearestCharacterAtPosition(worldPosition, 1f);
					if (nearestCharacterAtPosition != null)
					{
						m_TargetCharacter = nearestCharacterAtPosition;
						m_TargetPosition = m_TargetCharacter.transform.position;
						m_ActiveAnim = m_BindCharacterAnimation;
						return true;
					}
					Vector3 tilePos = new Vector3(targetTileRow, targetTileColumn, m_Owner.CurrentFloor.m_FloorIndex);
					CCTVCamera cameraAtTile = CCTVCamera.GetCameraAtTile(tilePos);
					if (cameraAtTile != null && cameraAtTile.isActiveAndEnabled)
					{
						m_TargetCctvCamera = cameraAtTile;
						m_TargetPosition = m_TargetCctvCamera.transform.position;
						m_ActiveAnim = m_BindCameraAnimation;
						return true;
					}
				}
			}
		}
		return false;
	}

	protected Character GetNearestCharacterAtPosition(Vector3 worldPosition, float radius)
	{
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			int row = 0;
			int column = 0;
			FloorManager.Floor floor = instance.FindFloorAtZ(worldPosition.z);
			instance.GetTileGridPoint(floor, FloorManager.TileSystem_Type.TileSystem_Ground, worldPosition, out row, out column);
			Collider[] array = instance.BoxCollideTileArea(floor, row, column, checkTriggers: true);
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].transform.parent != null)
					{
						Character component = array[i].transform.parent.GetComponent<Character>();
						if (component != null && component != m_Owner && component.GetIsKnockedOut() && !component.m_bIsBound && !component.IsPreparingToBeCarried && component.GetCarryingCharacter() == null)
						{
							return component;
						}
					}
				}
			}
		}
		return null;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(m_ActiveAnim, useTime);
		if (base.ParentItem != null && m_Owner != null)
		{
			int targetTileRow = m_Owner.GetTargetTileRow();
			int targetTileColumn = m_Owner.GetTargetTileColumn();
			FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
			if (targetTileRow != -1 && targetTileColumn != -1)
			{
				m_BindingSetupTime = useTime;
				m_ElapsedBindTime = 0f;
				m_Owner.m_bActionRenderersRequired = true;
				m_Owner.m_CharacterAnimator.StartAnimation(m_ActiveAnim);
				if (m_Owner.m_CharacterStats.m_bIsPlayer)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Climb, m_Owner.gameObject);
				}
				if (OnStartOfUse != null)
				{
					OnStartOfUse();
				}
				return true;
			}
		}
		return false;
	}

	public override bool UpdateUsing()
	{
		m_ElapsedBindTime += UpdateManager.deltaTime;
		if (m_ElapsedBindTime >= m_BindingSetupTime)
		{
			FinishUsing();
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			return false;
		}
		Vector3 vector = ((!(m_TargetCharacter != null)) ? m_TargetCctvCamera.transform.position : m_TargetCharacter.transform.position);
		bool flag = (vector - m_TargetPosition).sqrMagnitude > 0.05f;
		if (!flag && m_TargetCharacter != null)
		{
			flag = m_TargetCharacter.GetPickedUpBy() != null || m_TargetCharacter.IsPreparingToBeCarried;
		}
		if (flag)
		{
			CancelUsing();
			if (OnEndOfUse != null)
			{
				OnEndOfUse();
			}
			return false;
		}
		m_TargetPosition = vector;
		return true;
	}

	public override bool CancelUsing()
	{
		if (!base.CancelUsing())
		{
			return false;
		}
		if (m_Owner != null)
		{
			m_Owner.m_CharacterAnimator.StopAnimation(m_ActiveAnim);
			m_Owner.m_bActionRenderersRequired = false;
			if (m_Owner.m_CharacterStats.m_bIsPlayer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Climb, m_Owner.gameObject);
			}
		}
		return true;
	}

	public void FinishUsing()
	{
		if (m_Owner != null)
		{
			m_Owner.m_CharacterAnimator.StopAnimation(m_ActiveAnim);
			m_Owner.m_bActionRenderersRequired = false;
			if (m_Owner.m_CharacterStats.m_bIsPlayer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Player_Climb, m_Owner.gameObject);
			}
			if (m_TargetCharacter != null)
			{
				m_TargetCharacter.BindRPC(base.ParentItem, m_BindDuration, m_Owner);
			}
			else if (m_TargetCctvCamera != null)
			{
				m_TargetCctvCamera.ToggleBindRPC(bind: true, m_BindDuration);
			}
		}
		if (base.ParentItem != null)
		{
			base.ParentItem.DecreaseHealth(m_ItemDecayPerUse);
		}
	}
}
