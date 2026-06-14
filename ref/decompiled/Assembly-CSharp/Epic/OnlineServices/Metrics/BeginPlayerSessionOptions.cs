using System;

namespace Epic.OnlineServices.Metrics;

public class BeginPlayerSessionOptions
{
	public int ApiVersion => 1;

	public MetricsAccountIdType AccountIdType { get; set; }

	public IntPtr AccountId { get; set; }

	public string DisplayName { get; set; }

	public UserControllerType ControllerType { get; set; }

	public string ServerIp { get; set; }

	public string GameSessionId { get; set; }
}
