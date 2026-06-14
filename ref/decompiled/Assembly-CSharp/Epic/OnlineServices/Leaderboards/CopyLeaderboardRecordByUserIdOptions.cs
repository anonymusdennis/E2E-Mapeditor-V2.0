namespace Epic.OnlineServices.Leaderboards;

public class CopyLeaderboardRecordByUserIdOptions
{
	public int ApiVersion => 1;

	public ProductUserId UserId { get; set; }
}
