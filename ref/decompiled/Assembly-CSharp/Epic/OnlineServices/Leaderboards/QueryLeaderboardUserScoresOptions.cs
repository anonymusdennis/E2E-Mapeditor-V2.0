namespace Epic.OnlineServices.Leaderboards;

public class QueryLeaderboardUserScoresOptions
{
	public int ApiVersion => 1;

	public ProductUserId[] UserIds { get; set; }

	public UserScoresQueryStatInfo[] StatInfo { get; set; }

	public long StartTime { get; set; }

	public long EndTime { get; set; }
}
