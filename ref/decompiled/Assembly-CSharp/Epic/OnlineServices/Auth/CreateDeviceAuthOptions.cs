namespace Epic.OnlineServices.Auth;

public class CreateDeviceAuthOptions
{
	public int ApiVersion => 1;

	public EpicAccountId LocalUserId { get; set; }

	public DeviceInfo DeviceInfo { get; set; }
}
