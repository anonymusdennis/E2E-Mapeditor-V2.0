using System.Collections.Generic;

public class PlumberJob : HandymanJob
{
	protected override List<HandymanInteraction> SearchForAllPossibleInteractions()
	{
		List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.InmateCell);
		List<HandymanInteraction> list = new List<HandymanInteraction>();
		for (int num = allRoomsByLocation.Count - 1; num >= 0; num--)
		{
			RoomBlob_Cell roomBlobData = allRoomsByLocation[num].GetRoomBlobData<RoomBlob_Cell>();
			InteractiveObject cellObject = roomBlobData.GetCellObject(typeof(ToiletInteraction));
			if (cellObject != null)
			{
				PlumberInteraction plumberInteraction = cellObject.gameObject.GetComponent<PlumberInteraction>();
				if (plumberInteraction == null)
				{
					plumberInteraction = cellObject.gameObject.AddComponent<PlumberInteraction>();
					plumberInteraction.CheckSelfForToiletInteraction();
				}
				list.Add(plumberInteraction);
			}
		}
		return list;
	}

	protected override void SetInteractionForJobTimeActive(HandymanInteraction interaction, bool state, bool? gotFixed)
	{
		base.SetInteractionForJobTimeActive(interaction, state, gotFixed);
		if (state && T17NetManager.IsMasterClient)
		{
			ToiletInteraction component = interaction.gameObject.GetComponent<ToiletInteraction>();
			if (component != null)
			{
				component.FloodToilet();
			}
		}
	}

	protected override List<HandymanInteraction> CopyPossibleInteractionsForNewJobTime()
	{
		List<HandymanInteraction> list = base.CopyPossibleInteractionsForNewJobTime();
		list.RemoveAll((HandymanInteraction x) => x.m_NetObjectLock.IsLocked());
		return list;
	}
}
