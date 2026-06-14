namespace UTJ;

public class AbcUtils
{
	public static int CeilDiv(int v, int d)
	{
		return v / d + ((v % d != 0) ? 1 : 0);
	}
}
