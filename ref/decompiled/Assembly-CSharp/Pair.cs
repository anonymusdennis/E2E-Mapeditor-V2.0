public struct Pair<T>
{
	public T ValueOne;

	public T ValueTwo;

	public Pair(T one, T two)
	{
		ValueOne = one;
		ValueTwo = two;
	}

	public bool ValuesEqual(Pair<T> other)
	{
		return other.ValueOne.Equals(ValueOne) && other.ValueTwo.Equals(ValueTwo);
	}
}
