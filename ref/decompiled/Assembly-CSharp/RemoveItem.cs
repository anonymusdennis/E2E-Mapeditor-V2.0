using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Item")]
public class RemoveItem : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<int> m_ItemDataID;

	protected override void OnExecute()
	{
		if (base.agent.m_ItemContainer != null)
		{
			Item firstItemWithItemID = base.agent.m_ItemContainer.GetFirstItemWithItemID(m_ItemDataID.value);
			if (firstItemWithItemID != null)
			{
				base.agent.m_ItemContainer.RemoveItemRPC(firstItemWithItemID, releaseToManager: true);
			}
		}
		EndAction(true);
	}
}
