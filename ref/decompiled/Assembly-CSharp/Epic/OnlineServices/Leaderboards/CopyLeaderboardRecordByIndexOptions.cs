namespace Epic.OnlineServices.Leaderboards;

public class CopyLeaderboardRecordByIndexOptions
{
	public int ApiVersion => 1;

	public uint LeaderboardRecordIndex { get; set; }
}
