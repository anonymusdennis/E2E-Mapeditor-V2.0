namespace Epic.OnlineServices.Achievements;

public class QueryDefinitionsOptions
{
	public int ApiVersion => 1;

	public ProductUserId UserId { get; set; }

	public EpicAccountId EpicUserId { get; set; }

	public string[] HiddenAchievementIds { get; set; }
}
