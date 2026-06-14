namespace Epic.OnlineServices.Lobby;

public class Attribute
{
	public int ApiVersion => 1;

	public AttributeData Data { get; set; }

	public LobbyAttributeVisibility Visbility { get; set; }
}
