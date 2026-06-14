namespace Epic.OnlineServices.Leaderboards;

public class Definition
{
	public int ApiVersion => 1;

	public string LeaderboardId { get; set; }

	public string StatName { get; set; }

	public LeaderboardAggregation Aggregation { get; set; }

	public long StartTime { get; set; }

	public long EndTime { get; set; }
}
