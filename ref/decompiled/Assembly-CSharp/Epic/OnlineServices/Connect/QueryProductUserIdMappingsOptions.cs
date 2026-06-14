namespace Epic.OnlineServices.Connect;

public class QueryProductUserIdMappingsOptions
{
	public int ApiVersion => 1;

	public ProductUserId LocalUserId { get; set; }

	public ExternalAccountType AccountIdType { get; set; }

	public ProductUserId[] ProductUserIds { get; set; }
}
