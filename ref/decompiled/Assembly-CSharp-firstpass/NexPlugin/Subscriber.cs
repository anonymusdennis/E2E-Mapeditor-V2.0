namespace NexPlugin;

public static class Subscriber
{
	public const uint MAX_TOPIC_CONTENT_SIZE = 100u;

	public const uint MAX_TIMELINE_CONTENT_SIZE = 100u;

	public const uint MAX_FOLLOWING_SIZE = 20u;

	public const uint NUM_RESERVED_TOPICS = 128u;

	public const uint INVALID_RESERVED_TOPIC_NUM = uint.MaxValue;

	public const uint MAX_GET_FOLLOWER_SIZE = 1000u;

	public const uint MAX_CONTENT_MESSAGE_LEN = 140u;

	public const uint MAX_CONTENT_BINARY_SIZE = 256u;

	public const uint MAX_POST_CONTENT_TOPIC_SIZE = 10u;

	public const uint MAX_GET_CONTENT_PARAMS_SIZE = 3u;
}
