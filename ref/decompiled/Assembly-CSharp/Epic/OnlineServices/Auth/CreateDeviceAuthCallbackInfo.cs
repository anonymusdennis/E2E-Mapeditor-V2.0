namespace Epic.OnlineServices.Auth;

public class CreateDeviceAuthCallbackInfo
{
	public Result ResultCode { get; set; }

	public object ClientData { get; set; }

	public EpicAccountId LocalUserId { get; set; }

	public Credentials Credentials { get; set; }
}
