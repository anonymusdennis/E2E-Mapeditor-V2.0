namespace Epic.OnlineServices.Auth;

public class LoginOptions
{
	public int ApiVersion => 1;

	public Credentials Credentials { get; set; }
}
