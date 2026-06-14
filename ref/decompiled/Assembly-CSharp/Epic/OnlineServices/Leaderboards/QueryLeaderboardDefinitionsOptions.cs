namespace Epic.OnlineServices.Leaderboards;

public class QueryLeaderboardDefinitionsOptions
{
	public int ApiVersion => 1;

	public long StartTime { get; set; }

	public long EndTime { get; set; }
}
