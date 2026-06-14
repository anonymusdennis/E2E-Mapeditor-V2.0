using UnityEngine;

[CreateAssetMenu(fileName = "Debug Material Functionality", menuName = "Team17/Items/Functionalities/Debug/Create Material Debugger Functionality")]
public class DebugMaterialFunctionality : BaseItemFunctionality
{
	public override void Init()
	{
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
		return false;
	}

	public override bool IsImmediateUse()
	{
		return true;
	}

	public override bool CanUse(bool intendsOnUsingImmediately = false)
	{
		return true;
	}

	public override bool StartUsing(AnimState useAnimation, float useTime)
	{
		base.StartUsing(useAnimation, useTime);
		int targetTileRow = m_Owner.GetTargetTileRow();
		int targetTileColumn = m_Owner.GetTargetTileColumn();
		FloorManager.Floor currentFloor = m_Owner.CurrentFloor;
		FloorManager.GetInstance().GetTileCentrePosition(currentFloor.m_FloorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, targetTileRow, targetTileColumn, out var worldPosition);
		Vector3 halfExtents = new Vector3(0.25f, 0.25f, 5f);
		worldPosition.z -= 2.5f;
		int num = EscapistsRaycast.OverlapBoxNonAlloc(worldPosition, halfExtents, -1, QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = EscapistsRaycast.ColliderOverlapList[i].gameObject;
			if (gameObject.layer == 11)
			{
				gameObject = gameObject.transform.parent.gameObject;
			}
			MultiplatformLog(string.Concat("Hit ", gameObject.transform.name, " at ", gameObject.transform.position, " which is active in scene? ", gameObject.activeInHierarchy.ToString()));
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
			MultiplatformLog("\tNumber of renderers: " + componentsInChildren.Length);
			foreach (Renderer renderer in componentsInChildren)
			{
				MultiplatformLog(string.Concat("\tRenderer on ", renderer.transform.name, " at ", renderer.transform.position, " is enabled? ", renderer.enabled.ToString(), " and active? ", renderer.gameObject.activeInHierarchy.ToString()));
				for (int k = 0; k < renderer.materials.Length; k++)
				{
					MultiplatformLog("\t\tMaterial " + k + " is " + renderer.materials[k].name);
				}
			}
			Animator[] componentsInChildren2 = gameObject.GetComponentsInChildren<Animator>(includeInactive: true);
			MultiplatformLog("\tNumber of animators: " + componentsInChildren2.Length);
			foreach (Animator animator in componentsInChildren2)
			{
				MultiplatformLog(string.Concat("\tAnimator on ", animator.transform.name, " at ", animator.transform.position, " is enabled? ", animator.enabled.ToString(), " and active? ", animator.gameObject.activeInHierarchy.ToString()));
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				MultiplatformLog("\t\tState info: " + currentAnimatorStateInfo.shortNameHash + " time " + currentAnimatorStateInfo.normalizedTime + " " + currentAnimatorStateInfo.ToString());
			}
			DamagableTile component = gameObject.GetComponent<DamagableTile>();
			if ((bool)component)
			{
				MultiplatformLog("\tThis is a damagable tile. Fully damaged: " + component.IsFullyDamaged() + ", Is holding item: " + component.IsHoldingItem() + ", Should stay visible: " + component.m_StayVisible);
			}
		}
		return false;
	}

	private void MultiplatformLog(string message)
	{
		Debug.Log(message);
	}

	public override bool UpdateUsing()
	{
		return false;
	}

	public override bool CancelUsing()
	{
		base.CancelUsing();
		return true;
	}

	public override Functionality GetFunctionalityType()
	{
		return Functionality.None;
	}
}
