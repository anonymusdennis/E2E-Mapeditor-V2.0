namespace Epic.OnlineServices.Achievements;

public class UnlockedAchievement
{
	public int ApiVersion => 1;

	public string AchievementId { get; set; }

	public long UnlockTime { get; set; }
}
