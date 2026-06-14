namespace Epic.OnlineServices.Achievements;

public class PlayerAchievement
{
	public int ApiVersion => 1;

	public string AchievementId { get; set; }

	public double Progress { get; set; }

	public long UnlockTime { get; set; }

	public PlayerStatInfo[] StatInfo { get; set; }
}
