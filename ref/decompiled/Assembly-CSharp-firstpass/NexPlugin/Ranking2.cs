namespace NexPlugin;

public static class Ranking2
{
	public enum Ranking2SortFlags
	{
		NOTHING = 0,
		MOVE_TO_TOP_IN_TIE = 4
	}

	public enum Ranking2GetOptionFlags
	{
		NOTHING,
		MII_REQUIRED
	}

	public enum Ranking2Mode : byte
	{
		USER_RANKING = 0,
		NEAR_RANKING = 1,
		RANGE_RANKING = 2,
		FRIEND_RANKING = 3,
		MIN = 0,
		MAX = 3
	}

	public enum Ranking2ResetMode : byte
	{
		NOTHING = 0,
		EVERYDAY = 1,
		EVERYWEEK = 2,
		MULTI_MONTH = 3,
		MULTI_MONTH_WEEKDAY = 4,
		MIN = 0,
		MAX = 4
	}

	public const uint MAX_PUT_MULTI_SCORES = 20u;

	public const uint MAX_RANKING_LENGTH = 100u;

	public const int MAX_USERNAME_LENGTH = 16;

	public const int MAX_BINARY_DATA_LENGTH = 100;

	public const bool SCORE_ORDER_ASC = false;

	public const bool SCORE_ORDER_DESC = true;
}
