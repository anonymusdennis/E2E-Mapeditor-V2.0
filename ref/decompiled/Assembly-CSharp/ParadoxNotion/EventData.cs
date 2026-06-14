namespace ParadoxNotion;

public class EventData
{
	public string name;

	public object value => GetValue();

	public EventData(string name)
	{
		this.name = name;
	}

	protected virtual object GetValue()
	{
		return null;
	}
}
public class EventData<T> : EventData
{
	public new T value { get; private set; }

	public EventData(string name, T value)
		: base(name)
	{
		this.value = value;
	}

	protected override object GetValue()
	{
		return value;
	}
}
