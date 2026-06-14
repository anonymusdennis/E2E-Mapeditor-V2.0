namespace Epic.OnlineServices.Stats;

public class IngestStatOptions
{
	public int ApiVersion => 1;

	public ProductUserId UserId { get; set; }

	public IngestData[] Stats { get; set; }
}
