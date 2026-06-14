namespace Epic.OnlineServices.Auth;

public class PinGrantInfo
{
	public int ApiVersion => 1;

	public string UserCode { get; set; }

	public string VerificationURI { get; set; }

	public int ExpiresIn { get; set; }
}
