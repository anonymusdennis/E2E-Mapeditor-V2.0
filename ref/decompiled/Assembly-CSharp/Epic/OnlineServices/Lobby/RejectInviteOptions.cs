namespace Epic.OnlineServices.Lobby;

public class RejectInviteOptions
{
	public int ApiVersion => 1;

	public string LobbyId { get; set; }

	public ProductUserId LocalUserId { get; set; }
}
