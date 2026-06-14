using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class UseItem : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<int> m_ItemDataID;

	public BBParameter<InteractiveObject> m_TargetObject;

	protected override void OnExecute()
	{
		Item item = base.agent.m_Character.GetEquippedItem();
		if (item == null || item.ItemDataID != m_ItemDataID.value)
		{
			if (item != null && item.IsInUse())
			{
				EndAction(false);
				return;
			}
			item = base.agent.m_Character.m_ItemContainer.GetFirstItemWithItemID(m_ItemDataID.value);
			if (item != null)
			{
				base.agent.m_Character.SetEquippedItem(item);
			}
		}
		if (item != null)
		{
			if (m_TargetObject.value != null)
			{
				int row = -1;
				int column = -1;
				int floor = 0;
				FloorManager.GetInstance().GetTileGridPoint(m_TargetObject.value.transform.position, FloorManager.TileSystem_Type.TileSystem_Ground, out row, out column, out floor);
				base.agent.m_Character.SetTargetTile(row, column);
			}
			item.Use();
			EndAction(true);
		}
		else
		{
			EndAction(false);
		}
	}
}
