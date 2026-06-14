using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class GiveItem : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<int> m_TargetCharacterID;

	[BlackboardOnly]
	public BBParameter<int> m_ItemDataID;

	private ItemContainer m_TargetContainer;

	private int m_ResponseID = -1;

	protected override void OnExecute()
	{
		PhotonView photonView = PhotonView.Find(m_TargetCharacterID.value);
		if (photonView != null)
		{
			m_TargetContainer = photonView.GetComponent<ItemContainer>();
			if (m_TargetContainer != null && !m_TargetContainer.IsVisibleFull())
			{
				ItemManager.GetInstance().AssignItemRPC(m_TargetCharacterID.value, m_ItemDataID.value, OnItemResponse, ref m_ResponseID);
				return;
			}
		}
		EndAction(false);
	}

	private void OnItemResponse(Item item, int eventID)
	{
		if (item != null && eventID == m_ResponseID)
		{
			if (!m_TargetContainer.IsVisibleFull())
			{
				if (m_TargetContainer.AddItemRPC(item))
				{
					EndAction(true);
					return;
				}
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
			else
			{
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
		}
		EndAction(false);
	}
}
