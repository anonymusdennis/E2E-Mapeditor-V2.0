namespace Epic.OnlineServices.UserInfo;

public class UserInfoData
{
	public int ApiVersion => 1;

	public EpicAccountId UserId { get; set; }

	public string Country { get; set; }

	public string DisplayName { get; set; }

	public string PreferredLanguage { get; set; }
}
