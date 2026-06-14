namespace Epic.OnlineServices.Leaderboards;

public class LeaderboardRecord
{
	public int ApiVersion => 1;

	public ProductUserId UserId { get; set; }

	public uint Rank { get; set; }

	public int Score { get; set; }
}
