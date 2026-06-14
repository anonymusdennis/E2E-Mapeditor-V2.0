namespace Epic.OnlineServices.Stats;

public class IngestStatCompleteCallbackInfo
{
	public Result ResultCode { get; set; }

	public object ClientData { get; set; }

	public ProductUserId UserId { get; set; }
}
