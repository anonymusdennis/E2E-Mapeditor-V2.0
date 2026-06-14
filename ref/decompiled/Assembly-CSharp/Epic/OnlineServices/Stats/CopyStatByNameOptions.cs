namespace Epic.OnlineServices.Stats;

public class CopyStatByNameOptions
{
	public int ApiVersion => 1;

	public ProductUserId UserId { get; set; }

	public string Name { get; set; }
}
