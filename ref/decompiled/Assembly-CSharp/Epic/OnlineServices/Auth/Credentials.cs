namespace Epic.OnlineServices.Auth;

public class Credentials
{
	public int ApiVersion => 1;

	public string Id { get; set; }

	public string Token { get; set; }

	public LoginCredentialType Type { get; set; }
}
