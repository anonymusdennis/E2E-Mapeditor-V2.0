namespace Epic.OnlineServices.Stats;

public class Stat
{
	public int ApiVersion => 1;

	public string Name { get; set; }

	public long StartTime { get; set; }

	public long EndTime { get; set; }

	public int Value { get; set; }
}
