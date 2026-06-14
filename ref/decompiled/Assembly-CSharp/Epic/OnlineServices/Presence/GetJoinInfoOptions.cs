namespace Epic.OnlineServices.Presence;

public class GetJoinInfoOptions
{
	public int ApiVersion { get; set; }

	public EpicAccountId LocalUserId { get; set; }

	public EpicAccountId TargetUserId { get; set; }
}
