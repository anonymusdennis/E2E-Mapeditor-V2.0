using System;

namespace Epic.OnlineServices.Metrics;

public class EndPlayerSessionOptions
{
	public int ApiVersion => 1;

	public MetricsAccountIdType AccountIdType { get; set; }

	public IntPtr AccountId { get; set; }
}
