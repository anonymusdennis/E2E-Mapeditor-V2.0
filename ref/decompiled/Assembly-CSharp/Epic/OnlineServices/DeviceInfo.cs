namespace Epic.OnlineServices;

public class DeviceInfo
{
	public int ApiVersion => 1;

	public string Type { get; set; }

	public string Model { get; set; }

	public string OS { get; set; }
}
