namespace Epic.OnlineServices.Auth;

public class DeleteDeviceAuthCallbackInfo
{
	public Result ResultCode { get; set; }

	public object ClientData { get; set; }

	public EpicAccountId LocalUserId { get; set; }
}
