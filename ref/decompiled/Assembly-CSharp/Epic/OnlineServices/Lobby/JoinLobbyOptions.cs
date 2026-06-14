namespace Epic.OnlineServices.Lobby;

public class JoinLobbyOptions
{
	public int ApiVersion => 1;

	public LobbyDetails LobbyDetailsHandle { get; set; }

	public ProductUserId LocalUserId { get; set; }
}
