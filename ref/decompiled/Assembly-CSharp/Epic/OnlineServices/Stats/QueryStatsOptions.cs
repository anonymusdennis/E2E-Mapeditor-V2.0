namespace Epic.OnlineServices.Stats;

public class QueryStatsOptions
{
	public int ApiVersion => 1;

	public ProductUserId UserId { get; set; }

	public long StartTime { get; set; }

	public long EndTime { get; set; }

	public string[] StatNames { get; set; }
}
