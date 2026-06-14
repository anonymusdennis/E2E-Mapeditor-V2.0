namespace Epic.OnlineServices.Auth;

public class DeleteDeviceAuthOptions
{
	public int ApiVersion => 1;

	public EpicAccountId LocalUserId { get; set; }

	public Credentials Credentials { get; set; }
}
